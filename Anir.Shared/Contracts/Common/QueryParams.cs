namespace Anir.Shared.Contracts.Common;

public class QueryParams
{
    public int Page { get; set; } = 1;
    public int Size { get; set; } = 20;

    // Ordenamiento dinámico
    public string? Sort { get; set; }
    public bool Desc { get; set; }

    // Cálculo interno
    public int Skip => (Page - 1) * Size;
}
