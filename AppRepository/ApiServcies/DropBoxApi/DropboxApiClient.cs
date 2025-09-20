using Dropbox.Api.Files;
using Dropbox.Api;
using Newtonsoft.Json;


namespace Servcies.ApiServcies.DropBoxApi
{
    public class DropboxApiClient
    {
        private string _accessToken;
        private readonly string _refreshToken;
        private readonly string _appKey;
        private readonly string _appSecret;

        public DropboxApiClient(string refreshToken, string appKey, string appSecret)
        {
            _refreshToken = refreshToken;
            _appKey = appKey;
            _appSecret = appSecret;
        }

        private async Task<string> RefreshAccessTokenAsync()
        {
            var client = new HttpClient();
            var requestContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "refresh_token"),
                new KeyValuePair<string, string>("refresh_token", _refreshToken),
                new KeyValuePair<string, string>("client_id", _appKey),
                new KeyValuePair<string, string>("client_secret", _appSecret)
            });

            var response = await client.PostAsync("https://api.dropbox.com/oauth2/token", requestContent);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseContent);

            _accessToken = tokenResponse["access_token"];
            return _accessToken;
        }
        
        public async Task<string> GetViewLinkAsync(string filePath, string fileName)
        {
            var baseSegments = new[] { "Приложения", "Studio2", "Orders" }
                .Select(Uri.EscapeDataString);
            var pathSegments = filePath.Split('/', StringSplitOptions.RemoveEmptyEntries)
                .Select(Uri.EscapeDataString);
            var fileSegment = Uri.EscapeDataString(fileName);
            var allSegments = baseSegments.Concat(pathSegments).Concat(new[] { fileSegment });
            var encodedPath = string.Join('/', allSegments);
            return $"https://www.dropbox.com/home/{encodedPath}";
        }

        public async Task UploadFileAsync(string localFilePath, string dropboxFolderPath, string dropboxFileName)
        {
            await RefreshAccessTokenAsync();

            using (var dropboxClient = new DropboxClient(_accessToken))
            {
                using (var fileStream = new FileStream(localFilePath, FileMode.Open, FileAccess.Read))
                {
                    await dropboxClient.Files.UploadAsync(
                        path: $"{dropboxFolderPath}/{dropboxFileName}",
                        WriteMode.Overwrite.Instance,
                        body: fileStream
                    );
                }
            }
        }
        
        public async Task<Dictionary<string, Dictionary<string, string>>> GetFolderContentsAsync(string folderPath)
        {
            var result = new Dictionary<string, Dictionary<string, string>>();
            try
            {
                await RefreshAccessTokenAsync();
                using (var client = new DropboxClient(_accessToken))
                {
                    // Получаем список файлов и папок в указанной папке
                    var list = await client.Files.ListFolderAsync(folderPath);

                    // Проходим по всем элементам в папке
                    foreach (var item in list.Entries.Where(i => i.IsFolder))
                    {
                        var folderName = item.AsFolder.Name;
                        var folderPathLower = item.AsFolder.PathLower;

                        // Получаем список файлов в подпапке
                        var folderFiles = await client.Files.ListFolderAsync(folderPathLower);

                        // Создаем словарь для хранения названий файлов и ссылок
                        var fileLinks = new Dictionary<string, string>();
                        foreach (var file in folderFiles.Entries.Where(i => i.IsFile))
                        {
                            var fileName = file.AsFile.Name; // Оригинальное название файла
                            var filePath = file.AsFile.PathLower;
                            var link = await client.Files.GetTemporaryLinkAsync(filePath);
                            fileLinks[fileName] = link.Link; // Добавляем название файла и ссылку
                        }

                        // Добавляем в результат
                        result[folderName] = fileLinks;
                    }
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
           

            return result;
        }
    }
}
