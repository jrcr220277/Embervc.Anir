using Anir.Infrastructure.Reports.Template.Pdfs;
using Anir.Shared.Contracts.Common;
using Anir.Shared.Contracts.Companies;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Anir.Infrastructure.Reports.Template.Pdfs;

public class CompanyReportPdf : BasePdfTemplate
{
    private readonly List<CompanyDto> _companies;

    public CompanyReportPdf(List<CompanyDto> companies, ReportConfigDto config)
    {
        _companies = companies ?? throw new ArgumentNullException(nameof(companies));
        Config = config ?? throw new ArgumentNullException(nameof(config));

        Title = "Listado de Empresas";
        Subtitle = $"Total: {_companies.Count} registros";

        IsLandscape = true;
        PageSize = PageSizes.Letter;
    }

    protected override void ComposeContent(IContainer container)
    {
        if (_companies.Count == 0)
        {
            container.PaddingVertical(50).AlignCenter()
                   .Text("No hay datos para mostrar").FontSize(12).FontColor("#888888");
            return;
        }

        container.PaddingVertical(5).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(60);
                columns.ConstantColumn(80);
                columns.RelativeColumn(3);
                columns.RelativeColumn(2);
                columns.RelativeColumn(2);
                columns.RelativeColumn(2);
                columns.RelativeColumn(3);
                columns.ConstantColumn(90);
                columns.RelativeColumn(2.5f);
                columns.ConstantColumn(70);
            });

            table.Header(header =>
            {
                header.Cell().Element(ApplyHeaderStyle).Text("Código");
                header.Cell().Element(ApplyHeaderStyle).Text("Abrev.");
                header.Cell().Element(ApplyHeaderStyle).Text("Nombre");
                header.Cell().Element(ApplyHeaderStyle).Text("Organismo");
                header.Cell().Element(ApplyHeaderStyle).Text("Provincia");
                header.Cell().Element(ApplyHeaderStyle).Text("Municipio");
                header.Cell().Element(ApplyHeaderStyle).Text("Dirección");
                header.Cell().Element(ApplyHeaderStyle).Text("Teléfono");
                header.Cell().Element(ApplyHeaderStyle).Text("Correo");
                header.Cell().Element(c => ApplyHeaderStyle(c).AlignCenter().Text("Estado"));
            });

            for (int i = 0; i < _companies.Count; i++)
            {
                var c = _companies[i];
                var isAlternate = i % 2 == 1;

                table.Cell().Element(cell => ApplyCellStyle(cell, isAlternate).Text(c.Code));
                table.Cell().Element(cell => ApplyCellStyle(cell, isAlternate).Text(c.ShortName));
                table.Cell().Element(cell => ApplyCellStyle(cell, isAlternate).Text(c.Name));
                table.Cell().Element(cell => ApplyCellStyle(cell, isAlternate).Text(c.OrganismName ?? "-"));
                table.Cell().Element(cell => ApplyCellStyle(cell, isAlternate).Text(c.ProvinceName ?? "-"));
                table.Cell().Element(cell => ApplyCellStyle(cell, isAlternate).Text(c.MunicipalityName ?? "-"));
                table.Cell().Element(cell => ApplyCellStyle(cell, isAlternate).Text(c.Address ?? "-"));
                table.Cell().Element(cell => ApplyCellStyle(cell, isAlternate).Text(c.Phone ?? "-"));
                table.Cell().Element(cell => ApplyCellStyle(cell, isAlternate).Text(c.Email ?? "-"));
                table.Cell().Element(cell => ApplyCellStyle(cell, isAlternate).AlignCenter().Text(c.Active ? "Activo" : "Inactivo"));
            }
        });
    }

    // Usamos Border(1) para el recuadro completo y Background para el color
    private static IContainer ApplyHeaderStyle(IContainer container)
    {
        return container
            .Border(1)
            .BorderColor("#FFFFFF")
            .Background("#4A6FA5")
            .PaddingVertical(5)
            .PaddingHorizontal(3)
            .DefaultTextStyle(x => x.Bold().FontSize(8).FontColor("#FFFFFF"));
    }

    private static IContainer ApplyCellStyle(IContainer container, bool isAlternate = false)
    {
        return container
            .Border(1)
            .BorderColor("#CCCCCC")
            .Background(isAlternate ? "#F5F5F5" : "#FFFFFF")
            .PaddingVertical(4)
            .PaddingHorizontal(3)
            .DefaultTextStyle(x => x.FontSize(8));
    }
}