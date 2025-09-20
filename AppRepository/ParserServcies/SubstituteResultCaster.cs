using Newtonsoft.Json.Linq;
using OzonDomains.TecDocModels.Substitute;
using Attribute = OzonDomains.TecDocModels.Substitute.Attribute;

namespace Servcies.ParserServcies
{
    public class SubstituteResultCaster
    {
        public async Task<SubstituteResultSchema> ParseSubstituteResultSchemaAsync(JObject json)
        {
            var result = new SubstituteResultSchema();

            if (json != null)
            {
                result.SubstituteCount = json["SubstitutesCount"]?.ToObject<int>() ?? 0;

                if (json["Substitutes"] is JArray substitutesArray)
                {
                    var substituteSchemas = new List<SubstituteSchema>();

                    foreach (var substituteItem in substitutesArray)
                    {
                        if (substituteSchemas.Count >= 100)
                        {
                            break;
                        }
                        var substituteSchema = new SubstituteSchema
                        {
                            Type = substituteItem["Type"]?.ToString(),
                            Name = substituteItem["Name"]?.ToString(),
                            Modification = substituteItem["Modification"]?.ToObject<Modification>(),
                            Attributes = substituteItem["Attributes"]?.ToObject<List<Attribute>>() ?? new List<Attribute>()
                        };

                        substituteSchemas.Add(substituteSchema);
                    }

                    result.SubstituteSchema = substituteSchemas;
                }
            }

            return await Task.FromResult(result);
        }
    }
}
