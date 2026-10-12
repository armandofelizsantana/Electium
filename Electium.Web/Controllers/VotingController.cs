using Electium.Application.Interfaces;
using Electium.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Electium.Web.Controllers;

[AllowAnonymous]
public class VotingController : Controller
{
    private const int MaximumImageSizeBytes = 5 * 1024 * 1024;

    private const string CitizenSessionKey = "VoterCitizenId";
    private const string ElectionSessionKey = "VoterElectionId";
    private const string VerifiedSessionKey = "VoterIdentityVerified";

    private readonly IVoterEligibilityService _eligibilityService;
    private readonly IIdentityDocumentOcrService _ocrService;
    private readonly ICitizenService _citizenService;
    private readonly ILogger<VotingController> _logger;

    public VotingController(
        IVoterEligibilityService eligibilityService,
        IIdentityDocumentOcrService ocrService,
        ICitizenService citizenService,
        ILogger<VotingController> logger)
    {
        _eligibilityService = eligibilityService;
        _ocrService = ocrService;
        _citizenService = citizenService;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View(new VoterStartViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CheckEligibility(
        VoterStartViewModel viewModel,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View("Index", viewModel);
        }

        var result = await _eligibilityService.CheckEligibilityAsync(
            viewModel.IdentityNumber,
            cancellationToken);

        if (result.Status !=
            Electium.Application.Security.VoterEligibilityStatus.Eligible)
        {
            viewModel.IsError = true;
            viewModel.Message = result.Status switch
            {
                Electium.Application.Security.VoterEligibilityStatus.InvalidIdentityNumber
                    => "Ingresa un número de documento válido.",

                Electium.Application.Security.VoterEligibilityStatus.NoActiveElection
                    => "Actualmente no hay una elección en proceso.",

                Electium.Application.Security.VoterEligibilityStatus.CitizenNotFound
                    => "No se encontró un ciudadano con ese número de documento.",

                Electium.Application.Security.VoterEligibilityStatus.CitizenInactive
                    => "Tu registro de ciudadano está inactivo. No puedes votar.",

                Electium.Application.Security.VoterEligibilityStatus.AlreadyParticipated
                    => "Ya registraste tu participación en esta elección.",

                _ => "No fue posible validar tu elegibilidad."
            };

            return View("Index", viewModel);
        }

        if (result.CitizenId is null || result.ElectionId is null)
        {
            viewModel.IsError = true;
            viewModel.Message =
                "No fue posible iniciar la verificación. Inténtalo nuevamente.";

            return View("Index", viewModel);
        }

        HttpContext.Session.Clear();
        HttpContext.Session.SetInt32(CitizenSessionKey, result.CitizenId.Value);
        HttpContext.Session.SetInt32(ElectionSessionKey, result.ElectionId.Value);
        HttpContext.Session.SetString(VerifiedSessionKey, "false");

        return RedirectToAction(nameof(UploadIdentity));
    }

    [HttpGet]
    public IActionResult UploadIdentity()
    {
        if (!HasVoterSession())
        {
            HttpContext.Session.Clear();
            return RedirectToAction(nameof(Index));
        }

        return View(new UploadIdentityViewModel());
    }

    [HttpPost]
    [RequestSizeLimit(MaximumImageSizeBytes)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadIdentity(
        UploadIdentityViewModel viewModel,
        CancellationToken cancellationToken)
    {
        if (!HasVoterSession())
        {
            HttpContext.Session.Clear();
            return RedirectToAction(nameof(Index));
        }

        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        var image = viewModel.FrontImage!;

        if (image.Length == 0 || image.Length > MaximumImageSizeBytes)
        {
            ModelState.AddModelError(
                nameof(viewModel.FrontImage),
                "La imagen debe tener contenido y no superar 5 MB.");

            return View(viewModel);
        }

        await using var memoryStream = new MemoryStream();
        await image.CopyToAsync(memoryStream, cancellationToken);
        var imageBytes = memoryStream.ToArray();

        if (!HasSupportedImageSignature(imageBytes))
        {
            ModelState.AddModelError(
                nameof(viewModel.FrontImage),
                "El archivo debe ser una imagen JPG o PNG válida.");

            return View(viewModel);
        }

        var citizenId = HttpContext.Session.GetInt32(CitizenSessionKey);
        var electionId = HttpContext.Session.GetInt32(ElectionSessionKey);

        var citizen = await _citizenService.GetByIdAsync(
            citizenId!.Value,
            cancellationToken);

        if (citizen is null)
        {
            HttpContext.Session.Clear();
            return RedirectToAction(nameof(Index));
        }

        // Revalida por si el ciudadano ya participó o la elección cambió
        // mientras se cargaba la imagen.
        var eligibility = await _eligibilityService.CheckEligibilityAsync(
            citizen.IdentityNumber,
            cancellationToken);

        if (eligibility.Status !=
                Electium.Application.Security.VoterEligibilityStatus.Eligible ||
            eligibility.ElectionId != electionId)
        {
            HttpContext.Session.Clear();

            return View(
                "Index",
                new VoterStartViewModel
                {
                    IsError = true,
                    Message =
                        "Tu elegibilidad cambió y no se puede continuar. " +
                        "Verifica nuevamente tu documento."
                });
        }

        try
        {
            var recognizedText = _ocrService.ReadText(imageBytes);
            var expectedIdentity = Normalize(citizen.IdentityNumber);
            var recognizedContent = Normalize(recognizedText);

            if (expectedIdentity.Length == 0 ||
                !recognizedContent.Contains(
                    expectedIdentity,
                    StringComparison.Ordinal))
            {
                ModelState.AddModelError(
                    nameof(viewModel.FrontImage),
                    "No pudimos verificar el número de documento en la imagen. " +
                    "Revisa que sea la parte frontal de tu cédula e inténtalo otra vez.");

                return View(viewModel);
            }
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "No se pudo procesar la imagen de identidad con OCR.");

            ModelState.AddModelError(
                string.Empty,
                "No se pudo leer la imagen. Verifica el archivo e inténtalo nuevamente.");

            return View(viewModel);
        }

        HttpContext.Session.SetString(VerifiedSessionKey, "true");

        return View("IdentityVerified");
    }

    private bool HasVoterSession()
    {
        return HttpContext.Session.GetInt32(CitizenSessionKey).HasValue &&
               HttpContext.Session.GetInt32(ElectionSessionKey).HasValue;
    }

    private static string Normalize(string value)
    {
        return new string(
            value
                .Where(char.IsLetterOrDigit)
                .Select(char.ToUpperInvariant)
                .ToArray());
    }

    private static bool HasSupportedImageSignature(byte[] bytes)
    {
        var isJpeg =
            bytes.Length >= 3 &&
            bytes[0] == 0xFF &&
            bytes[1] == 0xD8 &&
            bytes[2] == 0xFF;

        var isPng =
            bytes.Length >= 8 &&
            bytes[0] == 0x89 &&
            bytes[1] == 0x50 &&
            bytes[2] == 0x4E &&
            bytes[3] == 0x47 &&
            bytes[4] == 0x0D &&
            bytes[5] == 0x0A &&
            bytes[6] == 0x1A &&
            bytes[7] == 0x0A;

        return isJpeg || isPng;
    }
}