using Electium.Application.Interfaces;
using Electium.Application.Security;
using Electium.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Electium.Web.Controllers;

public class AccountController : Controller
{
    private readonly IAuthenticationService _authenticationService;

    public AccountController(IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login()
    {
        return View(new LoginViewModel());
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _authenticationService.LoginAsync(
            model.UserName,
            model.Password);

        if (result.Status != LoginStatus.Success)
        {
            var message = result.Status switch
            {
                LoginStatus.Inactive =>
                    "El usuario esta inactivo",

                LoginStatus.PartyNotAssigned =>
                    "No tiene un partido politico asignado, por lo tanto no puede iniciar sesion, mire a ver si se pone en contacto con un administrador",

                LoginStatus.UnauthorizedRole =>
                    "El usuario no tiene un rol autorizado para acceder al sistema",

                _ =>
                    "Los datos de acceso son invalidos"
            };

            ModelState.AddModelError(string.Empty, message);
            return View(model);
        }

        if (result.RoleName == RoleNames.Administrator)
        {
            return RedirectToAction("Index", "Admin");
        }

        if (result.RoleName == RoleNames.PoliticalLeader)
        {
            return RedirectToAction("Index", "Leader");
        }

        await _authenticationService.LogoutAsync();
        return RedirectToAction(nameof(AccessDenied));
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _authenticationService.LogoutAsync();
        return RedirectToAction("Index", "Home");
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }
}