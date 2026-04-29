using System.Globalization;
using System.Security.Claims;
using System.Security.Cryptography;
using BackEndAPI.Data;
using BackEndAPI.Models;
using BackEndAPI.Models.Approval;
using BackEndAPI.Models.Banks;
using BackEndAPI.Models.Document;
using BackEndAPI.Models.Other;
using BackEndAPI.Models.Promotion;
using BackEndAPI.Service.EventAggregator;
using BackEndAPI.Service.Zalo;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace BackEndAPI.Service.Document;

public partial class DocumentService
{
    public async Task UpdateDocAddress(int docId, List<DOC12> address)
    {
        if (address.Count is 0 or > 2)
        {
            throw new BadHttpRequestException("Invalid address length");
        }

        var doc = await _context.ODOC.Include(e => e.Address).FirstOrDefaultAsync(e => e.Id == docId);
        if (doc is null) throw new Exception("not found");

        if (doc.Status == "HUY" || doc.Status == "HUY2" || doc.Status == "DGH" || doc.Status == "DHT")
        {
            throw new Exception("Không thể cập địa chỉ đơn hàng");
        }

        if (doc.Address is null) return;
        doc.Address.RemoveAll(e => e.FatherId == docId);
        doc.Address.AddRange(address);

        await _systemLog.SaveAsync("INFO", "Update", $"Cập nhật địa chỉ đơn hàng {doc.InvoiceCode}", "Order", doc.Id);
        await _context.SaveChangesAsync();
    }

    public async Task RemoveDraft(int docId)
    {
        var model = await _context.ODOC.Where(e => e.Id == docId).FirstOrDefaultAsync();
        if (model == null)
        {
            throw new KeyNotFoundException();
        }

        if (model.Status != "NHAP")
        {
            throw new KeyNotFoundException("Đây không phải là bản nháp");
        }

        model.Status = "DXL";

        string notiType = "";
        string notiTitle = "";
        var message = "";
        var codes = "";

        notiType = "approval";
        message = "Yêu cầu cấp mẫu thử nghiệm cần phê duyệt";
        notiTitle = "Yêu cầu cấp mẫu thử nghiệm {0}";
        var apts = _context.WTM2
            .Include(a => a.OWST)
            .ThenInclude(b => b!.WST1)
            .Include(d => d.OWTM)
            .ThenInclude(d => d.RUsers)
            .Include(d => d.OWTM)
            .ThenInclude(d => d.WTM3)
            .Where(c => c.Sort == 1 && c.OWTM.WTM3.Select(e => e.TransType).ToList().Contains(50)).ToList();
        List<Models.Approval.Approval> approval = new List<Models.Approval.Approval>();
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            foreach (var apt in apts)
            {
                var l = apt.OWTM.RUsers.Select(e => e.Id).ToList();
                if (l.Contains((int)model.UserId))
                {
                    var line = new List<ApprovalLine>();
                    if (apt is { OWST.WST1: not null })
                        line.AddRange
                        (
                            apt.OWST.WST1.Select(item => new ApprovalLine
                                { Status = "P", StepCode = 1, UserId = item.UserId, WstId = item.FatherId }
                            ));
                    Models.Approval.Approval app1 = new Models.Approval.Approval
                    {
                        ActorId = model.UserId,
                        DocId = model.Id,
                        TransType = model.ObjType,
                        CurStep = 1,
                        WtmId = apt.OWTM.Id,
                        MaxReqr = apt.OWST.MaxReqr,
                        MaxRejReqr = apt.OWST.MaxRejReqr,
                        Lines = line,
                    };

                    approval.Add(app1);
                }
            }

            if (approval.Count > 0)
            {
                _context.Approval.AddRange(approval);
                await _context.SaveChangesAsync();
                foreach (var item in approval)
                {
                    var sendUsers = item.Lines.Select(p => p.UserId).ToList();
                    _eventAggregator.Publish(new Models.NotificationModels.Notification
                    {
                        Message = string.Format(message, model.InvoiceCode),
                        Title = string.Format(notiTitle, model.InvoiceCode),
                        Type = "info",
                        Object = new Models.NotificationModels.NotificationObject
                        {
                            ObjId = item.Id,
                            ObjName = "-",
                            ObjType = notiType,
                        },
                        SendUsers = sendUsers,
                    });
                }
            }
            else
            {
                model.Status = "DXN";
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
        }
    }

