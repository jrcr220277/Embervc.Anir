using Anir.Shared.Contracts.Common;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Anir.Infrastructure.Reports.Template.Pdfs;

public abstract class BasePdfTemplate : IDocument
{
    public string Title { get; set; } = "Reporte";
    public string Subtitle { get; set; } = "";
    public ReportConfigDto Config { get; set; } = new();

    public bool IsLandscape { get; set; } = false;
    public PageSize PageSize { get; set; } = PageSizes.Letter;

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(IsLandscape ? PageSize.Landscape() : PageSize);
            page.Margin(40);

            page.Header().Element(ComposeHeader);
            page.Content().Element(ComposeContent);
            page.Footer().Element(ComposeFooter);
        });
    }

    private void ComposeHeader(IContainer container)
    {
        container.Column(col =>
        {
            col.Item().Row(row =>
            {
                row.ConstantItem(60).PaddingRight(10).Element(c =>
                {
                    if (Config.LogoBytes != null && Config.LogoBytes.Length > 0)
                    {
                        c.Image(Config.LogoBytes).FitArea();
                    }
                    else
                    {
                        var initials = GetInitials(Config.CompanyName);
                        c.Background("#E0E0E0")
                         .Padding(5)
                         .AlignCenter()
                         .AlignMiddle()
                         .Text(initials)
                         .Bold()
                         .FontSize(20)
                         .FontColor("#666666");
                    }
                });

                row.RelativeItem().Column(innerCol =>
                {
                    innerCol.Item().Text(Config.CompanyName).Bold().FontSize(14).FontColor(GetPrimaryColor());
                    if (!string.IsNullOrEmpty(Config.HeaderText))
                        innerCol.Item().Text(Config.HeaderText).FontSize(8).FontColor("#888888");
                    innerCol.Item().Text($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(7).FontColor("#888888");
                });
            });

            col.Item().PaddingVertical(5).LineHorizontal(1).LineColor(GetPrimaryColor());

            col.Item().PaddingTop(5).Row(row =>
            {
                row.RelativeItem().Text(Title).Bold().FontSize(12);
                if (!string.IsNullOrEmpty(Subtitle))
                    row.RelativeItem().AlignRight().Text(Subtitle).FontSize(8).FontColor("#888888");
            });

            col.Item().PaddingBottom(10);
        });
    }

    private void ComposeFooter(IContainer container)
    {
        container.Column(col =>
        {
            col.Item().PaddingTop(10).LineHorizontal(1).LineColor(GetPrimaryColor());

            col.Item().PaddingTop(5).Row(row =>
            {
                row.RelativeItem().Column(innerCol =>
                {
                    if (!string.IsNullOrEmpty(Config.FooterText))
                        innerCol.Item().Text(Config.FooterText).FontSize(7).FontColor("#888888");
                    innerCol.Item().Text($"{Config.CompanyName} - Sistema Interno").FontSize(7).FontColor("#888888");
                });

                row.ConstantItem(100).AlignRight().Text(text =>
                {
                    text.DefaultTextStyle(x => x.FontSize(8).FontColor("#888888"));
                    text.CurrentPageNumber();
                    text.Span(" / ");
                    text.TotalPages();
                });
            });
        });
    }

    protected abstract void ComposeContent(IContainer container);

    private static string GetInitials(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return "??";

        var words = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (words.Length >= 2)
            return $"{words[0][0]}{words[1][0]}".ToUpper();

        return name.Length >= 2
            ? name.Substring(0, 2).ToUpper()
            : name.ToUpper();
    }

    private string GetPrimaryColor() => Config.PrimaryColor ?? "#4A6FA5";
}