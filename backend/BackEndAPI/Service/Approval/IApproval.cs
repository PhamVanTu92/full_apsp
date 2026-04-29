namespace BackEndAPI.Service.Approval;

public interface IApproval
{
    Task<(ICollection<Models.Approval.Approval>?, Models.Other.Mess?)> GetApprovals(int pageNumber, int pageSize);
}