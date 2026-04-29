using BackEndAPI.Data;
using BackEndAPI.Models.BPGroups;
using BackEndAPI.Models.ItemMasterData;
using BackEndAPI.Models.Other;
using BackEndAPI.Models.Promotion;
using BackEndAPI.Models.Unit;
using Microsoft.EntityFrameworkCore;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace BackEndAPI.Service.Unit
{
    public class OUGPService : Service<OUGP>, IOUGPService
    {
        private readonly AppDbContext _context;
        private readonly IHostingEnvironment _webHostEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public OUGPService(AppDbContext context, IHostingEnvironment webHostEnvironment, IHttpContextAccessor httpContextAccessor) : base(context)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<(bool, Mess)> DeleteOUGPAsync(int id)
        {
            Mess mess = new Mess();
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var ougp = await _context.OUGP
                        .AsNoTracking()
                        .FirstOrDefaultAsync(p => p.Id == id);
                    if (ougp == null)
                    {
                        mess.Status = 400;
                        mess.Errors = "Đơn vị tính không tồn tại";
                        return (false, mess);
                    }
                    _context.OUGP.Remove(ougp);
                    await _context.SaveChangesAsync();
                    transaction.Commit();
                    return (true, mess);
                }
                catch (Exception ex)
                {
                    mess.Status = 900;
                    mess.Errors = ex.Message;
                    transaction.Rollback();
                    return (false, mess);
                }
            }
        }

        public async Task<(OUGP,Mess)> GetOUGPByIdAsync(int id)
        {
            Mess mess = new Mess();
            try
            {
                var items = await _context.OUGP
                    .AsNoTracking()
                    .Include(p => p.OUOM)
                    .Include(p => p.UGP1)
                    .ThenInclude(p=>p.OUOM)
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

        public async Task<(List<OUGP>?, Mess?, int)> GetOUGPs(string? search, int skip, int limit)
        {
            var mess = new Mess();
            try
            {
                var query = _context.OUGP.AsQueryable();
                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(p => p.UgpCode.ToLower().Contains(search.ToLower()) || p.UgpName.ToLower().Contains(search.ToLower()));
                }
                
                var total = await query.CountAsync();
                var data = await query.OrderByDescending(p => p.Id).Skip(skip*limit).Take(limit).ToListAsync();

                return (data, null, total);
            }

           
            catch (Exception ex)
            {
                mess.Errors = ex.Message;
                mess.Status = 500;
                return (null, mess, 0);
            }
        }

        public async Task<(bool, Mess)> SyncOUGPAsync(List<OUGP> models)
        {
            Mess mes = new Mess();
            try
            {
                foreach (var model in models)
                {
                    var ougp = await _context.OUGP.FirstOrDefaultAsync(p => p.SapId == model.SapId);
                    var ouom = await _context.OUOM.FirstOrDefaultAsync(p => p.SapId == model.BaseUom);
                    if (ougp != null)
                    {
                        
                        if (ougp.UgpCode != model.UgpCode || ougp.UgpName != model.UgpName || ougp.BaseUom != ouom.Id)
                        {
                            ougp.UgpCode = model.UgpCode;
                            ougp.UgpName = model.UgpName;
                            ougp.BaseUom = ouom.Id;
                            _context.OUGP.Update(ougp);
                            await _context.SaveChangesAsync();
                        }
                    }
                    ougp = await _context.OUGP.FirstOrDefaultAsync(p => p.SapId == null && (p.UgpName == model.UgpName || p.UgpCode == model.UgpCode));
                    if (ougp != null)
                    {
                        ougp.SapId = model.SapId;
                        ougp.UgpName = model.UgpName;
                        ougp.UgpCode = model.UgpCode;
                        ougp.BaseUom = ouom.Id;
                        _context.OUGP.Update(ougp);
                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                        ougp = await _context.OUGP.FirstOrDefaultAsync(p => p.SapId == model.SapId && p.UgpName == model.UgpName);
                        if (ougp == null)
                        {
                            OUGP u1 = new OUGP();
                            u1.UgpCode = model.UgpCode;
                            u1.UgpName = model.UgpName;
                            u1.SapId = model.SapId;
                            u1.BaseUom = ouom.Id;
                            u1.Status = true;
                            _context.OUGP.Add(u1);
                            await _context.SaveChangesAsync();
                        }
                        else
                        {
                            ougp.SapId = model.SapId;
                            ougp.UgpName = model.UgpName;
                            ougp.UgpCode = model.UgpCode;
                            ougp.BaseUom = ouom.Id;
                            _context.OUGP.Update(ougp);
                            await _context.SaveChangesAsync();
                        }
                    }
                }
                return (true, null);
            }
            catch (Exception ex)
            {
                mes.Status = 900;
                mes.Errors = ex.Message;
                return (false, mes);
            }
        }

        public async Task<(OUGP,Mess)> UpdateOUGPAsync(int id, OUGP model)
        {
            Mess mess = new Mess();
            try
            {
                var ougp = await _context.OUGP
               .AsNoTracking()
               .Include(p => p.UGP1)
               .FirstOrDefaultAsync(p => p.Id == id);
                if (ougp == null)
                {
                    mess.Status = 400;
                    mess.Errors = "Nhóm đơn vị tính không tồn tại";
                    return (null, mess);
                }
                if (id != model.Id)
                {
                    mess.Status = 400;
                    mess.Errors = "Nhóm đơn vị tính không không khớp";
                    return (null, mess);
                }
                var dtoType = model.GetType();
                var entityType = ougp.GetType();

                foreach (var prop in dtoType.GetProperties())
                {
                    var dtoValue = prop.GetValue(model);
                    if (dtoValue != null)
                    {
                        var entityProp = entityType.GetProperty(prop.Name);
                        if (!prop.Name.Equals("CreatedDate"))
                        {
                            if (entityProp != null)
                            {
                                entityProp.SetValue(ougp, dtoValue);
                            }
                        }
                    }
                }
                foreach (var ugp1s in model.UGP1.ToList())
                {
                    if (string.IsNullOrEmpty(ugp1s.Status))
                    { }
                    else if (ugp1s.Status.Equals("D"))
                    {
                        var ugp1 = ougp.UGP1.FirstOrDefault(i => i.Id == ugp1s.Id);
                        if (ugp1 != null)
                        {
                            _context.UGP1.Remove(ugp1);
                            ougp.UGP1.Remove(ugp1);
                        }
                    }
                    else if (ugp1s.Status.Equals("U"))
                    {
                        var ugp1 = ougp.UGP1.FirstOrDefault(i => i.Id == ugp1s.Id);
                        if (ugp1 != null)
                        {
                            ugp1.UomId = ugp1s.UomId;
                            ugp1.BaseQty = ugp1s.BaseQty;
                            ugp1.AltQty = ugp1s.AltQty;
                        }
                    }
                    else if (ugp1s.Status.Equals("A"))
                    {
                        ougp.UGP1.Add(new UGP1
                        {
                            UomId = ugp1s.UomId,
                            BaseQty = ugp1s.BaseQty,
                            AltQty = ugp1s.AltQty
                        });
                    }
                }
                _context.OUGP.Update(ougp);
                await _context.SaveChangesAsync();
                var (voucherDTO,mes) = await GetOUGPByIdAsync(ougp.Id);
                if(mes != null)
                {
                    return (null, mes);
                }    
                return (voucherDTO,null);
            }
            catch(Exception ex)
            {
                mess.Status = 900;
                mess.Errors = ex.Message;
                return (null, mess);
            }
        }
    }
}
