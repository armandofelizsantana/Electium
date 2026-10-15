using AutoMapper;
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
public class UsersController : Controller
{
    private readonly IUserManagementService _userService;
    private readonly IMapper _mapper;

    public UsersController(
        IUserManagementService userService,
        IMapper mapper)
    {
        _userService = userService;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        CancellationToken cancellationToken)
    {
        var users = await _userService.GetAllAsync(cancellationToken);
        var viewModels = _mapper.Map<List<UserListViewModel>>(users);

        return View(viewModels);
    }

    [HttpGet]
    public IActionResult Create()
    {
        var viewModel = new CreateUserViewModel();
        LoadRoles(viewModel);

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        CreateUserViewModel viewModel,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            LoadRoles(viewModel);
            return View(viewModel);
        }

        try
        {
            var dto = _mapper.Map<CreateSystemUserDto>(viewModel);
            await _userService.CreateAsync(dto, cancellationToken);

            TempData["Success"] = "El usuario se creó correctamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (BusinessRuleException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            LoadRoles(viewModel);
            return View(viewModel);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(
        int id,
        CancellationToken cancellationToken)
    {
        var user = await _userService.GetByIdAsync(id, cancellationToken);

        if (user is null)
        {
            return NotFound();
        }

        var viewModel = _mapper.Map<EditUserViewModel>(user);
        LoadRoles(viewModel);

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        EditUserViewModel viewModel,
        CancellationToken cancellationToken)
    {
        if (id != viewModel.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            LoadRoles(viewModel);
            return View(viewModel);
        }

        try
        {
            var dto = _mapper.Map<UpdateSystemUserDto>(viewModel);
            var updatedUser = await _userService.UpdateAsync(
                dto,
                cancellationToken);

            if (updatedUser is null)
            {
                return NotFound();
            }

            TempData["Success"] = "Los cambios del usuario se guardaron.";
            return RedirectToAction(nameof(Index));
        }
        catch (BusinessRuleException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            LoadRoles(viewModel);
            return View(viewModel);
        }
    }

    [HttpGet]
    public async Task<IActionResult> ConfirmSetActive(
        int id,
        CancellationToken cancellationToken)
    {
        var user = await _userService.GetByIdAsync(id, cancellationToken);

        if (user is null)
        {
            return NotFound();
        }

        var viewModel = _mapper.Map<ConfirmUserStatusViewModel>(user);
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetActive(
        int id,
        string operation,
        CancellationToken cancellationToken)
    {
        bool desiredState;

        if (operation == "activate")
        {
            desiredState = true;
        }
        else if (operation == "deactivate")
        {
            desiredState = false;
        }
        else
        {
            return BadRequest();
        }

        try
        {
            var updated = await _userService.SetActiveAsync(
                id,
                desiredState,
                cancellationToken);

            if (!updated)
            {
                return NotFound();
            }

            TempData["Success"] = desiredState
                ? "El usuario se activó correctamente."
                : "El usuario se desactivó correctamente.";
        }
        catch (BusinessRuleException exception)
        {
            TempData["Error"] = exception.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    private static void LoadRoles(CreateUserViewModel viewModel)
    {
        viewModel.Roles = GetRoleOptions();
    }

    private static void LoadRoles(EditUserViewModel viewModel)
    {
        viewModel.Roles = GetRoleOptions();
    }

    private static IEnumerable<SelectListItem> GetRoleOptions()
    {
        return
        [
            new SelectListItem(
                RoleNames.Administrator,
                RoleNames.Administrator),
            new SelectListItem(
                RoleNames.PoliticalLeader,
                RoleNames.PoliticalLeader)
        ];
    }
}