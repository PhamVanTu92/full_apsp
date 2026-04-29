using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackEndAPI.Data;
using BackEndAPI.Dtos;
using BackEndAPI.Models.Approval_V2;
using BackEndAPI.Models.Other;
using Gridify;
using Microsoft.EntityFrameworkCore;

namespace BackEndAPI.Service.Approval_V2.ApprovalLevel;

public class ApprovalLevelService : IApprovalLevelService
{
    private readonly AppDbContext _context;

    public ApprovalLevelService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<(List<Models.Approval_V2.ApprovalLevel>, int)> GetAllAsync(GridifyQuery paramQuery,
        string? search = null)
    {
        var query = _context.ApprovalLevels.AsNoTracking().ApplyFiltering(paramQuery).AsQueryable();

        if (search is not null) query = query.Where(x => search.Contains(x.ApprovalLevelName));

        var count = await query.CountAsync();

        var result = await query
            .Include(x => x.ApprovalLevelLines)
            .ApplyOrdering(paramQuery)
            .ApplyFiltering(paramQuery)
            .ToListAsync();

        return (result, count);
    }

    public async Task<(Models.Approval_V2.ApprovalLevel?, Mess?)> GetByIdAsync(int id)
    {
        var result = await _context.ApprovalLevels.Include(x => x.ApprovalLevelLines)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (result is null)
        {
            return (null, new Mess
            {
                Status = 404,
                Errors = "Không tìm thấy cấp phê duyệt"
            });
        }

        return (result, null);
    }

    public async Task<Mess?> DeleteAsync(int id)
    {
        var result = await _context.ApprovalLevels.FirstOrDefaultAsync(x => x.Id == id);
        if (result is null)
            return new Mess
            {
                Status = 400,
                Errors = "Không tìm thấy cấp phê duyệt"
            };
        _context.ApprovalLevels.Remove(result);
        await _context.SaveChangesAsync();
        return null;
    }

    public async Task<(Models.Approval_V2.ApprovalLevel?, Mess?)> UpdateAsync(int id, CreateApprovalLevel dto)
    {
        var result = await EntityFrameworkQueryableExtensions.Include(_context.ApprovalLevels
                                                                          .Include(x => x.ApprovalLevelLines),
                                                                      approvalLevel => approvalLevel.ApprovalLevelLines)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (result is null)
            return (null, new Mess()
            {
                Status = 404,
                Errors = "không tìm thấy cấp phê duyệt"
            });


        result.ApprovalLevelName = dto.ApprovalLevelName;
        result.Description       = dto.Description;
        result.ApprovalNumber    = dto.ApprovalNumber;
        result.DeclineNumber     = dto.DeclineNumber;
        result.IsActive          = dto.IsActive;


        var toRemove = dto.ApprovalLevelLines;
        result.ApprovalLevelLines.RemoveAll(x => !toRemove.Contains(x.Id));


        foreach (var item in dto.ApprovalLevelLines)
        {
            var existing = result.ApprovalLevelLines.FirstOrDefault(x => x.Id == item);
            if (existing is null)
                result.ApprovalLevelLines.Add(new ApprovalLevelLine()
                {
                    ApprovalUserId = item,
                });
        }

        await _context.SaveChangesAsync();

        return (result, null);
    }

    public async Task<(Models.Approval_V2.ApprovalLevel?, Mess?)> CreateAsync(CreateApprovalLevel dto)
    {
        var existName =
            await _context.ApprovalLevels.FirstOrDefaultAsync(x => x.ApprovalLevelName == dto.ApprovalLevelName);
        if (existName is not null)
            return (null, new Mess
            {
                Status = 400,
                Errors = "Cấp phê duyệt đã tồn tại"
            });

        var newItem = new Models.Approval_V2.ApprovalLevel
        {
            ApprovalLevelName = dto.ApprovalLevelName,
            Description       = dto.Description,
            ApprovalNumber    = dto.ApprovalNumber,
            DeclineNumber     = dto.DeclineNumber,
            IsActive          = dto.IsActive,
            ApprovalLevelLines = dto.ApprovalLevelLines.Select(x => new ApprovalLevelLine
            {
                ApprovalUserId = x,
            }).ToList()
        };

        _context.ApprovalLevels.Add(newItem);
        await _context.SaveChangesAsync();
        return (newItem, null);
    }
}