using BackEndAPI.Data;
using BackEndAPI.Models.Document;
using BackEndAPI.Models.ItemMasterData;
using BackEndAPI.Models.Other;
using BackEndAPI.Models.Reports;
using MailKit;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using BackEndAPI.Models.Account;
using System.Data.Entity.Infrastructure;
using Org.BouncyCastle.Asn1.Pkcs;
using NHibernate.Criterion;

namespace BackEndAPI.Service.Report;

public class AdminDashboardModel
{
    public double CurrentMonthlyRevenue { get; set; }
    public int TotalOrderInTheMonth { get; set; }
    public int TotalCustomers { get; set; }
    public int TotalNewCustomerInTheMonth { get; set; }
    public List<ODOC> LatestOrder { get; set; } = new List<ODOC>();

    public List<TopSaleItem> TopSaleItem { get; set; } = new List<TopSaleItem>();
    public ChartReport ChartReport { get; set; } = new ChartReport();
    public OrderChart OrderChart { get; set; } = new OrderChart();
}
public class ChartReport
{
    public string Title { get; set; }
    public string[] Categories { get; set; }
    public List<ChartSeries> Series { get; set; }
}
public class OrderChart
{
    public int TotalOrders { get; set; }
    public List<OrderRegionData> Regions { get; set; } = new();
}

public class OrderRegionData
{
    public string RegionName { get; set; }
    public int OrderCount { get; set; }
    public double Percentage { get; set; }
}
public class ChartSeries
{
    public string Name { get; set; }
    public int[] Data { get; set; }
}
public class TopSaleItem
{
    public int ItemId { get; set; }
    public Item? Item { get; set; }
    public double? Quantity { get; set; }
}

public class CustomerDashboardModel
{
    public int TotalOrderInTheMonth { get; set; }
    public int TotalIncompleteOrder { get; set; }
    public int TotalOrderComplete { get; set; }
    public int TotalOrder { get; set; }
    public List<ODOC> LatestOrder { get; set; } = [];
}

public class OrderStatus
{
}

public class DashboardService
{
    private readonly AppDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    public DashboardService(AppDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }
    private string GetStatusDisplayName(string status) => status switch
    {
        "DGH" => "Đang giao hàng",
        "DHT" => "Đã hoàn thành",
        "DXN" => "Đã xác nhận",
        "DXL" => "Đang xử lý",
        "HUY2" or "HUY" => "Hủy",
        "DONG" => "Đóng",
        "CTT" => "Chờ thanh toán",
        "DGHR" => "Đang giao hàng",
        "DTT" => "Đã thanh toán",
        _ => status
    };
    public async Task<(AdminDashboardModel?, Mess?)> GetAdminDashboard(int? month, bool isInterCom, DateTime? FromDate, DateTime? ToDate )
    {
        var mess = new Mess();
        try
        {
            var user = await GetCurrentUserAsync();
            var (usrIds1, customerGroupId) = await GetUserRoleDataAsync(user);

            if (month == null || month == 0)
            {
                return await GenerateDashboardAsync(FromDate, ToDate, isInterCom, usrIds1, customerGroupId, null);
            }
            else
            {
                return await GenerateDashboardAsync(FromDate, ToDate, isInterCom, usrIds1, customerGroupId, month);
            }

        }
        catch (Exception ex)
        {
            mess.Errors = ex.Message;
            mess.Status = 500;
            return (null, mess);
        }
    }

