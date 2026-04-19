namespace Anir.Infrastructure.Storage
{
    public interface IFileStorage
    {
        Task<string> SaveAsync(Stream stream, string extension, string folder);
        Task<bool> DeleteAsync(string fileName, string folder);
        Task<(Stream Stream, string ContentType, string FileName)?> GetAsync(string fileName, string folder);
    }
}
