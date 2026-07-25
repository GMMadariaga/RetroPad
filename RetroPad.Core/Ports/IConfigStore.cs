namespace RetroPad.Core.Ports;

using RetroPad.Core.Entities;

public interface IConfigStore
{
    Task<AppConfig> LoadAsync(CancellationToken ct = default);
    Task SaveAsync(AppConfig config, CancellationToken ct = default);
}
