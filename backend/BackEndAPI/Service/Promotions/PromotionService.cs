using BackEndAPI.Data;
using BackEndAPI.Models.Banks;
using BackEndAPI.Models.BusinessPartners;
using BackEndAPI.Models.Common;
using BackEndAPI.Models.Document;
using BackEndAPI.Models.ItemGroups;
using BackEndAPI.Models.ItemMasterData;
using BackEndAPI.Models.Other;
using BackEndAPI.Models.Promotion;
using BackEndAPI.Models.Unit;
using BackEndAPI.Service.BPGroups;
using BackEndAPI.Service.Committeds;
using BackEndAPI.Service.Payments;
using BackEndAPI.Service.Unit;
using Gridify;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;

namespace BackEndAPI.Service.Promotions
{
    public class PromotionService : Service<Promotion>, IPromotionService
    {
        private readonly IBPGroupService _bpgroupService;
        private readonly PromotionCalculatorService _cService;
        private readonly AppDbContext _context;
        private readonly ICommittedService _committedService;
        private readonly IModelUpdater _modelUpdater;

        public PromotionService(AppDbContext context, IModelUpdater modelUpdater, IBPGroupService bpgroupService,
            ICommittedService committedService, PromotionCalculatorService cService) : base(context)
        {
            _context = context;
            _committedService = committedService;
            _modelUpdater = modelUpdater;
            _bpgroupService = bpgroupService;
            _cService = cService;   
        }

