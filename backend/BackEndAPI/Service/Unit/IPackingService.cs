using BackEndAPI.Models.ItemGroups;
using BackEndAPI.Models.Other;
using BackEndAPI.Models.Unit;

namespace BackEndAPI.Service.Unit
{
    public interface IPackingService : IService<Packing>
    {
        Task<(bool, Mess)> SyncPackingAsync(List<Packing> model);
    }
}
