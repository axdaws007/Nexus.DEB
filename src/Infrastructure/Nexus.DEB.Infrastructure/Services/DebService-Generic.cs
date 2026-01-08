using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Nexus.DEB.Domain.Models;
using Task = System.Threading.Tasks.Task;

namespace Nexus.DEB.Infrastructure.Services
{
    public partial class DebService
    {
		#region Workflow
		public async Task<Guid?> GetWorkflowIdAsync(
            Guid moduleId, 
            string entityType, 
            CancellationToken cancellationToken = default)
        {
            var settingName = $"PawsWorkFlowID:{entityType}";

            var value = await _dbContext.ModuleSettings
                .AsNoTracking()
                .Where(x => x.ModuleId == moduleId && x.Name == settingName)
                .Select(x => x.Value)
                .FirstOrDefaultAsync(cancellationToken);

            return Guid.TryParse(value, out var result) ? result : (Guid?)null;
        }

        public Guid? GetWorkflowId(
            Guid moduleId,
            string entityType)
        {
            var settingName = $"PawsWorkFlowID:{entityType}";

            var value = _dbContext.ModuleSettings
                .AsNoTracking()
                .Where(x => x.ModuleId == moduleId && x.Name == settingName)
                .Select(x => x.Value)
                .FirstOrDefault();

            return Guid.TryParse(value, out var result) ? result : (Guid?)null;
        }

        public async Task<List<Guid>> GetDefaultOwnerRoleIdsForEntityTypeAsync(
            Guid moduleId,
            string entityType,
            CancellationToken cancellationToken = default)
        {
            var settingName = $"DefaultOwnerRoleIds:{entityType}";

            var value = await _dbContext.ModuleSettings
                .AsNoTracking()
                .Where(x => x.ModuleId == moduleId && x.Name == settingName)
                .Select(x => x.Value)
                .FirstOrDefaultAsync(cancellationToken);

            if (string.IsNullOrEmpty(value))
                return new List<Guid>();

            return value
                .Split(',')
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(Guid.Parse)
                .ToList();
        }

        public async Task<PawsState?> GetWorkflowStatusByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
            => await _dbContext.PawsStates.AsNoTracking().FirstOrDefaultAsync(x => x.EntityId == id, cancellationToken);

        public async Task<PawsEntityDetail?> GetCurrentWorkflowStatusForEntityAsync(
            Guid entityId,
            CancellationToken cancellationToken)
            => await _dbContext.PawsEntityDetails.AsNoTracking().FirstOrDefaultAsync(x => x.EntityId == entityId, cancellationToken);

        public async Task<IReadOnlyDictionary<Guid, string?>> GetWorkflowPseudoStateTitleForEntitiesAsync(List<Guid> entityIds, CancellationToken cancellationToken = default)
            => await _dbContext.PawsEntityDetails.AsNoTracking().Where(x => entityIds.Contains(x.EntityId)).ToDictionaryAsync(x => x.EntityId, x => x.PseudoStateTitle, cancellationToken);
		#endregion Workflow

		#region Comments
		public async Task<ICollection<CommentDetail>> GetCommentsForEntityAsync(Guid entityId, CancellationToken cancellationToken)
            => await _dbContext.CommentDetails.AsNoTracking()
                        .Where(x => x.EntityId == entityId)
                        .OrderByDescending(x => x.CreatedDate)
                        .ToListAsync(cancellationToken);

        public async Task<int> GetCommentsCountForEntityAsync(Guid entityId, CancellationToken cancellationToken)
            => await _dbContext.Comments.AsNoTracking().Where(x => x.EntityId == entityId).CountAsync(cancellationToken);

        public async Task<Comment?> GetCommentByIdAsync(long id, CancellationToken cancellationToken = default)
            => await _dbContext.Comments.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        public async Task<CommentDetail?> CreateCommentAsync(
            Comment comment,
            CancellationToken cancellationToken)
        {
            await _dbContext.Comments.AddAsync(comment);
            await _dbContext.SaveChangesAsync();

            return await _dbContext.CommentDetails.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == comment.Id, cancellationToken);
        }

        public async Task<bool> DeleteCommentByIdAsync(
            long id, 
            CancellationToken cancellationToken)
            => (await _dbContext.Comments.Where(x => x.Id == id).ExecuteDeleteAsync(cancellationToken) == 1);
		#endregion Comments

		#region ChangeRecords
		public async Task<int> GetChangeRecordsCountForEntityAsync(
            Guid entityId,
            CancellationToken cancellationToken)
            => await _dbContext.ChangeRecords.AsNoTracking()
                .Where(x => !x.IsDeleted && x.EntityId == entityId)
                .CountAsync(cancellationToken);

        public async Task<ICollection<ChangeRecord>> GetChangeRecordsForEntityAsync(
			Guid entityId,
			CancellationToken cancellationToken)
			=> await _dbContext.ChangeRecords.AsNoTracking()
				.Where(x => !x.IsDeleted && x.EntityId == entityId)
				.OrderByDescending(x => x.ChangeDate)
				.ToListAsync(cancellationToken);

		public async Task<ICollection<ChangeRecordItem>> GetChangeRecordItemsForChangeRecordAsync(
			long changeRecordId,
			CancellationToken cancellationToken)
			=> await _dbContext.ChangeRecordItems.AsNoTracking()
				.Where(x => !x.IsDeleted && x.ChangeRecordId == changeRecordId)
				.OrderBy(x => x.FriendlyFieldName)
				.ToListAsync(cancellationToken);

