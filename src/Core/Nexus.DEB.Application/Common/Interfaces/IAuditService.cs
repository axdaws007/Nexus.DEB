using Nexus.DEB.Application.Common.Models;

namespace Nexus.DEB.Application.Common.Interfaces
{
    public interface IAuditService
    {
        Task DataExported(object entityId, string entityTypeTitle, string eventContext, UserDetails? userDetails, AuditData? data = default);
        Task DataImported(object entityId, string entityTypeTitle, string eventContext, UserDetails? userDetails, AuditData? data = default);
        Task EntityDeleted(object entityId, string entityTypeTitle, string eventContext, UserDetails? userDetails, AuditData? data = default);
        Task EntityRead(object entityId, string entityTypeTitle, string eventContext, UserDetails? userDetails, AuditData? data = default);
        Task EntitySaved(object entityId, string entityTypeTitle, string eventContext, UserDetails? userDetails, AuditData? data = default);
        Task ReportGenerated(object entityId, string entityTypeTitle, string eventContext, UserDetails? userDetails, AuditData? data = default);
        Task WorkflowSignoff(object entityId, string entityTypeTitle, string eventContext, UserDetails? userDetails, AuditData? data = default);
    }
}
