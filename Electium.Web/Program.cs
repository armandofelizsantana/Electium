using Electium.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Electium.Application.Interfaces;
using Electium.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

var connectionString = builder.Configuration
    .GetConnectionString("ElectiumConnection")
    ?? throw new InvalidOperationException(
        "No se encontró la cadena de conexión 'ElectiumConnection'.");

builder.Services.AddDbContext<ElectiumDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped(
    typeof(IGenericRepository<>),
    typeof(GenericRepository<>));

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
