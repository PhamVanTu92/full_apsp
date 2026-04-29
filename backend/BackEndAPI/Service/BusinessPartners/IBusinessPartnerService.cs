using BackEndAPI.Models.BusinessPartners;
using BackEndAPI.Models.Other;
using Gridify;

namespace BackEndAPI.Service.BusinessPartners
{
    public interface IBusinessPartnerService : IService<BP>
    {
        Task<(IEnumerable<BP>?, Mess?, int total)> GetAllBPAsync(int skip, int limit, string search, GridifyQuery q,
            int? usrId);
        Task<(IEnumerable<BPGroup>?, Mess?, int total)> GetAllBPAsync(int skip, int limit,string search, GridifyQuery q);
        Task<List<CRD3>> GetCRD3Async(int id);
        Task<Models.Committed.Committed?> GetCurrentCommited(int id);
        Task<(BP, Mess)> GetBPByIdAsync(int id);
        Task<(IEnumerable<BP>, Mess)> GetBPAsync(string search, string cardType);
        Task<(BP?, Mess?)> CreateBPAsync(BusinessPartnerView model, string cardType);
        Task<(BP, Mess)> UpdateBPAsync(int id, BusinessPartnerView model, string cardType);
        Task<(bool, Mess)> DeleteBPAsync(int id);
        Task<(bool, Mess)> SyncBPAsync(List<BPView> bps);
        Task<(bool, Mess)> SyncBPAsync();
        Task<(BP?, Mess?)> AddClassify(int BpId, List<BpClassify> classify);
        Task<Mess?> UpdateClassify(int bpId, List<BpClassify> classifys);
        Task<(BP?, Mess?)> RemoveClassify(int bpId, List<int> classIds);
        Task<(BP?, Mess?)> AddAddress(int bpId, CRD1 address);
        Task<(List<CRD3>?, Mess?)> CUDCRD3(int BpId,List<CRD3Dto> address);
        Task<(BP?, Mess?)> UpdateAddress(int bpId, CRD1 address);
        Task<(BP?, Mess?)> RemoveAddress(int bpId, List<int> addressIds);
        Task<(BP?, Mess?)> AddFiles(int bpId, int userId, List<IFormFile> files, string[] notes);
        Task<(BP?, Mess?)> RemoveFile(int bpId, List<int> fileIds);
        Task<Mess?> UpdateFiles(int bpId, List<CRD6> files);
        Task<(BP?, Mess?)> ChangeSaleStaff(int bpId, int staffId);
        Task UpdateCrd3(int id, List<CRD3> model);
        Task<bool> SyncBPCRD4Async();
        Task<bool> SyncTTDHAsync();
        Task<bool> SyncTTDH1Async(string Invoice);
        Task<bool> SyncTTDHHAsync();
        Task<bool> SyncCancelYCHGsync();
        Task<bool> SyncIssueCancelAsync();
        // SyncTCRD4Async, SyncCardCodeCRD4Async: đã xoá — replaced bởi
        // BackEndAPI.Service.Sync.IBPSyncService.SyncCRD4DeltaAsync /
        // SyncCardBalanceCRD4DeltaAsync (delta sync qua B1SLayer).
    }
}