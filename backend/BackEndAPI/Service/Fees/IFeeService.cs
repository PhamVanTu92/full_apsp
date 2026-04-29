using BackEndAPI.Models.Fee;
using BackEndAPI.Models.ItemMasterData;
using BackEndAPI.Models.Other;
using System.Threading.Tasks;
using Gridify;

namespace BackEndAPI.Service.Fees
{
    public interface IFeeService :IService<Fee>
    {
        Task<(Fee, Mess)> CreateFeeAsync(Fee model);
        Task<(Fee, Mess)> UpdateFeeAsync(int id, Fee model);
        Task<(Fee, Mess)> GetFeeByIdAsync(int id);
        Task<(IEnumerable<FeeLine>, Mess)> GetFeeLineAsync();
        Task<(IEnumerable<Fee>, Mess, int)> GetAllFeeLAsync(string? search, GridifyQuery q);
        Task<(bool, Mess)> ActiveFeeAsync(int id);
        Task<(IEnumerable<FeePeriod>, Mess)> GetFeePeriodAsync(int year, int period);

        
    }
    public interface IFeebyCustomerService : IService<FeebyCustomer>
    {
        Task<Mess> GetFeebyCustomerAsync(int year, int period);
        Task<(IEnumerable<FeebyCustomer>, Mess, int)> GetAllFeeByCustomerAsync(int skip = 0, int limit = 30);
        Task<(IEnumerable<FeebyCustomer>, Mess, int)> GetAllFeeByCustomerAsync(int year, int period, string? search, GridifyQuery gq, int skip = 0, int limit = 30);
        Task<(IEnumerable<FeebyCustomer>, Mess, int)> GetAllFeeByCustomerAsync(string userId, int skip = 0, int limit = 30);
        Task<(IEnumerable<FeebyCustomer>, Mess, int)> GetAllFeeByCustomerAsync( string userId,int year, int period, int skip = 0, int limit = 30);
        Task<(FeebyCustomer, Mess)> GetFeeByCustomerAsync(int id);
        Task<(FeebyCustomer, Mess)> GetFeeByCustomerAsync(int id, string userId); 
        Task<(IEnumerable<FeebyCustomer>, Mess)> UpdateStatus(List<int> model);
        Task<(IEnumerable<FeebyCustomer>, Mess)> UpdateConfirmStatus(List<int> model);
        Task<(IEnumerable<FeebyCustomer>, Mess)> UpdatePayStatus(List<int> model);
        Task<Mess?> UpdateNoteFeeByCus(int id, string note);
    }
    public interface IFeeLevelService : IService<FeeLevel>
    {
        Task<(IEnumerable<FeeLevel>, Mess)> CreateFeeLevelAsync(IEnumerable<FeeLevel> model);
        Task<(FeeLevel, Mess)> UpdateFeeLevelAsync(int id, FeeLevel model);
        Task<(FeeLevel, Mess)> GetFeeLevelByIdAsync(int id);
        Task<(IEnumerable<FeeLevel>, Mess,int)> GetAllFeeLevelAsync(int skip = 0, int limit = 30,string search = null);
        Task<(bool, Mess)> DeleteFeeLevelAsync(int id);
    }
}
