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
public class PoliticalLeadersController : Controller
{
    private readonly IPoliticalLeaderService _leaderService;
    private readonly IPoliticalPartyService _partyService;

    public PoliticalLeadersController(
        IPoliticalLeaderService leaderService,
        IPoliticalPartyService partyService)
    {
        _leaderService = leaderService;
        _partyService = partyService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        CancellationToken cancellationToken)
    {
        var leaders = await _leaderService.GetAllAsync(cancellationToken);

        var viewModels = leaders.Select(leader =>
            new PoliticalLeaderListViewModel
            {
                Id = leader.Id,
                FirstName = leader.FirstName,
                LastName = leader.LastName,
                UserName = leader.UserName,
                Email = leader.Email,
                PoliticalPartyName = leader.PoliticalPartyName,
                IsActive = leader.IsActive
            }).ToList();

        return View(viewModels);
    }

    [HttpGet]
    public async Task<IActionResult> Create(
        CancellationToken cancellationToken)
    {
        var viewModel = new CreatePoliticalLeaderViewModel();
        await LoadActivePartiesAsync(viewModel, cancellationToken);

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        CreatePoliticalLeaderViewModel viewModel,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await LoadActivePartiesAsync(viewModel, cancellationToken);
            return View(viewModel);
        }

        var dto = new CreatePoliticalLeaderDto
        {
            FirstName = viewModel.FirstName,
            LastName = viewModel.LastName,
            UserName = viewModel.UserName,
            Email = viewModel.Email,
            Password = viewModel.Password,
            PoliticalPartyId = viewModel.PoliticalPartyId
        };

        try
        {
            await _leaderService.CreateAsync(dto, cancellationToken);

            TempData["Success"] =
                "El dirigente se creó y quedó asignado al partido.";

            return RedirectToAction(nameof(Index));
        }
        catch (BusinessRuleException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            await LoadActivePartiesAsync(viewModel, cancellationToken);
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
            var updated = await _leaderService.SetActiveAsync(
                id,
                isActive,
                cancellationToken);

            if (!updated)
            {
                return NotFound();
            }

            TempData["Success"] = isActive
                ? "El dirigente se activó correctamente."
                : "El dirigente se desactivó correctamente.";
        }
        catch (BusinessRuleException exception)
        {
            TempData["Error"] = exception.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task LoadActivePartiesAsync(
        CreatePoliticalLeaderViewModel viewModel,
        CancellationToken cancellationToken)
    {
        var parties = await _partyService.GetActiveAsync(cancellationToken);

        viewModel.ActiveParties = parties
            .Select(party => new SelectListItem(
                $"{party.Name} ({party.Acronym})",
                party.Id.ToString()))
            .ToList();
    }
}