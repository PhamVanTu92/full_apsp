using BackEndAPI.Data;
using BackEndAPI.Models.Approval;
using BackEndAPI.Models.Other;
using BackEndAPI.Service.EventAggregator;
using Gridify;
using Microsoft.EntityFrameworkCore;

namespace BackEndAPI.Service.Approval;

public class Approval : Service<IApproval>
{
    private readonly AppDbContext         _context;
    private readonly IServiceScopeFactory _serviceProvider;
    private readonly IEventAggregator     _eventAggregator;
    private readonly LoggingSystemService _systemLog;

    public Approval(AppDbContext context, IServiceScopeFactory serviceProvider, IEventAggregator eventAggregator,
        LoggingSystemService systemLog) :
        base(context)
    {
        _context = context;
        // No publishers exist for Models.Approval.Approval — the previous
        // Subscribe call here was dead code. CreateApprovalAsync is now invoked
        // directly from ActionApproval.
        _eventAggregator = eventAggregator;
        _serviceProvider = serviceProvider;
        _systemLog       = systemLog;
    }

    public async Task<(Models.Approval.Approval?, Mess?)> ActionApproval(int docId)
    {
        var mess = new Mess();
        var doc  = await _context.ODOC.Include(e => e.Approval).FirstOrDefaultAsync(x => x.Id == docId);
        if (doc == null)
        {
            mess.Errors = "Document not found";
            return (null, mess);
        }

        if (doc.Approvalx?.Status != "R" && doc.Approvalx is not null)
        {
            mess.Errors = "Tài liệu này vẫn đang trong quy trình phê duyệt";
            return (null, mess);
        }

        int userId = 0;
        var user   = await _context.Users.FirstOrDefaultAsync(p => p.Id == doc.UserId && p.UserType == "NPP");
        if (user != null)
            userId = _context.BP.FirstOrDefault(p => p.Id == doc.CardId).SaleId ?? 0;
        else
            userId = doc.UserId ?? 0;
        var app = new Models.Approval.Approval
        {
            ActorId   = userId,
            DocId     = docId,
            TransType = doc.ObjType,
            CurStep   = 1,
            Document  = doc,
        };
        doc.Status = "DXL";
        await CreateApprovalAsync(app);

        //if (app.Id == 0)
        //{
        //    doc.Status = "DXN";
        //}

        await _systemLog.SaveWithTransAsync("INFO", "EnableApproval",
            $"Kích hoạt phê duyệt cho chứng từ {doc.InvoiceCode}", "Document", doc.Id);
        await _context.SaveChangesAsync();


        return (app, null);
    }

