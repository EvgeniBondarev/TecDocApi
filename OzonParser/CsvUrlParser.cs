using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OzonParser
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

                        // Используем CsvHelper для чтения CSV файла
                        using (var reader = new StreamReader(contentStream))
                        using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
                        {
                            Delimiter = ";",
                            HasHeaderRecord = true // Предполагаем, что первая строка содержит заголовки столбцов
                        }))
                        {
                            // Считываем записи из CSV файла
                            var records = csv.GetRecords<dynamic>().ToList();

                            // Преобразуем записи в формат JSON
                            var json = JsonConvert.SerializeObject(records);

                            // Десериализуем JSON в объект JArray
                            var jsonArray = JArray.Parse(json);

                            return jsonArray;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Произошла ошибка при чтении файла: {ex.Message}");
            }
        }
    }
}
