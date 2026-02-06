using Microsoft.AspNetCore.Http;
using Nexus.DEB.Application.Common.Interfaces;

namespace Nexus.DEB.Infrastructure.Services
{
    public class CorrelationIdAccessor : ICorrelationIdAccessor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CorrelationIdAccessor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string? CorrelationId =>
            _httpContextAccessor.HttpContext?.Items["CorrelationId"] as string;
    }
}
