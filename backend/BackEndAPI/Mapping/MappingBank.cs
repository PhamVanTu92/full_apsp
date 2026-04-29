using AutoMapper;
using BackEndAPI.Models.Banks;
using BackEndAPI.Models.BusinessPartners;

namespace BackEndAPI.Mapping
{
    public class MappingBank :Profile
    {
        public MappingBank()
        {
            CreateMap<Bank, BankView>()
            //.ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.BankCode, opt => opt.MapFrom(opt => opt.BankCode))
            .ForMember(dest => dest.BankName, opt => opt.MapFrom(opt => opt.BankName))
            .ForMember(dest => dest.GlobalName, opt => opt.MapFrom(opt => opt.GlobalName));
        }
    }
    public class MappingBankCard : Profile
    {
        public MappingBankCard()
        {
            CreateMap<BankCard, BankCardCreate>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.BankCode, opt => opt.MapFrom(opt => opt.BankCode))
            .ForMember(dest => dest.BankName, opt => opt.MapFrom(opt => opt.BankName))
            .ForMember(dest => dest.BankCardCode, opt => opt.MapFrom(opt => opt.BankCardCode))
            .ForMember(dest => dest.Cardholder, opt => opt.MapFrom(opt => opt.Cardholder))
            .ForMember(dest => dest.Remarks, opt => opt.MapFrom(opt => opt.Remarks));
        }
    }
}
