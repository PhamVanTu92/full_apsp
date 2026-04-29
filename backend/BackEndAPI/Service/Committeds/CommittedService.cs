using System.Runtime.CompilerServices;
using AutoMapper;
using BackEndAPI.Controllers;
using BackEndAPI.Data;
using BackEndAPI.Models.Branchs;
using BackEndAPI.Models.Committed;
using BackEndAPI.Models.Document;
using BackEndAPI.Models.ItemGroups;
using BackEndAPI.Models.ItemMasterData;
using BackEndAPI.Models.NotificationModels;
using BackEndAPI.Models.Other;
using BackEndAPI.Service.Document;
using BackEndAPI.Service.EventAggregator;
using Gridify;
using LinqKit;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using MimeKit.Cryptography;

namespace BackEndAPI.Service.Committeds
{
    public class CommittedService(
        AppDbContext context,
        IModelUpdater modelUpdater,
        IMapper mapper,
        IWebHostEnvironment webHostEnvironment,
        IEventAggregator eventAggregator,
        IHttpContextAccessor httpContextAccessor)
        : Service<Committed>(context), ICommittedService
    {
        private readonly AppDbContext _context = context;
        private readonly IWebHostEnvironment _webHostEnvironment = webHostEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        private readonly IModelUpdater _modelUpdater = modelUpdater;
        private readonly IMapper _mapper = mapper;
        private readonly IEventAggregator _eventAggregator = eventAggregator;

        public async Task<(Committed?, Mess?)> CreateCommited(Committed model)
        {
            var mess = new Mess();
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var existsCommitedInYear = await _context.Committed.AsNoTracking()
                    .Where(c => c.CardId == model.CardId &&
                                c.CommittedYear.Value.Year == model.CommittedYear.Value.Year && c.DocStatus != "R")
                    .CountAsync();
                if (existsCommitedInYear > 0)
                {
                    mess.Errors = $"Trong năm {model.CommittedYear.Value.AddDays(10).Year} đã có cam kết.";
                    mess.Status = 400;
                    await transaction.RollbackAsync();
                    return (null, mess);
                }

                var maxCode = _context.Committed
                    .Where(c => c.CommittedCode.StartsWith($"CKSL{model.CardCode}"))
                    .OrderByDescending(c => c.CommittedCode)
                    .Select(c => c.CommittedCode)
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

                if (string.IsNullOrEmpty(model.CommittedCode))
                {
                    model.CommittedCode = $"CKSL{model.CardCode}{newNumber:0000000}";
                }


                foreach (var line in model!.CommittedLine!)
                {
                    foreach (var lineSub in line?.CommittedLineSub!)
                    {
                        lineSub.Brand = new List<Brand>();
                        lineSub.Brand = await _context.Brand.Where(x => lineSub.BrandIds.Contains(x.Id)).ToListAsync();

                        lineSub.ItemTypes = await _context.ItemType.Where(x => lineSub.ItemTypeIds.Contains(x.Id))
                            .ToListAsync();
                    }
                }

                _context.Committed.Add(model);
                await _context.SaveChangesAsync();
                var userIds = await _context.AppUser.Where(u => u.CardId == model.CardId).Select(u => u.Id)
                    .ToListAsync();
                await transaction.CommitAsync();
                if (model.DocStatus == "P")
                {
                    _eventAggregator.Publish(new Models.NotificationModels.Notification
                    {
                        Message = "Xác nhận cam kết sản lượng",
                        Title = $"Cam kết sản lượng mã {model.CommittedCode} cần bạn xác nhận",
                        Type = "info",
                        SendUsers = userIds,
                        Object = new NotificationObject
                        {
                            ObjId = model.Id,
                            ObjName = "Sale Forecast",
                            ObjType = "commited",
                        }
                    });
                }

                return (model, null);
            }
            catch (DbUpdateException ex)
            {
                mess.Status = 400;
                if (ex.InnerException is SqlException sqlEx)
                {
                    // Xử lý lỗi SQL
                    switch (sqlEx.Number)
                    {
                        case 2627: // Vi phạm ràng buộc UNIQUE
                        case 2601: // Vi phạm ràng buộc UNIQUE khác
                            mess.Errors = "Mã hoặc tên của cam kết đã tồn tại";
                            break;
                        default:
                            mess.Errors = $"SQL Error {sqlEx.Number}: {sqlEx.Message}";
                            break;
                    }
                }
                else
                {
                    // Xử lý các lỗi khác
                    mess.Errors = $"An error occurred: {ex.Message}";
                }

                await transaction.RollbackAsync();
                return (null, mess);
            }
            catch (Exception ex)
            {
                mess.Errors = ex.Message;
                mess.Status = 500;
                await transaction.RollbackAsync();
                return (null, mess);
            }
        }