        public async Task<(Promotion, Mess)> AddPromotionAsync(Promotion model)
        {
            Mess mess = new Mess();
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    if (string.IsNullOrEmpty(model.PromotionCode))
                    {
                        var maxCode = _context.Promotion
                            .Where(c => c.PromotionCode.StartsWith("CTKM"))
                            .OrderByDescending(c => c.PromotionCode)
                            .Select(c => c.PromotionCode)
                            .FirstOrDefault();

                        int newNumber = 1;
                        if (!string.IsNullOrEmpty(maxCode))
                        {
                            var numberPart = maxCode.Substring(4);
                            if (int.TryParse(numberPart, out var currentNumber))
                            {
                                newNumber = currentNumber + 1;
                            }
                        }

                        model.PromotionCode = $"CTKM{newNumber:00000}";
                    }

                    var checks =
                        await _context.Promotion.FirstOrDefaultAsync(e => e.PromotionCode == model.PromotionCode);
                    if (checks != null)
                    {
                        mess.Status = 400;
                        mess.Errors = "Mã chương trình khuyến mại đã tồn tại";
                        return (null, mess);
                    }

                    _context.Promotion.Add(model);
                    await _context.SaveChangesAsync();
                    transaction.Commit();
                    return (model, null);
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    var isDuplicate = ex.Message.Contains("Cannot insert duplicate") ||
                                      ex.InnerException?.Message.Contains("Cannot insert duplicate key") == true;
                    mess.Status = isDuplicate ? 400 : 900;
                    mess.Errors = isDuplicate ? "Mã chương trình khuyến mại đã tồn tại" : ex.Message;
                    return (null, mess);
                }
            }
        }


        public async Task<(IEnumerable<Promotion>, int total, Mess)> GetPromotionAsync(int skip, int limit,
            string? search, GridifyQuery q, int userId)
        {
            Mess mess = new Mess();
            try
            {
                var query = _context.Promotion.AsNoTracking().AsSplitQuery().ApplyFiltering(q).AsQueryable();
                var usr = await _context.AppUser.AsSplitQuery().AsQueryable().AsNoTracking()
                    .Include(xx => xx.DirectStaff)
                    .Where(xx => xx.Id == userId)
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
                            query = query.Where(e => (e.PromotionCustomer.Any(c =>
                                usr.Role.RoleFillCustomerGroups.Select(d => d.CustomerGroupId).ToList().Contains(c.CustomerId) && c.Type == "G")) || (e.IsAllCustomer == true)|| (e.PromotionCustomer.Any(c => cusids.Contains(c.CustomerId) && c.Type == "C")));
                        }
                        if (usr.Role.IsSaleRole)
                        {
                            var (usrIds1, groupId) = await _cService.GetAllCustomerUnderManager(userId);
                            query = query.Where(e => (e.PromotionCustomer.Any(x => usrIds1.Contains(x.CustomerId) && x.Type == "C") || ((!e.PromotionCustomer.Any() && e.IsAllCustomer)) || (e.PromotionCustomer.Any(x => groupId.Contains(x.CustomerId) && x.Type == "G") || (!e.PromotionCustomer.Any() && e.IsAllCustomer))));
                        }
                    }
                }
                if (!string.IsNullOrEmpty(search))
                {
                    search = search.Trim().ToLower();
                    query = query.Where(p =>
                        p.PromotionCode.ToLower().Contains(search) ||
                        p.PromotionDescription.ToLower().Contains(search) ||
                        p.PromotionName.ToLower().Contains(search) ||
                        p.PromotionCustomer.Any(z => search.Contains(z.CustomerName.ToLower())));
                }

                var totalCount = await query.CountAsync();
                var items = await query.OrderByDescending(p => p.Id).Skip(skip * limit).Take(limit)
                    .WithFullDetails()
                    .ToListAsync();
                return (items, totalCount, null);
            }
            catch (Exception ex)
            {
                mess.Status = 900;
                mess.Errors = ex.ToString();
                return (null, 0, mess);
            }
        }

        public async Task<(PromotionOrder, Mess)> GetPromotionAsync(PromotionParam promotionParam)
        {
            Mess mess = new Mess();
            try
            {
                if (!string.IsNullOrEmpty(promotionParam.DocType))
                {
                    mess.Status = 400;
                    mess.Errors = "Không có chương trình khuyến mại";
                    return (null, mess);
                }
                var promotions = await _context.Promotion
                    .AsNoTracking()
                    .AsSplitQuery()
                    .Where(p => p.FromDate.Date <= promotionParam.OrderDate.Date &&
                                p.ToDate.Date >= promotionParam.OrderDate.Date && p.PromotionStatus == "A")
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
                    .ThenInclude(p => p.PromotionSubUnit)
                    .ToListAsync();
                if (promotions.Count == 0)
                {
                    mess.Status = 400;
                    mess.Errors = "Không có chương trình khuyến mại";
                    return (null, mess);
                }

                promotionParam.PromotionParamLine.ForEach(item => item.QuantityRef = item.Quantity);
                var promotionByCustomer = promotions.Where(p => p.IsAllCustomer == false
                                                                && p.PromotionCustomer.Any(p =>
                                                                    p.CustomerId.ToString() == promotionParam.CardId)
                                                                && p.PromotionCustomer.Any(p =>
                                                                    p.Type.ToString() == "C") && p.IsIgnore == false)
                    .ToList();
                PromotionOrder promotionOrder1 = new PromotionOrder();
                PromotionOrder promotionOrder = new PromotionOrder();
                promotionOrder.OrderDate = promotionParam.OrderDate;
                promotionOrder.CardId = promotionParam.CardId;
                // Theo đích danh khách hàng
                int FlagCheck = 0;
                if (promotionByCustomer.Count() > 0)
                {
                    var pro = promotionByCustomer.Where(e => e.IsBirthday == true).ToList();
                    if (pro.Count > 0)
                    {
                        getPromotionsBirthday(pro, promotionParam, ref promotionOrder1);
                        promotionOrder.PromotionOrderLine = promotionOrder1.PromotionOrderLine;
                    }

                    var pro1 = promotionByCustomer.Where(e => e.IsBirthday == false).ToList();
                    if (pro1.Count > 0)
                    {
                        getPromotionNews(pro1, promotionParam, ref promotionOrder1);
                        if (promotionOrder.PromotionOrderLine == null)
                            promotionOrder.PromotionOrderLine = promotionOrder1.PromotionOrderLine;
                        else
                            promotionOrder.PromotionOrderLine.AddRange(promotionOrder1.PromotionOrderLine);
                    }

                    if (!_cService.checkPromotion(promotionOrder))
                    {
                        FlagCheck = FlagCheck + 1;
                    }
                }

                if (FlagCheck > 0 || promotionByCustomer.Count() < 1)
                {
                    FlagCheck = 0;
                    var promotionGroup = promotions.Where(p => p.IsAllCustomer == false
                                                               && p.PromotionCustomer.Any(p =>
                                                                   p.Type.ToString() == "G") && p.IsIgnore == false);

                    foreach (var pg in promotionGroup)
                    {
                        var idb = pg.PromotionCustomer.Select(obj => obj.CustomerId).ToList();
                        foreach (var id in idb)
                        {
                            var (bpGroup, mes) = await _bpgroupService.GetBPGroupById(id);
                            if (bpGroup != null)
                            {
                                var bp = bpGroup.Customers.Where(p => p.Id.ToString() == promotionParam.CardId);
                                if (bp.Count() > 0)
                                {
                                    if (!promotionByCustomer.Select(e => e.PromotionCode).Contains(pg.PromotionCode))
                                        promotionByCustomer.Add(pg);
                                }
                            }
                        }
                    }

                    if (promotionByCustomer.Count > 0)
                    {
                        var pro = promotionByCustomer.Where(e => e.IsBirthday == true).ToList();
                        if (pro.Count > 0)
                        {
                            getPromotionsBirthday(pro, promotionParam, ref promotionOrder1);
                            promotionOrder.PromotionOrderLine = promotionOrder1.PromotionOrderLine;
                        }

                        var pro1 = promotionByCustomer.Where(e => e.IsBirthday == false).ToList();
                        if (pro1.Count > 0)
                        {
                            getPromotionNews(pro1, promotionParam, ref promotionOrder1);
                            if (promotionOrder.PromotionOrderLine == null)
                                promotionOrder.PromotionOrderLine = promotionOrder1.PromotionOrderLine;
                            else
                                promotionOrder.PromotionOrderLine.AddRange(promotionOrder1.PromotionOrderLine);
                        }

                        if (!_cService.checkPromotion(promotionOrder))
                        {
                            FlagCheck = FlagCheck + 1;
                        }
                    }

                    if (FlagCheck > 0 || promotionByCustomer.Count() < 1)
                    {
                        FlagCheck = 0;
                        //var promotionGroupIgnore = promotions.Where(p => p.IsAllCustomer == false && p.PromotionCustomer.Any(p =>p.Type.ToString() == "G") &&p.IsIgnore == true);
                        //foreach (var pg in promotionGroupIgnore)
                        //{
                        //    var idCustG = pg.PromotionCustomer.Select(obj => obj.CustomerId).ToList();
                        //    var idbs = _context.OCRG.Select(e => new { e.Id }).Where(e => !idCustG.Contains(e.Id))
                        //        .ToList();
                        //    var idb = idbs.Select(e => e.Id).ToList();
                        //    foreach (var id in idb)
                        //    {
                        //        var (bpGroup, mes) = await _bpgroupService.GetBPGroupById(id);
                        //        if (bpGroup != null)
                        //        {
                        //            var bp = bpGroup.Customers.Where(p => p.Id.ToString() == promotionParam.CardId);
                        //            if (bp.Count() > 0)
                        //            {
                        //                if (!promotionByCustomer.Select(e => e.PromotionCode)
                        //                        .Contains(pg.PromotionCode))
                        //                    promotionByCustomer.Add(pg);
                        //            }
                        //        }
                        //    }
                        //}
                        //}
                        var customerGroupIds = await _context.OCRG
                        .AsNoTracking()
                        .Where(g => g.Customers.Any(c => c.Id.ToString() == promotionParam.CardId))
                        .Select(g => g.Id).ToListAsync();

                        var promotionGroupIgnore = promotions.Where(p => !p.IsAllCustomer && p.IsIgnore && p.PromotionCustomer.Any(pc => pc.Type == "G")).ToList();
                        foreach (var pg in promotionGroupIgnore)
                        {
                            var ignoredGroupIds = pg.PromotionCustomer
                                .Select(x => x.CustomerId)
                                .ToHashSet();
                            var isValid = customerGroupIds.Any(gid => !ignoredGroupIds.Contains(gid));

                            if (isValid)
                            {
                                if (!promotionByCustomer.Any(e => e.PromotionCode == pg.PromotionCode))
                                {
                                    promotionByCustomer.Add(pg);
                                }
                            }
                        }
                        if (promotionByCustomer.Count > 0)
                        {
                            var pro = promotionByCustomer.Where(e => e.IsBirthday == true).ToList();
                            if (pro.Count > 0)
                            {
                                getPromotionsBirthday(pro, promotionParam, ref promotionOrder1);
                                promotionOrder.PromotionOrderLine = promotionOrder1.PromotionOrderLine;
                            }

                            var pro1 = promotionByCustomer.Where(e => e.IsBirthday == false).ToList();
                            if (pro1.Count > 0)
                            {
                                getPromotionNews(pro1, promotionParam, ref promotionOrder1);
                                if (promotionOrder.PromotionOrderLine == null)
                                    promotionOrder.PromotionOrderLine = promotionOrder1.PromotionOrderLine;
                                else
                                    promotionOrder.PromotionOrderLine.AddRange(promotionOrder1.PromotionOrderLine);
                            }

                            if (!_cService.checkPromotion(promotionOrder))
                            {
                                FlagCheck = FlagCheck + 1;
                            }
                        }

                        if (FlagCheck > 0 || promotionByCustomer.Count() < 1)
                        {
                            FlagCheck = 0;
                            var promotionCustIgnore = promotions.Where(p => p.IsAllCustomer == false
                                                                            && p.PromotionCustomer.Any(p =>
                                                                                p.CustomerId.ToString() !=
                                                                                promotionParam.CardId)
                                                                            && p.PromotionCustomer.Any(p =>
                                                                                p.Type.ToString() == "C") &&
                                                                            p.IsIgnore == true).ToList();
                            var promotionByCustomer1 = promotions.Where(p => p.IsAllCustomer == true).ToList();
                            promotionByCustomer = promotionCustIgnore;
                            foreach (var pg in promotionCustIgnore.ToList())
                            {
                                var idb = pg.PromotionCustomer.Select(obj => obj.CustomerId).ToList();
                                foreach (var id in idb)
                                {
                                    if (id.ToString() == promotionParam.CardId)
                                    {
                                        promotionByCustomer.Remove(pg);
                                    }
                                }
                            }

                            promotionByCustomer.AddRange(promotionByCustomer1);
                            if (promotionByCustomer.Count > 0)
                            {
                                var pro = promotionByCustomer.Where(e => e.IsBirthday == true).ToList();
                                if (pro.Count > 0)
                                {
                                    getPromotionsBirthday(pro, promotionParam, ref promotionOrder1);
                                    promotionOrder.PromotionOrderLine = promotionOrder1.PromotionOrderLine;
                                }

                                var pro1 = promotionByCustomer.Where(e => e.IsBirthday == false).ToList();
                                if (pro1.Count > 0)
                                {
                                    getPromotionNews(pro1, promotionParam, ref promotionOrder1);
                                    if (promotionOrder.PromotionOrderLine == null)
                                        promotionOrder.PromotionOrderLine = promotionOrder1.PromotionOrderLine;
                                    else
                                        promotionOrder.PromotionOrderLine.AddRange(promotionOrder1.PromotionOrderLine);
                                }

                                if (!_cService.checkPromotion(promotionOrder))
                                {
                                    FlagCheck = FlagCheck + 1;
                                }
                            }

                            if (FlagCheck > 0 || promotionByCustomer.Count() < 1)
                            {
                                FlagCheck = 0;
                                if (promotionByCustomer.Count > 0)
                                {
                                    var pro = promotionByCustomer.Where(e => e.IsBirthday == true).ToList();
                                    if (pro.Count > 0)
                                    {
                                        getPromotionsBirthday(pro, promotionParam, ref promotionOrder1);
                                        promotionOrder.PromotionOrderLine = promotionOrder1.PromotionOrderLine;
                                    }

                                    var pro1 = promotionByCustomer.Where(e => e.IsBirthday == false).ToList();
                                    if (pro1.Count > 0)
                                    {
                                        getPromotionNews(pro1, promotionParam, ref promotionOrder1);
                                        if (promotionOrder.PromotionOrderLine == null)
                                            promotionOrder.PromotionOrderLine = promotionOrder1.PromotionOrderLine;
                                        else
                                            promotionOrder.PromotionOrderLine.AddRange(promotionOrder1
                                                .PromotionOrderLine);
                                    }
                                }
                            }
                        }
                    }
                }
                if (_cService.checkPromotion(promotionOrder))
                    return (promotionOrder, null);
                else
                {
                    mess.Status = 400;
                    mess.Errors = "Không có chương trình khuyến mại";
                    return (null, mess);
                }
                
            }
            catch (Exception ex)
            {
                mess.Status = 900;
                mess.Errors = ex.ToString();
                return (null, mess);
            }
        }

        public async Task<(Promotion, Mess)> GetPromotionByIdAsync(int id)
        {
            Mess mess = new Mess();
            try
            {
                var items = await _context.Promotion.GetFullDetailsByIdAsync(id);
                return (items, null);
            }
            catch (Exception ex)
            {
                mess.Status = 900;
                mess.Errors = ex.Message;
                return (null, mess);
            }
        }

        public async Task<(IEnumerable<Promotion>, int total, Mess)> GetSearchPromotionAsync(int skip, int limit,
            string search)
        {
            Mess mess = new Mess();
            try
            {
                var query = _context.Promotion.AsQueryable();
                var totalCount = await query.Where(p =>
                    p.PromotionCode.Contains(search) || p.PromotionDescription.Contains(search) ||
                    p.PromotionName.Contains(search)).CountAsync();
                var items = await query.Where(p =>
                        p.PromotionCode.Contains(search) || p.PromotionDescription.Contains(search) ||
                        p.PromotionName.Contains(search))
                    .Skip(skip * limit).Take(limit)
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
                    .ThenInclude(p => p.PromotionSubUnit)
                    .ToListAsync();
                return (items, totalCount, null);
            }
            catch (Exception ex)
            {
                mess.Status = 900;
                mess.Errors = ex.Message;
                return (null, 0, mess);
            }
        }

        public async Task<(Promotion, Mess)> UpdatePromotionAsync(int id, Promotion model)
        {
            Mess mess = new Mess();
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var promotion = await _context.Promotion.GetFullDetailsByIdAsync(id);
                    if (promotion == null)
                    {
                        mess.Status = 400;
                        mess.Errors = "Không tìm thấy bản ghi cập nhập";
                        return (null, mess);
                    }

                    if (promotion.Id != model.Id)

                    {
                        mess.Status = 400;
                        mess.Errors = "Bản ghi cập nhập không khớp";
                        return (null, mess);
                    }

                    if (promotion.Status == "U")
                    {
                        if (model.PromotionStatus != null)
                            promotion.PromotionStatus = model.PromotionStatus;
                        if (model.PromotionDescription != null)
                            promotion.PromotionDescription = model.PromotionDescription;
                        _context.Promotion.Update(promotion);
                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                        _modelUpdater.UpdateModel(promotion, model, "CreatedDate", "PromotionLine", "PromotionCustomer",
                            "PromotionBrand", "PromotionIndustry", "NA");
                        foreach (var line in model.PromotionLine.ToList())
                        {
                            var existLine = promotion.PromotionLine.FirstOrDefault(p => p.Id == line.Id && p.Id != 0);
                            if (existLine == null)
                            {
                                promotion.PromotionLine.Add(line);
                                ;
                            }
                            else
                            {
                                if (string.IsNullOrEmpty(line.Status)) continue;
                                if (line.Status == "D")
                                {
                                    _context.PromotionLine.Remove(existLine);
                                    promotion.PromotionLine.Remove(existLine);
                                }
                                else if (line.Status == "A")
                                    promotion.PromotionLine.Add(line);
                                else if (line.Status == "U")
                                    _modelUpdater.UpdateModel(existLine, line, "PromotionLineSub", "1", "2", "3", "4",
                                        "NA");

                                foreach (var lineSub in line.PromotionLineSub.ToList())
                                {
                                    var existLineSub =
                                        existLine.PromotionLineSub.FirstOrDefault(p => p.Id == lineSub.Id && p.Id != 0);
                                    if (existLineSub == null)
                                    {
                                        existLine.PromotionLineSub.Add(lineSub);
                                    }
                                    else
                                    {
                                        if (string.IsNullOrEmpty(lineSub.Status))
                                        {
                                        }

                                        if (lineSub.Status == "D")
                                        {
                                            _context.PromotionLineSub.Remove(existLineSub);
                                            existLine.PromotionLineSub.Remove(existLineSub);
                                        }
                                        else if (lineSub.Status == "A")
                                            existLine.PromotionLineSub.Add(lineSub);
                                        else if (lineSub.Status == "U")
                                            _modelUpdater.UpdateModel(existLineSub, lineSub, "PromotionLineSubSub",
                                                "PromotionItemBuy", "PromotionUnit", "3", "4", "NA");

                                        if (lineSub.PromotionItemBuy != null)
                                        {
                                            foreach (var lineItemBuy in lineSub.PromotionItemBuy.ToList())
                                            {
                                                var existLineItemBuy =
                                                    existLineSub.PromotionItemBuy.FirstOrDefault(p =>
                                                        p.Id == lineItemBuy.Id && p.Id != 0);
                                                if (existLineItemBuy == null)
                                                {
                                                    existLineSub.PromotionItemBuy.Add(lineItemBuy);
                                                    ;
                                                }
                                                else
                                                {
                                                    if (string.IsNullOrEmpty(lineItemBuy.Status))
                                                    {
                                                    }

                                                    if (lineItemBuy.Status == "D")
                                                    {
                                                        _context.PromotionItemBuy.Remove(existLineItemBuy);
                                                        existLineSub.PromotionItemBuy.Remove(existLineItemBuy);
                                                    }
                                                    else if (lineItemBuy.Status == "A")
                                                        existLineSub.PromotionItemBuy.Add(lineItemBuy);
                                                    else if (lineItemBuy.Status == "U")
                                                        _modelUpdater.UpdateModel(existLineItemBuy, lineItemBuy,
                                                            "PromotionSubItemAdd", "PromotionSubUnit", "2", "3", "4",
                                                            "NA");
                                                }
                                            }
                                        }

                                        if (lineSub.PromotionUnit != null)
                                        {
                                            foreach (var lineUnit in lineSub.PromotionUnit.ToList())
                                            {
                                                var existLineUnit =
                                                    existLineSub.PromotionUnit.FirstOrDefault(p =>
                                                        p.Id == lineUnit.Id && p.Id != 0);
                                                if (existLineUnit == null)
                                                {
                                                    existLineSub.PromotionUnit.Add(lineUnit);
                                                    ;
                                                }
                                                else
                                                {
                                                    if (string.IsNullOrEmpty(lineUnit.Status))
                                                    {
                                                    }

                                                    if (lineUnit.Status == "D")
                                                    {
                                                        _context.PromotionUnit.Remove(existLineUnit);
                                                        existLineSub.PromotionUnit.Remove(existLineUnit);
                                                    }
                                                    else if (lineUnit.Status == "A")
                                                        existLineSub.PromotionUnit.Add(lineUnit);
                                                    else if (lineUnit.Status == "U")
                                                        _modelUpdater.UpdateModel(existLineUnit, lineUnit,
                                                            "PromotionSubItemAdd", "PromotionSubUnit", "2", "3", "4",
                                                            "NA");
                                                }
                                            }
                                        }

                                        if (lineSub.PromotionLineSubSub != null)
                                        {
                                            foreach (var lineSubSub in lineSub.PromotionLineSubSub.ToList())
                                            {
                                                var existLineSubsub =
                                                    existLineSub.PromotionLineSubSub.FirstOrDefault(p =>
                                                        p.Id == lineSubSub.Id && p.Id != 0);
                                                if (existLineSubsub == null)
                                                {
                                                    existLineSub.PromotionLineSubSub.Add(lineSubSub);
                                                    ;
                                                }
                                                else
                                                {
                                                    if (string.IsNullOrEmpty(lineSubSub.Status))
                                                    {
                                                    }

                                                    if (lineSubSub.Status == "D")
                                                    {
                                                        _context.PromotionLineSubSub.Remove(existLineSubsub);
                                                        existLineSub.PromotionLineSubSub.Remove(existLineSubsub);
                                                    }
                                                    else if (lineSubSub.Status == "A")
                                                        existLineSub.PromotionLineSubSub.Add(lineSubSub);
                                                    else if (lineSubSub.Status == "U")
                                                        _modelUpdater.UpdateModel(existLineSubsub, lineSubSub,
                                                            "PromotionSubItemAdd", "PromotionSubUnit", "2", "3", "4",
                                                            "NA");

                                                    if (lineSubSub.PromotionSubItemAdd != null)
                                                    {
                                                        foreach (var lineSubItemAdd in lineSubSub.PromotionSubItemAdd
                                                                     .ToList())
                                                        {
                                                            var existLineItemAdd =
                                                                existLineSubsub.PromotionSubItemAdd.FirstOrDefault(p =>
                                                                    p.Id == lineSubItemAdd.Id && p.Id != 0);
                                                            if (existLineItemAdd == null)
                                                            {
                                                                existLineSubsub.PromotionSubItemAdd.Add(lineSubItemAdd);
                                                                ;
                                                            }
                                                            else
                                                            {
                                                                if (string.IsNullOrEmpty(lineSubItemAdd.Status))
                                                                {
                                                                }

                                                                if (lineSubItemAdd.Status == "D")
                                                                {
                                                                    _context.PromotionSubItemAdd.Remove(
                                                                        existLineItemAdd);
                                                                    existLineSubsub.PromotionSubItemAdd.Remove(
                                                                        existLineItemAdd);
                                                                }
                                                                else if (lineSubItemAdd.Status == "A")
                                                                    existLineSubsub.PromotionSubItemAdd.Add(
                                                                        lineSubItemAdd);
                                                                else if (lineSubItemAdd.Status == "U")
                                                                    _modelUpdater.UpdateModel(existLineItemAdd,
                                                                        lineSubItemAdd,
                                                                        "PromotionSubItemAdd", "PromotionSubUnit", "2",
                                                                        "3", "4", "NA");
                                                            }
                                                        }
                                                    }

                                                    if (lineSubSub.PromotionSubUnit != null)
                                                    {
                                                        foreach (var lineSubUnit in
                                                                 lineSubSub.PromotionSubUnit.ToList())
                                                        {
                                                            var existLineSubUnit =
                                                                existLineSubsub.PromotionSubUnit.FirstOrDefault(p =>
                                                                    p.Id == lineSubUnit.Id && p.Id != 0);
                                                            if (existLineSubUnit == null)
                                                            {
                                                                existLineSubsub.PromotionSubUnit.Add(lineSubUnit);
                                                            }
                                                            else
                                                            {
                                                                if (string.IsNullOrEmpty(lineSubUnit.Status))
                                                                {
                                                                }

                                                                if (lineSubUnit.Status == "D")
                                                                {
                                                                    _context.PromotionSubUnit.Remove(existLineSubUnit);
                                                                    existLineSubsub.PromotionSubUnit.Remove(
                                                                        existLineSubUnit);
                                                                }
                                                                else if (lineSubUnit.Status == "A")
                                                                    existLineSubsub.PromotionSubUnit.Add(lineSubUnit);
                                                                else if (lineSubUnit.Status == "U")
                                                                    _modelUpdater.UpdateModel(existLineSubUnit,
                                                                        lineSubUnit,
                                                                        "PromotionSubItemAdd", "PromotionSubUnit", "2",
                                                                        "3", "4", "NA");
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        foreach (var line in model.PromotionBrand.ToList())
                        {
                            var existLine = promotion.PromotionBrand.FirstOrDefault(p => p.Id == line.Id && p.Id != 0);
                            if (existLine == null)
                            {
                                promotion.PromotionBrand.Add(line);
                                ;
                            }
                            else
                            {
                                if (string.IsNullOrEmpty(line.Status))
                                {
                                }

                                if (line.Status == "D")
                                {
                                    _context.PromotionBrand.Remove(existLine);
                                    promotion.PromotionBrand.Remove(existLine);
                                }
                                else if (line.Status == "A")
                                    promotion.PromotionBrand.Add(line);
                                else if (line.Status == "U")
                                    _modelUpdater.UpdateModel(existLine, line, "0", "1", "2", "3", "4", "NA");
                            }
                        }

                        foreach (var line in model.PromotionIndustry.ToList())
                        {
                            var existLine =
                                promotion.PromotionIndustry.FirstOrDefault(p => p.Id == line.Id && p.Id != 0);
                            if (existLine == null)
                            {
                                promotion.PromotionIndustry.Add(line);
                                ;
                            }
                            else
                            {
                                if (string.IsNullOrEmpty(line.Status))
                                {
                                }

                                if (line.Status == "D")
                                {
                                    _context.PromotionIndustry.Remove(existLine);
                                    promotion.PromotionIndustry.Remove(existLine);
                                }
                                else if (line.Status == "A")
                                    promotion.PromotionIndustry.Add(line);
                                else if (line.Status == "U")
                                    _modelUpdater.UpdateModel(existLine, line, "0", "1", "2", "3", "4", "NA");
                            }
                        }

                        foreach (var line in model.PromotionCustomer.ToList())
                        {
                            var existLine =
                                promotion.PromotionCustomer.FirstOrDefault(p => p.Id == line.Id && p.Id != 0);
                            if (existLine == null)
                            {
                                promotion.PromotionCustomer.Add(line);
                                ;
                            }
                            else
                            {
                                if (string.IsNullOrEmpty(line.Status))
                                {
                                }

                                if (line.Status == "D")
                                {
                                    _context.PromotionCustomer.Remove(existLine);
                                    promotion.PromotionCustomer.Remove(existLine);
                                }
                                else if (line.Status == "A")
                                    promotion.PromotionCustomer.Add(line);
                                else if (line.Status == "U")
                                    _modelUpdater.UpdateModel(existLine, line, "0", "1", "2", "3", "4", "NA");
                            }
                        }
                    }

                    _context.Promotion.Update(promotion);
                    await _context.SaveChangesAsync();
                    transaction.Commit();
                    return (promotion, null);
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    mess.Status = 900;
                    mess.Errors = ex.Message;
                    return (null, mess);
                }
            }
        }

        public void getPromotionsBirthday(List<Promotion> promotionByCustomer, PromotionParam promotionParam,
            ref PromotionOrder promotionOrder)
        {
            List<PromotionOrderLine> prooderline = new List<PromotionOrderLine>();
            int FlagCust = 0, FlagCust2 = 0;
            var cust = _context.BP.AsNoTracking().Select(e => new { e.Id, e.DateOfBirth })
                .FirstOrDefault(e => e.Id.ToString() == promotionParam.CardId);
            if (cust != null)
            {
                foreach (var promotion in promotionByCustomer.ToList())
                {
                    if (cust.DateOfBirth != null)
                    {
                        if ((promotion.IsPayNow == true) || (promotion.IsCredit == true) ||
                            (promotion.IsCreditGuarantee == true))
                        {
                            List<string> tList = new List<string>();
                            if (promotion.IsPayNow == true)
                                tList.Add("PayNow");
                            if (promotion.IsCredit == true)
                                tList.Add("PayCredit");
                            if (promotion.IsCreditGuarantee == true)
                                tList.Add("PayGuarantee");
                            double beforeDate = 0, afterDate = 0;
                            double.TryParse(promotion.BeforeDay.ToString(), out beforeDate);
                            double.TryParse(promotion.AfterDay.ToString(), out afterDate);
                            int Month = 0, Day = 0;
                            DateTime datessss = (DateTime)cust.DateOfBirth;
                            Day = datessss.Day;
                            Month = datessss.Month;

                            DateTime birthdayThisYear = new DateTime(DateTime.Now.Year, Month, Day);
                            DateTime startDate = DateTime.Now, endDate = DateTime.Now;
                            int Flaga = 0;
                            if (DateTime.Now < birthdayThisYear)
                            {
                                startDate = birthdayThisYear.AddDays(-beforeDate);
                                endDate = birthdayThisYear.AddDays(afterDate);
                            }
                            else if (DateTime.Now > birthdayThisYear.AddDays(afterDate) &&
                                     DateTime.Now.Month == birthdayThisYear.AddDays(afterDate).Month)
                            {
                                startDate = birthdayThisYear.AddDays(-beforeDate);
                                endDate = birthdayThisYear.AddDays(afterDate);
                            }
                            else if (DateTime.Now > birthdayThisYear.AddDays(afterDate))
                            {
                                birthdayThisYear = birthdayThisYear.AddYears(1);
                                startDate = birthdayThisYear.AddDays(-beforeDate);
                                endDate = birthdayThisYear.AddDays(afterDate);
                            }

                            if ((DateTime.Now.Date >= startDate && birthdayThisYear >= DateTime.Now.Date) ||
                                (DateTime.Now.Date <= endDate && birthdayThisYear <= DateTime.Now.Date))
                            {
                                var idb = promotion.PromotionBrand.Select(obj => obj.BrandId).ToList();
                                var idI = promotion.PromotionIndustry.Select(obj => obj.IndustryId).ToList();
                                var itemDss = _context.Item.AsNoTracking().AsSplitQuery().Where(x =>
                                        idb.Contains((int)x.BrandId) &&
                                        idI.Contains((int)x.IndustryId))
                                    .Include(e => e.Packing)
                                    .Include(e => e.ItemType)
                                    .ToList();
                                var prolines = promotion.PromotionLine.Where(e => e.HasException == false).ToList();
                                foreach (var proline in prolines.ToList())
                                {
                                    List<PromotionOrderLineSub> prooderlinesub = new List<PromotionOrderLineSub>();
                                    PromotionOrderLine promotionOrderLine = new PromotionOrderLine();
                                    promotionOrderLine.PromotionId = promotion.Id;
                                    promotionOrderLine.IsOtherPromotion = promotion.IsOtherPromotion;
                                    promotionOrderLine.IsOtherDist = promotion.IsOtherDist;
                                    promotionOrderLine.IsOtherPay = promotion.IsOtherPay;
                                    promotionOrderLine.HasException = promotion.HasException;
                                    promotionOrderLine.IsOtherPromotionExc = promotion.IsOtherPromotionExc;
                                    promotionOrderLine.IsOtherDistExc = promotion.IsOtherDistExc;
                                    promotionOrderLine.IsOtherPayExc = promotion.IsOtherPayExc;
                                    promotionOrderLine.PromotionCode = promotion.PromotionCode;
                                    promotionOrderLine.PromotionDesc = promotion.Note;
                                    promotionOrderLine.PromotionName = promotion.PromotionName;
                                    var promotionLineSubs = proline.PromotionLineSub.ToList();
                                    // Mua hàng tặng hàng
                                    if (proline.SubType == 1)
                                    {
                                        bool checkmin = false, checkIsQty = false;
                                        int Qty = 0;
                                        // Tính cho đơn hàng có mặt hàng khác loại
                                        foreach (var promotionLineSub in promotionLineSubs
                                                     .Where(e => e.IsSameType == true && e.AddType == "R")
                                                     .OrderByDescending(e => e.Quantity).ToList())
                                        {
                                            var idItemType = promotionLineSub.PromotionItemBuy
                                                .Where(e => e.ItemType == "G").Select(obj => obj.ItemId).ToList();
                                            var idItemId = promotionLineSub.PromotionItemBuy
                                                .Where(e => e.ItemType == "I").Select(obj => obj.ItemId).ToList();
                                            var idunit = promotionLineSub.PromotionUnit.Select(obj => obj.UomId)
                                                .ToList();
                                            var iditem = promotionParam.PromotionParamLine
                                                .Where(e => tList.Contains(e.PayMethod)).Select(obj => obj.ItemId)
                                                .ToList();
                                            List<Item> items = null;
                                            items = _context.Item.AsNoTracking().AsSplitQuery()
                                                .Include(e => e.Packing)
                                                .Include(e => e.ItemType)
                                                .Where(x => iditem.Contains((int)x.Id)
                                                            && idb.Contains((int)x.BrandId) &&
                                                            idI.Contains((int)x.IndustryId)
                                                            && idItemType.Contains((int)x.ItemTypeId) &&
                                                            idunit.Contains((int)x.PackingId)
                                                ).ToList();
                                            if (items.Count <= 0)
                                            {
                                                items = _context.Item.AsNoTracking().AsSplitQuery()
                                                    .Include(e => e.Packing)
                                                    .Include(e => e.ItemType)
                                                    .Where(x => iditem.Contains((int)x.Id)
                                                                && idb.Contains((int)x.BrandId) &&
                                                                idI.Contains((int)x.IndustryId)
                                                                && idItemId.Contains((int)x.Id)
                                                    ).ToList();
                                            }

                                            if (items.Count > 0)
                                            {
                                                var iditemx = items.Select(obj => obj.Id).ToList();
                                                Qty = (int)promotionParam.PromotionParamLine
                                                    .Where(x => iditemx.Contains((int)x.ItemId) &&
                                                                tList.Contains(x.PayMethod)).Sum(e => e.Quantity);
                                                var listparams = promotionParam.PromotionParamLine
                                                    .Where(x => iditemx.Contains((int)x.ItemId) && x.Quantity > 0 &&
                                                                tList.Contains(x.PayMethod))
                                                    .ToList();
                                                if (Qty >= promotionLineSub.Quantity)
                                                {
                                                    int FlagQty = 0;
                                                    foreach (var param in listparams)
                                                    {
                                                        param.QuantityRef = param.Quantity;
                                                        var itemasc = items.FirstOrDefault(x => x.Id == param.ItemId);
                                                        PromotionOrderLineSub prolinesub = new PromotionOrderLineSub();
                                                        prolinesub.Cond = "AND";
                                                        prolinesub.InGroup = 0;
                                                        prolinesub.LineId = param.LineId;
                                                        prolinesub.AddAccumulate = proline.AddAccumulate;
                                                        prolinesub.ItemId = itemasc.Id;
                                                        if (itemasc.ItemType.Name.ToUpper()
                                                            .Equals("VẬT PHẨM KHUYẾN MÃI"))
                                                            prolinesub.ItemGroup = "VPKM";
                                                        else
                                                            prolinesub.ItemGroup = "KH";
                                                        prolinesub.ItemCode = itemasc.ItemCode;
                                                        prolinesub.ItemName = itemasc.ItemName;
                                                        prolinesub.PackingId = (int)itemasc.PackingId;
                                                        prolinesub.PackingName = itemasc.Packing.Name;
                                                        if (promotionLineSub.AddType == "R")
                                                        {
                                                            int addqty =
                                                                ((int)param.Quantity / (int)promotionLineSub.AddBuy) *
                                                                promotionLineSub.AddQty;
                                                            FlagQty = ((int)param.Quantity /
                                                                       (int)promotionLineSub.AddBuy);
                                                            prolinesub.QuantityAdd = addqty;
                                                            param.Quantity = param.Quantity -
                                                                             (addqty * (int)promotionLineSub.AddBuy /
                                                                              promotionLineSub.AddQty);
                                                        }

                                                        int and = 0, or = 0, check = 0;
                                                        if (FlagQty > 0)
                                                            foreach (var subsub in promotionLineSub.PromotionLineSubSub
                                                                         .ToList())
                                                            {
                                                                foreach (var itemadd in subsub.PromotionSubItemAdd
                                                                             .ToList())
                                                                {
                                                                    if (subsub.Cond == "AND")
                                                                        and = and + 1;
                                                                    else if (subsub.Cond == "OR")
                                                                        or = or + 1;
                                                                    check = check + 1;

                                                                    PromotionOrderLineSub prolinesub1 =
                                                                        new PromotionOrderLineSub();
                                                                    var subitems = _context.Item.AsNoTracking()
                                                                        .AsSplitQuery()
                                                                        .Where(x => x.Id == itemadd.ItemId)
                                                                        .Include(e => e.ItemType)
                                                                        .Include(e => e.Packing)
                                                                        .FirstOrDefault();
                                                                    prolinesub1.Cond = subsub.Cond;
                                                                    prolinesub1.InGroup = subsub.InGroup;
                                                                    if (subitems.ItemType.Name.ToUpper()
                                                                        .Equals("VẬT PHẨM KHUYẾN MÃI"))
                                                                        prolinesub1.ItemGroup = "VPKM";
                                                                    else
                                                                        prolinesub1.ItemGroup = "KH";
                                                                    prolinesub1.ItemId = subitems.Id;
                                                                    prolinesub1.LineId = param.LineId;
                                                                    prolinesub1.ItemCode = subitems.ItemCode;
                                                                    prolinesub1.ItemName = subitems.ItemName;
                                                                    prolinesub1.PackingId = (int)subitems.PackingId;
                                                                    prolinesub1.PackingName = subitems.Packing.Name;
                                                                    prolinesub1.QuantityAdd = FlagQty * subsub.Quantity;
                                                                    prooderlinesub.Add(prolinesub1);
                                                                }
                                                            }

                                                        if (check > 0 && and > 0)
                                                            prolinesub.Cond = "AND";
                                                        if (check > 0 && and == 0 && or > 0)
                                                            prolinesub.Cond = "OR";
                                                        if (prolinesub.QuantityAdd > 0)
                                                            prooderlinesub.Add(prolinesub);
                                                        FlagQty = 0;
                                                    }
                                                }
                                            }
                                            else
                                                continue;
                                        }

                                        foreach (var promotionLineSub in promotionLineSubs
                                                     .Where(e => e.IsSameType == true && e.AddType == "R")
                                                     .OrderByDescending(e => e.Quantity).ToList())
                                        {
                                            var idItemType = promotionLineSub.PromotionItemBuy
                                                .Where(e => e.ItemType == "G").Select(obj => obj.ItemId).ToList();
                                            var idItemId = promotionLineSub.PromotionItemBuy
                                                .Where(e => e.ItemType == "I").Select(obj => obj.ItemId).ToList();
                                            var idunit = promotionLineSub.PromotionUnit.Select(obj => obj.UomId)
                                                .ToList();
                                            var iditem = promotionParam.PromotionParamLine
                                                .Where(e => tList.Contains(e.PayMethod)).Select(obj => obj.ItemId)
                                                .ToList();
                                            List<Item> items = null;
                                            items = _context.Item
                                                .Include(e => e.Packing)
                                                .Include(e => e.ItemType)
                                                .Where(x => iditem.Contains((int)x.Id)
                                                            && idb.Contains((int)x.BrandId) &&
                                                            idI.Contains((int)x.IndustryId)
                                                            && idItemType.Contains((int)x.ItemTypeId) &&
                                                            idunit.Contains((int)x.PackingId)
                                                ).ToList();
                                            if (items.Count <= 0)
                                            {
                                                items = _context.Item
                                                    .Include(e => e.Packing)
                                                    .Include(e => e.ItemType)
                                                    .Where(x => iditem.Contains((int)x.Id)
                                                                && idb.Contains((int)x.BrandId) &&
                                                                idI.Contains((int)x.IndustryId)
                                                                && idItemId.Contains((int)x.Id)
                                                    ).ToList();
                                            }

                                            if (items.Count > 0)
                                            {
                                                var iditemx = items.Select(obj => obj.Id).ToList();
                                                Qty = (int)promotionParam.PromotionParamLine
                                                    .Where(x => iditemx.Contains((int)x.ItemId) &&
                                                                tList.Contains(x.PayMethod)).Sum(e => e.Quantity);
                                                if (Qty >= promotionLineSub.Quantity)
                                                {
                                                    int QtyFlag = 0;
                                                    var listId = promotionParam.PromotionParamLine
                                                        .Where(x => iditemx.Contains((int)x.ItemId) && x.Quantity > 0 &&
                                                                    tList.Contains(x.PayMethod))
                                                        .Select(e => e.ItemId);
                                                    var itemasc = items.Where(x => listId.Contains(x.Id))
                                                        .OrderBy(e => e.Price).FirstOrDefault();
                                                    var ps = promotionParam.PromotionParamLine.FirstOrDefault(x =>
                                                        x.ItemId == itemasc.Id && tList.Contains(x.PayMethod));
                                                    PromotionOrderLineSub prolinesub = new PromotionOrderLineSub();
                                                    prolinesub.Cond = "AND";
                                                    prolinesub.InGroup = 0;
                                                    prolinesub.LineId = ps.LineId;
                                                    prolinesub.ItemId = itemasc.Id;
                                                    prolinesub.AddAccumulate = proline.AddAccumulate;
                                                    if (itemasc.ItemType.Name.ToUpper().Equals("VẬT PHẨM KHUYẾN MÃI"))
                                                        prolinesub.ItemGroup = "VPKM";
                                                    else
                                                        prolinesub.ItemGroup = "KH";
                                                    prolinesub.ItemCode = itemasc.ItemCode;
                                                    prolinesub.ItemName = itemasc.ItemName;
                                                    prolinesub.PackingId = (int)itemasc.PackingId;
                                                    prolinesub.PackingName = itemasc.Packing.Name;
                                                    while (Qty >= promotionLineSub.Quantity && promotionLineSub.Quantity > 0)
                                                    {
                                                        int addqty = (Qty / (int)promotionLineSub.AddBuy) *
                                                                     promotionLineSub.AddQty;
                                                        prolinesub.QuantityAdd = addqty +
                                                            (int)prolinesub.QuantityAdd.GetValueOrDefault();
                                                        Qty = Qty - promotionLineSub.Quantity * addqty;
                                                        QtyFlag = QtyFlag + addqty;
                                                    }

                                                    int and = 0, or = 0, check = 0;
                                                    if (QtyFlag > 0)
                                                        foreach (var subsub in promotionLineSub.PromotionLineSubSub
                                                                     .ToList())
                                                        {
                                                            foreach (var itemadd in subsub.PromotionSubItemAdd.ToList())
                                                            {
                                                                if (subsub.Cond == "AND")
                                                                    and = and + 1;
                                                                else if (subsub.Cond == "OR")
                                                                    or = or + 1;
                                                                check = check + 1;

                                                                PromotionOrderLineSub prolinesub1 =
                                                                    new PromotionOrderLineSub();
                                                                var subitems = _context.Item
                                                                    .Where(x => x.Id == itemadd.ItemId)
                                                                    .Include(e => e.ItemType)
                                                                    .Include(e => e.Packing)
                                                                    .FirstOrDefault();
                                                                prolinesub1.Cond = subsub.Cond;
                                                                prolinesub1.InGroup = subsub.InGroup;
                                                                if (subitems.ItemType.Name.ToUpper()
                                                                    .Equals("VẬT PHẨM KHUYẾN MÃI"))
                                                                    prolinesub1.ItemGroup = "VPKM";
                                                                else
                                                                    prolinesub1.ItemGroup = "KH";
                                                                prolinesub1.ItemId = subitems.Id;
                                                                prolinesub1.LineId = ps.LineId;
                                                                prolinesub1.AddAccumulate = proline.AddAccumulate;
                                                                prolinesub1.ItemCode = subitems.ItemCode;
                                                                prolinesub1.ItemName = subitems.ItemName;
                                                                prolinesub1.PackingId = (int)subitems.PackingId;
                                                                prolinesub1.PackingName = subitems.Packing.Name;
                                                                prolinesub1.QuantityAdd = QtyFlag * subsub.Quantity;
                                                                prooderlinesub.Add(prolinesub1);
                                                            }
                                                        }

                                                    if (check > 0 && and > 0)
                                                        prolinesub.Cond = "AND";
                                                    if (check > 0 && and == 0 && or > 0)
                                                        prolinesub.Cond = "OR";
                                                    if (prolinesub.QuantityAdd > 0)
                                                    {
                                                        prooderlinesub.Add(prolinesub);
                                                        promotionParam.PromotionParamLine
                                                            .Where(x => iditemx.Contains((int)x.ItemId) &&
                                                                        tList.Contains(x.PayMethod)).ForEach(e =>
                                                                e.Quantity = e.Quantity - promotionLineSub.Quantity);
                                                    }
                                                }
                                                else
                                                    continue;
                                            }
                                            else
                                                continue;
                                        }

                                        foreach (var promotionLineSub in promotionLineSubs
                                                     .Where(e => e.IsSameType == true && e.AddType == "Q")
                                                     .OrderByDescending(e => e.Quantity).ToList())
                                        {
                                            var idItemType = promotionLineSub.PromotionItemBuy
                                                .Where(e => e.ItemType == "G").Select(obj => obj.ItemId).ToList();
                                            var idItemId = promotionLineSub.PromotionItemBuy
                                                .Where(e => e.ItemType == "I").Select(obj => obj.ItemId).ToList();
                                            var idunit = promotionLineSub.PromotionUnit.Select(obj => obj.UomId)
                                                .ToList();
                                            var iditem = promotionParam.PromotionParamLine
                                                .Where(e => tList.Contains(e.PayMethod)).Select(obj => obj.ItemId)
                                                .ToList();
                                            List<Item> items = null;
                                            items = _context.Item.AsNoTracking().AsSplitQuery()
                                                .Include(e => e.Packing)
                                                .Include(e => e.ItemType)
                                                .Where(x => iditem.Contains((int)x.Id)
                                                            && idb.Contains((int)x.BrandId) &&
                                                            idI.Contains((int)x.IndustryId)
                                                            && idItemType.Contains((int)x.ItemTypeId) &&
                                                            idunit.Contains((int)x.PackingId)
                                                ).ToList();
                                            if (items.Count <= 0)
                                            {
                                                items = _context.Item.AsNoTracking().AsSplitQuery()
                                                    .Include(e => e.Packing)
                                                    .Include(e => e.ItemType)
                                                    .Where(x => iditem.Contains((int)x.Id)
                                                                && idb.Contains((int)x.BrandId) &&
                                                                idI.Contains((int)x.IndustryId)
                                                                && idItemId.Contains((int)x.Id)
                                                    ).ToList();
                                            }

                                            if (items.Count < 2)
                                                continue;
                                            var iditemx = items.Select(obj => obj.Id).ToList();
                                            Qty = (int)promotionParam.PromotionParamLine
                                                .Where(x => iditemx.Contains((int)x.ItemId) &&
                                                            tList.Contains(x.PayMethod)).Sum(e => e.Quantity);
                                            var listparams = promotionParam.PromotionParamLine
                                                .Where(x => iditemx.Contains((int)x.ItemId) && x.Quantity > 0 &&
                                                            tList.Contains(x.PayMethod)).ToList();
                                            if (items.Count > 0 && Qty >= promotionLineSub.Quantity &&
                                                checkmin == false)
                                            {
                                                int FlagQty = 0;
                                                foreach (var param in listparams)
                                                {
                                                    if (param.Quantity >= promotionLineSub.Quantity)
                                                    {
                                                        var itemasc = items.FirstOrDefault(x => x.Id == param.ItemId);
                                                        PromotionOrderLineSub prolinesub = new PromotionOrderLineSub();
                                                        prolinesub.Cond = "AND";
                                                        prolinesub.InGroup = 0;
                                                        prolinesub.LineId = param.LineId;
                                                        prolinesub.ItemId = itemasc.Id;
                                                        if (itemasc.ItemType.Name.ToUpper()
                                                            .Equals("VẬT PHẨM KHUYẾN MÃI"))
                                                            prolinesub.ItemGroup = "VPKM";
                                                        else
                                                            prolinesub.ItemGroup = "KH";
                                                        prolinesub.ItemCode = itemasc.ItemCode;
                                                        prolinesub.ItemName = itemasc.ItemName;
                                                        prolinesub.AddAccumulate = proline.AddAccumulate;
                                                        prolinesub.PackingId = (int)itemasc.PackingId;
                                                        prolinesub.PackingName = itemasc.Packing.Name;
                                                        while (param.Quantity >= promotionLineSub.Quantity && promotionLineSub.Quantity > 0)
                                                        {
                                                            prolinesub.QuantityAdd =
                                                                (int)prolinesub.QuantityAdd.GetValueOrDefault() +
                                                                promotionLineSub.AddQty;
                                                            Qty = Qty - promotionLineSub.Quantity;
                                                            FlagQty = FlagQty + 1;
                                                            param.Quantity = param.Quantity -
                                                                             (int)promotionLineSub.Quantity;
                                                        }

                                                        int and = 0, or = 0, check = 0;
                                                        if (FlagQty > 0)
                                                            foreach (var subsub in promotionLineSub.PromotionLineSubSub
                                                                         .ToList())
                                                            {
                                                                foreach (var itemadd in subsub.PromotionSubItemAdd
                                                                             .ToList())
                                                                {
                                                                    if (subsub.Cond == "AND")
                                                                        and = and + 1;
                                                                    else if (subsub.Cond == "OR")
                                                                        or = or + 1;
                                                                    check = check + 1;

                                                                    PromotionOrderLineSub prolinesub1 =
                                                                        new PromotionOrderLineSub();
                                                                    var subitems = _context.Item.AsNoTracking()
                                                                        .AsSplitQuery()
                                                                        .Where(x => x.Id == itemadd.ItemId)
                                                                        .Include(e => e.ItemType)
                                                                        .Include(e => e.Packing)
                                                                        .FirstOrDefault();
                                                                    prolinesub1.Cond = subsub.Cond;
                                                                    prolinesub1.InGroup = subsub.InGroup;
                                                                    prolinesub1.AddAccumulate = proline.AddAccumulate;
                                                                    if (subitems.ItemType.Name.ToUpper()
                                                                        .Equals("VẬT PHẨM KHUYẾN MÃI"))
                                                                        prolinesub1.ItemGroup = "VPKM";
                                                                    else
                                                                        prolinesub1.ItemGroup = "KH";
                                                                    prolinesub1.ItemId = subitems.Id;
                                                                    prolinesub1.LineId = param.LineId;
                                                                    prolinesub1.ItemCode = subitems.ItemCode;
                                                                    prolinesub1.ItemName = subitems.ItemName;
                                                                    prolinesub1.PackingId = (int)subitems.PackingId;
                                                                    prolinesub1.PackingName = subitems.Packing.Name;
                                                                    prolinesub1.QuantityAdd = FlagQty * subsub.Quantity;
                                                                    prooderlinesub.Add(prolinesub1);
                                                                }
                                                            }

                                                        if (check > 0 && and > 0)
                                                            prolinesub.Cond = "AND";
                                                        if (check > 0 && and == 0 && or > 0)
                                                            prolinesub.Cond = "OR";
                                                        if (prolinesub.QuantityAdd > 0)
                                                            prooderlinesub.Add(prolinesub);
                                                        FlagQty = 0;
                                                    }
                                                    else
                                                        continue;
                                                }
                                            }
                                        }

                                        foreach (var promotionLineSub in promotionLineSubs
                                                     .Where(e => e.IsSameType == true && e.AddType == "Q")
                                                     .OrderByDescending(e => e.Quantity).ToList())
                                        {
                                            var idItemType = promotionLineSub.PromotionItemBuy
                                                .Where(e => e.ItemType == "G").Select(obj => obj.ItemId).ToList();
                                            var idItemId = promotionLineSub.PromotionItemBuy
                                                .Where(e => e.ItemType == "I").Select(obj => obj.ItemId).ToList();
                                            var idunit = promotionLineSub.PromotionUnit.Select(obj => obj.UomId)
                                                .ToList();
                                            var iditem = promotionParam.PromotionParamLine
                                                .Where(e => tList.Contains(e.PayMethod)).Select(obj => obj.ItemId)
                                                .ToList();
                                            List<Item> items = null;
                                            items = _context.Item
                                                .Include(e => e.Packing)
                                                .Include(e => e.ItemType)
                                                .Where(x => iditem.Contains((int)x.Id)
                                                            && idb.Contains((int)x.BrandId) &&
                                                            idI.Contains((int)x.IndustryId)
                                                            && idItemType.Contains((int)x.ItemTypeId) &&
                                                            idunit.Contains((int)x.PackingId)
                                                ).ToList();
                                            if (items.Count <= 0)
                                            {
                                                items = _context.Item
                                                    .Include(e => e.Packing)
                                                    .Include(e => e.ItemType)
                                                    .Where(x => iditem.Contains((int)x.Id)
                                                                && idb.Contains((int)x.BrandId) &&
                                                                idI.Contains((int)x.IndustryId)
                                                                && idItemId.Contains((int)x.Id)
                                                    ).ToList();
                                            }

                                            if (items.Count == 0)
                                                continue;
                                            var iditemx = items.Select(obj => obj.Id).ToList();
                                            Qty = (int)promotionParam.PromotionParamLine
                                                .Where(x => iditemx.Contains((int)x.ItemId) &&
                                                            tList.Contains(x.PayMethod)).Sum(e => e.Quantity);
                                            var listparams = promotionParam.PromotionParamLine
                                                .Where(x => iditemx.Contains((int)x.ItemId) && x.Quantity > 0 &&
                                                            tList.Contains(x.PayMethod)).ToList();
                                            if (items.Count > 0 && Qty >= promotionLineSub.Quantity &&
                                                checkmin == false)
                                            {
                                                if (Qty >= promotionLineSub.Quantity)
                                                {
                                                    int QtyFlag = 0;
                                                    var listId = promotionParam.PromotionParamLine
                                                        .Where(x => iditemx.Contains((int)x.ItemId) && x.Quantity > 0 &&
                                                                    tList.Contains(x.PayMethod))
                                                        .Select(e => e.ItemId);
                                                    var itemasc = items.Where(x => listId.Contains(x.Id))
                                                        .OrderBy(e => e.Price).FirstOrDefault();
                                                    var ps = promotionParam.PromotionParamLine.FirstOrDefault(x =>
                                                        x.ItemId == itemasc.Id && tList.Contains(x.PayMethod));
                                                    PromotionOrderLineSub prolinesub = new PromotionOrderLineSub();
                                                    prolinesub.Cond = "AND";
                                                    prolinesub.InGroup = 0;
                                                    prolinesub.LineId = ps.LineId;
                                                    prolinesub.ItemId = itemasc.Id;
                                                    prolinesub.AddAccumulate = proline.AddAccumulate;
                                                    if (itemasc.ItemType.Name.ToUpper().Equals("VẬT PHẨM KHUYẾN MÃI"))
                                                        prolinesub.ItemGroup = "VPKM";
                                                    else
                                                        prolinesub.ItemGroup = "KH";
                                                    prolinesub.ItemCode = itemasc.ItemCode;
                                                    prolinesub.ItemName = itemasc.ItemName;
                                                    prolinesub.PackingId = (int)itemasc.PackingId;
                                                    prolinesub.PackingName = itemasc.Packing.Name;
                                                    while (Qty >= promotionLineSub.Quantity && promotionLineSub.Quantity > 0)
                                                    {
                                                        if (promotionLineSub.AddType == "Q")
                                                        {
                                                            prolinesub.QuantityAdd =
                                                                (int)prolinesub.QuantityAdd.GetValueOrDefault() +
                                                                promotionLineSub.AddQty;
                                                            Qty = Qty - promotionLineSub.Quantity;
                                                            QtyFlag = QtyFlag + 1;
                                                        }
                                                    }

                                                    if (QtyFlag > 0)
                                                    {
                                                        int and = 0, or = 0, check = 0;
                                                        promotionParam.PromotionParamLine
                                                            .Where(x => iditemx.Contains((int)x.ItemId) &&
                                                                        tList.Contains(x.PayMethod))
                                                            .ForEach(e => e.Quantity = 0);
                                                        foreach (var subsub in promotionLineSub.PromotionLineSubSub
                                                                     .ToList())
                                                        {
                                                            foreach (var itemadd in subsub.PromotionSubItemAdd.ToList())
                                                            {
                                                                if (subsub.Cond == "AND")
                                                                    and = and + 1;
                                                                else if (subsub.Cond == "OR")
                                                                    or = or + 1;
                                                                check = check + 1;

                                                                PromotionOrderLineSub prolinesub1 =
                                                                    new PromotionOrderLineSub();
                                                                var subitems = _context.Item
                                                                    .Where(x => x.Id == itemadd.ItemId)
                                                                    .Include(e => e.ItemType)
                                                                    .Include(e => e.Packing)
                                                                    .FirstOrDefault();
                                                                prolinesub1.Cond = subsub.Cond;
                                                                prolinesub1.InGroup = subsub.InGroup;
                                                                if (subitems.ItemType.Name.ToUpper()
                                                                    .Equals("VẬT PHẨM KHUYẾN MÃI"))
                                                                    prolinesub1.ItemGroup = "VPKM";
                                                                else
                                                                    prolinesub1.ItemGroup = "KH";
                                                                prolinesub1.ItemId = subitems.Id;
                                                                prolinesub1.LineId = ps.LineId;
                                                                prolinesub1.AddAccumulate = proline.AddAccumulate;
                                                                prolinesub1.ItemCode = subitems.ItemCode;
                                                                prolinesub1.ItemName = subitems.ItemName;
                                                                prolinesub1.PackingId = (int)subitems.PackingId;
                                                                prolinesub1.PackingName = subitems.Packing.Name;
                                                                prolinesub1.QuantityAdd = QtyFlag * subsub.Quantity;
                                                                prooderlinesub.Add(prolinesub1);
                                                            }
                                                        }

                                                        if (check > 0 && and > 0)
                                                            prolinesub.Cond = "AND";
                                                        if (check > 0 && and == 0 && or > 0)
                                                            prolinesub.Cond = "OR";
                                                        if (prolinesub.QuantityAdd > 0)
                                                            prooderlinesub.Add(prolinesub);
                                                    }
                                                }
                                                else
                                                    continue;
                                            }
                                        }

                                        //Tính cho đơn hàng có mặt hàng cùng loại
                                        foreach (var proItem in promotionParam.PromotionParamLine
                                                     .Where(x => tList.Contains(x.PayMethod)).ToList())
                                        {
                                            Qty = (int)proItem.Quantity;
                                            var itemDs = itemDss.Where(x => x.Id == proItem.ItemId).ToList();
                                            foreach (var promotionLineSub in promotionLineSubs
                                                         .Where(e => e.IsSameType == false)
                                                         .OrderByDescending(e => e.Quantity).ToList())
                                            {
                                                if (Qty < promotionLineSub.Quantity)
                                                    continue;
                                                else
                                                {
                                                    int QtyFlag = 0;
                                                    var idItemType = promotionLineSub.PromotionItemBuy
                                                        .Where(obj => obj.ItemType == "G").Select(obj => obj.ItemId)
                                                        .ToList();
                                                    var idItemId = promotionLineSub.PromotionItemBuy
                                                        .Where(obj => obj.ItemType == "I").Select(obj => obj.ItemId)
                                                        .ToList();
                                                    var itemss = itemDs.Where(x => idItemId.Contains((int)x.Id))
                                                        .FirstOrDefault();
                                                    var idunit = promotionLineSub.PromotionUnit.Select(obj => obj.UomId)
                                                        .ToList();
                                                    Item items = null;
                                                    items = itemDs.Where(x =>
                                                            idItemType.Contains((int)x.ItemTypeId) &&
                                                            idunit.Contains((int)x.PackingId))
                                                        .FirstOrDefault();
                                                    if (items == null)
                                                    {
                                                        items = itemDs.Where(x => idItemId.Contains((int)x.Id))
                                                            .FirstOrDefault();
                                                    }

                                                    if (items != null)
                                                    {
                                                        Qty = (int)proItem.Quantity;
                                                        PromotionOrderLineSub prolinesub = new PromotionOrderLineSub();
                                                        prolinesub.Cond = "AND";
                                                        prolinesub.InGroup = 0;
                                                        prolinesub.LineId = proItem.LineId;
                                                        prolinesub.ItemId = proItem.ItemId;
                                                        if (items.ItemType.Name.Equals("VẬT PHẨM KHUYẾN MÃI"))
                                                            prolinesub.ItemGroup = "VPKM";
                                                        else
                                                            prolinesub.ItemGroup = "KH";
                                                        prolinesub.ItemCode = items.ItemCode;
                                                        prolinesub.AddAccumulate = proline.AddAccumulate;
                                                        prolinesub.ItemName = items.ItemName;
                                                        prolinesub.PackingId = (int)items.PackingId;
                                                        prolinesub.PackingName = items.Packing.Name;
                                                        while (Qty >= promotionLineSub.Quantity && promotionLineSub.Quantity > 0)
                                                        {
                                                            if (promotionLineSub.AddType == "Q")
                                                            {
                                                                prolinesub.QuantityAdd =
                                                                    (int)prolinesub.QuantityAdd.GetValueOrDefault() +
                                                                    promotionLineSub.AddQty;
                                                                Qty = Qty - promotionLineSub.Quantity;
                                                                QtyFlag = QtyFlag + 1;
                                                            }
                                                            else
                                                            {
                                                                int addqty = (Qty / (int)promotionLineSub.AddBuy) *
                                                                             promotionLineSub.AddQty;
                                                                prolinesub.QuantityAdd = addqty +
                                                                    (int)prolinesub.QuantityAdd.GetValueOrDefault();
                                                                Qty = Qty - promotionLineSub.Quantity * addqty;
                                                                QtyFlag = QtyFlag + 1;
                                                            }

                                                            if (prolinesub.QuantityAdd > 0)
                                                                proItem.Quantity = proItem.Quantity -
                                                                    promotionLineSub.Quantity;
                                                        }

                                                        int and = 0, or = 0, check = 0;

                                                        foreach (var subsub in promotionLineSub.PromotionLineSubSub
                                                                     .ToList())
                                                        {
                                                            foreach (var itemadd in subsub.PromotionSubItemAdd)
                                                            {
                                                                if (subsub.Cond == "AND")
                                                                    and = and + 1;
                                                                else if (subsub.Cond == "OR")
                                                                    or = or + 1;
                                                                check = check + 1;

                                                                PromotionOrderLineSub prolinesub1 =
                                                                    new PromotionOrderLineSub();
                                                                var subitems = _context.Item.AsNoTracking()
                                                                    .AsSplitQuery()
                                                                    .Where(x => x.Id == itemadd.ItemId)
                                                                    .Include(e => e.ItemType)
                                                                    .Include(e => e.Packing)
                                                                    .FirstOrDefault();
                                                                prolinesub1.Cond = subsub.Cond;
                                                                prolinesub1.InGroup = subsub.InGroup;
                                                                if (subitems.ItemType.Name.ToUpper()
                                                                    .Equals("VẬT PHẨM KHUYẾN MÃI"))
                                                                    prolinesub.ItemGroup = "VPKM";
                                                                else
                                                                    prolinesub.ItemGroup = "KH";
                                                                prolinesub1.ItemId = subitems.Id;
                                                                prolinesub1.LineId = proItem.LineId;
                                                                prolinesub1.AddAccumulate = proline.AddAccumulate;
                                                                prolinesub1.ItemCode = subitems.ItemCode;
                                                                prolinesub1.ItemName = subitems.ItemName;
                                                                prolinesub1.PackingId = (int)subitems.PackingId;
                                                                prolinesub1.PackingName = subitems.Packing.Name;
                                                                prolinesub1.QuantityAdd = QtyFlag * subsub.Quantity;
                                                                prooderlinesub.Add(prolinesub1);
                                                            }
                                                        }

                                                        if (check > 0 && and > 0)
                                                            prolinesub.Cond = "AND";
                                                        if (check > 0 && and == 0 && or > 0)
                                                            prolinesub.Cond = "OR";
                                                        if (prolinesub.QuantityAdd > 0)
                                                            prooderlinesub.Add(prolinesub);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    else if (proline.SubType == 2)
                                    {
                                        int Flag2 = 0;
                                        if (Flag2 == 0)
                                            promotionParam.PromotionParamLine.Where(x => tList.Contains(x.PayMethod))
                                                .ForEach(item =>
                                                    item.Quantity = item.QuantityRef);
                                        if (FlagCust2 > 0)
                                            promotionParam.PromotionParamLine.Where(x => tList.Contains(x.PayMethod))
                                                .ForEach(item =>
                                                    item.Quantity = item.QuantityRef);
                                        // Theo số lượng khác loại
                                        bool checkmin = false;
                                        int Qty = 0;
                                        foreach (var promotionLineSub in promotionLineSubs
                                                     .Where(e => e.IsSameType == true)
                                                     .OrderByDescending(e => e.Quantity).ToList())
                                        {
                                            var idItemType = promotionLineSub.PromotionItemBuy
                                                .Where(e => e.ItemType == "G").Select(obj => obj.ItemId).ToList();
                                            var idItemId = promotionLineSub.PromotionItemBuy
                                                .Where(e => e.ItemType == "I").Select(obj => obj.ItemId).ToList();
                                            var idunit = promotionLineSub.PromotionUnit.Select(obj => obj.UomId)
                                                .ToList();
                                            var iditem = promotionParam.PromotionParamLine
                                                .Where(x => tList.Contains(x.PayMethod)).Select(obj => obj.ItemId)
                                                .ToList();
                                            List<Item> items = null;
                                            items = _context.Item
                                                .Include(e => e.Packing)
                                                .Include(e => e.ItemType)
                                                .Where(x => iditem.Contains((int)x.Id)
                                                            && idb.Contains((int)x.BrandId) &&
                                                            idI.Contains((int)x.IndustryId)
                                                            && idItemType.Contains((int)x.ItemTypeId) &&
                                                            idunit.Contains((int)x.PackingId)
                                                ).ToList();
                                            if (items.Count <= 0)
                                            {
                                                items = _context.Item
                                                    .Include(e => e.Packing)
                                                    .Include(e => e.ItemType)
                                                    .Where(x => iditem.Contains((int)x.Id)
                                                                && idb.Contains((int)x.BrandId) &&
                                                                idI.Contains((int)x.IndustryId)
                                                                && idItemId.Contains((int)x.Id)
                                                    ).ToList();
                                            }

                                            if (items.Count == 0)
                                                continue;
                                            var iditemx = items.Select(obj => obj.Id).ToList();
                                            Qty = (int)promotionParam.PromotionParamLine
                                                .Where(x => iditemx.Contains((int)x.ItemId) &&
                                                            tList.Contains(x.PayMethod))
                                                .Sum(e => e.Quantity);
                                            var listparams = promotionParam.PromotionParamLine
                                                .Where(x => iditemx.Contains((int)x.ItemId) && x.Quantity > 0 &&
                                                            tList.Contains(x.PayMethod)).ToList();
                                            if (items.Count > 0 && Qty >= promotionLineSub.Quantity &&
                                                checkmin == false)
                                            {
                                                foreach (var param in listparams)
                                                {
                                                    var itemasc = items.FirstOrDefault(x => x.Id == param.ItemId);
                                                    PromotionOrderLineSub prolinesub = new PromotionOrderLineSub();
                                                    prolinesub.Cond = "AND";
                                                    prolinesub.InGroup = 0;
                                                    prolinesub.ItemId = itemasc.Id;
                                                    prolinesub.ItemCode = itemasc.ItemCode;
                                                    prolinesub.LineId = param.LineId;
                                                    prolinesub.ItemName = itemasc.ItemName;
                                                    prolinesub.AddAccumulate = proline.AddAccumulate;
                                                    prolinesub.PackingId = (int)itemasc.PackingId;
                                                    prolinesub.PackingName = itemasc.Packing.Name;
                                                    prolinesub.Discount = promotionLineSub.Discount;
                                                    prolinesub.DiscountType = promotionLineSub.DiscountType;
                                                    prolinesub.PriceType = promotionLineSub.PriceType;
                                                    if (prolinesub.Discount > 0)
                                                    {
                                                        param.Quantity = 0;
                                                        prooderlinesub.Add(prolinesub);
                                                        checkmin = true;
                                                        Flag2 = Flag2 + 1;
                                                    }
                                                }
                                            }
                                        }

                                        // Theo số lượng cùng loại
                                        foreach (var proItem in promotionParam.PromotionParamLine
                                                     .Where(x => tList.Contains(x.PayMethod)).ToList())
                                        {
                                            int QtyAdd = 0;
                                            QtyAdd = (int)proItem.Quantity;
                                            foreach (var promotionLineSub in promotionLineSubs
                                                         .Where(e => e.FollowBy == 1 && e.IsSameType == false)
                                                         .OrderByDescending(e => e.Quantity).ToList())
                                            {
                                                if (QtyAdd >= promotionLineSub.Quantity)
                                                {
                                                    var idItemType = promotionLineSub.PromotionItemBuy
                                                        .Where(e => e.ItemType == "G").Select(obj => obj.ItemId)
                                                        .ToList();
                                                    var idItemId = promotionLineSub.PromotionItemBuy
                                                        .Where(e => e.ItemType == "I").Select(obj => obj.ItemId)
                                                        .ToList();
                                                    var idpacking = promotionLineSub.PromotionUnit
                                                        .Select(obj => obj.UomId)
                                                        .ToList();
                                                    Item itemss = null;
                                                    itemss = _context.Item.AsNoTracking().AsSplitQuery()
                                                        .Include(e => e.Packing)
                                                        .Include(e => e.ItemType)
                                                        .Where(x => x.Id == proItem.ItemId
                                                                    && idItemType.Contains((int)x.ItemTypeId) &&
                                                                    idpacking.Contains((int)x.PackingId)
                                                        ).FirstOrDefault();
                                                    if (itemss == null)
                                                    {
                                                        itemss = _context.Item.AsNoTracking().AsSplitQuery()
                                                            .Include(e => e.Packing)
                                                            .Include(e => e.ItemType)
                                                            .Where(x => x.Id == proItem.ItemId
                                                                        && idItemId.Contains((int)x.Id)
                                                            ).FirstOrDefault();
                                                    }

                                                    if (itemss != null)
                                                    {
                                                        PromotionOrderLineSub prolinesub = new PromotionOrderLineSub();
                                                        prolinesub.Cond = "AND";
                                                        prolinesub.InGroup = 0;
                                                        prolinesub.ItemId = itemss.Id;
                                                        prolinesub.LineId = proItem.LineId;
                                                        prolinesub.ItemCode = itemss.ItemCode;
                                                        prolinesub.AddAccumulate = proline.AddAccumulate;
                                                        prolinesub.ItemName = itemss.ItemName;
                                                        prolinesub.PackingId = (int)itemss.PackingId;
                                                        prolinesub.PackingName = itemss.Packing.Name;
                                                        prolinesub.Discount = promotionLineSub.Discount;
                                                        prolinesub.DiscountType = promotionLineSub.DiscountType;
                                                        prolinesub.PriceType = promotionLineSub.PriceType;
                                                        prooderlinesub.Add(prolinesub);
                                                        Flag2 = Flag2 + 1;
                                                        break;
                                                    }
                                                }
                                                else
                                                    continue;
                                            }

                                            double Flag = 0;
                                            if (Flag == 0)
                                            {
                                                double totalVolumn = 0;
                                                foreach (var promotionLineSub in promotionLineSubs
                                                             .Where(e => e.FollowBy == 2).ToList())
                                                {
                                                    if (promotionParam.OrderDate.Date >=
                                                        promotionLineSub.FromDate.Value.Date &&
                                                        promotionParam.OrderDate.Date <=
                                                        promotionLineSub.ToDate.Value.Date)
                                                    {
                                                        Item itemss = null;
                                                        foreach (var prLine in promotionParam.PromotionParamLine
                                                                     .Where(x => tList.Contains(x.PayMethod))
                                                                     .ToList())
                                                        {
                                                            var idItemType = promotionLineSub.PromotionItemBuy
                                                                .Select(obj => obj.ItemId).ToList();
                                                            var idItemId = promotionLineSub.PromotionItemBuy
                                                                .Where(e => e.ItemType == "I")
                                                                .Select(obj => obj.ItemId).ToList();
                                                            var idpacking = promotionLineSub.PromotionUnit
                                                                .Select(obj => obj.UomId).ToList();
                                                            itemss = _context.Item.AsNoTracking().AsSplitQuery()
                                                                .Include(e => e.Packing)
                                                                .Where(x => x.Id == prLine.ItemId
                                                                            && idItemType.Contains((int)x.ItemTypeId) &&
                                                                            idpacking.Contains((int)x.PackingId)
                                                                ).FirstOrDefault();
                                                            if (itemss == null)
                                                            {
                                                                itemss = _context.Item.AsNoTracking().AsSplitQuery()
                                                                    .Include(e => e.Packing)
                                                                    .Where(x => x.Id == prLine.ItemId
                                                                        && idItemId.Contains((int)x.Id)
                                                                    ).FirstOrDefault();
                                                            }

                                                            if (itemss != null)
                                                                totalVolumn = totalVolumn + (double)prLine.Quantity *
                                                                    (double)itemss.Packing.Volumn;
                                                        }

                                                        Flag = Flag + 1;
                                                    }

                                                    foreach (var prLine in promotionParam.PromotionParamLine
                                                                 .Where(x => tList.Contains(x.PayMethod)).ToList())
                                                    {
                                                        if (totalVolumn >= promotionLineSub.MinVolumn)
                                                        {
                                                            Item itemss = null;
                                                            var idItemType = promotionLineSub.PromotionItemBuy
                                                                .Select(obj => obj.ItemId).ToList();
                                                            var idItemId = promotionLineSub.PromotionItemBuy
                                                                .Where(e => e.ItemType == "I")
                                                                .Select(obj => obj.ItemId).ToList();
                                                            var idpacking = promotionLineSub.PromotionUnit
                                                                .Select(obj => obj.UomId).ToList();
                                                            itemss = _context.Item.AsNoTracking().AsSplitQuery()
                                                                .Include(e => e.Packing)
                                                                .Where(x => x.Id == prLine.ItemId
                                                                            && idItemType.Contains((int)x.ItemTypeId) &&
                                                                            idpacking.Contains((int)x.PackingId)
                                                                ).FirstOrDefault();
                                                            if (itemss == null)
                                                            {
                                                                itemss = _context.Item.AsNoTracking().AsSplitQuery()
                                                                    .Include(e => e.Packing)
                                                                    .Where(x => x.Id == prLine.ItemId
                                                                        && idItemId.Contains((int)x.Id)
                                                                    ).FirstOrDefault();
                                                            }

                                                            PromotionOrderLineSub prolinesub =
                                                                new PromotionOrderLineSub();
                                                            prolinesub.Cond = "AND";
                                                            prolinesub.InGroup = 0;
                                                            prolinesub.ItemId = itemss.Id;
                                                            prolinesub.ItemCode = itemss.ItemCode;
                                                            prolinesub.LineId = prLine.LineId;
                                                            prolinesub.AddAccumulate = proline.AddAccumulate;
                                                            prolinesub.ItemName = itemss.ItemName;
                                                            prolinesub.PackingId = (int)itemss.PackingId;
                                                            prolinesub.PackingName = itemss.Packing.Name;
                                                            prolinesub.Discount = promotionLineSub.Discount;
                                                            prolinesub.DiscountType = promotionLineSub.DiscountType;
                                                            prolinesub.PriceType = promotionLineSub.PriceType;
                                                            prooderlinesub.Add(prolinesub);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    promotionOrderLine.PromotionOrderLineSub = prooderlinesub;
                                    if (promotionOrderLine.PromotionOrderLineSub != null)
                                        prooderline.Add(promotionOrderLine);
                                }

                                var prolineA = prooderline.Where(e => e.PromotionOrderLineSub.Count > 0).ToList();
                                promotionOrder.PromotionOrderLine = prolineA;
                                //if(prolineA.Count > 0)
                                //{
                                //    if (promotionOrder.PromotionOrderLine == null)
                                //        promotionOrder.PromotionOrderLine = prolineA;
                                //    else
                                //        promotionOrder.PromotionOrderLine.AddRange(prolineA);
                                //}   
                                FlagCust = FlagCust + 1;
                                FlagCust2 = FlagCust2 + 1;
                            }
                        }
                    }
                }
            }
        }

        public void getPromotionNews(List<Promotion> promotionByCustomer, PromotionParam promotionParam,
            ref PromotionOrder promotionOrder)
        {
            //try
            //{
            List<PromotionOrderLine> prooderline = new List<PromotionOrderLine>();
            int FlagCust = 0, FlagCust2 = 0;
            foreach (var promotion in promotionByCustomer.ToList())
            {
                promotionParam.PromotionParamLine.ForEach(item => item.Quantity = item.QuantityRef);
                var idb = promotion.PromotionBrand.Select(obj => obj.BrandId).ToList();
                var idI = promotion.PromotionIndustry.Select(obj => obj.IndustryId).ToList();
                var itemDss = _context.Item.AsNoTracking().AsSplitQuery().Where(x => idb.Contains((int)x.BrandId) &&
                        idI.Contains((int)x.IndustryId))
                    .Include(e => e.Packing)
                    .Include(e => e.ItemType)
                    .ToList();
                var prolines = promotion.PromotionLine.Where(e => e.HasException == false).ToList();
                foreach (var proline in prolines.ToList())
                {
                    List<PromotionOrderLineSub> prooderlinesub = new List<PromotionOrderLineSub>();
                    PromotionOrderLine promotionOrderLine = new PromotionOrderLine();
                    promotionOrderLine.PromotionId = promotion.Id;
                    promotionOrderLine.IsOtherPromotion = promotion.IsOtherPromotion;
                    promotionOrderLine.IsOtherDist = promotion.IsOtherDist;
                    promotionOrderLine.IsOtherPay = promotion.IsOtherPay;
                    promotionOrderLine.HasException = false;
                    promotionOrderLine.IsOtherPromotionExc = promotion.IsOtherPromotionExc;
                    promotionOrderLine.IsOtherDistExc = promotion.IsOtherDistExc;
                    promotionOrderLine.IsOtherPayExc = promotion.IsOtherPayExc;
                    promotionOrderLine.PromotionCode = promotion.PromotionCode;
                    promotionOrderLine.PromotionDesc = promotion.Note;
                    promotionOrderLine.PromotionName = promotion.PromotionName;
                    var promotionLineSubs = proline.PromotionLineSub.ToList();
                    // Mua hàng tặng hàng
                    if (proline.SubType == 1)
                    {
                        promotionParam.PromotionParamLine.ForEach(item => item.Quantity = item.QuantityRef);
                        bool checkmin = false, checkIsQty = false;
                        int Qty = 0;
                        //Tính cho đơn hàng có mặt hàng cùng loại
                        foreach (var proItem in promotionParam.PromotionParamLine.ToList())
                        {
                            Qty = (int)proItem.Quantity;
                            var itemDs = itemDss.FirstOrDefault(x => x.Id == proItem.ItemId);
                            if (itemDs != null)
                                foreach (var promotionLineSub in promotionLineSubs.Where(e => e.IsSameType == false)
                                             .OrderByDescending(e => e.Quantity).ToList())
                                {
                                    if (Qty < promotionLineSub.Quantity)
                                        continue;
                                    else
                                    {
                                        int QtyFlag = 0;
                                        var idItemType = promotionLineSub.PromotionItemBuy
                                            .Where(obj => obj.ItemType == "G").Select(obj => obj.ItemId).ToList();
                                        var idItemId = promotionLineSub.PromotionItemBuy
                                            .Where(obj => obj.ItemType == "I").Select(obj => obj.ItemId).ToList();
                                        var idunit = promotionLineSub.PromotionUnit.Select(obj => obj.UomId).ToList();
                                        if ((idItemType.Contains((int)itemDs.ItemTypeId) &&
                                             idunit.Contains((int)itemDs.PackingId)) ||
                                            idItemId.Contains((int)itemDs.Id))
                                        {
                                            Qty = (int)proItem.Quantity;
                                            PromotionOrderLineSub prolinesub = new PromotionOrderLineSub();
                                            prolinesub.Cond = "AND";
                                            prolinesub.InGroup = 0;
                                            prolinesub.LineId = proItem.LineId;
                                            prolinesub.AddAccumulate = proline.AddAccumulate;
                                            prolinesub.ItemId = proItem.ItemId;
                                            if (itemDs.ItemType.Name.Equals("VẬT PHẨM KHUYẾN MÃI"))
                                                prolinesub.ItemGroup = "VPKM";
                                            else
                                                prolinesub.ItemGroup = "KH";
                                            prolinesub.ItemCode = itemDs.ItemCode;
                                            prolinesub.ItemName = itemDs.ItemName;
                                            prolinesub.PackingId = (int)itemDs.PackingId;
                                            prolinesub.PackingName = itemDs.Packing.Name;
                                            while (Qty >= promotionLineSub.Quantity && promotionLineSub.Quantity > 0)
                                            {
                                                if (promotionLineSub.AddType == "Q")
                                                {
                                                    prolinesub.QuantityAdd =
                                                        (int)prolinesub.QuantityAdd.GetValueOrDefault() +
                                                        promotionLineSub.AddQty;
                                                    Qty = Qty - promotionLineSub.Quantity;
                                                    QtyFlag = QtyFlag + 1;
                                                }
                                                else
                                                {
                                                    int addqty =
                                                        (promotionLineSub.Quantity / (int)promotionLineSub.AddBuy) *
                                                        promotionLineSub.AddQty;
                                                    prolinesub.QuantityAdd = addqty +
                                                                             (int)prolinesub.QuantityAdd
                                                                                 .GetValueOrDefault();
                                                    Qty = Qty - promotionLineSub.Quantity;
                                                    QtyFlag = QtyFlag + 1;
                                                }

                                                if (prolinesub.QuantityAdd > 0)
                                                    proItem.Quantity = proItem.Quantity - promotionLineSub.Quantity;
                                            }

                                            int and = 0, or = 0, check = 0;
                                            foreach (var subsub in promotionLineSub.PromotionLineSubSub.ToList())
                                            {
                                                foreach (var itemadd in subsub.PromotionSubItemAdd)
                                                {
                                                    if (subsub.Cond == "AND")
                                                        and = and + 1;
                                                    else if (subsub.Cond == "OR")
                                                        or = or + 1;
                                                    check = check + 1;
                                                    PromotionOrderLineSub prolinesuba = new PromotionOrderLineSub();
                                                    var subitems = _context.Item.AsNoTracking().AsSplitQuery()
                                                        .Where(x => x.Id == itemadd.ItemId)
                                                        .Include(e => e.ItemType)
                                                        .Include(e => e.Packing)
                                                        .FirstOrDefault();
                                                    prolinesuba.Cond = subsub.Cond;
                                                    prolinesuba.InGroup = subsub.InGroup;
                                                    if (subitems.ItemType.Name.ToUpper().Equals("VẬT PHẨM KHUYẾN MÃI"))
                                                        prolinesuba.ItemGroup = "VPKM";
                                                    else
                                                        prolinesuba.ItemGroup = "KH";
                                                    prolinesuba.ItemId = subitems.Id;
                                                    prolinesuba.LineId = proItem.LineId;
                                                    prolinesuba.AddAccumulate = proline.AddAccumulate;
                                                    prolinesuba.ItemCode = subitems.ItemCode;
                                                    prolinesuba.ItemName = subitems.ItemName;
                                                    prolinesuba.PackingId = (int)subitems.PackingId;
                                                    prolinesuba.PackingName = subitems.Packing.Name;
                                                    prolinesuba.QuantityAdd = QtyFlag * subsub.Quantity;
                                                    prooderlinesub.Add(prolinesuba);
                                                }
                                            }

                                            if (check > 0 && and > 0)
                                                prolinesub.Cond = "AND";
                                            if (check > 0 && and == 0 && or > 0)
                                                prolinesub.Cond = "OR";
                                            if (prolinesub.QuantityAdd > 0)
                                                prooderlinesub.Add(prolinesub);
                                        }
                                    }
                                }
                        }

                        // Tính cho đơn hàng có mặt hàng khác loại
                        double TotalQty = 0;
                        foreach (var promotionLineSub in promotionLineSubs.Where(e => e.IsSameType == true)
                                     .OrderByDescending(e => e.Quantity).ToList())
                        {
                            //TotalQty = promotionLineSub.Quantity;
                            var idItemType = promotionLineSub.PromotionItemBuy.Where(e => e.ItemType == "G")
                                .Select(obj => obj.ItemId).ToList();
                            var idItemId = promotionLineSub.PromotionItemBuy.Where(e => e.ItemType == "I")
                                .Select(obj => obj.ItemId).ToList();
                            var idunit = promotionLineSub.PromotionUnit.Select(obj => obj.UomId).ToList();
                            var iditem = promotionParam.PromotionParamLine.Where(e => e.Quantity > 0)
                                .Select(obj => obj.ItemId).ToList();
                            int[] listID = null;
                            List<Item> checkItem = null;
                            bool IsItem = false;
                            if (idItemType.Count > 0)
                            {
                                checkItem = itemDss.Where(e => idItemType.Contains((int)e.ItemTypeId) &&
                                                               idunit.Contains((int)e.PackingId)).ToList();
                            }
                            else
                            {
                                checkItem = itemDss.Where(e => idItemId.Contains((int)e.Id)).ToList();
                                IsItem = true;
                            }

                            if (checkItem.Count > 0 && IsItem == false)
                            {
                                var listcheck = promotionParam.PromotionParamLine.Join(
                                        checkItem,
                                        paramLine => paramLine.ItemId,
                                        item => item.Id,
                                        (paramLine, item) => new
                                        {
                                            paramLine.ItemId,
                                            item.Packing.Type
                                        })
                                    .GroupBy(x => x.Type)
                                    .Select(g => new
                                    {
                                        Type = g.Key,
                                        ItemIds = g.Select(x => x.ItemId).Distinct().ToList()
                                    })
                                    .ToList();
                                foreach (var checkss in listcheck)
                                {
                                    TotalQty = 0;
                                    if (checkss.ItemIds.Count <= 1)
                                        continue;
                                    foreach (var proItem in promotionParam.PromotionParamLine
                                                 .Where(e => e.Quantity > 0 && checkss.ItemIds.Contains(e.ItemId))
                                                 .ToList())
                                    {
                                        var checkItemTypeId = checkItem.Select(obj => obj.ItemTypeId).ToList();
                                        var checkPackingId = checkItem.Select(obj => obj.PackingId).ToList();
                                        var checkOther = itemDss.Where(e =>
                                                checkItemTypeId.Contains(e.ItemTypeId) &&
                                                checkPackingId.Contains(e.PackingId) &&
                                                iditem.Contains(e.Id) && checkss.ItemIds.Contains(e.Id))
                                            .Select(e => e.Id)
                                            .ToList();
                                        var itemParam = promotionParam.PromotionParamLine.Where(e =>
                                            e.Quantity > 0 && checkOther.Contains(e.ItemId) &&
                                            checkss.ItemIds.Contains(e.ItemId));
                                        var minItem = itemDss.OrderBy(e => e.Price).FirstOrDefault(e =>
                                            checkItemTypeId.Contains(e.ItemTypeId) &&
                                            checkPackingId.Contains(e.PackingId) &&
                                            iditem.Contains(e.Id) && checkss.ItemIds.Contains(e.Id));
                                        if (minItem == null)
                                        {
                                            continue;
                                        }

                                        Qty = (int)itemParam.Sum(e => e.Quantity) - (int)TotalQty;
                                        listID = itemParam.Select(e => e.LineId).ToArray();
                                        int QtyFlag = 0;
                                        PromotionOrderLineSub prolinesub = new PromotionOrderLineSub();
                                        prolinesub.Cond = "AND";
                                        prolinesub.InGroup = 0;
                                        prolinesub.LineId = proItem.LineId;
                                        prolinesub.ListLineId = listID;
                                        prolinesub.ItemId = proItem.ItemId;
                                        if (minItem.ItemType.Name.Equals("VẬT PHẨM KHUYẾN MÃI"))
                                            prolinesub.ItemGroup = "VPKM";
                                        else
                                            prolinesub.ItemGroup = "KH";
                                        prolinesub.ItemCode = minItem.ItemCode;
                                        prolinesub.AddAccumulate = proline.AddAccumulate;
                                        prolinesub.ItemName = minItem.ItemName;
                                        prolinesub.PackingId = (int)minItem.PackingId;
                                        prolinesub.PackingName = minItem.Packing.Name;
                                        while (Qty >= promotionLineSub.Quantity && promotionLineSub.Quantity > 0)
                                        {
                                            if (promotionLineSub.AddType == "Q")
                                            {
                                                prolinesub.QuantityAdd =
                                                    (int)prolinesub.QuantityAdd.GetValueOrDefault() +
                                                    promotionLineSub.AddQty;
                                                Qty = Qty - promotionLineSub.Quantity;
                                                TotalQty = TotalQty + promotionLineSub.Quantity;
                                                QtyFlag = QtyFlag + 1;
                                            }
                                            else
                                            {
                                                int addqty = (Qty / (int)promotionLineSub.AddBuy) *
                                                             promotionLineSub.AddQty;
                                                prolinesub.QuantityAdd = addqty +
                                                                         (int)prolinesub.QuantityAdd
                                                                             .GetValueOrDefault();
                                                Qty = Qty - promotionLineSub.Quantity * addqty;
                                                TotalQty = TotalQty + promotionLineSub.Quantity * addqty;
                                                QtyFlag = QtyFlag + 1;
                                            }

                                            //if (prolinesub.QuantityAdd > 0)
                                            //    proItem.Quantity = 0;
                                        }

                                        int and = 0, or = 0, check = 0;
                                        foreach (var subsub in promotionLineSub.PromotionLineSubSub.ToList())
                                        {
                                            if (subsub.Cond == "AND")
                                                and = and + 1;
                                            else if (subsub.Cond == "OR")
                                                or = or + 1;
                                            foreach (var itemadd in subsub.PromotionSubItemAdd)
                                            {
                                                if (QtyFlag * subsub.Quantity > 0)
                                                {
                                                    check = check + 1;
                                                    PromotionOrderLineSub prolinesuba = new PromotionOrderLineSub();
                                                    var subitems = _context.Item.AsNoTracking().AsSplitQuery()
                                                        .Where(x => x.Id == itemadd.ItemId)
                                                        .Include(e => e.ItemType)
                                                        .Include(e => e.Packing)
                                                        .FirstOrDefault();
                                                    prolinesuba.Cond = subsub.Cond;
                                                    prolinesuba.InGroup = subsub.InGroup;
                                                    if (subitems.ItemType.Name.ToUpper().Equals("VẬT PHẨM KHUYẾN MÃI"))
                                                        prolinesuba.ItemGroup = "VPKM";
                                                    else
                                                        prolinesuba.ItemGroup = "KH";
                                                    prolinesuba.ItemId = subitems.Id;
                                                    prolinesuba.AddAccumulate = proline.AddAccumulate;
                                                    prolinesuba.LineId = proItem.LineId;
                                                    prolinesuba.ListLineId = listID;
                                                    prolinesuba.ItemCode = subitems.ItemCode;
                                                    prolinesuba.ItemName = subitems.ItemName;
                                                    prolinesuba.PackingId = (int)subitems.PackingId;
                                                    prolinesuba.PackingName = subitems.Packing.Name;
                                                    prolinesuba.QuantityAdd = QtyFlag * subsub.Quantity;
                                                    prooderlinesub.Add(prolinesuba);
                                                }
                                            }
                                        }

                                        if (check > 0 && and > 0)
                                            prolinesub.Cond = "AND";
                                        if (check > 0 && and == 0 && or > 0)
                                            prolinesub.Cond = "OR";
                                        if (prolinesub.QuantityAdd > 0)
                                        {
                                            prooderlinesub.Add(prolinesub);
                                            break;
                                        }
                                    }
                                }
                            }
                            else if(checkItem.Count >= 2 && IsItem == true)
                            {
                                var listcheck = promotionParam.PromotionParamLine.Join(
                                        checkItem,
                                        paramLine => paramLine.ItemId,
                                        item => item.Id,
                                        (paramLine, item) => new
                                        {
                                            paramLine.ItemId
                                        })
                                    .GroupBy(x => x.ItemId)
                                    .Select(g => new
                                    {
                                        ItemIds = g.Select(x => x.ItemId).Distinct().ToList()
                                    })
                                    .ToList();
                                if(listcheck.Count >= 2)
                                {
                                    var listItem = listcheck.Select(e => e.ItemIds).ToArray();
                                    TotalQty = 0;
                                    //if (checkss.ItemIds.Count <= 1)
                                    //    continue;
                                    foreach (var proItem in promotionParam.PromotionParamLine
                                                 .Where(e => e.Quantity > 0 && listItem.SelectMany(x => x).Contains(e.ItemId))
                                                 .ToList())
                                    {
                                        var checkItemTypeId = checkItem.Select(obj => obj.ItemTypeId).ToList();
                                        var checkPackingId = checkItem.Select(obj => obj.PackingId).ToList();
                                        var checkOther = itemDss.Where(e =>
                                                //checkItemTypeId.Contains(e.ItemTypeId) &&
                                                //checkPackingId.Contains(e.PackingId) &&
                                                iditem.Contains(e.Id) && listItem.SelectMany(x => x).Contains(e.Id))
                                            .Select(e => e.Id)
                                            .ToList();
                                        var itemParam = promotionParam.PromotionParamLine.Where(e =>
                                            e.Quantity > 0 && checkOther.Contains(e.ItemId) &&
                                            listItem.SelectMany(x => x).Contains(e.ItemId));
                                        var minItem = itemDss.OrderBy(e => e.Price).FirstOrDefault(e =>
                                            checkItemTypeId.Contains(e.ItemTypeId) &&
                                            checkPackingId.Contains(e.PackingId) &&
                                            iditem.Contains(e.Id) && listItem.SelectMany(x => x).Contains(e.Id));
                                        if (minItem == null)
                                        {
                                            continue;
                                        }

                                        Qty = (int)itemParam.Sum(e => e.Quantity) - (int)TotalQty;
                                        listID = itemParam.Select(e => e.LineId).ToArray();
                                        int QtyFlag = 0;
                                        PromotionOrderLineSub prolinesub = new PromotionOrderLineSub();
                                        prolinesub.Cond = "AND";
                                        prolinesub.InGroup = 0;
                                        prolinesub.LineId = proItem.LineId;
                                        prolinesub.ListLineId = listID;
                                        prolinesub.ItemId = proItem.ItemId;
                                        if (minItem.ItemType.Name.Equals("VẬT PHẨM KHUYẾN MÃI"))
                                            prolinesub.ItemGroup = "VPKM";
                                        else
                                            prolinesub.ItemGroup = "KH";
                                        prolinesub.ItemCode = minItem.ItemCode;
                                        prolinesub.AddAccumulate = proline.AddAccumulate;
                                        prolinesub.ItemName = minItem.ItemName;
                                        prolinesub.PackingId = (int)minItem.PackingId;
                                        prolinesub.PackingName = minItem.Packing.Name;
                                        while (Qty >= promotionLineSub.Quantity && promotionLineSub.Quantity > 0)
                                        {
                                            if (promotionLineSub.AddType == "Q")
                                            {
                                                prolinesub.QuantityAdd =
                                                    (int)prolinesub.QuantityAdd.GetValueOrDefault() +
                                                    promotionLineSub.AddQty;
                                                Qty = Qty - promotionLineSub.Quantity;
                                                TotalQty = TotalQty + promotionLineSub.Quantity;
                                                QtyFlag = QtyFlag + 1;
                                            }
                                            else
                                            {
                                                int addqty = (Qty / (int)promotionLineSub.AddBuy) *
                                                             promotionLineSub.AddQty;
                                                prolinesub.QuantityAdd = addqty +
                                                                         (int)prolinesub.QuantityAdd
                                                                             .GetValueOrDefault();
                                                Qty = Qty - promotionLineSub.Quantity * addqty;
                                                TotalQty = TotalQty + promotionLineSub.Quantity * addqty;
                                                QtyFlag = QtyFlag + 1;
                                            }

                                            //if (prolinesub.QuantityAdd > 0)
                                            //    proItem.Quantity = 0;
                                        }

                                        int and = 0, or = 0, check = 0;
                                        foreach (var subsub in promotionLineSub.PromotionLineSubSub.ToList())
                                        {
                                            if (subsub.Cond == "AND")
                                                and = and + 1;
                                            else if (subsub.Cond == "OR")
                                                or = or + 1;
                                            foreach (var itemadd in subsub.PromotionSubItemAdd)
                                            {
                                                if (QtyFlag * subsub.Quantity > 0)
                                                {
                                                    check = check + 1;
                                                    PromotionOrderLineSub prolinesuba = new PromotionOrderLineSub();
                                                    var subitems = _context.Item.AsNoTracking().AsSplitQuery()
                                                        .Where(x => x.Id == itemadd.ItemId)
                                                        .Include(e => e.ItemType)
                                                        .Include(e => e.Packing)
                                                        .FirstOrDefault();
                                                    prolinesuba.Cond = subsub.Cond;
                                                    prolinesuba.InGroup = subsub.InGroup;
                                                    if (subitems.ItemType.Name.ToUpper().Equals("VẬT PHẨM KHUYẾN MÃI"))
                                                        prolinesuba.ItemGroup = "VPKM";
                                                    else
                                                        prolinesuba.ItemGroup = "KH";
                                                    prolinesuba.ItemId = subitems.Id;
                                                    prolinesuba.AddAccumulate = proline.AddAccumulate;
                                                    prolinesuba.LineId = proItem.LineId;
                                                    prolinesuba.ListLineId = listID;
                                                    prolinesuba.ItemCode = subitems.ItemCode;
                                                    prolinesuba.ItemName = subitems.ItemName;
                                                    prolinesuba.PackingId = (int)subitems.PackingId;
                                                    prolinesuba.PackingName = subitems.Packing.Name;
                                                    prolinesuba.QuantityAdd = QtyFlag * subsub.Quantity;
                                                    prooderlinesub.Add(prolinesuba);
                                                }
                                            }
                                        }

                                        if (check > 0 && and > 0)
                                            prolinesub.Cond = "AND";
                                        if (check > 0 && and == 0 && or > 0)
                                            prolinesub.Cond = "OR";
                                        if (prolinesub.QuantityAdd > 0)
                                        {
                                            prooderlinesub.Add(prolinesub);
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else if (proline.SubType == 2)
                    {
                        int Flag2 = 0;
                        if (Flag2 == 0)
                            promotionParam.PromotionParamLine.ForEach(item => item.Quantity = item.QuantityRef);
                        if (FlagCust2 > 0)
                            promotionParam.PromotionParamLine.ForEach(item => item.Quantity = item.QuantityRef);
                        bool checkmin = false;
                        int Qty = 0;
                        double Flag = 0;
                        foreach (var proItem in promotionParam.PromotionParamLine.ToList())
                        {
                            Qty = (int)proItem.Quantity;
                            var itemDs = itemDss.FirstOrDefault(x => x.Id == proItem.ItemId);
                            if (itemDs != null)
                                foreach (var promotionLineSub in promotionLineSubs
                                             .Where(e => e.IsSameType == false && e.FollowBy == 1)
                                             .OrderByDescending(e => e.Quantity).ToList())
                                {
                                    if (Qty < promotionLineSub.Quantity)
                                        continue;
                                    else
                                    {
                                        int QtyFlag = 0;
                                        var idItemType = promotionLineSub.PromotionItemBuy
                                            .Where(obj => obj.ItemType == "G").Select(obj => obj.ItemId).ToList();
                                        var idItemId = promotionLineSub.PromotionItemBuy
                                            .Where(obj => obj.ItemType == "I").Select(obj => obj.ItemId).ToList();
                                        var idunit = promotionLineSub.PromotionUnit.Select(obj => obj.UomId).ToList();
                                        if ((idItemType.Contains((int)itemDs.ItemTypeId) &&
                                             idunit.Contains((int)itemDs.PackingId)) ||
                                            idItemId.Contains((int)itemDs.Id))
                                        {
                                            PromotionOrderLineSub prolinesub = new PromotionOrderLineSub();
                                            prolinesub.Cond = "AND";
                                            prolinesub.InGroup = 0;
                                            prolinesub.ItemId = itemDs.Id;
                                            prolinesub.LineId = proItem.LineId;
                                            prolinesub.ItemCode = itemDs.ItemCode;
                                            prolinesub.AddAccumulate = proline.AddAccumulate;
                                            prolinesub.ItemName = itemDs.ItemName;
                                            prolinesub.PackingId = (int)itemDs.PackingId;
                                            prolinesub.PackingName = itemDs.Packing.Name;
                                            prolinesub.Discount = promotionLineSub.Discount;
                                            prolinesub.DiscountType = promotionLineSub.DiscountType;
                                            prolinesub.PriceType = promotionLineSub.PriceType;
                                            proItem.Quantity = 0;
                                            prooderlinesub.Add(prolinesub);
                                            Flag2 = Flag2 + 1;
                                            break;
                                        }
                                    }
                                }

                            if (Flag == 0)
                            {
                                int[] listID1 = null;
                                double totalVolumn = 0;

                                List<MinValue> lm = new List<MinValue>();
                                foreach (var promotionLineSub in promotionLineSubs.Where(e => e.FollowBy == 2)
                                             .OrderByDescending(e => e.MinVolumn).ToList())
                                {
                                    MinValue m = new MinValue();
                                    m.Min = promotionLineSub.MinVolumn ?? 0;
                                    m.count = promotionLineSubs.Where(e => e.FollowBy == 2 && e.MinVolumn == m.Min)
                                        .Count();
                                    var itemid = promotionLineSub.PromotionItemBuy.Where(obj => obj.ItemType == "I")
                                        .Select(obj => obj.ItemId).ToList();
                                    if (itemid.Count <= 0)
                                    {
                                        var ItemTypeId = promotionLineSub.PromotionItemBuy
                                            .Where(obj => obj.ItemType == "G").Select(obj => obj.ItemId).ToList();
                                        itemid = _context.Item.Where(e => ItemTypeId.Contains(e.ItemTypeId ?? 0))
                                            .Select(e => e.Id).ToList();
                                    }

                                    string itemIdsString = string.Join(",", itemid);
                                    m.ItemId = itemIdsString;
                                    var uomid = promotionLineSub.PromotionUnit.Select(obj => obj.UomId).ToList();
                                    string uomidString = string.Join(",", uomid);
                                    m.UomId = uomidString;

                                    m.count = promotionLineSubs.Where(e => e.FollowBy == 2 && e.MinVolumn == m.Min)
                                        .Count();
                                    m.refCount = m.count;
                                    lm.Add(m);
                                    var uomidList = uomidString.Split(",");
                                    if (lm.Any(e => e.ItemId == itemIdsString && e.count <= 0))
                                        continue;
                                    totalVolumn = 0;
                                    DateTime FromDate;
                                    DateTime ToDate;
                                    if (promotionLineSub.FromDate != null)
                                        FromDate = promotionLineSub.FromDate.Value.Date;
                                    else
                                        FromDate = DateTime.MinValue.Date;
                                    if (promotionLineSub.ToDate != null)
                                        ToDate = promotionLineSub.ToDate.Value.Date;
                                    else
                                        ToDate = DateTime.MaxValue.Date;

                                    if (promotionParam.OrderDate.Date >= FromDate &&
                                        promotionParam.OrderDate.Date <= ToDate)
                                    {
                                        Item itemss = null;
                                        foreach (var prLine in promotionParam.PromotionParamLine.ToList())
                                        {
                                            var idItemType = promotionLineSub.PromotionItemBuy
                                                .Select(obj => obj.ItemId).ToList();
                                            var idItemId = promotionLineSub.PromotionItemBuy
                                                .Where(e => e.ItemType == "I")
                                                .Select(obj => obj.ItemId).ToList();
                                            var idpacking = promotionLineSub.PromotionUnit
                                                .Select(obj => obj.UomId).ToList();
                                            itemss = itemDss.Where(x => x.Id == prLine.ItemId
                                                                        && idItemType.Contains((int)x.ItemTypeId) &&
                                                                        idpacking.Contains((int)x.PackingId)
                                            ).FirstOrDefault();
                                            if (itemss == null)
                                            {
                                                itemss = itemDss.Where(x => x.Id == prLine.ItemId
                                                                            && idItemId.Contains((int)x.Id)
                                                ).FirstOrDefault();
                                            }

                                            if (itemss != null)
                                            {
                                                totalVolumn = totalVolumn + (double)prLine.Quantity *
                                                    (double)itemss.Packing.Volumn;
                                                if (listID1 == null)
                                                    listID1 = new int[] { prLine.LineId };
                                                else
                                                    listID1.Append(prLine.LineId);
                                            }
                                        }
                                    }


                                        if (totalVolumn >= promotionLineSub.MinVolumn )
                                        {
                                        var check = lm.FirstOrDefault(e => e.count < e.refCount && e.ItemId == itemIdsString && (e.UomId.Split(",").Any(u => uomidList.Contains(u)) || uomidList.Any(u => e.UomId.Split(',').Contains(u))));
                                        if(check ==  null)
                                         {
                                            foreach (var prLine in promotionParam.PromotionParamLine.ToList())
                                                {
                                                    Item itemss = null;
                                                    var idItemType = promotionLineSub.PromotionItemBuy
                                                        .Select(obj => obj.ItemId).ToList();
                                                    var idItemId = promotionLineSub.PromotionItemBuy
                                                        .Where(e => e.ItemType == "I")
                                                        .Select(obj => obj.ItemId).ToList();
                                                    var idpacking = promotionLineSub.PromotionUnit
                                                        .Select(obj => obj.UomId).ToList();
                                                    itemss = itemDss.Where(x => x.Id == prLine.ItemId
                                                                    && idItemType.Contains((int)x.ItemTypeId) &&
                                                                    idpacking.Contains((int)x.PackingId)
                                                        ).FirstOrDefault();
                                                    if (itemss == null)
                                                    {
                                                        itemss = itemDss.Where(x => x.Id == prLine.ItemId
                                                                        && idItemId.Contains((int)x.Id)
                                                            ).FirstOrDefault();
                                                    }
                                            //
                                                    if (itemss != null)
                                                    {
                                                        PromotionOrderLineSub prolinesub = new PromotionOrderLineSub();
                                                        prolinesub.Cond = "AND";
                                                        prolinesub.InGroup = 0;
                                                        prolinesub.ItemId = itemss.Id;
                                                        prolinesub.ItemCode = itemss.ItemCode;
                                                        prolinesub.LineId = prLine.LineId;
                                                        prolinesub.ListLineId = listID1;
                                                        prolinesub.ItemName = itemss.ItemName;
                                                        prolinesub.AddAccumulate = proline.AddAccumulate;
                                                        prolinesub.PackingId = (int)itemss.PackingId;
                                                        prolinesub.PackingName = itemss.Packing.Name;
                                                        prolinesub.Discount = promotionLineSub.Discount;
                                                        prolinesub.DiscountType = promotionLineSub.DiscountType;
                                                        prolinesub.PriceType = promotionLineSub.PriceType;
                                                        prooderlinesub.Add(prolinesub);
                                                        var uomidList1 = uomidString.Split(",");
                                                        lm.Where(e => e.Min == (promotionLineSub.MinVolumn ?? 0) && e.ItemId == itemIdsString && (e.UomId.Split(",").Any(u => uomidList.Contains(u)) || uomidList.Any(u => e.UomId.Split(',').Contains(u)))).ForEach(e => e.count = e.count - 1);
                                                    }
                                                }
                                         } 
                                            
                                            var valid = lm.FirstOrDefault(e => e.count < e.refCount && e.Min == (promotionLineSub.MinVolumn ?? 0) && e.ItemId == itemIdsString && (e.UomId.Split(",").Any(u => uomidList.Contains(u)) || uomidList.Any(u => e.UomId.Split(',').Contains(u))));
                                            if (valid != null)
                                                Flag = Flag + 1;
                                            totalVolumn = 0;                            
                                        }
                                    }
                                }
                            }
                            // Tính cho đơn hàng có mặt hàng khác loại
                            foreach (var promotionLineSub in promotionLineSubs.Where(e => e.IsSameType == true && e.FollowBy == 1).OrderByDescending(e => e.Quantity).ToList())
                            {
                                var idItemType = promotionLineSub.PromotionItemBuy.Where(e => e.ItemType == "G")
                                    .Select(obj => obj.ItemId).ToList();
                                var idItemId = promotionLineSub.PromotionItemBuy.Where(e => e.ItemType == "I")
                                    .Select(obj => obj.ItemId).ToList();
                                var idunit = promotionLineSub.PromotionUnit.Select(obj => obj.UomId).ToList();
                                var iditem = promotionParam.PromotionParamLine.Where(e => e.Quantity > 0)
                                    .Select(obj => obj.ItemId).ToList();
                                foreach (var proItem in promotionParam.PromotionParamLine.Where(e => e.Quantity > 0)
                                             .ToList())
                                {
                                    Item checkItem = null;
                                    if (idItemType.Count > 0)
                                    {
                                        checkItem = itemDss.FirstOrDefault(e =>
                                            e.Id == proItem.ItemId && idItemType.Contains((int)e.ItemTypeId) &&
                                            idunit.Contains((int)e.PackingId));
                                    }
                                    else
                                    {
                                        checkItem = itemDss.FirstOrDefault(e =>
                                            e.Id == proItem.ItemId && idItemId.Contains((int)e.Id));
                                    }

                                if (checkItem != null)
                                {
                                    var checkOther = itemDss.Where(e =>
                                        e.ItemTypeId == checkItem.ItemTypeId && e.PackingId == checkItem.PackingId &&
                                        iditem.Contains(e.Id)).Select(e => e.Id).ToList();
                                    var itemParam = promotionParam.PromotionParamLine.Where(e =>
                                        e.Quantity > 0 && checkOther.Contains(e.ItemId));
                                    var minItem = itemDss.OrderBy(e => e.Price).FirstOrDefault(e =>
                                        e.ItemTypeId == checkItem.ItemTypeId && e.PackingId == checkItem.PackingId &&
                                        iditem.Contains(e.Id));
                                    Qty = (int)itemParam.Sum(e => e.Quantity);
                                    int QtyFlag = 0;
                                    if (Qty >= promotionLineSub.Quantity)
                                    {
                                        PromotionOrderLineSub prolinesub = new PromotionOrderLineSub();
                                        prolinesub.Cond = "AND";
                                        prolinesub.InGroup = 0;
                                        prolinesub.ItemId = minItem.Id;
                                        prolinesub.LineId = proItem.LineId;
                                        prolinesub.ItemCode = minItem.ItemCode;
                                        prolinesub.ItemName = minItem.ItemName;
                                        prolinesub.AddAccumulate = proline.AddAccumulate;
                                        prolinesub.PackingId = (int)minItem.PackingId;
                                        prolinesub.PackingName = minItem.Packing.Name;
                                        prolinesub.Discount = promotionLineSub.Discount;
                                        prolinesub.DiscountType = promotionLineSub.DiscountType;
                                        prolinesub.PriceType = promotionLineSub.PriceType;
                                        proItem.Quantity = 0;
                                        prooderlinesub.Add(prolinesub);
                                    }
                                }
                                else
                                    continue;
                            }
                        }

                        promotionOrderLine.PromotionOrderLineSub = prooderlinesub;
                        if (promotionOrderLine.PromotionOrderLineSub != null)
                            prooderline.Add(promotionOrderLine);
                    }

                    promotionOrderLine.PromotionOrderLineSub = prooderlinesub;
                    if (promotionOrderLine.PromotionOrderLineSub != null)
                        prooderline.Add(promotionOrderLine);
                }

                var prolinesEx = promotion.PromotionLine.Where(e => e.HasException == true).ToList();
                if (prooderline.FirstOrDefault(e => e.PromotionCode == promotion.PromotionCode) != null)
                    if (prooderline.FirstOrDefault(e => e.PromotionCode == promotion.PromotionCode)
                            .PromotionOrderLineSub != null)
                        if (prooderline.FirstOrDefault(e => e.PromotionCode == promotion.PromotionCode)
                                .PromotionOrderLineSub.Count() < 1)
                            foreach (var proline in prolinesEx.ToList())
                            {
                                List<PromotionOrderLineSub> prooderlinesub = new List<PromotionOrderLineSub>();
                                PromotionOrderLine promotionOrderLine = new PromotionOrderLine();
                                promotionOrderLine.PromotionId = promotion.Id;
                                promotionOrderLine.IsOtherPromotion = promotion.IsOtherPromotion;
                                promotionOrderLine.IsOtherDist = promotion.IsOtherDist;
                                promotionOrderLine.IsOtherPay = promotion.IsOtherPay;
                                promotionOrderLine.HasException = true;
                                promotionOrderLine.IsOtherPromotionExc = promotion.IsOtherPromotionExc;
                                promotionOrderLine.IsOtherDistExc = promotion.IsOtherDistExc;
                                promotionOrderLine.IsOtherPayExc = promotion.IsOtherPayExc;
                                promotionOrderLine.PromotionCode = promotion.PromotionCode;
                                promotionOrderLine.PromotionDesc = promotion.Note;
                                promotionOrderLine.PromotionName = promotion.PromotionName;
                                var promotionLineSubs = proline.PromotionLineSub.ToList();
                                // Mua hàng tặng hàng
                                if (proline.SubType == 1)
                                {
                                    bool checkmin = false, checkIsQty = false;
                                    int Qty = 0;
                                    //Tính cho đơn hàng có mặt hàng cùng loại
                                    foreach (var proItem in promotionParam.PromotionParamLine.ToList())
                                    {
                                        Qty = (int)proItem.Quantity;
                                        var itemDs = itemDss.FirstOrDefault(x => x.Id == proItem.ItemId);
                                        if (itemDs != null)
                                            foreach (var promotionLineSub in promotionLineSubs
                                                         .Where(e => e.IsSameType == false)
                                                         .OrderByDescending(e => e.Quantity).ToList())
                                            {
                                                if (Qty > promotionLineSub.Quantity)
                                                    continue;
                                                else
                                                {
                                                    int QtyFlag = 0;
                                                    var idItemType = promotionLineSub.PromotionItemBuy
                                                        .Where(obj => obj.ItemType == "G").Select(obj => obj.ItemId)
                                                        .ToList();
                                                    var idItemId = promotionLineSub.PromotionItemBuy
                                                        .Where(obj => obj.ItemType == "I").Select(obj => obj.ItemId)
                                                        .ToList();
                                                    var idunit = promotionLineSub.PromotionUnit.Select(obj => obj.UomId)
                                                        .ToList();
                                                    if ((idItemType.Contains((int)itemDs.ItemTypeId) &&
                                                         idunit.Contains((int)itemDs.PackingId)) ||
                                                        idItemId.Contains((int)itemDs.Id))
                                                    {
                                                        Qty = (int)proItem.Quantity;
                                                        PromotionOrderLineSub prolinesub = new PromotionOrderLineSub();
                                                        prolinesub.Cond = "AND";
                                                        prolinesub.InGroup = 0;
                                                        prolinesub.LineId = proItem.LineId;
                                                        prolinesub.AddAccumulate = proline.AddAccumulate;
                                                        prolinesub.ItemId = proItem.ItemId;
                                                        if (itemDs.ItemType.Name.Equals("VẬT PHẨM KHUYẾN MÃI"))
                                                            prolinesub.ItemGroup = "VPKM";
                                                        else
                                                            prolinesub.ItemGroup = "KH";
                                                        prolinesub.ItemCode = itemDs.ItemCode;
                                                        prolinesub.ItemName = itemDs.ItemName;
                                                        prolinesub.PackingId = (int)itemDs.PackingId;
                                                        prolinesub.PackingName = itemDs.Packing.Name;
                                                        while (Qty < promotionLineSub.Quantity && Qty > 0)
                                                        {
                                                            if (promotionLineSub.AddType == "Q")
                                                            {
                                                                prolinesub.QuantityAdd =
                                                                    (int)prolinesub.QuantityAdd.GetValueOrDefault() +
                                                                    promotionLineSub.AddQty;
                                                                Qty = Qty + promotionLineSub.Quantity;
                                                                QtyFlag = QtyFlag + 1;
                                                            }
                                                            else
                                                            {
                                                                int addqty =
                                                                    (promotionLineSub.Quantity /
                                                                     (int)promotionLineSub.AddBuy) *
                                                                    promotionLineSub.AddQty;
                                                                prolinesub.QuantityAdd = addqty +
                                                                    (int)prolinesub.QuantityAdd
                                                                        .GetValueOrDefault();
                                                                Qty = Qty + promotionLineSub.Quantity;
                                                                QtyFlag = QtyFlag + 1;
                                                            }

                                                            if (prolinesub.QuantityAdd > 0)
                                                                proItem.Quantity = proItem.Quantity -
                                                                    promotionLineSub.Quantity;
                                                        }

                                                        int and = 0, or = 0, check = 0;
                                                        foreach (var subsub in promotionLineSub.PromotionLineSubSub
                                                                     .ToList())
                                                        {
                                                            foreach (var itemadd in subsub.PromotionSubItemAdd)
                                                            {
                                                                if (subsub.Cond == "AND")
                                                                    and = and + 1;
                                                                else if (subsub.Cond == "OR")
                                                                    or = or + 1;
                                                                check = check + 1;
                                                                PromotionOrderLineSub prolinesuba =
                                                                    new PromotionOrderLineSub();
                                                                var subitems = _context.Item.AsNoTracking()
                                                                    .AsSplitQuery()
                                                                    .Where(x => x.Id == itemadd.ItemId)
                                                                    .Include(e => e.ItemType)
                                                                    .Include(e => e.Packing)
                                                                    .FirstOrDefault();
                                                                prolinesuba.Cond = subsub.Cond;
                                                                prolinesuba.InGroup = subsub.InGroup;
                                                                if (subitems.ItemType.Name.ToUpper()
                                                                    .Equals("VẬT PHẨM KHUYẾN MÃI"))
                                                                    prolinesuba.ItemGroup = "VPKM";
                                                                else
                                                                    prolinesuba.ItemGroup = "KH";
                                                                prolinesuba.ItemId = subitems.Id;
                                                                prolinesuba.LineId = proItem.LineId;
                                                                prolinesuba.AddAccumulate = proline.AddAccumulate;
                                                                prolinesuba.ItemCode = subitems.ItemCode;
                                                                prolinesuba.ItemName = subitems.ItemName;
                                                                prolinesuba.PackingId = (int)subitems.PackingId;
                                                                prolinesuba.PackingName = subitems.Packing.Name;
                                                                prolinesuba.QuantityAdd = QtyFlag * subsub.Quantity;
                                                                prooderlinesub.Add(prolinesuba);
                                                            }
                                                        }

                                                        if (check > 0 && and > 0)
                                                            prolinesub.Cond = "AND";
                                                        if (check > 0 && and == 0 && or > 0)
                                                            prolinesub.Cond = "OR";
                                                        if (prolinesub.QuantityAdd > 0)
                                                            prooderlinesub.Add(prolinesub);
                                                    }
                                                }
                                            }
                                    }

                                    // Tính cho đơn hàng có mặt hàng khác loại
                                    double TotalQty = 0;
                                    foreach (var promotionLineSub in promotionLineSubs.Where(e => e.IsSameType == true)
                                                 .OrderByDescending(e => e.Quantity).ToList())
                                    {
                                        var idItemType = promotionLineSub.PromotionItemBuy.Where(e => e.ItemType == "G")
                                            .Select(obj => obj.ItemId).ToList();
                                        var idItemId = promotionLineSub.PromotionItemBuy.Where(e => e.ItemType == "I")
                                            .Select(obj => obj.ItemId).ToList();
                                        var idunit = promotionLineSub.PromotionUnit.Select(obj => obj.UomId).ToList();
                                        var iditem = promotionParam.PromotionParamLine.Where(e => e.Quantity > 0)
                                            .Select(obj => obj.ItemId).ToList();
                                        List<Item> checkItem = null;
                                        if (idItemType.Count > 0)
                                        {
                                            checkItem = itemDss.Where(e => idItemType.Contains((int)e.ItemTypeId) &&
                                                                           idunit.Contains((int)e.PackingId)).ToList();
                                        }
                                        else
                                        {
                                            checkItem = itemDss.Where(e => idItemId.Contains((int)e.Id)).ToList();
                                        }

                                        if (checkItem.Count > 0)
                                        {
                                            var listcheck = promotionParam.PromotionParamLine.Join(
                                                    checkItem,
                                                    paramLine => paramLine.ItemId,
                                                    item => item.Id,
                                                    (paramLine, item) => new
                                                    {
                                                        paramLine.ItemId,
                                                        item.Packing.Type
                                                    })
                                                .GroupBy(x => x.Type)
                                                .Select(g => new
                                                {
                                                    Type = g.Key,
                                                    ItemIds = g.Select(x => x.ItemId).Distinct().ToList()
                                                })
                                                .ToList();
                                            foreach (var checkss in listcheck)
                                            {
                                                if (checkss.ItemIds.Count <= 1)
                                                    continue;
                                                foreach (var proItem in promotionParam.PromotionParamLine
                                                             .Where(e => e.Quantity > 0 &&
                                                                         checkss.ItemIds.Contains(e.ItemId)).ToList())
                                                {
                                                    var checkItemTypeId =
                                                        checkItem.Select(obj => obj.ItemTypeId).ToList();
                                                    var checkPackingId =
                                                        checkItem.Select(obj => obj.PackingId).ToList();
                                                    var checkOther = itemDss.Where(e =>
                                                            checkItemTypeId.Contains(e.ItemTypeId) &&
                                                            checkPackingId.Contains(e.PackingId) &&
                                                            iditem.Contains(e.Id) && checkss.ItemIds.Contains(e.Id))
                                                        .Select(e => e.Id).ToList();
                                                    var itemParam = promotionParam.PromotionParamLine.Where(e =>
                                                        e.Quantity > 0 && checkOther.Contains(e.ItemId) &&
                                                        checkss.ItemIds.Contains(e.ItemId));
                                                    var minItem = itemDss.OrderBy(e => e.Price).FirstOrDefault(e =>
                                                        checkItemTypeId.Contains(e.ItemTypeId) &&
                                                        checkPackingId.Contains(e.PackingId) &&
                                                        iditem.Contains(e.Id) && checkss.ItemIds.Contains(e.Id));
                                                    Qty = (int)itemParam.Sum(e => e.Quantity);
                                                    int QtyFlag = 0;
                                                    PromotionOrderLineSub prolinesub = new PromotionOrderLineSub();
                                                    prolinesub.Cond = "AND";
                                                    prolinesub.InGroup = 0;
                                                    prolinesub.LineId = proItem.LineId;
                                                    prolinesub.ItemId = proItem.ItemId;
                                                    if (minItem.ItemType.Name.Equals("VẬT PHẨM KHUYẾN MÃI"))
                                                        prolinesub.ItemGroup = "VPKM";
                                                    else
                                                        prolinesub.ItemGroup = "KH";
                                                    prolinesub.ItemCode = minItem.ItemCode;
                                                    prolinesub.AddAccumulate = proline.AddAccumulate;
                                                    prolinesub.ItemName = minItem.ItemName;
                                                    prolinesub.PackingId = (int)minItem.PackingId;
                                                    prolinesub.PackingName = minItem.Packing.Name;
                                                    while (Qty < promotionLineSub.Quantity && Qty > 0)
                                                    {
                                                        if (promotionLineSub.AddType == "Q")
                                                        {
                                                            prolinesub.QuantityAdd =
                                                                (int)prolinesub.QuantityAdd.GetValueOrDefault() +
                                                                promotionLineSub.AddQty;
                                                            Qty = Qty - promotionLineSub.Quantity;
                                                            QtyFlag = QtyFlag + 1;
                                                        }
                                                        else
                                                        {
                                                            int addqty = (Qty / (int)promotionLineSub.AddBuy) *
                                                                         promotionLineSub.AddQty;
                                                            prolinesub.QuantityAdd = addqty +
                                                                (int)prolinesub.QuantityAdd.GetValueOrDefault();
                                                            Qty = Qty - promotionLineSub.Quantity;
                                                            QtyFlag = QtyFlag + 1;
                                                        }

                                                        if (prolinesub.QuantityAdd > 0)
                                                            proItem.Quantity = 0;
                                                    }

                                                    int and = 0, or = 0, check = 0;
                                                    foreach (var subsub in promotionLineSub.PromotionLineSubSub
                                                                 .ToList())
                                                    {
                                                        foreach (var itemadd in subsub.PromotionSubItemAdd)
                                                        {
                                                            if (QtyFlag * subsub.Quantity > 0)
                                                            {
                                                                if (subsub.Cond == "AND")
                                                                    and = and + 1;
                                                                else if (subsub.Cond == "OR")
                                                                    or = or + 1;
                                                                check = check + 1;
                                                                PromotionOrderLineSub prolinesuba =
                                                                    new PromotionOrderLineSub();
                                                                var subitems = _context.Item.AsNoTracking()
                                                                    .AsSplitQuery()
                                                                    .Where(x => x.Id == itemadd.ItemId)
                                                                    .Include(e => e.ItemType)
                                                                    .Include(e => e.Packing)
                                                                    .FirstOrDefault();
                                                                prolinesuba.Cond = subsub.Cond;
                                                                prolinesuba.InGroup = subsub.InGroup;
                                                                if (subitems.ItemType.Name.ToUpper()
                                                                    .Equals("VẬT PHẨM KHUYẾN MÃI"))
                                                                    prolinesuba.ItemGroup = "VPKM";
                                                                else
                                                                    prolinesuba.ItemGroup = "KH";
                                                                prolinesuba.ItemId = subitems.Id;
                                                                prolinesuba.LineId = proItem.LineId;
                                                                prolinesuba.ItemCode = subitems.ItemCode;
                                                                prolinesuba.AddAccumulate = proline.AddAccumulate;
                                                                prolinesuba.ItemName = subitems.ItemName;
                                                                prolinesuba.PackingId = (int)subitems.PackingId;
                                                                prolinesuba.PackingName = subitems.Packing.Name;
                                                                prolinesuba.QuantityAdd = QtyFlag * subsub.Quantity;
                                                                prooderlinesub.Add(prolinesuba);
                                                            }
                                                        }
                                                    }

                                                    if (check > 0 && and > 0)
                                                        prolinesub.Cond = "AND";
                                                    if (check > 0 && and == 0 && or > 0)
                                                        prolinesub.Cond = "OR";
                                                    if (prolinesub.QuantityAdd > 0)
                                                    {
                                                        prooderlinesub.Add(prolinesub);
                                                        break;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                else if (proline.SubType == 2)
                                {
                                    int Flag2 = 0;
                                    if (Flag2 == 0)
                                        promotionParam.PromotionParamLine.ForEach(item =>
                                            item.Quantity = item.QuantityRef);
                                    if (FlagCust2 > 0)
                                        promotionParam.PromotionParamLine.ForEach(item =>
                                            item.Quantity = item.QuantityRef);
                                    bool checkmin = false;
                                    int Qty = 0;

                                    double Flag = 0;
                                    foreach (var proItem in promotionParam.PromotionParamLine.ToList())
                                    {
                                        Qty = (int)proItem.Quantity;
                                        var itemDs = itemDss.FirstOrDefault(x => x.Id == proItem.ItemId);
                                        if (itemDs != null)
                                            foreach (var promotionLineSub in promotionLineSubs
                                                         .Where(e => e.IsSameType == false && e.FollowBy == 1)
                                                         .OrderByDescending(e => e.Quantity).ToList())
                                            {
                                                if (Qty >= promotionLineSub.Quantity)
                                                    continue;
                                                else
                                                {
                                                    int QtyFlag = 0;
                                                    var idItemType = promotionLineSub.PromotionItemBuy
                                                        .Where(obj => obj.ItemType == "G").Select(obj => obj.ItemId)
                                                        .ToList();
                                                    var idItemId = promotionLineSub.PromotionItemBuy
                                                        .Where(obj => obj.ItemType == "I").Select(obj => obj.ItemId)
                                                        .ToList();
                                                    var idunit = promotionLineSub.PromotionUnit.Select(obj => obj.UomId)
                                                        .ToList();
                                                    if ((idItemType.Contains((int)itemDs.ItemTypeId) &&
                                                         idunit.Contains((int)itemDs.PackingId)) ||
                                                        idItemId.Contains((int)itemDs.Id))
                                                    {
                                                        PromotionOrderLineSub prolinesub = new PromotionOrderLineSub();
                                                        prolinesub.Cond = "AND";
                                                        prolinesub.InGroup = 0;
                                                        prolinesub.ItemId = itemDs.Id;
                                                        prolinesub.LineId = proItem.LineId;
                                                        prolinesub.AddAccumulate = proline.AddAccumulate;
                                                        prolinesub.ItemCode = itemDs.ItemCode;
                                                        prolinesub.ItemName = itemDs.ItemName;
                                                        prolinesub.PackingId = (int)itemDs.PackingId;
                                                        prolinesub.PackingName = itemDs.Packing.Name;
                                                        prolinesub.Discount = promotionLineSub.Discount;
                                                        prolinesub.DiscountType = promotionLineSub.DiscountType;
                                                        prolinesub.PriceType = promotionLineSub.PriceType;
                                                        proItem.Quantity = 0;
                                                        prooderlinesub.Add(prolinesub);
                                                        Flag2 = Flag2 + 1;
                                                        break;
                                                    }
                                                }
                                            }

                                        if (Flag == 0)
                                        {
                                            double totalVolumn = 0;
                                            List<MinValue> lm = new List<MinValue>();
                                            foreach (var promotionLineSub in promotionLineSubs
                                                         .Where(e => e.FollowBy == 2)
                                                         .OrderByDescending(e => e.MinVolumn).ToList())
                                            {
                                                totalVolumn = 0;
                                                MinValue m = new MinValue();
                                                m.Min = promotionLineSub.MinVolumn ?? 0;
                                                m.count = promotionLineSubs
                                                    .Where(e => e.FollowBy == 2 && e.MinVolumn == m.Min).Count();
                                                m.refCount = m.count;
                                                var itemid = promotionLineSub.PromotionItemBuy.Select(obj => obj.ItemId)
                                                    .ToList();
                                                string itemIdsString = string.Join(",", itemid);
                                                m.ItemId = itemIdsString;
                                                lm.Add(m);
                                                if (promotionParam.OrderDate.Date >=
                                                    promotionLineSub.FromDate.Value.Date &&
                                                    promotionParam.OrderDate.Date <= promotionLineSub.ToDate.Value.Date)
                                                {
                                                    Item itemss = null;
                                                    foreach (var prLine in promotionParam.PromotionParamLine.ToList())
                                                    {
                                                        var idItemType = promotionLineSub.PromotionItemBuy
                                                            .Select(obj => obj.ItemId).ToList();
                                                        var idItemId = promotionLineSub.PromotionItemBuy
                                                            .Where(e => e.ItemType == "I")
                                                            .Select(obj => obj.ItemId).ToList();
                                                        var idpacking = promotionLineSub.PromotionUnit
                                                            .Select(obj => obj.UomId).ToList();
                                                        itemss = itemDss.Where(x => x.Id == prLine.ItemId
                                                            && idItemType.Contains((int)x.ItemTypeId) &&
                                                            idpacking.Contains((int)x.PackingId)
                                                        ).FirstOrDefault();
                                                        if (itemss == null)
                                                        {
                                                            itemss = itemDss.Where(x => x.Id == prLine.ItemId
                                                                && idItemId.Contains((int)x.Id)
                                                            ).FirstOrDefault();
                                                        }

                                                        if (itemss != null)
                                                            totalVolumn = totalVolumn + (double)prLine.Quantity *
                                                                (double)itemss.Packing.Volumn;
                                                    }
                                                }

                                            if (totalVolumn >= promotionLineSub.MinVolumn && Flag == 0)
                                            {
                                                    var check = lm.FirstOrDefault(e => e.count < e.refCount && e.ItemId == itemIdsString);                                                    if(check == null)
                                                    if(check == null)
                                                    {
                                                            foreach (var prLine in promotionParam.PromotionParamLine.ToList())
                                                            {
                                                                Item itemss = null;
                                                                var idItemType = promotionLineSub.PromotionItemBuy
                                                                    .Select(obj => obj.ItemId).ToList();
                                                                var idItemId = promotionLineSub.PromotionItemBuy
                                                                    .Where(e => e.ItemType == "I")
                                                                    .Select(obj => obj.ItemId).ToList();
                                                                var idpacking = promotionLineSub.PromotionUnit
                                                                    .Select(obj => obj.UomId).ToList();
                                                                itemss = itemDss.Where(x => x.Id == prLine.ItemId
                                                                                && idItemType.Contains((int)x.ItemTypeId) &&
                                                                                idpacking.Contains((int)x.PackingId)
                                                                    ).FirstOrDefault();
                                                                if (itemss == null)
                                                                {
                                                                    itemss = itemDss.Where(x => x.Id == prLine.ItemId
                                                                                    && idItemId.Contains((int)x.Id)
                                                                        ).FirstOrDefault();
                                                                }

                                                                //var check = lm.FirstOrDefault(e => e.count < e.refCount && e.ItemId == itemIdsString);
                                                                if (itemss != null)
                                                                {
                                                                    PromotionOrderLineSub prolinesub = new PromotionOrderLineSub();
                                                                    prolinesub.Cond = "AND";
                                                                    prolinesub.InGroup = 0;
                                                                    prolinesub.ItemId = itemss.Id;
                                                                    prolinesub.ItemCode = itemss.ItemCode;
                                                                    prolinesub.LineId = prLine.LineId;
                                                                    prolinesub.AddAccumulate = proline.AddAccumulate;
                                                                    prolinesub.ItemName = itemss.ItemName;
                                                                    prolinesub.PackingId = (int)itemss.PackingId;
                                                                    prolinesub.PackingName = itemss.Packing.Name;
                                                                    prolinesub.Discount = promotionLineSub.Discount;
                                                                    prolinesub.DiscountType = promotionLineSub.DiscountType;
                                                                    prolinesub.PriceType = promotionLineSub.PriceType;
                                                                    prooderlinesub.Add(prolinesub);
                                                                    lm.Where(e => e.Min == (promotionLineSub.MinVolumn ?? 0)).ForEach(e => e.count = e.count - 1);
                                                                }
                                                            }
                                                        }
                                                    
                                                var valid = lm.FirstOrDefault(e => e.Min == (promotionLineSub.MinVolumn ?? 0) && e.count < e.refCount);
                                                if (valid != null)
                                                   Flag = Flag + 1;
                                                totalVolumn = 0;
                                            }
                                        }
                                    }
                                }

                                    // Tính cho đơn hàng có mặt hàng khác loại
                                    foreach (var promotionLineSub in promotionLineSubs
                                                 .Where(e => e.IsSameType == true && e.FollowBy == 1)
                                                 .OrderByDescending(e => e.Quantity).ToList())
                                    {
                                        var idItemType = promotionLineSub.PromotionItemBuy.Where(e => e.ItemType == "G")
                                            .Select(obj => obj.ItemId).ToList();
                                        var idItemId = promotionLineSub.PromotionItemBuy.Where(e => e.ItemType == "I")
                                            .Select(obj => obj.ItemId).ToList();
                                        var idunit = promotionLineSub.PromotionUnit.Select(obj => obj.UomId).ToList();
                                        var iditem = promotionParam.PromotionParamLine.Where(e => e.Quantity > 0)
                                            .Select(obj => obj.ItemId).ToList();
                                        foreach (var proItem in promotionParam.PromotionParamLine
                                                     .Where(e => e.Quantity > 0)
                                                     .ToList())
                                        {
                                            Item checkItem = null;
                                            if (idItemType.Count > 0)
                                            {
                                                checkItem = itemDss.FirstOrDefault(e =>
                                                    e.Id == proItem.ItemId && idItemType.Contains((int)e.ItemTypeId) &&
                                                    idunit.Contains((int)e.PackingId));
                                            }
                                            else
                                            {
                                                checkItem = itemDss.FirstOrDefault(e =>
                                                    e.Id == proItem.ItemId && idItemId.Contains((int)e.Id));
                                            }

                                            if (checkItem != null)
                                            {
                                                var checkOther = itemDss.Where(e =>
                                                    e.ItemTypeId == checkItem.ItemTypeId &&
                                                    e.PackingId == checkItem.PackingId &&
                                                    iditem.Contains(e.Id)).Select(e => e.Id).ToList();
                                                var itemParam = promotionParam.PromotionParamLine.Where(e =>
                                                    e.Quantity > 0 && checkOther.Contains(e.ItemId));
                                                var minItem = itemDss.OrderBy(e => e.Price).FirstOrDefault(e =>
                                                    e.ItemTypeId == checkItem.ItemTypeId &&
                                                    e.PackingId == checkItem.PackingId &&
                                                    iditem.Contains(e.Id));
                                                Qty = (int)itemParam.Sum(e => e.Quantity);
                                                int QtyFlag = 0;
                                                if (Qty < promotionLineSub.Quantity && Qty > 0)
                                                {
                                                    PromotionOrderLineSub prolinesub = new PromotionOrderLineSub();
                                                    prolinesub.Cond = "AND";
                                                    prolinesub.InGroup = 0;
                                                    prolinesub.ItemId = minItem.Id;
                                                    prolinesub.LineId = proItem.LineId;
                                                    prolinesub.AddAccumulate = proline.AddAccumulate;
                                                    prolinesub.ItemCode = minItem.ItemCode;
                                                    prolinesub.ItemName = minItem.ItemName;
                                                    prolinesub.PackingId = (int)minItem.PackingId;
                                                    prolinesub.PackingName = minItem.Packing.Name;
                                                    prolinesub.Discount = promotionLineSub.Discount;
                                                    prolinesub.DiscountType = promotionLineSub.DiscountType;
                                                    prolinesub.PriceType = promotionLineSub.PriceType;
                                                    proItem.Quantity = 0;
                                                    prooderlinesub.Add(prolinesub);
                                                }
                                            }
                                            else
                                                continue;
                                        }
                                    }

                                    promotionOrderLine.PromotionOrderLineSub = prooderlinesub;
                                    if (promotionOrderLine.PromotionOrderLineSub != null)
                                        prooderline.Add(promotionOrderLine);
                                }

                                promotionOrderLine.PromotionOrderLineSub = prooderlinesub;
                                if (promotionOrderLine.PromotionOrderLineSub != null)
                                    prooderline.Add(promotionOrderLine);
                            }

                var prolineA = prooderline.Where(e => e.PromotionOrderLineSub.Count > 0).ToList();
                promotionOrder.PromotionOrderLine = prolineA;
                FlagCust = FlagCust + 1;
                FlagCust2 = FlagCust2 + 1;
            }
        }

        public async Task<PaymentInfo> GetPaymentInfo(PriceDocCheck model)
        {
            double total = 0;
            double TotalPayNow = 0;
            double TotalDebt = 0;
            double TotalDebtGuarantee = 0;

            double TotalDiscount = 0;
            var bp = await _context.BP.AsNoTracking().FirstOrDefaultAsync(b => b.Id == model.CardId);
            if (bp == null)
            {
                throw new KeyNotFoundException("Không tìm thấy nhà phân phối");
            }

            var commited = await _committedService.GetCommitedDiscount(model.CardId ?? 0,
                model.ItemDetail.Select(p => new ItemChecking
                {
                    Price = p.Price,
                    Quantity = p.Quantity,
                    ItemId = p.ItemId ?? 0,
                }).ToList()
            );

            double bonusCommited = 0;
            if (commited != null)
            {
                bonusCommited = commited.CommittedLine.Sum(p =>
                    p.CommittedLineSub.Sum(x => x.TotalBonus));
            }

            if (bp.SapGroupCode == 1112)
            {
                bonusCommited *= 0.985;
            }
            return model.PaymentInfo;
        }
        
        
    }
}

