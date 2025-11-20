using Microsoft.Extensions.Configuration;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Application.Common.Models;

namespace Nexus.DEB.Infrastructure.Services
{
    public abstract class DomainServiceBase
    {
        protected readonly ICbacService cbacService;
        protected readonly ICisService cisService;
        protected readonly IConfiguration configuration;
        protected readonly ICurrentUserService currentUserService;
        protected readonly IDateTimeProvider dateTimeProvider;
        protected readonly IDebService debService;
        protected readonly IApplicationSettingsService applicationSettingsService;

        protected List<ValidationError> validationErrors = new List<ValidationError>();

        protected Guid moduleId { get; private set; }
        protected Guid instanceId { get; private set; }

        public DomainServiceBase(
            ICisService cisService,
            ICbacService cbacService,
            IConfiguration configuration,
            ICurrentUserService currentUserService,
            IDateTimeProvider dateTimeProvider,

            IDebService debService,
            IApplicationSettingsService applicationSettingsService)
        {
            this.cisService = cisService;
            this.cbacService = cbacService;
            this.configuration = configuration;
            this.currentUserService = currentUserService;
            this.dateTimeProvider = dateTimeProvider;
            this.debService = debService;
            this.applicationSettingsService = applicationSettingsService;


            this.moduleId = this.applicationSettingsService.GetModuleId("DEB");
            this.instanceId = this.applicationSettingsService.GetInstanceId();
        }
    }
}