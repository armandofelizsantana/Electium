using Electium.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Electium.Application.Interfaces;
using Electium.Infrastructure.Repositories;
using Electium.Application.Mapping;
using Electium.Application.Services;
using Electium.Web.Mapping;
using Electium.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Electium.Web.Data;
using Electium.Infrastructure.Services;


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

builder.Services.AddScoped<
    IPoliticalLeaderService,
    PoliticalLeaderService>();

builder.Services.AddScoped<
    ILeaderCandidateRepository,
    LeaderCandidateRepository>();

builder.Services.AddScoped<
    ILeaderCandidateService,
    LeaderCandidateService>();

builder.Services.AddScoped<
    ILeaderNominationRepository,
    LeaderNominationRepository>();

builder.Services.AddScoped<
    ILeaderNominationService,
    LeaderNominationService>();
//Debajo

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddAutoMapper(
    cfg => { },
    typeof(CitizenMappingProfile),
    typeof(CitizenWebMappingProfile),
    typeof(ElectionWebMappingProfile),
    typeof(PoliticalPartyWebMappingProfile),
    typeof(CandidateWebMappingProfile));

builder.Services.AddScoped<
    IIdentityDocumentOcrService,
    TesseractIdentityDocumentOcrService>();

builder.Services.AddScoped(
    typeof(IGenericService<,,,>),
    typeof(GenericService<,,,>));

builder.Services.AddScoped<
    IPoliticalPartyService,
    PoliticalPartyService>();

builder.Services.AddScoped<
    IVoterEligibilityRepository,
    VoterEligibilityRepository>();

builder.Services.AddScoped<
    IVoterEligibilityService,
    VoterEligibilityService>();

builder.Services.AddAutoMapper(
    cfg => { },
    typeof(CitizenMappingProfile),
    typeof(CitizenWebMappingProfile),
    typeof(ElectionWebMappingProfile));

builder.Services.AddScoped<ICitizenRepository, CitizenRepository>();
builder.Services.AddScoped<ICitizenService, CitizenService>();

builder.Services.AddScoped<IElectivePositionService,
    ElectivePositionService>();

builder.Services.AddScoped<IElectivePositionRepository,
    ElectivePositionRepository>();

builder.Services.AddScoped<IElectionRepository, ElectionRepository>();

builder.Services.AddScoped<IElectionService, ElectionService>();


builder.Services.AddIdentity<ApplicationUser, IdentityRole<int>>(options =>
    {
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<ElectiumDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

builder.Services.AddScoped<
    IAuthenticationService,
    IdentityAuthenticationService>();

builder.Services.AddScoped<
    IPoliticalPartyRepository,
    PoliticalPartyRepository>();

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(10);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

await IdentitySeeder.SeedAsync(app.Services, app.Configuration);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
