using Anir.Shared.Contracts.Companies;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

public class CompanyReportPdf : BasePdfTemplate
{
    private readonly List<CompanyDto> _companies;

    public CompanyReportPdf(List<CompanyDto> companies)
    {
        _companies = companies;
        Title = "Listado de Empresas"; // aquí personalizas el título
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

            foreach (var c in _companies)
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
