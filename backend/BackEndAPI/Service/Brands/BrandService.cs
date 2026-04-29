using BackEndAPI.Data;
using BackEndAPI.Models.ItemGroups;
using BackEndAPI.Models.Other;
using BackEndAPI.Models.Unit;
using Microsoft.EntityFrameworkCore;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace BackEndAPI.Service.Brands
{
    public class BrandService: Service<Brand>, IBrandService
    {
        private readonly AppDbContext _context;
        private readonly IHostingEnvironment _webHostEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;
        Mess mes = new Mess();
        public BrandService(AppDbContext context, IHostingEnvironment webHostEnvironment, IHttpContextAccessor httpContextAccessor) : base(context)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<(bool, Mess)> SyncBrandAsync(List<Brand> models)
        {
            try
            {
                foreach (var model in models)
                {
                    var brand = await _context.Brand.FirstOrDefaultAsync(p => p.SapCode == model.SapCode);
                    if (brand != null)
                    {

                        if (brand.Code != model.Code || brand.Name != model.Name)
                        {
                            brand.Code = model.Code;
                            brand.Name = model.Name;
                            _context.Brand.Update(brand);
                            await _context.SaveChangesAsync();
                        }
                    }
                    brand = await _context.Brand.FirstOrDefaultAsync(p => p.SapCode == null && (p.Code == model.Code || p.Name == model.Name));
                    if (brand != null)
                    {
                        brand.SapCode = model.SapCode;
                        brand.Code = model.Code;
                        brand.Name = model.Name;
                        _context.Brand.Update(brand);
                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                        brand = await _context.Brand.FirstOrDefaultAsync(p => p.SapCode == model.SapCode && p.Name == model.Name);
                        if (brand == null)
                        {
                            Brand u1 = new Brand();
                            u1.Code = model.Code;
                            u1.Name = model.Name;
                            u1.SapCode = model.SapCode;
                            _context.Brand.Add(u1);
                            await _context.SaveChangesAsync();
                        }
                        else
                        {
                            brand.SapCode = model.SapCode;
                            brand.Code = model.Code;
                            brand.Name = model.Name;
                            _context.Brand.Update(brand);
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
