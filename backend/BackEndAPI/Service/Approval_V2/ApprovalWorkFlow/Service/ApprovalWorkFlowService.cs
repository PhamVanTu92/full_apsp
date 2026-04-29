using System.Security.Claims;
using BackEndAPI.Data;
using BackEndAPI.Dtos;
using BackEndAPI.Models.Approval_V2;
using BackEndAPI.Models.NotificationModels;
using BackEndAPI.Models.Other;
using BackEndAPI.Service.Approval_V2.ApprovalWorkFlow.Engine;
using BackEndAPI.Service.EventAggregator;
using Gridify;
using Microsoft.EntityFrameworkCore;

namespace BackEndAPI.Service.Approval_V2.ApprovalWorkFlow.Service;

public class ApprovalWorkFlowService
    : IApprovalWorkFlowService
{
    private readonly AppDbContext             _context;
    private readonly IApprovalWorkFlowFactory _approvalWorkFlowFactory;
    private readonly IHttpContextAccessor     _httpContextAccessor;
    private readonly IEventAggregator         _eventAggregator;

    public ApprovalWorkFlowService(AppDbContext context, IApprovalWorkFlowFactory approvalWorkFlowFactory,
        IHttpContextAccessor httpContextAccessor, IEventAggregator eventAggregator)
    {
        _context                 = context;
        _approvalWorkFlowFactory = approvalWorkFlowFactory;
        _httpContextAccessor     = httpContextAccessor;
        _eventAggregator         = eventAggregator;
    }


    public async
        Task<(List<Models.Approval_V2.ApprovalWorkFlow>, int)> GetAllAsync(GridifyQuery gridifyQuery, string? search)
    {
        var query = _context.ApprovalWorkFlows.AsNoTracking().ApplyFiltering(gridifyQuery).AsQueryable();

        var count = await query.CountAsync();
        var result = await query
            .Include(x => x.Creator)
            .Include(x => x.ApprovalLevel)
            .Include(x => x.ApprovalSample)
            .Include(x => x.ApprovalWorkFlowLines)
            .Include(x => x.ApprovalWorkFlowDocumentLines)
            .ApplyOrdering(gridifyQuery).ApplyPaging(gridifyQuery).ToListAsync();

        foreach (var res in result)
        {
            var engine =
                _approvalWorkFlowFactory.GetEngine(res.ApprovalWorkFlowDocumentLines.FirstOrDefault()!.DocumentType);
            var item = await engine.GetEntityAsync(res.ApprovalWorkFlowDocumentLines[0].DocId);
            if (item is not null)
            {
                res.ApprovalWorkFlowDocumentLines[0].DocObj = item;
            }
        }


        return (result, count);
    }

    public async Task<(Models.Approval_V2.ApprovalWorkFlow?, Mess?)> GetByIdAsync(int id)
    {
        var result = await _context.ApprovalWorkFlows
            .Include(x => x.Creator)
            .Include(x => x.ApprovalLevel)
            .Include(x => x.ApprovalSample)
            .Include(x => x.ApprovalWorkFlowLines)
            .ThenInclude(x => x.ApprovalLevel)
            .Include(x => x.ApprovalWorkFlowLines)
            .ThenInclude(x => x.ApprovalUser)
            .Include(x => x.ApprovalWorkFlowDocumentLines)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (result is null)
            return (null, new Mess
            {
                Status = 404,
                Errors = "Không tìm thấy quy trình duyệt"
            });

        var engine =
            _approvalWorkFlowFactory.GetEngine(result.ApprovalWorkFlowDocumentLines.FirstOrDefault()!.DocumentType);
        var item = await engine.GetEntityAsync(result.ApprovalWorkFlowDocumentLines[0].DocId);
        result.ApprovalWorkFlowDocumentLines[0].DocObj = item;

        return (result, null);
    }

    public async Task<List<Models.Approval_V2.ApprovalWorkFlow>> CreateAsync(List<IdAndTypeDocDto> documentId,
        int userId,
        List<Models.Approval_V2.ApprovalSample> approvalWorkFlows)
    {
        #region Kiểm tra xem có tồn tại phiếu nào trong luồng duyệt không ? Nếu không có thì mới tạo ra 1 luồng duyệt

        var documentIds = documentId.Select(x => x.Id).ToList();
        var workFlowLines = await _context.ApprovalWorkFlowDocumentLines.Where(x => documentIds.Contains(x.DocId))
            .ToListAsync();
        var workFlowLineIds = workFlowLines.Select(x => x.FatherId).ToList();

        var workFlowExisted = await _context.ApprovalWorkFlows
            .Where(x => workFlowLineIds.Contains(x.Id) && x.ApprovalStatus == ApprovalStatus.Pending).ToListAsync();

        if (workFlowExisted.Count > 0) throw new Exception("Phiếu đang trong luồng duyệt");

        #endregion

        var engine = _approvalWorkFlowFactory.GetEngine(documentId[0].Type);
        var result = await engine.CreateApprovalWorkFlow(documentId, userId, approvalWorkFlows);

        if (result.Count > 0)
        {
            _context.ApprovalWorkFlows.AddRange(result);
            await _context.SaveChangesAsync();
            return result ?? [];
        }


        foreach (var approvalWorkFlow in result)
        {
            var userIds = approvalWorkFlow.ApprovalWorkFlowLines.Select(x => x.ApprovalUserId).ToList();
            _eventAggregator.Publish(new Models.NotificationModels.Notification
            {
                Message = "Phê duyệt yêu cầu!",
                Title =
                    $"Bạn có một yêu cầu {approvalWorkFlow.Id} cần được phê duyệt",
                Type      = "approval_sample",
                SendUsers = userIds,
                Object = new NotificationObject
                {
                    ObjId   = approvalWorkFlow.ApprovalWorkFlowDocumentLines[0].DocId,
                    ObjName = "Approval WorkFlow",
                    ObjType = "approval",
                }
            });
        }

        return result;
    }

    public async Task<(bool, Mess?)> ApprovalAsync(CreateApprovalRequest request)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user is null)
            return (false, new Mess
            {
                Status = 400,
                Errors = "Người dùng không hợp lệ"
            });
        var userId = int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)!.Value);


        var approval = await _context.ApprovalWorkFlows
            .Include(x => x.ApprovalLevel)
            .ThenInclude(x => x!.ApprovalLevelLines)
            .Include(x => x.ApprovalWorkFlowLines)
            .ThenInclude(x => x.ApprovalLevel)
            .Include(x => x.ApprovalWorkFlowDocumentLines)
            .Include(x => x.ApprovalSample)
            .ThenInclude(x => x!.ApprovalSampleProcessesLines)
            .ThenInclude(x => x.ApprovalLevel)
            .ThenInclude(x => x!.ApprovalLevelLines)
            .Include(x => x.ApprovalSample)
            .ThenInclude(x => x!.ApprovalSampleDocumentsLines)
            .Include(x => x.ApprovalSample)
            .ThenInclude(x => x!.ApprovalSampleMembersLines)
            .FirstOrDefaultAsync(x => x.Id == request.ApprovalDecisionId);

        if (approval is null)
            return (false, new Mess
            {
                Status = 400,
                Errors = "Duyệt không hợp lệ"
            });

        var engine =
            _approvalWorkFlowFactory.GetEngine(approval.ApprovalWorkFlowDocumentLines.FirstOrDefault()!.DocumentType);

        var existed = await engine.GetDocStatus(approval.ApprovalWorkFlowDocumentLines[0].DocId);
        if (!string.IsNullOrWhiteSpace(existed) && existed != "DXL")
        {
            return (false, new Mess
            {
                Status = 400,
                Errors = "Phiếu đã được huỷ không thể duyệt"
            });
        }


        var firstApprovalWorkFlowLine = approval.ApprovalWorkFlowLines.FirstOrDefault(x =>
                x.ApprovalUserId == userId && x.Id == request.ApprovalDecisionLineId);


        if (firstApprovalWorkFlowLine is null)
            return (false, new Mess
            {
                Status = 400,
                Errors = "Người duyệt không hợp lệ"
            });

        firstApprovalWorkFlowLine.Status    = request.Status;
        firstApprovalWorkFlowLine.Note      = request.Note;
        firstApprovalWorkFlowLine.UpdatedAt = DateTime.Now;


        approval.ApprovalStatus = CheckAppoved(approval);


        await _context.SaveChangesAsync();


        if (approval.ApprovalStatus == ApprovalStatus.Approved)
            await engine.HandleAfterApproveAsync(approval);
        else if (approval.ApprovalStatus == ApprovalStatus.Rejected)
            await engine.HandleAftherDeclineAsync(approval);

        return (true, null);
    }

    public bool ApprovalLevelIsApproved(Models.Approval_V2.ApprovalWorkFlow approval, int level)
    {
        var levelApprovalLines = approval.ApprovalWorkFlowLines.Where(x => x.SortId == level).ToList();

        if (levelApprovalLines.Count == 0) return false;

        var approvalCount = levelApprovalLines.Count(x => x.Status == ApprovalAction.Approved);
        var declineCount  = levelApprovalLines.Count(x => x.Status == ApprovalAction.Rejected);

        if (declineCount >= approval.DeclineNumber)
        {
            var check = IsApproved(approval);
            if (check == ApprovalAction.Rejected) return false;
        }

        return approvalCount >= approval.ApprovalNumber || declineCount >= approval.DeclineNumber;
    }


    private ApprovalStatus CheckAppoved(Models.Approval_V2.ApprovalWorkFlow approvalWorkFlow)
    {
        var result = TryNextApprovalLevel(approvalWorkFlow);
        if (result) return ApprovalStatus.Pending;
        if (IsApproved(approvalWorkFlow) == ApprovalAction.Approved)
            return ApprovalStatus.Approved;
        else if (IsApproved(approvalWorkFlow) == ApprovalAction.Rejected)
            return ApprovalStatus.Rejected;
        return ApprovalStatus.Cancelled;
    }


    private bool TryNextApprovalLevel(Models.Approval_V2.ApprovalWorkFlow approval)
    {
        var maxLevel = approval.ApprovalSample?.ApprovalSampleProcessesLines.Count;
        var isUpdate = false;
        for (var i = 1; i < maxLevel; i++)
        {
            if (approval.ApprovalWorkFlowLines != null && approval.ApprovalWorkFlowLines.Count > 0 &&
                !ApprovalLevelIsApproved(approval, 1)) return false;

            var approvalLevel = approval.ApprovalSample!.ApprovalSampleProcessesLines[i].ApprovalLevel;

            if (approvalLevel is null) return false;


            approval.ApprovalNumber = approvalLevel.ApprovalNumber;
            approval.DeclineNumber  = approvalLevel.DeclineNumber;
            approval.ApprovalLevel  = approvalLevel;

            var listApproval =
                approval.ApprovalSample?.ApprovalSampleProcessesLines[i].ApprovalLevel?.ApprovalLevelLines;

            if (listApproval is null || listApproval.Count == 0) continue;

            foreach (var approvalLine in listApproval)
            {
                var existingApproval = approval.ApprovalWorkFlowLines!.FirstOrDefault(x =>
                        x.ApprovalLevelId == approvalLine.FatherId && x.ApprovalUserId == approvalLine.ApprovalUserId);

                if (existingApproval == null)
                {
                    var newWdd1 = new ApprovalWorkFlowLine()
                    {
                        ApprovalLevelId = approvalLine.FatherId,
                        ApprovalUserId  = approvalLine.ApprovalUserId,
                        SortId          = i + 1
                    };

                    approval.ApprovalWorkFlowLines!.Add(newWdd1);
                    isUpdate = true;
                }
            }


            #region MyRegion

            var userIds = listApproval.Select(x => x.ApprovalUserId).ToList();
            _eventAggregator.Publish(new Models.NotificationModels.Notification
            {
                Message = "Phê duyệt yêu cầu!",
                Title = $"Bạn có một yêu cầu {approval.ApprovalWorkFlowDocumentLines[0].DocCode} cần được phê duyệt",
                Type = "approval_sample",
                SendUsers = userIds,
                Object = new NotificationObject
                {
                    ObjId   = approval.ApprovalWorkFlowDocumentLines[0].DocId,
                    ObjName = "Approval WorkFlow",
                    ObjType = "approval",
                }
            });

            #endregion
        }

        return isUpdate;
    }


    private ApprovalAction? IsApproved(Models.Approval_V2.ApprovalWorkFlow approvalWorkFlow)
    {
        var maxSort = approvalWorkFlow.ApprovalWorkFlowLines.Max(x => x.SortId);

        var wdd1 = approvalWorkFlow.ApprovalWorkFlowLines.Where(x => x.SortId == maxSort).ToList();

        var approvalNumber = wdd1.Count(x => x.Status == ApprovalAction.Approved);
        var declineNumber  = wdd1.Count(x => x.Status == ApprovalAction.Rejected);

        if (approvalNumber >= approvalWorkFlow.ApprovalNumber) return ApprovalAction.Approved;
        if (declineNumber  >= approvalWorkFlow.DeclineNumber) return ApprovalAction.Rejected;

        return null;
    }

    public async Task<List<Models.Approval_V2.ApprovalSample>> CheckApprovalAsync(int documentId, DocumentEnum docType)
    {
        var engine = _approvalWorkFlowFactory.GetEngine(docType);
        var result = await engine.IsApprovalWorkFlow(documentId);
        return result;
    }
}