        public async Task AddChangeRecordItem(Guid entityId, string fieldName, string friendlyFieldName, string oldValue, string newValue, CancellationToken cancellationToken)
        {
            var eventId = _dbContext.EventId;
            var userDetails = _dbContext.UserDetails;

            _dbContext.Database.ExecuteSqlRawAsync(
                "EXEC dbo.CreateChangeRecordItem @entityId, @fieldName, @friendlyFieldName, @oldValue, @newValue",
                new SqlParameter("@entityId", entityId),
                new SqlParameter("@fieldName", fieldName),
                new SqlParameter("friendlyFieldName", friendlyFieldName),
                new SqlParameter("@oldValue", oldValue),
                new SqlParameter("@newValue", newValue)
            );
        }
		#endregion ChangeRecords

		#region SavedSearch
		public async Task<ICollection<SavedSearch>> GetSavedSearchesByContextAsync(string context, CancellationToken cancellationToken)
        {
            var currentPostId = _currentUserService.PostId;
			return await _dbContext.SavedSearches.AsNoTracking()
				.Where(x => x.PostId == currentPostId && x.Context == context)
				.OrderBy(x => x.Name)
				.ToListAsync(cancellationToken);
		}

		public async Task<ICollection<SavedSearch>> GetSavedSearchesForCurrentPostAsync(CancellationToken cancellationToken)
		{
			var currentPostId = _currentUserService.PostId;
			return await _dbContext.SavedSearches.AsNoTracking()
				.Where(x => x.PostId == currentPostId)
				.OrderBy(x => x.Name)
				.ToListAsync(cancellationToken);
		}

		public async Task<SavedSearch?> GetSavedSearchAsync(string context, string name, CancellationToken cancellationToken)
        {
			var currentPostId = _currentUserService.PostId;
			return await _dbContext.SavedSearches.AsNoTracking()
				.Where(x => x.PostId == currentPostId && x.Context == context && x.Name == name)
				.FirstOrDefaultAsync(cancellationToken);
		}

        public async Task<SavedSearch> SaveSavedSearchAsync(SavedSearch savedSearch, bool isNew, CancellationToken cancellationToken)
        {
            if (isNew)
            {
                await _dbContext.SavedSearches.AddAsync(savedSearch);
            }
            else
            {
                _dbContext.SavedSearches.Update(savedSearch);
            }
			await _dbContext.SaveChangesAsync(cancellationToken);

            return savedSearch;
		}
		#endregion SavedSearch

		#region SerialNumber
		public async Task<string> GenerateSerialNumberAsync(
            Guid moduleId,
            Guid instanceId,
            string entityType,
            Dictionary<string, object>? tokenValues = null,
            CancellationToken cancellationToken = default)
        {
            var results = await GenerateSerialNumbersAsync(
                moduleId,
                instanceId,
                entityType,
                1,
                _ => tokenValues,
                cancellationToken);

            return results[0];
        }

        public async Task<List<string>> GenerateSerialNumbersAsync(
            Guid moduleId,
            Guid instanceId,
            string entityType,
            int numberToGenerate,
            Func<int, Dictionary<string, object>?>? tokenValuesFactory = null,
            CancellationToken cancellationToken = default)
        {
            var config = await _dbContext.SerialNumbers
                .Where(x => x.ModuleId == moduleId &&
                            x.InstanceId == instanceId &&
                            x.EntityType == entityType)
                .FirstOrDefaultAsync(cancellationToken);

            if (config == null)
            {
                throw new Exception("damn it");
            }

            var serialNumbers = new List<string>();
            var startCounter = config.NextValue;

            for (int i = 0; i < numberToGenerate; i++)
            {
                var currentCounter = startCounter + i;

                var tokenValues = tokenValuesFactory?.Invoke(i) ?? new Dictionary<string, object>();

                tokenValues["counter"] = currentCounter;

                var serialNumber = FormatSerialNumber(config.Format, tokenValues);
                serialNumbers.Add(serialNumber);
            }

            config.NextValue = startCounter + numberToGenerate;
            await _dbContext.SaveChangesAsync(cancellationToken);

            return serialNumbers;
        }

        private string FormatSerialNumber(string template, Dictionary<string, object> tokenValues)
        {
            var result = template;

            // Process all tokens in the format {tokenName} or {tokenName:format}
            var matches = System.Text.RegularExpressions.Regex.Matches(
                template,
                @"\{([^}:]+)(?::([^}]+))?\}");

            foreach (System.Text.RegularExpressions.Match match in matches)
            {
                var tokenName = match.Groups[1].Value;
                var format = match.Groups[2].Value;

                // Support nested property access with dot notation (e.g., parent.serialNumber)
                var value = GetTokenValue(tokenValues, tokenName);

                if (value == null)
                {
                    throw new Exception(
                        $"Token '{tokenName}' not found in provided values. Available tokens: {string.Join(", ", tokenValues.Keys)}");
                }

                string formattedValue;
                if (!string.IsNullOrEmpty(format) && value is IFormattable formattable)
                {
                    formattedValue = formattable.ToString(format, System.Globalization.CultureInfo.InvariantCulture);
                }
                else
                {
                    formattedValue = value.ToString() ?? string.Empty;
                }

                result = result.Replace(match.Value, formattedValue);
            }

            return result;
        }

        private object? GetTokenValue(Dictionary<string, object> tokenValues, string tokenName)
        {
            // Support nested property access (e.g., "parent.serialNumber")
            var parts = tokenName.Split('.');

            if (parts.Length == 1)
            {
                return tokenValues.TryGetValue(tokenName, out var value) ? value : null;
            }

            // Navigate through nested properties
            object? current = tokenValues.TryGetValue(parts[0], out var rootValue) ? rootValue : null;

            for (int i = 1; i < parts.Length && current != null; i++)
            {
                var property = current.GetType().GetProperty(parts[i]);
                if (property == null)
                {
                    return null;
                }
                current = property.GetValue(current);
            }

            return current;
        }
		#endregion SerialNumber
	}
}
