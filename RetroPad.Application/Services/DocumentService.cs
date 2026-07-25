namespace RetroPad.Application.Services;

using RetroPad.Core.Entities;
using RetroPad.Core.Ports;

public class DocumentService
{
    private readonly IDocumentRepository _repository;

    public DocumentService(IDocumentRepository repository)
    {
        _repository = repository;
    }

    public async Task<string> OpenAsync(string filePath, CancellationToken ct = default)
    {
        return await _repository.ReadAsync(filePath, ct);
    }

    public async Task SaveAsync(string filePath, string content, CancellationToken ct = default)
    {
        await _repository.WriteAsync(filePath, content, ct);
    }

    public bool FileExists(string filePath)
    {
        return _repository.Exists(filePath);
    }
}
