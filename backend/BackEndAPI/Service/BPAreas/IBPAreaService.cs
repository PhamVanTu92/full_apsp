
using BackEndAPI.Models.BusinessPartners;
using BackEndAPI.Models.Other;

namespace BackEndAPI.Service.ItemAreas
{
    public interface IBPAreaService : IService<BPArea>
    {
        Task<(List<BPArea>,Mess)> GetBPAreaAsync();
        Task<(List<BPArea>,Mess)> GetBPAreaAsync(string search);
        Task<(BPArea, Mess)> CreateBPAreaAsync(BPArea model);
        Task<(BPArea, Mess)> UpdateBPAreaAsync(int id, BPArea model);
        Task<(bool,Mess)> DeleteBPAreaAsync(int id);
        Task<(bool, Mess)> BPAreaExistsAsync(int? parentId, int? currentId = null);
        Task<(bool, Mess)> CanDeleteBPAreaAsync(int id);
    }
}
