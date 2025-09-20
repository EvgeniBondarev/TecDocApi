using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Servcies.ParserServcies
{
    public class CsvUrlParser
    {
        public async Task<JArray> ReadFileFromUrl(string fileUrl)
        {
            if (string.IsNullOrEmpty(fileUrl))
            {
                throw new ArgumentException($"Ссылка на файл не была передана");
            }

            try
            {
                using (var httpClient = new HttpClient())
                {
                    using (var response = await httpClient.GetAsync(fileUrl))
                    {
                        if (!response.IsSuccessStatusCode)
                        {
                            throw new ArgumentException($"Не удалось получить файл. Код состояния: {response.StatusCode}");
                        }

                        var contentStream = await response.Content.ReadAsStreamAsync();
                        
                        using (var reader = new StreamReader(contentStream))
                        using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
                        {
                            Delimiter = ";",
                            HasHeaderRecord = true 
                        }))
                        {
                            var records = csv.GetRecords<dynamic>().ToList();
                            var json = JsonConvert.SerializeObject(records);
                            var jsonArray = JArray.Parse(json);
                            return jsonArray;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Произошла ошибка при чтении файла: {ex.Message} -> {fileUrl}");
            }
        }
    }
}
