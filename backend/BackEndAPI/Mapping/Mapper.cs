using AutoMapper;
using BackEndAPI.Models;
using BackEndAPI.Models.Document;
using BackEndAPI.Models.ItemGroups;

namespace BackEndAPI.Mapping
{
    public class Mapper : Profile
    {
        public Mapper()
        {
            CreateMap<Models.StorageFee.StorageFeeLineDto, Models.StorageFee.StorageFeeLine>();
            CreateMap<Models.StorageFee.StorageFeeDto, Models.StorageFee.StorageFee>();
            CreateMap<Models.StorageFee.FeeMilestoneDto, Models.StorageFee.FeeMilestone>();

            CreateMap<Models.Committed.Committed, Models.Committed.Committed>()
                .ForMember(c => c.CommittedLine, opt => opt.Ignore());
            
            CreateMap<Models.Committed.CommittedLine, Models.Committed.CommittedLine>()
                .ForMember(c => c.CommittedLineSub, opt => opt.Ignore());

            CreateMap<Models.Committed.CommittedLineSub, Models.Committed.CommittedLineSub>()
                .ForMember(d => d.Brand, opt => opt.Ignore())
                .ForMember(d => d.ItemTypes, opt => opt.Ignore())
                .ForMember(dest => dest.CommittedLineSubSub, opt => opt.Ignore());
            CreateMap<Models.Committed.CommittedLineSubSub, Models.Committed.CommittedLineSubSub>();

            CreateMap<Models.SaleForecastModel.SaleForecast, Models.SaleForecastModel.SaleForecast>()
                .ForMember(dest => dest.SaleForecastItems, opt => opt.Ignore())
                .ForMember(p => p.Id, opt => opt.Ignore())
                .ForAllMembers(opt =>
                    opt.Condition((src, dest, srcMember) =>
                        srcMember != null && !(srcMember is string str && string.IsNullOrWhiteSpace(str))));

            CreateMap<Models.SaleForecastModel.SaleForecastItem, Models.SaleForecastModel.SaleForecastItem>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(p => p.Periods, opt => opt.Ignore())
                .ForMember(p => p.SaleForecastId, opt => opt.Ignore())
                .ForAllMembers(opt =>
                    opt.Condition((src, dest, srcMember) =>
                        srcMember != null && !(srcMember is string str && string.IsNullOrWhiteSpace(str))));

            CreateMap<Models.SaleForecastModel.SaleForecastItemPeriod,
                    Models.SaleForecastModel.SaleForecastItemPeriod>()
                .ForMember(dest => dest.SaleForecastItem, opt => opt.Ignore())
                .ForMember(p => p.Id, opt => opt.Ignore())
                .ForMember(p => p.SaleForecastItemId, opt => opt.Ignore())
                .ForAllMembers(opt =>
                    opt.Condition((src, dest, srcMember) =>
                        srcMember != null && !(srcMember is string str && string.IsNullOrWhiteSpace(str))));

            CreateMap<Models.BusinessPartners.BpClassify, Models.BusinessPartners.BpClassify>()
                .ForMember(d => d.Industry, opt => opt.Ignore())
                .ForMember(d => d.Brands, opt => opt.Ignore())
                .ForMember(d => d.Region, opt => opt.Ignore())
                .ForMember(d => d.Sizes, opt => opt.Ignore())
                .ForMember(d => d.BpSizeIds, opt => opt.Ignore())
                .ForMember(d => d.BrandIds, opt => opt.Ignore());

            CreateMap<Models.BPGroups.ConditionCusGroup, Models.BPGroups.ConditionCusGroup>()
                .ForMember(d => d.Values, opt => opt.Ignore())
                .ForMember(d => d.CondValues, opt => opt.Ignore());

            CreateMap<ODOC, ODOC>()
                .ForAllMembers(opts =>
                {
                    opts.Condition((src, dest, srcMember, context) =>
                    {
                        if (srcMember == null) return false; // Bỏ qua giá trị null

                        var defaultValue = srcMember.GetType().IsValueType
                            ? Activator.CreateInstance(srcMember.GetType())
                            : null; // Giá trị mặc định của kiểu dữ liệu

                        return !srcMember.Equals(defaultValue); // Bỏ qua giá trị mặc định
                    });
                });
            CreateMap<OrganizationUnitDto, OrganizationUnit>();
            CreateMap<OIBT, OIBT>();
            CreateMap<ConditionItemGroup, ConditionItemGroup>();
            CreateMap<ConditionItemGroupValue, ConditionItemGroupValue>();
        }
    }
}