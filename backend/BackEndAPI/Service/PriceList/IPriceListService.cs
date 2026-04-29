using BackEndAPI.Models.Other;
using BackEndAPI.Models.PriceList;
using Gridify;

namespace BackEndAPI.Service.PriceLists
{
    public interface IPriceListService
    {
        Task<(IEnumerable<PriceListDTO>,Mess,int)> GetAllAsync(GridifyQuery gridifyQuery);
        Task<(PriceListDTO,Mess)> GetByIdAsync(int id);
        Task<(PriceListDTO, Mess)> CreateAsync(PriceList priceList);
        Task<(PriceListDTO, Mess)> UpdateAsync(int id, PriceList priceList);
        Task<(bool,Mess)> DeleteAsync(int id);
    }
}
