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

        protected List<ValidationError> validationErrors = new List<ValidationError>();

        protected Guid moduleId { get; private set; }

        public DomainServiceBase(
            ICisService cisService,
            ICbacService cbacService,
            IConfiguration configuration,
            ICurrentUserService currentUserService,
            IDateTimeProvider dateTimeProvider,
            IDebService debService)
        {
            this.cisService = cisService;
            this.cbacService = cbacService;
            this.configuration = configuration;
            this.currentUserService = currentUserService;
            this.dateTimeProvider = dateTimeProvider;
            this.debService = debService;

            var moduleIdString = configuration["Modules:DEB"] ?? throw new InvalidOperationException("Modules:DEB not configured in appsettings");

            if (!Guid.TryParse(moduleIdString, out var moduleId))
            {
                throw new InvalidOperationException("Modules:DEB must be a valid GUID");
            }

            this.moduleId = moduleId;
        }
    }
}