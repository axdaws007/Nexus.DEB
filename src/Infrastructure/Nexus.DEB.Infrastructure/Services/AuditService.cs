using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Nexus.DEB.Application.Common.Extensions;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Application.Common.Models;
using System.Net.Http.Json;
using System.Text.Json;

namespace Nexus.DEB.Infrastructure.Services
{
    public class AuditService : LegacyApiServiceBase<AuditService>, IAuditService
    {
        protected override string HttpClientName => "AuditApi";

        private readonly IApplicationSettingsService _applicationSettingsService;

        public AuditService(
            IHttpClientFactory httpClientFactory,
            ILogger<AuditService> logger,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration,
            IApplicationSettingsService applicationSettingsService) : base(httpClientFactory, logger, httpContextAccessor, configuration)
        {
            _applicationSettingsService = applicationSettingsService;
        }

        public async Task DataExported(object? entityId, string entityTypeTitle, string eventContext, UserDetails? userDetails, AuditData? data = default)
            => await GenerateAuditAsync("DataExported", entityId, entityTypeTitle, eventContext, userDetails, data);

        public async Task DataImported(object? entityId, string entityTypeTitle, string eventContext, UserDetails? userDetails, AuditData? data = default)
            => await GenerateAuditAsync("DataImported", entityId, entityTypeTitle, eventContext, userDetails, data);

        public async Task EntityDeleted(object entityId, string entityTypeTitle, string eventContext, UserDetails? userDetails, AuditData? data = default)
            => await GenerateAuditAsync("EntityDeleted", entityId, entityTypeTitle, eventContext, userDetails, data);

        public async Task EntityRead(object entityId, string entityTypeTitle, string eventContext, UserDetails? userDetails, AuditData? data = default)
            => await GenerateAuditAsync("EntityRead", entityId, entityTypeTitle, eventContext, userDetails, data);

        public async Task EntitySaved(object entityId, string entityTypeTitle, string eventContext, UserDetails? userDetails, AuditData? data = default)
            => await GenerateAuditAsync("EntitySaved", entityId, entityTypeTitle, eventContext, userDetails, data);

        public async Task ReportGenerated(object entityId, string entityTypeTitle, string eventContext, UserDetails? userDetails, AuditData? data = default)
            => await GenerateAuditAsync("ReportGenerated", entityId, entityTypeTitle, eventContext, userDetails, data);

        public async Task WorkflowSignoff(object entityId, string entityTypeTitle, string eventContext, UserDetails? userDetails, AuditData? data = default)
            => await GenerateAuditAsync("WorkflowSignoff", entityId, entityTypeTitle, eventContext, userDetails, data);

        private async Task GenerateAuditAsync(string eventType, object? entityId, string entityTypeTitle, string eventContext, UserDetails? userDetails, AuditData? data = default)
        {
            try
            {
                var auditConfiguration = _applicationSettingsService.GetAuditConfiguration();

                // Create request DTO
                var request = new AuditRequest
                {
                    EntityId = entityId?.ToJsonElement(),
                    EntityType = entityTypeTitle,
                    PlatformTeam = auditConfiguration.PlatformTeam,
                    ApplicationName = auditConfiguration.ApplicationName,
                    ApplicationInstance = auditConfiguration.ApplicationInstance,
                    EnvironmentName = auditConfiguration.EnvironmentName,
                    UserId = userDetails.UserId,
                    UserName = userDetails.UserName,
                    PostId = userDetails.PostId,
                    PostName = userDetails.PostTitle,
                    Data = data?.Data,
                    DataTypeName = data?.TypeName,
                    EventContext = eventContext,
                    EventType = eventType
                };

                // Create JSON content (JsonOptions from base class)
                var content = JsonContent.Create(request, options: JsonOptions);

                // Use base class method - it gets the auth cookie from HttpContext automatically!
                await SendAuthenticatedRequestAsync(
                    HttpMethod.Post,
                    "api/Audit",
                    operationName: $"Generate audit message for {entityId} entityId",
                    content: content);

                return;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error fetching batch workflow statuses");
                throw;
            }
        }

    }
}
