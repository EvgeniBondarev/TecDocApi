namespace Services.CacheServcies.Cache
{
    public interface IAppCache<T>
    {
        public Task<List<T>> Get();

        public Task<List<T>> Get(int page);
        public Task<List<T>> Set();
        public Task Update();
    }
}
