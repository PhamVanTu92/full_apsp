using BackEndAPI.Models.Branchs;
using BackEndAPI.Models.Other;

namespace BackEndAPI.Service.Branchs
{
    public interface IBranchService : IService<Branch>
    {
        Task<(IEnumerable<Branch>,Mess, int total)> GetAllBranchAsync(int skip, int limit);
        Task<(IEnumerable<Branch>, Mess)> GetAllBranchAsync();
        Task<(Branch, Mess)> GetBranchByIdAsync(int id);
        Task<(IEnumerable<Branch>, Mess)> GetBranchAsync(string search);
        Task<(Branch, Mess)> CreateBranchAsync(Branch model);
        Task<(Branch, Mess)> UpdateBranchAsync(int id, Branch model);
        Task<(Branch, Mess)> UpdateBranchAsync(int id, string status);
    }
    public interface IBranchAddressService : IService<BranchAddress>
    {
        Task<(IEnumerable<BranchAddress>, Mess, int total)> GetAllBranchAddressAsync(int id,int skip, int limit);
        Task<(BranchAddress, Mess)> CreateBranchAddressAsync(BranchAddress model);
        Task<(BranchAddress, Mess)> UpdateBranchAddressAsync(int id, BranchAddress model);
    }
}