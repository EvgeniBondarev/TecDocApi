using System.Xml.Linq;
using Servcies.ApiServcies.AvdApiConfig;

public class AvdApiClient
{
    private static readonly HttpClient _httpClient = new HttpClient();

    public async Task<XDocument> MakeSoapRequestAsync(string soapAction, string soapBody)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, AvdApiUrl.BASE_URL);
        request.Headers.Add("SOAPAction", soapAction);
        request.Content = new StringContent(soapBody, System.Text.Encoding.UTF8, "text/xml");

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var xmlString = await response.Content.ReadAsStringAsync();
        return XDocument.Parse(xmlString);
    }
}