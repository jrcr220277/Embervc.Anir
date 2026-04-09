using Anir.Shared.Contracts.Organisms;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

public class OrganismReportPdf : BasePdfTemplate
{
    private readonly List<OrganismDto> _organisms;

    public OrganismReportPdf(List<OrganismDto> organisms)
    {
        _organisms = organisms;
        Title = "Listado de Organismos"; // aquí personalizas el título
    }

    protected override void ComposeContent(IContainer container)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
              
                columns.RelativeColumn(50);   // Code
                columns.RelativeColumn();   // ShortName
                columns.RelativeColumn();   // Name
                
            });

            table.Header(header =>
            {
               
                header.Cell().Text("Codigo");
                header.Cell().Text("Abreviatura");
                header.Cell().Text("Nombre");
               
            });

            foreach (var c in _organisms)
            {
                table.Cell().Text(c.Code);
                table.Cell().Text(c.ShortName ?? "");
                table.Cell().Text(c.Name ?? "");
              
            }
        });
    }
}
