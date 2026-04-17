using Anir.Infrastructure.Storage;

public class FileStorageService : IFileStorage
{
    private readonly string _root;

    public FileStorageService(string root)
    {
        _root = root;
    }

    public async Task<string> SaveAsync(byte[] content, string extension, string folder)
    {
        string folderPath = Path.Combine(_root, folder);

        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        string newName = $"{Guid.NewGuid()}{extension}";
        string path = Path.Combine(folderPath, newName);

        await File.WriteAllBytesAsync(path, content);

        // ⭐ DEVUELVE SOLO EL NOMBRE DEL ARCHIVO
        return newName;
    }

    public Task<bool> DeleteAsync(string fileId, string folder)
    {
        string fullPath = Path.Combine(_root, folder, fileId);

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
            return Task.FromResult(true);
        }

        return Task.FromResult(false);
    }
}
