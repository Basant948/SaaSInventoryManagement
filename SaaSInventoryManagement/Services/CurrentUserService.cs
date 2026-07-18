using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using SaaSInventoryManagement.Services.Interfaces_;

namespace SaaSInventoryManagement.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string UserId =>
            _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        public string Username =>
            _httpContextAccessor.HttpContext?.User?.Identity?.Name;

        public string CorrelationId =>
            _httpContextAccessor.HttpContext?.TraceIdentifier;

        public string IPAddress =>
            _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();

        public string RequestPath
        {
            get
            {
                var request = _httpContextAccessor.HttpContext?.Request;
                return request is null ? null : $"{request.Method} {request.Path}";
            }
        }
    }
}