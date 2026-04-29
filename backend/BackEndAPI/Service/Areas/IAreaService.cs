using BackEndAPI.Models.ARInvoice;
using BackEndAPI.Models.Other;
using Function.Address;

namespace BackEndAPI.Service.Areas
{
    public interface IAreaService:IService<AreaService>
    {
        Task<(IEnumerable<Location>, Mess)> GetFLocation(string search);
        Task<(IEnumerable<Location>, Mess)> GetFArea(int id, string search);
        Task<(IEnumerable<Location>, Mess)> GetNewArea(string search);
        Task<(IEnumerable<Location>, Mess)> GetALocation(string search);
        Task<(IEnumerable<Location>, Mess)> GetAArea(int id, string search);
        Task<(IEnumerable<Location>, Mess)> GetNewArea(int id, string search);
    }
}
