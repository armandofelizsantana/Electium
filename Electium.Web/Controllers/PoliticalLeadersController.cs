using Electium.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Electium.Web.Controllers;

[Authorize(Roles = RoleNames.Administrator)]
public class PoliticalLeadersController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return RedirectToAction(
            "Index",
            "LeaderPartyAssignments");
    }
}