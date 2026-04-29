using BackEndAPI.Data;
using BackEndAPI.Models.Approval;
using BackEndAPI.Models.Other;
using Gridify;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace BackEndAPI.Service.Approval
{
    public class ApprovalStageService : Service<OWST>, IApprovalStageService
    {
        private readonly AppDbContext         _context;
        private readonly IHostingEnvironment  _webHostEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IModelUpdater        _modelUpdater;

        public ApprovalStageService(AppDbContext context, IHostingEnvironment webHostEnvironment,
            IModelUpdater modelUpdater, IHttpContextAccessor httpContextAccessor) : base(context)
        {
            _context             = context;
            _webHostEnvironment  = webHostEnvironment;
            _modelUpdater        = modelUpdater;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<(OWST?, Mess?)> CreateOWSTAsync(OWST model)
        {
            Mess mess = new Mess();
            try
            {
                _context.OWST.Add(model);
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

        public async Task<(bool, Mess?)> DeleteOWSTAsync(int id)
        {
            var mess = new Mess();
            using (var tran = _context.Database.BeginTransaction())
            {
                try
                {
                    var owst = await _context.OWST.FindAsync(id);
                    if (owst == null)
                    {
                        mess.Status = 404;
                        mess.Errors = "Cấp phê duyệt không tồn tại";
                        await tran.RollbackAsync();
                        return (false, mess);
                    }

                    var checkExist = await _context.WTM2.CountAsync(p => p.WtsId == id);
                    if (checkExist > 0)
                    {
                        mess.Status = 400;
                        mess.Errors = "Cấp phê duyệt đã được sử dụng";
                        await tran.RollbackAsync();
                        return (false, mess);
                    }

                    _context.OWST.Remove((OWST)owst);
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

        public async Task<(IEnumerable<OWST>, Mess, int total)> GetAllOWSTAsync(int skip, int limit, string? search,
            GridifyQuery q)
        {
            Mess mess = new Mess();
            try
            {
                var query = _context.Set<OWST>().AsQueryable().ApplyFiltering(q);
                if (!search.IsNullOrEmpty())
                {
                    query = query.Where(e => e.Name.Contains(search) || e.Remarks.Contains(search));
                }

                var totalCount = await query.CountAsync();
                var items = await query
                    .Include(p => p.WST1)!
                    .ThenInclude(p => p.User)
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

        public async Task<(OWST, Mess)> GetOWSTByIdAsync(int id)
        {
            Mess mess = new Mess();
            try
            {
                var items = await _context.OWST.AsNoTracking()
                    .Include(p => p.WST1)!
                    .ThenInclude(p => p.User)
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

        public async Task<(OWST, Mess)> UpdateOWSTAsync(int id, OWST model)
        {
            Mess mess = new Mess();
            try
            {
                var item = await _context.OWST
                    .AsNoTracking()
                    .Include(p => p.WST1)
                    .FirstOrDefaultAsync(p => p.Id == id);
                var checkExist = await _context.WTM2.CountAsync(p => p.WtsId == id);
                if (checkExist > 0)
                {
                    mess.Status = 400;
                    mess.Errors = "Mẫu phê duyệt đã được sử dụng";
                    return (null, mess);
                }

                if (item == null)
                {
                    mess.Status = 400;
                    mess.Errors = "Không tìm thấy bản ghi để cập nhập";
                    return (null, mess);
                }

                if (id != model.Id)
                {
                    mess.Status = 400;
                    mess.Errors = "Không tìm thấy bản ghi để cập nhập";
                    return (null, mess);
                }

                _modelUpdater.UpdateModel(item, model, "WST1", "NA1", "NA1", "NA1", "NA1", "NA5");
                if (model.WST1 != null && model.WST1.Count != 0)
                {
                    var fwstList = model.WST1;
                    model.WST1 = null;
                    foreach (var fWst in fwstList)
                    {
                        if (fWst.Status == null) fWst.Status = "";

                        if (fWst.Status!.Equals("D"))
                        {
                            _context.WST1.Remove(fWst);
                            item?.WST1?.Remove(fWst);
                        }
                        else if (fWst.Status.Equals("U"))
                        {
                            var dtoCRD1    = fWst.GetType();
                            var entityCRD1 = fWst.GetType();

                            foreach (var prop in dtoCRD1.GetProperties())
                            {
                                var dtoValue = prop.GetValue(fWst);
                                if (dtoValue != null)
                                {
                                    var entityProp = entityCRD1.GetProperty(prop.Name);
                                    if (entityProp != null)
                                    {
                                        entityProp.SetValue(fWst, dtoValue);
                                    }
                                }
                            }
                        }
                        else if (fWst.Status.Equals("A") || fWst.Id == 0)
                            item?.WST1?.Add(fWst);
                    }

                    item.WST1 = fwstList;
                }

                _context.OWST.Update(item);
                await _context.SaveChangesAsync();
                return (item, null);
            }
            catch (Exception ex)
            {
                mess.Status = 400;
                mess.Errors = ex.Message;
                return (null, mess);
            }
        }
    }
}