using BackEndAPI.Models.ItemGroups;
using BackEndAPI.Models.Other;
using BackEndAPI.Models.Promotion;

namespace BackEndAPI.Service.GoldPriceLists
{
    public interface IGoldPriceListService : IService<GoldPriceList>
    {
        Task<(IEnumerable<GoldPriceList>, int total, Mess)> GetGoldPriceListAsync(int skip, int limit);

        Task<(GoldPriceList, Mess)> GetGoldPriceListByIdAsync(int id);

        Task<(IEnumerable<GoldPriceList>, Mess)> GetGoldPriceListAsync(string search);
        Task<(IEnumerable<GoldPriceList>, Mess)> GetAllGoldPriceListAsync();
    }
}
