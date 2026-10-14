using AutoMapper;
using Electium.Application.Interfaces;
using Electium.Application.Security;
using Electium.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Electium.Web.Controllers;

[Authorize(Roles = RoleNames.Administrator)]
public class AdminController : Controller
{
    private readonly IElectionService _electionService;
    private readonly IMapper _mapper;

    public AdminController(
        IElectionService electionService,
        IMapper mapper)
    {
        _electionService = electionService;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        int? year,
        CancellationToken cancellationToken)
    {
        var summaries = await _electionService.GetSummariesAsync(
            cancellationToken);

        var availableYears = summaries
            .Select(election => election.Year)
            .Distinct()
            .OrderByDescending(electionYear => electionYear)
            .ToList();

        int? selectedYear = null;

        if (availableYears.Count > 0)
        {
            selectedYear =
                year.HasValue && availableYears.Contains(year.Value)
                    ? year.Value
                    : availableYears[0];
        }

        var selectedSummaries = selectedYear.HasValue
            ? summaries
                .Where(election => election.Year == selectedYear.Value)
                .ToList()
            : [];

        var viewModel = new AdminDashboardViewModel
        {
            AvailableYears = availableYears,
            SelectedYear = selectedYear,
            ElectionSummaries =
                _mapper.Map<List<AdminElectionSummaryViewModel>>(
                    selectedSummaries)
        };

        return View(viewModel);
    }
}