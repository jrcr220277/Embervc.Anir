using Anir.Shared.Contracts.Common;
using Microsoft.EntityFrameworkCore;

namespace Anir.Infrastructure.Extensions;

public static class QueryablePagingExtensions
{
    public static IQueryable<T> ApplySorting<T>(this IQueryable<T> query, BaseQuery qp)
    {
        if (string.IsNullOrWhiteSpace(qp.Sort))
            return query.OrderBy(e => EF.Property<object>(e, "Id")); // default seguro

        var parts = qp.Sort.Split('.');

        if (parts.Length == 1)
        {
            return qp.Desc
                ? query.OrderByDescending(e => EF.Property<object>(e, parts[0]))
                : query.OrderBy(e => EF.Property<object>(e, parts[0]));
        }

        // Soporte para navegación: Municipality.Name
        if (parts.Length == 2)
        {
            return qp.Desc
                ? query.OrderByDescending(e =>
                    EF.Property<object>(
                        EF.Property<object>(e, parts[0]),
                        parts[1]))
                : query.OrderBy(e =>
                    EF.Property<object>(
                        EF.Property<object>(e, parts[0]),
                        parts[1]));
        }

        return query; // fallback
    }

    public static async Task<PagedResponse<T>> ToPagedResultAsync<T>(
        this IQueryable<T> query,
        BaseQuery qp,
        CancellationToken ct = default)
    {
        var total = await query.CountAsync(ct);
        var items = await query
            .Skip(qp.Skip)
            .Take(qp.Size)
            .ToListAsync(ct);

        return new PagedResponse<T>
        {
            Items = items,
            TotalCount = total,  // ya cambiaste Total → TotalCount
            Page = qp.Page,
            Size = qp.Size
        };
    }
}