        public async Task<(IEnumerable<Committed>?, int total, Mess?)> GetCommitted(int skip, int limit, string? search,
            GridifyQuery q, int cardId)
        {
            var mess = new Mess();
            try
            {
                var kk = _context.AppUser.FirstOrDefault();
                var query = _context.Committed.AsNoTracking().AsQueryable();
                if (!string.IsNullOrEmpty(search))
                {
                    search = search.Trim();
                    query = query.Where(p =>
                        p.CommittedName.ToLower().Contains(search.ToLower()) ||
                        p.CommittedCode.ToLower().Contains(search.ToLower())
                        || p.BP!.CardName!.ToLower().Contains(search.ToLower()));
                }

                if (cardId != 0)
                {
                    query = query.Where(p => p.BP.Id == cardId && p.DocStatus != "D");
                }

                var count = await query.ApplyFiltering(q).CountAsync();
                q.Page = skip + 1;
                q.PageSize = limit;
                q.OrderBy = q.OrderBy ?? "Id desc";
                var filteredQuery = query.ApplyFiltering(q);
                var total = await filteredQuery.CountAsync();
                var sortedQuery = filteredQuery
                     .ApplyOrdering(q)
                     .ApplyPaging(q);

                var data = await sortedQuery
                    .Include(e => e.Creator)
                    .Include(e => e.BP)
                    .ToListAsync();
                //var data = await query
                //    .Include(p => p.BP)
                //    .Include(p => p.Creator)
                //    .Include(p => p.CommittedLine)
                //    .ThenInclude(p => p.CommittedLineSub)
                //    .ThenInclude(p => p.Industry)
                //    .Include(p => p.CommittedLine)
                //    .ThenInclude(p => p.CommittedLineSub)
                //    .ThenInclude(p => p.Brand)
                //    .Include(p => p.CommittedLine)
                //    .ThenInclude(p => p.CommittedLineSub)
                //    .ThenInclude(p => p.CommittedLineSubSub)
                //    .OrderByDescending(p => p.Id)
                //    .ApplyFiltering(q)//.Skip(skip * limit).Take(limit)
                //    .ApplyPaging(q)
                //    .ToListAsync();
                return (data, count, null);
            }
            catch (Exception ex)
            {
                mess.Errors = ex.Message;
                mess.Status = 500;
                return (null, 0, mess);
            }
        }

        public async Task<(Committed?, Mess?)> GetCommitedById(int id)
        {
            var mess = new Mess();
            try
            {
                var query = _context.Committed.AsQueryable();
                var data = await query
                    .Include(p => p.BP)
                    .Include(p => p.CommittedLine)!
                    .ThenInclude(p => p.CommittedLineSub)!
                    .ThenInclude(p => p.Industry)
                    .Include(p => p.CommittedLine)!
                    .ThenInclude(p => p.CommittedLineSub)!
                    .ThenInclude(p => p.Brand)!
                    .Include(p => p.CommittedLine)!
                    .ThenInclude(p => p.CommittedLineSub)!
                    .ThenInclude(p => p.CommittedLineSubSub)
                    .Include(p => p.CommittedLine)!
                    .ThenInclude(p => p.CommittedLineSub)!
                    .ThenInclude(p => p.ItemTypes)
                    .Where(p => p.Id == id)
                    .OrderByDescending(p => p.Id)
                    .FirstOrDefaultAsync();
                return (data, null);
            }
            catch (Exception ex)
            {
                // mess.Errors = ex.Message;
                mess.Status = 500;
                return (null, mess);
            }
        }

