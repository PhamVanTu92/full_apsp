using BackEndAPI.Models.Promotion;
using Microsoft.EntityFrameworkCore;

namespace BackEndAPI.Service.Promotions
{
    public static class PromotionExtensions
    {
        public static IQueryable<Promotion> WithFullDetails(this IQueryable<Promotion> query)
        {
            return query
                .AsSplitQuery()
                .Include(p => p.PromotionCustomer)
                .Include(p => p.PromotionBrand)
                .Include(p => p.PromotionIndustry)
                .Include(p => p.PromotionLine)
                    .ThenInclude(p => p.PromotionLineSub)
                        .ThenInclude(p => p.PromotionItemBuy)
                .Include(p => p.PromotionLine)
                    .ThenInclude(p => p.PromotionLineSub)
                        .ThenInclude(p => p.PromotionUnit)
                .Include(p => p.PromotionLine)
                    .ThenInclude(p => p.PromotionLineSub)
                        .ThenInclude(p => p.PromotionLineSubSub)
                            .ThenInclude(p => p.PromotionSubItemAdd)
                .Include(p => p.PromotionLine)
                    .ThenInclude(p => p.PromotionLineSub)
                        .ThenInclude(p => p.PromotionLineSubSub)
                            .ThenInclude(p => p.PromotionSubUnit);
        }
        public static async Task<Promotion?> GetFullDetailsByIdAsync(this IQueryable<Promotion> query, int id)
        {
            var data = await query
                .AsNoTracking()
                .Where(p => p.Id == id)
                .Select(p => new
                {
                    Entity = p,
                    PromotionCustomer = p.PromotionCustomer!.ToList(),
                    PromotionBrand = p.PromotionBrand!.ToList(),
                    PromotionIndustry = p.PromotionIndustry!.ToList(),
                    Lines = p.PromotionLine.Select(l => new
                    {
                        LineEntity = l,
                        Subs = l.PromotionLineSub.Select(s => new
                        {
                            SubEntity = s,
                            ItemBuys = s.PromotionItemBuy!.ToList(),
                            Units = s.PromotionUnit!.ToList(),
                            SubSubs = s.PromotionLineSubSub!.Select(ss => new
                            {
                                SubSubEntity = ss,
                                SubItemAdds = ss.PromotionSubItemAdd.ToList(),
                                SubUnits = ss.PromotionSubUnit.ToList()
                            }).ToList()
                        }).ToList()
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (data == null) return null;

            var promotion = data.Entity;
            promotion.PromotionCustomer = data.PromotionCustomer;
            promotion.PromotionBrand = data.PromotionBrand;
            promotion.PromotionIndustry = data.PromotionIndustry;

            promotion.PromotionLine = data.Lines.Select(l =>
            {
                var line = l.LineEntity;
                line.PromotionLineSub = l.Subs.Select(s =>
                {
                    var sub = s.SubEntity;
                    sub.PromotionItemBuy = s.ItemBuys;
                    sub.PromotionUnit = s.Units;
                    sub.PromotionLineSubSub = s.SubSubs.Select(ss =>
                    {
                        var subsub = ss.SubSubEntity;
                        subsub.PromotionSubItemAdd = ss.SubItemAdds;
                        subsub.PromotionSubUnit = ss.SubUnits;
                        return subsub;
                    }).ToList();
                    return sub;
                }).ToList();
                return line;
            }).ToList();

            return promotion;
        }
    }
}
