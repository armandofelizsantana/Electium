using AutoMapper;
using Electium.Application.DTOs;
using Electium.Application.Exceptions;
using Electium.Application.Interfaces;
using Electium.Application.Security;
using Electium.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Electium.Web.Controllers;

[Authorize(Roles = RoleNames.Administrator)]
public class LeaderPartyAssignmentsController : Controller
{
    private readonly ILeaderPartyAssignmentService _assignmentService;
    private readonly IMapper _mapper;

    public LeaderPartyAssignmentsController(
        ILeaderPartyAssignmentService assignmentService,
        IMapper mapper)
    {
        _assignmentService = assignmentService;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        CancellationToken cancellationToken)
    {
        var assignments = await _assignmentService.GetAllAsync(
            cancellationToken);

        return View(
            _mapper.Map<List<LeaderPartyAssignmentListViewModel>>(
                assignments));
    }

    [HttpGet]
    public async Task<IActionResult> Create(
        CancellationToken cancellationToken)
    {
        var viewModel = new CreateLeaderPartyAssignmentViewModel();
        await LoadOptionsAsync(viewModel, cancellationToken);

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        CreateLeaderPartyAssignmentViewModel viewModel,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await LoadOptionsAsync(viewModel, cancellationToken);
            return View(viewModel);
        }

        try
        {
            var dto = _mapper.Map<CreateLeaderPartyAssignmentDto>(
                viewModel);

            await _assignmentService.AssignAsync(dto, cancellationToken);

            TempData["Success"] =
                "El dirigente quedó asignado al partido correctamente.";

            return RedirectToAction(nameof(Index));
        }
        catch (BusinessRuleException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            await LoadOptionsAsync(viewModel, cancellationToken);
            return View(viewModel);
        }
    }

    [HttpGet]
    public async Task<IActionResult> ConfirmRemove(
        int leaderUserId,
        CancellationToken cancellationToken)
    {
        var assignment = await _assignmentService.GetByLeaderIdAsync(
            leaderUserId,
            cancellationToken);

        if (assignment is null)
        {
            return NotFound();
        }

        return View(
            _mapper.Map<ConfirmRemoveLeaderPartyAssignmentViewModel>(
                assignment));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(
        int leaderUserId,
        CancellationToken cancellationToken)
    {
        try
        {
            var removed = await _assignmentService.RemoveAsync(
                leaderUserId,
                cancellationToken);

            if (!removed)
            {
                return NotFound();
            }

            TempData["Success"] =
                "El dirigente se desvinculó del partido correctamente.";
        }
        catch (BusinessRuleException exception)
        {
            TempData["Error"] = exception.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task LoadOptionsAsync(
        CreateLeaderPartyAssignmentViewModel viewModel,
        CancellationToken cancellationToken)
    {
        var leaders =
            await _assignmentService.GetUnassignedActiveLeadersAsync(
                cancellationToken);

        var parties =
            await _assignmentService.GetActivePartiesAsync(
                cancellationToken);

        viewModel.AvailableLeaders = leaders
            .Select(leader => new SelectListItem(
                $"{leader.DisplayName} ({leader.UserName})",
                leader.Id.ToString()))
            .ToList();

        viewModel.ActiveParties = parties
            .Select(party => new SelectListItem(
                $"{party.Acronym} — {party.Name}",
                party.Id.ToString()))
            .ToList();
    }
}