    private async Task CreateApprovalAsync(Models.Approval.Approval approval)
    {
        using var scope   = _serviceProvider.CreateScope();
        var       context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await using var trans = await context.Database.BeginTransactionAsync();
        try
        {
            #region Check Credit

            // var appTemplate = context.OWTM
            //     .Include(p => p.WTM1)
            //     .Include(p => p.WTM3)
            //     .Include(p => p.WTM4)
            //     .ToList();
            //
            // var pp = context
            //     .CRD3
            //     .FirstOrDefault(p => approval.Document != null && p.BPId == approval.Document.CardId && p.PaymentMethodCode == "");
            //
            // if (pp?.BalanceLimit >= approval.Document?.TotalPayment) return;

            #endregion


            var apts = context.WTM2
                .Include(a => a.OWST)
                .ThenInclude(b => b!.WST1)
                .Include(d => d.OWTM)
                .ThenInclude(d => d.RUsers)
                .Where(a => a.OWTM.Active)
                .Where(c => c.Sort == 1)
                .ToList();

            var apt = apts.FirstOrDefault(e =>
                e.OWTM.RUsers.Any(p => p.Id == approval.ActorId.Value) && e.OWTM.RUsers.Count != 0);

            if (approval.ActorId == null || apt is null)
            {
                approval.IsApp = false;
                return;
            }

            approval.WtmId      = apt?.OWTM?.Id         ?? 0;
            approval.MaxReqr    = apt?.OWST?.MaxReqr    ?? 0;
            approval.MaxRejReqr = apt?.OWST?.MaxRejReqr ?? 0;

            var line = new List<ApprovalLine>();
            if (apt is { OWST.WST1: not null })
                line.AddRange
                (
                    apt.OWST.WST1.Select(item => new ApprovalLine
                        { Status = "P", StepCode = 1, UserId = item.UserId, WstId = item.FatherId }
                    ));

            approval.Lines = line;

            var doc = approval.Document;

            approval.Document = null;
            context.Approval.Add(approval);
            await context.SaveChangesAsync();
            await trans.CommitAsync();


            if (apt?.OWST?.WST1 != null)
            {
                string message = "", title = "";

                if (doc?.ObjType == 22)
                {
                    title   = "Phê duyệt đơn hàng";
                    message = "Bạn có một Đơn hàng {0} cần duyệt";
                }
                else if (doc?.ObjType == 1250000001)
                {
                    title   = "Phê duyệt yêu cầu lấy hàng";
                    message = "Bạn có một Yêu cầu lấy hàng {0} cần duyệt";
                }

                var userIds = apt?.OWST.WST1.Select(p => p.UserId).ToList();
                _eventAggregator.Publish(new Models.NotificationModels.Notification
                {
                    Message = string.Format(message, doc?.InvoiceCode),
                    Title   = title,
                    Type    = "info",
                    Object = new Models.NotificationModels.NotificationObject
                    {
                        ObjId   = approval.Id,
                        ObjName = "Approval",
                        ObjType = "approval",
                    },
                    SendUsers = userIds!,
                });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            await trans.RollbackAsync();
        }
    }

    public async Task<Mess?> Approve(int docId, int userId, string status, string note)
    {
        var mess = new Mess();
        if (status != "A" && status != "R")
        {
            mess.Errors = "Status not found";
            mess.Status = 400;
            return mess;
        }

        await using var trans = await _context.Database.BeginTransactionAsync();
        try
        {
            var approval = await _context.Approval
                .Include(p => p.Lines)
                .Include(approval => approval.Document)
                .Include(p => p.Document)
                .OrderByDescending(p => p.Id)
                .Where(p => p.DocId == docId)
                // .Where(p => p.Status == "P")
                .FirstOrDefaultAsync();
            if (approval == null)
            {
                mess.Errors = "No approval line was found";
                mess.Status = 403;
                await trans.RollbackAsync();
                return mess;
            }

            var line = approval.Lines.LastOrDefault(p => p.UserId == userId && p.Status == "P");
            if (line == null)
            {
                mess.Errors = "No approval line was found";
                mess.Status = 403;
                await trans.RollbackAsync();
                return mess;
            }

            line.Status     = status;
            line.ApprovalAt = DateTime.Now;
            line.Note       = note;

            await _context.SaveChangesAsync();

            var countApproved   = approval.Lines.Count(p => p.Status == "A" && p.StepCode == approval.CurStep);
            var countDeApproved = approval.Lines.Count(p => p.Status == "R" && p.StepCode == approval.CurStep);

            int curentStep = approval.CurStep;

            string message = "Khong xac dinh", title = "Khong xac dinh";

            if (approval.Document?.ObjType == 22)
            {
                title   = "Phê duyệt đơn hàng";
                message = "Bạn có một Đơn hàng {0} cần duyệt";
            }
            else if (approval.Document?.ObjType == 1250000001)
            {
                title   = "Phê duyệt yêu cầu lấy hàng";
                message = "Bạn có một Yêu cầu lấy hàng {0} cần duyệt";
            }
            else if (approval.Document?.ObjType == 50)
            {
                title   = "Phê duyệt Yêu cầu cấp mẫu thử nghiệm";
                message = "Bạn có một Yêu cầu cấp mẫu thử nghiệm {0} cần duyệt";
            }

            if (countDeApproved >= approval.MaxRejReqr)
            {
                if (approval.Document != null)
                {
                    approval.Document.Status = "HUY";
                    approval.Status          = "R";
                }
            }
            else if (countApproved >= approval.MaxReqr)
            {
                NextStep(approval);
                if (approval.CurStep != curentStep)
                {
                    _eventAggregator.Publish(new Models.NotificationModels.Notification
                    {
                        Message = string.Format(message, approval.Document?.InvoiceCode),
                        Title   = title,
                        Type    = "info",
                        Object = new Models.NotificationModels.NotificationObject
                        {
                            ObjId   = approval.Id,
                            ObjName = "Approval",
                            ObjType = "approval",
                        },
                        SendUsers = approval.Lines.Where(p => p.StepCode == approval.CurStep).Select(p => p.UserId)
                            .ToList(),
                    });
                }
            }

            if (approval.Status is "A" or "R")
            {
                var tile = approval.Status == "A" ? "Đơn hàng của bạn đã được duyệt" : "Đơn hàng của bạn đã bị hủy";
                _eventAggregator.Publish(new Models.NotificationModels.Notification
                {
                    Message = tile,
                    Title   = tile,
                    Type    = "info",
                    Object = new Models.NotificationModels.NotificationObject
                    {
                        ObjId   = approval.Id,
                        ObjName = "Order",
                        ObjType = "order",
                    },
                    SendUsers = new List<int>((int)approval?.Document?.UserId!)
                });
                if (approval.Status == "A")
                    await _systemLog.SaveWithTransAsync("INFO", "Noti", $"Duyệt đơn hàng", "Approval", approval.Id);
                if (approval.Status == "R")
                    await _systemLog.SaveWithTransAsync("INFO", "Noti", $"Từ chối đơn hàng", "Approval", approval.Id);
            }


            await _context.SaveChangesAsync();
            await trans.CommitAsync();
            return null;
        }
        catch (Exception e)
        {
            mess.Errors = e.Message;
            mess.Status = 500;
            await trans.RollbackAsync();
            return mess;
        }
    }

    private void NextStep(Models.Approval.Approval approve)
    {
        var apt = _context.WTM2
            .Include(a => a.OWST)
            .ThenInclude(b => b!.WST1)
            .Include(d => d.OWTM)
            .FirstOrDefault(c => c.Sort == approve.CurStep + 1 && c.FatherId == approve.WtmId);
        if (apt == null)
        {
            approve.Status = "A";
            if (approve.Document != null)
                if (approve.Document.ObjType == 50)
                    approve.Document.Status = "DXN";
            return;
        }

        if (apt.OWST?.WST1 == null) return;
        foreach (var w in apt.OWST?.WST1!)
        {
            approve.Lines.Add(new ApprovalLine
            {
                StepCode = apt.Sort,
                UserId   = w.UserId,
                WddId    = approve.Id,
                WstId    = w.FatherId,
            });
        }

        approve.CurStep    = approve.CurStep + 1;
        approve.MaxReqr    = apt.OWST?.MaxReqr    ?? 0;
        approve.MaxRejReqr = apt.OWST?.MaxRejReqr ?? 0;
    }

    public async Task<(ICollection<Models.Approval.Approval>?, Mess?, int)> GetApprovals(int skip,
        int limit, string? status, string? search, GridifyQuery q)
    {
        var mess = new Mess();
        try
        {
            var query = _context.Approval.AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                string[] statusList = status.Split(',');
                query = query.Where(p => statusList.Contains(p.Status));
            }

            if (!string.IsNullOrEmpty(search))
            {
                query = query
                    .Where(w => w.Document != null && w.Document.InvoiceCode != null &&
                        w.Document.InvoiceCode.Contains(search));
            }

            var total = await query.ApplyFiltering(q).CountAsync();


            var approvalList = await query
                .Include(p => p.Lines)
                .ThenInclude(p => p.User)
                .Include(p => p.Lines)
                .ThenInclude(p => p.Owst)
                .Include(p => p.Document)
                .Include(p => p.Actor)
                .Include(p => p.Owtm)
                .ThenInclude(p => p!.WTM2)!
                .ThenInclude(p => p.OWST)
                .ApplyFiltering(q)
                .OrderByDescending(a => a.Id)
                .Skip(skip * limit)
                .Take(limit)
                .ToListAsync();

            return (approvalList, null, total);
        }
        catch (Exception ex)
        {
            mess.Errors = ex.Message;
            mess.Status = 500;

            return (null, mess, 0);
        }
    }

    public async Task<(Models.Approval.Approval?, Mess?)> GetApprovalById(int id)
    {
        var mess = new Mess();
        try
        {
            var appr = await _context.Approval
                .Include(p => p.Lines)
                .ThenInclude(p => p.User)
                .Include(p => p.Lines)
                .ThenInclude(p => p.Owst)
                .Include(p => p.Lines)
                .Include(p => p.Document)
                .Include(p => p.Actor)
                .Include(p => p.Owtm)
                .ThenInclude(p => p!.WTM2)!
                .ThenInclude(p => p.OWST)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (appr != null) return (appr, null);
            mess.Errors = "No approval line was found";
            mess.Status = 404;
            return (null, mess);
        }
        catch (Exception ex)
        {
            mess.Errors = ex.Message;
            mess.Status = 500;
            return (null, mess);
        }
    }

    // private string GetTitle(int objType, out string message)
    // {
    //     switch (objType)
    //     {
    //         
    //     }
    //     message = "";
    //     return "";
    // }
}