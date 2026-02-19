using Microsoft.Extensions.Logging;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Application.Common.Models;
using System.Globalization;
using System.Text.RegularExpressions;

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
        protected readonly IAuditService AuditService;
        protected readonly ILogger<DomainServiceBase> Logger;

		protected UserDetails? UserDetails { get; init; }

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
            IAuditService auditService,
			ILogger<DomainServiceBase> logger,
			string entityType)
        {
            this.CisService = cisService;
            this.CbacService = cbacService;
            this.CurrentUserService = currentUserService;
            this.DateTimeProvider = dateTimeProvider;
            this.DebService = debService;
            this.ApplicationSettingsService = applicationSettingsService;
            this.PawsService = pawsService;
            this.AuditService = auditService;
            this.Logger = logger;

            this.ModuleId = this.ApplicationSettingsService.GetModuleId("DEB");
            this.InstanceId = this.ApplicationSettingsService.GetInstanceId();

            if (!string.IsNullOrEmpty(entityType))
            {
                this.WorkflowId = debService.GetWorkflowId(this.ModuleId, entityType);
            }
        }

        protected virtual void ValidateTitle(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                ValidationErrors.Add(
                    new ValidationError()
                    {
                        Code = "INVALID_TITLE",
                        Field = nameof(title),
                        Message = "The 'title' is empty."
                    });
            }
        }

        protected virtual void ValidateString(string stringToValidate, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(stringToValidate))
            {
                ValidationErrors.Add(
                    new ValidationError()
                    {
                        Code = $"INVALID_{fieldName.ToUpper()}",
                        Field = nameof(fieldName),
                        Message = $"The '{ToTitleFromCamel(fieldName)}' is empty."
                    });
            }
        }

        protected virtual async Task ValidateOwnerAsync(Guid ownerId)
        {
            var posts = await CisService.GetAllPosts();

            if (posts != null && posts.FirstOrDefault(x => x.ID == ownerId) == null)
            {
                ValidationErrors.Add(
                    new ValidationError()
                    {
                        Code = "INVALID_OWNER",
                        Field = nameof(ownerId),
                        Message = "The 'Owner ID' provided does not exist as a valid Post."
                    });
            }
        }

        private static string ToTitleFromCamel(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return input;

            // Insert space before capital letters
            var withSpaces = Regex.Replace(input, "([a-z])([A-Z])", "$1 $2");

            // Capitalize first letter of each word
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(withSpaces);
        }

    }
}