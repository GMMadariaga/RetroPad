namespace RetroPad.Infrastructure.Persistence;

using Newtonsoft.Json;
using RetroPad.Core.Entities;
using RetroPad.Core.Ports;

public class FileSessionStore : ISessionStore
{
    private static readonly string SessionDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "RetroPad", "Session");

    private static readonly string SessionFile = Path.Combine(SessionDir, "session.json");

    public async Task SaveAsync(SessionState session, CancellationToken ct = default)
    {
        if (!Directory.Exists(SessionDir))
            Directory.CreateDirectory(SessionDir);

        session.SavedAt = DateTime.UtcNow;
        var json = JsonConvert.SerializeObject(session, Formatting.Indented);
        await File.WriteAllTextAsync(SessionFile, json, ct);
    }

    public async Task<SessionState?> LoadAsync(CancellationToken ct = default)
    {
        if (!File.Exists(SessionFile))
            return null;

        var json = await File.ReadAllTextAsync(SessionFile, ct);
        return JsonConvert.DeserializeObject<SessionState>(json);
    }

    public void Clear()
    {
        if (Directory.Exists(SessionDir))
            Directory.Delete(SessionDir, true);
    }
}