    public async Task LogProvicy(ODOC doc)
    {
        try
        {
            var po = await _context.Article.AsNoTracking().FirstOrDefaultAsync(e => e.Status == "A");
            if (po == null) return;
            var fileUrl = po.FilePath;

            if (fileUrl == null) return;

            var uri = new Uri(fileUrl);
            var relativePath = uri.AbsolutePath; // e.g. "/uploads/xxx.pdf"
            var wwwrootPath = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
            var localPath = Path.Combine(wwwrootPath, relativePath.TrimStart('/'));

            Directory.CreateDirectory(Path.GetDirectoryName(localPath)!);

            await using var fs = new FileStream(localPath, FileMode.Create, FileAccess.ReadWrite);

            fs.Position = 0; // Reset vị trí để băm
            using var sha256 = SHA256.Create();
            byte[] hash = await sha256.ComputeHashAsync(fs);

            var hashString = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();

            var context = _httpContextAccessor.HttpContext;
            var ip = context?.Request.Headers["X-Forwarded-For"].FirstOrDefault() ??
                     context?.Connection.RemoteIpAddress?.ToString();

            var userAgent = context?.Request.Headers["User-Agent"].ToString();

            var newPoLog = new PolicyOrderLog
            {
                CardCode = doc.CardCode ?? "",
                CardName = doc.CardName ?? "",
                OrderCode = doc.InvoiceCode ?? "",
                //PolicyHash = hashString,
                PolicyVersion = po.Id.ToString(),
                AgreeAt = doc.DocDate ?? DateTime.Now,
                PolicyId = po.Id,
                IpAddress = ip ?? "",
                OrderId = doc.Id,
                Device = userAgent ?? ""
            };

            _context.PolicyOrderLogs.Add(newPoLog);
            await _context.SaveChangesAsync();
        }
        catch
        {
            return;
        }
    }

    public async Task ConfirmPayment(int id)
    {
        var doc = await _context.ODOC
            .Include(e => e.Payments)
            .Include(odoc => odoc.PaymentInfo)
            .FirstOrDefaultAsync(e => e.Id == id);
        if (doc == null) throw new KeyNotFoundException();
        _eventAggregator.Publish(new Models.NotificationModels.Notification
        {
            Title = $"Đơn hàng {doc.InvoiceCode} đã được thanh toán",
            Message =
                $"Đơn hàng {doc.InvoiceCode} đã được khách hàng thanh toán số tiền {doc.PaymentInfo?.TotalPayNow} qua cổng OnePay. Vui lòng kiểm tra và xác nhận đơn hàng",
            Type = "info",
            Object = new Models.NotificationModels.NotificationObject
            {
                ObjId = doc.Id,
                ObjName = "-",
                ObjType = "order",
            },
            // SendUsers = wUser.Select(u => u.Id).ToList(),
            SendUsers = [doc.BP?.SaleId ?? 0]
        });
        doc.Status = "CXN";
        doc.Payments.Where(e => e.PaymentMethodCode == "PayNow").FirstOrDefault().Status = "A";
        await _systemLog.SaveAsync("INFO", "ConfirmPayment", $"Thanh toán đơn hàng {doc.InvoiceCode}", "Order", doc.Id);
        await _context.SaveChangesAsync();
    }

