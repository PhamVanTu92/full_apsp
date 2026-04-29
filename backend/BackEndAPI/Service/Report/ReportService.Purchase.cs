using BackEndAPI.Models.Document;
using BackEndAPI.Models.Promotion;
using BackEndAPI.Models.Reports;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace BackEndAPI.Service.Report;

public partial class ReportService
{
    public async Task<Models.Reports.PurchaseReport> GetPurchaseReport(DateTime startTime, DateTime endTime,
        int cardId)
    {
        try
        {
            var query = _context.ODOC.Where(d=>d.ObjType == 22).AsNoTracking().AsQueryable();

            if (cardId != 0) query = query.Where(c => c.CardId == cardId);
            var user = _httpContextAccessor.HttpContext?.User;
            var userId = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var usr = await _context.AppUser.AsSplitQuery().AsQueryable().AsNoTracking()
                        .Include(xx => xx.DirectStaff)
                        .Where(xx => xx.Id == int.Parse(userId))
                        .Include(xx => xx.Role)
                        .ThenInclude(xx => xx.RoleFillCustomerGroups)
                        .ThenInclude(x => x.Ocrg)
                        .ThenInclude(x => x.Customers)
                        .FirstOrDefaultAsync();

            if (usr != null && usr.Role != null)
            {
                if (!usr.Role.Name.ToUpper().Equals("QUẢN TRỊ VIÊN"))
                {
                    if (usr.Role.IsFillCustomerGroup)
                    {
                        var cusids = usr.Role.RoleFillCustomerGroups.SelectMany(e => e.Ocrg.Customers.Select(e => e.Id))
                        .ToList();
                        query = query.Where(e => cusids.Contains(e.CardId ?? 0));
                    }
                    if (usr.Role.IsSaleRole)
                    {
                        var (usrIds1, groupId) = await GetAllCustomerUnderManager(int.Parse(userId));
                        query = query.Where(e => usrIds1.Contains(e.CardId ?? 0));
                    }
                }
            }
            var d = await query
                .Include(o => o.ItemDetail)!
                .ThenInclude(i => i.Item)
                .ThenInclude(i => i.Brand)
                .Include(o => o.ItemDetail)
                .ThenInclude(i => i.Item)
                .ThenInclude(i => i.Industry)
                .Include(o => o.ItemDetail)
                .ThenInclude(i => i.Item)
                .ThenInclude(i => i.Brand)
                .Include(o => o.ItemDetail)
                .ThenInclude(i => i.Item)
                .ThenInclude(i => i.ItemType)
                .Include(o => o.ItemDetail)
                .ThenInclude(i => i.Item)
                .ThenInclude(i => i.Packing)
                .Include(i=>i.Promotion)
                .Where(p => p.Status != "DONG" && p.Status != "HUY2" && p.Status != "CXN" &&(string.IsNullOrEmpty(p.DocType) || p.DocType == "NET"))
                .Where(o => o.DeliveryTime.Value.Date >= startTime.Date && o.DeliveryTime.Value.Date <= endTime.Date)
                .ToListAsync();

            var itemDetails = d
                 .SelectMany(o => o.ItemDetail)
                 .Where(i => i.Item != null)
                 .GroupBy(i => new
                 {
                     i.ItemId,
                     i.ItemCode,
                     i.ItemName,
                     Brand = i.Item.Brand?.Name,
                     Industry = i.Item.Industry?.Name,
                     ItemType = i.Item.ItemType?.Name,
                     Packaging = i.Item.Packing?.Name
                 })
                 .Select(g => new
                 {
                     g.Key.ItemId,
                     g.Key.ItemCode,
                     g.Key.ItemName,
                     g.Key.Brand,
                     g.Key.Industry,
                     g.Key.ItemType,
                     g.Key.Packaging,
                     PurchasedQuantity = g.Where(ol => string.IsNullOrEmpty(ol.Type)).Sum(ol => ol.Quantity),
                     TotalOrders = g.Select(ol => ol.FatherId).Distinct().Count()
                 })
                 .ToList();
            var promotions = d
                .SelectMany(o => o.Promotion ?? new List<DOC2>())
                .Where(p => p.ItemId != null)
                .GroupBy(p => p.ItemId)
                .Select(g => new
                {
                    ItemId = g.Key,
                    PromotionalQuantity = g.Sum(p => p.QuantityAdd)
                })
                .ToList();
            var pr = (from i in itemDetails
                      join p in promotions on i.ItemId equals p.ItemId into promoJoin
                      from promo in promoJoin.DefaultIfEmpty()
                      select new Models.Reports.PurchaseReportLine
                      {
                          ItemCode = i.ItemCode,
                          ItemName = i.ItemName,
                          Brand = i.Brand ?? "<unknown>",
                          Industry = i.Industry ?? "<unknown>",
                          ItemType = i.ItemType ?? "<unknown>",
                          Packaging = i.Packaging ?? "<unknown>",
                          PurchasedQuantity = i.PurchasedQuantity,
                          PromotionalQuantity = promo?.PromotionalQuantity ?? 0,
                          TotalOrders = i.TotalOrders
                      })
                      .ToList();
            return new PurchaseReport()
            {
                PromotionalQuantity = pr.Sum(p => p.PromotionalQuantity),
                TotalOrders = pr.Sum(p => p.TotalOrders),
                PurchasedQuantity = pr.Sum(p => p.PurchasedQuantity),
                TotalAccumulatedVolume = pr.Sum(p => p.TotalAccumulatedVolume),
                Lines = pr,
            };
        }
        catch
        {
            throw;
        }
    }


