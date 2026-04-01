using System.Linq.Expressions;
using Anir.Shared.Contracts.Common;
using Microsoft.EntityFrameworkCore;

namespace Anir.Infrastructure.Extensions;

/// <summary>
/// Extensiones para ordenamiento dinámico y paginación.
/// Funcionan con cualquier entidad y soportan navegación profunda.
/// </summary>
public static class QueryablePagingExtensions
{
    /// <summary>
    /// Ordenamiento dinámico basado en qp.Sort y qp.Desc.
    /// Soporta:
    /// - Campos simples: "Name"
    /// - Navegación de 2 niveles: "Municipality.Name"
    /// - Navegación de 3 niveles: "Municipality.Province.Name"
    /// - Cualquier profundidad
    ///
    /// NO usa EF.Property → evita errores por null.
    /// EF Core traduce la expresión a SQL seguro.
    /// </summary>
    public static IQueryable<T> ApplySorting<T>(this IQueryable<T> query, BaseQuery qp)
    {
        // Si no se especifica Sort → orden por Id
        if (string.IsNullOrWhiteSpace(qp.Sort))
            return query.OrderBy(e => EF.Property<object>(e, "Id"));

        // Dividimos Sort por puntos para detectar navegación
        var parts = qp.Sort.Split('.');

        // Creamos el parámetro: x =>
        var parameter = Expression.Parameter(typeof(T), "x");

        // Construimos la expresión dinámica: x.Prop1.Prop2.Prop3...
        Expression property = parameter;

        foreach (var part in parts)
        {
            property = Expression.PropertyOrField(property, part);
        }

        // Convertimos a object para OrderBy
        var converted = Expression.Convert(property, typeof(object));

        // Lambda final: x => (object)x.Prop1.Prop2.Prop3
        var lambda = Expression.Lambda<Func<T, object>>(converted, parameter);

        // Aplicamos orden asc/desc
        return qp.Desc
            ? query.OrderByDescending(lambda)
            : query.OrderBy(lambda);
    }

    /// <summary>
    /// Paginación estándar usando Skip y Take.
    /// Devuelve un PagedResponse<T>.
    /// </summary>
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
            TotalCount = total,
            Page = qp.Page,
            Size = qp.Size
        };
    }
}
