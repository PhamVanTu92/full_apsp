using BackEndAPI.Models.Approval;
using BackEndAPI.Models.Other;
using Gridify;

namespace BackEndAPI.Service.Approval
{
    public interface IApprovalStageService : IService<OWST>
    {
        Task<(bool, Mess?)>                        DeleteOWSTAsync(int id);
        Task<(IEnumerable<OWST>, Mess, int total)> GetAllOWSTAsync(int skip, int limit, string? search, GridifyQuery q);
        Task<(OWST, Mess)>                         GetOWSTByIdAsync(int id);
        Task<(OWST, Mess)>                         CreateOWSTAsync(OWST model);
        Task<(OWST, Mess)>                         UpdateOWSTAsync(int id, OWST model);
    }
}