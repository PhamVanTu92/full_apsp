using BackEndAPI.Models.ItemMasterData;
using BackEndAPI.Models.Other;
using BackEndAPI.Models.Promotion;

namespace BackEndAPI.Service
{
    public interface IService<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync(int skip, int limit);
        Task<(IEnumerable<T>,Mess)> GetAllAsync();
        Task<(T, Mess)> GetByIdAsync(int id, params string[] child);
        Task<(T,Mess)> AddAsync(T entity);
        Task<IEnumerable<T>> AddListAsync(List<T> entity);
        Task<(T, Mess)> UpdateAsync(int id,T entity);
        Task<(bool,Mess)> DeleteAsync(int id);
        Task<int> CountAsync();
        Task<(IEnumerable<T> t, int total)> GetAllWithPaginationAsync(int skip, int limit);
        Task<(IEnumerable<T> t, int total, Mess)> GetAllWithPaginationAsync(int skip, int limit, params string[] child);
        Task<string> GenerateByCode(string prefix, string suffixes, int length, T entity);
        Task<(IEnumerable<T>, Mess)> GetAsync(List<string> child, params string[] search);
        Task<(IEnumerable<T>, Mess)> GetAllAsync(params string[] child);
    }
    public interface IModelUpdater
    {
        void UpdateModel<TModel, TViewModel>(TModel model, TViewModel viewModel, params string[] param);
    }
}
