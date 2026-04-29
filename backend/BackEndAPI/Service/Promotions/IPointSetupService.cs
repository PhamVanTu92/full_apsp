using BackEndAPI.Models.Other;
using BackEndAPI.Models.Promotion;
using Gridify;

namespace BackEndAPI.Service.Promotions
{
    public interface IPointSetupService 
    {
        Task<(PointSetupViewData, Mess)> GetByIdAsync(int id);
        Task<(IEnumerable<PointSetupViewData>,int,Mess)> GetAllAsync(GridifyQuery q);
        Task<(PointSetupViewData, Mess)> CreateAsync(PointSetupCreateDto dto);
        Task<(PointSetupViewData, Mess)> UpdateAsync(PointSetupUpdateDto dto);
        // Legacy methods (giữ tương thích — internally forward sang OnDocumentStatusChangedAsync).
        Task CalculatePointsCircle(int DocId, int CardId, string Type, string? Status);
        Task CalculatePoints(CalculatorPoint p);
        Task<Mess> RedeemPoints(CalculatorPoint p, string Type);

        /// <summary>
        /// Gọi mỗi khi ODOC.Status thay đổi → service tự quyết định cộng/trừ/hoàn điểm
        /// dựa trên (objType, newStatus). Idempotent: gọi 2 lần với cùng status không double-count.
        ///
        /// Business rules:
        /// • ObjType=22 (Order):  DHT  → cộng điểm vào cycle
        ///                         HUY/DONG/HUY2 → hoàn điểm (reverse)
        /// • ObjType=12 (VPKM):   DXN  → trừ điểm (FIFO qua cycle còn hiệu lực)
        ///                         HUY/DONG/HUY2 → hoàn điểm (refund)
        /// </summary>
        Task OnDocumentStatusChangedAsync(int docId, int customerId, int objType, string newStatus, CancellationToken ct = default);
        Task<IEnumerable<CalculatorPointReturn>> CalculatePointCheck(CalculatorPoint p);
        Task<(IEnumerable<ReportPoint>, int ,Mess)> GetReportPoint(string? cardId, DateTime fromDate, DateTime toDate, int page = 1, int pageSize = 30);
    }
}
