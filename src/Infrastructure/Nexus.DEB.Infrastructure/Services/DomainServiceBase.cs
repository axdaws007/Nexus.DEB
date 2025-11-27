using Microsoft.AspNetCore.DataProtection.XmlEncryption;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Application.Common.Models;
using Nexus.DEB.Domain.Models.Common;

namespace Nexus.DEB.Infrastructure.Services
{
    public abstract class DomainServiceBase
    {
        protected readonly ICbacService CbacService;
        protected readonly ICisService CisService;
        protected readonly ICurrentUserService CurrentUserService;
        protected readonly IDateTimeProvider DateTimeProvider;
        protected readonly IDebService DebService;
        protected readonly IApplicationSettingsService ApplicationSettingsService;
        protected readonly IPawsService PawsService;

        protected List<ValidationError> ValidationErrors = new List<ValidationError>();

        protected Guid ModuleId { get; init; }
        protected Guid InstanceId { get; init; }
        protected Guid? WorkflowId { get; init; }

        public DomainServiceBase(
            ICisService cisService,
            ICbacService cbacService,
            IApplicationSettingsService applicationSettingsService,
            ICurrentUserService currentUserService,
            IDateTimeProvider dateTimeProvider,
            IDebService debService,
            IPawsService pawsService,
            string entityType)
        {
            this.CisService = cisService;
            this.CbacService = cbacService;
            this.CurrentUserService = currentUserService;
            this.DateTimeProvider = dateTimeProvider;
            this.DebService = debService;
            this.ApplicationSettingsService = applicationSettingsService;
            this.PawsService = pawsService;

            this.ModuleId = this.ApplicationSettingsService.GetModuleId("DEB");
            this.InstanceId = this.ApplicationSettingsService.GetInstanceId();

            if (!string.IsNullOrEmpty(entityType))
            {
                this.WorkflowId = debService.GetWorkflowId(this.ModuleId, entityType);
            }
        }
    }
}