using Newtonsoft.Json.Linq;

namespace SlqStudio.Application.Services.AppSettingsServices;

public class AppSettingsService : IAppSettingsService
{

    public JObject ReadConfig(string configPath)
    {
        var json = File.ReadAllText(configPath);
        return JObject.Parse(json);
    }

    public void WriteConfig(JObject config, string configPath)
    {
        var updatedJson = config.ToString();
        File.WriteAllText(configPath, updatedJson);
    }
}