using System.Collections.Generic;
using System.Threading.Tasks;
using BackEndAPI.Dtos;
using BackEndAPI.Models.Other;
using Gridify;

namespace BackEndAPI.Service.Approval_V2.ApprovalLevel;

public interface IApprovalLevelService
{
    Task<(List<Models.Approval_V2.ApprovalLevel>, int)> GetAllAsync(GridifyQuery paramQuery, string? query);
    Task<(Models.Approval_V2.ApprovalLevel?, Mess?)>    GetByIdAsync(int id);
    Task<Mess?>                                         DeleteAsync(int id);
    Task<(Models.Approval_V2.ApprovalLevel?, Mess?)>    UpdateAsync(int id, CreateApprovalLevel dto);
    Task<(Models.Approval_V2.ApprovalLevel?, Mess?)>      CreateAsync(CreateApprovalLevel dto);
}