    public async Task<(CustomerDashboardModel?, Mess?)> GetCustomerDashboard(int cardId)
    {
        var mess = new Mess();

        try
        {
            var docQuery = _context.ODOC.AsNoTracking().Where(d => d.CardId == cardId && d.ObjType == 22).AsQueryable();

            var dash = new CustomerDashboardModel
            {
                LatestOrder = await docQuery.OrderByDescending(p => p.DocDate).Take(20).ToListAsync(),
                TotalOrderInTheMonth = docQuery.Count(p => p.DocDate!.Value.Month == DateTime.Now.Month),
                TotalOrder = docQuery.Count(),
                TotalIncompleteOrder = docQuery.Count(p => p.Status != "DHT" && p.Status != "HUY"),
                TotalOrderComplete = docQuery.Count(p => p.Status == "DHT")
            };

            return (dash, null);
        }
        catch (Exception ex)
        {
            mess.Errors = ex.Message;
            mess.Status = 500;
            return (null, mess);
        }
    }
    private async Task<AppUser?> GetCurrentUserAsync()
    {
        var userId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return await _context.AppUser.AsSplitQuery()
            .AsNoTracking()
            .Include(xx => xx.DirectStaff)
            .Include(xx => xx.Role)
            .ThenInclude(xx => xx.RoleFillCustomerGroups)
            .FirstOrDefaultAsync(xx => xx.Id == int.Parse(userId));
    }
    private async Task<(List<int>, List<int>)> GetUserRoleDataAsync(AppUser? user)
    {
        var usrIds1 = new List<int>();
        var customerGroupId = new List<int>();

        if (user?.Role != null)
        {
            if (user.Role.IsFillCustomerGroup)
            {
                customerGroupId = user.Role.RoleFillCustomerGroups.Select(d => d.CustomerGroupId).ToList();
            }
            if (user.Role.IsSaleRole)
            {
                usrIds1 = await GetAllCustomerIdWithUserId(user.Id);
            }
        }
        return (usrIds1, customerGroupId);
    }
    private async Task<(AdminDashboardModel?, Mess?)> GenerateDashboardAsync(
       DateTime? FromDate, DateTime? ToDate, bool isInterCom, List<int> usrIds1, List<int> customerGroupId, int? month)
    {
        var year = DateTime.Now.Year;
        var docQuery = _context.ODOC.AsNoTracking().AsSplitQuery()
            .Where(p => p.IsIncoterm == isInterCom &&
                        (p.DocType == null || p.DocType == "" || p.DocType == "NET") &&
                        (usrIds1.Count == 0 || usrIds1.Contains(p.BP.SaleId.Value)) &&
                        (customerGroupId.Count == 0 || p.BP.Groups.Any(c => customerGroupId.Contains(c.Id))));

        if (month.HasValue && month > 0)
        {
            docQuery = docQuery.Where(p => p.DocDate!.Value.Month == month && p.DocDate!.Value.Year == year);
        }
        else
        {
            docQuery = docQuery.Where(p => p.DocDate!.Value.Date >= FromDate!.Value.Date && p.DocDate!.Value.Date <= ToDate!.Value.Date);
        }

        var doc = await docQuery.Include(p => p.ItemDetail).ToListAsync();
        var listDOC = doc.Select(p => p.Id).ToList();

        var chartReport = await GenerateChartReportAsync(year, isInterCom, usrIds1, customerGroupId);
        var orderChart = await GenerateOrderReportAsync(month, FromDate, ToDate, isInterCom, usrIds1, customerGroupId);
        var dashboard = await BuildAdminDashboardAsync(doc, listDOC,month, FromDate, ToDate, isInterCom, usrIds1, customerGroupId, chartReport);
        dashboard.OrderChart = orderChart;
        return (dashboard, null);
    }
    private async Task<ChartReport> GenerateChartReportAsync(int year, bool isInterCom, List<int> usrIds1, List<int> customerGroupId)
    {
        var statuses = new[] { "DGH", "DHT", "DXL", "DXN", "HUY", "HUY2", "DONG", "CTT", "DGHR", "DTT" };
        var rawData = await _context.ODOC.AsNoTracking()
            .Where(o => o.DocDate.HasValue && o.DocDate.Value.Year == year && o.ObjType == 22 &&
                        o.IsIncoterm == isInterCom &&
                        (o.DocType == null || o.DocType == "" || o.DocType == "NET") &&
                        (usrIds1.Count == 0 || usrIds1.Contains(o.BP.SaleId.Value)) &&
                        (customerGroupId.Count == 0 || o.BP.Groups.Any(c => customerGroupId.Contains(c.Id))))
            .GroupBy(o => new { o.DocDate.Value.Month, o.Status })
            .Select(g => new { g.Key.Month, g.Key.Status, Count = g.Count() })
            .ToListAsync();

        var months = Enumerable.Range(1, 12).ToArray();
        var series = rawData
        .GroupBy(x => GetStatusDisplayName(x.Status)) // gộp HUY, HUY2, DGHR, DGH,...
        .Select(g => new ChartSeries
        {
            Name = g.Key,
            Data = months.Select(month =>
                g.Where(x => x.Month == month)
                 .Sum(x => x.Count)
            ).ToArray()
        })
        .ToList();
        //var series = statuses.Select(status => new ChartSeries
        //{
        //    Name = GetStatusDisplayName(status),
        //    Data = months.Select(month => rawData.FirstOrDefault(x => x.Month == month && x.Status == status)?.Count ?? 0).ToArray()
        //}).ToList();

        return new ChartReport
        {
            Title = $"Báo cáo mua hàng năm {year}",
            Categories = months.Select(m => $"Tháng {m}").ToArray(),
            Series = series
        };
    }
    private async Task<OrderChart> GenerateOrderReportAsync(int? month, DateTime? FromDate, DateTime? ToDate, bool isInterCom, List<int> usrIds1, List<int> customerGroupId)
    {
        int TotalOrders = 0;
        List<ODOC> Order = new List<ODOC>();
        var statuses = new[] { "DGH", "DHT", "DXL", "DXN", "HUY", "HUY2", "DONG", "CTT", "DGHR", "DTT" };
        int year = DateTime.Now.Year;
        if (month.HasValue && month > 0)
        {
            Order = await _context.ODOC.AsNoTracking()
            .Where(o => o.DocDate.HasValue && o.DocDate.Value.Year == year && o.DocDate.Value.Month == month && o.ObjType == 22 &&
                        o.IsIncoterm == isInterCom &&
                        (o.DocType == null || o.DocType == "" || o.DocType == "NET") &&
                        (usrIds1.Count == 0 || usrIds1.Contains(o.BP.SaleId.Value)) &&
                        (customerGroupId.Count == 0 || o.BP.Groups.Any(c => customerGroupId.Contains(c.Id))))
            .ToListAsync();
            TotalOrders = Order.Count;
            

        }
        else
        {
            Order = await _context.ODOC.AsNoTracking()
            .Where(o => o.DocDate.HasValue && o.DocDate.Value.Date >= FromDate.Value.Date && o.DocDate.Value.Date <= ToDate.Value.Date && o.ObjType == 22 &&
                        o.IsIncoterm == isInterCom &&
                        (o.DocType == null || o.DocType == "" || o.DocType == "NET") &&
                        (usrIds1.Count == 0 || usrIds1.Contains(o.BP.SaleId.Value)) &&
                        (customerGroupId.Count == 0 || o.BP.Groups.Any(c => customerGroupId.Contains(c.Id))))
            .ToListAsync();
            TotalOrders = Order.Count;
           
        }
        var query = from o in Order
                    join bp in _context.BpClassify.Select(x => new { x.BpId, x.RegionId }).Distinct() on o.CardId equals bp.BpId
                    join r in _context.Regions on bp.RegionId equals r.Id

                    select new
                    {
                        RegionName = r.Name,
                        o.Id
                    };

        var groupedData = query
            .GroupBy(x => x.RegionName)
            .Select(g => new OrderRegionData
            {
                RegionName = g.Key,
                OrderCount = g.Count(),
                Percentage = 0
            })
            .ToList();
        int totalOrders = groupedData.Sum(x => x.OrderCount);
        foreach (var item in groupedData)
        {
            item.Percentage = Math.Round((double)item.OrderCount / totalOrders * 100, 2);
        }

        // Trả về model tổng hợp
        var result = new OrderChart
        {
            TotalOrders = totalOrders,
            Regions = groupedData
        };
        return result;
    }
    private async Task<AdminDashboardModel> BuildAdminDashboardAsync(
       List<ODOC> doc, List<int> listDOC,int? month, DateTime? FromDate, DateTime? ToDate, bool isInterCom, List<int> usrIds1, List<int> customerGroupId, ChartReport chartReport)
    {
        var statuses = new[] { "DGH", "DHT", "DXN", "DGHR", "DTT" };
        if (month.HasValue && month > 0)
        {
            int year = DateTime.Now.Year;
            double CurrentMonthlyRevenue = doc.Where(e=> statuses.Contains(e.Status)).Sum(p => p.Total ?? 0);
            int TotalCustomers = await _context.BP.AsNoTracking().CountAsync(p => p.CardType == "C");
            int TotalNewCustomerInTheMonth = await _context.BP.AsNoTracking().Where(p => p.CreatedDate!.Value.Year == year && p.CreatedDate!.Value.Month == month).CountAsync(p =>
                p.ItemType == "C");
            List<ODOC> LisLatestOrder = await _context.ODOC.AsNoTracking()
                .Where(e => e.ObjType == 22 && e.IsIncoterm == isInterCom && e.DocDate!.Value.Month == month && e.DocDate!.Value.Year == year &&
                            (e.DocType == null || e.DocType == "" || e.DocType == "NET") &&
                            (usrIds1.Count == 0 || usrIds1.Contains(e.BP.SaleId.Value)) &&
                            (customerGroupId.Count == 0 || e.BP.Groups.Any(c => customerGroupId.Contains(c.Id))))
                .OrderByDescending(p => p.DocDate)
                .Take(5)
                .ToListAsync();
            int TotalOrderInTheMonth = doc.Count;
            List<TopSaleItem> TopSaleItem = _context.DOC1.AsNoTracking().AsSplitQuery()
                    .Where(p => listDOC.Where(x => LisLatestOrder.Select(e => e.Id).ToArray().Contains(x)).ToList().Contains(p.FatherId))
                    .Include(x => x.Item).ThenInclude(x => x.ITM1)
                    .GroupBy(oi => oi.ItemId)
                    .Select(oi => new TopSaleItem
                    {
                        ItemId = oi.Key ?? 0,
                        Item = oi.First().Item,
                        Quantity = oi.Sum(oi => oi.Quantity),
                    })
                    .OrderByDescending(oi => oi.Quantity)
                    .Take(10)
                    .ToList();
            return new AdminDashboardModel
            {
                CurrentMonthlyRevenue = CurrentMonthlyRevenue,
                TotalCustomers = TotalCustomers,
                TotalNewCustomerInTheMonth = TotalNewCustomerInTheMonth,
                TotalOrderInTheMonth = TotalOrderInTheMonth,
                LatestOrder = LisLatestOrder,
                TopSaleItem = TopSaleItem,
                ChartReport = chartReport
            };
        }
        else
        {
            double CurrentMonthlyRevenue = doc.Where(e => statuses.Contains(e.Status)).Sum(p => p.Total ?? 0);
            int TotalCustomers = await _context.BP.AsNoTracking().CountAsync(p => p.CardType == "C");
            int TotalNewCustomerInTheMonth = await _context.BP.AsNoTracking().Where(p => p.CreatedDate!.Value.Date >= FromDate!.Value.Date && p.CreatedDate!.Value.Date <= ToDate!.Value.Date).CountAsync(p =>
                p.ItemType == "C");
            List<ODOC> LisLatestOrder = await _context.ODOC.AsNoTracking()
                .Where(e => e.ObjType == 22 && e.IsIncoterm == isInterCom && e.DocDate!.Value.Date >= FromDate!.Value.Date && e.DocDate!.Value.Date <= ToDate!.Value.Date &&
                            (e.DocType == null || e.DocType == "" || e.DocType == "NET") &&
                            (usrIds1.Count == 0 || usrIds1.Contains(e.BP.SaleId.Value)) &&
                            (customerGroupId.Count == 0 || e.BP.Groups.Any(c => customerGroupId.Contains(c.Id))))
                .OrderByDescending(p => p.DocDate)
                .Take(5)
                .ToListAsync();
            int TotalOrderInTheMonth = doc.Count;
            List<TopSaleItem> TopSaleItem = _context.DOC1.AsNoTracking().AsSplitQuery()
                    .Where(p => listDOC.Where(x => LisLatestOrder.Select(e => e.Id).ToArray().Contains(x)).ToList().Contains(p.FatherId))
                    .Include(x => x.Item).ThenInclude(x => x.ITM1)
                    .GroupBy(oi => oi.ItemId)
                    .Select(oi => new TopSaleItem
                    {
                        ItemId = oi.Key ?? 0,
                        Item = oi.First().Item,
                        Quantity = oi.Sum(oi => oi.Quantity),
                    })
                    .OrderByDescending(oi => oi.Quantity)
                    .Take(10)
                    .ToList();
            return new AdminDashboardModel
            {
                CurrentMonthlyRevenue = CurrentMonthlyRevenue,
                TotalCustomers = TotalCustomers,
                TotalNewCustomerInTheMonth = TotalNewCustomerInTheMonth,
                TotalOrderInTheMonth = TotalOrderInTheMonth,
                LatestOrder = LisLatestOrder,
                TopSaleItem = TopSaleItem,
                ChartReport = chartReport
            };
        }
        
    }
    public async Task<List<int>> GetAllCustomerIdWithUserId(int managerUserId)
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
                return [];
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

        return result.ToList();
    }
}