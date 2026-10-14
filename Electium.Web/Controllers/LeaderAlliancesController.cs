using System.Security.Claims;
using AutoMapper;
using Electium.Application.Exceptions;
using Electium.Application.Interfaces;
using Electium.Application.Security;
using Electium.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Electium.Domain.Enums;

namespace Electium.Web.Controllers;

[Authorize(Roles = RoleNames.PoliticalLeader)]
public class LeaderAlliancesController : Controller
{
    private readonly ILeaderAllianceService _allianceService;
    private readonly IMapper _mapper;

    public LeaderAlliancesController(
        ILeaderAllianceService allianceService,
        IMapper mapper)
    {
        _allianceService = allianceService;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        CancellationToken cancellationToken)
    {
        try
        {
            var dashboard = await _allianceService.GetDashboardAsync(
                GetLeaderUserId(),
                cancellationToken);

            return View(
                _mapper.Map<LeaderAllianceDashboardViewModel>(dashboard));
        }
        catch (BusinessRuleException exception)
        {
            TempData["Error"] = exception.Message;
            return View(new LeaderAllianceDashboardViewModel());
        }
    }

    [HttpGet]
    public async Task<IActionResult> Create(
        CancellationToken cancellationToken)
    {
        var viewModel = new CreateAllianceRequestViewModel();

        await PopulateAvailablePartiesAsync(
            viewModel,
            cancellationToken);

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        CreateAllianceRequestViewModel viewModel,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await PopulateAvailablePartiesAsync(
                viewModel,
                cancellationToken);

            return View(viewModel);
        }

        try
        {
            await _allianceService.CreateRequestAsync(
                GetLeaderUserId(),
                viewModel.ReceivingPartyId,
                cancellationToken);

            TempData["Success"] =
                "La solicitud de alianza se envió correctamente.";

            return RedirectToAction(nameof(Index));
        }
        catch (BusinessRuleException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);

            await PopulateAvailablePartiesAsync(
                viewModel,
                cancellationToken);

            return View(viewModel);
        }
    }

    [HttpGet]
    public async Task<IActionResult> ConfirmResponse(
        int id,
        bool accept,
        CancellationToken cancellationToken)
    {
        var dashboard = await _allianceService.GetDashboardAsync(
            GetLeaderUserId(),
            cancellationToken);

        var request = dashboard.IncomingRequests
            .FirstOrDefault(item => item.Id == id);

        if (request is null)
        {
            return NotFound();
        }

        return View(new ConfirmAllianceResponseViewModel
        {
            Id = request.Id,
            Accept = accept,
            OtherPartyName = request.RequestingPartyName,
            OtherPartyAcronym = request.RequestingPartyAcronym
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Respond(
        int id,
        bool accept,
        CancellationToken cancellationToken)
    {
        try
        {
            await _allianceService.RespondToRequestAsync(
                GetLeaderUserId(),
                id,
                accept,
                cancellationToken);

            TempData["Success"] = accept
                ? "La solicitud fue aceptada y la alianza quedó activa."
                : "La solicitud fue rechazada.";
        }
        catch (BusinessRuleException exception)
        {
            TempData["Error"] = exception.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> ConfirmDelete(
        int id,
        CancellationToken cancellationToken)
    {
        var dashboard = await _allianceService.GetDashboardAsync(
            GetLeaderUserId(),
            cancellationToken);

        var request = dashboard.OutgoingRequests
            .FirstOrDefault(item => item.Id == id);

        if (request is null)
        {
            return NotFound();
        }

        if (request.Status == AllianceRequestStatus.Accepted)
        {
            TempData["Error"] =
                "La solicitud ya fue aceptada y la alianza continúa vigente.";

            return RedirectToAction(nameof(Index));
        }

        return View(new ConfirmDeleteAllianceRequestViewModel
        {
            Id = request.Id,
            ReceivingPartyName = request.ReceivingPartyName,
            ReceivingPartyAcronym = request.ReceivingPartyAcronym
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(
        int id,
        CancellationToken cancellationToken)
    {
        try
        {
            var deleted = await _allianceService.DeleteOutgoingRequestAsync(
                GetLeaderUserId(),
                id,
                cancellationToken);

            if (!deleted)
            {
                return NotFound();
            }

            TempData["Success"] =
                "La solicitud se retiró correctamente.";
        }
        catch (BusinessRuleException exception)
        {
            TempData["Error"] = exception.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateAvailablePartiesAsync(
        CreateAllianceRequestViewModel viewModel,
        CancellationToken cancellationToken)
    {
        var parties = await _allianceService.GetAvailablePartiesAsync(
            GetLeaderUserId(),
            cancellationToken);

        viewModel.AvailableParties = parties
            .Select(party => new SelectListItem
            {
                Value = party.Id.ToString(),
                Text = $"{party.Name} ({party.Acronym})"
            })
            .ToList();
    }

    private int GetLeaderUserId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userId, out var parsedUserId))
        {
            throw new UnauthorizedAccessException(
                "No se pudo identificar la cuenta del dirigente.");
        }

        return parsedUserId;
    }
}