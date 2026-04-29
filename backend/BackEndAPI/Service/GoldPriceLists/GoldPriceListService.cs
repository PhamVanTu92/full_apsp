using BackEndAPI.Data;
using BackEndAPI.Models.ItemGroups;
using BackEndAPI.Models.Other;
using BackEndAPI.Models.Promotion;
using BackEndAPI.Service.ItemGroups;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace BackEndAPI.Service.GoldPriceLists
{
    public class GoldPriceListService : Service<GoldPriceList>, IGoldPriceListService
    {
        private readonly AppDbContext _context;
        private readonly IHostingEnvironment _webHostEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public GoldPriceListService(AppDbContext context, IHostingEnvironment webHostEnvironment, IHttpContextAccessor httpContextAccessor) : base(context)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<(IEnumerable<GoldPriceList>, Mess)> GetAllGoldPriceListAsync()
        {
            Mess mess = new Mess();
            try
            {
                var query = _context.Set<GoldPriceList>().AsQueryable();
                var items = await query.Include(p => p.GoldPriceListLine)
                        .ThenInclude(p => p.GoldBrand)
                     .Include(p => p.GoldPriceListLine)
                        .ThenInclude(p => p.GoldType)
                     .Include(p => p.GoldPriceListLine)
                        .ThenInclude(p => p.GoldContent)
                    .ToListAsync();
                return (items, null);
            }
            catch (Exception ex)
            {
                mess.Status = 900;
                mess.Errors = ex.Message;
                return (null, mess);
            }
        }

        public async Task<(IEnumerable<GoldPriceList>, int total, Mess)> GetGoldPriceListAsync(int skip, int limit)
        {
            Mess mess = new Mess();
            try
            {
                var query = _context.Set<GoldPriceList>().AsQueryable();
                var totalCount = await query.CountAsync();
                var items = await query.Skip(skip * limit).Take(limit)
                     .Include(p => p.GoldPriceListLine)
                        .ThenInclude(p => p.GoldBrand)
                     .Include(p => p.GoldPriceListLine)
                        .ThenInclude(p => p.GoldType)
                     .Include(p => p.GoldPriceListLine)
                        .ThenInclude(p => p.GoldContent)
                    .ToListAsync();
                return (items, totalCount, null);
            }
            catch (Exception ex)
            {
                mess.Status = 900;
                mess.Errors = ex.Message;
                return (null, 0, mess);
            }
        }

        public async Task<(IEnumerable<GoldPriceList>, Mess)> GetGoldPriceListAsync( string search)
        {
            Mess mess = new Mess();
            try
            {
                var query = _context.Set<GoldPriceList>().AsQueryable();
                var totalCount = await query.CountAsync();
                var items = await query.Include(p => p.GoldPriceListLine)
                        .ThenInclude(p => p.GoldBrand)
                     .Include(p => p.GoldPriceListLine)
                        .ThenInclude(p => p.GoldType)
                     .Include(p => p.GoldPriceListLine)
                        .ThenInclude(p => p.GoldContent)
                        .Where(p=>p.PriceListName.Contains(search))
                    .ToListAsync();
                return (items, null);
            }
            catch (Exception ex)
            {
                mess.Status = 900;
                mess.Errors = ex.Message;
                return (null, mess);
            }
        }

        public async Task<(GoldPriceList, Mess)> GetGoldPriceListByIdAsync(int id)
        {
            Mess mess = new Mess();
            try
            {
                var query = _context.Set<GoldPriceList>().AsQueryable();
                var items = await query.Include(p => p.GoldPriceListLine)
                        .ThenInclude(p => p.GoldBrand)
                     .Include(p => p.GoldPriceListLine)
                        .ThenInclude(p => p.GoldType)
                     .Include(p => p.GoldPriceListLine)
                        .ThenInclude(p => p.GoldContent)
                    .FirstOrDefaultAsync(p=>p.Id == id);
                return (items, null);
            }
            catch (Exception ex)
            {
                mess.Status = 900;
                mess.Errors = ex.Message;
                return (null, mess);
            }
        }
    }
}
