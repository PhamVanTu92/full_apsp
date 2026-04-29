using BackEndAPI.Data;
using BackEndAPI.Models.Approval;
using BackEndAPI.Models.Other;
using Gridify;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace BackEndAPI.Service.Approval
{
    public class ApprovalTemplateService : Service<OWTM>, IApprovalTemplateService
    {
        private readonly AppDbContext         _context;
        private readonly IWebHostEnvironment  _webHostEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IModelUpdater        _modelUpdater;

        public ApprovalTemplateService(AppDbContext context, IWebHostEnvironment webHostEnvironment,
            IModelUpdater modelUpdater, IHttpContextAccessor httpContextAccessor) : base(context)
        {
            _context             = context;
            _webHostEnvironment  = webHostEnvironment;
            _modelUpdater        = modelUpdater;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<(OWTM, Mess)> CreateOWTMAsync(OWTM model)
        {
            Mess mess = new Mess();
            try
            {
                var rUsers = await _context.AppUser.Where(p => model.RUserIds.Contains(p.Id)).ToListAsync();
                model.RUsers = rUsers;
                _context.OWTM.Add(model);
                await _context.SaveChangesAsync();
                return (model, null);
            }
            catch (Exception ex)
            {
                mess.Status = 900;
                mess.Errors = ex.Message;
                return (null, mess);
            }
        }

        public async Task<(bool, Mess)> DeleteOWTMAsync(int id)
        {
            var mess = new Mess();
            using (var tran = _context.Database.BeginTransaction())
            {
                try
                {
                    var appr = await _context.Approval.FirstOrDefaultAsync(p => p.WtmId == id);
                    if (appr != null)
                    {
                        mess.Status = 403;
                        mess.Errors = "Không thể xóa mẫu đã được dùng";
                    }

                    var owtm = await _context.OWTM.FindAsync(id);
                    if (owtm == null)
                    {
                        mess.Status = 404;
                        mess.Errors = "Không tồn tại mẫu phê duyệt";
                        await tran.RollbackAsync();
                        return (false, mess);
                    }

                    _context.OWTM.Remove(owtm);
                    await _context.SaveChangesAsync();
                    await tran.CommitAsync();
                }
                catch (Exception ex)
                {
                    mess.Errors = ex.Message;
                    mess.Status = 500;
                }
            }

            return (true, null);
        }

        public async Task<(IEnumerable<OWTM>, Mess, int total)> GetAllOWTMAsync(int skip, int limit, string? search,
            GridifyQuery q)
        {
            Mess mess = new Mess();
            try
            {
                var query = _context.Set<OWTM>().AsQueryable().ApplyFiltering(q);
                if (!search.IsNullOrEmpty())
                {
                    query = query.Where(e => e.Name.Contains(search) || e.Remarks.Contains(search));
                }

                var totalCount = await query.CountAsync();
                var items = await query
                    .Include(p => p.WTM1)
                    .Include(p => p.WTM2)
                    .Include(p => p.WTM3)
                    .Include(p => p.WTM4)
                    .OrderByDescending(p => p.Id)
                    .ToListAsync();
                return (items, null, totalCount);
            }
            catch (Exception ex)

            {
                mess.Status = 900;
                mess.Errors = ex.Message;
                return (null, mess, 0);
            }
        }

        public async Task<(OWTM, Mess)> GetOWTMByIdAsync(int id)
        {
            Mess mess = new Mess();
            try
            {
                var items = await _context.OWTM.AsNoTracking()
                    .Include(p => p.WTM1)
                    .Include(p => p.WTM2)
                    .Include(p => p.WTM3)
                    .Include(p => p.WTM4)
                    .Include(p => p.RUsers)
                    .ThenInclude(p => p.Role)
                    .ThenInclude(p => p.UserRoles)
                    .FirstOrDefaultAsync(p => p.Id == id);
                return (items, null);
            }
            catch (Exception ex)
            {
                mess.Status = 900;
                mess.Errors = ex.Message;
                return (null, mess);
            }
        }

        public async Task<(OWTM, Mess)> UpdateOWTMAsync(int id, OWTM items)
        {
            Mess mess = new Mess();
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var item = await _context.OWTM.AsNoTracking()
                        .Include(p => p.WTM1)
                        .Include(p => p.WTM2)
                        .Include(p => p.WTM3)
                        .Include(p => p.WTM4)
                        .Include(p => p.RUsers)
                        .FirstOrDefaultAsync(p => p.Id == id);
                    if (item == null)
                    {
                        mess.Status = 400;
                        mess.Errors = "Không tìm thấy bản ghi để cập nhập";
                        return (null, mess);
                    }

                    if (id != items.Id)
                    {
                        mess.Status = 400;
                        mess.Errors = "Không tìm thấy bản ghi để cập nhập";
                        return (null, mess);
                    }

                    var newRUser = await _context.AppUser.Where(p => items.RUserIds.Contains(p.Id)).ToListAsync();
                    foreach (var i in item.RUsers)
                    {
                        items.RUsers.Remove(i);
                    }

                    _context.Database.ExecuteSqlRaw("DELETE FROM m2m_ApprovalOWTM WHERE OWTMId  = {0}", items.Id);
                    items.RUsers.AddRange(newRUser);

                    _modelUpdater.UpdateModel(item, items, "WTM1", "WTM2", "WTM3", "WTM4", "CRD5", "NA5");
                    if (items.WTM1 != null)
                    {
                        foreach (var crd1 in items.WTM1.ToList())
                        {
                            var crd1s = item.WTM1
                                .FirstOrDefault(c => c.Id == crd1.Id && c.Id != 0);

                            if (crd1s != null)
                            {
                                if (string.IsNullOrEmpty(crd1.Status))
                                {
                                }
                                else if (crd1.Status.Equals("D"))
                                {
                                    _context.WTM1.Remove(crd1s);
                                    item.WTM1.Remove(crd1s);
                                }
                                else if (crd1.Status.Equals("U"))
                                {
                                    var dtoCRD1    = crd1.GetType();
                                    var entityCRD1 = crd1s.GetType();

                                    foreach (var prop in dtoCRD1.GetProperties())
                                    {
                                        var dtoValue = prop.GetValue(crd1);
                                        if (dtoValue != null)
                                        {
                                            var entityProp = entityCRD1.GetProperty(prop.Name);
                                            if (entityProp != null)
                                            {
                                                entityProp.SetValue(crd1s, dtoValue);
                                            }
                                        }
                                    }
                                }
                            }
                            else
                                item.WTM1.Add(crd1);
                        }
                    }

                    if (items.WTM2 != null)
                    {
                        foreach (var crd2 in items.WTM2.ToList())
                        {
                            var crd2s = item.WTM2
                                .FirstOrDefault(c => c.Id == crd2.Id && c.Id != 0);

                            if (crd2s != null)
                            {
                                if (string.IsNullOrEmpty(crd2.Status))
                                {
                                }
                                else if (crd2.Status.Equals("D"))
                                {
                                    _context.WTM2.Remove(crd2s);
                                    item.WTM2.Remove(crd2s);
                                }
                                else if (crd2.Status.Equals("U"))
                                {
                                    var dtoCRD1    = crd2.GetType();
                                    var entityCRD1 = crd2s.GetType();

                                    foreach (var prop in dtoCRD1.GetProperties())
                                    {
                                        var dtoValue = prop.GetValue(crd2);
                                        if (dtoValue != null)
                                        {
                                            var entityProp = entityCRD1.GetProperty(prop.Name);
                                            if (entityProp != null)
                                            {
                                                entityProp.SetValue(crd2s, dtoValue);
                                            }
                                        }
                                    }
                                }
                            }
                            else
                                item.WTM2.Add(crd2);
                        }
                    }

                    if (items.WTM3 != null)
                    {
                        foreach (var crd2 in items.WTM3.ToList())
                        {
                            var crd2s = item.WTM3
                                .FirstOrDefault(c => c.Id == crd2.Id && c.Id != 0);

                            if (crd2s != null)
                            {
                                if (string.IsNullOrEmpty(crd2.Status))
                                {
                                }
                                else if (crd2.Status.Equals("D"))
                                {
                                    _context.WTM3.Remove(crd2s);
                                    item.WTM3.Remove(crd2s);
                                }
                                else if (crd2.Status.Equals("U"))
                                {
                                    var dtoCRD1    = crd2.GetType();
                                    var entityCRD1 = crd2s.GetType();

                                    foreach (var prop in dtoCRD1.GetProperties())
                                    {
                                        var dtoValue = prop.GetValue(crd2);
                                        if (dtoValue != null)
                                        {
                                            var entityProp = entityCRD1.GetProperty(prop.Name);
                                            if (entityProp != null)
                                            {
                                                entityProp.SetValue(crd2s, dtoValue);
                                            }
                                        }
                                    }
                                }
                            }
                            else
                                item.WTM3.Add(crd2);
                        }
                    }

                    if (items.WTM4 != null)
                    {
                        var crd2  = items.WTM4;
                        var crd2s = item.WTM4;

                        if (crd2s != null)
                        {
                            if (string.IsNullOrEmpty(crd2.Status))
                            {
                            }
                            else if (crd2.Status.Equals("U"))
                            {
                                var dtoCRD1    = crd2.GetType();
                                var entityCRD1 = crd2s.GetType();

                                foreach (var prop in dtoCRD1.GetProperties())
                                {
                                    var dtoValue = prop.GetValue(crd2);
                                    if (dtoValue != null)
                                    {
                                        var entityProp = entityCRD1.GetProperty(prop.Name);
                                        if (entityProp != null)
                                        {
                                            entityProp.SetValue(crd2s, dtoValue);
                                        }
                                    }
                                }
                            }
                        }
                        else
                            item.WTM4 = crd2;
                    }

                    _context.OWTM.Update(item);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return (item, null);
                }
                catch (Exception ex)
                {
                    mess.Status = 400;
                    mess.Errors = ex.Message;
                    await transaction.RollbackAsync();
                    return (null, mess);
                }
            }
        }
    }
}