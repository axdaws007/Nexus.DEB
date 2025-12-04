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
			IPawsService pawsService) : base(cisService, cbacService, applicationSettingsService, currentUserService, dateTimeProvider, debService, pawsService, string.Empty)
		{
		}

		public async Task<Result<SavedSearch>> CreateSavedSearchAsync(string context, string name, string filter, CancellationToken cancellationToken)
		{
			await ValidateFieldsAsync(context, name, filter);

			if (ValidationErrors.Count > 0)
			{
				return Result<SavedSearch>.Failure(ValidationErrors);
			}

			try
			{
				var savedSearch = new SavedSearch()
				{
					PostId = CurrentUserService.PostId,
					Context = context,
					Name = name,
					Filter = filter,
					CreatedDate = DateTimeProvider.Now
				};

				var newSavedSearch = await DebService.CreateSavedSearchAsync(savedSearch, cancellationToken);

				if (newSavedSearch == null)
				{
					return Result<SavedSearch>.Failure("Saved Search was not created.");
				}

				return Result<SavedSearch>.Success(newSavedSearch);
			}
			catch (Exception ex)
			{
				return Result<SavedSearch>.Failure($"An error occurred creating the Saved Search: {ex.Message}");
			}
		}

		private async Task ValidateFieldsAsync(string context, string name, string filter)
		{
			ValidateContext(context);
			
			await ValidateName(name, context);

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

		private async Task ValidateName(string name, string context, CancellationToken cancellationToken = default)
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

			var savedSearches = await DebService.GetSavedSearchesByContextAsync(context, cancellationToken);
			if (savedSearches.Any(a => a.Name == name))
			{
				ValidationErrors.Add(
					new ValidationError()
					{
						Code = "INVALID_NAME",
						Field = nameof(name),
						Message = "The 'name' already exists for this 'context'."
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
