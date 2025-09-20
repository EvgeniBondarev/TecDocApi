namespace OzonRepositories.Data
{
    public interface IRepository<T>
    {
        Task<T> GetAsync(int id);

        Task<T> GetAsync(T value);

        Task<List<T>> Get();

        Task<int> Add(T value);

        Task<int> Update(T value);

        Task<int> Delete(T value);
    }
}
