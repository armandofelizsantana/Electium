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
public class ElectivePositionsController : Controller
{
    private readonly IElectivePositionService _electivePositionService;
    private readonly IMapper _mapper;

    public ElectivePositionsController(
        IElectivePositionService electivePositionService,
        IMapper mapper)
    {
        _electivePositionService = electivePositionService;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        CancellationToken cancellationToken)
    {
        var positions = await _electivePositionService.GetAllAsync(
            cancellationToken);

        var viewModels =
            _mapper.Map<List<ElectivePositionListViewModel>>(positions);

        return View(viewModels);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new CreateElectivePositionViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        CreateElectivePositionViewModel viewModel,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        var dto = _mapper.Map<CreateElectivePositionDto>(viewModel);

        try
        {
            await _electivePositionService.CreateAsync(dto, cancellationToken);
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
        var position = await _electivePositionService.GetByIdAsync(
            id,
            cancellationToken);

        if (position is null)
        {
            return NotFound();
        }

        var viewModel = _mapper.Map<EditElectivePositionViewModel>(position);

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        EditElectivePositionViewModel viewModel,
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

        var dto = _mapper.Map<UpdateElectivePositionDto>(viewModel);

        try
        {
            var updatedPosition =
                await _electivePositionService.UpdateAsync(
                    id,
                    dto,
                    cancellationToken);

            if (updatedPosition is null)
            {
                return NotFound();
            }

            TempData["Success"] = "Los cambios del puesto se guardaron correctamente.";

            return RedirectToAction(nameof(Index));

        }
        catch (BusinessRuleException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            return View(viewModel);
        }
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
            var updated = await _electivePositionService.SetActiveAsync(
                id,
                isActive,
                cancellationToken);

            if (!updated)
            {
                return NotFound();
            }

            TempData["Success"] = isActive
                ? "El puesto se activo correctamente"
                : "El puesto se desactivo correctamente";
        }
        catch (BusinessRuleException exception)
        {
            TempData["Error"] = exception.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}