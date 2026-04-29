using BackEndAPI.Data;
using BackEndAPI.Models.Promotion;
using BackEndAPI.Service.BPGroups;
using BackEndAPI.Service.Committeds;
using Microsoft.EntityFrameworkCore;

namespace BackEndAPI.Service.Promotions
{
    public class PromotionCalculatorService : Service<Promotion>
    {
        private readonly AppDbContext _context;
        public PromotionCalculatorService(AppDbContext context) : base(context)
        {
            _context = context;
        }
        public async Task<(List<int>, List<int>)> GetAllCustomerUnderManager(int managerUserId)
        {
            var result = new HashSet<int>();
            result.Add(managerUserId);
            var toProcess = new Queue<int>();
            toProcess.Enqueue(managerUserId);

            while (toProcess.Count > 0)
            {
                var currentManagerId = toProcess.Dequeue();

                var usr = await _context.AppUser.AsNoTracking().Where(u => u.Id == managerUserId).FirstOrDefaultAsync();

                if (usr == null)
                {
                    return ([], []);
                }

                var dep = await _context.OrganizationUnit
                    .AsNoTracking()
                    .Include(u => u.Employees)
                    .Where(u => u.ParentId == usr.OrganizationId)
                    .ToListAsync();

                // Lấy StaffUsers của manager hiện tại
                var staffIds = await _context.Users.AsNoTracking()
                    .Where(u => u.Id == currentManagerId)
                    .SelectMany(u => u.DirectStaff.Select(s => s.Id))
                    .ToListAsync();
                staffIds.AddRange(dep.SelectMany(u => u.Employees).Select(e => e.Id));

                foreach (var staffId in staffIds)
                {
                    if (result.Add(staffId)) // Nếu chưa có trong kết quả thì thêm và tiếp tục duyệt
                    {
                        toProcess.Enqueue(staffId);
                    }
                }
            }

            var ids = result.ToList();
            var cusIds = await _context.BP.AsNoTracking().Where(e => ids.Contains(e.SaleId ?? 0)).Select(e => new { Id = e.Id, Group = e.Groups }).ToListAsync();

            return (cusIds.Select(e => e.Id).ToList(), cusIds.SelectMany(e => e.Group.Select(x => x.Id)).ToList());
        }
        public bool checkPromotion(PromotionOrder promotionOrder)
        {
            bool flag = true;
            if (promotionOrder.PromotionOrderLine == null)
            {
                return false;
            }

            var PromotionOrderLine = promotionOrder.PromotionOrderLine
                .GroupBy(l => new
                {
                    l.Id,
                    l.FatherId,
                    l.LineId,
                    l.ItemId,
                    l.Quantity,
                    l.PromotionId,
                    l.PromotionCode,
                    l.IsOtherPromotion,
                    l.IsOtherDist,
                    l.IsOtherPay,
                    l.HasException,
                    l.IsOtherPromotionExc,
                    l.IsOtherDistExc,
                    l.IsOtherPayExc,
                    l.PromotionName,
                    l.PromotionDesc
                })
                .Select(promotionOrderLine => new PromotionOrderLine
                {
                    Id = promotionOrderLine.Key.Id,
                    FatherId = promotionOrderLine.Key.FatherId,
                    LineId = promotionOrderLine.Key.LineId,
                    ItemId = promotionOrderLine.Key.ItemId,
                    Quantity = promotionOrderLine.Key.Quantity,
                    PromotionId = promotionOrderLine.Key.PromotionId,
                    PromotionCode = promotionOrderLine.Key.PromotionCode,
                    IsOtherPromotion = promotionOrderLine.Key.IsOtherPromotion,
                    IsOtherDist = promotionOrderLine.Key.IsOtherDist,
                    IsOtherPay = promotionOrderLine.Key.IsOtherPay,
                    HasException = promotionOrderLine.Key.HasException,
                    IsOtherPromotionExc = promotionOrderLine.Key.IsOtherPromotionExc,
                    IsOtherDistExc = promotionOrderLine.Key.IsOtherDistExc,
                    IsOtherPayExc = promotionOrderLine.Key.IsOtherPayExc,
                    PromotionName = promotionOrderLine.Key.PromotionName,
                    PromotionDesc = promotionOrderLine.Key.PromotionDesc,
                    PromotionOrderLineSub = promotionOrderLine.SelectMany(l => l.PromotionOrderLineSub)
                        .GroupBy(ls => new
                        {
                            ls.Id,
                            ls.FatherId,
                            ls.Cond,
                            ls.InGroup,
                            ls.ItemGroup,
                            ls.AddAccumulate,
                            ls.LineId,
                            ListLineId = ls.ListLineId != null ? string.Join(",", ls.ListLineId.OrderBy(x => x)) : "",
                            ls.ItemId,
                            ls.ItemCode,
                            ls.ItemName,
                            ls.PackingId,
                            ls.PackingName,
                            ls.Note,
                            ls.Discount,
                            ls.DiscountType,
                            ls.PriceType
                        })
                        .Select(promotionOrderLineSub => new PromotionOrderLineSub
                        {
                            Id = promotionOrderLineSub.Key.Id,
                            FatherId = promotionOrderLineSub.Key.FatherId,
                            Cond = promotionOrderLineSub.Key.Cond,
                            LineId = promotionOrderLineSub.Key.LineId,
                            InGroup = promotionOrderLineSub.Key.InGroup,
                            ItemGroup = promotionOrderLineSub.Key.ItemGroup,
                            AddAccumulate = promotionOrderLineSub.Key.AddAccumulate,
                            ItemId = promotionOrderLineSub.Key.ItemId,
                            ItemCode = promotionOrderLineSub.Key.ItemCode,
                            ItemName = promotionOrderLineSub.Key.ItemName,
                            PackingId = promotionOrderLineSub.Key.PackingId,
                            PackingName = promotionOrderLineSub.Key.PackingName,
                            QuantityAdd = promotionOrderLineSub.Sum(x => x.QuantityAdd),
                            Note = promotionOrderLineSub.Key.Note,
                            Discount = promotionOrderLineSub.Key.Discount,
                            DiscountType = promotionOrderLineSub.Key.DiscountType,
                            PriceType = promotionOrderLineSub.Key.PriceType,
                            ListLineId = promotionOrderLineSub.SelectMany(x => x.ListLineId ?? Enumerable.Empty<int>())
                                .Distinct().ToArray()
                        }).Where(ls => ls.QuantityAdd > 0 || ls.Discount > 0).ToList()
                }).ToList();
            promotionOrder.PromotionOrderLine =
                (List<PromotionOrderLine>)PromotionOrderLine.Where(e => e.PromotionOrderLineSub.Count > 0)
                    .ToList();
            if (promotionOrder.PromotionOrderLine.Count == 0)
            {
                return false;
            }

            return flag;
        }
    }
}
