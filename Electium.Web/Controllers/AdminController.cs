using Electium.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Electium.Web.Controllers;

[Authorize(Roles = RoleNames.Administrator)]
public class AdminController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}