using BackEndAPI.Models.ItemGroups;
using BackEndAPI.Models.Other;
using BackEndAPI.Models.Promotion;

namespace BackEndAPI.Service.Promotions
{
    public interface IVoucherService : IService<Voucher>
    {
        Task<Voucher> GetVoucherByIdAsync(int id);
        Task<(IEnumerable<Voucher>, int total)> GetVoucherAsync(int skip, int limit);
        Task<Voucher> UpdateVoucherAsync(int id, Voucher model);
        Task<Voucher> AddVoucherAsync(Voucher model);
    }
    public interface IVoucherLineService : IService<VoucherLine>
    {
        Task<(IEnumerable<VoucherLine>, int total)> GetVoucherLineAsync(int skip, int limit, int id, string Status, string voucherCode);
        Task<(IEnumerable<VoucherLine>, Mess mess)> CreateVoucherLineAsync(VoucherCodeRule model);
        Task<(IEnumerable<VoucherLine>, Mess mess)> CreateAVoucherLineAsync(int id,List<VoucherLineAdd> model);
        Task<(IEnumerable<VoucherLine>, Mess mess)> UpdateVoucherLineAysnc(int id, string Type, VoucherListToRelease model);
        Task<(IEnumerable<VoucherLine>, Mess mess)> CancelVoucherLineAysnc(int id, string Type, VoucherListToCancel model);
    }
}
