using BackEndAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace BackEndAPI.Service.Privile;

public class PrivileService(AppDbContext dbContext) : IPrivileService
{
    public async Task<bool> UserHasPrivilegeAsync(int userId, string privilegeName)
    {
        var user = await dbContext.AppUser
            .Include(p => p.Role)
            .ThenInclude(p => p.RoleClaims)
            .ThenInclude(p => p.Privilege)
            .Include(p => p.PersonRole)
            .ThenInclude(p => p.RoleClaims)
            .ThenInclude(r => r.Privilege)
            .AsNoTracking().AsSplitQuery()
            .FirstOrDefaultAsync(p => p.Id == userId && p.Status != "D");
        if (user == null) return false;

        if (user.IsSupperUser ||  user.UserType == "NPP") return true;

        if (user.Role != null)
            foreach (var u in user!.Role!.RoleClaims)
            {
                if (privilegeName.Trim() == u.PrivilegeCode.Trim()) return true;
            }

        if (user.PersonRole != null)
        {
            foreach (var u in user!.PersonRole!.RoleClaims)
            {
                if (privilegeName.Trim() == u.PrivilegeCode.Trim()) return true;
            }
        }

        return false;
    }
}