using BackEndAPI.Models.ItemMasterData;
using BackEndAPI.Models.Other;
using BackEndAPI.Models.Promotion;

namespace BackEndAPI.Service.Promotions
{
    public interface ICouponService: IService<Coupon>
    {
        Task<Coupon> GetCouponByIdAsync(int id);
        Task<(IEnumerable<Coupon>, int total)> GetCouponAsync(int skip, int limit);
        Task<Coupon> UpdateCouponAsync(int id, Coupon model);
        Task<Coupon> AddCouponAsync(Coupon model);
    }
    public interface ICouponLineService : IService<CouponLine>
    {
        Task<(IEnumerable<CouponLine>, int total)> GetCouponLineAsync(int skip, int limit, int id, string Status, string couponCode);
        Task<IEnumerable<CouponLine>> CreateCouponLineAsync(CouponCodeRule model);
        Task<(IEnumerable<CouponLine>, Mess mess)> UpdateCouponLineAysnc(int id, string Type, List<CouponLineView> model);
    }
}
