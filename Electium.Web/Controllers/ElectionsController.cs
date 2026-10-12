using AutoMapper;
using Electium.Application.DTOs;
using Electium.Application.Exceptions;
using Electium.Application.Interfaces;
using Electium.Application.Security;
using Electium.Domain.Enums;
using Electium.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Electium.Web.Controllers;

[Authorize(Roles = RoleNames.Administrator)]
public class ElectionsController : Controller
{
    private readonly IElectionService _electionService;
    private readonly IMapper _mapper;

    public ElectionsController(
        IElectionService electionService,
        IMapper mapper)
    {
        _electionService = electionService;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        CancellationToken cancellationToken)
    {
        var summaries = await _electionService.GetSummariesAsync(
            cancellationToken);

        var viewModels = _mapper.Map<List<ElectionListViewModel>>(summaries);

        return View(viewModels);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new CreateElectionViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        CreateElectionViewModel viewModel,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        var dto = _mapper.Map<CreateElectionDto>(viewModel);

        try
        {
            await _electionService.CreateAsync(dto, cancellationToken);

            TempData["Success"] =
                "La elección se creó y comenzó correctamente.";

            return RedirectToAction(nameof(Index));
        }
        catch (BusinessRuleException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            return View(viewModel);
        }
    }

    [HttpGet]
    public async Task<IActionResult> ConfirmComplete(
        int id,
        CancellationToken cancellationToken)
    {
        var election = await _electionService.GetByIdAsync(
            id,
            cancellationToken);

        if (election is null)
        {
            return NotFound();
        }

        if (election.Status != ElectionStatus.Active)
        {
            return BadRequest(
                "Solo se puede finalizar una elección que esté en proceso.");
        }

        return View(election);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Complete(
        int id,
        CancellationToken cancellationToken)
    {
        try
        {
            var completed = await _electionService.CompleteAsync(
                id,
                cancellationToken);

            if (!completed)
            {
                return NotFound();
            }

            TempData["Success"] = "La elección se finalizó correctamente.";
        }
        catch (BusinessRuleException exception)
        {
            TempData["Error"] = exception.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Results(
        int id,
        CancellationToken cancellationToken)
    {
        var results = await _electionService.GetResultsAsync(
            id,
            cancellationToken);

        if (results is null)
        {
            return NotFound();
        }

        var viewModel = _mapper.Map<ElectionResultsViewModel>(results);

        return View(viewModel);
    }
}