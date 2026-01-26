using Microsoft.EntityFrameworkCore;
using Nexus.DEB.Application.Common.Models.Filters;
using Nexus.DEB.Domain.Models;

namespace Nexus.DEB.Infrastructure.Services
{
    public partial class DebService
    {
        public IQueryable<CommentType> GetCommentTypes()
        {
            var query = from s in _dbContext.CommentTypes
                        where s.IsEnabled == true
                        orderby s.Ordinal
                        select s;

            return query;
        }

        public IQueryable<Standard> GetStandards()
        {
            var query = from s in _dbContext.Standards
                        where s.IsEnabled == true
                        orderby s.Ordinal
                        select s;

            return query;
        }

        public async Task<ICollection<FilterItem>> GetStandardsLookupAsync(CancellationToken cancellationToken)
        {
            return await (from s in _dbContext.Standards.AsNoTracking()
                          orderby s.Ordinal
                          select new FilterItem()
                          {
                              Id = s.Id,
                              Value = s.Title,
                              IsEnabled = s.IsEnabled
                          }).ToListAsync(cancellationToken);
        }


        public IQueryable<TaskType> GetTaskTypes()
        {
            var query = from s in _dbContext.TaskTypes
                        where s.IsEnabled == true
                        orderby s.Ordinal
                        select s;

            return query;
        }

        public async Task<ICollection<FilterItem>> GetTaskTypesLookupAsync(CancellationToken cancellationToken)
        {
            return await (from s in _dbContext.TaskTypes.AsNoTracking()
                          orderby s.Ordinal
                          select new FilterItem()
                          {
                              Id = s.Id,
                              Value = s.Title,
                              IsEnabled = s.IsEnabled
                          }).ToListAsync(cancellationToken);
        }

        public IQueryable<UserAndPost> GetPostsWithUsers(string? searchText, ICollection<Guid> postIds, bool includeDeletedUsers = false, bool includeDeletedPosts = false)
        {
            var query = from up in _dbContext.UsersAndPosts.AsNoTracking()
                        join gu in _dbContext.GroupUsers.AsNoTracking() on up.UserId equals gu.EntityId
                        select new UserAndPost
                        {
                            UserId = up.UserId,
                            UserName = up.UserName,
                            IsUserDeleted = up.IsUserDeleted,
                            IsPostDeleted = up.IsPostDeleted,
                            PostId = up.PostId,
                            PostTitle = up.PostTitle,
                            FirstName = gu.UserFirstName,
                            LastName = gu.UserLastName
                        };

            if (!includeDeletedPosts)
                query = query.Where(x => x.IsPostDeleted == false);

            if (!includeDeletedUsers)
                query = query.Where(x => x.IsUserDeleted == false);

            if (!string.IsNullOrEmpty(searchText))
            {
                var searchTextLower = searchText.ToLower();
                query = query.Where(x => 
                    x.PostTitle.ToLower().Contains(searchTextLower) || 
                    x.UserName.ToLower().Contains(searchTextLower) ||
                    (x.FirstName != null && x.FirstName.ToLower().Contains(searchTextLower)) ||
                    (x.LastName != null && x.LastName.ToLower().Contains(searchTextLower)));
            }

            if (postIds.Count > 0)
            {
                query = query.Where(x => postIds.Contains(x.PostId));
            }

            return query;
        }
    }
}
