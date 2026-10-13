using System.Security.Claims;
using Electium.Application.Exceptions;
using Electium.Application.Interfaces;
using Electium.Application.Security;
using Electium.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Electium.Web.Controllers;

[Authorize(Roles = RoleNames.PoliticalLeader)]
public class LeaderNominationsController : Controller
{
    private readonly ILeaderNominationService _nominationService;

    public LeaderNominationsController(
        ILeaderNominationService nominationService)
    {
        _nominationService = nominationService;
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
            var nominations = await _nominationService.GetAllAsync(
                leaderId,
                cancellationToken);

            var viewModel = new NominationIndexViewModel
            {
                HasActiveElection =
                    await _nominationService.HasActiveElectionAsync(
                        cancellationToken),

                Nominations = nominations.Select(nomination =>
                    new NominationRowViewModel
                    {
                        Id = nomination.Id,
                        CandidateFullName = nomination.CandidateFullName,
                        CandidatePhotoPath = nomination.CandidatePhotoPath,
                        CandidatePartyName = nomination.CandidatePartyName,
                        ElectivePositionName =
                            nomination.ElectivePositionName,
                        IsActive = nomination.IsActive
                    }).ToList()
            };

            return View(viewModel);
        }
        catch (BusinessRuleException exception)
        {
            return ShowLeaderError(exception);
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
            if (await _nominationService.HasActiveElectionAsync(
                    cancellationToken))
            {
                TempData["Error"] =
                    "No se pueden crear asignaciones mientras haya una elección activa.";

                return RedirectToAction(nameof(Index));
            }

            var viewModel = new CreateNominationViewModel();

            await LoadPositionsAsync(
                viewModel,
                leaderId,
                cancellationToken);

            return View(viewModel);
        }
        catch (BusinessRuleException exception)
        {
            return ShowLeaderError(exception);
        }
    }

    [HttpGet]
    public async Task<IActionResult> CandidatesForPosition(
        int electivePositionId,
        CancellationToken cancellationToken)
    {
        if (!TryGetLeaderId(out var leaderId))
        {
            return Forbid();
        }

        try
        {
            if (await _nominationService.HasActiveElectionAsync(
                    cancellationToken))
            {
                return Conflict(new
                {
                    message =
                        "No se pueden modificar asignaciones durante una elección activa."
                });
            }

            var candidates =
                await _nominationService.GetAvailableCandidatesAsync(
                    leaderId,
                    electivePositionId,
                    cancellationToken);

            return Json(candidates.Select(candidate => new
            {
                id = candidate.Id,
                fullName = candidate.FullName,
                originPartyName = candidate.OriginPartyName
            }));
        }
        catch (BusinessRuleException exception)
        {
            return Conflict(new { message = exception.Message });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Review(
        CreateNominationViewModel viewModel,
        CancellationToken cancellationToken)
    {
        if (!TryGetLeaderId(out var leaderId))
        {
            return Forbid();
        }

        try
        {
            if (await _nominationService.HasActiveElectionAsync(
                    cancellationToken))
            {
                TempData["Error"] =
                    "No se pueden crear asignaciones mientras haya una elección activa.";

                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid)
            {
                await LoadPositionsAsync(
                    viewModel,
                    leaderId,
                    cancellationToken);

                return View("Create", viewModel);
            }

            var availablePositions =
                await _nominationService.GetAvailablePositionsAsync(
                    leaderId,
                    cancellationToken);

            var selectedPosition = availablePositions.FirstOrDefault(
                position => position.Id == viewModel.ElectivePositionId);

            if (selectedPosition is null)
            {
                ModelState.AddModelError(
                    nameof(viewModel.ElectivePositionId),
                    "El puesto seleccionado ya no está disponible.");

                await LoadPositionsAsync(
                    viewModel,
                    leaderId,
                    cancellationToken);

                return View("Create", viewModel);
            }

            var availableCandidates =
                await _nominationService.GetAvailableCandidatesAsync(
                    leaderId,
                    viewModel.ElectivePositionId,
                    cancellationToken);

            var selectedCandidate = availableCandidates.FirstOrDefault(
                candidate => candidate.Id == viewModel.CandidateId);

            if (selectedCandidate is null)
            {
                ModelState.AddModelError(
                    nameof(viewModel.CandidateId),
                    "El candidato seleccionado no está disponible para ese puesto.");

                await LoadPositionsAsync(
                    viewModel,
                    leaderId,
                    cancellationToken);

                return View("Create", viewModel);
            }

            return View(
                "ConfirmCreate",
                new ConfirmCreateNominationViewModel
                {
                    ElectivePositionId = selectedPosition.Id,
                    ElectivePositionName = selectedPosition.Name,
                    CandidateId = selectedCandidate.Id,
                    CandidateFullName = selectedCandidate.FullName,
                    CandidatePartyName = selectedCandidate.OriginPartyName
                });
        }
        catch (BusinessRuleException exception)
        {
            return ShowLeaderError(exception);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateConfirmed(
        ConfirmCreateNominationViewModel viewModel,
        CancellationToken cancellationToken)
    {
        if (!TryGetLeaderId(out var leaderId))
        {
            return Forbid();
        }

        try
        {
            await _nominationService.CreateAsync(
                leaderId,
                viewModel.CandidateId,
                viewModel.ElectivePositionId,
                cancellationToken);

            TempData["Success"] =
                "El candidato se asignó al puesto correctamente.";

            return RedirectToAction(nameof(Index));
        }
        catch (BusinessRuleException exception)
        {
            return ShowLeaderError(exception);
        }
    }

    [HttpGet]
    public async Task<IActionResult> ConfirmDeactivate(
        int id,
        CancellationToken cancellationToken)
    {
        if (!TryGetLeaderId(out var leaderId))
        {
            return Forbid();
        }

        try
        {
            if (await _nominationService.HasActiveElectionAsync(
                    cancellationToken))
            {
                TempData["Error"] =
                    "No se pueden modificar asignaciones mientras haya una elección activa.";

                return RedirectToAction(nameof(Index));
            }

            var nomination = (await _nominationService.GetAllAsync(
                    leaderId,
                    cancellationToken))
                .FirstOrDefault(item => item.Id == id && item.IsActive);

            if (nomination is null)
            {
                return NotFound();
            }

            return View(
                new ConfirmDeactivateNominationViewModel
                {
                    Id = nomination.Id,
                    CandidateFullName = nomination.CandidateFullName,
                    ElectivePositionName =
                        nomination.ElectivePositionName,
                    IsActive = nomination.IsActive
                });
        }
        catch (BusinessRuleException exception)
        {
            return ShowLeaderError(exception);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Deactivate(
        int id,
        CancellationToken cancellationToken)
    {
        if (!TryGetLeaderId(out var leaderId))
        {
            return Forbid();
        }

        try
        {
            var updated = await _nominationService.DeactivateAsync(
                leaderId,
                id,
                cancellationToken);

            if (!updated)
            {
                return NotFound();
            }

            TempData["Success"] =
                "La asignación se retiró correctamente.";

            return RedirectToAction(nameof(Index));
        }
        catch (BusinessRuleException exception)
        {
            return ShowLeaderError(exception);
        }
    }

    private async Task LoadPositionsAsync(
        CreateNominationViewModel viewModel,
        int leaderId,
        CancellationToken cancellationToken)
    {
        var positions =
            await _nominationService.GetAvailablePositionsAsync(
                leaderId,
                cancellationToken);

        viewModel.ActivePositions = positions
            .Select(position => new SelectListItem(
                position.Name,
                position.Id.ToString()))
            .ToList();
    }

    private IActionResult ShowLeaderError(
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
}