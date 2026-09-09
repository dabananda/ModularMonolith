using Microsoft.EntityFrameworkCore;

namespace ModularMonolith.Shared.Common
{
    public static class QueryableExtensions
    {
        public const int MaxPageSize = 100;
        public const int DefaultPageSize = 20;

        public static async Task<PagedResult<T>> ToPagedResultAsync<T>(this IQueryable<T> query,int pageNumber,int pageSize,CancellationToken cancellationToken = default)
        {
            pageNumber = pageNumber < 1 ? 1 : pageNumber;
            pageSize = pageSize < 1 ? DefaultPageSize : Math.Min(pageSize, MaxPageSize);

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return PagedResult<T>.Create(items, pageNumber, pageSize, totalCount);
        }
    }
}
