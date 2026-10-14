using System.Security.Claims;
using Electium.Application.Interfaces;
using Electium.Application.Security;
using Electium.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Electium.Web.Controllers;

[Authorize(Roles = RoleNames.PoliticalLeader)]
public class LeaderController : Controller
{
    private readonly IPoliticalLeaderService _leaderService;

    public LeaderController(IPoliticalLeaderService leaderService)
    {
        _leaderService = leaderService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        CancellationToken cancellationToken)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdValue, out var userId))
        {
            return Forbid();
        }

        var leader = await _leaderService.GetByIdAsync(
            userId,
            cancellationToken);

        if (leader is null)
        {
            return Forbid();
        }

        var viewModel = new LeaderDashboardViewModel
        {
            FullName = $"{leader.FirstName} {leader.LastName}",
            PoliticalPartyName = leader.PoliticalPartyName
        };

        return View(viewModel);
    }
}