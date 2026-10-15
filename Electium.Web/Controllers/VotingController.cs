using System.Security.Claims;
using System.Text.Json;
using System.Text.RegularExpressions;
using AutoMapper;
using Electium.Application.DTOs;
using Electium.Application.Exceptions;
using Electium.Application.Interfaces;
using Electium.Application.Security;
using Electium.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace Electium.Web.Controllers;

[AllowAnonymous]
public class VotingController : Controller
{
    private const int MaximumImageSizeBytes = 5 * 1024 * 1024;

    private const string CitizenSessionKey = "VoterCitizenId";
    private const string ElectionSessionKey = "VoterElectionId";
    private const string VerifiedSessionKey = "VoterIdentityVerified";
    private const string SelectionsSessionKey = "VoterSelections";

    // El valor 0 representa la opción Ninguno en la sesión.
    private const int NoneSelectionValue = 0;

    private readonly IVoterEligibilityService _eligibilityService;
    private readonly IIdentityDocumentOcrService _ocrService;
    private readonly ICitizenService _citizenService;
    private readonly IVoterBallotService _voterBallotService;
    private readonly IMapper _mapper;
    private readonly ILogger<VotingController> _logger;

    public VotingController(
        IVoterEligibilityService eligibilityService,
        IIdentityDocumentOcrService ocrService,
        ICitizenService citizenService,
        IVoterBallotService voterBallotService,
        IMapper mapper,
        ILogger<VotingController> logger)
    {
        _eligibilityService = eligibilityService;
        _ocrService = ocrService;
        _citizenService = citizenService;
        _voterBallotService = voterBallotService;
        _mapper = mapper;
        _logger = logger;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        // El administrador no puede entrar al flujo de votación del elector.
        if (User.Identity?.IsAuthenticated == true &&
            User.IsInRole(RoleNames.Administrator))
        {
            context.Result = RedirectToAction("Index", "Admin");
            return;
        }

        base.OnActionExecuting(context);
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

        if (result.Status != VoterEligibilityStatus.Eligible)
        {
            viewModel.IsError = true;
            viewModel.Message = GetEligibilityMessage(result.Status);

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

        if (!citizenId.HasValue || !electionId.HasValue)
        {
            HttpContext.Session.Clear();
            return RedirectToAction(nameof(Index));
        }

        var citizen = await _citizenService.GetByIdAsync(
            citizenId.Value,
            cancellationToken);

        if (citizen is null)
        {
            HttpContext.Session.Clear();
            return RedirectToAction(nameof(Index));
        }

        // Revalida la elegibilidad y la elección antes de procesar el OCR.
        var eligibility = await _eligibilityService.CheckEligibilityAsync(
            citizen.IdentityNumber,
            cancellationToken);

        if (eligibility.Status != VoterEligibilityStatus.Eligible ||
            eligibility.ElectionId != electionId ||
            eligibility.CitizenId != citizenId)
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
            var expectedIdentity = new string(
                citizen.IdentityNumber.Where(char.IsDigit).ToArray());

            if (expectedIdentity.Length == 0 ||
                !ContainsIdentityNumber(recognizedText, expectedIdentity))
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
        HttpContext.Session.Remove(SelectionsSessionKey);

        TempData["Success"] =
            "Tu identidad se verificó correctamente. Ya puedes completar la boleta.";

        return RedirectToAction(nameof(Positions));
    }

    [HttpGet]
    public async Task<IActionResult> Positions(
        CancellationToken cancellationToken)
    {
        var validationResult =
            await ValidateVerifiedVoterSessionAsync(cancellationToken);

        if (validationResult is not null)
        {
            return validationResult;
        }

        var electionId = HttpContext.Session.GetInt32(ElectionSessionKey)!.Value;
        var ballot = await _voterBallotService.GetActiveBallotAsync(
            electionId,
            cancellationToken);

        if (ballot is null)
        {
            return ReturnToStart(
                "La elección ya no está activa. No se puede continuar.");
        }

        var viewModel =
            _mapper.Map<VoterBallotPositionsViewModel>(ballot);

        var savedSelections = ReadSelections();

        foreach (var position in viewModel.Positions)
        {
            if (!savedSelections.TryGetValue(
                    position.ElectivePositionId,
                    out var selectedValue))
            {
                continue;
            }

            position.IsSelected = true;

            if (selectedValue == NoneSelectionValue)
            {
                position.SelectedOptionText = "Ninguno";
                continue;
            }

            var positionDto = ballot.Positions.FirstOrDefault(
                item => item.ElectivePositionId ==
                        position.ElectivePositionId);

            var selectedCandidate = positionDto?.Candidates.FirstOrDefault(
                candidate => candidate.ElectionCandidateId == selectedValue);

            position.SelectedOptionText = selectedCandidate is null
                ? "Selección guardada"
                : $"{selectedCandidate.FirstName} " +
                  $"{selectedCandidate.LastName} — " +
                  $"{selectedCandidate.PartyName} " +
                  $"({selectedCandidate.PartyAcronym})";
        }

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> VotePosition(
        int positionId,
        CancellationToken cancellationToken)
    {
        var validationResult =
            await ValidateVerifiedVoterSessionAsync(cancellationToken);

        if (validationResult is not null)
        {
            return validationResult;
        }

        var electionId = HttpContext.Session.GetInt32(ElectionSessionKey)!.Value;
        var ballot = await _voterBallotService.GetActiveBallotAsync(
            electionId,
            cancellationToken);

        if (ballot is null)
        {
            return ReturnToStart(
                "La elección ya no está activa. No se puede continuar.");
        }

        var position = ballot.Positions.FirstOrDefault(
            item => item.ElectivePositionId == positionId);

        if (position is null)
        {
            return NotFound();
        }

        var viewModel = _mapper.Map<VoterPositionViewModel>(position);
        viewModel.ElectionName = ballot.ElectionName;

        var savedSelections = ReadSelections();

        if (savedSelections.TryGetValue(positionId, out var selectedValue))
        {
            if (selectedValue == NoneSelectionValue)
            {
                viewModel.IsNoneSelected = true;
            }
            else
            {
                viewModel.SelectedElectionCandidateId = selectedValue;
            }
        }

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitSelection(
        SubmitVoteSelectionViewModel viewModel,
        CancellationToken cancellationToken)
    {
        var validationResult =
            await ValidateVerifiedVoterSessionAsync(cancellationToken);

        if (validationResult is not null)
        {
            return validationResult;
        }

        if (viewModel.IsNone && viewModel.ElectionCandidateId.HasValue)
        {
            TempData["Error"] =
                "La opción Ninguno no puede tener un candidato asociado.";

            return RedirectToAction(
                nameof(VotePosition),
                new { positionId = viewModel.ElectivePositionId });
        }

        if (!viewModel.IsNone &&
            !viewModel.ElectionCandidateId.HasValue)
        {
            TempData["Error"] =
                "Selecciona un candidato o la opción Ninguno antes de continuar.";

            return RedirectToAction(
                nameof(VotePosition),
                new { positionId = viewModel.ElectivePositionId });
        }

        var electionId = HttpContext.Session.GetInt32(ElectionSessionKey)!.Value;
        var ballot = await _voterBallotService.GetActiveBallotAsync(
            electionId,
            cancellationToken);

        var position = ballot?.Positions.FirstOrDefault(
            item => item.ElectivePositionId ==
                    viewModel.ElectivePositionId);

        if (position is null)
        {
            return NotFound();
        }

        if (!viewModel.IsNone &&
            !position.Candidates.Any(candidate =>
                candidate.ElectionCandidateId ==
                viewModel.ElectionCandidateId))
        {
            TempData["Error"] =
                "El candidato seleccionado no participa en ese puesto.";

            return RedirectToAction(
                nameof(VotePosition),
                new { positionId = viewModel.ElectivePositionId });
        }

        var savedSelections = ReadSelections();

        savedSelections[viewModel.ElectivePositionId] =
            viewModel.IsNone
                ? NoneSelectionValue
                : viewModel.ElectionCandidateId!.Value;

        SaveSelections(savedSelections);

        TempData["Success"] =
            $"Se guardó tu selección para {position.PositionName}.";

        return RedirectToAction(nameof(Positions));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> FinalizeVote(
        CancellationToken cancellationToken)
    {
        var validationResult =
            await ValidateVerifiedVoterSessionAsync(cancellationToken);

        if (validationResult is not null)
        {
            return validationResult;
        }

        var citizenId = HttpContext.Session.GetInt32(CitizenSessionKey)!.Value;
        var electionId = HttpContext.Session.GetInt32(ElectionSessionKey)!.Value;

        var ballot = await _voterBallotService.GetActiveBallotAsync(
            electionId,
            cancellationToken);

        if (ballot is null)
        {
            return ReturnToStart(
                "La elección ya no está activa. No se puede registrar el voto.");
        }

        var savedSelections = ReadSelections();

        var missingPositions = ballot.Positions
            .Where(position =>
                !savedSelections.ContainsKey(position.ElectivePositionId))
            .Select(position => position.PositionName)
            .ToList();

        if (missingPositions.Count > 0)
        {
            TempData["Error"] =
                "Debes seleccionar un candidato o Ninguno para estos puestos: " +
                string.Join(", ", missingPositions);

            return RedirectToAction(nameof(Positions));
        }

        var selections = ballot.Positions
            .Select(position =>
            {
                var selectedValue =
                    savedSelections[position.ElectivePositionId];

                return new CastVoteSelectionDto
                {
                    ElectivePositionId = position.ElectivePositionId,
                    IsNone = selectedValue == NoneSelectionValue,
                    ElectionCandidateId =
                        selectedValue == NoneSelectionValue
                            ? null
                            : selectedValue
                };
            })
            .ToList();

        try
        {
            var receipt = await _voterBallotService.CastVoteAsync(
                electionId,
                citizenId,
                selections,
                cancellationToken);

            var viewModel =
                _mapper.Map<VoteCompletedViewModel>(receipt);

            // Evita que se pueda volver a enviar la boleta desde esta sesión.
            HttpContext.Session.Clear();

            return View("VoteCompleted", viewModel);
        }
        catch (BusinessRuleException exception)
        {
            TempData["Error"] = exception.Message;
            return RedirectToAction(nameof(Positions));
        }
        catch (DbUpdateException exception)
        {
            _logger.LogError(
                exception,
                "No se pudo guardar la boleta para la elección {ElectionId}.",
                electionId);

            HttpContext.Session.Clear();

            return View(
                "Index",
                new VoterStartViewModel
                {
                    IsError = true,
                    Message =
                        "No se pudo registrar el voto. Vuelve a validar tu identidad."
                });
        }
    }

    private async Task<IActionResult?> ValidateVerifiedVoterSessionAsync(
        CancellationToken cancellationToken)
    {
        var verified =
            HttpContext.Session.GetString(VerifiedSessionKey) == "true";

        if (!HasVoterSession() || !verified)
        {
            HttpContext.Session.Clear();

            return View(
                "Index",
                new VoterStartViewModel
                {
                    IsError = true,
                    Message =
                        "La sesión de verificación expiró. Ingresa nuevamente tu documento."
                });
        }

        var citizenId = HttpContext.Session.GetInt32(CitizenSessionKey)!.Value;
        var electionId = HttpContext.Session.GetInt32(ElectionSessionKey)!.Value;

        var citizen = await _citizenService.GetByIdAsync(
            citizenId,
            cancellationToken);

        if (citizen is null)
        {
            HttpContext.Session.Clear();

            return View(
                "Index",
                new VoterStartViewModel
                {
                    IsError = true,
                    Message =
                        "No se encontró el registro del ciudadano. Verifica nuevamente tu identidad."
                });
        }

        var eligibility = await _eligibilityService.CheckEligibilityAsync(
            citizen.IdentityNumber,
            cancellationToken);

        if (eligibility.Status != VoterEligibilityStatus.Eligible ||
            eligibility.CitizenId != citizenId ||
            eligibility.ElectionId != electionId)
        {
            HttpContext.Session.Clear();

            return View(
                "Index",
                new VoterStartViewModel
                {
                    IsError = true,
                    Message = eligibility.Status ==
                              VoterEligibilityStatus.AlreadyParticipated
                        ? "Ya has ejercido tu derecho al voto en esta elección."
                        : "Tu elegibilidad cambió y no se puede continuar. " +
                          "Verifica nuevamente tu documento."
                });
        }

        return null;
    }

    private IActionResult ReturnToStart(string message)
    {
        HttpContext.Session.Clear();

        return View(
            "Index",
            new VoterStartViewModel
            {
                IsError = true,
                Message = message
            });
    }

    private Dictionary<int, int> ReadSelections()
    {
        var json = HttpContext.Session.GetString(SelectionsSessionKey);

        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<Dictionary<int, int>>(json)
                ?? [];
        }
        catch (JsonException)
        {
            HttpContext.Session.Remove(SelectionsSessionKey);
            return [];
        }
    }

    private void SaveSelections(Dictionary<int, int> selections)
    {
        HttpContext.Session.SetString(
            SelectionsSessionKey,
            JsonSerializer.Serialize(selections));
    }

    private bool HasVoterSession()
    {
        return HttpContext.Session.GetInt32(CitizenSessionKey).HasValue &&
               HttpContext.Session.GetInt32(ElectionSessionKey).HasValue;
    }

    private static string GetEligibilityMessage(
        VoterEligibilityStatus status)
    {
        return status switch
        {
            VoterEligibilityStatus.InvalidIdentityNumber
                => "Ingresa un número de documento válido.",

            VoterEligibilityStatus.NoActiveElection
                => "Actualmente no hay una elección en proceso.",

            VoterEligibilityStatus.CitizenNotFound
                => "No se encontró un ciudadano con ese número de documento.",

            VoterEligibilityStatus.CitizenInactive
                => "Tu registro de ciudadano está inactivo. No puedes votar.",

            VoterEligibilityStatus.AlreadyParticipated
                => "Ya has ejercido tu derecho al voto en esta elección.",

            _ => "No fue posible validar tu elegibilidad."
        };
    }

    private static bool ContainsIdentityNumber(
        string recognizedText,
        string expectedIdentity)
    {
        // Acepta separadores impresos entre dígitos y evita aceptar el número
        // cuando solo aparece como parte de una secuencia numérica más larga.
        var digitPattern = string.Join(
            @"[\s.\-/]*",
            expectedIdentity.Select(digit => Regex.Escape(digit.ToString())));

        return Regex.IsMatch(
            recognizedText,
            $@"(?<!\d){digitPattern}(?!\d)");
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
