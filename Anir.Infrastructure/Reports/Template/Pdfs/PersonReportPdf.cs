using Anir.Shared.Contracts.Companies;
using Anir.Shared.Contracts.Persons;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

public class PersonReportPdf : BasePdfTemplate
{
    private readonly List<PersonDto> _persons;

    public PersonReportPdf(List<PersonDto> persons)
    {
        _persons = persons;
        Title = "Listado de Persona"; // aquí personalizas el título
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
                header.Cell().Text("DNI");
                header.Cell().Text("Nombre");
                header.Cell().Text("CELULAR");
                header.Cell().Text("EMAIL");
                header.Cell().Text("Activo");
            });

            foreach (var entity in _persons)
            {
                table.Cell().Text(entity.Dni);
                table.Cell().Text(entity.FullName);
                table.Cell().Text(entity.CellPhone ?? "");
                table.Cell().Text(entity.Email ?? "");
                table.Cell().Text(entity.Active ? "Sí" : "No");
            }
        });
    }
}
