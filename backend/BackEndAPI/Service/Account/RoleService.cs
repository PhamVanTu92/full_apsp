using BackEndAPI.Data;
using BackEndAPI.Models.Account;
using BackEndAPI.Models.Other;
using BackEndAPI.Models.Promotion;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Gridify;

namespace BackEndAPI.Service.Account
{
    public class RoleService : IRoleService
    {
        private readonly AppDbContext _context;
        private readonly IModelUpdater _modelUpdater;
        private readonly RoleManager<AppRole> _roleManager;
        private readonly LoggingSystemService _systemLog;

        public RoleService(AppDbContext context, IModelUpdater modelUpdater, RoleManager<AppRole> roleManager, LoggingSystemService systemLog)
        {
            _context = context;
            _modelUpdater = modelUpdater;
            _roleManager = roleManager;
            _systemLog = systemLog;
        }

        public async Task<(AppRole, Mess)> AddRole(AppRole appRole)
        {
            Mess mes = new Mess();
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                if (AppDbContext.IsUniqueAsync<AppRole>(_context, u => u.Name == appRole.Name))
                {
                    mes.Errors = "Tên đã tồn tại";
                    mes.Status = 400;
                    return (null, mes);
                }

                if (appRole.Name.Length <= 3)
                {
                    mes.Errors = "Name must be at least 3 characters long.";
                    mes.Status = 400;
                    return (appRole, mes);
                }

                _context.AppRole.Add(appRole);
                await _context.SaveChangesAsync();
                await _systemLog.SaveWithTransAsync("INFO", "Create", $"Tạo vai trò mới {appRole.Name}", "Role", appRole.Id);
                await transaction.CommitAsync();
                return (appRole, null);
            }
            catch (Exception ex)
            {
                mes.Status = 900;
                mes.Errors = ex.Message;
                await transaction.RollbackAsync();
                return (null, mes);
            }
        }

        public async Task<(bool, Mess)> DeleteRole(int RoleId)
        {
            Mess mes = new Mess();
            try
            {
                var appRole = await _context.Set<AppRole>().FirstOrDefaultAsync(x => x.Id == RoleId);
                if (appRole == null)
                {
                    mes.Status = 400;
                    mes.Errors = "Không tồn tại vai trò";
                    return (false, mes);
                }

                var appUser = _context.Set<AppUserRole>().FirstOrDefault(x => x.RoleId == RoleId);
                if (appUser != null)
                {
                    mes.Status = 400;
                    mes.Errors = "Vai trò đã được sử dụng";
                    return (false, mes);
                }

                _context.Set<AppRole>().Remove(appRole);
                await _systemLog.SaveAsync("INFO", "Delete", $"Xóa vai trò {appRole.Name}", "Role", appRole.Id);
                await _context.SaveChangesAsync();
                return (true, null);
            }
            catch (Exception ex)
            {
                mes.Status = 900;
                mes.Errors = ex.Message;
                return (false, mes);
            }
        }


        public async Task<(IEnumerable<AppRole>, Mess)> GetRole()
        {
            Mess mes = new Mess();
            try
            {
                var appRole = await _context.Set<AppRole>().AsQueryable()
                    .AsNoTracking()
                    .Include(x => x.RoleClaims)
                    .Where(x => x.Name != "Admin")
                    .Where(x => x.IsPersonRole == false)
                    .ToListAsync();
                appRole.ForEach(a =>
                {
                    var countPersonInRole = _context.Users.Count(p => p.RoleId == a.Id);
                    a.CountUserInRole = countPersonInRole;
                });
                return (appRole, null);
            }
            catch (Exception ex)
            {
                mes.Status = 900;
                mes.Errors = ex.Message;
                return (null, mes);
            }
        }

        public List<AppRoleClaim> BuildTree(List<AppRoleClaim> roleClaims)
        {
            // Dictionary để lưu các node cha và quyền con của chúng
            var lookup = roleClaims.ToLookup(c => c.Privilege?.ParentId);

            // Hàm đệ quy để xây dựng cây
            List<AppRoleClaim> BuildNode(int? parentId)
            {
                var claims = lookup[parentId].ToList();

                foreach (var claim in claims)
                {
                    // Gọi đệ quy để lấy quyền con của mỗi node
                    claim.Children = BuildNode(claim?.Privilege?.Id);
                }

                return claims;
            }

            // Xây dựng cây bắt đầu từ các quyền gốc (ParentId = null)
            return BuildNode(null);
        }

        public async Task<(AppRole?, Mess?)> GetRoleClaim(int RoleId)
        {
            Mess mes = new Mess();
            try
            {
                var appRoleClaim = await _context.Set<AppRole>().AsQueryable()
                    .Include(x => x.RoleClaims)
                    .ThenInclude(x => x.Privilege)
                    .Include(e => e.RoleFillCustomerGroups)
                    .Where(r => r.Id == RoleId)
                    .OrderByDescending(e => e.Id)
                    .FirstOrDefaultAsync();


                return (appRoleClaim, null);
            }
            catch (Exception ex)
            {
                mes.Status = 900;
                mes.Errors = ex.Message;
                return (null, mes);
            }
        }

