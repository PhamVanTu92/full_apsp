using BackEndAPI.Models.BusinessPartners;
using BackEndAPI.Models.ItemGroups;
using BackEndAPI.Models.Other;
namespace BackEndAPI.Service.Brands
{
    public interface IBrandService : IService<Brand>
    {
        Task<(bool, Mess)> SyncBrandAsync(List<Brand> brand);
    }
}
