using System.Net;
using Microsoft.EntityFrameworkCore;
using System.Security.Authentication;
using BackEndAPI.Data;
using BackEndAPI.Infr.Exceptions;
using BackEndAPI.Models.NotificationModels;
using BackEndAPI.Service.EventAggregator;
using Gridify;

namespace BackEndAPI.Service.DebtReconciliation;

public class DebtReconcilicationService(
    AppDbContext dbContext,
    IWebHostEnvironment webHostEnvironment,
    IEventAggregator eventAggregator,
    IHttpContextAccessor httpContextAccessor) : IDebtReconcilicationService
{
    public async Task<(List<Models.DebtReconciliation>, int)> GetDebtReconciliations(string? search,
        GridifyQuery gridifyQuery, int bpId = 0)
    {
        var query = dbContext.DebtReconciliations.AsQueryable().AsNoTracking().ApplyFiltering(gridifyQuery);
        if (!string.IsNullOrEmpty(search))
        {
            search = search.Trim().ToLower();
            query = query.Where(x => x.Name.Contains(search));
        }

        if (bpId != 0)
        {
            query = query.Where(x => x.CustomerId == bpId);
        }

        var total = await query.CountAsync();
        var debtList = await query
            .Include(x => x.User)
            .Include(x => x.Customer)
            .ApplyOrdering(gridifyQuery).ApplyPaging(gridifyQuery).ToListAsync();

        return (debtList, total);
    }

    public async Task<Models.DebtReconciliation> GetDebtReconciliationById(int id)
    {
        var debtReconciliation = await dbContext.DebtReconciliations
            .Include(x => x.User)
            .Include(x => x.Customer)
            .Include(x => x.Attachments)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (debtReconciliation == null)
        {
            throw new BusinessException("Debt reconciliation not found", HttpStatusCode.NotFound, "DEBT_404");
        }

        var request = httpContextAccessor.HttpContext?.Request;
        debtReconciliation.Attachments.ForEach(x =>
        {
            x.FileUrl = $"{request?.Scheme}://{request?.Host}/{x.FilePath}";
        });

        debtReconciliation.BpAttachments = debtReconciliation.Attachments.Where(x => x.Type == "bp").ToList();
        debtReconciliation.Attachments.RemoveAll(x => x.Type == "bp");

        return debtReconciliation;
    }

    public async Task<Models.DebtReconciliation> CreateDebtReconciliation(Models.DebtReconciliation debtReconciliation)
    {
        var maxCode = dbContext.DebtReconciliations
            .Where(c => c.Code.StartsWith("BBDCCN"))
            .OrderByDescending(c => c.Code)
            .Select(c => c.Code)
            .FirstOrDefault();

        int newNumber = 1;
        if (!string.IsNullOrEmpty(maxCode))
        {
            var numberPart = maxCode.Substring(6);
            if (int.TryParse(numberPart, out var currentNumber))
            {
                newNumber = currentNumber + 1;
            }
        }

        if (string.IsNullOrEmpty(debtReconciliation.Code))
        {
            debtReconciliation.Code = $"BBDCCN{newNumber:00000}";
        }

        var bpUserId = await dbContext.AppUser.Where(x => x.CardId == debtReconciliation.CustomerId).Select(x => x.Id)
            .FirstOrDefaultAsync();
        eventAggregator.Publish(new Models.NotificationModels.Notification
        {
            Message = "Bản đối chiếu công nợ",
            Title = $"Biên bán đối chiếu công nợ {debtReconciliation.Code} mới cần bạn xác nhận",
            Type = "info",
            SendUsers = new List<int>() { bpUserId },
            Object = new NotificationObject()
            {
                ObjId = debtReconciliation.Id,
                ObjName = "DebtReconciliation",
                ObjType = "debt_reconciliation",
            }
        });

        debtReconciliation.SendingDate = DateTime.UtcNow;
        dbContext.DebtReconciliations.Add(debtReconciliation);
        await dbContext.SaveChangesAsync();

        return debtReconciliation;
    }

    public async Task<Models.DebtReconciliation> AddAttachmentToDebtReconciliation(int id, List<IFormFile> files,
        string type = "system")
    {
        var debtReconciliation = await dbContext.DebtReconciliations.Include(x => x.Attachments)
            .FirstOrDefaultAsync(x => x.Id == id);
        var uploadsFolder = Path.Combine(webHostEnvironment.ContentRootPath, "uploads");
        Directory.CreateDirectory(uploadsFolder);

        if (debtReconciliation == null)
        {
            throw new BusinessException("Debt reconciliation not found", HttpStatusCode.NotFound, "DEBT_404");
        }

        foreach (var file in files)
        {
            if (file.Length > 0)
            {
                var guid = Guid.NewGuid().ToString();
                var fileName = guid + Path.GetExtension(file.FileName);
                var filePath = Path.Combine("uploads", fileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                var request = httpContextAccessor.HttpContext?.Request;
                if (request == null) continue;
                var baseUrl = $"{request.Scheme}://{request.Host}";
                var fileUrl = $"{baseUrl}/uploads/{fileName}";

                var newAttDoc = new Models.DebtReconciliationAttachment()
                {
                    FileGuid = guid,
                    FileName = file.FileName,
                    FilePath = filePath,
                    FileUrl = fileUrl,
                    Type = type,
                };
                debtReconciliation.Attachments.Add(newAttDoc);
            }
        }

        await dbContext.SaveChangesAsync();

        return debtReconciliation;
    }

    public async Task<Models.DebtReconciliation> RemoveAttachmentToDebtReconciliation(int id, List<int> attIds)
    {
        var debtReconciliation = await dbContext.DebtReconciliations.Include(x => x.Attachments)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (debtReconciliation == null)
        {
            throw new BusinessException("Debt reconciliation not found", HttpStatusCode.NotFound, "DEBT_404");
        }

        debtReconciliation.Attachments.RemoveAll(x => attIds.Contains(x.Id));

        await dbContext.SaveChangesAsync();

        return debtReconciliation;
    }

    public async Task<Models.DebtReconciliation> ChangeStatusDebtReconiliation(int id, string status,
        string? rejectReason = null)
    {
        var debtReconciliation = await dbContext.DebtReconciliations.Include(x => x.Attachments)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (debtReconciliation == null)
        {
            throw new BusinessException("Debt reconciliation not found", HttpStatusCode.NotFound, "DEBT_404");
        }

        if (debtReconciliation.Status == "C")
        {
            throw new BusinessException("Bạn không thể thay đổi trạng thái khi phiếu đã bị hủy",
                HttpStatusCode.BadRequest, "DEBT_404");
        }


        debtReconciliation.Status = status;
        debtReconciliation.RejectReason = rejectReason;


        var noti = new Models.NotificationModels.Notification
        {
            Type = "info",
            // SendUsers = new List<int>() { bpUserId },
            Object = new NotificationObject()
            {
                ObjId = debtReconciliation.Id,
                ObjName = "DebtReconciliation",
                ObjType = "debt_reconciliation",
            }
        };

        InitMessage(noti, debtReconciliation);

        eventAggregator.Publish(noti);

        await dbContext.SaveChangesAsync();

        return debtReconciliation;
    }

    public async Task<Models.DebtReconciliation> UpdateDebtReconiliation(int id, Models.DebtReconciliation debt)
    {
        var debtReconciliation = await dbContext.DebtReconciliations.Include(x => x.Attachments)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (debtReconciliation == null)
        {
            throw new BusinessException("Debt reconciliation not found", HttpStatusCode.NotFound, "DEBT_404");
        }

        if (debtReconciliation.Status != "R")
        {
            throw new BusinessException("Bạn chỉ có thể thay đổi phiếu khi phiếu bị từ chối", HttpStatusCode.BadRequest,
                "DEBT_404");
        }

        debtReconciliation.Note = debt.Note;
        debtReconciliation.Name = debt.Name;
        debtReconciliation.CustomerId = debt.CustomerId;
        debtReconciliation.Reason = debt.Reason;
        // debtReconciliation.Status = debt.Status;

        await dbContext.SaveChangesAsync();

        return debtReconciliation;
    }

    private void InitMessage(Models.NotificationModels.Notification notification, Models.DebtReconciliation debt)
    {
        switch (debt.Status)
        {
            case "P":
                var bpUserId = dbContext.AppUser.Where(x => x.CardId == debt.CustomerId)
                    .Select(x => x.Id)
                    .FirstOrDefault();
                notification.SendUsers = new List<int>(bpUserId);
                notification.Message = "Bản đối chiếu công nợ";
                notification.Title = $"Biên bán đối chiếu công nợ {debt.Code} mới cần bạn xác nhận";
                break;
            case "R":
                notification.SendUsers = new List<int>(debt.UserId);
                notification.Message = "Bản đối chiếu công nợ";
                notification.Title = $"Biên bán đối chiếu công nợ {debt.Code} bị từ chổi";
                break;
            case "A":
                debt.ConfirmationDate = DateTime.UtcNow;
                notification.SendUsers = new List<int>(debt.UserId);
                notification.Message = "Bản đối chiếu công nợ";
                notification.Title = $"Biên bán đối chiếu công nợ {debt.Code} đã được xác nhận";
                break;
            default:
                return;
        }
    }
}