using Gridify;

namespace BackEndAPI.Service.GenericeService;

public interface IGenericService<T> where T : class
{
    Task<(IEnumerable<T> Items, int TotalCount)> GetListAsync(GridifyQuery gridifyQuery);
    Task<T?> GetByIdAsync(int id);
    Task<T> CreateAsync(T entity);
    Task<T> UpdateAsync(T entity);
    Task<bool> DeleteAsync(int id);
}