    public async Task SendPaymentRequest(int id)
    {
        var doc = await _context.ODOC
            .Include(e => e.Payments)
            .Include(odoc => odoc.PaymentInfo)
            .FirstOrDefaultAsync(e => e.Id == id);
        if (doc == null) throw new KeyNotFoundException();

        if (doc.Payments.Where(e => e.PaymentCode == "PayNow").Any(e => e.Status == "A"))
        {
            throw new Exception("Không có hóa đơn cần thanh toán cho đơn hàng này");
        }

        var payments = doc.Payments.FirstOrDefault(e => e.PaymentCode == "PayNow");
        if (payments is not null)
        {
            payments.Status = "A";
        }

        var xUser = await _context.AppUser
            .Where(u => u.CardId == doc.CardId).Select(u => u.Id).FirstOrDefaultAsync();
        _eventAggregator.Publish(new Models.NotificationModels.Notification
        {
            Title = $"Thanh toán đơn hàng {doc.InvoiceCode}",
            Message =
                $"Đơn hàng {doc.InvoiceCode} đã được xác nhận, cần bạn thanh toán số tiền {doc.PaymentInfo.TotalPayNow} VND của thanh toán ngay",
            Type = "info",
            Object = new Models.NotificationModels.NotificationObject
            {
                ObjId = doc.Id,
                ObjName = "-",
                ObjType = "order",
            },
            // SendUsers = wUser.Select(u => u.Id).ToList(),
            SendUsers = [xUser]
        });
        doc.Status = "CTT";

        await _context.SaveChangesAsync();
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


    public async Task<Mess?> UpdateStatus(int id, string status, List<IFormFile> files, int ObjType, string reason = "")
    {
        var mess = new Mess();
        using (var transaction = _context.Database.BeginTransaction())
        {
            try
            {
                var doc = await _context.ODOC
                    .Include(x => x.ItemDetail).ThenInclude(doc1 => doc1.Item)
                    .Include(x => x.BP)
                    .Include(x => x.Promotion)
                    .Include(x => x.Payments)
                    .Include(p => p.AttachFile).Include(odoc => odoc.PaymentInfo).Include(odoc => odoc.AttDocuments)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (doc == null)
                {
                    mess.Errors = "not found";
                    mess.Status = 404;
                    return mess;
                }
                string cust = "";
                var CRD5 = _context.BP.Include(e => e.CRD5.Where(e => e.Default == "Y")).Where(e=>e.Id == doc.CardId).Select(e=>e.CRD5).FirstOrDefault();

                if (CRD5?.FirstOrDefault()?.Person ==  "" || CRD5?.FirstOrDefault()?.Person == null)
                    cust = doc.CardName;
                else
                    cust = CRD5?.FirstOrDefault()?.Person;
                ZNSOrderConfirm confirm = new ZNSOrderConfirm();
                confirm.phone = "84"+ CRD5?.FirstOrDefault()?.Phone.Substring(1);
                confirm.tracking_id = doc.InvoiceCode;
                confirm.template_id = "509811";
                confirm.template_data = new detail1
                {
                    amount = (int)Math.Round(doc.Total ?? 0, MidpointRounding.AwayFromZero),
                    orderId = doc.InvoiceCode,
                    store = doc.CardName,
                    customer = cust,
                    date1 = DateTime.UtcNow.ToString("dd/MM/yyyy"),
                    date2 = DateTime.UtcNow.AddDays(3).ToString("dd/MM/yyyy"),
                };

                ZNSOrderCompleted completed = new ZNSOrderCompleted();
                completed.phone = "84" + CRD5?.FirstOrDefault()?.Phone.Substring(1);
                completed.tracking_id = doc.InvoiceCode;
                completed.template_id = "509815";
                completed.template_data = new detail2
                {
                    orderId = doc.InvoiceCode,
                    amount = (int)Math.Round(doc.Total ?? 0, MidpointRounding.AwayFromZero),
                    store = doc.CardName,
                    customer = cust,
                    date1 = DateTime.UtcNow.ToString("dd/MM/yyyy")
                };
                if (doc.ObjType == 22)
                {
                    switch (status)
                    {
                        case "DXN":
                            doc.ConfirmAt = DateTime.UtcNow;
                            var wUser = await _context.AppUser
                                .Include(u => u.UserRoles)
                                .ThenInclude(ur => ur.Role)
                                .Where(u => u.UserRoles.Any(ur => ur.Role.Name == "Nhân viên CSKH"))
                                .ToListAsync();
                            // Business rule: KHÔNG cộng điểm vào cycle ở DXN.
                            // Chỉ cộng khi đơn hoàn thành (DHT). CustomerPoint pending đã được
                            // tạo từ DocumentService.CalculatePoints khi đặt đơn.
                            await _committedService.UpdateVolumn(doc.CardId ?? 0, doc.ItemDetail ?? new List<DOC1>());
                            var saleFos = await _context.SaleForecasts
                                .Include(s => s.SaleForecastItems)
                                .ThenInclude(s => s.Periods)
                                .Where(s => s.Status == "A" && s.StartDate <= DateTime.Now &&
                                            s.EndDate >= DateTime.Now && s.CustomerId == doc.CardId)
                                .ToListAsync();

                            DateTime today = DateTime.Now;
                            CultureInfo culture = CultureInfo.CurrentCulture;

                            int currentWeek =
                                culture.Calendar.GetWeekOfYear(today, CalendarWeekRule.FirstDay, DayOfWeek.Monday);
                            int currentMonth = DateTime.Now.Month;

                            foreach (var saleFo in saleFos)
                            {
                                string currentPeriod = "";
                                if (saleFo.PeriodType == "M")
                                {
                                    currentPeriod = $"Tháng {currentMonth}";
                                }
                                else
                                {
                                    currentPeriod = $"Tuần {currentWeek}";
                                }

                                saleFo.SaleForecastItems.ForEach(s =>
                                {
                                    var found = doc.ItemDetail.FirstOrDefault(p => p.ItemId == s.ItemId);
                                    if (found is null) return;

                                    s.Periods.ForEach(p =>
                                    {
                                        if (p.PeriodName != currentPeriod) return;
                                        var packing = _context.Packing
                                            .AsNoTracking().FirstOrDefault(p => p.Id == found.Item.PackingId);

                                        if (found.Quantity != null)
                                            p.ActualQuantity += (int)found.Quantity * (int)packing.Volumn.Value;
                                    });
                                });
                            }


                            _eventAggregator.Publish(new Models.NotificationModels.Notification
                            {
                                Message = $"Đơn hàng {doc.InvoiceCode} đã được xác nhận",
                                Title = $"Đơn hàng {doc.InvoiceCode} đã được xác nhận, cần bạn lên kế hoạch giao hàng",
                                Type = "info",
                                Object = new Models.NotificationModels.NotificationObject
                                {
                                    ObjId = doc.Id,
                                    ObjName = "-",
                                    ObjType = "order",
                                },
                                SendUsers = wUser.Select(u => u.Id).ToList(),
                            });

                            await _systemLog.SaveAsync("INFO", "ChangeStatus", $"Xác nhận đơn hàng {doc.InvoiceCode}","Order", doc.Id);

                            break;
                        case "DGH":
                            var sUser = await _context.AppUser
                                .Include(u => u.UserRoles)
                                .ThenInclude(ur => ur.Role)
                                .Where(u => u.UserRoles.Any(ur => ur.Role.Name == "Nhân viên CSKH"))
                                .ToListAsync();
                            var cUser = await _context.AppUser
                                .Where(u => u.CardId == doc.CardId).Select(u => u.Id).FirstOrDefaultAsync();
                            var sendUser = sUser.Select(u => u.Id).ToList();
                            sendUser.Add(cUser);

                            _eventAggregator.Publish(new Models.NotificationModels.Notification
                            {
                                Title = $"Đơn hàng {doc.InvoiceCode} đang được giao",
                                Message = "Đơn hàng đang được giao đến bạn",
                                Type = "info",
                                Object = new Models.NotificationModels.NotificationObject
                                {
                                    ObjId = doc.Id,
                                    ObjName = "-",
                                    ObjType = "order",
                                },
                                SendUsers = sendUser,
                            });
                            await _systemLog.SaveAsync("INFO", "ChangeStatus",
                                $"Đơn hàng {doc.InvoiceCode} đang được giao", "Order", doc.Id);
                            break;
                        case "HUY2":
                            var cUser2 = await _context.AppUser
                                .Where(u => u.CardId == doc.CardId).Select(u => u.Id).FirstOrDefaultAsync();
                            if (doc.DocType.IsNullOrEmpty() && doc.CardId.HasValue)
                            {
                                await _pointService.OnDocumentStatusChangedAsync(
                                    doc.Id, doc.CardId.Value, doc.ObjType ?? 22, "HUY2");
                            }
                            _eventAggregator.Publish(new Models.NotificationModels.Notification
                            {
                                Title = $"Đơn hàng {doc.InvoiceCode} đã bị hủy",
                                Message = "Đơn hàng đã bị hủy do sai thông tin",
                                Type = "info",
                                Object = new Models.NotificationModels.NotificationObject
                                {
                                    ObjId = doc.Id,
                                    ObjName = "-",
                                    ObjType = "order",
                                },
                                SendUsers = [cUser2],
                            });
                            doc.ReasonForCancellation = reason;
                            await _systemLog.SaveAsync("INFO", "ChangeStatus", $"Hủy đơn hàng {doc.InvoiceCode}",
                                "Order", doc.Id);
                            var crd4 = _context.CRD4.FirstOrDefault(e => e.InvoiceNumber == doc.InvoiceCode);
                            if(crd4 != null)
                                _context.CRD4.Remove(crd4);
                            break;
                        case "UNC":
                            if (doc.Payments.Any(e => e.PaymentMethodCode == "PayNow" && e.Status == "A"))
                            {
                                throw new Exception("Đơn hàng đã được thanh toán");
                            }

                            var maxCode = _context.Payment
                                .Where(c => c.PaymentCode.StartsWith("HD"))
                                .OrderByDescending(c => c.PaymentCode)
                                .Select(c => c.PaymentCode)
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

                            var payment = new Payment
                            {
                                PaymentDate = DateTime.UtcNow,
                                TotalAmount = doc.Total,
                                PaymentAmount = doc.PaymentInfo?.TotalPayNow ?? 0,
                                PartnerId = doc.CardId,
                                PartnerName = doc.CardName,
                                PartnerContactNo = doc.CardCode,
                                PaymentMethodCode = "PayNow",
                                PaymentMethodName = "Bank",
                                Status = "A",
                                CreatedDate = DateTime.UtcNow,
                                DocId = doc.Id,
                                DocCode = doc.InvoiceCode,
                                DocType = doc.ObjType,
                                Crd4 = null,
                            };
                            payment.PaymentCode = $"HD{newNumber:00000}";
                            doc.Payments.Add(payment);


                            _eventAggregator.Publish(new Models.NotificationModels.Notification
                            {
                                Title = $"Đơn hàng {doc.InvoiceCode} đã được thanh toán",
                                Message =
                                    $"Đơn hàng {doc.InvoiceCode} đã được khách hàng thanh toán số tiền {doc.PaymentInfo?.TotalPayNow} bằng hình thức uỷ nhiệm chi/ chuyển khoản ngân hàng. Vui lòng kiểm tra và xác nhận đơn hàng",
                                Type = "info",
                                Object = new Models.NotificationModels.NotificationObject
                                {
                                    ObjId = doc.Id,
                                    ObjName = "-",
                                    ObjType = "order",
                                },
                                // SendUsers = wUser.Select(u => u.Id).ToList(),
                                SendUsers = [doc.BP?.SaleId ?? 0]
                            });
                            status = "CXN";
                            break;
                        case "DHT":
                            var cUser1 = await _context.AppUser
                                .Where(u => u.CardId == doc.CardId).Select(u => u.Id).FirstOrDefaultAsync();
                            // Business rule: cộng điểm thật vào cycle khi đơn hoàn thành.
                            if (doc.DocType.IsNullOrEmpty() && doc.CardId.HasValue)
                            {
                                await _pointService.OnDocumentStatusChangedAsync(
                                    doc.Id, doc.CardId.Value, doc.ObjType ?? 22, "DHT");
                            }
                            _eventAggregator.Publish(new Models.NotificationModels.Notification
                            {
                                Title = $"Đơn hàng {doc.InvoiceCode} đã hoàn thành",
                                Message = "Đơn hàng đã giao đến bạn thành công, cảm ơn bạn đã tin tưởng chúng tôi",
                                Type = "info",
                                Object = new Models.NotificationModels.NotificationObject
                                {
                                    ObjId = doc.Id,
                                    ObjName = "-",
                                    ObjType = "order",
                                },
                                SendUsers = new List<int> { cUser1 },
                            });
                            await _systemLog.SaveAsync("INFO", "ChangeStatus", $"Hoàn thành đơn hàng {doc.InvoiceCode}",
                                "Order", doc.Id);
                            break;
                    }
                }

                if (doc.ObjType == 1250000001)
                {
                    switch (status)
                    {
                        case "DXN":
                            var wUser = await _context.AppUser
                                .Include(u => u.UserRoles)
                                .ThenInclude(ur => ur.Role)
                                .Where(u => u.UserRoles.Any(ur => ur.Role.Name == "Nhân viên CSKH"))
                                .ToListAsync();

                            _eventAggregator.Publish(new Models.NotificationModels.Notification
                            {
                                Message = $"Yêu cầu lấy hàng {doc.InvoiceCode} đã được xác nhận",
                                Title =
                                    $"Yêu cầu lấy hàng {doc.InvoiceCode} đã được xác nhận, cần bạn lên kế hoạch giao hàng",
                                Type = "info",
                                Object = new Models.NotificationModels.NotificationObject
                                {
                                    ObjId = doc.Id,
                                    ObjName = "-",
                                    ObjType = "order_request",
                                },
                                SendUsers = wUser.Select(u => u.Id).ToList(),
                            });
                            await _systemLog.SaveAsync("INFO", "ChangeStatus", $"Yêu cầu lấy hàng {doc.InvoiceCode} đẫ được xác nhận",
                              "OrderRequest", doc.Id);

                            break;
                        case "DGH":
                            var sUser = await _context.AppUser
                                .Include(u => u.UserRoles)
                                .ThenInclude(ur => ur.Role)
                                .Where(u => u.UserRoles.Any(ur => ur.Role.Name == "Nhân viên CSKH"))
                                .ToListAsync();
                            var cUser = await _context.AppUser
                                .Where(u => u.CardId == doc.CardId).Select(u => u.Id).FirstOrDefaultAsync();
                            var sendUser = sUser.Select(u => u.Id).ToList();
                            sendUser.Add(cUser);

                            _eventAggregator.Publish(new Models.NotificationModels.Notification
                            {
                                Title = $"Yêu cầu lấy hàng {doc.InvoiceCode} đang được giao",
                                Message = "Yêu cầu lấy hàng đang được giao đến bạn",
                                Type = "info",
                                Object = new Models.NotificationModels.NotificationObject
                                {
                                    ObjId = doc.Id,
                                    ObjName = "-",
                                    ObjType = "order_request",
                                },
                                SendUsers = sendUser,
                            });
                            await _systemLog.SaveAsync("INFO", "ChangeStatus", $"Yêu cầu lấy hàng {doc.InvoiceCode} đang được giao",
                               "OrderRequest", doc.Id);
                            break;
                        case "DHT":
                            var cUser1 = await _context.AppUser
                                .Where(u => u.CardId == doc.CardId).Select(u => u.Id).FirstOrDefaultAsync();
                            _eventAggregator.Publish(new Models.NotificationModels.Notification
                            {
                                Title = $"Yêu cầu lấy hàng {doc.InvoiceCode} đã hoàn thành",
                                Message =
                                    "Yêu cầu lấy hàng đã giao đến bạn thành công, cảm ơn bạn đã tin tưởng chúng tôi",
                                Type = "info",
                                Object = new Models.NotificationModels.NotificationObject
                                {
                                    ObjId = doc.Id,
                                    ObjName = "-",
                                    ObjType = "order_request",
                                },
                                SendUsers = new List<int> { cUser1 },
                            });
                            await _systemLog.SaveAsync("INFO", "ChangeStatus", $"Hoàn thành yêu cầu lấy hàng {doc.InvoiceCode}",
                                "OrderRequest", doc.Id);
                            break;
                        case "HUY":
                            var cUser2 = await _context.AppUser
                                .Where(u => u.CardId == doc.CardId).Select(u => u.Id).FirstOrDefaultAsync();
                            if (doc.DocType.IsNullOrEmpty() && doc.CardId.HasValue)
                            {
                                await _pointService.OnDocumentStatusChangedAsync(
                                    doc.Id, doc.CardId.Value, doc.ObjType ?? 22, "HUY");
                            }
                            _eventAggregator.Publish(new Models.NotificationModels.Notification
                            {
                                Title = $"Yêu cầu lấy hàng {doc.InvoiceCode} đã bị hủy",
                                Message = "Yêu cầu lấy hàng đã bị hủy do sai thông tin",
                                Type = "info",
                                Object = new Models.NotificationModels.NotificationObject
                                {
                                    ObjId = doc.Id,
                                    ObjName = "-",
                                    ObjType = "order_request",
                                },
                                SendUsers = [cUser2],
                            });
                            doc.ReasonForCancellation = reason;
                            await _systemLog.SaveAsync("INFO", "ChangeStatus", $"Hủy yêu cầu lấy hàng {doc.InvoiceCode}",
                                "OrderRequest", doc.Id);
                            break;
                    }
                }

                if (doc.ObjType == 12)
                {
                    switch (status)
                    {
                        case "DXN":
                            doc.ConfirmAt = DateTime.UtcNow;
                            var wUser = await _context.AppUser
                                .Include(u => u.UserRoles)
                                .ThenInclude(ur => ur.Role)
                                .Where(u => u.UserRoles.Any(ur => ur.Role.Name == "Nhân viên CSKH"))
                                .ToListAsync();
                            DateTime today = DateTime.Now;
                            CultureInfo culture = CultureInfo.CurrentCulture;

                            int currentWeek =
                                culture.Calendar.GetWeekOfYear(today, CalendarWeekRule.FirstDay, DayOfWeek.Monday);
                            int currentMonth = DateTime.Now.Month;

                            _eventAggregator.Publish(new Models.NotificationModels.Notification
                            {
                                Message = $"Yêu cầu đổi vật phẩm khuyến mại {doc.InvoiceCode} đã được xác nhận",
                                Title = $"Yêu cầu đổi vật phẩm khuyến mại {doc.InvoiceCode} đã được xác nhận, cần bạn lên kế hoạch giao hàng",
                                Type = "info",
                                Object = new Models.NotificationModels.NotificationObject
                                {
                                    ObjId = doc.Id,
                                    ObjName = "-",
                                    ObjType = "exchange",
                                },
                                SendUsers = wUser.Select(u => u.Id).ToList(),
                            });

                            await _systemLog.SaveAsync("INFO", "ChangeStatus", $"Xác nhận Yêu cầu đổi vật phẩm khuyến mại {doc.InvoiceCode}", "exchange", doc.Id);

                            break;
                        case "DGH":
                            var sUser = await _context.AppUser
                                .Include(u => u.UserRoles)
                                .ThenInclude(ur => ur.Role)
                                .Where(u => u.UserRoles.Any(ur => ur.Role.Name == "Nhân viên CSKH"))
                                .ToListAsync();
                            var cUser = await _context.AppUser
                                .Where(u => u.CardId == doc.CardId).Select(u => u.Id).FirstOrDefaultAsync();
                            var sendUser = sUser.Select(u => u.Id).ToList();
                            sendUser.Add(cUser);

                            _eventAggregator.Publish(new Models.NotificationModels.Notification
                            {
                                Title = $"Yêu cầu đổi vật phẩm khuyến mại {doc.InvoiceCode} đang được giao",
                                Message = "Yêu cầu đổi vật phẩm khuyến mại đang được giao đến bạn",
                                Type = "info",
                                Object = new Models.NotificationModels.NotificationObject
                                {
                                    ObjId = doc.Id,
                                    ObjName = "-",
                                    ObjType = "exchange",
                                },
                                SendUsers = sendUser,
                            });
                            await _systemLog.SaveAsync("INFO", "ChangeStatus",
                                $"Yêu cầu đổi vật phẩm khuyến mại {doc.InvoiceCode} đang được giao", "exchange", doc.Id);
                            break;
                        case "HUY2":
                            var cUser2 = await _context.AppUser
                                .Where(u => u.CardId == doc.CardId).Select(u => u.Id).FirstOrDefaultAsync();
                            CalculatorPoint calPoint = new CalculatorPoint();
                            calPoint.CardId = doc.CardId ?? 0;
                            calPoint.DocId = doc.Id;
                            calPoint.CalculatorPointLine = doc.ItemDetail
                            .Select(e => new CalculatorPointLine
                            {
                                ItemId = e.ItemId ?? 0,
                                Quantity = e.Quantity,
                                Point = e.Price
                            })
                            .ToList();
                            await _pointService.RedeemPoints(calPoint, "C");
                            _eventAggregator.Publish(new Models.NotificationModels.Notification
                            {
                                Title = $"Yêu cầu đổi vật phẩm khuyến mại {doc.InvoiceCode} đã bị hủy",
                                Message = "Yêu cầu đổi vật phẩm khuyến mại đã bị hủy do sai thông tin",
                                Type = "info",
                                Object = new Models.NotificationModels.NotificationObject
                                {
                                    ObjId = doc.Id,
                                    ObjName = "-",
                                    ObjType = "exchange",
                                },
                                SendUsers = [cUser2],
                            });
                            doc.ReasonForCancellation = reason;
                            await _systemLog.SaveAsync("INFO", "ChangeStatus", $"Hủy yêu cầu đổi vật phẩm khuyến mại {doc.InvoiceCode}",
                                "exchange", doc.Id);
                            break;
                        case "DHT":
                            var cUser1 = await _context.AppUser
                                .Where(u => u.CardId == doc.CardId).Select(u => u.Id).FirstOrDefaultAsync();
                            _eventAggregator.Publish(new Models.NotificationModels.Notification
                            {
                                Title = $"Yêu cầu đổi vật phẩm khuyến mại {doc.InvoiceCode} đã hoàn thành",
                                Message = "Yêu cầu đổi vật phẩm khuyến mại đã giao đến bạn thành công, cảm ơn bạn đã tin tưởng chúng tôi",
                                Type = "info",
                                Object = new Models.NotificationModels.NotificationObject
                                {
                                    ObjId = doc.Id,
                                    ObjName = "-",
                                    ObjType = "exchange",
                                },
                                SendUsers = new List<int> { cUser1 },
                            });
                            await _systemLog.SaveAsync("INFO", "ChangeStatus", $"Hoàn thành đơn hàng {doc.InvoiceCode}",
                                "exchange", doc.Id);
                            break;
                    }
                }
                switch (status)
                {
                    case "DXN": await _zService.SendOrderConfirm(confirm); break;
                    case "DHT": await _zService.SendOrderCompleted(completed); break;
                }    
               var uploadFiles = new List<string>();
                var uploadsFolder = Path.Combine(_webHostEnvironment.ContentRootPath, "uploads");
                Directory.CreateDirectory(uploadsFolder);

                foreach (var file in files)
                {
                    if (file.Length > 0)
                    {
                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                        var filePath = Path.Combine(uploadsFolder, fileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(fileStream);
                        }

                        uploadFiles.Add(filePath);
                        var user = _httpContextAccessor.HttpContext?.User;
                        var userId = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                        HttpRequest? request = _httpContextAccessor.HttpContext?.Request;
                        if (request != null)
                        {
                            var baseUrl = $"{request.Scheme}://{request.Host}";
                            var fileUrl = $"{baseUrl}/uploads/{fileName}";

                            var newAttDoc = new AttDocument()
                            {
                                FatherId = doc.Id,
                                FileName = file.FileName,
                                AuthorId = int.Parse(userId),
                                FilePath = fileUrl,
                            };
                            if (doc.AttDocuments == null)
                                doc.AttDocuments = new List<AttDocument>();
                            doc.AttDocuments.Add(newAttDoc);
                        }
                    }
                }

                doc.Status = status;
                _context.ODOC.Update(doc);
                await _context.SaveChangesAsync();
                transaction.Commit();
                ;
                return null;
            }
            catch (Exception ex)
            {
                mess.Errors = ex.Message;
                mess.Status = 900;
                transaction.Rollback();
                ;
                return mess;
            }
        }
    }
}