using BackEndAPI.Data;
using BackEndAPI.Models.Other;
using BackEndAPI.Models.Promotion;
using Microsoft.EntityFrameworkCore;
using System.Text;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace BackEndAPI.Service.Promotions
{
    public class VoucherService : Service<Voucher>, IVoucherService
    {
        private readonly AppDbContext _context;
        private readonly IHostingEnvironment _webHostEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public VoucherService(AppDbContext context, IHostingEnvironment webHostEnvironment, IHttpContextAccessor httpContextAccessor) : base(context)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<Voucher> AddVoucherAsync(Voucher model)
        {
            if (string.IsNullOrEmpty(model.VoucherCode))
            {
                var codes = await GenerateByCode("PHVC", "", 10, model);
                model.VoucherCode = codes;
            }
            model.VoucherLine = null;
            _context.Voucher.Add(model);
            await _context.SaveChangesAsync();
            return model;
        }

        public async Task<(IEnumerable<Voucher>, int total)> GetVoucherAsync(int skip, int limit)
        {
            var query = _context.Set<Voucher>().AsQueryable();
            var totalCount = await query.CountAsync();
            var items = await query.Skip(skip * limit).Take(limit)
                .Include(p => p.VoucherLine)
                .Include(p => p.VoucherItem)
                .Include(p => p.VoucherCustomer)
                .Include(p => p.VoucherSystem)
                .Include(p => p.VoucherSeller)
                .ToListAsync();
            return (items, totalCount);
        }

        public async Task<Voucher> GetVoucherByIdAsync(int id)
        {
            return await _context.Voucher
                    .AsNoTracking()
                    .Include(p => p.VoucherLine)
                    .Include(p => p.VoucherItem)
                    .Include(p => p.VoucherCustomer)
                    .Include(p => p.VoucherSystem)
                    .Include(p => p.VoucherSeller)
                    .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Voucher> UpdateVoucherAsync(int id, Voucher model)
        {
            var voucher = await _context.Voucher
           .AsNoTracking()
           .Include(p => p.VoucherItem)
           .FirstOrDefaultAsync(p => p.Id == id);
            if (voucher == null)
            {
                return null;
            }
            if (id != model.Id)
                return null;
            var dtoType = model.GetType();
            var entityType = voucher.GetType();

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
                            entityProp.SetValue(voucher, dtoValue);
                        }
                    }
                }
            }
            foreach (var voucherItems in model.VoucherItem.ToList())
            {
                if (string.IsNullOrEmpty( voucherItems.Status))
                { }    
                else if (voucherItems.Status.Equals("D"))
                {
                    var voucherItem = voucher.VoucherItem.FirstOrDefault(i => i.Id == voucherItems.Id);
                    if (voucherItem != null)
                    {
                        _context.VoucherItem.Remove(voucherItem);
                        voucher.VoucherItem.Remove(voucherItem);
                    }
                }
                else if (voucherItems.Status.Equals("U"))
                {
                    var voucherItem = voucher.VoucherItem.FirstOrDefault(i => i.Id == voucherItems.Id);
                    if (voucherItem != null)
                    {
                        voucherItem.Type = voucherItems.Type;
                        voucherItem.ItemId = voucherItems.ItemId;
                        voucherItem.ItemCode = voucherItems.ItemCode;
                        voucherItem.ItemName = voucherItems.ItemName;
                        voucherItem.ItmsGrpName = voucherItems.ItmsGrpName;
                        voucherItem.ItemGroupId = voucherItems.ItemGroupId;
                    }
                }
                else if (voucherItems.Status.Equals("A"))
                {
                    voucher.VoucherItem.Add(new VoucherItem
                    {
                        Type = voucherItems.Type,
                        ItemId = voucherItems.ItemId,
                        ItemCode = voucherItems.ItemCode,
                        ItemName = voucherItems.ItemName,
                        ItmsGrpName = voucherItems.ItmsGrpName,
                        ItemGroupId = voucherItems.ItemGroupId
                    });
                }
            }
            _context.Voucher.Update(voucher);
            await _context.SaveChangesAsync();
            var voucherDTO = await GetVoucherByIdAsync(voucher.Id);
            return voucherDTO;
        }
    }
    public class VoucherLineService : Service<VoucherLine>, IVoucherLineService
    {
        private readonly AppDbContext _context;
        private readonly IHostingEnvironment _webHostEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private static readonly Random _random = new Random();
        public VoucherLineService(AppDbContext context, IHostingEnvironment webHostEnvironment, IHttpContextAccessor httpContextAccessor) : base(context)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<(IEnumerable<VoucherLine>, Mess)> CreateVoucherLineAsync(VoucherCodeRule model)
        {
            Mess mess = new Mess();
            var voucher = await _context.Voucher
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == model.VoucherId);
            if (voucher == null)
            {
                mess.Status = 800;
                mess.Errors = "Đợt phát hành voucher không tồn tại";
                return (null, mess);
            }
            for (int i = 0; i < model.Quantity; i++)
            {
                string voucherCode;
                do
                {
                    voucherCode = model.StartChar + GenerateRandomCode(model.Length - model.StartChar.Length - model.EndChar.Length) + model.EndChar;
                } while (await _context.VoucherLine.AnyAsync(c => c.VoucherCode == model.StartChar + voucherCode + model.EndChar));
                var VoucherLine = new VoucherLine
                {
                    VoucherId = model.VoucherId,
                    VoucherCode = voucherCode,
                    Status = "NU"
                };

                _context.VoucherLine.Add(VoucherLine);
            }

            await _context.SaveChangesAsync();
            var (vouchers, total) = await GetVoucherLineAsync(0, 30, model.VoucherId);
            return (vouchers, null);
        }
        private string GenerateRandomCode(int length)
        {
            var characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var stringBuilder = new StringBuilder(length);

            for (int i = 0; i < length; i++)
            {
                stringBuilder.Append(characters[_random.Next(characters.Length)]);
            }

            return stringBuilder.ToString();
        }
        public async Task<(IEnumerable<VoucherLine>, int total)> GetVoucherLineAsync(int skip = 0, int limit = 30, int id = 0, string Status = "", string voucherCode = "")
        {
            var query = _context.Set<VoucherLine>().AsQueryable();
            var items = await query.Where(p => p.VoucherId == id
                                && (Status == "" ? true : p.Status == Status)
                                && (voucherCode == "" ? true : p.VoucherCode.Contains(voucherCode))
                                )
                            .Skip(skip * limit).Take(limit).ToListAsync();
            var totalCount = await query.Where(p => p.VoucherId == id
                                && (Status == "" ? true : p.Status == Status)
                                && (voucherCode == "" ? true : p.VoucherCode.Contains(voucherCode))
                                ).CountAsync();
            return (items, totalCount);
        }

        public async Task<(IEnumerable<VoucherLine>, Mess)> UpdateVoucherLineAysnc(int id, string Status, VoucherListToRelease model)
        {
            Mess mess = new Mess();
            try
            {
                if (Status == "R")
                {
                    var voucher = await _context.VoucherLine
                        .AsNoTracking().FirstOrDefaultAsync(p => p.VoucherId == id);
                    if (voucher == null)
                    {
                        mess.Status = 800;
                        mess.Errors = "Voucher không tồn tại";
                        return (null, mess);
                    }
                    var voucherLineDTO = model.VoucherLine.Select(c => c.VoucherId).ToList();
                    var voucherLine = await _context.VoucherLine
                        .AsNoTracking()
                        .Where(c => voucherLineDTO.Contains(c.Id) && c.Status != "NU").ToListAsync();
                    var voucherLineCheck = await _context.VoucherLine
                        .AsNoTracking()
                        .Where(c => voucherLineDTO.Contains(c.Id)).ToListAsync();
                    if (voucherLine.Count > 0)
                    {
                        mess.Status = 800;
                        mess.Errors = "Voucher khác trạng thái chưa sử dụng.";
                        return (null, mess);
                    }
                    if (voucherLineDTO.Count != voucherLineCheck.Count)
                    {
                        mess.Status = 801;
                        mess.Errors = "Voucher không tồn tại trong cơ sở dữ liệu.";
                        return (null, mess);
                    }
                    foreach (var c in voucherLineCheck)
                    {
                        c.Status = "R";
                        c.ReleaseDate = DateTime.Now;
                    }
                    _context.VoucherLine.UpdateRange(voucherLineCheck);
                    await _context.SaveChangesAsync();
                    var (vouchers, total) = await GetVoucherLineAsync(0, 30, id);
                    return (vouchers, null);
                }
                else
                {
                    mess.Status = 801;
                    mess.Errors = "Không tồn tại trạng thái này.";
                    return (null, mess);
                }
            }catch(Exception ex)
            {
                mess.Status = 900;
                mess.Errors = ex.Message;
                return (null, mess);
            }
            
        }

        public async Task<(IEnumerable<VoucherLine>, Mess)> CreateAVoucherLineAsync(int id, List<VoucherLineAdd> model)
        {
            Mess mess = new Mess();
            try
            {
                var voucher = await _context.Voucher
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == id);
                if (voucher == null)
                {
                    mess.Status = 800;
                    mess.Errors = "Đợt phát hành voucher không tồn tại";
                    return (null, mess);
                }
                List<VoucherLine> l = new List<VoucherLine>(); 
                foreach(var vch in model)
                {

                    VoucherLine vou = new VoucherLine
                    {
                        VoucherId = vch.VoucherId,
                        VoucherCode = vch.VoucherCode,
                        Status = "NU"
                    };
                    l.Add(vou);
                    
                }
                _context.VoucherLine.AddRange(l);
                await _context.SaveChangesAsync();
                return (l, null);
            }
            catch (Exception ex)
            {
                if(ex.Message.Contains("Cannot insert duplicate key"))
                {
                    mess.Status = 800;
                    mess.Errors = "Mã voucher đã tồn tại";
                    return (null, mess);
                }    
                mess.Status = 900;
                mess.Errors = ex.Message;
                return (null, mess);
            }
        }

        public async Task<(IEnumerable<VoucherLine>, Mess mess)> CancelVoucherLineAysnc(int id, string Status, VoucherListToCancel model)
        {
            Mess mess = new Mess();
            try
            {
                if (Status == "C")
                {
                    var voucher = await _context.VoucherLine
                        .AsNoTracking().FirstOrDefaultAsync(p => p.VoucherId == id);
                    if (voucher == null)
                    {
                        mess.Status = 800;
                        mess.Errors = "Voucher không tồn tại";
                        return (null, mess);
                    }
                    var voucherLineDTO = model.VoucherLine.Select(c => c.VoucherId).ToList();
                    var voucherLine = await _context.VoucherLine
                        .AsNoTracking()
                        .Where(c => voucherLineDTO.Contains(c.Id) && (c.Status == "U" || c.Status == "NU")).ToListAsync();
                    var voucherLineCheck = await _context.VoucherLine
                       .AsNoTracking()
                       .Where(c => voucherLineDTO.Contains(c.Id)).ToListAsync();
                    if (voucherLine.Count > 0)
                    {
                        mess.Status = 800;
                        mess.Errors = "Voucher đã được sử dụng.";
                        return (null, mess);
                    }
                    if (voucherLineDTO.Count != voucherLineCheck.Count)
                    {
                        mess.Status = 801;
                        mess.Errors = "Voucher không tồn tại trong cơ sở dữ liệu.";
                        return (null, mess);
                    }
                    foreach (var c in voucherLineCheck)
                    {
                        c.Status = "C";
                    }
                    _context.VoucherLine.UpdateRange(voucherLineCheck);
                    await _context.SaveChangesAsync();
                    var (vouchers, total) = await GetVoucherLineAsync(0, 30, id);
                    return (vouchers, null);
                }
                else
                {
                    mess.Status = 801;
                    mess.Errors = "Không tồn tại trạng thái này.";
                    return (null, mess);
                }
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
