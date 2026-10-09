using Electium.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Electium.Web.Controllers;

[Authorize(Roles = RoleNames.PoliticalLeader)]
public class LeaderController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}