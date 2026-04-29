using BackEndAPI.Models.BusinessPartners;
using BackEndAPI.Models.Other;
using Gridify;

namespace BackEndAPI.Service.BusinessPartners
{
    public interface IPaymentRuleService:IService<PaymentRule>
    {
        Task<(IEnumerable<PaymentRule>, Mess, int total)> GetAllRuleAsync(GridifyQuery q);
        Task<(PaymentRule, Mess)> UpdateRuleAsync(int id, PaymentRuleView model);
    }
}
