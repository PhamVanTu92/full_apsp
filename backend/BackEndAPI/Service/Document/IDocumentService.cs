using BackEndAPI.Models.Approval;
using BackEndAPI.Models.Committed;
using BackEndAPI.Models.Document;
using BackEndAPI.Models.Other;
using BackEndAPI.Models.Promotion;
using Gridify;
using Microsoft.AspNetCore.JsonPatch;
using System.Threading.Tasks;
using BackEndAPI.Models;

namespace BackEndAPI.Service.Document
{
    public interface IDocumentService : IService<ODOC>
    {
        Task<(Redeem, Mess)> CreateAsync(RedeemCreateDto dto);
        Task<(ODOC, Mess)>   AddDocumentAsync(ODOC model, int ObjType);
        Task                 ConfirmPayment(int id);
        Task<(ODOC, Mess)>   AddDocumentORFSAsync(ORFS doc);
        Task<(ORFS, Mess)>   UpdateDocumentORFSAsync(int id, ORFS doc);

        Task<(bool, Mess)> AddApprovalFirst(int id, int ObjType);

        //Task<(ODOC, Mess)> AddPurchaseOrderAsync(ODOC model);
        Task<(ODOC, Mess)> UpdateDocumentAsync(int id, DOCUpdate model, int ObjType);
        Task<(ODOC, Mess)> GetDocumentByIdAsync(int id, int ObjType);
        Task               SendPaymentRequest(int id);

        Task<(IEnumerable<ODOC>, int total, Mess)> GetAllDocumentAsync(int skip, int limit, int ObjType, GridifyQuery q,
            string? search, int? userId);

        Task<(IEnumerable<ORFS>, int total, Mess)> GetAllDocumentORFSAsync(GridifyQuery q, string? search, int? userId);
        Task<(ORFS, Mess)> GetAllDocumentORFSAsync(int id);
        Task<(IEnumerable<ODOC>, int total, Mess)> GetAllDocumentAsync(int ObjType);
        Task<(IEnumerable<ODOC>, int total, Mess)> GetAllDocumentAsync(string Status, int ObjType);
        Task<(IEnumerable<ODOC>, Mess)> GetDocumentAsync(string search, int ObjType);
        Task<(bool, Mess)> DeleteAsync(int? id, int ObjType);
        Task<(PaymentInfo, Committed, CommittedTracking)> GetPaymentInfo(PriceDocCheck model);
        Task<(PaymentInfo, Committed, CommittedTracking)> GetPaymentInfo1(PriceDocCheck model);

        Task<(IEnumerable<ODOC>?, int total, Mess?)> GetAllDocumentByCardIdIdAsync(int skip, int limit, int userId,
            int objType, string? search, GridifyQuery q);

        Task<Mess?> UpdateStatus(int id, string status, List<IFormFile> files, int ObjType, string reason = "");
        Task<Mess?> UpdateIssueStatus(int id, string status);
        Task<Mess?> PathUpdate(int id, ODOC patchDoc);
        Task<Mess?> AddAttDocuments(int id, int userId, List<IFormFile> files);
        Task<Mess?> RemoveAttDocuments(int id, List<int> fileIDs);
        Task<Mess?> AddAttFile(int id, int userId, List<IFormFile> files);
        Task<Mess?> RemoveAttFile(int id, List<int> fileIDs);
        Task UpdateDocAddress(int docId, List<DOC12> address);
        Task<bool> SyncToSapAsync();
        Task<bool> SyncToSapDraftAsync();
        Task<bool> SyncVPKMToSapAsync();
        Task<bool> SyncIssueToSapAsync();
        Task RemoveDraft(int docId);
        Task<Mess?> SendZalo(int DocId, string TypeMess);
        Task<(Rating, Mess)> CreateOrderRatingAsync(CreateOrderRatingDto dto);
        Task<(Rating, Mess)> CreateGeneralRatingAsync(CreateGeneralRatingDto dto);
        Task<(IEnumerable<RatingDto>, Mess, int)> GetGeneralRatingAsync(GridifyQuery q);
        Task<(RatingDto, Mess)> GetGeneralRatingAsync(int Id);
        Task<(IEnumerable<SyncInvoiceErrorDto>, Mess, int)> SyncInvoiceError(GridifyQuery q);
        Task<Mess> SyncInvoiceError(int DocID, int ObjType);
        Task<(IEnumerable<OrderReturnDto>, Mess, int)> GetOrderReturnAsync(int? userId,GridifyQuery q);

        Task<(ODOC, Mess)> UpdateDocumentReturnAsync(int id, OrderReturn model, int ObjType);
    }
}