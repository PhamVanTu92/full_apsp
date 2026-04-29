using BackEndAPI.Data;
using BackEndAPI.Models.Other;
using Gridify;
using Microsoft.EntityFrameworkCore;

namespace BackEndAPI.Service.LocationService;

public class Localtion(AppDbContext context) : ILocaltion
{
    private readonly AppDbContext _context = context;

    public async Task<(List<Models.LocationModels.Area>?, int, Mess?)> GetAreas(int skip, int limit, GridifyQuery q,
        string? search)
    {
        var mess = new Mess();
        try
        {
            var query = _context.Areas.AsNoTracking().AsQueryable().ApplyFiltering(q);
            if (!string.IsNullOrEmpty(search))
            {
                search = search.Trim();
                query = query.Where(a => a.Name.ToLower().Contains(search.ToLower()));
            }

            var total = await query.CountAsync();
            var areas = await query.Include(a => a.Regions).Skip(skip * limit).Take(limit).ToListAsync();

            return (areas, total, null);
        }
        catch (Exception ex)
        {
            mess.Errors = ex.Message;
            mess.Status = 500;
            return (null, 0, null);
        }
    }

    public async Task<(List<Models.LocationModels.Region>?, int, Mess?)> GetRegions(int skip, int limit, GridifyQuery q,
        string? search)
    {
        var mess = new Mess();
        try
        {
            var query = _context.Regions.AsNoTracking().AsQueryable().ApplyFiltering(q);
            if (!string.IsNullOrEmpty(search))
            {
                search = search.Trim();
                query = query.Where(a => a.Name.ToLower().Contains(search.ToLower()));
            }

            var total = await query.CountAsync();
            var regions = await query.Include(p => p.Areas).Skip(skip * limit).Take(limit).ToListAsync();

            return (regions, total, null);
        }
        catch (Exception ex)
        {
            mess.Errors = ex.Message;
            mess.Status = 500;
            return (null, 0, null);
        }
    }
}