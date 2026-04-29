using BackEndAPI.Data;
using Gridify;
using Microsoft.EntityFrameworkCore;

namespace BackEndAPI.Service.GenericeService;

public class GenericService<T> : IGenericService<T> where T : class
{
    private readonly AppDbContext _context;
    private readonly DbSet<T> _dbSet;

    public GenericService(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<(IEnumerable<T> Items, int TotalCount)> GetListAsync(GridifyQuery gridifyQuery)
    {
        // Áp dụng filter trên IQueryable (chưa execute)
        var filtered = _dbSet.AsNoTracking().ApplyFiltering(gridifyQuery);

        // Đếm theo bộ lọc — không phải toàn bảng
        var totalCount = await filtered.CountAsync();

        // Sort + skip + take ngay trên DB
        var items = await filtered
            .ApplyOrdering(gridifyQuery)
            .ApplyPaging(gridifyQuery)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<T?> GetByIdAsync(int id)
    {
        return await _dbSet.FindAsync(id);
    }

    public async Task<T> CreateAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<T> UpdateAsync(T entity)
    {
        _dbSet.Update(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _dbSet.FindAsync(id);
        if (entity == null) return false;

        _dbSet.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }
}