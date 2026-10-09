using Electium.Application.Security;
using Electium.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace Electium.Web.Data;

public static class IdentitySeeder
{
    public static async Task SeedAsync(
        IServiceProvider services,
        IConfiguration configuration)
    {
        using var scope = services.CreateScope();

        var roleManager =
            scope.ServiceProvider.GetRequiredService<
                RoleManager<IdentityRole<int>>>();

        var userManager =
            scope.ServiceProvider.GetRequiredService<
                UserManager<ApplicationUser>>();

        await EnsureRoleAsync(
            roleManager,
            RoleNames.Administrator);

        await EnsureRoleAsync(
            roleManager,
            RoleNames.PoliticalLeader);

        var userName = configuration["BootstrapAdmin:UserName"];
        var email = configuration["BootstrapAdmin:Email"];
        var password = configuration["BootstrapAdmin:Password"];

        if (string.IsNullOrWhiteSpace(userName) ||
            string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException(
                "Faltan las credenciales BootstrapAdmin en User Secrets.");
        }

        var adminUser = await userManager.FindByNameAsync(userName);

        if (adminUser is null)
        {
            adminUser = new ApplicationUser
            {
                UserName = userName,
                Email = email,
                EmailConfirmed = true,
                FirstName = "Administrador",
                LastName = "Inicial",
                IsActive = true,
                CreatedAtUtc = DateTime.UtcNow
            };

            var createResult =
                await userManager.CreateAsync(adminUser, password);

            if (!createResult.Succeeded)
            {
                var errors = string.Join(
                    "; ",
                    createResult.Errors.Select(error => error.Description));

                throw new InvalidOperationException(
                    $"No se pudo crear el administrador inicial: {errors}");
            }
        }

        if (!await userManager.IsInRoleAsync(
                adminUser,
                RoleNames.Administrator))
        {
            var roleResult = await userManager.AddToRoleAsync(
                adminUser,
                RoleNames.Administrator);

            if (!roleResult.Succeeded)
            {
                var errors = string.Join(
                    "; ",
                    roleResult.Errors.Select(error => error.Description));

                throw new InvalidOperationException(
                    $"No se pudo asignar el rol administrador: {errors}");
            }
        }
    }

    private static async Task EnsureRoleAsync(
        RoleManager<IdentityRole<int>> roleManager,
        string roleName)
    {
        if (await roleManager.RoleExistsAsync(roleName))
        {
            return;
        }

        var result = await roleManager.CreateAsync(
            new IdentityRole<int>(roleName));

        if (!result.Succeeded)
        {
            var errors = string.Join(
                "; ",
                result.Errors.Select(error => error.Description));

            throw new InvalidOperationException(
                $"No se pudo crear el rol '{roleName}': {errors}");
        }
    }
}