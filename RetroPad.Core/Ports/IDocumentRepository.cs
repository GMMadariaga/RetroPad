namespace RetroPad.Core.Ports;

using RetroPad.Core.Entities;

public interface IDocumentRepository
{
    Task<string> ReadAsync(string filePath, CancellationToken ct = default);
    Task WriteAsync(string filePath, string content, CancellationToken ct = default);
    bool Exists(string filePath);
}
