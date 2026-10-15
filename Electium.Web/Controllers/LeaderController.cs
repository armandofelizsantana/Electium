using System.Security.Claims;
using AutoMapper;
using Electium.Application.Exceptions;
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
    private readonly IPoliticalPartyService _partyService;
    private readonly ILeaderCandidateService _candidateService;
    private readonly ILeaderNominationService _nominationService;
    private readonly ILeaderAllianceService _allianceService;
    private readonly IMapper _mapper;

    public LeaderController(
        IPoliticalLeaderService leaderService,
        IPoliticalPartyService partyService,
        ILeaderCandidateService candidateService,
        ILeaderNominationService nominationService,
        ILeaderAllianceService allianceService,
        IMapper mapper)
    {
        _leaderService = leaderService;
        _partyService = partyService;
        _candidateService = candidateService;
        _nominationService = nominationService;
        _allianceService = allianceService;
        _mapper = mapper;
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

        var viewModel = _mapper.Map<LeaderDashboardViewModel>(leader);

        if (leader.PoliticalPartyId.HasValue)
        {
            var party = await _partyService.GetByIdAsync(
                leader.PoliticalPartyId.Value,
                cancellationToken);

            if (party is not null)
            {
                _mapper.Map(party, viewModel);
            }

            try
            {
                var candidates = await _candidateService.GetAllForLeaderAsync(
                    userId,
                    cancellationToken);

                viewModel.ActiveCandidateCount =
                    candidates.Count(candidate => candidate.IsActive);

                viewModel.InactiveCandidateCount =
                    candidates.Count(candidate => !candidate.IsActive);

                var nominations = await _nominationService.GetAllAsync(
                    userId,
                    cancellationToken);

                viewModel.AssignedCandidateCount =
                    nominations.Count(nomination => nomination.IsActive);

                var allianceDashboard =
                    await _allianceService.GetDashboardAsync(
                        userId,
                        cancellationToken);

                viewModel.ActiveAllianceCount =
                    allianceDashboard.ActiveAlliances.Count;

                viewModel.PendingIncomingAllianceRequestCount =
                    allianceDashboard.IncomingRequests.Count;
            }
            catch (BusinessRuleException exception)
            {
                TempData["Error"] = exception.Message;
            }
        }

        return View(viewModel);
    }
}