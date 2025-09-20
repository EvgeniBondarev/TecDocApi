public class AbcpApiUrl
{
    public const string BASE_URL = "https://abcp78593.public.api.abcp.ru";
    
    public static string SearchBatchUrl(string domain) => 
        string.Format(BASE_URL, domain) + "/search/batch";
}