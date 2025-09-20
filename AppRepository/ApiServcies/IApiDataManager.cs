namespace Servcies.ApiServcies
{
    public interface IApiDataManager<T> where T : IApiDataManager<T>
    {
        Task<bool> GetTestRequest(string clientId, string apiKey);
        T SetClient(string clientId, string apiKey);
    }
}
