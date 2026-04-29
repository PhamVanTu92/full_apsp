using BackEndAPI.Models.ItemGroups;
using BackEndAPI.Models.ItemMasterData;
using BackEndAPI.Models.Other;
using Gridify;
using Microsoft.EntityFrameworkCore;

namespace BackEndAPI.Service.ItemGroups
{
    public interface IItemGroupService:IService<OIBT>
    {
        Task<(List<OIBT>, int)> GetItemGroupAsync(GridifyQuery query, string? search);
        Task<List<OIBT>> GetItemGroupAsync(string search);
        Task<OIBT> CreateItemAsync(OIBT model);
        Task<OIBT> UpdateItemGroupAsync(int id, OIBT model);
        Task<bool> DeleteItemGroupAsync(int id);
        Task<bool> ItemGroupExistsAsync(int? parentId, int? currentId = null);
        // Task<(OIBT?, Mess?)> AddCustomerToGroup(int groupId, List<int> customerIds);
        // Task<(OIBT?, Mess?)> RemoveCustomerFromGroup(int groupId, List<int> customerIds);
        Task<(OIBT?, Mess?)> AddCondToGroup(int groupId, List<ConditionItemGroup> conds);
        Task<(OIBT?, Mess?)> RemoveCondFromGroup(int groupId, List<int> condIds);
        Task<Mess?> UpdateGroupCondtion(int groupId, List<ConditionItemGroup> conds);
        Task<OIBT?> GetItemGroupByIdAsync(int id);
    }
}
