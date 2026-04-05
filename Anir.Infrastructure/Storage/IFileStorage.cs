namespace Anir.Infrastructure.Storage;

public interface IFileStorage
{
    Task<string> SaveAsync(byte[] content, string extension, string folder);
    Task<bool> DeleteAsync(string fileId, string folder);
        
}
