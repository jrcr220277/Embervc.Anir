using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

public abstract class BasePdfTemplate : IDocument
{
    public string Title { get; set; } = "Reporte del Sistema";

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            // Configuración de página
            page.Size(PageSizes.Letter);   // o PageSizes.A4 si prefieres
            page.Margin(40);               // márgenes uniformes

            // Encabezado
            page.Header().Row(row =>
            {
                row.RelativeItem().Text(Title).Bold().FontSize(16);
                // row.Constant(50).Image("logo.png"); // opcional
            });

            // Contenido dinámico (cada reporte lo define)
            page.Content().Element(ComposeContent);

            // Pie de página
            page.Footer().Row(row =>
            {
                row.RelativeItem().Text(DateTime.Now.ToString("dd/MM/yyyy"));
                row.ConstantItem(50).AlignRight().Text(text =>
                {
                    text.CurrentPageNumber();
                    text.Span(" / ");
                    text.TotalPages();
                });
            });
        });
    }

    // Método abstracto para que cada reporte defina su contenido
    protected abstract void ComposeContent(IContainer container);
}
