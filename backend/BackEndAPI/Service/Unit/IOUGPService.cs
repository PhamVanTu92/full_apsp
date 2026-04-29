using BackEndAPI.Models.ItemGroups;
using BackEndAPI.Models.ItemMasterData;
using BackEndAPI.Models.Other;
using BackEndAPI.Models.Unit;

namespace BackEndAPI.Service.Unit
{
    public interface IOUGPService:IService<OUGP>
    {
        Task<(OUGP,Mess)> GetOUGPByIdAsync(int id);
        Task<(OUGP,Mess)> UpdateOUGPAsync(int id, OUGP model);
        Task<(bool, Mess)> DeleteOUGPAsync(int id);

        Task<(List<OUGP>?, Mess?, int)> GetOUGPs(string? search, int skip, int limit);
        Task<(bool, Mess)> SyncOUGPAsync(List<OUGP> model);
    }
}
