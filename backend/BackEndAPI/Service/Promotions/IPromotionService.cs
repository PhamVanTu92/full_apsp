using BackEndAPI.Models.Document;
using BackEndAPI.Models.Other;
using BackEndAPI.Models.Promotion;
using BackEndAPI.Models.Unit;
using Gridify;

namespace BackEndAPI.Service.Promotions
{
    public interface IPromotionService:IService<Promotion>
    {
        Task<(Promotion,Mess)> GetPromotionByIdAsync(int id);
        Task<(IEnumerable<Promotion>, int total,Mess)> GetPromotionAsync(int skip, int limit, string? search, GridifyQuery q, int userId);
        Task<(IEnumerable<Promotion>, int total, Mess)> GetSearchPromotionAsync(int skip, int limit,string search);
        Task<(Promotion, Mess)> UpdatePromotionAsync(int id, Promotion model);
        Task<(Promotion, Mess)> AddPromotionAsync(Promotion model);
        Task<(PromotionOrder, Mess)> GetPromotionAsync(PromotionParam promotionParam);
        Task<PaymentInfo> GetPaymentInfo(PriceDocCheck model);
    }

}
