

using BackEndAPI.Models.BPGroups;
using BackEndAPI.Models.Other;

namespace BackEndAPI.Service.Privilege
{
    public interface IPrivilegesServicecs : IService<Privileges>
    {
        Task<(List<Privileges>,Mess)> GetPrivilegesAsync();
    }
}
