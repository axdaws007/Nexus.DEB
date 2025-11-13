using Microsoft.EntityFrameworkCore;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Application.Common.Models;
using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Infrastructure.Services
{
    public partial class DebService
    {
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

        public async Task<PawsState?> GetWorkflowStatusByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
            => await _dbContext.PawsStates.AsNoTracking().FirstOrDefaultAsync(x => x.EntityId == id, cancellationToken);

        public async Task<ICollection<CommentDetail>> GetCommentsForEntityAsync(Guid entityId, CancellationToken cancellationToken)
            => await _dbContext.CommentDetails.AsNoTracking()
                        .Where(x => x.EntityId == entityId)
                        .OrderByDescending(x => x.CreatedDate)
                        .ToListAsync(cancellationToken);

        public Task<int> GetCommentsCountForEntityAsync(Guid entityId, CancellationToken cancellationToken)
            => _dbContext.Comments.AsNoTracking().Where(x => x.EntityId == entityId).CountAsync(cancellationToken);

        public async Task<CommentDetail?> CreateCommentAsync(
            Comment comment,
            CancellationToken cancellationToken)
        {
            await _dbContext.Comments.AddAsync(comment);
            await _dbContext.SaveChangesAsync();

            return await _dbContext.CommentDetails.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == comment.Id);
        }
    }
}
