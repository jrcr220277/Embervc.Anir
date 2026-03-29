using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

public class PdfService : IPdfService
{
    public async Task<byte[]> GenerateAsync(IDocument document, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() => document.GeneratePdf(), cancellationToken);
    }
}



