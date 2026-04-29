using BackEndAPI.Data;
using BackEndAPI.Models.BPGroups;
using BackEndAPI.Models.Document;
using BackEndAPI.Models.ItemGroups;
using BackEndAPI.Models.Other;
using BackEndAPI.Models.Reports;
using Gridify;
using MailKit.Search;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Claims;
using static BackEndAPI.Models.Reports.InventoryReport;

namespace BackEndAPI.Service.Report;

public partial class ReportService
{
    private readonly AppDbContext _context;
     Endpoints _endpoints;
    private readonly IHttpContextAccessor _httpContextAccessor;
    public  ReportService(AppDbContext context,   IOptions<Endpoints> options, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _endpoints = options.Value;
        _httpContextAccessor = httpContextAccessor;
    }
    public async Task<(OrderStateReport?, IEnumerable<ODOC>?, int total, double totalBeforeVat, double bonusAmount, double bonusCommited, double totalAfterVat, Mess?)> GetOrderState(int cardId, DateTime from, DateTime to, bool? isConterm, GridifyQuery query1)
    {
        var mess = new Mess();
        try
        {
            var query = _context.ODOC.Where(p => p.ObjType == 22)
                .AsNoTracking().Include(p => p.PaymentInfo).AsQueryable();
            if (cardId != 0)
            {
                query = query.Where(o => o.CardId == cardId);
            }
            var user = _httpContextAccessor.HttpContext?.User;
            var userId = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int id = 0;
            Int32.TryParse(userId, out id);
            var usr = _context.AppUser.AsNoTracking().FirstOrDefault(xx => xx.Id == id && xx.CardId == null);
            if(usr != null)
                query = query.Where(x => x.IsIncoterm == isConterm);   
            query = query.Where(x => x.DocDate >= from && x.DocDate <= to);
            var item = new OrderStateReport()
            {
                Complete = await query.CountAsync(p => p.Status == "DHT"),
                TotalComplete = await query.Where(p => p.Status == "DHT").SumAsync(p => p.PaymentInfo.TotalBeforeVat),
                OnDelivery = await query.CountAsync(p => p.Status == "DGH"),
                TotalOnDelivery = await query.Where(x => x.Status == "DGH").SumAsync(p => p.PaymentInfo.TotalBeforeVat),
                Processing = await query.CountAsync(p => p.Status == "DXL"),
                TotalProcessing = await query.Where(x => x.Status == "DXL").SumAsync(p => p.PaymentInfo.TotalBeforeVat),
                Confirmed = await query.CountAsync(p => p.Status == "DXN"),
                TotalConfirmed = await query.Where(x => x.Status == "DXN").SumAsync(p => p.PaymentInfo.TotalBeforeVat),
                Cancelled = await query.CountAsync(p => p.Status == "HUY" || p.Status == "HUY2"),
                TotalCancelled = await query.Where(x => x.Status == "HUY" || x.Status == "HUY2").SumAsync(p => p.PaymentInfo.TotalBeforeVat),
                Closed = await query.CountAsync(p => p.Status == "DONG"),
                TotalClosed = await query.Where(x => x.Status == "DONG").SumAsync(p => p.PaymentInfo.TotalBeforeVat),
                InPaying = await query.CountAsync(p => p.Status == "CTT"),
                TotalInPaying = await query.Where(x => x.Status == "CTT").SumAsync(p => p.PaymentInfo.TotalBeforeVat),
                Delivied = await query.CountAsync(p => p.Status == "DGHR"),
                TotalDelivied = await query.Where(x => x.Status == "DGHR").SumAsync(p => p.PaymentInfo.TotalBeforeVat),
                Paid = await query.CountAsync(p => p.Status == "DTT"),
                TotalPaid = await query.Where(x => x.Status == "DTT").SumAsync(p => p.PaymentInfo.TotalBeforeVat),
                Pending = await query.CountAsync(p => p.Status == "CXN"),
                TotalPending = await query.Where(x => x.Status == "CXN").SumAsync(p => p.PaymentInfo.TotalBeforeVat),
            };
            var query2 = query.Where(p => p.ObjType == 22);
            if (usr != null)
                query2 = query2.Where(x => x.IsIncoterm == isConterm);
            var totalCount = 0;
            double totalBeforeVat = 0, bonusAmount = 0,bonusCommited = 0, totalAfterVat = 0;
            List<ODOC> items;
            if (cardId != 0)
            {
                items = await query2.ApplyFiltering(query1)
                .ApplyPaging(query1.Page, query1.PageSize)
                .Where(p => p.CardId == cardId).OrderByDescending(p => p.DocDate)
                .Include(p => p.ItemDetail)
                .Include(p => p.Address)
                .Include(p => p.AttDocuments)
                .ThenInclude(p => p.Author)
                .Include(p => p.PaymentMethod)
                .Include(p => p.AttachFile)
                .Include(p => p.Approval)
                .Include(p => p.Author)
                .Include(p => p.Promotion)
                .ToListAsync();
                totalCount = await query2.ApplyFiltering(query1).Where(p => p.CardId == cardId).CountAsync();
                totalBeforeVat = await query2.ApplyFiltering(query1).Where(p => p.CardId == cardId).SumAsync(p => p.PaymentInfo.TotalBeforeVat);
                bonusAmount = await query2.ApplyFiltering(query1).Where(p => p.CardId == cardId).SumAsync(p => p.PaymentInfo.BonusAmount);
                bonusCommited = await query2.ApplyFiltering(query1).Where(p => p.CardId == cardId).SumAsync(p => p.PaymentInfo.BonusCommited);
                totalAfterVat = await query2.ApplyFiltering(query1).Where(p => p.CardId == cardId).SumAsync(p => p.PaymentInfo.TotalAfterVat);
            }    
            else
            {
                items = await query2.ApplyFiltering(query1).OrderByDescending(p => p.DocDate)
                .ApplyPaging(query1.Page, query1.PageSize)
                .Include(p => p.ItemDetail)
                .Include(p => p.Address)
                .Include(p => p.AttDocuments)
                .ThenInclude(p => p.Author)
                .Include(p => p.PaymentMethod)
                .Include(p => p.AttachFile)
                .Include(p => p.Approval)
                .Include(p => p.Author)
                .Include(p => p.Promotion)
                .ToListAsync();
                totalCount = await query2.ApplyFiltering(query1).CountAsync();
                totalBeforeVat = await query2.ApplyFiltering(query1).SumAsync(p => p.PaymentInfo.TotalBeforeVat);
                bonusAmount = await query2.ApplyFiltering(query1).SumAsync(p => p.PaymentInfo.BonusAmount);
                bonusCommited = await query2.ApplyFiltering(query1).SumAsync(p => p.PaymentInfo.BonusCommited);
                totalAfterVat = await query2.ApplyFiltering(query1).SumAsync(p => p.PaymentInfo.TotalAfterVat);
            }    
            return (item, items, totalCount,totalBeforeVat, bonusAmount, bonusCommited, totalAfterVat, null);
        }
        catch (Exception ex)
        {
            mess.Errors = ex.Message;
            mess.Status = 500;
            return (null,null,0,0,0,0,0, mess);
        }
    }

