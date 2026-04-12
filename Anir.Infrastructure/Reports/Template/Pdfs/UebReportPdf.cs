using Anir.Data.Entities;
using Anir.Shared.Contracts.Uebs;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

public class UebReportPdf : BasePdfTemplate
{
    private readonly List<UebDto> _uebs;

    public UebReportPdf(List<UebDto> uebs)
    {
        _uebs = uebs;
        Title = "Listado de Uebs"; // aquí personalizas el título
    }

    protected override void ComposeContent(IContainer container)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(50); // Id
                columns.RelativeColumn();   // Nombre
                columns.RelativeColumn();   // Provincia
                columns.RelativeColumn();   // Municipio
                columns.ConstantColumn(60); // Activo
            });

            table.Header(header =>
            {
                header.Cell().Text("ID");
                header.Cell().Text("Nombre");
                header.Cell().Text("Provincia");
                header.Cell().Text("Municipio");
                header.Cell().Text("Activo");
            });

            foreach (var c in _uebs)
            {
                table.Cell().Text(c.Id.ToString());
                table.Cell().Text(c.Name);
                table.Cell().Text(c.ProvinceName ?? "");
                table.Cell().Text(c.MunicipalityName ?? "");
                table.Cell().Text(c.Active ? "Sí" : "No");
            }
        });
    }
}
