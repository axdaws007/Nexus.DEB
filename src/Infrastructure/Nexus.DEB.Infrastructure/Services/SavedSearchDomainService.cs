using Microsoft.Extensions.Logging;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Application.Common.Models;
using Nexus.DEB.Domain.Models;
using System.Text.Json;
using System.Threading;
using Task = System.Threading.Tasks.Task;

namespace Nexus.DEB.Infrastructure.Services
{
	public class SavedSearchDomainService : DomainServiceBase, ISavedSearchDomainService
	{
		public SavedSearchDomainService(ICisService cisService,
			ICbacService cbacService,
			IApplicationSettingsService applicationSettingsService,
			ICurrentUserService currentUserService,
			IDateTimeProvider dateTimeProvider,
			IDebService debService,
			IPawsService pawsService,
			IAuditService auditService,
			ILogger<SavedSearchDomainService> logger) : base(cisService, cbacService, applicationSettingsService, currentUserService, dateTimeProvider, debService, pawsService, auditService, logger, string.Empty)
		{
		}

		public async Task<Result<SavedSearch>> SaveSavedSearchAsync(string context, string name, string filter, CancellationToken cancellationToken)
		{
			await ValidateFieldsAsync(context, name, filter);

			if (ValidationErrors.Count > 0)
			{
				return Result<SavedSearch>.Failure(ValidationErrors);
			}

			try
			{
				SavedSearch savedSearch;

				var savedSearches = await DebService.GetSavedSearchesByContextAsync(context, cancellationToken);
				if (savedSearches.Any(a => a.Name == name))
				{
					savedSearch = savedSearches.First(a => a.Name == name);
					savedSearch.Filter = filter;
					savedSearch.LastModifiedDate = DateTimeProvider.Now;
				}
				else
				{
					savedSearch = new SavedSearch()
					{
						PostId = CurrentUserService.PostId,
						Context = context,
						Name = name,
						Filter = filter,
						CreatedDate = DateTimeProvider.Now
					};
				}

				var dbSavedSearch = await DebService.SaveSavedSearchAsync(savedSearch, !savedSearches.Any(a => a.Name == name), cancellationToken);

				if (dbSavedSearch == null)
				{
					return Result<SavedSearch>.Failure("Saved Search was not created.");
				}

				return Result<SavedSearch>.Success(dbSavedSearch);
			}
			catch (Exception ex)
			{
				return Result<SavedSearch>.Failure($"An error occurred creating the Saved Search: {ex.Message}");
			}
		}

		private async Task ValidateFieldsAsync(string context, string name, string filter)
		{
			ValidateContext(context);
			
			ValidateName(name, context);

			ValidateFilter(filter);
		}

		private void ValidateContext(string context)
		{
			if (string.IsNullOrWhiteSpace(context))
			{
				ValidationErrors.Add(
					new ValidationError()
					{
						Code = "INVALID_CONTEXT",
						Field = nameof(context),
						Message = "The 'context' is empty."
					});
			}
		}

		private void ValidateName(string name, string context)
		{
			if (string.IsNullOrWhiteSpace(name))
			{
				ValidationErrors.Add(
					new ValidationError()
					{
						Code = "INVALID_NAME",
						Field = nameof(name),
						Message = "The 'name' is empty."
					});
			}
		}

		private void ValidateFilter(string filter)
		{
			if (string.IsNullOrWhiteSpace(filter))
			{
				ValidationErrors.Add(
					new ValidationError()
					{
						Code = "INVALID_FILTER",
						Field = nameof(filter),
						Message = "The 'filter' is empty."
					});
			}

			try
			{
				using var _ = JsonDocument.Parse(filter);
			}
			catch (JsonException ex)
			{
				ValidationErrors.Add(
					new ValidationError()
					{
						Code = "INVALID_FILTER",
						Field = nameof(filter),
						Message = "The 'filter' is not valid Json."
					});
			}
		}
	}
}
