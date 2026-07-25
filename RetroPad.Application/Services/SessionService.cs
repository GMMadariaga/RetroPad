namespace RetroPad.Application.Services;

using RetroPad.Core.Entities;
using RetroPad.Core.Ports;

public class SessionService
{
    private readonly ISessionStore _sessionStore;
    private readonly IDocumentRepository _documentRepository;

    public SessionService(ISessionStore sessionStore, IDocumentRepository documentRepository)
    {
        _sessionStore = sessionStore;
        _documentRepository = documentRepository;
    }

    public async Task SaveSessionAsync(IEnumerable<Document> documents, int activeTabIndex, CancellationToken ct = default)
    {
        var tabs = documents.Select((doc, index) => new TabState
        {
            DocumentId = doc.Id,
            FileName = doc.FileName,
            FilePath = doc.FilePath,
            Language = doc.Language,
            CursorOffset = doc.CursorOffset,
            ScrollOffset = doc.ScrollOffset,
            IsActive = index == activeTabIndex
        }).ToList();

        var session = new SessionState
        {
            Tabs = tabs,
            ActiveTabIndex = activeTabIndex
        };

        await _sessionStore.SaveAsync(session, ct);

        foreach (var doc in documents.Where(d => string.IsNullOrEmpty(d.FilePath)))
        {
            var tempPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "RetroPad", "Session", $"{doc.Id}.tmp");
            await _documentRepository.WriteAsync(tempPath, doc.Content, ct);
        }
    }

    public async Task<SessionState?> LoadSessionAsync(CancellationToken ct = default)
    {
        return await _sessionStore.LoadAsync(ct);
    }

    public async Task<string?> LoadTempContentAsync(string documentId, CancellationToken ct = default)
    {
        var tempPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "RetroPad", "Session", $"{documentId}.tmp");

        if (_documentRepository.Exists(tempPath))
            return await _documentRepository.ReadAsync(tempPath, ct);

        return null;
    }

    public void ClearSession()
    {
        _sessionStore.Clear();
    }
}
