using BackEndAPI.Data;
using BackEndAPI.Models.BusinessPartners;
using BackEndAPI.Models.Other;
using Gridify;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackEndAPI.Service.BusinessPartners
{
    public class PaymentRuleService : Service<PaymentRule>, IPaymentRuleService
    {
        private readonly AppDbContext _context;

        public PaymentRuleService(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<(IEnumerable<PaymentRule>, Mess, int total)> GetAllRuleAsync(GridifyQuery q)
        {
            var query = _context.PaymentRule.AsQueryable().ApplyFiltering(q);

            var total = await query.CountAsync();

            var itemGroup = await _context.PaymentRule
                .ApplyPaging(q.Page, q.PageSize)
                .ToListAsync();

            return (itemGroup, null, total);
        }

        public async Task<(PaymentRule, Mess)> UpdateRuleAsync(int id, [FromBody] PaymentRuleView model)
        {
            Mess mess = new Mess();
            try
            {
                var query = _context.PaymentRule.FirstOrDefault(x => x.Id == id);
                if (query == null)
                {
                    mess.Errors = "BP with id not found";
                    mess.Status = 404;

                    return (null, mess);
                }
                else
                {
                    query.BonusVolumn  = model.BonusVolumn;
                    query.BonusPaynow  = model.BonusPaynow;
                    query.PromotionTax = model.PromotionTax;
                    _context.PaymentRule.Update(query);
                    await _context.SaveChangesAsync();
                    return (query, null);
                }
            }
            catch
            {
                mess.Errors = "Đã có lỗi xảy ra";
                mess.Status = 900;

                return (null, mess);
            }
        }
    }
}