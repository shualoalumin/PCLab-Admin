using System.IO;
using System.Text.Json;
using PCLab.Client.Models;

namespace PCLab.Client.Services;

public static class AppConfigurationLoader
{
    public static ClientConfiguration Load()
    {
        string configPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");

        if (!File.Exists(configPath))
        {
            return new ClientConfiguration();
        }

        string json = File.ReadAllText(configPath);

        return JsonSerializer.Deserialize<ClientConfiguration>(
                   json,
                   new JsonSerializerOptions
                   {
                       PropertyNameCaseInsensitive = true,
                   })
               ?? new ClientConfiguration();
    }
}
