using BackEndAPI.Models.BusinessPartners;
using BackEndAPI.Models.Other;

namespace BackEndAPI.Service.BusinessPartners.Address;

/// <summary>
/// Quản lý địa chỉ giao hàng/xuất hoá đơn của Business Partner (CRD1).
/// Tách từ BusinessPartnerService như POC theo docs/REFACTOR_PLAN.md.
/// </summary>
public interface IBPAddressService
{
    Task<(BP?, Mess?)> AddAsync(int bpId, CRD1 address);
    Task<(BP?, Mess?)> UpdateAsync(int bpId, CRD1 address);
    Task<(BP?, Mess?)> RemoveAsync(int bpId, List<int> addressIds);
}
