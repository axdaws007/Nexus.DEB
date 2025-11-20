using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Application.Common.Models;

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

        protected List<ValidationError> ValidationErrors = new List<ValidationError>();

        protected Guid ModuleId { get; private set; }
        protected Guid InstanceId { get; private set; }

        public DomainServiceBase(
            ICisService cisService,
            ICbacService cbacService,
            IApplicationSettingsService applicationSettingsService,
            ICurrentUserService currentUserService,
            IDateTimeProvider dateTimeProvider,
            IDebService debService)
        {
            this.CisService = cisService;
            this.CbacService = cbacService;
            this.CurrentUserService = currentUserService;
            this.DateTimeProvider = dateTimeProvider;
            this.DebService = debService;
            this.ApplicationSettingsService = applicationSettingsService;


            this.ModuleId = this.ApplicationSettingsService.GetModuleId("DEB");
            this.InstanceId = this.ApplicationSettingsService.GetInstanceId();
        }
    }
}