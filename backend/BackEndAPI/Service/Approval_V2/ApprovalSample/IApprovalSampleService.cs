using System.Collections.Generic;
using System.Threading.Tasks;
using BackEndAPI.Dtos;
using BackEndAPI.Models.Other;
using Gridify;

namespace BackEndAPI.Service.Approval_V2.ApprovalSample;

public interface IApprovalSampleService
{
    Task<(List<Models.Approval_V2.ApprovalSample>, int)> GetAllAsync(GridifyQuery paramQuery, string? query);

    Task<(bool isValid, string ErrorMessage)> ValidateDuplicatesConditions(
        Models.Approval_V2.ApprovalSample approvalSample);

    Task<(Models.Approval_V2.ApprovalSample?, Mess?)> GetByIdAsync(int id);
    Task<Mess?>                                       DeleteAsync(int id);
    Task<(Models.Approval_V2.ApprovalSample?, Mess?)> UpdateAsync(int id, CreateApprovalSample dto);
    Task<(Models.Approval_V2.ApprovalSample?, Mess?)> CreateAsync(CreateApprovalSample dto);
}