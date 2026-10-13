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
public class CitizensController : Controller
{
    private readonly ICitizenService _citizenService;
    private readonly IMapper _mapper;

    public CitizensController(
        ICitizenService citizenService,
        IMapper mapper)
    {
        _citizenService = citizenService;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        CancellationToken cancellationToken)
    {
        var citizens = await _citizenService.GetAllAsync(cancellationToken);
        var viewModels = _mapper.Map<List<CitizenListViewModel>>(citizens);

        return View(viewModels);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new CreateCitizenViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        CreateCitizenViewModel viewModel,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        try
        {
            var dto = _mapper.Map<CreateCitizenDto>(viewModel);

            await _citizenService.CreateAsync(dto, cancellationToken);

            TempData["Success"] = "El ciudadano se creó correctamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (BusinessRuleException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            return View(viewModel);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(
        int id,
        CancellationToken cancellationToken)
    {
        var citizen = await _citizenService.GetByIdAsync(
            id,
            cancellationToken);

        if (citizen is null)
        {
            return NotFound();
        }

        var viewModel = _mapper.Map<EditCitizenViewModel>(citizen);

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        EditCitizenViewModel viewModel,
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

        try
        {
            var dto = _mapper.Map<UpdateCitizenDto>(viewModel);

            var updatedCitizen = await _citizenService.UpdateAsync(
                id,
                dto,
                cancellationToken);

            if (updatedCitizen is null)
            {
                return NotFound();
            }

            TempData["Success"] = "Los cambios del ciudadano se guardaron correctamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (BusinessRuleException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            return View(viewModel);
        }
    }

    [HttpGet]
    public async Task<IActionResult> ConfirmSetActive(
        int id,
        bool isActive,
        CancellationToken cancellationToken)
    {
        var citizen = await _citizenService.GetByIdAsync(
            id,
            cancellationToken);

        if (citizen is null)
        {
            return NotFound();
        }

        return View(new ConfirmCitizenStatusViewModel
        {
            Id = citizen.Id,
            FullName = $"{citizen.FirstName} {citizen.LastName}",
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
            var updated = await _citizenService.SetActiveAsync(
                id,
                isActive,
                cancellationToken);

            if (!updated)
            {
                return NotFound();
            }

            TempData["Success"] = isActive
                ? "El ciudadano se activó correctamente."
                : "El ciudadano se desactivó correctamente.";
        }
        catch (BusinessRuleException exception)
        {
            TempData["Error"] = exception.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}