using BackEndAPI.Data;
using BackEndAPI.Models.BusinessPartners;
using BackEndAPI.Models.Other;
using BackEndAPI.Models.Promotion;
using Gridify;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Utilities.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace BackEndAPI.Service.Promotions
{
    public class PointSetupService : IPointSetupService
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<PointSetupService> _logger;
        public PointSetupService(AppDbContext context, IHttpContextAccessor httpContextAccessor, ILogger<PointSetupService> logger)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }
        public async Task<(PointSetupViewData, Mess)> CreateAsync(PointSetupCreateDto dto)
        {
            Mess mess = new Mess();
            try
            {
                var entity = new PointSetup
                {
                    Name = dto.Name,
                    FromDate = dto.FromDate,
                    ToDate = dto.ToDate,
                    ExtendedToDate = dto.ExtendedToDate,
                    IsAllCustomer = dto.IsAllCustomer,
                    NotifyBeforeDays = dto.NotifyBeforeDays,
                    IsActive = dto.IsActive,
                    Note = dto.Note,
                    PointSetupCustomer = dto.Customers.Select(c => new PointSetupCustomer
                    {
                        Type = c.Type,
                        CustomerId = c.CustomerId,
                        CustomerCode = c.CustomerCode,
                        CustomerName = c.CustomerName
                    }).ToList(),
                    PointSetupLine = dto.Lines.Select(l => new PointSetupLine
                    {
                        Point = l.Point,
                        Industry = l.IndustryIds.Select(i => new PointSetupIndustry { IndustryId = i }).ToList(),
                        Brands = l.BrandIds.Select(b => new PointSetupBrand { BrandId = b }).ToList(),
                        Packings = l.PackingIds.Select(p => new PointSetupPacking { PackingId = p }).ToList(),
                        ItemType = l.ItemType.Select(pt => new PointSetupItemType
                        {
                            ItemType = pt.ItemType,
                            ItemId = pt.ItemId,
                            ItemCode = pt.ItemCode,
                            ItemName = pt.ItemName
                        }).ToList()
                    }).ToList()
                };

                _context.PointSetups.Add(entity);
                await _context.SaveChangesAsync();
                var entity1 = await _context.PointSetups
               .Include(p => p.PointSetupCustomer)
               .Include(p => p.PointSetupLine).ThenInclude(l => l.Industry).ThenInclude(c => c.Industry)
               .Include(p => p.PointSetupLine).ThenInclude(l => l.Brands).ThenInclude(b => b.Brand)
               .Include(p => p.PointSetupLine).ThenInclude(l => l.Packings).ThenInclude(pk => pk.Packing)
               .Include(p => p.PointSetupLine).ThenInclude(l => l.ItemType)
               .FirstOrDefaultAsync(p => p.Id == entity.Id);
                var items = MapToViewData(entity1);
                return (items, null);
            }
            catch (Exception ex)
            {
                mess.Status = 900;
                mess.Errors = ex.Message;
                return (null, mess);
            }
        }

        public async Task<(IEnumerable<PointSetupViewData>, int, Mess)> GetAllAsync(GridifyQuery query)
        {
            Mess mess = new Mess();
            try
            {
                var gridifyQuery = _context.PointSetups.AsQueryable()
                    .Include(x => x.PointSetupLine)
                        .ThenInclude(l => l.Industry).ThenInclude(c => c.Industry)
                    .Include(x => x.PointSetupLine)
                        .ThenInclude(l => l.Brands).ThenInclude(b => b.Brand)
                    .Include(x => x.PointSetupLine)
                        .ThenInclude(l => l.Packings).ThenInclude(p => p.Packing)
                    .Include(x => x.PointSetupLine)
                        .ThenInclude(l => l.ItemType)
                    .AsNoTracking()
                    .ApplyFiltering(query);

                var total = await gridifyQuery.CountAsync();
                var data = await gridifyQuery.ApplyOrdering(query).ApplyPaging(query).ToListAsync();

                var items = data.Select(MapToViewData).ToList();
                return (items, total, null);
            }
            catch (Exception ex)
            {
                mess.Status = 900;
                mess.Errors = ex.Message;
                return (null, 0, mess);
            }
        }

        public async Task<(PointSetupViewData, Mess)> GetByIdAsync(int id)
        {
            Mess mess = new Mess();
            try
            {
                var entity = await _context.PointSetups
                .Include(p => p.PointSetupCustomer)
                .Include(p => p.PointSetupLine).ThenInclude(l => l.Industry).ThenInclude(c => c.Industry)
                .Include(p => p.PointSetupLine).ThenInclude(l => l.Brands).ThenInclude(b => b.Brand)
                .Include(p => p.PointSetupLine).ThenInclude(l => l.Packings).ThenInclude(pk => pk.Packing)
                .Include(p => p.PointSetupLine).ThenInclude(l => l.ItemType)
                .FirstOrDefaultAsync(p => p.Id == id);

                if (entity == null) throw new Exception("PointSetup not found");

                var items =  MapToViewData(entity);
                return (items, null);
            }
            catch (Exception ex)
            {
                mess.Status = 900;
                mess.Errors = ex.Message;
                return (null, mess);
            }
        }

        public async Task<(PointSetupViewData, Mess)> UpdateAsync(PointSetupUpdateDto dto)
        {
            Mess mess = new Mess();
            try
            {
                var entity = await _context.PointSetups
            .Include(p => p.PointSetupCustomer)
            .Include(p => p.PointSetupLine).ThenInclude(l => l.Industry)
            .Include(p => p.PointSetupLine).ThenInclude(l => l.Brands)
            .Include(p => p.PointSetupLine).ThenInclude(l => l.Packings)
            .Include(p => p.PointSetupLine).ThenInclude(l => l.ItemType)
            .FirstOrDefaultAsync(p => p.Id == dto.Id);

                if (entity == null) throw new Exception("Not found");

                // ── Guard: nếu chương trình đã PHÁT SINH BẤT KỲ giao dịch tích điểm nào
                // (kể cả pending chưa confirmed lẫn cycle đã confirmed) → CHỈ cho sửa
                //   • Name (đặt lại tên cho dễ đọc)
                //   • ExtendedToDate (gia hạn ngày hết hạn nhận quà)
                // Mọi field khác đều bị BLOCK để bảo vệ tính toàn vẹn dữ liệu tích điểm.
                //
                // Check 3 nguồn để bao phủ mọi trạng thái:
                //   1. CustomerPointLine: line item chi tiết — tạo ngay khi đặt đơn DXN (pending)
                //   2. CustomerPointCycles: tài khoản điểm — tạo khi đơn DHT
                //   3. CustomerPoint (header) → check qua Details có PoitnSetupId match
                bool hasLineEarnings = await _context.CustomerPointLine
                    .AnyAsync(p => p.PoitnSetupId == entity.Id);
                bool hasCycleEarnings = await _context.CustomerPointCycles
                    .AnyAsync(c => c.PoitnSetupId == entity.Id);
                bool hasEarnings = hasLineEarnings || hasCycleEarnings;

                _logger.LogInformation(
                    "UpdatePointSetup id={SetupId} hasEarnings={HasEarnings} (line={Line}, cycle={Cycle})",
                    entity.Id, hasEarnings, hasLineEarnings, hasCycleEarnings);

                if (hasEarnings)
                {
                    var blockedFields = new List<string>();

                    if (entity.FromDate.Date != dto.FromDate.Date)
                        blockedFields.Add("FromDate");
                    if (entity.ToDate.Date != dto.ToDate.Date)
                        blockedFields.Add("ToDate");
                    if (entity.IsAllCustomer != dto.IsAllCustomer)
                        blockedFields.Add("IsAllCustomer");
                    if (entity.NotifyBeforeDays != dto.NotifyBeforeDays)
                        blockedFields.Add("NotifyBeforeDays");
                    if (entity.IsActive != dto.IsActive)
                        blockedFields.Add("IsActive");
                    if (!string.Equals(entity.Note ?? "", dto.Note ?? "", StringComparison.Ordinal))
                        blockedFields.Add("Note");

                    // Customers/Lines: detect khác biệt qua count + content
                    if (HasCustomersChanged(entity.PointSetupCustomer, dto.Customers))
                        blockedFields.Add("Customers");
                    if (HasLinesChanged(entity.PointSetupLine, dto.Lines))
                        blockedFields.Add("Lines");

                    if (blockedFields.Count > 0)
                    {
                        mess.Status = 400;
                        mess.Errors = $"Chương trình đã có lịch sử tích điểm — chỉ được sửa Name và ExtendedToDate. " +
                                      $"Các field bị thay đổi không hợp lệ: {string.Join(", ", blockedFields)}.";
                        return (null, mess);
                    }

                    // Cho phép sửa Name + ExtendedToDate
                    entity.Name = dto.Name;
                    entity.ExtendedToDate = dto.ExtendedToDate;

                    // Khi gia hạn ExtendedToDate → đồng bộ ExpiryDate cho cycle còn active
                    if (dto.ExtendedToDate.HasValue)
                    {
                        var cycles = await _context.CustomerPointCycles
                            .Where(c => c.PoitnSetupId == entity.Id && c.Status == 0)
                            .ToListAsync();
                        foreach (var cyc in cycles)
                        {
                            cyc.ExpiryDate = dto.ExtendedToDate.Value;
                        }
                    }

                    await _context.SaveChangesAsync();
                    var refreshed = await GetByIdAsync(entity.Id);
                    return refreshed;
                }

                // ── Chưa có tích điểm: free to edit toàn bộ
                // update main
                entity.Name = dto.Name;
                entity.FromDate = dto.FromDate;
                entity.ToDate = dto.ToDate;
                entity.ExtendedToDate = dto.ExtendedToDate;
                entity.IsAllCustomer = dto.IsAllCustomer;
                entity.NotifyBeforeDays = dto.NotifyBeforeDays;
                entity.IsActive = dto.IsActive;
                entity.Note = dto.Note;

                // reset child collections
                entity.PointSetupCustomer.Clear();
                entity.PointSetupLine.Clear();

                // add new customers
                foreach (var c in dto.Customers)
                {
                    entity.PointSetupCustomer.Add(new PointSetupCustomer
                    {
                        Type = c.Type,
                        CustomerId = c.CustomerId,
                        CustomerCode = c.CustomerCode,
                        CustomerName = c.CustomerName
                    });
                }

                // add new lines
                foreach (var l in dto.Lines)
                {
                    entity.PointSetupLine.Add(new PointSetupLine
                    {
                        Point = l.Point,
                        Industry = l.IndustryIds.Select(i => new PointSetupIndustry { IndustryId = i }).ToList(),
                        Brands = l.BrandIds.Select(b => new PointSetupBrand { BrandId = b }).ToList(),
                        Packings = l.PackingIds.Select(p => new PointSetupPacking { PackingId = p }).ToList(),
                        ItemType = l.ItemType.Select(pt => new PointSetupItemType
                        {
                            ItemType = pt.ItemType,
                            ItemId = pt.ItemId,
                            ItemCode = pt.ItemCode,
                            ItemName = pt.ItemName
                        }).ToList()
                    });
                }

                await _context.SaveChangesAsync();
                var entity1 = await _context.PointSetups
               .Include(p => p.PointSetupCustomer)
               .Include(p => p.PointSetupLine).ThenInclude(l => l.Industry).ThenInclude(c => c.Industry)
               .Include(p => p.PointSetupLine).ThenInclude(l => l.Brands).ThenInclude(b => b.Brand)
               .Include(p => p.PointSetupLine).ThenInclude(l => l.Packings).ThenInclude(pk => pk.Packing)
               .Include(p => p.PointSetupLine).ThenInclude(l => l.ItemType)
               .FirstOrDefaultAsync(p => p.Id == entity.Id);
                var items =  MapToViewData(entity1);
                return (items, null);
            }
            catch (Exception ex)
            {
                mess.Status = 900;
                mess.Errors = ex.Message;
                return (null, mess);
            }
        }

        private PointSetupViewDto MapToViewDto(PointSetup entity)
        {
            return new PointSetupViewDto
            {
                Id = entity.Id,
                Name = entity.Name,
                FromDate = entity.FromDate,
                ToDate = entity.ToDate,
                ExtendedToDate = entity.ExtendedToDate,
                IsAllCustomer = entity.IsAllCustomer,
                NotifyBeforeDays = entity.NotifyBeforeDays,
                IsActive = entity.IsActive,
                Note = entity.Note,
                Customers = entity.PointSetupCustomer.Select(c => new PointSetupCustomerViewDto
                {
                    Id = c.Id,
                    Type = c.Type,
                    CustomerId = c.CustomerId,
                    CustomerCode = c.CustomerCode,
                    CustomerName = c.CustomerName
                }).ToList(),
                Lines = entity.PointSetupLine.Select(l => new PointSetupLineViewDto
                {
                    Id = l.Id,
                    Point = l.Point,

                    Industries = l.Industry?.Where(i => i?.Industry != null).Select(i => new IndustryViewDto
                    {
                        Id = i.Industry.Id,
                        IndustryCode = i.Industry.Code,
                        IndustryName = i.Industry.Name
                    }).ToList() ?? new List<IndustryViewDto>(),
                    Brands = l.Brands?.Where(i => i?.Brand != null).Select(b => new BrandViewDto
                    {
                        Id = b.Brand.Id,
                        BrandCode = b.Brand.Code,
                        BrandName = b.Brand.Name
                    }).ToList() ?? new List<BrandViewDto>(),
                    Packings = l.Packings?.Where(i => i?.Packing != null).Select(p => new PackingViewDto
                    {
                        Id = p.Packing.Id,
                        PackingCode = p.Packing.Code,
                        PackingName = p.Packing.Name
                    }).ToList() ?? new List<PackingViewDto>(),
                    ItemType = l.ItemType ?.Where(i => i?.ItemType != null).Select(pt => new PointSetupItemTypeViewDto
                    {
                        Id = pt.Id,
                        ItemType = pt.ItemType,
                        ItemId = pt.ItemId,
                        ItemCode = pt.ItemCode,
                        ItemName = pt.ItemName
                    }).ToList() ?? new List<PointSetupItemTypeViewDto>()
                }).ToList()
            };
        }

        private PointSetupViewData MapToViewData(PointSetup entity)
        {
            return new PointSetupViewData
            {
                Id = entity.Id,
                Name = entity.Name,
                FromDate = entity.FromDate,
                ToDate = entity.ToDate,
                ExtendedToDate = entity.ExtendedToDate,
                IsAllCustomer = entity.IsAllCustomer,
                NotifyBeforeDays = entity.NotifyBeforeDays,
                IsActive = entity.IsActive,
                Note = entity.Note,
                Customers = entity.PointSetupCustomer.Select(c => new PointSetupCustomerViewDto
                {
                    Id = c.Id,
                    Type = c.Type,
                    CustomerId = c.CustomerId,
                    CustomerCode = c.CustomerCode,
                    CustomerName = c.CustomerName
                }).ToList(),
                Lines = entity.PointSetupLine.Select(l => new PointSetupLineViewData
                {
                    Id = l.Id,
                    Point = l.Point,

                    IndustryIds = l.Industry?.Where(i => i?.Industry != null).Select(i => i.IndustryId).ToList() ?? new List<int>(),
                    BrandIds = l.Brands?.Where(i => i?.Brand != null).Select(b => b.BrandId).ToList() ?? new List<int>(),
                    PackingIds = l.Packings?.Where(i => i?.Packing != null).Select(p => p.PackingId).ToList() ?? new List<int>(),
                    ItemType = l.ItemType?.Where(i => i?.ItemType != null).Select(pt => new PointSetupItemTypeViewDto
                    {
                        Id = pt.Id,
                        ItemType = pt.ItemType,
                        ItemId = pt.ItemId,
                        ItemCode = pt.ItemCode,
                        ItemName = pt.ItemName
                    }).ToList() ?? new List<PointSetupItemTypeViewDto>()
                }).ToList()
            };
        }

        //public async Task CalculatePoints(int DocId)
        //{
        //   Mess mess = new Mess();
        //    try
        //    {
        //        var doc = _context.ODOC.AsNoTracking()
        //        .Include(o => o.ItemDetail)
        //            .ThenInclude(d => d.Item)
        //        .FirstOrDefault(o => o.Id == DocId && string.IsNullOrEmpty(o.DocType));
        //        if (doc == null)
        //            return;
        //        int DocType = doc?.ObjType ?? 0;
        //        DateTime?  DocDate = doc.DocDate;
        //        var docLines = doc?.ItemDetail?.Select(d => new DocLine
        //        {
        //            CustomerId = doc.CardId ?? 0,
        //            IndustryId = d.Item?.IndustryId ?? 0,
        //            BrandId = d.Item?.BrandId ?? 0,
        //            ItemTypeId = d.Item?.ItemTypeId ?? 0,
        //            ItemId = d.ItemId ?? 0,
        //            ItemCode = d.ItemCode ?? "",
        //            PackingId = d.Item?.PackingId ?? 0,
        //            Quantity = d.Quantity
        //        }).ToList();


        //        var validSetups = _context.PointSetups.AsNoTracking()
        //        .Include(e => e.PointSetupCustomer)
        //        .Include(e => e.PointSetupLine)
        //            .ThenInclude(l => l.Industry)
        //        .Include(e => e.PointSetupLine)
        //            .ThenInclude(l => l.Brands)
        //        .Include(e => e.PointSetupLine)
        //            .ThenInclude(l => l.ItemType)
        //        .Include(e => e.PointSetupLine)
        //            .ThenInclude(l => l.Packings)
        //        .Where(e => e.IsActive == true
        //                 && e.FromDate.Date <= doc.DocDate.Value.Date
        //                 && e.ToDate.Date >= doc.DocDate.Value.Date)
        //        .ToList();
        //        var customerId = docLines.Select(e => e.CustomerId).Distinct().ToArray();
        //        var customerGroup = _context.BP.Include(p => p.Groups).Where(e => customerId.Contains(e.Id)).SelectMany(e => e.Groups.Select(e => e.Id)).ToArray();
        //        var setups = validSetups
        //        .Where(s => s.PointSetupCustomer.Any(c => c.Type == "C" && customerId.Contains(c.CustomerId)))
        //        .ToList();
        //        if (!setups.Any())
        //        {
        //            setups = validSetups.Where(s => s.PointSetupCustomer.Any(c => c.Type == "G" && customerGroup.Contains(c.CustomerId))).ToList();
        //            if (!setups.Any())
        //                setups = validSetups.Where(s => s.IsAllCustomer).ToList();
        //        }
        //        double totalPoints = 0;
        //        foreach(var setup in setups)
        //        {
        //            foreach (var line in setup.PointSetupLine)
        //            {
        //                foreach (var docLine in docLines)
        //                {
        //                    bool matchIndustry = !line.Industry.Any() || line.Industry.Any(i => i.IndustryId == docLine.IndustryId);
        //                    bool matchBrand = !line.Brands.Any() || line.Brands.Any(b => b.BrandId == docLine.BrandId);
        //                    if (line.ItemType.Where(e=>e.ItemType == "I").Any(i => i.ItemCode == docLine.ItemCode))
        //                    {
        //                        if (matchIndustry && matchBrand)
        //                        {
        //                            totalPoints += line.Point * docLine.Quantity;
        //                        }
        //                    }
        //                    else
        //                    {
        //                        bool matchItemType = !line.ItemType.Where(e => e.ItemType == "I").Any()
        //                            && line.ItemType.Where(e => e.ItemType == "G").Any(i => i.ItemId == docLine.ItemTypeId);
        //                        bool matchPacking = !line.Packings.Any() || line.Packings.Any(p => p.PackingId == docLine.PackingId);
        //                        if (matchIndustry && matchBrand && matchItemType && matchPacking)
        //                        {
        //                            totalPoints += line.Point * docLine.Quantity;
        //                        }
        //                    }
        //                }
        //            }
        //            if (totalPoints > 0)
        //            {
        //                var cycle =  _context.CustomerPointCycles
        //                .FirstOrDefault(x => x.CustomerId == customerId[0] &&
        //                                          x.PoitnSetupId == setup.Id &&
        //                                          x.Status == 0);

        //                if (cycle == null)
        //                {
        //                    cycle = new CustomerPointCycle
        //                    {
        //                        CustomerId = customerId[0],
        //                        PoitnSetupId = setup.Id,
        //                        StartDate = setup.FromDate,
        //                        EndDate = setup.ToDate,
        //                        ExpiryDate = setup.ExtendedToDate ?? setup.ToDate,
        //                        EarnedPoint = 0,
        //                        RedeemedPoint = 0,
        //                        RemainingPoint = 0,
        //                        Status = 0
        //                    };
        //                    _context.CustomerPointCycles.Add(cycle);
        //                }
        //                var history = _context.CustomerPoint.FirstOrDefault(e => e.CustomerId == customerId[0] && e.DocId == DocId && e.DocType == DocType && e.AddPoint ==true);
        //                if (history == null)
        //                {
        //                    history = new CustomerPointHistory
        //                    {
        //                        CustomerId = customerId[0],
        //                        DocId = DocId,
        //                        DocType = DocType,
        //                        PoitnSetupId = setup.Id,
        //                        PointChange = totalPoints,
        //                        PointBefore = (cycle.RemainingPoint - totalPoints) > 0 ? (cycle.RemainingPoint - totalPoints) : 0,
        //                        PointAfter = cycle.RemainingPoint,
        //                        DocDate = DocDate,
        //                        Note = $"Cộng {totalPoints} điểm theo chương trình {setup.Name}"
        //                    };
        //                    _context.CustomerPointHistories.Add(history);
        //                }
        //                cycle.EarnedPoint += totalPoints;
        //                cycle.RemainingPoint += totalPoints;


        //            }
        //            totalPoints = 0;
        //        }
        //        await _context.SaveChangesAsync();
        //    }
        //    catch(Exception ex)
        //    {

        //    }
        //}

        public async Task<IEnumerable<CalculatorPointReturn>> CalculatePointCheck(CalculatorPoint p)
        {
            Mess mess = new Mess();
            try
            {
                var calculatorLines = p.CalculatorPointLine.ToList();
                var items = _context.Item.Where(e=> calculatorLines.Select(e=>e.ItemId).ToArray().Contains(e.Id)).ToList();
                
                
                // Join ở bộ nhớ
                var docLines = items.Select(d => new DocLine
                {
                    CustomerId = p.CardId,
                    IndustryId = d.IndustryId ?? 0,
                    BrandId = d.BrandId ?? 0,
                    ItemTypeId = d.ItemTypeId ?? 0,
                    ItemId = d.Id,
                    ItemCode = d.ItemCode ?? "",
                    PackingId = d.PackingId ?? 0,
                    Quantity = calculatorLines.FirstOrDefault(e => e.ItemId == d.Id)?.Quantity ?? 0
                }).ToList();

                var validSetups = _context.PointSetups.AsNoTracking()
                .Include(e => e.PointSetupCustomer)
                .Include(e => e.PointSetupLine)
                    .ThenInclude(l => l.Industry)
                .Include(e => e.PointSetupLine)
                    .ThenInclude(l => l.Brands)
                .Include(e => e.PointSetupLine)
                    .ThenInclude(l => l.ItemType)
                .Include(e => e.PointSetupLine)
                    .ThenInclude(l => l.Packings)
                .Where(e => e.IsActive == true
                         && e.FromDate.Date <= DateTime.Now.Date
                         && e.ToDate.Date >= DateTime.Now.Date)
                .ToList();
                var customerId = p.CardId;
                var customerGroup = _context.BP.Include(p => p.Groups).Where(e => e.Id == customerId).SelectMany(e => e.Groups.Select(e => e.Id)).ToArray();
                var setups = validSetups
                .Where(s => s.PointSetupCustomer.Any(c => c.Type == "C" && customerId ==c.CustomerId))
                .ToList();
                if (!setups.Any())
                {
                    setups = validSetups.Where(s => s.PointSetupCustomer.Any(c => c.Type == "G" && customerGroup.Contains(c.CustomerId))).ToList();
                    if (!setups.Any())
                        setups = validSetups.Where(s => s.IsAllCustomer).ToList();
                }
                double totalPoints = 0;
                List<CalculatorPointReturn> cals = new List<CalculatorPointReturn>();
                foreach (var setup in setups)
                {
                    foreach (var docLine in docLines)
                    {
                        foreach (var line in setup.PointSetupLine)
                        {
                            // Dùng PointRuleMatcher để identical với CalculatePoints (lưu thật).
                            // Business rule: ItemType=I bỏ qua Packing, ItemType=G check Packing.
                            var docLineForRule = new DocLineForPoint
                            {
                                IndustryId = docLine.IndustryId,
                                BrandId = docLine.BrandId,
                                PackingId = docLine.PackingId,
                                ItemTypeId = docLine.ItemTypeId,
                                ItemId = docLine.ItemId,
                                ItemCode = docLine.ItemCode ?? "",
                                Quantity = docLine.Quantity
                            };
                            totalPoints += PointRuleMatcher.CalculatePoints(line, docLineForRule);

                            CalculatorPointReturn cal = new CalculatorPointReturn();
                            cal.ItemId = docLine.ItemId;
                            cal.Quantity = docLine.Quantity;
                            cal.Points = line.Point;
                            cal.PointSetupName = setup.Name ?? "";
                            cal.PointSetupId = setup.Id;
                            cal.TotalPoint = totalPoints;
                            cals.Add(cal);
                            totalPoints = 0;
                        }
                    }
                }
                return cals;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public async Task CalculatePoints(CalculatorPoint p)
        {
            int DocType = 22;
            DateTime DocDate = DateTime.Now;

            var calculatorLines = p.CalculatorPointLine.ToList();
            var itemIds = calculatorLines.Select(e => e.ItemId).Distinct().ToList();

            var items = _context.Item
                .Where(i => itemIds.Contains(i.Id))
                .ToList();
            var docLines = (
                from i in items
                join c in calculatorLines on i.Id equals c.ItemId
                select new DocLine
                {
                    CustomerId = p.CardId,
                    IndustryId = i.IndustryId ?? 0,
                    BrandId = i.BrandId ?? 0,
                    ItemTypeId = i.ItemTypeId ?? 0,
                    PackingId = i.PackingId ?? 0,
                    ItemId = i.Id,
                    ItemCode = i.ItemCode ?? "",
                    ItemName = i.ItemName ?? "",
                    Quantity = c.Quantity
                }).ToList();

            // Load all setups active
            var validSetups = _context.PointSetups.AsNoTracking()
                .Include(s => s.PointSetupCustomer)
                .Include(s => s.PointSetupLine).ThenInclude(l => l.Industry)
                .Include(s => s.PointSetupLine).ThenInclude(l => l.Brands)
                .Include(s => s.PointSetupLine).ThenInclude(l => l.ItemType)
                .Include(s => s.PointSetupLine).ThenInclude(l => l.Packings)
                .Where(s => s.IsActive &&
                            s.FromDate.Date <= DocDate.Date &&
                            s.ToDate.Date >= DocDate.Date)
                .ToList();

            var customerGroups = _context.BP
                .Include(x => x.Groups)
                .Where(x => x.Id == p.CardId)
                .SelectMany(x => x.Groups.Select(g => g.Id))
                .ToList();
            var setups =
                validSetups.Where(s => s.PointSetupCustomer.Any(c => c.Type == "C" && c.CustomerId == p.CardId)).ToList();

            if (!setups.Any())
            {
                setups = validSetups
                    .Where(s => s.PointSetupCustomer.Any(c => c.Type == "G" && customerGroups.Contains(c.CustomerId)))
                    .ToList();
            }

            if (!setups.Any())
            {
                setups = validSetups.Where(s => s.IsAllCustomer).ToList();
            }
            CustomerPoint custPoint = new CustomerPoint
            {
                AddPoint = true,
                CustomerId = p.CardId,
                DocId = p.DocId,
                DocType = DocType,
                Details = new List<CustomerPointLine>()
            };
            foreach (var setup in setups)
            {
                foreach (var rule in setup.PointSetupLine)
                {
                    foreach (var dl in docLines)
                    {
                        // Dùng PointRuleMatcher (pure function) để đảm bảo logic match
                        // GIỐNG NHAU giữa CalculatePoints (lưu thật) và CalculatePointCheck (preview).
                        // Business rule chốt: ItemType=I bỏ qua Packing, ItemType=G check Packing.
                        var docLineForRule = new DocLineForPoint
                        {
                            IndustryId = dl.IndustryId,
                            BrandId = dl.BrandId,
                            PackingId = dl.PackingId,
                            ItemTypeId = dl.ItemTypeId,
                            ItemId = dl.ItemId,
                            ItemCode = dl.ItemCode ?? "",
                            Quantity = dl.Quantity
                        };

                        double point = PointRuleMatcher.CalculatePoints(rule, docLineForRule);
                        if (point == 0) continue;

                        custPoint.Details.Add(new CustomerPointLine
                        {
                            CustomerId = p.CardId,
                            DocId = p.DocId,
                            DocType = DocType,
                            PoitnSetupId = setup.Id,
                            ItemId = dl.ItemId,
                            ItemCode = dl.ItemCode,
                            ItemName = dl.ItemName,
                            PointChange = point,
                            PointBefore = 0,
                            PointAfter = 0,
                            DocDate = DocDate,
                            Note = $"Cộng {point} điểm theo chương trình {setup.Name}"
                        });
                    }
                }
            }
            var oldLines = _context.CustomerPointLine
                .Where(e => e.DocId == p.DocId && e.DocType == DocType && e.CustomerId == p.CardId)
                .ToList();

            if (oldLines.Any())
            {
                _context.CustomerPointLine.RemoveRange(oldLines);
            }
            custPoint.TotalPointChange = custPoint.Details.Sum(e => e.PointChange);

            _context.CustomerPoint.Add(custPoint);

            await _context.SaveChangesAsync();
        }
        
        public async Task CalculatePointsCircle(int DocId, int CardId, string Type, string? Status)
        {
            var cust = await _context.CustomerPoint
                .Include(e => e.Details)
                .FirstOrDefaultAsync(e => e.CustomerId == CardId && e.DocId == DocId);

            if (cust == null || cust.Details.Count == 0)
                return;

            var details = cust.Details;
            var setupIds = details.Select(d => d.PoitnSetupId).Distinct().ToList();

            var setups = await _context.PointSetups
                .Where(e => setupIds.Contains(e.Id))
                .ToListAsync();
            var grouped = details
                .GroupBy(d => d.PoitnSetupId)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.PointChange));
            CustomerPointCycle GetOrCreateCycle(PointSetup setup)
            {
                var cycle = _context.CustomerPointCycles
                    .FirstOrDefault(x => x.CustomerId == CardId &&
                                         x.PoitnSetupId == setup.Id &&
                                         x.Status == 0);

                if (cycle != null)
                    return cycle;
                cycle = new CustomerPointCycle
                {
                    CustomerId = CardId,
                    PoitnSetupId = setup.Id,
                    StartDate = setup.FromDate,
                    EndDate = setup.ToDate,
                    ExpiryDate = setup.ExtendedToDate ?? setup.ToDate,
                    EarnedPoint = 0,
                    RedeemedPoint = 0,
                    RemainingPoint = 0,
                    Status = 0
                };
                _context.CustomerPointCycles.Add(cycle);
                return cycle;
            }
            if (string.IsNullOrEmpty(Status) && Type == "CP")
            {
                foreach (var setup in setups)
                {
                    var cycle = GetOrCreateCycle(setup);
                    var sum = grouped[setup.Id];

                    cycle.EarnedPoint += sum;
                    cycle.RemainingPoint += sum;
                }
            }
            else if (string.IsNullOrEmpty(Status) && Type == "C")
            {
                var docRef = _context.ODOC.FirstOrDefault(e => e.Id == DocId && (e.Status == "DXN" || e.Status == "DGH"));
                if(docRef != null)
                {
                    foreach (var setup in setups)
                    {
                        var cycle = GetOrCreateCycle(setup);
                        var sum = grouped[setup.Id];
                        cycle.RemainingPoint -= sum;
                        cycle.EarnedPoint -= sum;

                    }
                }


                var reverseDetails =   details
                    .Where(d => d.PointChange > 0)
                    .Select(d => new CustomerPointLine
                    {
                        CustomerId = d.CustomerId,
                        PoitnSetupId = d.PoitnSetupId,
                        DocId = d.DocId,
                        DocType = d.DocType,
                        PointChange = -d.PointChange,
                        ItemId = d.ItemId,
                        ItemCode = d.ItemCode,
                        ItemName = d.ItemName,
                        PointBefore = 0,
                        PointAfter = 0,
                        DocDate = DateTime.UtcNow,
                        Note = "Hủy " + d.Note
                    }).ToList();
                var reverse = new CustomerPoint
                {
                    CustomerId = CardId,
                    DocId = DocId,
                    DocType = cust.DocType,
                    AddPoint = false,
                    Details = reverseDetails,
                    TotalPointChange = reverseDetails.Sum(x => x.PointChange)
                };
                if (reverse.Details.Count > 0)
                    _context.CustomerPoint.Add(reverse);
            }
            else
            {
                var doc = await _context.ODOC.FirstOrDefaultAsync(e => e.Id == DocId);
                if (doc == null) return;
                if (doc.ObjType == 22)
                {
                    foreach (var setup in setups)
                    {
                        var cycle = GetOrCreateCycle(setup);
                        var sum = grouped[setup.Id];
                        cycle.RemainingPoint -= sum;
                        cycle.EarnedPoint -= sum;
                    }

                    var reverseDetails = details
                    .Where(d => d.PointChange > 0)
                    .Select(d => new CustomerPointLine
                    {
                        CustomerId = d.CustomerId,
                        PoitnSetupId = d.PoitnSetupId,
                        DocId = d.DocId,
                        DocType = d.DocType,
                        PointChange = -d.PointChange,
                        ItemId = d.ItemId,
                        ItemCode = d.ItemCode,
                        ItemName = d.ItemName,
                        DocDate = DateTime.UtcNow,
                        Note = $"Đóng đơn - Trừ {d.PointChange} điểm"
                    })
                    .ToList();

                    var reverse = new CustomerPoint
                    {
                        CustomerId = CardId,
                        DocId = DocId,
                        DocType = cust.DocType,
                        AddPoint = false,
                        Details = reverseDetails,
                        TotalPointChange = reverseDetails.Sum(x => x.PointChange)
                    };

                    if (reverse.Details.Count > 0)
                        _context.CustomerPoint.Add(reverse);
                }

                else if (doc.ObjType == 12)
                {
                    foreach (var setup in setups)
                    {
                        var cycle = GetOrCreateCycle(setup);
                        var sum = grouped[setup.Id];

                        cycle.RedeemedPoint += sum;
                        cycle.RemainingPoint -= sum;
                    }
                    var reverseDetails = details
                    .Where(d => d.PointChange < 0)
                    .Select(d => new CustomerPointLine
                    {
                        CustomerId = d.CustomerId,
                        PoitnSetupId = d.PoitnSetupId,
                        DocId = d.DocId,
                        DocType = d.DocType,
                        PointChange = -d.PointChange,
                        ItemId = d.ItemId,
                        ItemCode = d.ItemCode,
                        ItemName = d.ItemName,
                        DocDate = DateTime.UtcNow,
                        Note = $"Đóng đổi VPKM - Cộng {d.PointChange} điểm"
                    })
                    .ToList();

                    var reverse = new CustomerPoint
                    {
                        CustomerId = CardId,
                        DocId = DocId,
                        DocType = cust.DocType,
                        AddPoint = true,
                        Details = reverseDetails,
                        TotalPointChange = reverseDetails.Sum(x => x.PointChange)
                    };
                    if (reverse.Details.Count > 0)
                        _context.CustomerPoint.Add(reverse);
                }
            }

            var check = await _context.SaveChangesAsync();
        }
        public async Task<Mess> RedeemPoints(CalculatorPoint p, string Type)
        {
            Mess mess = new Mess();
            try
            {
                var history = _context.CustomerPoint
                    .Include(e => e.Details)
                    .Where(e => e.CustomerId == p.CardId &&
                                e.DocId == p.DocId &&
                                e.AddPoint == false);
                var setups = _context.PointSetups
                    .Where(e => history
                        .SelectMany(h => h.Details)
                        .Select(d => d.PoitnSetupId)
                        .Contains(e.Id));
                if (Type.Equals("CP"))
                {
                    int DocType = 12;
                    DateTime DocDate = DateTime.Now;

                    var calculatorLines = p.CalculatorPointLine.ToList();
                    var totalPoint = calculatorLines.Sum(l => l.Quantity * l.Point);
                    //var totalAvailablePoints = _context.CustomerPointCycles
                    //    .Where(c => c.CustomerId == p.CardId &&
                    //                c.ExpiryDate.Date >= DateTime.Now.Date)
                    //    .Sum(c => c.RemainingPoint);
                    // Chỉ tổng hợp điểm khả dụng từ các cycle CÒN HIỆU LỰC.
                    var todayUtcAvail = DateTime.UtcNow.Date;
                    var totalAvailablePoints = _context.CustomerPointCycles
                        .Where(c => c.CustomerId == p.CardId &&
                                    c.ExpiryDate.Date >= todayUtcAvail &&
                                    c.Status == 0)
                        .Sum(c => (double?)c.RemainingPoint) ?? 0;
                    if (totalAvailablePoints < totalPoint)
                    {
                        mess.Errors = "Không đủ điểm để đổi vật phẩm / Chương trình đã hết hạn";
                        mess.Status = 400;
                        return mess;
                    }
                    // Chỉ lấy cycle còn HIỆU LỰC (chương trình chưa hết hạn).
                    // Theo business rule: chương trình hết hiệu lực không được dùng để đổi quà.
                    // FIFO: cycle có ExpiryDate sớm nhất bị trừ trước → khuyến khích user dùng điểm sắp hết hạn.
                    var todayUtc = DateTime.UtcNow.Date;
                    var cycles = _context.CustomerPointCycles
                        .Where(c => c.CustomerId == p.CardId &&
                                    c.ExpiryDate.Date >= todayUtc &&
                                    c.RemainingPoint > 0 &&
                                    c.Status == 0)
                        .OrderBy(c => c.ExpiryDate)
                        .ToList();

                    var items = _context.Item
                        .Where(e => calculatorLines.Select(l => l.ItemId).Contains(e.Id))
                        .ToList();
                    CustomerPoint custPoint = new CustomerPoint
                    {
                        AddPoint = false,
                        CustomerId = p.CardId,
                        DocId = p.DocId,
                        DocType = DocType,
                        Details = new List<CustomerPointLine>()
                    };

                    // FIFO logic
                    foreach (var line in calculatorLines)
                    {
                        double pointsNeeded = (double)(line.Quantity * line.Point);
                        double remainingToRedeem = pointsNeeded;

                        foreach (var cycle in cycles)
                        {
                            if (remainingToRedeem <= 0) break;
                            if (cycle.RemainingPoint <= 0) continue;

                            double usePoint = Math.Min(remainingToRedeem, cycle.RemainingPoint);

                            cycle.RedeemedPoint += usePoint;
                            cycle.RemainingPoint -= usePoint;
                            remainingToRedeem -= usePoint;

                            custPoint.Details.Add(new CustomerPointLine
                            {
                                CustomerId = p.CardId,
                                PoitnSetupId = cycle.PoitnSetupId,
                                DocId = p.DocId,
                                DocType = DocType,
                                PointChange = -usePoint,
                                ItemId = line.ItemId,
                                ItemCode = items.First(i => i.Id == line.ItemId).ItemCode,
                                ItemName = items.First(i => i.Id == line.ItemId).ItemName,
                                PointBefore = cycle.RemainingPoint + usePoint,
                                PointAfter = cycle.RemainingPoint,
                                DocDate = DateTime.Now,
                                Note = $"Đổi quà - Trừ {usePoint} điểm (Redeem)"
                            });
                        }

                        if (remainingToRedeem > 0)
                            throw new InvalidOperationException(
                                $"Không đủ điểm để đổi sản phẩm '{items.First(i => i.Id == line.ItemId).ItemName}'.");
                    }

                    custPoint.TotalPointChange = custPoint.Details.Sum(e => e.PointChange);
                    _context.CustomerPoint.Add(custPoint);
                }
                else if (Type.Equals("C"))
                {
                    var historyList = history
                        .SelectMany(h => h.Details)
                        .Where(e => e.PointChange < 0)
                        .ToList();
                    var historyGrouped = historyList
                        .GroupBy(h => h.PoitnSetupId)
                        .ToDictionary(g => g.Key, g => g.Sum(x => x.PointChange)); // âm

                    var cycles = _context.CustomerPointCycles
                        .Where(c => c.CustomerId == p.CardId)
                        .ToList();
                    foreach (var kv in historyGrouped)
                    {
                        int setupId = kv.Key;
                        double usedPoint = Math.Abs(kv.Value);

                        var cycle = cycles.FirstOrDefault(c => c.PoitnSetupId == setupId);
                        if (cycle != null)
                        {
                            cycle.RedeemedPoint -= usedPoint;
                            cycle.RemainingPoint += usedPoint;
                        }
                    }
                    CustomerPoint custPoint = new CustomerPoint
                    {
                        AddPoint = true,
                        CustomerId = p.CardId,
                        DocId = p.DocId,
                        DocType = 12,
                        Details = new List<CustomerPointLine>()
                    };

                    foreach (var h in historyList)
                    {
                        custPoint.Details.Add(new CustomerPointLine
                        {
                            CustomerId = h.CustomerId,
                            PoitnSetupId = h.PoitnSetupId,
                            DocId = h.DocId,
                            DocType = h.DocType,
                            PointChange = Math.Abs(h.PointChange),
                            ItemId = h.ItemId,
                            ItemCode = h.ItemCode,
                            ItemName = h.ItemName,
                            PointBefore = 0,
                            PointAfter = 0,
                            DocDate = DateTime.Now,
                            Note = $"Hủy đổi quà - Cộng {Math.Abs(h.PointChange)} điểm (Refund)"
                        });
                    }

                    custPoint.TotalPointChange = custPoint.Details.Sum(e => e.PointChange);
                    _context.CustomerPoint.Add(custPoint);
                }

                await _context.SaveChangesAsync();
                return null;
            }
            catch (Exception ex)
            {
                mess.Errors = ex.Message;
                mess.Status = 400;
                return mess;
            }
        }

        // ── Report constants ───────────────────────────────────────────────
        private static readonly string[] _orderStatusesForReport = { "DGH", "DHT", "DXN", "DGHR", "DTT" };
        private static readonly string[] _vpkmStatusesForReport = { "DGH", "DHT", "DXN", "DGHR", "DTT", "CXN", "DXL" };

        public async Task<(IEnumerable<ReportPoint>, int, Mess)> GetReportPoint(
            string? cardId, DateTime fromDate, DateTime toDate, int page = 1, int pageSize = 30)
        {
            try
            {
                // 1. Xác định scope: nếu user đăng nhập có CardId → report 1 khách hàng đó.
                //    Nếu không → admin xem report nhiều khách (filter theo cardId param hoặc tất cả).
                var loggedInCardId = GetLoggedInCardId();

                if (loggedInCardId.HasValue)
                {
                    var report = await BuildReportForCustomerAsync(loggedInCardId.Value, fromDate, toDate);
                    return (new[] { report }, 1, null);
                }

                return await BuildReportForAdminAsync(cardId, fromDate, toDate, page, pageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetReportPoint failed cardId={CardId}", cardId);
                return (null, 0, new Mess { Status = 900, Errors = ex.Message });
            }
        }

        // ── Helper: lấy CardId của user đăng nhập (nếu có) ─────────────────
        private int? GetLoggedInCardId()
        {
            var userIdStr = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out var userId)) return null;

            return _context.Users.AsNoTracking()
                .Where(u => u.Id == userId)
                .Select(u => u.CardId)
                .FirstOrDefault();
        }

        // ── Build report cho 1 customer (single-customer mode) ─────────────
        private async Task<ReportPoint> BuildReportForCustomerAsync(int customerId, DateTime fromDate, DateTime toDate)
        {
            var docIds = await GetEligibleDocIdsAsync(new[] { customerId });
            var histories = await LoadPointHistoriesAsync(new[] { customerId }, docIds, fromDate, toDate);
            var docMap = await LoadDocCodeMapAsync(histories.Select(h => h.DocId ?? 0).Distinct().ToArray());
            var setupMap = await LoadSetupMapAsync(histories.Select(h => h.PoitnSetupId).Distinct().ToArray());

            var cycles = await _context.CustomerPointCycles
                .AsNoTracking()
                .Where(e => e.CustomerId == customerId)
                .ToListAsync();

            var customer = await _context.BP.AsNoTracking()
                .Where(b => b.Id == customerId)
                .Select(b => new { b.CardCode, b.CardName })
                .FirstOrDefaultAsync();

            return AssembleReport(
                cardCode: customer?.CardCode ?? "",
                cardName: customer?.CardName ?? "",
                histories: histories,
                cycles: cycles,
                docMap: docMap,
                setupMap: setupMap);
        }

        // ── Build report đa khách hàng (admin mode) ────────────────────────
        private async Task<(IEnumerable<ReportPoint>, int, Mess)> BuildReportForAdminAsync(
            string? cardIdsCsv, DateTime fromDate, DateTime toDate, int page, int pageSize)
        {
            int[] customerIdsFilter = Array.Empty<int>();
            if (!string.IsNullOrWhiteSpace(cardIdsCsv))
            {
                var codes = cardIdsCsv.Split(',', StringSplitOptions.RemoveEmptyEntries);
                customerIdsFilter = await _context.BP.AsNoTracking()
                    .Where(b => codes.Contains(b.CardCode))
                    .Select(b => b.Id)
                    .ToArrayAsync();
            }

            var docIds = await GetEligibleDocIdsAsync(customerIdsFilter);

            var historyQuery = _context.CustomerPointLine.AsNoTracking()
                .Where(e => e.DocDate.HasValue
                            && e.DocDate.Value.Date >= fromDate.Date
                            && e.DocDate.Value.Date <= toDate.Date
                            && docIds.Contains(e.DocId ?? 0)
                            && (customerIdsFilter.Length == 0 || customerIdsFilter.Contains(e.CustomerId)));

            // Pagination theo customer (không phải theo line)
            var totalCount = await historyQuery.Select(e => e.CustomerId).Distinct().CountAsync();
            var pagedCustomerIds = await historyQuery
                .Select(e => e.CustomerId).Distinct()
                .OrderBy(id => id)
                .Skip((page - 1) * pageSize).Take(pageSize)
                .ToListAsync();

            var histories = await historyQuery
                .Where(e => pagedCustomerIds.Contains(e.CustomerId))
                .ToListAsync();

            var docMap = await LoadDocCodeMapAsync(histories.Select(h => h.DocId ?? 0).Distinct().ToArray());
            var setupMap = await LoadSetupMapAsync(histories.Select(h => h.PoitnSetupId).Distinct().ToArray());
            var customers = await _context.BP.AsNoTracking()
                .Where(b => pagedCustomerIds.Contains(b.Id))
                .Select(b => new { b.Id, b.CardCode, b.CardName })
                .ToListAsync();
            var cycles = await _context.CustomerPointCycles.AsNoTracking()
                .Where(c => pagedCustomerIds.Contains(c.CustomerId))
                .ToListAsync();

            var reports = histories
                .GroupBy(h => h.CustomerId)
                .Select(g =>
                {
                    var c = customers.FirstOrDefault(x => x.Id == g.Key);
                    var cycleForCust = cycles.Where(cy => cy.CustomerId == g.Key).ToList();
                    return AssembleReport(
                        cardCode: c?.CardCode ?? "",
                        cardName: c?.CardName ?? "",
                        histories: g.ToList(),
                        cycles: cycleForCust,
                        docMap: docMap,
                        setupMap: setupMap);
                })
                .ToList();

            return (reports, totalCount, null);
        }

        // ── Helper: tổng hợp ReportPoint từ raw data ───────────────────────
        // Chia history theo PoitnSetupId → mỗi group là 1 ReportPointSetupGroup.
        // Đây là nơi chính thực hiện business "tách báo cáo theo từng PointSetup".
        private static ReportPoint AssembleReport(
            string cardCode,
            string cardName,
            IList<CustomerPointLine> histories,
            IList<CustomerPointCycle> cycles,
            IDictionary<int, string> docMap,
            IDictionary<int, PointSetupReportInfo> setupMap)
        {
            var todayUtc = DateTime.UtcNow.Date;

            // Group histories theo setup
            var setupGroups = histories
                .GroupBy(h => h.PoitnSetupId)
                .Select(g =>
                {
                    setupMap.TryGetValue(g.Key, out var info);
                    var name = info?.Name ?? $"Setup #{g.Key}";

                    var lines = g
                        .Select(h => MapToReportLine(h, docMap, name))
                        .GroupBy(l => new { l.Type, l.InvoiceCode, l.DocDate })
                        .Select(grp => new ReportPointLine
                        {
                            PointSetupId = g.Key,
                            PointSetupName = name,
                            Type = grp.Key.Type,
                            InvoiceCode = grp.Key.InvoiceCode,
                            DocDate = grp.Key.DocDate,
                            Point = grp.Sum(x => x.Point)
                        })
                        .OrderByDescending(x => x.DocDate)
                        .ThenByDescending(x => x.InvoiceCode)
                        .ToList();

                    var cycleForSetup = cycles.Where(c => c.PoitnSetupId == g.Key).ToList();

                    return new ReportPointSetupGroup
                    {
                        PointSetupId = g.Key,
                        PointSetupName = name,
                        FromDate = info?.FromDate ?? DateTime.MinValue,
                        ToDate = info?.ToDate ?? DateTime.MinValue,
                        ExtendedToDate = info?.ExtendedToDate,
                        IsActive = (info?.ExtendedToDate ?? info?.ToDate)?.Date >= todayUtc,
                        TotalPoint = cycleForSetup.Sum(c => c.EarnedPoint),
                        RedeemPoint = cycleForSetup.Sum(c => c.RedeemedPoint),
                        RemainingPoint = cycleForSetup.Sum(c => c.RemainingPoint),
                        Lines = lines
                    };
                })
                .OrderBy(s => s.PointSetupName)
                .ToList();

            return new ReportPoint
            {
                CardCode = cardCode,
                CardName = cardName,
                TotalPoint = cycles.Sum(c => c.EarnedPoint),
                RedeemPoint = cycles.Sum(c => c.RedeemedPoint),
                RemainingPoint = cycles.Sum(c => c.RemainingPoint),
                Setups = setupGroups,
                // Backwards-compat: gộp tất cả lines từ mọi setup
                ReportPoints = setupGroups.SelectMany(s => s.Lines).ToList()
            };
        }

        private static ReportPointLine MapToReportLine(CustomerPointLine h, IDictionary<int, string> docMap, string setupName)
        {
            string type = h.DocType switch
            {
                22 when h.PointChange > 0 => "Đơn hàng tích điểm",
                22 when h.PointChange < 0 => "Hủy đơn hàng trừ điểm",
                12 when h.PointChange < 0 => "Đổi vật phẩm",
                12 when h.PointChange > 0 => "Hủy đổi vật phẩm",
                _ => "Khác"
            };

            docMap.TryGetValue(h.DocId ?? 0, out var invoiceCode);

            return new ReportPointLine
            {
                PointSetupId = h.PoitnSetupId,
                PointSetupName = setupName,
                Type = type,
                InvoiceCode = invoiceCode ?? "",
                DocDate = h.DocDate?.Date ?? DateTime.MinValue,
                Point = h.PointChange
            };
        }

        // ── Data loaders ────────────────────────────────────────────────────
        private async Task<int[]> GetEligibleDocIdsAsync(int[] customerIds)
        {
            var query = _context.ODOC.AsNoTracking()
                .Where(d =>
                    (_orderStatusesForReport.Contains(d.Status) && d.ObjType == 22) ||
                    (_vpkmStatusesForReport.Contains(d.Status) && d.ObjType == 12));

            if (customerIds.Length > 0)
                query = query.Where(d => customerIds.Contains(d.CardId ?? 0));

            return await query.Select(d => d.Id).ToArrayAsync();
        }

        private async Task<List<CustomerPointLine>> LoadPointHistoriesAsync(
            int[] customerIds, int[] docIds, DateTime fromDate, DateTime toDate)
        {
            return await _context.CustomerPointLine.AsNoTracking()
                .Where(e => e.DocDate.HasValue
                            && e.DocDate.Value.Date >= fromDate.Date
                            && e.DocDate.Value.Date <= toDate.Date
                            && customerIds.Contains(e.CustomerId)
                            && docIds.Contains(e.DocId ?? 0))
                .ToListAsync();
        }

        private async Task<IDictionary<int, string>> LoadDocCodeMapAsync(int[] docIds)
        {
            if (docIds.Length == 0) return new Dictionary<int, string>();
            return await _context.ODOC.AsNoTracking()
                .Where(d => docIds.Contains(d.Id))
                .Select(d => new { d.Id, d.InvoiceCode })
                .ToDictionaryAsync(d => d.Id, d => d.InvoiceCode ?? "");
        }

        private async Task<IDictionary<int, PointSetupReportInfo>> LoadSetupMapAsync(int[] setupIds)
        {
            if (setupIds.Length == 0) return new Dictionary<int, PointSetupReportInfo>();
            return await _context.PointSetups.AsNoTracking()
                .Where(s => setupIds.Contains(s.Id))
                .Select(s => new PointSetupReportInfo
                {
                    Id = s.Id,
                    Name = s.Name,
                    FromDate = s.FromDate,
                    ToDate = s.ToDate,
                    ExtendedToDate = s.ExtendedToDate
                })
                .ToDictionaryAsync(s => s.Id);
        }

        /// <summary>Internal DTO chỉ dùng để tổng hợp report — giảm memory so với load full PointSetup.</summary>
        private class PointSetupReportInfo
        {
            public int Id { get; set; }
            public string Name { get; set; } = "";
            public DateTime FromDate { get; set; }
            public DateTime ToDate { get; set; }
            public DateTime? ExtendedToDate { get; set; }
        }

        /// <summary>
        /// Public router theo business rules — gọi mỗi khi ODOC.Status thay đổi.
        /// Decoupled khỏi caller, dễ test, idempotent (đã có CustomerPoint cùng action → skip).
        /// </summary>
        public async Task OnDocumentStatusChangedAsync(int docId, int customerId, int objType, string newStatus, CancellationToken ct = default)
        {
            using var scope = _logger.BeginScope("PointStatusChange Doc={DocId} Cust={CustomerId} ObjType={ObjType} Status={Status}",
                docId, customerId, objType, newStatus);

            await using var trans = await _context.Database.BeginTransactionAsync(ct);
            try
            {
                if (objType == PointObjTypes.Order)
                {
                    if (PointStatuses.IsOrderCompleted(newStatus))
                    {
                        await ConfirmOrderEarnAsync(docId, customerId, ct);
                    }
                    else if (PointStatuses.IsReverse(newStatus))
                    {
                        await ReverseOrderEarnAsync(docId, customerId, ct);
                    }
                    else
                    {
                        _logger.LogDebug("No-op: order status không thuộc tích/hoàn điểm");
                    }
                }
                else if (objType == PointObjTypes.Vpkm)
                {
                    if (PointStatuses.IsReverse(newStatus))
                    {
                        await ReverseVpkmRedeemAsync(docId, customerId, ct);
                    }
                    // VPKM: trừ điểm xảy ra ở RedeemPoints khi tạo đơn DXN — không trigger ở đây.
                }

                await _context.SaveChangesAsync(ct);
                await trans.CommitAsync(ct);
            }
            catch (Exception ex)
            {
                await trans.RollbackAsync(ct);
                _logger.LogError(ex, "OnDocumentStatusChangedAsync failed");
                throw;
            }
        }

        /// <summary>
        /// DHT: chuyển CustomerPoint pending (đã tạo lúc đặt đơn DXN) thành điểm thực vào cycle.
        /// Idempotent: nếu cycle đã được +điểm cho doc này thì skip.
        /// </summary>
        private async Task ConfirmOrderEarnAsync(int docId, int customerId, CancellationToken ct)
        {
            var custPoint = await _context.CustomerPoint
                .Include(e => e.Details)
                .FirstOrDefaultAsync(e =>
                    e.CustomerId == customerId &&
                    e.DocId == docId &&
                    e.DocType == PointObjTypes.Order &&
                    e.AddPoint == true, ct);

            if (custPoint == null || custPoint.Details.Count == 0)
            {
                _logger.LogWarning("Không có CustomerPoint pending để confirm — bỏ qua");
                return;
            }

            // Idempotent guard: đã có cycle update cho doc này chưa? Dùng Note marker.
            const string ConfirmedMarker = "[CONFIRMED]";
            if (custPoint.Details.Any(d => (d.Note ?? "").Contains(ConfirmedMarker)))
            {
                _logger.LogInformation("Order DHT đã được confirm trước đây — skip");
                return;
            }

            var grouped = custPoint.Details
                .GroupBy(d => d.PoitnSetupId)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.PointChange));

            var setupIds = grouped.Keys.ToList();
            var setups = await _context.PointSetups
                .Where(s => setupIds.Contains(s.Id))
                .ToListAsync(ct);

            foreach (var setup in setups)
            {
                var cycle = await GetOrCreateCycleAsync(customerId, setup, ct);
                var sum = grouped[setup.Id];

                // Invariant: EarnedPoint >= RemainingPoint → set EarnedPoint trước (tăng)
                cycle.EarnedPoint += sum;
                cycle.RemainingPoint += sum;
            }

            // Đánh dấu đã confirm để chống double-add
            foreach (var d in custPoint.Details)
            {
                d.Note = $"{ConfirmedMarker} {d.Note}";
            }
        }

        /// <summary>
        /// HUY/DONG/HUY2 cho order: hoàn lại điểm vào cycle, tạo reverse history.
        /// Chỉ reverse nếu trước đó đã ConfirmEarn (DHT) — nếu chưa, chỉ xoá pending.
        /// </summary>
        private async Task ReverseOrderEarnAsync(int docId, int customerId, CancellationToken ct)
        {
            var custPoint = await _context.CustomerPoint
                .Include(e => e.Details)
                .FirstOrDefaultAsync(e =>
                    e.CustomerId == customerId &&
                    e.DocId == docId &&
                    e.DocType == PointObjTypes.Order &&
                    e.AddPoint == true, ct);

            if (custPoint == null || custPoint.Details.Count == 0) return;

            const string ConfirmedMarker = "[CONFIRMED]";
            bool wasConfirmed = custPoint.Details.Any(d => (d.Note ?? "").Contains(ConfirmedMarker));

            if (!wasConfirmed)
            {
                // Pending chưa confirm → chỉ cần xoá CustomerPoint, không touch cycle
                _logger.LogInformation("Order pending bị huỷ trước khi hoàn thành — xoá pending");
                _context.CustomerPointLine.RemoveRange(custPoint.Details);
                _context.CustomerPoint.Remove(custPoint);
                return;
            }

            // Idempotent: kiểm tra đã reverse chưa
            var alreadyReversed = await _context.CustomerPoint
                .AnyAsync(e => e.CustomerId == customerId && e.DocId == docId &&
                          e.DocType == PointObjTypes.Order && e.AddPoint == false, ct);
            if (alreadyReversed)
            {
                _logger.LogInformation("Order đã được reverse trước đây — skip");
                return;
            }

            var grouped = custPoint.Details
                .GroupBy(d => d.PoitnSetupId)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.PointChange));

            foreach (var kv in grouped)
            {
                var cycle = await _context.CustomerPointCycles
                    .Where(c => c.CustomerId == customerId && c.PoitnSetupId == kv.Key && c.Status == 0)
                    .OrderBy(c => c.Id)
                    .FirstOrDefaultAsync(ct);

                if (cycle == null) continue;

                // Invariant: EarnedPoint >= RemainingPoint luôn → phải set RemainingPoint trước
                var newRemaining = Math.Max(0, cycle.RemainingPoint - kv.Value);
                var newEarned = Math.Max(0, cycle.EarnedPoint - kv.Value);
                cycle.RemainingPoint = newRemaining;
                cycle.EarnedPoint = Math.Max(newEarned, newRemaining);
            }

            var reverseDetails = custPoint.Details
                .Where(d => d.PointChange > 0)
                .Select(d => new CustomerPointLine
                {
                    CustomerId = d.CustomerId,
                    PoitnSetupId = d.PoitnSetupId,
                    DocId = d.DocId,
                    DocType = d.DocType,
                    PointChange = -d.PointChange,
                    ItemId = d.ItemId,
                    ItemCode = d.ItemCode,
                    ItemName = d.ItemName,
                    PointBefore = 0,
                    PointAfter = 0,
                    DocDate = DateTime.UtcNow,
                    Note = $"Hoàn điểm do huỷ/đóng đơn"
                })
                .ToList();

            if (reverseDetails.Count > 0)
            {
                _context.CustomerPoint.Add(new CustomerPoint
                {
                    CustomerId = customerId,
                    DocId = docId,
                    DocType = PointObjTypes.Order,
                    AddPoint = false,
                    Details = reverseDetails,
                    TotalPointChange = reverseDetails.Sum(x => x.PointChange)
                });
            }
        }

        /// <summary>
        /// HUY/DONG/HUY2 cho VPKM: hoàn điểm về đúng cycle đã trừ FIFO trước đó.
        /// Theo đúng từng cycle thay vì dồn về cycle đầu (fix bug RFC #3.3).
        /// </summary>
        private async Task ReverseVpkmRedeemAsync(int docId, int customerId, CancellationToken ct)
        {
            var redeemPoint = await _context.CustomerPoint
                .Include(e => e.Details)
                .FirstOrDefaultAsync(e =>
                    e.CustomerId == customerId &&
                    e.DocId == docId &&
                    e.DocType == PointObjTypes.Vpkm &&
                    e.AddPoint == false, ct);

            if (redeemPoint == null || redeemPoint.Details.Count == 0)
            {
                _logger.LogWarning("Không có redemption để reverse");
                return;
            }

            // Idempotent check
            var alreadyReversed = await _context.CustomerPoint
                .AnyAsync(e => e.CustomerId == customerId && e.DocId == docId &&
                          e.DocType == PointObjTypes.Vpkm && e.AddPoint == true, ct);
            if (alreadyReversed)
            {
                _logger.LogInformation("VPKM đã reverse trước đây — skip");
                return;
            }

            // Group theo cycle gốc đã trừ (PoitnSetupId), refund đúng từng cycle
            var perSetup = redeemPoint.Details
                .Where(d => d.PointChange < 0)
                .GroupBy(d => d.PoitnSetupId)
                .Select(g => new { SetupId = g.Key, Used = -g.Sum(x => x.PointChange) }) // dương
                .ToList();

            foreach (var grp in perSetup)
            {
                // Hoàn về cycle Active đầu tiên của setup. Nếu nhiều cycle, hoàn vào cycle gần nhất tới ExpiryDate.
                var cycle = await _context.CustomerPointCycles
                    .Where(c => c.CustomerId == customerId && c.PoitnSetupId == grp.SetupId)
                    .OrderByDescending(c => c.ExpiryDate)
                    .FirstOrDefaultAsync(ct);

                if (cycle == null) continue;

                cycle.RedeemedPoint -= grp.Used;
                cycle.RemainingPoint += grp.Used;
                if (cycle.RedeemedPoint < 0) cycle.RedeemedPoint = 0;
            }

            var refundDetails = redeemPoint.Details
                .Where(d => d.PointChange < 0)
                .Select(d => new CustomerPointLine
                {
                    CustomerId = d.CustomerId,
                    PoitnSetupId = d.PoitnSetupId,
                    DocId = d.DocId,
                    DocType = d.DocType,
                    PointChange = -d.PointChange, // dương
                    ItemId = d.ItemId,
                    ItemCode = d.ItemCode,
                    ItemName = d.ItemName,
                    DocDate = DateTime.UtcNow,
                    Note = $"Hoàn điểm do huỷ đổi VPKM"
                })
                .ToList();

            if (refundDetails.Count > 0)
            {
                _context.CustomerPoint.Add(new CustomerPoint
                {
                    CustomerId = customerId,
                    DocId = docId,
                    DocType = PointObjTypes.Vpkm,
                    AddPoint = true,
                    Details = refundDetails,
                    TotalPointChange = refundDetails.Sum(x => x.PointChange)
                });
            }
        }

        /// <summary>So sánh tập Customer giữa entity (hiện tại) và DTO (request) — true nếu có thay đổi.</summary>
        private static bool HasCustomersChanged(
            ICollection<PointSetupCustomer> existing,
            IEnumerable<PointSetupCustomerUpdateDto>? incoming)
        {
            var inc = incoming?.ToList() ?? new List<PointSetupCustomerUpdateDto>();
            if (existing.Count != inc.Count) return true;

            // Compare bằng key (Type, CustomerId) bất kể thứ tự
            var existKeys = existing.Select(c => (c.Type ?? "", c.CustomerId)).OrderBy(x => x).ToList();
            var incKeys = inc.Select(c => (c.Type ?? "", c.CustomerId)).OrderBy(x => x).ToList();
            return !existKeys.SequenceEqual(incKeys);
        }

        /// <summary>So sánh tập Line — true nếu có thay đổi (số lượng, point, hoặc tập filter).</summary>
        private static bool HasLinesChanged(
            ICollection<PointSetupLine> existing,
            IEnumerable<PointSetupLineUpdateDto>? incoming)
        {
            var inc = incoming?.ToList() ?? new List<PointSetupLineUpdateDto>();
            if (existing.Count != inc.Count) return true;

            // So sánh từng line theo cặp Point + tập IndustryIds + BrandIds + PackingIds + ItemType
            // Đơn giản hoá: bất cứ thay đổi point hoặc tập filter nào đều là "changed".
            var existSorted = existing.OrderBy(l => l.Id).ToList();
            var incSorted = inc.OrderBy(l => l.Id).ToList();

            for (int i = 0; i < existSorted.Count; i++)
            {
                var e = existSorted[i];
                var d = incSorted[i];

                if (Math.Abs(e.Point - d.Point) > 0.0001) return true;

                var eIndustries = (e.Industry?.Select(x => x.IndustryId) ?? Array.Empty<int>()).OrderBy(x => x).ToList();
                var dIndustries = (d.IndustryIds ?? new List<int>()).OrderBy(x => x).ToList();
                if (!eIndustries.SequenceEqual(dIndustries)) return true;

                var eBrands = (e.Brands?.Select(x => x.BrandId) ?? Array.Empty<int>()).OrderBy(x => x).ToList();
                var dBrands = (d.BrandIds ?? new List<int>()).OrderBy(x => x).ToList();
                if (!eBrands.SequenceEqual(dBrands)) return true;

                var ePackings = (e.Packings?.Select(x => x.PackingId) ?? Array.Empty<int>()).OrderBy(x => x).ToList();
                var dPackings = (d.PackingIds ?? new List<int>()).OrderBy(x => x).ToList();
                if (!ePackings.SequenceEqual(dPackings)) return true;

                var eItems = (e.ItemType?.Select(x => (x.ItemType ?? "", x.ItemId, x.ItemCode ?? "")) ?? Array.Empty<(string, int, string)>())
                    .OrderBy(x => x).ToList();
                var dItems = (d.ItemType?.Select(x => (x.ItemType ?? "", x.ItemId, x.ItemCode ?? "")) ?? Array.Empty<(string, int, string)>())
                    .OrderBy(x => x).ToList();
                if (!eItems.SequenceEqual(dItems)) return true;
            }

            return false;
        }

        private async Task<CustomerPointCycle> GetOrCreateCycleAsync(int customerId, PointSetup setup, CancellationToken ct)
        {
            var cycle = await _context.CustomerPointCycles
                .FirstOrDefaultAsync(x => x.CustomerId == customerId &&
                                          x.PoitnSetupId == setup.Id &&
                                          x.Status == 0, ct);
            if (cycle != null) return cycle;

            cycle = new CustomerPointCycle
            {
                CustomerId = customerId,
                PoitnSetupId = setup.Id,
                StartDate = setup.FromDate,
                EndDate = setup.ToDate,
                ExpiryDate = setup.ExtendedToDate ?? setup.ToDate,
                EarnedPoint = 0,
                RedeemedPoint = 0,
                RemainingPoint = 0,
                Status = 0
            };
            _context.CustomerPointCycles.Add(cycle);
            return cycle;
        }
    }
}
