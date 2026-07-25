namespace RetroPad.Core.Ports;

using RetroPad.Core.Entities;

public interface ISessionStore
{
    Task SaveAsync(SessionState session, CancellationToken ct = default);
    Task<SessionState?> LoadAsync(CancellationToken ct = default);
    void Clear();
}
