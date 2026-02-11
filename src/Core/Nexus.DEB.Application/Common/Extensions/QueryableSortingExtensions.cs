using Nexus.DEB.Application.Common.Models.Sorting;
using System.Linq.Expressions;
using System.Reflection;

namespace Nexus.DEB.Application.Common.Extensions
{
    public static class QueryableSortingExtensions
    {
        /// <summary>
        /// Applies dynamic sorting to an IQueryable based on a collection of SortByItem.
        /// Only sorts by property names that exist on <typeparamref name="T"/> (case-insensitive).
        /// Unrecognised column names are silently ignored.
        /// </summary>
        public static IQueryable<T> ApplySorting<T>(
            this IQueryable<T> query,
            ICollection<SortByItem>? sortBy,
            Dictionary<string, string>? columnMap = null)
        {
            if (sortBy is null || sortBy.Count == 0)
                return query;

            var entityType = typeof(T);
            bool isFirst = true;

            foreach (var sort in sortBy)
            {
                // Resolve the actual property name — check the alias map first,
                // then fall back to a direct case-insensitive match on the entity.
                string propertyName;

                if (columnMap != null &&
                    columnMap.TryGetValue(sort.ColumnName, out var mapped))
                {
                    propertyName = mapped;
                }
                else
                {
                    var prop = entityType.GetProperty(
                        sort.ColumnName,
                        BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                    if (prop is null)
                        continue; // unknown column — skip it safely

                    propertyName = prop.Name;
                }

                // Build the expression: x => x.PropertyName
                var parameter = Expression.Parameter(entityType, "x");
                var property = Expression.Property(parameter, propertyName);
                var lambda = Expression.Lambda(property, parameter);

                // Choose OrderBy vs ThenBy, ascending vs descending
                var methodName = isFirst
                    ? (sort.IsAscending ? "OrderBy" : "OrderByDescending")
                    : (sort.IsAscending ? "ThenBy" : "ThenByDescending");

                var method = typeof(Queryable).GetMethods()
                    .First(m => m.Name == methodName && m.GetParameters().Length == 2)
                    .MakeGenericMethod(entityType, property.Type);

                query = (IQueryable<T>)method.Invoke(null, [query, lambda])!;
                isFirst = false;
            }

            return query;
        }
    }
}
