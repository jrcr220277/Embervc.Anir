using Anir.Shared.Contracts.Common;

namespace Anir.Shared.Contracts.Persons;

public class PersonQueryDto : BaseQuery
{
    /// <summary>
    /// Búsqueda general: Dni, nombre, correo, teléfono.
    /// </summary>
    public string? Search { get; set; }

    /// <summary>
    /// Filtrar por estado activo/inactivo (si aplica en tu lógica).
    /// </summary>
    public bool? ActiveFilter { get; set; }

    /// <summary>
    /// Filtrar por trabajos ANIR asociados.
    /// </summary>
    public int? AnirWorkId { get; set; }
}
