namespace PayrollApi.Services.Interfaces;

public interface ICacheService
{
    T? Get<T>(string key);
    void Set<T>(string key, T value, int minutes = 10);
    T GetOrSet<T>(string key, Func<T> factory, int minutes = 10);
    Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, int minutes = 10);
    void Remove(string key);
    void Clear();
}
