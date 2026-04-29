using BackEndAPI.Data;
using BackEndAPI.Models.BPGroups;
using BackEndAPI.Models.ItemGroups;
using BackEndAPI.Models.Other;
using BackEndAPI.Models.Unit;
using BackEndAPI.Service.Document;
using Microsoft.EntityFrameworkCore;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace BackEndAPI.Service.Unit
{
    public class OUOMService : Service<OUOM>, IOUOMService
    {
        private readonly AppDbContext _context;
        private readonly IHostingEnvironment _webHostEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public OUOMService(AppDbContext context, IHostingEnvironment webHostEnvironment, IHttpContextAccessor httpContextAccessor) : base(context)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<(bool, Mess)> DeleteOUOMAsync(int id)
        {
            Mess mess = new Mess();
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var item = await _context.OUGP
                        .AsNoTracking()
                        .FirstOrDefaultAsync(p => p.BaseUom == id);
                    if (item != null)
                    {
                        mess.Status = 400;
                        mess.Errors = "Đơn vị tính đã được sử dụng";
                        return (false, mess);
                    }
                    var items = await _context.UGP1
                        .AsNoTracking()
                        .FirstOrDefaultAsync(p => p.UomId == id);
                    if (item != null)
                    {
                        mess.Status = 400;
                        mess.Errors = "Đơn vị tính đã được sử dụng";
                        return (false, mess);
                    }
                    var ouom = await _context.OUOM
                        .AsNoTracking()
                        .FirstOrDefaultAsync(p => p.Id == id);
                    if (ouom == null)
                    {
                        mess.Status = 400;
                        mess.Errors = "Đơn vị tính không tồn tại";
                        return (false, mess);
                    }
                    _context.OUOM.Remove(ouom);
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

        public async Task<(bool, Mess)> SyncOUOMAsync(List<Packing> models)
        {
            Mess mes = new Mess();
            try
            {
                foreach (var model in models)
                {
                    var ouom = await _context.OUOM.FirstOrDefaultAsync(p => p.SapId == model.SapId);
                    if (ouom != null)
                    {
                        if (ouom.UomCode != model.Code || ouom.UomName != model.Name)
                        {
                            ouom.UomCode = model.Code;
                            ouom.UomName = model.Name;
                            _context.OUOM.Update(ouom);
                            await _context.SaveChangesAsync();
                        }
                    }
                    ouom = await _context.OUOM.FirstOrDefaultAsync(p => p.SapId == null && (p.UomName == model.Name || p.UomCode == model.Code));
                    if (ouom != null)
                    {
                        ouom.SapId = model.SapId;
                        ouom.UomName = model.Name;
                        ouom.UomCode = model.Code;
                        _context.OUOM.Update(ouom);
                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                        ouom = await _context.OUOM.FirstOrDefaultAsync(p => p.SapId == model.SapId && p.UomName == model.Name);
                        if (ouom == null)
                        {
                            OUOM u1 = new OUOM();
                            u1.UomCode = model.Code;
                            u1.UomName = model.Name;
                            u1.SapId = model.SapId;
                            u1.Status = true;
                            _context.OUOM.Add(u1);
                            await _context.SaveChangesAsync();
                        }
                        else
                        {
                            ouom.SapId = model.SapId;
                            ouom.UomName = model.Name;
                            ouom.UomCode = model.Code;
                            _context.OUOM.Update(ouom);
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
    }
}