    public async Task<(Models.Reports.DebtReport?, Mess?)> GetDebtReport()
    {
        var mess = new Mess();
        try
        {
            var query = _context.ODOC.AsNoTracking().AsQueryable();

            return (new DebtReport()
            {
                DebtAge = await query.CountAsync(p => p.Status == "DHT"),
                GuaranteedDebt = await query.CountAsync(p => p.Status == "DGH"),
                PayNow = await query.CountAsync(p => p.Status == "DXL"),
                TotalDebt = await query.CountAsync(p => p.Status == "DXN"),
                UnsecuredDebt = await query.CountAsync(p => p.Status == "HUY"),
            }, null);
        }
        catch (Exception ex)
        {
            mess.Errors = ex.Message;
            mess.Status = 500;
            return (null, mess);
        }
    }

    public async Task<(IEnumerable<Inventory>, Mess)> GetInventoryReport(DateTime FromDate, DateTime ToDate, string? CardCode)
    {
        var mess = new Mess();
        try
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get,
                _endpoints.Host + "/InventoryList?FromDate="+ FromDate.ToString("yyyy-MM-dd") + "&ToDate="+ ToDate.ToString("yyyy-MM-dd") + "&CardCode=" + CardCode);
            request.Headers.Add("accept", "*/*");
            var response = await client.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                // Read the response content as a JSON string
                var jsonString = await response.Content.ReadAsStringAsync();
                if (jsonString != "null")
                {
                    jsonString = jsonString.Replace("\"Inventory\":", "");
                    int jsonLen = jsonString.Length - 2;
                    string jsonStringL = jsonString.Substring(1, jsonLen);
                    var itemsCheck= JsonConvert.DeserializeObject<List<Inventory>>(jsonStringL);
                    
                    var listItem = itemsCheck.Select(e => e.ItemCode).ToList();
                    var oitm = _context.Item.Where (p=> listItem.Contains(p.ItemCode)).Select(p => new { p.ItemCode, Brand =p.Brand.Name, Packing = p.Packing.Name ?? "", Industry = p.Industry.Name ?? "", ItemType =p.ItemType.Name??"" }).ToList();
                    var items = itemsCheck.Where(p => oitm.Select(e => e.ItemCode).Contains(p.ItemCode)).ToList();
                    items.ForEach(item =>
                    {
                        item.Brand = oitm.Where(e=>e.ItemCode == item.ItemCode).Select(e => e.Brand).First() ?? item.Brand;
                        item.PackagingSpecifications = oitm.Where(e => e.ItemCode == item.ItemCode).Select(e => e.Packing).First() ?? item.PackagingSpecifications;
                        item.ProductType = oitm.Where(e => e.ItemCode == item.ItemCode).Select(e => e.ItemType).First() ?? item.ProductType;
                        item.Category = oitm.Where(e => e.ItemCode == item.ItemCode).Select(e => e.Industry).First() ?? item.Category;
                    });
                    return (items, null);
                }
                else
                {
                    return (null, null);
                }
            }
            else
            {
                mess.Status = (int)response.StatusCode;
                mess.Errors = "Lỗi đồng bộ";
                return (null, mess);
            }

        }
        catch (Exception ex)
        {
            mess.Errors = ex.Message;
            mess.Status = 500;
            return (null, mess);
        }
    }
    public async Task<(IEnumerable<BalanceBP>, Mess)> GetBalanceBPReport(DateTime FromDate, DateTime ToDate, string? CardCode)
    {
        var mess = new Mess();
        try
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get,
                _endpoints.Host + "/BalanceBP?FromDate=" + FromDate.ToString("yyyy-MM-dd") + "&ToDate=" + ToDate.ToString("yyyy-MM-dd") + "&CardCode=" + CardCode);
            request.Headers.Add("accept", "*/*");
            var response = await client.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                // Read the response content as a JSON string
                var jsonString = await response.Content.ReadAsStringAsync();
                if (jsonString != "null")
                {
                    jsonString = jsonString.Replace("\"BalanceBP\":", "");
                    int jsonLen = jsonString.Length - 2;
                    string jsonStringL = jsonString.Substring(1, jsonLen);
                    var items = JsonConvert.DeserializeObject<List<Balance>>(jsonStringL);
                    var BPs = items.DistinctBy(p => new { p.CardCode, p.CardName,p.OBCredit, p.OBDebit }).ToList();
                    List<BalanceBP> BPs1 = new List<BalanceBP>();
                    foreach (var bp in BPs)
                    {
                        BalanceBP b = new BalanceBP();
                        var item = items.Where(p => p.CardCode == bp.CardCode && p.CardName == bp.CardName).ToList();
                        var totalDebit =  item.Sum(p => p.Debit);
                        var totalCredit = item.Sum(p => p.Credit);
                        b.CardCode = bp.CardCode;
                        b.CardName = bp.CardName;
                        b.OBDebit = bp.OBDebit;
                        b.OBCredit = bp.OBCredit;
                        b.Debit = totalDebit;
                        b.Credit = totalCredit;
                        if (bp.OBDebit - bp.OBCredit + totalCredit - totalDebit > 0)
                        {
                            b.EBDebit = bp.OBDebit - bp.OBCredit + totalCredit - totalDebit;
                        }
                        else
                        {
                            b.EBCredit = -(bp.OBDebit - bp.OBCredit + totalCredit - totalDebit);
                        }
                        var detail = items.Where(p => p.CardCode == bp.CardCode)
                        .Select(p => new BPBalance
                        {
                            FCCurrency = p.FCCurrency,
                            Rate = p.Rate,
                            RefDate = p.RefDate,
                            TaxDate = p.TaxDate,
                            Debit = p.Debit,
                            VoucherNo = p.VoucherNo,
                            Credit = p.Credit,
                            LineMemo = p.LineMemo
                        });
                        b.Detail = detail.ToList();
                        BPs1.Add(b);
                    }    
                    return (BPs1, null);
                }
                else
                {
                    return (null, null);
                }
            }
            else
            {
                mess.Status = (int)response.StatusCode;
                mess.Errors = "Lỗi đồng bộ";
                return (null, mess);
            }

        }
        catch (Exception ex)
        {
            mess.Errors = ex.Message;
            mess.Status = 500;
            return (null, mess);
        }
    }

    public async Task<(IEnumerable<BCCN>, Mess)> GetdebtBP(DateTime ToDate, string? CardCode, string? Employee, string? location)
    {
        var mess = new Mess();
        try
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get,
                _endpoints.Host + "/debtBP?ToDate="+ ToDate + "&CardCode="+ CardCode + "&Employee="+Employee+"&Location="+location+"");
            request.Headers.Add("accept", "*/*");
            var response = await client.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                // Read the response content as a JSON string
                var jsonString = await response.Content.ReadAsStringAsync();
                if (jsonString != "null")
                {
                    jsonString = jsonString.Replace("\"BCCN\":", "");
                    int jsonLen = jsonString.Length - 2;
                    string jsonStringL = jsonString.Substring(1, jsonLen);
                    var items = JsonConvert.DeserializeObject<List<BCCN>>(jsonStringL);
                    return (items, null);
                }
                else
                {
                    return (null, null);
                }
            }
            else
            {
                mess.Status = (int)response.StatusCode;
                mess.Errors = "Lỗi đồng bộ";
                return (null, mess);
            }

        }
        catch (Exception ex)
        {
            mess.Errors = ex.Message;
            mess.Status = 500;
            return (null, mess);
        }
    }
    public async Task<(IEnumerable<ZaloReport>, Mess, int)> GetZalo([FromQuery] GridifyQuery query)
    {
        var mess = new Mess();
        try
        {
            var total = await _context.ODOC.Where(e => e.ZaloCompleted == false || e.ZaloConfirmed == false)
                .Select(e => new
                ZaloReport
                { DocId = e.Id, ObjType = e.ObjType ?? 0, InvoiceCode = e.InvoiceCode, ZaloMess = e.ZaloError, ZaloMess1 = e.ZaloError1 })
                .ApplyFiltering(query)
                .CountAsync();
            var zalo = _context.ODOC.Where(e => e.ZaloCompleted == false || e.ZaloConfirmed == false)
                .Select(e => new 
                ZaloReport { DocId = e.Id, ObjType = e.ObjType ?? 0,InvoiceCode = e.InvoiceCode, ZaloMess = e.ZaloError, ZaloMess1 = e.ZaloError1 })
                .ApplyFiltering(query)
                .OrderByDescending(p => p.DocId).ToList();
            return (zalo, null, total);
        }
        catch (Exception ex)
        {
            mess.Errors = ex.Message;
            mess.Status = 500;
            return (null, mess, 0);
        }
    }
    public async Task<(IEnumerable<LinkInovie>, Mess)> GetLinkInovie(DateTime fromdate, DateTime toDate)
    {
        var mess = new Mess();
        try
        {
           
            var user = _httpContextAccessor.HttpContext?.User;
            var userId = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var carId = _context.AppUser.Where(e=>e.Id == int.Parse(userId)).Select(e => e.CardId).FirstOrDefault();
            string CardCode = "";
            if(carId != null)
                CardCode = _context.BP.AsNoTracking().Where(e => e.Id == carId).Select(e => e.CardCode).FirstOrDefault();
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get,
                _endpoints.Host + "/linkInvoice?fromDate="+fromdate+"&todate="+toDate+ "&CardCode="+CardCode);
            request.Headers.Add("accept", "*/*");
            var response = await client.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                // Read the response content as a JSON string
                var jsonString = await response.Content.ReadAsStringAsync();
                if (jsonString != "null")
                {
                    jsonString = jsonString.Replace("\"LinkInovie\":", "");
                    int jsonLen = jsonString.Length - 2;
                    string jsonStringL = jsonString.Substring(1, jsonLen);
                    var items = JsonConvert.DeserializeObject<List<LinkInovie>>(jsonStringL);
                    return (items, null);
                }
                else
                {
                    return (null, null);
                }
            }
            else
            {
                mess.Status = (int)response.StatusCode;
                mess.Errors = "Lỗi đồng bộ";
                return (null, mess);
            }

        }
        catch (Exception ex)
        {
            mess.Errors = ex.Message;
            mess.Status = 500;
            return (null, mess);
        }
    }

    public async Task<(IEnumerable<ReportPayNow>, Mess)> GetPayNow(DateTime fromdate, DateTime toDate, string CardId)
    {
        var mess = new Mess();
        try
        {
            List<string> values = null;
            if (CardId != null)
                values = CardId.Split(',').ToList();
            List<ReportPayNow> listReportPayNow = new List<ReportPayNow>();
            List<ODOC> doc = new List<ODOC>();
            if(!CardId.IsNullOrEmpty())
                doc = await _context.ODOC.Include(p=>p.ItemDetail).AsNoTracking()
                .Where(p => p.DocDate.Value.Date >= fromdate && p.DocDate.Value.Date <= toDate.Date && (values.Contains(p.CardId.ToString() ?? "")) &&p.Status !="HUY").ToListAsync();
            else
                doc = await _context.ODOC.Include(p => p.ItemDetail).Include(p => p.PaymentInfo).AsNoTracking()
                    .Where(p => p.DocDate.Value.Date >= fromdate && p.DocDate.Value.Date <= toDate.Date  && p.Status != "HUY").ToListAsync();
            foreach (var d in doc)
            {
                if(d.Bonus != null && d.Bonus != 0)
                {
                    ReportPayNow rpt = new ReportPayNow();
                    rpt.CardId = d.CardId ?? 0;
                    rpt.CardCode = d.CardCode;
                    rpt.CardName = d.CardName;
                    rpt.DocDate = d.DocDate;
                    rpt.InvocieCode = d.InvoiceCode;
                    rpt.Value = d.BonusAmount / d.Bonus * 100;
                    if(d.PaymentInfo != null)
                        rpt.ValueInvoice = d.PaymentInfo.TotalPayNow;
                    rpt.Bonus = d.Bonus;
                    rpt.Total = d.BonusAmount;
                    if(rpt.Value > 0)
                        listReportPayNow.Add(rpt);
                }
            }
            return (listReportPayNow, null);

        }
        catch (Exception ex)
        {
            mess.Errors = ex.Message;
            mess.Status = 500;
            return (null, mess);
        }
    }

    public async Task<(IEnumerable<ProCommitment>, Mess)> GetCommited(DateTime fromdate, DateTime toDate, int CardId)
    {
        //try
        //{
        //    var query = _context.Committed.AsQueryable();
        //    var data = await query
        //        .Include(p => p.BP)
        //        .Include(p => p.CommittedLine)!
        //        .ThenInclude(p => p.CommittedLineSub)!
        //        .ThenInclude(p => p.Industry)
        //        .Include(p => p.CommittedLine)!
        //        .ThenInclude(p => p.CommittedLineSub)!
        //        .ThenInclude(p => p.Brand)!
        //        .Include(p => p.CommittedLine)!
        //        .ThenInclude(p => p.CommittedLineSub)!
        //        .ThenInclude(p => p.CommittedLineSubSub)
        //        .Include(p => p.CommittedLine)!
        //        .ThenInclude(p => p.CommittedLineSub)!
        //        .ThenInclude(p => p.ItemTypes)
        //        .OrderByDescending(p => p.Id)
        //        .FirstOrDefaultAsync(p=>p.CardId == CardId);
        //    List<ProCommitment> proCommitments = new List<ProCommitment>();
        //    if (data != null)
        //    {
        //        foreach (var item in data.CommittedLine.FirstOrDefault().CommittedLineSub.ToList())
        //        {
        //            ProCommitment proCommitment = new ProCommitment();
        //            proCommitment.Industry = item.Industry.Name;
        //            proCommitment.Brand = item.Brand.;
        //            proCommitment.ProductType = data.CommittedLine.FirstOrDefault().CommittedLineSub.ItemTypes.Name;
        //            List<DiscountInfo> discountInfos = new List<DiscountInfo>();
        //            foreach (var item in data.CommittedLine)
        //            {
        //                DiscountInfo discountInfo = new DiscountInfo();
        //                discountInfo.ActualProduction = item.ActualProduction;
        //                discountInfo.DiscountAmount = item.DiscountAmount;
        //                discountInfo.Percentage = item.Percentage;
        //                discountInfo.BonusAmount = item.BonusAmount;
        //                discountInfos.Add(discountInfo);
        //            }
        //            proCommitment.DiscountInfo = discountInfos;
        //            proCommitments.Add(proCommitment);
        //        }
        //    }
        //}
        //catch(Exception ex)
        //{
            
        //}
        return (null, null);
    }
}