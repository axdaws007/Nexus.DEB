using Nexus.DEB.Application.Common.Interfaces;

namespace Nexus.DEB.Infrastructure.Http
{
    public class CorrelationIdDelegatingHandler : DelegatingHandler
    {
        private readonly ICorrelationIdAccessor _correlationIdAccessor;
        private const string CorrelationIdHeaderName = "X-Correlation-ID";

        public CorrelationIdDelegatingHandler(ICorrelationIdAccessor correlationIdAccessor)
        {
            _correlationIdAccessor = correlationIdAccessor;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var correlationId = _correlationIdAccessor.CorrelationId;

            if (!string.IsNullOrEmpty(correlationId) &&
                !request.Headers.Contains(CorrelationIdHeaderName))
            {
                request.Headers.Add(CorrelationIdHeaderName, correlationId);
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
