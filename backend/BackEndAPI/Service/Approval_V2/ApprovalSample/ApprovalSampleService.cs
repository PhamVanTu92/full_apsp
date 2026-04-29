using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackEndAPI.Data;
using BackEndAPI.Dtos;
using BackEndAPI.Models.Approval_V2;
using BackEndAPI.Models.Other;
using Gridify;
using Microsoft.EntityFrameworkCore;

namespace BackEndAPI.Service.Approval_V2.ApprovalSample;

public class ApprovalSampleService(AppDbContext context) : IApprovalSampleService
{
    public async Task<(List<Models.Approval_V2.ApprovalSample>, int)> GetAllAsync(GridifyQuery paramQuery,
        string? search)
    {
        var query                     = context.ApprovalSamples.AsNoTracking().ApplyFiltering(paramQuery).AsQueryable();
        if (search is not null) query = query.Where(x => search.Contains(x.ApprovalSampleName));

        var count = await query.CountAsync();
        var result = await query
            .Include(x => x.ApprovalSampleDocumentsLines)
            .Include(x => x.ApprovalSampleProcessesLines)
            .Include(x => x.ApprovalSampleMembersLines)
            .ThenInclude(x => x.Creator)
            .ApplyOrdering(paramQuery).ApplyPaging(paramQuery).ToListAsync();

        return (result, count);
    }

    public async Task<(bool isValid, string ErrorMessage)> ValidateDuplicatesConditions(
        Models.Approval_V2.ApprovalSample approvalSample)
    {
        var existingSamples = await context.ApprovalSamples.Where(x => x.IsActive == true).ToListAsync();
        foreach (var existing in existingSamples)
        {
            // if(existing.IsDebtLimit ) 
        }

        return (true, "");
    }


    public async Task<(Models.Approval_V2.ApprovalSample?, Mess?)> GetByIdAsync(int id)
    {
        var result = await context.ApprovalSamples
            .Include(x => x.ApprovalSampleDocumentsLines)
            .Include(x => x.ApprovalSampleOcrgLines)
            .Include(x => x.ApprovalSampleMembersLines)
            .ThenInclude(x => x.Creator)
            .ThenInclude(x => x!.Role)
            .Include(x => x.ApprovalSampleProcessesLines)
            .ThenInclude(x => x.ApprovalLevel)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (result is null)
            return (null, new Mess
            {
                Status = 400,
                Errors = "Không tìm thấy mẫu phê duyệt"
            });

        return (result, null);
    }

    public async Task<Mess?> DeleteAsync(int id)
    {
        var result = await context.ApprovalSamples
            .FirstOrDefaultAsync(x => x.Id == id);
        if (result is null)
            return new Mess
            {
                Status = 400,
                Errors = "Không tìm thấy mẫu phê duyệt"
            };
        return null;
    }

