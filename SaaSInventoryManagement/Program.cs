using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SaaSInventoryManagement.Data;
using SaaSInventoryManagement.Data.Interceptors;
using SaaSInventoryManagement.Middleware;
using SaaSInventoryManagement.Models.Identity;
using SaaSInventoryManagement.Services;
using SaaSInventoryManagement.Services.Interfaces_;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<ITenantProvider, TenantProvider>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<AuditSaveChangesInterceptor>();
builder.Services.AddScoped<NavigationService>();

builder.Services.AddDbContext<ApplicationDbContext>((sp, options) =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
    options.AddInterceptors(sp.GetRequiredService<AuditSaveChangesInterceptor>());
});

// Identity
builder.Services
    .AddIdentity<Applicationuser, IdentityRole>(options =>
    {
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.AllowedForNewUsers = true;
        options.SignIn.RequireConfirmedAccount = false;
        options.SignIn.RequireConfirmedEmail = false;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddMemoryCache();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddScoped<IUserClaimsPrincipalFactory<Applicationuser>, TenantClaimsPrincipalFactory>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
});

var app = builder.Build();

app.UseExceptionHandling();

app.UseHttpsRedirection();
app.UseCorrelationId();

app.UseRouting();

app.UseStatusCodePagesWithReExecute("/Home/Error/{0}");

app.MapStaticAssets();

app.UseAuthentication();
app.UseAuthorization();

app.UseTenantMiddleware();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

using (var scope = app.Services.CreateScope())
    await DbSeeder.SeedAsync(scope.ServiceProvider);

app.Run();