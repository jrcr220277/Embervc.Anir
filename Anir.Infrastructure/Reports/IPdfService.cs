using QuestPDF.Infrastructure;

public interface IPdfService
{
    Task<byte[]> GenerateAsync(IDocument document, CancellationToken cancellationToken = default);
}

