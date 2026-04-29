using BackEndAPI.Data;
using BackEndAPI.Models.BusinessPartners;
using BackEndAPI.Models.ConfirmationSystem;
using BackEndAPI.Models.Document;
using BackEndAPI.Models.Other;
using BackEndAPI.Service.EventAggregator;
using Gridify;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackEndAPI.Service.ConfirmationSystems
{
    public class ConfirmationDocumentService(AppDbContext dbContext, IWebHostEnvironment webHostEnvironment,
    IHttpContextAccessor httpContextAccessor) :IConfirmationDocumentService
    {
        public async Task<(bool, Mess)> Approve(ActionRequest request, int userId)
        {
            Mess mess = new Mess();
            try
            {
                var usr = dbContext.AppUser.FirstOrDefault(xx => xx.Id == userId)?.FullName;
                var doc = await dbContext.ConfirmationDocuments.FindAsync(request.Id);
                if (doc == null)
                {
                    mess.Status = 400;
                    mess.Errors = "Không tìm thấy chứng từ";
                    return (false, mess);
                }

                doc.SentDate = DateTime.UtcNow;

                dbContext.ConfirmationLogs.Add(new ConfirmationLog
                {
                    DocumentId = request.Id,
                    Action = "Approve",
                    ActionBy = usr,
                    ActionDate = DateTime.UtcNow
                });

                await dbContext.SaveChangesAsync();
                return (true, null);
            }
            catch (Exception ex)
            {
                mess.Status = 400;
                mess.Errors = ex.Message;
                return (false, mess);
            }
        }

        public async Task<(ConfirmationDocument, Mess)> Create(ConfirmationDocumentNew doc, int? userId)
        {
            Mess mess = new Mess(); ;
            try
            {
                if (doc.File == null || doc.File.Length == 0)
                {
                    mess.Status = 400;
                    mess.Errors = "Không có file upload.";
                    return (null, mess);
                }


                var uploadsFolder = Path.Combine(webHostEnvironment.ContentRootPath, "uploads");
                Directory.CreateDirectory(uploadsFolder);
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(doc.File.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await doc.File.CopyToAsync(fileStream);
                }
                var usr =  dbContext.AppUser.FirstOrDefault(xx => xx.Id == userId)?.FullName;
                var document = new ConfirmationDocument
                {
                    CardId = doc.CardId,
                    CardCode = doc.CardCode,
                    CardName = doc.CardName,
                    FileName = doc.File.FileName,
                    FileUrl = "/uploads/" + fileName,
                    Status = DocumentStatus.Pending,
                    CreatedBy = usr,
                    CreatedDate = DateTime.UtcNow
                };
                dbContext.ConfirmationDocuments.Add(document);
                await dbContext.SaveChangesAsync();
                return (document, null);

            }
            catch (Exception ex)
            {
                mess.Status = 400;
                mess.Errors = ex.Message;
                return (null, mess);
            }
        }

        public async Task<(ConfirmationDocument, Mess)> GetDetail(int id)
        {
            Mess mess = new Mess(); ;
            try
            {
                var document =  dbContext.ConfirmationDocuments.Include(d => d.Logs)
               .FirstOrDefault(d => d.Id == id);

                return (document, null);

            }
            catch (Exception ex)
            {
                mess.Status = 400;
                mess.Errors = ex.Message;
                return (null, mess);
            }
        }

        public async Task<(List<ConfirmationDocument>, int, Mess)> GetList(GridifyQuery gridifyQuery, int cardId = 0)
        {
            Mess mess = new Mess();
            try
            {
                var CardId = dbContext.AppUser.FirstOrDefault(e => e.Id == cardId).CardId ?? 0;
                var total = dbContext.ConfirmationDocuments.Include(d => d.Logs).ApplyFiltering(gridifyQuery)
               .Where(d => d.CardId == CardId || CardId == 0).Count();
                var document = dbContext.ConfirmationDocuments.Include(d => d.Logs).ApplyFiltering(gridifyQuery)
                    .ApplyPaging(gridifyQuery).ApplyOrdering(gridifyQuery)
               .Where(d => d.CardId == CardId || CardId == 0).ToList();

                return (document, total, null);

            }
            catch (Exception ex)
            {
                mess.Status = 400;
                mess.Errors = ex.Message;
                return (null,0, mess);
            }
        }

        public async Task<(bool, Mess)> Reject(ActionRequest request, int userId)
        {
            Mess mess = new Mess();
            try
            {
                var usr = dbContext.AppUser.FirstOrDefault(xx => xx.Id == userId)?.FullName;
                var doc = await dbContext.ConfirmationDocuments.FindAsync(request.Id);
                if (doc == null)
                {
                    mess.Status = 400;
                    mess.Errors = "Không tìm thấy chứng từ";
                    return (false, mess);
                }

                doc.SentDate = DateTime.UtcNow;

                dbContext.ConfirmationLogs.Add(new ConfirmationLog
                {
                    DocumentId = request.Id,
                    Action = "Reject",
                    ActionBy = usr,
                    ActionDate = DateTime.UtcNow
                });

                await dbContext.SaveChangesAsync();
                return (true, null);
            }
            catch (Exception ex)
            {
                mess.Status = 400;
                mess.Errors = ex.Message;
                return (false, mess);
            }
        }

        public async Task<(bool, Mess)> Send(ActionRequest request, int userId)
        {
            Mess mess = new Mess();
            try
            {
                var usr = dbContext.AppUser.FirstOrDefault(xx => xx.Id == userId)?.FullName;
                var doc = await dbContext.ConfirmationDocuments.FindAsync(request.Id);
                if (doc == null)
                {
                    mess.Status = 400;
                    mess.Errors = "Không tìm thấy chứng từ";
                    return (false, mess);
                }    

                doc.SentDate = DateTime.UtcNow;

                dbContext.ConfirmationLogs.Add(new ConfirmationLog
                {
                    DocumentId = request.Id,
                    Action = "Send",
                    ActionBy = usr,
                    ActionDate = DateTime.UtcNow
                });

                await dbContext.SaveChangesAsync();
                return (true, null);
            }
            catch (Exception ex)
            {
                mess.Status = 400;
                mess.Errors = ex.Message;
                return (false, mess);
            }
        }
    }
}
