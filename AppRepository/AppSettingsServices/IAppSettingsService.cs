using Newtonsoft.Json.Linq;

namespace SlqStudio.Application.Services.AppSettingsServices;

public interface IAppSettingsService
{
    JObject ReadConfig(string configPath);
    void WriteConfig(JObject config, string configPath);
}