        public async Task<(Committed?, Mess?)> UpdateCommitted(int id, Committed model)
        {
            var mess = new Mess();
            model.Creator = null;
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var comm =
                    await _context.Committed
                        .AsNoTracking()
                        .Include(p => p.CommittedLine)!
                        .ThenInclude(p => p.CommittedLineSub)!
                        .ThenInclude(p => p.CommittedLineSubSub)
                        .Include(p => p.CommittedLine)
                        .ThenInclude(p => p.CommittedLineSub)
                        .ThenInclude(p => p.Brand)
                        .Include(p => p.CommittedLine)
                        .ThenInclude(p => p.CommittedLineSub)
                        .ThenInclude(p => p.ItemTypes)
                        .Where(p => p.Id == id)
                        .FirstOrDefaultAsync();

                if (comm == null)
                {
                    mess.Errors = "The commit was not found.";
                    mess.Status = 404;
                    await transaction.RollbackAsync();
                    return (null, mess);
                }

                if (comm.DocStatus == "A" || comm.DocStatus == "C" || comm.DocStatus == "P")
                {
                    mess.Errors = "Cam kết đã được xác nhận hoặc hủy";
                    mess.Status = 403;
                    await transaction.RollbackAsync();
                    return (null, mess);
                }

                if (model.UserType != "APSP")
                {
                    mess.Errors = "Bạn không thể cập nhật được cam kết này!";
                    mess.Status = 403;
                    await transaction.RollbackAsync();
                    return (null, mess);
                }


                #region draf

                _mapper.Map(model, comm);
                if (model.CommittedLine != null && model.CommittedLine.Count != 0)
                {
                    foreach (var line in model.CommittedLine)
                    {
                        if (line.Status == "A" || line.Id == 0)
                        {
                            if (comm.CommittedLine == null) comm.CommittedLine = new List<CommittedLine>();
                            comm.CommittedLine.Add(line);
                        }
                        else if (line.Status == "U")
                        {
                            var rLine = comm?.CommittedLine?.FirstOrDefault(p => p.Id == line.Id);
                            _mapper.Map(line, rLine);
                            foreach (var sub in line?.CommittedLineSub!)
                            {
                                if (sub.Status == "A" || sub.Id == 0) rLine?.CommittedLineSub?.Add(sub);
                                else if (sub.Status == "U")
                                {
                                    var rSub = rLine?.CommittedLineSub?.FirstOrDefault(p => p.Id == sub.Id);
                                    _mapper.Map(sub, rSub);
                                    var addItemType = await _context.ItemType.Where(x =>
                                        sub.ItemTypeIds.Contains(x.Id) &&
                                        !rSub.ItemTypes.Select(r => r.Id).Contains(x.Id)).ToListAsync();
                                    rSub.ItemTypes = addItemType;
                                    // rSub.ItemTypes.RemoveAll(r => !sub.ItemTypeIds.Contains(r.Id));
                                    // rSub.ItemTypes = await _context.ItemType.Where(x => sub.ItemTypeIds.Contains(x.Id))
                                    //     .ToListAsync();

                                    var addBrand = await _context.Brand.Where(x =>
                                        sub.BrandIds.Contains(x.Id) &&
                                        !rSub.Brand.Select(r => r.Id).Contains(x.Id)).ToListAsync();

                                    rSub.Brand = addBrand;

                                    foreach (var subsub in sub.CommittedLineSubSub)
                                    {
                                        if (subsub.Status == "A" || subsub.Id == 0)
                                            rSub?.CommittedLineSubSub?.Add(subsub);
                                        else if (subsub.Status == "U")
                                        {
                                            _mapper.Map(subsub,
                                                rSub?.CommittedLineSubSub.FirstOrDefault(p => p.Id == subsub.Id));
                                        }
                                        else if (subsub.Status == "D")
                                        {
                                            var rm = rSub.CommittedLineSubSub.FirstOrDefault(p => p.Id == subsub.Id);
                                            if (rm != null)
                                            {
                                                rSub?.CommittedLineSubSub?.Remove(rm);
                                                _context.CommittedLineSubSub.Remove(rm);
                                            }
                                        }
                                    }
                                }
                                else if (sub.Status == "D")
                                {
                                    var rm = rLine?.CommittedLineSub?.FirstOrDefault(p => p.Id == sub.Id);
                                    if (rm != null)
                                    {
                                        rLine?.CommittedLineSub?.Remove(rm);
                                        _context.CommittedLineSub.Remove(rm);
                                    }
                                }
                            }
                        }
                        else if (line.Status == "D")
                        {
                            var rm = comm.CommittedLine?.FirstOrDefault(p => p.Id == line.Id);
                            if (rm != null)
                            {
                                comm.CommittedLine?.Remove(rm);
                                _context.CommittedLine.Remove(rm);
                            }
                        }
                    }
                }

                #endregion

                _context.Committed.Update(comm);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                var userIds = await _context.AppUser.AsNoTracking().Where(u => u.CardId == comm.CardId)
                    .Select(u => u.Id)
                    .ToListAsync();
                if (model.DocStatus == "P")
                {
                    _eventAggregator.Publish(new Models.NotificationModels.Notification
                    {
                        Message = "Xác nhận cam kết sản lượng",
                        Title = $"Cam kết sản lượng mã {comm.CommittedCode} cần bạn xác nhận",
                        Type = "info",
                        SendUsers = userIds,
                        Object = new NotificationObject
                        {
                            ObjId = comm.Id,
                            ObjName = "Sale Forecast",
                            ObjType = "commited",
                        }
                    });
                }

                return (comm, null);
            }
            catch (Exception ex)
            {
                mess.Errors = ex.Message;
                mess.Status = 500;
                await transaction.RollbackAsync();

                return (null, mess);
            }
        }

        public async Task<(Committed?, Mess?)> AddLineToCommited(int id, CommittedLine model)
        {
            var mess = new Mess();
            try
            {
                var comm = await _context.Committed.Include(committed => committed.CommittedLine)
                    .FirstOrDefaultAsync(p => p.Id == id);
                if (comm == null)
                {
                    mess.Status = 404;
                    mess.Errors = "The commit was not found.";
                    return (null, mess);
                }

                if (comm?.CommittedLine == null)
                    if (comm != null)
                        comm.CommittedLine = new List<CommittedLine>();

                comm?.CommittedLine?.Add(model);

                await _context.SaveChangesAsync();
                return (comm, null);
            }
            catch (Exception ex)
            {
                mess.Errors = ex.Message;
                mess.Status = 500;

                return (null, mess);
            }
        }

        public async Task<(Committed?, Mess?)> UpdateLineFromCommited(int id, int lineId, CommittedLine model)
        {
            var mess = new Mess();
            try
            {
                var comm = await _context.Committed.Include(committed => committed.CommittedLine)
                    .FirstOrDefaultAsync(p => p.Id == id);
                if (comm == null)
                {
                    mess.Status = 404;
                    mess.Errors = "The commit was not found.";
                    return (null, mess);
                }

                var line = comm.CommittedLine?.FirstOrDefault(p => p.Id == lineId);
                if (line == null)
                {
                    mess.Status = 404;
                    mess.Errors = "The commit line was not found.";
                    return (null, mess);
                }

                _mapper.Map(model, line);

                await _context.SaveChangesAsync();
                return (comm, null);
            }
            catch (Exception ex)
            {
                mess.Errors = ex.Message;
                mess.Status = 500;
                return (null, mess);
            }
        }

        public async Task UpdateVolumn(int bpId, List<DOC1> items)
        {
            var commited = await _context.Committed
                .Where(x => x.CardId == bpId)
                .Include(committed => committed.CommittedLine)
                .ThenInclude(committedLine => committedLine.CommittedLineSub)
                .ThenInclude(committedLineSub => committedLineSub.Brand)
                .Include(committed => committed.CommittedLine)
                .ThenInclude(committedLine => committedLine.CommittedLineSub)
                .ThenInclude(committedLineSub => committedLineSub.ItemTypes)
                .Include(committed => committed.CommittedLine)
                .ThenInclude(committedLine => committedLine.CommittedLineSub)
                .ThenInclude(committedLineSub => committedLineSub.Industry)
                .Include(committed => committed.CommittedLine)
                .ThenInclude(committedLine => committedLine.CommittedLineSub)
                .ThenInclude(c => c.CommittedLineSubSub)
                .FirstOrDefaultAsync(p => p.CommittedYear!.Value.Year == DateTime.Now.Year && p.DocStatus == "A");

            if (commited == null)
            {
                return;
            }

            commited.CommittedLine?.ForEach
            (
                root =>
                {
                    root.CommittedLineSub?.ForEach(xx =>
                    {
                        var query = items
                            .Where(x => x.Item?.IndustryId == xx.IndustryId).AsQueryable();

                        if (xx.Brand.Count != 0)
                        {
                            query = query.Where(x => xx.Brand.Select(b => b.Id).Contains(x.Item.BrandId.Value));
                        }

                        if (xx.ItemTypes.Count != 0)
                        {
                            query = query.Where(x =>
                                xx.ItemTypes.Select(b => b.Id).Contains(x.Item.ItemTypeId.Value));
                        }

                        string propName = "";
                        string propDiscountName = "";
                        if (root.CommittedType == "Q")
                        {
                            var curQ = (DateTime.Today.Month - 1) / 3 + 1;
                            propName = $"Quarter{curQ}";
                            propDiscountName = "Discount";
                        }

                        if (root.CommittedType == "Y")
                        {
                            var curM = DateTime.Now.Month;
                            propName = $"Month{curM}";
                            propDiscountName = "DiscountMonth";
                        }

                        var totalafter = query.ToList();


                        var addVolumn = totalafter.Sum(x => (x.Item?.Packing?.Volumn.Value * x.Quantity ?? 0));
                        xx.CurrentVolumn += addVolumn;

                        var prop = typeof(CommittedLineSub).GetProperty(propName);
                        var propTotalName = $"Total{propName}";
                        var propTotal = typeof(CommittedLineSub).GetProperty(propTotalName);
                        var propDiscount = typeof(CommittedLineSub).GetProperty(propDiscountName);

                        var isVol = (double)propTotal.GetValue(xx);
                        propTotal.SetValue(xx, isVol + addVolumn);
                    });
                }
            );

            await _context.SaveChangesAsync();
        }

        public async Task<Mess?> UpdateStatus(int cmmId, string status, string note)
        {
            var mess = new Mess();
            try
            {
                var queryUser = _context.AppUser.AsNoTracking().AsQueryable();
                var plan = await _context.Committed.FirstOrDefaultAsync(p => p.Id == cmmId);
                if (plan == null)
                {
                    mess.Errors = $"Cam kết sản lượng với ID {cmmId} không tồn tại";
                    mess.Status = 404;
                    return mess;
                }

                if (plan.DocStatus == "C" || plan.DocStatus == "A")
                {
                    mess.Errors = $"Cam kết đã được xác nhận hoặc hủy";
                    mess.Status = 400;
                    return mess;
                }

                plan.DocStatus = status;
                if (status == "R")
                    plan.RejectReason = note;
                string message = "", title = "";
                if (status == "A")
                {
                    message = $"Cam kết sản lượng {plan.CommittedCode} đã được xác nhận";
                    title = $"Cam kết sản lượng được xác nhận";
                }
                else if (status == "C")
                {
                    message = $"Cam kết sản lượng {plan.CommittedCode} đã bị hủy";
                    title = $"Cam kết sản lượng bị hủy";
                }
                else if (status == "R")
                {
                    message = $"Cam kết sản lượng {plan.CommittedCode} đã bị từ chối";
                    title = $"Cam kết sản lượng bị từ chối";
                }

                if (status == "C")
                {
                    queryUser = queryUser.Where(p => p.CardId == plan.CardId);
                }
                else
                {
                    queryUser = queryUser.Where(p => p.UserType == "APSP");
                }

                await _context.SaveChangesAsync();
                var userIds = await queryUser.Select(p => p.Id).ToListAsync();
                _eventAggregator.Publish(new Models.NotificationModels.Notification
                {
                    Message = message,
                    Title = title,
                    Type = "info",
                    SendUsers = userIds,
                    Object = new NotificationObject
                    {
                        ObjId = plan.Id,
                        ObjName = "Commited",
                        ObjType = "commited",
                    }
                });
                return null;
            }
            catch (Exception ex)
            {
                mess.Errors = ex.Message;
                mess.Status = 500;
                return mess;
            }
        }

        public async Task<Committed> GetCommitedDiscount(int bpId, List<ItemChecking> items)
        {
            var mess = new Mess();
            try
            {
                var commited = await _context.Committed.AsNoTracking()
                    .Where(x => x.CardId == bpId)
                    .Include(committed => committed.CommittedLine)
                    .ThenInclude(committedLine => committedLine.CommittedLineSub)
                    .ThenInclude(committedLineSub => committedLineSub.Brand)
                    .Include(committed => committed.CommittedLine)
                    .ThenInclude(committedLine => committedLine.CommittedLineSub)
                    .ThenInclude(committedLineSub => committedLineSub.ItemTypes)
                    .Include(committed => committed.CommittedLine)
                    .ThenInclude(committedLine => committedLine.CommittedLineSub)
                    .ThenInclude(committedLineSub => committedLineSub.Industry)
                    .Include(committed => committed.CommittedLine)
                    .ThenInclude(committedLine => committedLine.CommittedLineSub)
                    .ThenInclude(c => c.CommittedLineSubSub)
                    .FirstOrDefaultAsync(p => p.CommittedYear!.Value.Year == DateTime.Now.Year && p.DocStatus == "A");
                if (commited == null)
                {
                    return null;
                }
                foreach (var line in commited.CommittedLine)
                {
                    foreach (var sub in line.CommittedLineSub)
                    {
                        sub.CurrentVolumn = (sub.Package ?? 0);
                    }
                }
                return commited;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<Mess?> DeleteCommited(int cmId)
        {
            var mess = new Mess();
            try
            {
                var cm = await _context.Committed.FirstOrDefaultAsync(p => p.Id == cmId);
                if (cm == null)
                {
                    mess.Errors = "The commit was not found.";
                    mess.Status = 404;
                    return mess;
                }

                _context.Committed.Remove(cm);

                await _context.SaveChangesAsync();
                return null;
            }
            catch (Exception ex)
            {
                mess.Errors = ex.Message;
                mess.Status = 500;
                return mess;
            }
        }

        public async Task<(CommittedTracking?, Mess?)> CreateCommitedTracking(CommittedTracking model)
        {
            Mess mess = new Mess();
            try
            {
                if (model.Id > 0)
                {
                    var commitedTracking = await _context.CommittedTracking.FirstOrDefaultAsync(p => p.Id == model.Id);
                    if (commitedTracking == null)
                    {
                        mess.Errors = "Không tìm thấy chứng từ";
                        mess.Status = 404;
                        return (null, mess);
                    }
                    foreach (var line in model.CommittedTrackingLine)
                    {
                        if(commitedTracking.CommittedTrackingLine == null)
                        {
                            commitedTracking.CommittedTrackingLine = new List<CommittedTrackingLine>();
                            commitedTracking.CommittedTrackingLine.Add(line);
                        }  
                        else
                        {
                            var lineSub = commitedTracking.CommittedTrackingLine.FirstOrDefault(p => p.Id == line.Id && p.Id != 0);
                            if (lineSub == null)
                            {
                                commitedTracking.CommittedTrackingLine.Add(line);
                            }
                            else
                            {
                                _mapper.Map(lineSub, line);
                            }
                        }
                        
                    }
                    _context.CommittedTracking.Update(commitedTracking);
                    await _context.SaveChangesAsync();
                    //transaction.Commit();
                    return (commitedTracking, null);
                }
                else
                {
                    _context.CommittedTracking.Add(model);
                    await _context.SaveChangesAsync();
                    //transaction.Commit();
                }
                return (model, null);
            }
            catch (Exception ex)
            {
                mess.Errors = ex.Message;
                mess.Status = 900;
                //transaction.Rollback();
                return (null, mess);
            }
        }
    }
}