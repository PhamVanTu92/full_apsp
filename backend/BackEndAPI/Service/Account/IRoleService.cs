using BackEndAPI.Models.Account;
using BackEndAPI.Models.Other;
using Gridify;

namespace BackEndAPI.Service.Account
{
    public interface IRoleService
    {
        Task<(IEnumerable<AppRole>, Mess)> GetRole();
        Task<(AppRole?, Mess)> GetRoleClaim(int RoleId);
        Task<(bool, Mess)> DeleteRole(int RoleId);
        Task<(AppRole, Mess)> AddRole(AppRole appRole);
        Task<(AppRole?, Mess?)> UpdateRole(int id,AppRole appRole);
    }
}
