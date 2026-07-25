namespace RetroPad.Infrastructure.Persistence;

using RetroPad.Core.Entities;
using RetroPad.Core.Ports;

public class FileDocumentRepository : IDocumentRepository
{
    public async Task<string> ReadAsync(string filePath, CancellationToken ct = default)
    {
        using var reader = new StreamReader(filePath);
        return await reader.ReadToEndAsync(ct);
    }

    public async Task WriteAsync(string filePath, string content, CancellationToken ct = default)
    {
        var dir = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        using var writer = new StreamWriter(filePath, false);
        await writer.WriteAsync(content.AsMemory(), ct);
    }

    public bool Exists(string filePath)
    {
        return File.Exists(filePath);
    }
}
