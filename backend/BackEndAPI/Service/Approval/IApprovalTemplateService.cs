using BackEndAPI.Models.Approval;
using BackEndAPI.Models.Other;
using Gridify;

namespace BackEndAPI.Service.Approval
{
    public interface IApprovalTemplateService : IService<OWTM>
    {
        Task<(IEnumerable<OWTM>, Mess, int total)> GetAllOWTMAsync(int skip, int limit, string search, GridifyQuery q);
        Task<(OWTM, Mess)> GetOWTMByIdAsync(int id);
        Task<(OWTM, Mess)> CreateOWTMAsync(OWTM model);
        Task<(OWTM, Mess)> UpdateOWTMAsync(int id, OWTM model);
        Task<(bool, Mess)> DeleteOWTMAsync(int id);
    }
}
