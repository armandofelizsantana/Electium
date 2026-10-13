using System.Security.Claims;
using AutoMapper;
using Electium.Application.DTOs;
using Electium.Application.Exceptions;
using Electium.Application.Interfaces;
using Electium.Application.Security;
using Electium.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Electium.Web.Controllers;

[Authorize(Roles = RoleNames.PoliticalLeader)]
public class CandidatesController : Controller
{
    private const long MaximumPhotoSize = 5 * 1024 * 1024;

    private readonly ILeaderCandidateService _candidateService;
    private readonly IMapper _mapper;
    private readonly IWebHostEnvironment _environment;

    public CandidatesController(
        ILeaderCandidateService candidateService,
        IMapper mapper,
        IWebHostEnvironment environment)
    {
        _candidateService = candidateService;
        _mapper = mapper;
        _environment = environment;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        CancellationToken cancellationToken)
    {
        if (!TryGetLeaderId(out var leaderId))
        {
            return Forbid();
        }

        try
        {
            var candidates = await _candidateService.GetAllForLeaderAsync(
                leaderId,
                cancellationToken);

            var viewModel = new CandidateIndexViewModel
            {
                Candidates =
                    _mapper.Map<List<CandidateListItemViewModel>>(candidates),

                HasActiveElection =
                    await _candidateService.HasActiveElectionAsync(
                        cancellationToken)
            };

            return View(viewModel);
        }
        catch (BusinessRuleException exception)
        {
            return RedirectToLeaderHomeWithError(exception);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Create(
        CancellationToken cancellationToken)
    {
        if (!TryGetLeaderId(out var leaderId))
        {
            return Forbid();
        }

        try
        {
            // También comprueba que la cuenta tenga un partido activo.
            await _candidateService.GetAllForLeaderAsync(
                leaderId,
                cancellationToken);

            if (await _candidateService.HasActiveElectionAsync(
                    cancellationToken))
            {
                TempData["Error"] =
                    "No se pueden crear candidatos mientras haya una elección activa.";

                return RedirectToAction(nameof(Index));
            }

            return View(new CreateCandidateViewModel());
        }
        catch (BusinessRuleException exception)
        {
            return RedirectToLeaderHomeWithError(exception);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        CreateCandidateViewModel viewModel,
        CancellationToken cancellationToken)
    {
        if (!TryGetLeaderId(out var leaderId))
        {
            return Forbid();
        }

        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        string? savedPhotoPath = null;

        try
        {
            savedPhotoPath = await SavePhotoAsync(
                viewModel.PhotoFile!,
                cancellationToken);

            var dto = _mapper.Map<CreateCandidateDto>(viewModel);
            dto.PhotoPath = savedPhotoPath;

            await _candidateService.CreateAsync(
                leaderId,
                dto,
                cancellationToken);

            TempData["Success"] = "El candidato se creó correctamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (BusinessRuleException exception)
        {
            DeletePhoto(savedPhotoPath);
            ModelState.AddModelError(string.Empty, exception.Message);
            return View(viewModel);
        }
        catch (ArgumentException exception)
        {
            DeletePhoto(savedPhotoPath);
            ModelState.AddModelError(
                nameof(viewModel.PhotoFile),
                exception.Message);

            return View(viewModel);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(
        int id,
        CancellationToken cancellationToken)
    {
        if (!TryGetLeaderId(out var leaderId))
        {
            return Forbid();
        }

        try
        {
            if (await _candidateService.HasActiveElectionAsync(
                    cancellationToken))
            {
                TempData["Error"] =
                    "No se pueden editar candidatos mientras haya una elección activa.";

                return RedirectToAction(nameof(Index));
            }

            var candidate = await _candidateService.GetForLeaderAsync(
                leaderId,
                id,
                cancellationToken);

            if (candidate is null)
            {
                return NotFound();
            }

            return View(_mapper.Map<EditCandidateViewModel>(candidate));
        }
        catch (BusinessRuleException exception)
        {
            return RedirectToLeaderHomeWithError(exception);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        EditCandidateViewModel viewModel,
        CancellationToken cancellationToken)
    {
        if (!TryGetLeaderId(out var leaderId))
        {
            return Forbid();
        }

        if (id != viewModel.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        string? savedPhotoPath = null;

        try
        {
            var dto = _mapper.Map<UpdateCandidateDto>(viewModel);

            if (viewModel.PhotoFile is not null)
            {
                savedPhotoPath = await SavePhotoAsync(
                    viewModel.PhotoFile,
                    cancellationToken);

                dto.PhotoPath = savedPhotoPath;
            }

            var updated = await _candidateService.UpdateAsync(
                leaderId,
                id,
                dto,
                cancellationToken);

            if (updated is null)
            {
                DeletePhoto(savedPhotoPath);
                return NotFound();
            }

            TempData["Success"] = "Los cambios del candidato se guardaron.";
            return RedirectToAction(nameof(Index));
        }
        catch (BusinessRuleException exception)
        {
            DeletePhoto(savedPhotoPath);
            ModelState.AddModelError(string.Empty, exception.Message);
            return View(viewModel);
        }
        catch (ArgumentException exception)
        {
            DeletePhoto(savedPhotoPath);
            ModelState.AddModelError(
                nameof(viewModel.PhotoFile),
                exception.Message);

            return View(viewModel);
        }
    }

    [HttpGet]
    public async Task<IActionResult> ConfirmSetActive(
        int id,
        bool isActive,
        CancellationToken cancellationToken)
    {
        if (!TryGetLeaderId(out var leaderId))
        {
            return Forbid();
        }

        try
        {
            if (await _candidateService.HasActiveElectionAsync(
                    cancellationToken))
            {
                TempData["Error"] =
                    "No se pueden cambiar estados durante una elección activa.";

                return RedirectToAction(nameof(Index));
            }

            var candidate = await _candidateService.GetForLeaderAsync(
                leaderId,
                id,
                cancellationToken);

            if (candidate is null)
            {
                return NotFound();
            }

            return View(new ConfirmCandidateStatusViewModel
            {
                Id = candidate.Id,
                FullName = $"{candidate.FirstName} {candidate.LastName}",
                IsActive = isActive
            });
        }
        catch (BusinessRuleException exception)
        {
            return RedirectToLeaderHomeWithError(exception);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetActive(
        int id,
        bool isActive,
        CancellationToken cancellationToken)
    {
        if (!TryGetLeaderId(out var leaderId))
        {
            return Forbid();
        }

        try
        {
            var updated = await _candidateService.SetActiveAsync(
                leaderId,
                id,
                isActive,
                cancellationToken);

            if (!updated)
            {
                return NotFound();
            }

            TempData["Success"] = isActive
                ? "El candidato se activó correctamente."
                : "El candidato se desactivó y sus postulaciones quedaron inactivas.";
        }
        catch (BusinessRuleException exception)
        {
            return RedirectToLeaderHomeWithError(exception);
        }

        return RedirectToAction(nameof(Index));
    }

    private IActionResult RedirectToLeaderHomeWithError(
        BusinessRuleException exception)
    {
        TempData["Error"] = exception.Message;
        return RedirectToAction("Index", "Leader");
    }

    private bool TryGetLeaderId(out int leaderId)
    {
        return int.TryParse(
            User.FindFirstValue(ClaimTypes.NameIdentifier),
            out leaderId);
    }

    private async Task<string> SavePhotoAsync(
        IFormFile file,
        CancellationToken cancellationToken)
    {
        if (file.Length == 0 || file.Length > MaximumPhotoSize)
        {
            throw new ArgumentException(
                "La foto debe tener contenido y no superar 5 MB.");
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (extension is not ".png" and not ".jpg" and not ".jpeg" and not ".webp")
        {
            throw new ArgumentException(
                "La foto debe ser PNG, JPG o WEBP.");
        }

        var webRoot = _environment.WebRootPath
            ?? Path.Combine(_environment.ContentRootPath, "wwwroot");

        var directory = Path.Combine(webRoot, "uploads", "candidates");
        Directory.CreateDirectory(directory);

        var fileName = $"{Guid.NewGuid():N}{extension}";
        var fullPath = Path.Combine(directory, fileName);

        await using var stream = new FileStream(
            fullPath,
            FileMode.CreateNew,
            FileAccess.Write);

        await file.CopyToAsync(stream, cancellationToken);

        return $"/uploads/candidates/{fileName}";
    }

    private void DeletePhoto(string? relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            return;
        }

        var webRoot = _environment.WebRootPath
            ?? Path.Combine(_environment.ContentRootPath, "wwwroot");

        var relativeFile = relativePath
            .TrimStart('/')
            .Replace('/', Path.DirectorySeparatorChar);

        var fullPath = Path.Combine(webRoot, relativeFile);

        if (System.IO.File.Exists(fullPath))
        {
            System.IO.File.Delete(fullPath);
        }
    }
}