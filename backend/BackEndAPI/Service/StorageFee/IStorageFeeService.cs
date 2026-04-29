using BackEndAPI.Models.Other;
using BackEndAPI.Models.StorageFee;

namespace BackEndAPI.Service.StorageFee
{
    public interface IStorageFeeService
    {
        Task<Mess?> AddLinesToStorageFeeAsync(int id, List<StorageFeeLineDto> list);
        Task<(List<FeeMilestone>?, Mess?)> CreateFeeMilestoneAsync(List<FeeMilestoneDto> feeDto);
        Task<(Models.StorageFee.StorageFee?, Mess?)> CreateStorageFeeAsync(StorageFeeDto storageFeeDto);
        Task<Mess?> DeleteStorageFeeAsync(int id);
        Task<(FeeMilestone?, Mess?)> GetFeeMilestoneByIdAsync(int id);
        Task<(IEnumerable<FeeMilestone>?, Mess?, int total)> GetFeeMilestonesAsync(int skip, int limit);
        Task<(IEnumerable<Models.StorageFee.StorageFee>?, Mess?, int total)> GetStorageFeeAync(int skip, int limit);
        Task<(Models.StorageFee.StorageFee?, Mess?)> GetStorageFeeByIdAync(int id);
        Task<Mess?> RemoveLinesFromStorageFee(List<int> lineIds);
        Task<Mess?> UpdateFeeMilestoneAsync(FeeMilestoneDto feeDto);
        Task<Mess?> UpdateStorageFeeAsync(Models.StorageFee.StorageFeeDto storageFee);
        Task<Mess?> DeleteFeeMilestoneAsync(int id);
    }
}