        public async Task<(AppRole?, Mess?)> UpdateRole(int id, AppRole appRole)
        {
            Mess mess = new Mess();
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                if (appRole.Name.Length <= 3)
                {
                    mess.Errors = "Name must be at least 3 characters long.";
                    mess.Status = 400;
                    return (appRole, mess);
                }

                var role = await _context.AppRole.Include(p => p.RoleClaims)
                    .Include(appRole => appRole.RoleFillCustomerGroups).FirstOrDefaultAsync(x => x.Id == id);
                if (role == null)
                {
                    mess.Status = 404;
                    mess.Errors = "Role not found";
                    await transaction.RollbackAsync();
                    return (null, mess);
                }

                role.RoleFillCustomerGroups.RemoveAll(e => e.RoleId == id);
                
                appRole.RoleFillCustomerGroups.ForEach(e => e.RoleId = id);

                role.Name = appRole.Name;
                role.NormalizedName = appRole?.Name?.ToUpper();
                role.IsActive = appRole.IsActive;
                role.Notes = appRole.Notes;
                role.RoleFillCustomerGroups.AddRange(appRole.RoleFillCustomerGroups);
                role.IsFillCustomerGroup = appRole.IsFillCustomerGroup;
                role.IsSaleRole = appRole.IsSaleRole;
                

                // var curPriIds = role.RoleClaims.Select(p => p.PrivilegesId).ToList();
                // var delPriIds = curPriIds.Except(appRole.PrivilegeIds).ToList();
                // role.RoleClaims.RemoveAll(p => delPriIds.Contains(p.PrivilegesId));
                // var addPriIds = appRole.PrivilegeIds.Except(curPriIds).ToList();
                //
                // foreach (var _id in addPriIds)
                // {
                //     role.RoleClaims.Add(new AppRoleClaim()
                //     {
                //         PrivilegesId = _id
                //     });
                // }

                role.RoleClaims.Clear();
                await _context.SaveChangesAsync();

                role.RoleClaims = appRole!.RoleClaims;

                await _context.SaveChangesAsync();
                await _systemLog.SaveWithTransAsync("INFO", "Update", $"Cập nhật vai trò {appRole.Name}", "Role", appRole.Id);
                await transaction.CommitAsync();
                return (role, null);
            }
            catch (Exception ex)
            {
                mess.Errors = ex.Message;
                mess.Status = 500;
                await transaction.RollbackAsync();

                return (null, mess);
            }
        }

        public async Task<(AppRole, Mess)> UpdateRoles(int id, AppRole appRole)
        {
            Mess mes = new Mess();
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var role = await _roleManager.FindByIdAsync(id.ToString());
                    if (role == null)
                    {
                        mes.Status = 400;
                        mes.Errors = "Không tồn tại bản ghi cập nhập";
                        transaction.Rollback();
                        return (null, mes);
                    }

                    if (role.Id != id)
                    {
                        mes.Status = 400;
                        mes.Errors = "Không tồn tại bản ghi cập nhập";
                        transaction.Rollback();
                        return (null, mes);
                    }

                    role.Name = appRole.Name;
                    _context.Set<AppRole>().Update(role);
                    var claimsC = await _roleManager.GetClaimsAsync(role);
                    if (claimsC.Count > 0)
                    {
                        foreach (var claimsCs in claimsC)
                        {
                            var resultR = await _roleManager.RemoveClaimAsync(role, claimsCs);
                            if (!resultR.Succeeded)
                            {
                                mes.Status = 500;
                                mes.Errors = "Không thể xóa phân quyền";
                                transaction.Rollback();
                                return (null, mes);
                            }
                        }
                    }

                    List<AppRoleClaim> listRoleClaim = new List<AppRoleClaim>();

                    foreach (var roleClaim in appRole.RoleClaims)
                    {
                        AppRoleClaim appRoleClaim = new AppRoleClaim();
                        appRoleClaim.RoleId = id;
                        appRoleClaim.PrivilegesId = roleClaim.PrivilegesId;
                        appRoleClaim.ClaimType = roleClaim.ClaimType;
                        appRoleClaim.ClaimValue = roleClaim.ClaimValue;
                        appRoleClaim.Checked = roleClaim.Checked;
                        listRoleClaim.Add(appRoleClaim);
                    }

                    var userClaimCheck = _context.Set<AppRoleClaim>().AddRangeAsync(listRoleClaim);
                    await _context.SaveChangesAsync();
                    var approles = await _roleManager.FindByIdAsync(id.ToString());
                    transaction.Commit();
                    return (approles, null);
                }
                catch (Exception ex)
                {
                    mes.Status = 900;
                    mes.Errors = ex.Message;
                    transaction.Rollback();
                    return (null, mes);
                }
            }
        }
    }
}