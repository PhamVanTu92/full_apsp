using BackEndAPI.Models.ItemGroups;
using BackEndAPI.Models.Other;

namespace BackEndAPI.Service.Brands
{
    public interface IIndustryService:IService<Industry>
    {
        Task<(bool, Mess)> SyncIndustryAsync(List<Industry> industry);
        Task<(List<Industry>?, Mess?)> GetIndustrysAsync();
        Task<(List<IndustryDTO>?, Mess?)> GetIndustrysNewAsync(int? CardId);
        Task<(List<IndustryView>?, Mess?)> GetIndustryViewAsync();
    }
}
