using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Servcies.ApiServcies.ZzapApi.Models.Response;

public class SearchResponseConverter : JsonConverter<SearchResponse>
{
    public override SearchResponse ReadJson(JsonReader reader, Type objectType, SearchResponse existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        JObject jsonObject = JObject.Load(reader);
        var response = new SearchResponse
        {
            Message = jsonObject["error"]?.ToString(),
            Data = new List<SearchResultItem>()
        };
        
        response.Status = string.IsNullOrEmpty(response.Message) ? "success" : "error";
        
        if (response.Status == "success")
        {
            var table = jsonObject["table"];
            if (table != null && table.Type == JTokenType.Array)
            {
                response.Data = table.ToObject<List<SearchResultItem>>(serializer);
            }
        }
        
        return response;
    }

    public override void WriteJson(JsonWriter writer, SearchResponse value, JsonSerializer serializer)
    {
        serializer.Serialize(writer, value);
    }
}