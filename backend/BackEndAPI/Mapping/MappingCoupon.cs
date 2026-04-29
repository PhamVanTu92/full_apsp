using AutoMapper;
using BackEndAPI.Models.BusinessPartners;
using BackEndAPI.Models.Promotion;
using Microsoft.EntityFrameworkCore.Design;

namespace BackEndAPI.Mapping
{
    public class MappingCoupon : Profile
    {
        public MappingCoupon()
        {
            CreateMap<Coupon, CouponDTO>()
                 .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.CouponLine.Where(e => e.Status != "C").Count()));
        }
    }
    public class MappingVoucher : Profile
    {
        public MappingVoucher()
        {
            CreateMap<Voucher, VoucherDTO>()
                 .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.VoucherLine.Where(e => e.Status != "C").Count()));
        }
    }
}