    public async Task<Models.Reports.ItemPurchaseReport> GetItemPurchaseReport(DateTime startTime, DateTime endTime,
        string itemCode, int cardId)
    {
        var queyr = _context.ODOC.Where(d => d.ObjType == 22).AsNoTracking().AsQueryable();
        if (cardId != 0) queyr = queyr.Where(c => c.CardId == cardId);
        var user = _httpContextAccessor.HttpContext?.User;
        var userId = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var usr = await _context.AppUser.AsSplitQuery().AsQueryable().AsNoTracking()
                    .Include(xx => xx.DirectStaff)
                    .Where(xx => xx.Id == int.Parse(userId))
                    .Include(xx => xx.Role)
                    .ThenInclude(xx => xx.RoleFillCustomerGroups)
                    .ThenInclude(x => x.Ocrg)
                    .ThenInclude(x => x.Customers)
                    .FirstOrDefaultAsync();

        if (usr != null && usr.Role != null)
        {
            if (!usr.Role.Name.ToUpper().Equals("QUẢN TRỊ VIÊN"))
            {
                if (usr.Role.IsFillCustomerGroup)
                {
                    var cusids = usr.Role.RoleFillCustomerGroups.SelectMany(e => e.Ocrg.Customers.Select(e => e.Id))
                    .ToList();
                    queyr = queyr.Where(e => cusids.Contains(e.CardId ?? 0));
                }
                if (usr.Role.IsSaleRole)
                {
                    var (usrIds1, groupId) = await GetAllCustomerUnderManager(int.Parse(userId));
                    queyr = queyr.Where(e => usrIds1.Contains(e.CardId ?? 0));
                }
            }
        }
        var d = await queyr
            .Where(p => p.ItemDetail.Any(i => i.ItemCode == itemCode))
            .Include(p => p.ItemDetail.Where(i => i.ItemCode == itemCode))
            .Include(p => p.Promotion.Where(i => i.ItemCode == itemCode))
            .Where(p => p.Status != "DONG" && p.Status != "HUY2" && p.Status != "CXN" && (string.IsNullOrEmpty(p.DocType) || p.DocType == "NET"))
            .Where(o => o.DeliveryTime.Value.Date >= startTime.Date && o.DeliveryTime.Value.Date <= endTime.Date)
            .ToListAsync();

        var dx = d.Select(od => new Models.Reports.ItemPurchaseReportLine
        {
            OrderId = od.Id,
            Currency = od.Currency,
            OrderDate = od.DocDate.Value,
            OrderCode = od.InvoiceCode,
            UnitPrice = od.ItemDetail.FirstOrDefault().Price,
            PromotionalQuantity = od.Promotion.Sum(p => p.QuantityAdd) ?? 0,
            Discount = od.ItemDetail.Where(p => p.Type != "KH" && p.Type != "VPKM").Sum(p => p.Discount ?? 0),
            DiscountType = od.ItemDetail.Where(p => p.Type != "KH" && p.Type != "VPKM").FirstOrDefault()?.DiscountType,
            PaymentType = od.ItemDetail.Where(p => p.Type != "KH" && p.Type != "VPKM").FirstOrDefault()?.PaymentMethodCode switch
            {
                "PayNow" => "Thanh toán ngay",
                "PayCredit" => "Công nợ - Tín chấp",
                "PayGuarantee" => "Công nợ - Bảo lãnh",
                _ => ""
            },
            TaxTotal = od.ItemDetail.Where(p => p.Type != "KH" && p.Type != "VPKM").Sum(p => p.VATAmount ?? 0),
            // TotalAmount = od.ItemDetail.Where(p => p.Type != "KH" && p.Type != "VPKM").Sum(p => (p.Quantity ?? 0 * p.Price ?? 0) + p.VATAmount ?? 0 ),
            QuantityPurchased = od.ItemDetail.Where(p => p.Type != "KH" && p.Type != "VPKM").Sum(p => p.Quantity),
        }).ToList();

        return new ItemPurchaseReport()
        {
            PromotionalQuantity = dx
                .Sum(p => p.PromotionalQuantity),
            Discount = dx.Sum(p => p.Discount),
            TaxTotal = dx.Sum(p => p.TaxTotal),
            TotalAmount = dx.Sum(p => p.TotalAmount),
            QuantityPurchased = dx.Sum(p => p.QuantityPurchased),
            Lines = dx,
        };
    }

