using Anir.Shared.Contracts.AnirWorks;
using Anir.Shared.Contracts.Companies;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

public class AnirWorkListPdf : BasePdfTemplate
{
    private readonly List<AnirWorkDto> _anirs;

    public AnirWorkListPdf(List<AnirWorkDto> anirs)
    {
        _anirs = anirs;
        Title = "Listado de Empresas"; // aquí personalizas el título
    }

    protected override void ComposeContent(IContainer container)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(50); // Id
            
            });

            table.Header(header =>
            {
                header.Cell().Text("ID");
              
            });

            foreach (var c in _anirs)
            {
                table.Cell().Text(c.Id.ToString());
               
            }
        });
    }
}
