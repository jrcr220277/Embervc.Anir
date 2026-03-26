namespace Anir.Shared.Contracts.Common;

public class BulkSelectionRequest
{
    /// <summary>
    /// true  => aplicar acción usando filtros (todas las coincidencias)
    /// false => aplicar acción usando Ids seleccionadas
    /// </summary>
    public bool SelectAll { get; set; } = false;

    /// <summary>
    /// Lista de Ids seleccionados manualmente (solo si SelectAll = false)
    /// </summary>
    public List<int>? Ids { get; set; }

    /// <summary>
    /// Filtros usados cuando SelectAll = true.
    /// Debe ser el mismo DTO de filtros de la entidad (ej: CompanyFilterRequest)
    /// </summary>
    public object? Filters { get; set; }

    /// <summary>
    /// Para operaciones que requieren confirmación (ej: eliminar masivo)
    /// </summary>
    public bool Force { get; set; } = false;
}