    public async Task<List<Models.Reports.TopItemReport>> TopItemReport(DateTime startTime, DateTime endTime, int cardId, int top)
    {
        var queyr = _context.ODOC.Where(d=>d.ObjType == 22).AsNoTracking().AsQueryable();
        if (cardId != 0) queyr = queyr.Where(c => c.CardId == cardId);
        var user = _httpContextAccessor.HttpContext?.User;
        var userId = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var usr = await _context.AppUser.AsSplitQuery().AsQueryable().AsNoTracking()
                    .Include(xx => xx.DirectStaff)
                    .Where(xx => xx.Id == int.Parse(userId))
                    .Include(xx => xx.Role)
                    .ThenInclude(xx => xx.RoleFillCustomerGroups)
                    .ThenInclude(x => x.Ocrg)
                    .ThenInclude(x => x.Customers)
                    .FirstOrDefaultAsync();

        if (usr != null && usr.Role != null)
        {
            if (!usr.Role.Name.ToUpper().Equals("QUẢN TRỊ VIÊN"))
            {
                if (usr.Role.IsFillCustomerGroup)
                {
                    var cusids = usr.Role.RoleFillCustomerGroups.SelectMany(e => e.Ocrg.Customers.Select(e => e.Id))
                    .ToList();
                    queyr = queyr.Where(e => cusids.Contains(e.CardId ?? 0));
                }
                if (usr.Role.IsSaleRole)
                {
                    var (usrIds1, groupId) = await GetAllCustomerUnderManager(int.Parse(userId));
                    queyr = queyr.Where(e => usrIds1.Contains(e.CardId ?? 0));
                }
            }
        }
        var d = await queyr.Include(p => p.ItemDetail)
            .Where(p => p.Status != "DONG" && p.Status != "HUY2" && p.Status != "CXN" && (string.IsNullOrEmpty(p.DocType) || p.DocType == "NET"))
            .Where(o => o.DeliveryTime.Value.Date >= startTime.Date && o.DeliveryTime.Value.Date <= endTime.Date)
            .ToListAsync();
        var dx = d.SelectMany(p => p.ItemDetail).GroupBy(p => p.ItemId)
            .Select(g => new Models.Reports.TopItemReport()
            {
                Quantity = g.Sum(p => p.Quantity),
                ItemName = g.FirstOrDefault()?.ItemName ?? "<unknown>",
            }).OrderByDescending(o => o.Quantity).Take(top).ToList();


        return dx;
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
}
