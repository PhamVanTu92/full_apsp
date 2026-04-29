using BackEndAPI.Models.ItemGroups;
using BackEndAPI.Models.Other;

namespace BackEndAPI.Service.Brands
{
    public interface IItemTypeService:IService<ItemType>
    {
        Task<(bool, Mess)> SyncItemTypeAsync(List<ItemType> models);
    }
}
