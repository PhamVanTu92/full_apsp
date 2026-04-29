using BackEndAPI.Models.BPGroups;
using BackEndAPI.Models.BusinessPartners;
using BackEndAPI.Models.Other;
using Gridify;

namespace BackEndAPI.Service.BPGroups
{
    public interface IBPGroupService : IService<OCRG>
    {
        Task<(List<OCRG>, int)> GetBPGroupAsync(string groupType, int skip, int limit, string search, GridifyQuery q);
        Task<List<OCRG>> GetBPGroupAsync(string search, string groupType);
        Task<(OCRG?, Mess?)> GetBPGroupById(int id);
        Task<(OCRG?, Mess?)> CreateGroup(OCRG model);
        Task<OCRG> UpdateBPGroupAsync(int id, OCRG model);
        Task<bool> DeleteBPGroupAsync(int id);
        Task<bool> BPGroupExistsAsync(int? parentId, int? currentId = null);
        Task<bool> CanDeleteBPGroupAsync(int id);
        Task<(OCRG?, Mess?)> AddCustomerToGroup(int groupId, List<int> customerIds);
        Task<(OCRG?, Mess?)> RemoveCustomerFromGroup(int groupId, List<int> customerIds);
        Task<(OCRG?, Mess?)> AddCondToGroup(int groupId, List<ConditionCusGroup> conds);
        Task<(OCRG?, Mess?)> RemoveCondFromGroup(int groupId, List<int> condIds);
        Task<Mess?> UpdateGroupCondtion(int groupId, List<ConditionCusGroup> conds);
       Task<(List<BP>?, Mess?)> GetCustomerInCond(OCRG bpGroup);
    }
}