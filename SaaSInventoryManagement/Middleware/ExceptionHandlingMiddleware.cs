using Microsoft.Extensions.Caching.Memory;
using SaaSInventoryManagement.Exceptions;
using SaaSInventoryManagement.ViewModels;
using System.Net;

namespace SaaSInventoryManagement.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;
        private readonly IMemoryCache _cache;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger, IMemoryCache cache)
        {
            _next = next;
            _logger = logger;
            _cache = cache;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                var correlationId = context.Items[CorrelationIdMiddleware.ItemsKey]?.ToString() ?? Guid.NewGuid().ToString();

                _logger.LogError(ex, "Unhandled exception while processing {Path}", context.Request.Path);

                await HandleExceptionAsync(context, ex, correlationId);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception, string correlationId)
        {
            if (context.Response.HasStarted)
            {
                _logger.LogWarning(
                    "Cannot handle exception: response already started for {Path}. CorrelationId: {CorrelationId}",
                    context.Request.Path, correlationId);
                return;
            }

            var statusCode = exception switch
            {
                AppException appException => (int)appException.StatusCode,
                ArgumentException => (int)HttpStatusCode.BadRequest,
                KeyNotFoundException => (int)HttpStatusCode.NotFound,
                UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
                _ => (int)HttpStatusCode.InternalServerError
            };

            var message = exception is AppException
                ? exception.Message
                : "An unexpected error occurred. Please try again, and share the reference ID below with support if it keeps happening.";

            var errorModel = new ErrorViewModel
            {
                StatusCode = statusCode,
                Message = message,
                CorrelationId = correlationId
            };

            if (IsJsonRequest(context))
            {
                context.Response.Clear();
                context.Response.StatusCode = statusCode;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new
                {
                    statusCode,
                    message,
                    correlationId
                });
                return;
            }

            _cache.Set($"error:{correlationId}", errorModel, TimeSpan.FromMinutes(5));
            context.Response.Redirect($"/Home/Error?correlationId={correlationId}");
        }

        private static bool IsJsonRequest(HttpContext context)
        {
            // Path-based: anything under /api is treated as an API call
            if (context.Request.Path.StartsWithSegments("/api"))
                return true;

            // Header-based: client explicitly wants JSON (fetch/AJAX/API clients)
            var acceptHeader = context.Request.Headers.Accept.ToString();
            if (acceptHeader.Contains("application/json"))
                return true;

            // XHR convention some JS frameworks still set
            if (context.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return true;

            return false;
        }
    }

    public static class ExceptionHandlingMiddlewareExtensions
    {
        public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder app)
        {
            return app.UseMiddleware<ExceptionHandlingMiddleware>();
        }
    }
}