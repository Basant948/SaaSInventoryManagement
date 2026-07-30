using SaaSInventoryManagement.Services.Interfaces_;

namespace SaaSInventoryManagement.Middleware
{
    public class TenantMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<TenantMiddleware> _logger;

        public TenantMiddleware(RequestDelegate next, ILogger<TenantMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, ITenantProvider tenantProvider)
        {
            if (context.User?.Identity?.IsAuthenticated == true
                && !tenantProvider.IsSuperAdmin
                && tenantProvider.TenantId is null)
            {

                _logger.LogWarning(
                    "Blocked request from authenticated user '{User}' with no resolvable tenant_id claim. Path: {Path}",
                    context.User.Identity?.Name, context.Request.Path);

                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsync("No tenant is associated with this account. Contact your administrator.");
                return;
            }

            await _next(context);
        }
    }

    public static class TenantMiddlewareExtensions
    {
        public static IApplicationBuilder UseTenantMiddleware(this IApplicationBuilder app) =>
            app.UseMiddleware<TenantMiddleware>();
    }
}
