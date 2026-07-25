namespace RetroPad.Application.Services;

using RetroPad.Core.Entities;
using RetroPad.Core.Ports;

public class ConfigService
{
    private readonly IConfigStore _configStore;

    public ConfigService(IConfigStore configStore)
    {
        _configStore = configStore;
    }

    public async Task<AppConfig> LoadAsync(CancellationToken ct = default)
    {
        return await _configStore.LoadAsync(ct);
    }

    public async Task SaveAsync(AppConfig config, CancellationToken ct = default)
    {
        await _configStore.SaveAsync(config, ct);
    }
}
