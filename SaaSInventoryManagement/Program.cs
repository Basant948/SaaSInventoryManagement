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

builder.Services.AddDbContext<ApplicationDbContext>((sp, options) =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
    options.AddInterceptors(sp.GetRequiredService<AuditSaveChangesInterceptor>());
});

builder.Services
    .AddIdentity<Applicationuser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddMemoryCache();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddScoped<IUserClaimsPrincipalFactory<Applicationuser>, TenantClaimsPrincipalFactory>();

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

app.Run();