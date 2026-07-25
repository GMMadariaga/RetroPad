namespace RetroPad.Infrastructure.Config;

using Newtonsoft.Json;
using RetroPad.Core.Entities;
using RetroPad.Core.Ports;

public class JsonConfigStore : IConfigStore
{
    private static readonly string ConfigDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "RetroPad");

    private static readonly string ConfigFile = Path.Combine(ConfigDir, "config.json");

    public async Task<AppConfig> LoadAsync(CancellationToken ct = default)
    {
        if (!File.Exists(ConfigFile))
            return new AppConfig();

        var json = await File.ReadAllTextAsync(ConfigFile, ct);
        return JsonConvert.DeserializeObject<AppConfig>(json) ?? new AppConfig();
    }

    public async Task SaveAsync(AppConfig config, CancellationToken ct = default)
    {
        if (!Directory.Exists(ConfigDir))
            Directory.CreateDirectory(ConfigDir);

        var json = JsonConvert.SerializeObject(config, Formatting.Indented);
        await File.WriteAllTextAsync(ConfigFile, json, ct);
    }
}
