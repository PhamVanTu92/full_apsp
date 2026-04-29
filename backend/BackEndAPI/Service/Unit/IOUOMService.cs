using BackEndAPI.Models.ItemGroups;
using BackEndAPI.Models.Other;
using BackEndAPI.Models.Unit;

namespace BackEndAPI.Service.Unit
{
    public interface IOUOMService:IService<OUOM>
    {
        Task<(bool, Mess)> DeleteOUOMAsync(int id);
        Task<(bool, Mess)> SyncOUOMAsync(List<Packing> model);
    }
}
