using AutoMapper;
using Electium.Application.DTOs;
using Electium.Application.Exceptions;
using Electium.Application.Interfaces;
using Electium.Application.Security;
using Electium.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Electium.Web.Controllers;

[Authorize(Roles = RoleNames.Administrator)]
public class PoliticalPartiesController : Controller
{
    private const long MaximumLogoSize = 5 * 1024 * 1024;

    private readonly IPoliticalPartyService _partyService;
    private readonly IMapper _mapper;
    private readonly IWebHostEnvironment _environment;

    public PoliticalPartiesController(
        IPoliticalPartyService partyService,
        IMapper mapper,
        IWebHostEnvironment environment)
    {
        _partyService = partyService;
        _mapper = mapper;
        _environment = environment;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        CancellationToken cancellationToken)
    {
        var parties = await _partyService.GetAllAsync(cancellationToken);
        var viewModels = _mapper.Map<List<PoliticalPartyListViewModel>>(parties);

        return View(viewModels);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new CreatePoliticalPartyViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        CreatePoliticalPartyViewModel viewModel,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        string? savedLogoPath = null;

        try
        {
            savedLogoPath = await SaveLogoAsync(
                viewModel.LogoFile!,
                cancellationToken);

            var dto = _mapper.Map<CreatePoliticalPartyDto>(viewModel);
            dto.LogoPath = savedLogoPath;

            await _partyService.CreateAsync(dto, cancellationToken);

            TempData["Success"] = "El partido se creó correctamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (BusinessRuleException exception)
        {
            DeleteLogo(savedLogoPath);
            ModelState.AddModelError(string.Empty, exception.Message);
            return View(viewModel);
        }
        catch (ArgumentException exception)
        {
            DeleteLogo(savedLogoPath);
            ModelState.AddModelError(nameof(viewModel.LogoFile), exception.Message);
            return View(viewModel);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(
        int id,
        CancellationToken cancellationToken)
    {
        var party = await _partyService.GetByIdAsync(id, cancellationToken);

        if (party is null)
        {
            return NotFound();
        }

        return View(_mapper.Map<EditPoliticalPartyViewModel>(party));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        EditPoliticalPartyViewModel viewModel,
        CancellationToken cancellationToken)
    {
        if (id != viewModel.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        string? newLogoPath = null;

        try
        {
            var dto = _mapper.Map<UpdatePoliticalPartyDto>(viewModel);

            if (viewModel.LogoFile is not null)
            {
                newLogoPath = await SaveLogoAsync(
                    viewModel.LogoFile,
                    cancellationToken);

                dto.LogoPath = newLogoPath;
            }

            var updated = await _partyService.UpdateAsync(
                id,
                dto,
                cancellationToken);

            if (updated is null)
            {
                DeleteLogo(newLogoPath);
                return NotFound();
            }

            TempData["Success"] = "Los cambios del partido se guardaron correctamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (BusinessRuleException exception)
        {
            DeleteLogo(newLogoPath);
            ModelState.AddModelError(string.Empty, exception.Message);
            return View(viewModel);
        }
        catch (ArgumentException exception)
        {
            DeleteLogo(newLogoPath);
            ModelState.AddModelError(nameof(viewModel.LogoFile), exception.Message);
            return View(viewModel);
        }
    }

    [HttpGet]
    public async Task<IActionResult> ConfirmSetActive(
        int id,
        bool isActive,
        CancellationToken cancellationToken)
    {
        var party = await _partyService.GetByIdAsync(id, cancellationToken);

        if (party is null)
        {
            return NotFound();
        }

        return View(new ConfirmPoliticalPartyStatusViewModel
        {
            Id = party.Id,
            Name = party.Name,
            IsActive = isActive
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetActive(
        int id,
        bool isActive,
        CancellationToken cancellationToken)
    {
        try
        {
            var updated = await _partyService.SetActiveAsync(
                id,
                isActive,
                cancellationToken);

            if (!updated)
            {
                return NotFound();
            }

            TempData["Success"] = isActive
                ? "El partido se activó correctamente."
                : "El partido se desactivó y sus candidatos y postulaciones quedaron inactivos.";
        }
        catch (BusinessRuleException exception)
        {
            TempData["Error"] = exception.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task<string> SaveLogoAsync(
        IFormFile file,
        CancellationToken cancellationToken)
    {
        if (file.Length == 0 || file.Length > MaximumLogoSize)
        {
            throw new ArgumentException(
                "El logo debe tener contenido y no superar 5 MB.");
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (extension is not ".png" and not ".jpg" and not ".jpeg" and not ".webp")
        {
            throw new ArgumentException(
                "El logo debe ser una imagen PNG, JPG o WEBP.");
        }

        var webRoot = _environment.WebRootPath
            ?? Path.Combine(_environment.ContentRootPath, "wwwroot");

        var uploadDirectory = Path.Combine(
            webRoot,
            "uploads",
            "parties");

        Directory.CreateDirectory(uploadDirectory);

        var fileName = $"{Guid.NewGuid():N}{extension}";
        var fullPath = Path.Combine(uploadDirectory, fileName);

        await using var stream = new FileStream(
            fullPath,
            FileMode.CreateNew,
            FileAccess.Write);

        await file.CopyToAsync(stream, cancellationToken);

        return $"/uploads/parties/{fileName}";
    }

    private void DeleteLogo(string? relativePath)
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