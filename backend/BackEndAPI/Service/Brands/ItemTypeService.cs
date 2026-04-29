using BackEndAPI.Data;
using BackEndAPI.Models.ItemGroups;
using BackEndAPI.Models.Other;
using Microsoft.EntityFrameworkCore;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace BackEndAPI.Service.Brands
{
    public class ItemTypeService:Service<ItemType>,IItemTypeService
    {
        private readonly AppDbContext _context;
        private readonly IHostingEnvironment _webHostEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;
        Mess mes = new Mess();
        public ItemTypeService(AppDbContext context, IHostingEnvironment webHostEnvironment, IHttpContextAccessor httpContextAccessor) : base(context)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<(bool, Mess)> SyncItemTypeAsync(List<ItemType> models)
        {
            try
            {
                foreach (var model in models)
                {
                    var itemType = await _context.ItemType.FirstOrDefaultAsync(p => p.SapCode == model.SapCode);
                    if (itemType != null)
                    {

                        if (itemType.Code != model.Code || itemType.Name != model.Name)
                        {
                            itemType.Code = model.Code;
                            itemType.Name = model.Name;
                            _context.ItemType.Update(itemType);
                            await _context.SaveChangesAsync();
                        }
                    }
                    itemType = await _context.ItemType.FirstOrDefaultAsync(p => p.SapCode == null && (p.Code == model.Code || p.Name == model.Name));
                    if (itemType != null)
                    {
                        itemType.SapCode = model.SapCode;
                        itemType.Code = model.Code;
                        itemType.Name = model.Name;
                        _context.ItemType.Update(itemType);
                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                        itemType = await _context.ItemType.FirstOrDefaultAsync(p => p.SapCode == model.SapCode && p.Name == model.Name);
                        if (itemType == null)
                        {
                            ItemType u1 = new ItemType();
                            u1.Code = model.Code;
                            u1.Name = model.Name;
                            u1.SapCode = model.SapCode;
                            _context.ItemType.Add(u1);
                            await _context.SaveChangesAsync();
                        }
                        else
                        {
                            itemType.SapCode = model.SapCode;
                            itemType.Code = model.Code;
                            itemType.Name = model.Name;
                            _context.ItemType.Update(itemType);
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