    public async Task<(Models.Approval_V2.ApprovalSample?, Mess?)> UpdateAsync(int id, CreateApprovalSample dto)
    {
        var result = await context.ApprovalSamples
            .Include(x => x.ApprovalSampleDocumentsLines)
            .Include(x => x.ApprovalSampleMembersLines)
            .Include(x => x.ApprovalSampleProcessesLines)
            .Include(approvalSample => approvalSample.ApprovalSampleOcrgLines)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (result is null)
            return (null, new Mess
            {
                Status = 400,
                Errors = "Không tìm thấy mẫu phê duyệt"
            });

        result.Description        = dto.Description;
        result.ApprovalSampleName = dto.ApprovalSampleName;
        result.IsActive           = dto.IsActive;
        result.IsOther            = dto.IsOther;
        result.IsDebtLimit        = dto.IsDebtLimit;
        result.IsOverdueDebt      = dto.IsOverdueDebt;

        var toRemoveDocument = dto.ApprovalSampleDocumentsLines;
        result.ApprovalSampleDocumentsLines.RemoveAll(x => !toRemoveDocument.Contains((DocumentEnum)x.Document!));

        var toRemoveMember = dto.ApprovalSampleMembersLines;
        result.ApprovalSampleMembersLines.RemoveAll(x => !toRemoveMember.Contains(x.CreatorId));

        var toRemoveProcess = dto.ApprovalSampleProcessesLines;
        result.ApprovalSampleProcessesLines.RemoveAll(x => !toRemoveProcess.Contains(x.ApprovalLevelId));

        var toRemoveOcrg = dto.ApprovalSampleOcrgLines;
        result.ApprovalSampleOcrgLines.RemoveAll(x => !toRemoveOcrg.Contains(x.BusinessPartnerGpId));

        foreach (var item in dto.ApprovalSampleOcrgLines)
        {
            var existing =
                result.ApprovalSampleOcrgLines.FirstOrDefault(x => x.BusinessPartnerGpId == item);
            if (existing is null)
                result.ApprovalSampleOcrgLines.Add(new ApprovalSampleOcrgLine
                {
                    BusinessPartnerGpId = item,
                });
        }

        foreach (var dtoApprovalSampleDocumentsLine in dto.ApprovalSampleDocumentsLines)
        {
            var existing =
                result.ApprovalSampleDocumentsLines.FirstOrDefault(x => x.Document == dtoApprovalSampleDocumentsLine);
            if (existing is null)
                result.ApprovalSampleDocumentsLines.Add(new ApprovalSampleDocumentsLine()
                {
                    Document = dtoApprovalSampleDocumentsLine
                });
        }

        foreach (var item in dto.ApprovalSampleMembersLines)
        {
            var existing =
                result.ApprovalSampleMembersLines.FirstOrDefault(x => x.CreatorId == item);
            if (existing is null)
                result.ApprovalSampleMembersLines.Add(new ApprovalSampleMembersLine()
                {
                    CreatorId = item
                });
        }

        foreach (var item in dto.ApprovalSampleProcessesLines)
        {
            var existing =
                result.ApprovalSampleProcessesLines.FirstOrDefault(x => x.ApprovalLevelId == item);
            if (existing is null)
                result.ApprovalSampleProcessesLines.Add(new ApprovalSampleProcessesLine()
                {
                    ApprovalLevelId = item
                });
        }

        await context.SaveChangesAsync();
        return (result, null);
    }

    public async Task<(Models.Approval_V2.ApprovalSample?, Mess?)> CreateAsync(CreateApprovalSample dto)
    {
        var isDuplicateName =
            await context.ApprovalSamples.FirstOrDefaultAsync(x => x.ApprovalSampleName == dto.ApprovalSampleName);
        if (isDuplicateName is not null)
        {
            return (null, new Mess
            {
                Status = 404,
                Errors = "Không tồn tại mẫu phê duyệt"
            });
        }

        var reuslt = new Models.Approval_V2.ApprovalSample
        {
            ApprovalSampleName = dto.ApprovalSampleName,
            Description        = dto.Description,
            IsActive           = dto.IsActive,
            IsDebtLimit        = dto.IsDebtLimit,
            IsOverdueDebt      = dto.IsOverdueDebt,
            IsOther            = dto.IsOther,
            ApprovalSampleMembersLines = dto.ApprovalSampleMembersLines.Select(x => new ApprovalSampleMembersLine
            {
                CreatorId = x,
            }).ToList(),
            ApprovalSampleDocumentsLines = dto.ApprovalSampleDocumentsLines.Select(x => new ApprovalSampleDocumentsLine
            {
                Document = x
            }).ToList(),
            ApprovalSampleProcessesLines = dto.ApprovalSampleProcessesLines.Select(x => new ApprovalSampleProcessesLine
            {
                ApprovalLevelId = x,
            }).ToList(),
            ApprovalSampleOcrgLines = dto.ApprovalSampleOcrgLines.Select(x => new ApprovalSampleOcrgLine
            {
                BusinessPartnerGpId = x,
            }).ToList()
        };

        context.ApprovalSamples.Add(reuslt);
        await context.SaveChangesAsync();
        return (reuslt, null);
    }
}