using BackEndAPI.Models.Other;
using BackEndAPI.Models.Promotion;
using Gridify;

namespace BackEndAPI.Service.Promotions
{
    public interface IExchangePointService : IService<ExchangePoint>
    {
        Task<(ExchangePointReadDto, Mess)> GetExchangePointByIdAsync(int id);
        Task<(IEnumerable<ExchangePointReadDto>, int total, Mess)> GetExchangePointAsync(GridifyQuery q);
        Task<(ExchangePointReadDto, Mess)> UpdateExchangePointAsync(ExchangePointUpdateDto model);
        Task<(ExchangePointReadDto, Mess)> AddExchangePointAsync(ExchangePointCreateDto model);
    }
}
