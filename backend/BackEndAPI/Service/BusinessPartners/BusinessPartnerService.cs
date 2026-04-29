using AutoMapper;
using B1SLayer;
using BackEndAPI.Controllers;
using BackEndAPI.Data;
using BackEndAPI.Models.Account;
using BackEndAPI.Models.BPGroups;
using BackEndAPI.Models.BusinessPartners;
using BackEndAPI.Models.Document;
using BackEndAPI.Models.ItemGroups;
using BackEndAPI.Models.ItemMasterData;
using BackEndAPI.Models.Other;
using BackEndAPI.Models.SAP;
using BackEndAPI.Service.Document;
using BackEndAPI.Service.Mail;
using BackEndAPI.Service.Promotions;
using Function.EncryptDecrypt;
using Gridify;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage.Json;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using NHibernate.Hql.Ast;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.WebSockets;

namespace BackEndAPI.Service.BusinessPartners
{
    public class CRD4CardCode
    {
        public string? CardCode { get; set; }
        public double CurrentAccountBalance { get; set; }
    }

    public class CRD4InfoUpdate
    {
        public int DocEntry { get; set; }
        public string? U_MDHPT { get; set; }
        public DateTime DocDate { get; set; }
        public DateTime DocDueDate { get; set; }
        public double DocTotal { get; set; }
        public double PaidToDate { get; set; }
        public string U_HTTT1 { get; set; }
    }

    public class KV
    {
        public string U_KV { get; set; }
    }

    public class KVRespond
    {
        public List<KV> Value { get; set; }
    }

    public class CRD4InfoUpdateResponse
    {
        [JsonProperty("value")] public List<CRD4InfoUpdate> Value { get; set; }
    }

    public class BusinessPartnerService : Service<BP>, IBusinessPartnerService
    {
        Endpoints                             _endpoints;
        IMailService                          _mail_Service;
        private readonly AppDbContext         _context;
        private readonly IWebHostEnvironment  _webHostEnvironment;
        UserManager<AppUser>                  _userManager;
        private readonly IConfiguration       _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IModelUpdater        _modelUpdater;
        private readonly IMapper              _mapper;
        private readonly SLConnection         _slConnection;
        private readonly LoggingSystemService _systemLog;
        private readonly IPointSetupService   _pointService;
        private readonly Address.IBPAddressService _addressService;

        public BusinessPartnerService(AppDbContext context, UserManager<AppUser> userManager,
            IConfiguration configuration,
            IModelUpdater modelUpdater, IMailService mailService, SLConnection slConnection
            , IWebHostEnvironment webHostEnvironment, IHttpContextAccessor httpContextAccessor,
            IPointSetupService pointService,
            IMapper mapper, IOptions<Endpoints> options, LoggingSystemService systemLog,
            Address.IBPAddressService addressService) : base(context)
        {
            _context             = context;
            _configuration       = configuration;
            _slConnection        = slConnection;
            _webHostEnvironment  = webHostEnvironment;
            _httpContextAccessor = httpContextAccessor;
            _modelUpdater        = modelUpdater;
            _userManager         = userManager;
            _mail_Service        = mailService;
            _mapper              = mapper;
            _endpoints           = options.Value;
            _pointService        = pointService;
            _systemLog           = systemLog;
            _addressService      = addressService;
        }

        public async Task<(BP?, Mess?)> CreateBPAsync(BusinessPartnerView model, string cardType)
        {
            string? json   = model.item;
            var     format = "dd/MM/yyyy";
            Mess    mess   = new Mess();
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var dateTimeConverter = new IsoDateTimeConverter { DateTimeFormat = format };
                    if (json is null)
                    {
                        mess.Status = 400;
                        mess.Errors = "JSON data null";

                        return (null, mess);
                    }

                    BP? bp = JsonConvert.DeserializeObject<BP>(json, dateTimeConverter);
                    if (bp is null)
                    {
                        mess.Status = 400;
                        mess.Errors = "JSON data null";

                        return (null, mess);
                    }

                    if (AppDbContext.IsUniqueAsync<BP>(_context, u => u.Email == bp.Email))
                    {
                        mess.Status = 400;
                        mess.Errors = "Email đã tồn tại";

                        return (null, mess);
                    }

                    if (AppDbContext.IsUniqueAsync<BP>(_context, u => u.CardCode == bp.CardCode))
                    {
                        mess.Status = 400;
                        mess.Errors = "Mã khách hàng đã tồn tại";

                        return (null, mess);
                    }

                    if (AppDbContext.IsUniqueAsync<BP>(_context, u => u.Phone == bp.Phone))
                    {
                        mess.Status = 400;
                        mess.Errors = "Số điện thoại đã tồn tại";

                        return (null, mess);
                    }

                    if (AppDbContext.IsUniqueAsync<BP>(_context, u => u.LicTradNum == bp.LicTradNum))
                    {
                        mess.Status = 400;
                        mess.Errors = "Mã số thuế đã tồn tại";

                        return (null, mess);
                    }

                    var validResults      = new List<ValidationResult>();
                    var validationContext = new ValidationContext(bp);
                    bp.CardType = cardType;
                    if (model.images != null)
                    {
                        var uploadsFolder = Path.Combine(_webHostEnvironment.ContentRootPath, "uploads");
                        Directory.CreateDirectory(uploadsFolder);
                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.images.FileName);
                        var filePath = Path.Combine(uploadsFolder, fileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await model.images.CopyToAsync(fileStream);
                        }

                        HttpRequest? request = _httpContextAccessor.HttpContext?.Request;
                        if (request != null)
                        {
                            var baseUrl  = $"{request.Scheme}://{request.Host}";
                            var imageUrl = $"{baseUrl}/uploads/{fileName}";
                            bp.Avatar = imageUrl;
                        }
                    }

                    _context.BP.Add(bp);
                    await _context.SaveChangesAsync();
                    var defaultUser = new AppUser
                    {
                        CardId   = bp.Id,
                        UserName = bp.Email!.Substring(0, bp.Email.IndexOf("@")),
                        FullName = bp.Email!.Substring(0, bp.Email.IndexOf("@")),
                        Email    = bp.Email,
                        UserType = "NPP"
                    };

                    string userPassword = bp.Email.Substring(0, bp.Email.IndexOf("@")) + "@A123456";
                    var    user         = await _userManager.FindByNameAsync(defaultUser.UserName);
                    if (user == null)
                    {
                        var checkUser = await _userManager.CreateAsync(defaultUser, userPassword);
                        var token     = await _userManager.GenerateEmailConfirmationTokenAsync(defaultUser);
                        var request   = _httpContextAccessor.HttpContext!.Request;
                        var link =
                            $"{request.Scheme}://{request.Host}?token{request.QueryString}={token}&userid{request.QueryString}={defaultUser.Id}";
                        //  .Action("ConfirmEmail", "Account",
                        //new { userId = defaultUser.Id, token = token }, "https");
                        //try
                        //{
                        //    MailData mail = new MailData();
                        //    mail.EmailToId = bp.Email;
                        //    mail.EmailSubject = "Register DRM Sai Gon Petro Portal";
                        //    mail.EmailToName = bp.Email.Substring(0, bp.Email.IndexOf("@"));
                        //    mail.Link = "link";
                        //    _mail_Service.SendMail(mail);
                        //}
                        //catch (Exception ex)
                        //{
                        //    mess.Status = 400;
                        //    mess.Errors = "Email không chính xác, hoặc không tồn tại";
                        //    transaction.Rollback();
                        //    return (null, mess);
                        //}
                    }


                    await _context.SaveChangesAsync();
                    await _systemLog.SaveWithTransAsync("INFO", "Create", $"Tạo mới khách hàng {bp.CardName}",
                                                        "Customer", bp.Id);

                    transaction.Commit();

                    return (bp, null);
                }
                catch (Exception ex)
                {
                    mess.Status = 400;
                    mess.Errors = ex.Message;
                    transaction.Rollback();
                    return (null, mess);
                }
            }
        }

        public async Task<(bool, Mess)> DeleteBPAsync(int id)
        {
            Mess mess = new Mess();
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var ocrd = await _context.BP
                        .AsNoTracking()
                        .FirstOrDefaultAsync(p => p.Id == id);
                    if (ocrd == null)
                    {
                        mess.Status = 400;
                        mess.Errors = "Dữ liệu trống";
                        return (false, mess);
                    }

                    var doc = await _context.ODOC
                        .AsNoTracking()
                        .FirstOrDefaultAsync(p => p.CardId == id);
                    if (doc != null)
                    {
                        mess.Status = 400;
                        mess.Errors = "Đối tác kinh doanh đã được sử dụng";
                        return (false, mess);
                    }

                    _context.BP.Remove(ocrd);
                    await _context.SaveChangesAsync();
                    transaction.Commit();
                    return (true, mess);
                }
                catch (Exception ex)
                {
                    mess.Status = 900;
                    mess.Errors = ex.Message;
                    transaction.Rollback();
                    return (false, mess);
                }
            }
        }


        public async Task<List<int>> GetAllStaffUnderManager(int managerUserId)
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

        public async Task<(IEnumerable<BP>?, Mess?, int total)> GetAllBPAsync(int skip, int limit, string search,
            GridifyQuery q, int? userId)
        {
            Mess mess = new Mess();
            try
            {
                var query = _context.Set<BP>().AsNoTracking().AsQueryable().ApplyFiltering(q);
                if (!string.IsNullOrWhiteSpace(search))
                {
                    query = query.Where(p => p.CardCode != null && (p.CardCode.Contains(search) ||
                                            p.CardName.Contains(search) || p.Email.Contains(search)));
                }

                if (userId is not null)
                {
                    var usr = await _context.AppUser.AsNoTracking()
                        .Include(xx => xx.DirectStaff)
                        .Where(xx => xx.Id == userId)
                        .Include(xx => xx.Role)
                        .ThenInclude(xx => xx.RoleFillCustomerGroups)
                        .FirstOrDefaultAsync();
                    if (usr.Role != null)
                    {
                        if (!usr.Role.Name.ToUpper().Equals("QUẢN TRỊ VIÊN"))
                        {
                            if (usr.Role.IsFillCustomerGroup)
                            {
                                query = query.Where(e => e.Groups.Any(c =>
                                                                          usr.Role.RoleFillCustomerGroups
                                                                              .Select(d => d.CustomerGroupId).ToArray()
                                                                              .Contains(c.Id)));
                            }

                            if (usr.Role.IsSaleRole)
                            {
                                var usrIds1 = await GetAllStaffUnderManager(userId ?? 0);
                                //  query = query
                                // .Where(x => x.BP.SaleId != null && (x.BP.SaleId == usr.Id
                                //                                     || usr.DirectStaff.Select(d => d.Id)
                                //                                         .Contains(x.BP.SaleId ?? -1)));

                                // var paths = usr.DirectStaffPath.Split("/");
                                query = query.Where(e => usrIds1.ToArray().Contains(e.SaleId.Value));
                            }
                        }
                    }
                }

                var totalCount = await query.CountAsync();
                var bp = await query.AsNoTracking().AsSplitQuery()
                    .Include(p => p.Commiteds)
                    .ThenInclude(p => p.CommittedLine)
                    .ThenInclude(p => p.CommittedLineSub)
                    .ThenInclude(p => p.Brand)
                    .Include(p => p.Commiteds)
                    .ThenInclude(p => p.CommittedLine)
                    .ThenInclude(p => p.CommittedLineSub)
                    .ThenInclude(p => p.Industry)
                    .Include(p => p.CRD1)
                    .Include(p => p.CRD2)
                    .Include(p => p.CRD3)
                    .Include(p => p.CRD6)
                    .ThenInclude(p => p.Author)
                    .Include(p => p.CRD4)!
                    .ThenInclude(p => p.Payments)
                    .OrderByDescending(p => p.Id)
                    .Include(p => p.CRD5)
                    .Include(b => b.UserInfo)
                    .Include(b => b.Groups)
                    .Include(b => b.SaleStaff)
                    .Skip(skip * limit)
                    .Take(limit)
                    .ToListAsync();
                var customerPoint = (from c in _context.CustomerPointCycles
                        join ps in _context.PointSetups
                            on c.PoitnSetupId equals ps.Id
                        where bp.Select(e => e.Id).Contains(c.CustomerId)
                        select new CustomerPointCycleDTO
                        {
                            Id           = c.Id,
                            CustomerId   = c.CustomerId,
                            PoitnSetupId = c.PoitnSetupId,
                            StartDate    = ps.FromDate,
                            EndDate      = ps.ToDate,
                            ExpiryDate   = ps.ExtendedToDate ?? ps.ToDate,

                            EarnedPoint = _context.CustomerPointLine
                                .Where(pl => pl.CustomerId    == c.CustomerId
                                           && pl.PoitnSetupId == c.PoitnSetupId
                                           && pl.DocType      == 22
                                           && pl.DocDate      >= ps.FromDate
                                           && pl.DocDate      <= ps.ToDate
                                           && pl.DocDate      <= (ps.ExtendedToDate ?? ps.ToDate))
                                .Sum(pl => (double?)pl.PointChange) ?? 0,

                            RedeemedPoint = _context.CustomerPointLine
                                .Where(pl => pl.CustomerId    == c.CustomerId
                                           && pl.PoitnSetupId == c.PoitnSetupId
                                           && pl.DocType      == 12
                                           && pl.DocDate      >= ps.FromDate
                                           && pl.DocDate      <= ps.ToDate
                                           && pl.DocDate      <= (ps.ExtendedToDate ?? ps.ToDate))
                                .Sum(pl => (double?)pl.PointChange) ?? 0,
                            RemainingPoint = _context.CustomerPointLine
                                .Where(pl => pl.CustomerId    == c.CustomerId
                                           && pl.PoitnSetupId == c.PoitnSetupId
                                           && pl.DocDate      >= ps.FromDate
                                           && pl.DocDate      <= ps.ToDate
                                           && pl.DocDate      <= (ps.ExtendedToDate ?? ps.ToDate))
                                .Sum(pl => (double?)pl.PointChange) ?? 0,
                            Name = ps.Name
                        })
                    .ToList();
                foreach (var b in bp)
                {
                    b.CustomerPoints = customerPoint.Where(p => p.CustomerId == b.Id).ToList();
                    b.CurrentCommited = b.Commiteds?
                        .FirstOrDefault(p => p.CommittedYear!.Value.Year == DateTime.Now.Year);
                    if (b.CurrentCommited == null) continue;

                    var doc = _context.ODOC.Include(p => p.ItemDetail).ThenInclude(p => p.Item)
                        .ThenInclude(p => p.Packing)
                        .Where(p => p.CardId == b.Id && p.DocDate.Value.Year == DateTime.Now.Year).ToList()
                        .SelectMany(p => p.ItemDetail).ToList();

                    try
                    {
                        b.CurrentCommited?.CommittedLine?.ForEach
                        (root =>
                            {
                                root.CommittedLineSub?.ForEach(xx =>
                                {
                                    var totalz = doc.Where(b =>
                                                               b.Item?.IndustryId == xx.IndustryId && xx!.Brand!
                                                                   .Select(m => m.Id).ToList()
                                                                   .Contains(b.Item!.BrandId!.Value)).ToList();
                                    xx.CurrentVolumn =
                                        totalz.Sum(x => (x.Item?.Packing?.Volumn.Value * x.Quantity ?? 0));
                                });
                            }
                        );
                    }
                    catch
                    {
                    }
                }

                return (bp, null, totalCount);
            }
            catch (Exception ex)
            {
                mess.Errors = ex.Message;
                mess.Status = 900;
                return (null, mess, 0);
            }
        }

        public async Task<(IEnumerable<BPGroup>?, Mess?, int total)> GetAllBPAsync(int skip, int limit, string search,
            GridifyQuery q)
        {
            Mess mess = new Mess();
            try
            {
                var query = _context.Set<BP>().AsNoTracking().AsQueryable().ApplyFiltering(q);
                if (!string.IsNullOrWhiteSpace(search))
                {
                    query = query.Where(p => p.CardCode != null && (p.CardCode.Contains(search) ||
                                            p.CardName.Contains(search) || p.Email.Contains(search)));
                }


                var totalCount = await query.CountAsync();
                var bp = await query.AsNoTracking().Select(x => new BPGroup
                    {
                        ID       = x.Id,
                        CardCode = x.CardCode,
                        CardName = x.CardName,
                        Email    = x.Email,
                        Phone    = x.Phone
                    })
                    .Skip(skip * limit)
                    .Take(limit)
                    .ToListAsync();
                return (bp, null, totalCount);
            }
            catch (Exception ex)
            {
                mess.Errors = ex.Message;
                mess.Status = 900;
                return (null, mess, 0);
            }
        }

        public async Task<(BP?, Mess?)> ChangeSaleStaff(int bpId, int staffId)
        {
            var             mess        = new Mess();
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var bp = await _context.BP.Include(b => b.SaleStaff).FirstOrDefaultAsync(b => b.Id == bpId);
                if (bp == null)
                {
                    mess.Errors = "BP with id not found";
                    mess.Status = 404;

                    return (null, mess);
                }

                var u = await _context.Users.Where(u => u.Id == staffId).FirstOrDefaultAsync();
                if (u == null)
                {
                    mess.Errors = "Staff with id not found";
                    mess.Status = 404;

                    return (null, mess);
                }

                if (u.Status == "I")
                {
                    mess.Errors = "Staff is inctive";
                    mess.Status = 400;
                    return (null, mess);
                }

                bp.SaleStaff = u;


                await _context.SaveChangesAsync();
                await _systemLog.SaveWithTransAsync("INFO", "Update", $"Cập nhật thông tin khách hàng {bp.CardName}",
                                                    "Customer", bp.Id);

                await transaction.CommitAsync();


                return (bp, null);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                mess.Errors = ex.Message;
                mess.Status = 500;

                return (null, mess);
            }
        }


        public async Task<(IEnumerable<BP>, Mess, int total)> GetAllBPAsync(int skip, int limit)
        {
            Mess mess = new Mess();
            try
            {
                var query      = _context.Set<BP>().AsQueryable();
                var totalCount = await query.Where(p => p.CardType.Equals("C")).CountAsync();
                var bp = await query
                    .Where(p => p.CardType.Equals("C"))
                    .Skip(skip * limit)
                    .Take(limit)
                    .Include(p => p.CRD1)
                    .Include(p => p.CRD2)
                    .Include(p => p.CRD3)
                    .Include(p => p.CRD4)
                    .ThenInclude(p => p.Payments)
                    .Include(p => p.CRD5)
                    .ToListAsync();
                return (bp, null, totalCount);
            }
            catch (Exception ex)
            {
                mess.Errors = ex.Message;
                mess.Status = 900;
                return (null, mess, 0);
            }
        }

        public async Task<(IEnumerable<BP>, Mess)> GetBPAsync(string search, string cardType)
        {
            Mess mess = new Mess();
            try
            {
                var bp = await _context.BP
                    .Where(p => p.CardCode.Contains(search) ||
                               p.CardName.Contains(search)  ||
                               p.Phone.Contains(search)     ||
                               p.LicTradNum.Contains(search)
                    )
                    .Where(p => p.CardType.Equals(cardType))
                    .Include(p => p.CRD1)
                    .Include(p => p.CRD2)
                    .Include(p => p.CRD3)
                    .Include(p => p.CRD4)
                    .ThenInclude(p => p.Payments)
                    .Include(p => p.CRD5)
                    .ToListAsync();
                return (bp, null);
            }
            catch (Exception ex)
            {
                mess.Errors = ex.Message;
                mess.Status = 900;
                return (null, mess);
            }
        }

        public async Task<List<CRD3>> GetCRD3Async(int id)
        {
            var bp = await _context.BP.Include(b => b.CRD3)
                .Include(bp => bp.CRD4.Where(e => e.AmountOverdue.HasValue && e.AmountOverdue.Value != 0))
                .FirstOrDefaultAsync(b => b.Id == id);
            if (bp == null)
            {
                throw new Exception("Không tìm thấy khách hàng");
            }

            if (bp.CRD3 == null || bp.CRD3.Count == 0)
            {
                bp.CRD3 = new List<CRD3>();
                bp.CRD3.Add(new CRD3()
                {
                    PaymentMethodID   = 2,
                    PaymentMethodCode = "PayCredit",
                    PaymentMethodName = "Công nợ - Tín chấp"
                });
                bp.CRD3.Add(new CRD3()
                {
                    PaymentMethodID   = 3,
                    PaymentMethodCode = "PayGuarantee",
                    PaymentMethodName = "Công nợ - Bảo lãnh"
                });
            }

            if (bp.CRD4 != null)
            {
                bp.CRD3.FirstOrDefault(x => x.PaymentMethodCode == "PayGuarantee")
                    .BalanceLimit = bp.CRD4.Where(x => x.PaymentMethodCode == "PayGuarantee")
                    .Sum(x => x.AmountOverdue ?? 0);
                bp.CRD3.FirstOrDefault(x => x.PaymentMethodCode == "PayGuarantee").Invoices =
                    bp.CRD4.Where(x => x.PaymentMethodCode      == "PayGuarantee").ToList();

                bp.CRD3.FirstOrDefault(x => x.PaymentMethodCode == "PayCredit")
                    .BalanceLimit = bp.CRD4.Where(x => x.PaymentMethodCode == "PayCredit")
                    .Sum(x => x.AmountOverdue ?? 0);
                bp.CRD3.FirstOrDefault(x => x.PaymentMethodCode == "PayCredit").Invoices =
                    bp.CRD4.Where(x => x.PaymentMethodCode      == "PayCredit").ToList();
            }

            return bp.CRD3.ToList();
        }

        public async Task<Models.Committed.Committed?> GetCurrentCommited(int id)
        {
            var bp = await _context.BP.AsNoTracking()
                .Include(bp => bp.Commiteds)
                .ThenInclude(committed => committed.CommittedLine)
                .ThenInclude(committedLine => committedLine.CommittedLineSub)
                .ThenInclude(committedLineSub => committedLineSub.Brand).FirstOrDefaultAsync(x => x.Id == id);
            if (bp.Commiteds == null)
            {
                return null;
            }

            bp.CurrentCommited = bp.Commiteds?
                .FirstOrDefault(p => p.CommittedYear!.Value.Year == DateTime.Now.Year && p.DocStatus == "A");
            if (bp.CurrentCommited == null)
            {
                return null;
            }

            var doc = _context.ODOC
                .AsNoTracking()
                .Include(p => p.ItemDetail)!.ThenInclude(p => p.Item).ThenInclude(p => p.Packing)
                .Where(p => p.Status == "DXN")
                .Where(p => p.CardId == bp.Id && p.DocDate.Value.Year == DateTime.Now.Year).ToList()
                .SelectMany(p => p!.ItemDetail!).ToList();

            // var doc = _context.ODOC.Include(p => p.ItemDetail)!.ThenInclude(p => p.Item)
            //     .Where(p => p.CardId == bp.Id && p.DocDate.Value.Year == DateTime.Now.Year).ToList();

            bp.CurrentCommited.CommittedLine?.ForEach
            (root =>
                {
                    root.CommittedLineSub?.ForEach(xx =>
                    {
                        var totalz = doc.Where(b =>
                                                   b.Item?.IndustryId == xx.IndustryId && xx.Brand != null &&
                                                   b.Item.BrandId     != null          && xx
                                                       .Brand.Select(m => m.Id).ToList()
                                                       .Contains(b.Item.BrandId.Value)).ToList();
                        xx.CurrentVolumn = totalz.Sum(x => (x.Item?.Packing?.Volumn.Value * x.Quantity ?? 0));
                    });
                }
            );

            return bp.CurrentCommited;
        }

        public async Task<(BP, Mess)> GetBPByIdAsync(int id)
        {
            Mess mess = new Mess();
            try
            {
                var bp = await _context.BP
                    .AsNoTracking()
                    .AsQueryable()
                    .AsSplitQuery()
                    .Include(p => p.Groups)
                    .Include(p => p.Commiteds)
                    .ThenInclude(p => p.CommittedLine)
                    .ThenInclude(p => p.CommittedLineSub)
                    .ThenInclude(p => p.Brand)
                    .Include(p => p.Commiteds)
                    .ThenInclude(p => p.CommittedLine)
                    .ThenInclude(p => p.CommittedLineSub)
                    .ThenInclude(p => p.Industry)
                    .Include(p => p.Classify)
                    .ThenInclude(p => p.Brands)
                    .Include(p => p.Classify)
                    .ThenInclude(p => p.ItemType)
                    .Include(p => p.Classify)
                    .ThenInclude(p => p.Sizes)
                    .Include(p => p.CRD1)
                    .Include(p => p.CRD2)
                    .Include(p => p.CRD3)
                    .Include(p => p.CRD6)
                    .ThenInclude(p => p.Author)
                    .Include(p => p.CRD4)!
                    .ThenInclude(p => p.Payments)
                    .Include(p => p.CRD5)
                    .Include(p => p.SaleStaff)
                    .FirstOrDefaultAsync(p => p.Id == id);
                if (bp == null)
                {
                    mess.Errors = "BP not found";
                    mess.Status = 404;
                    return (null, mess);
                }

                bp.Classify.ForEach(p =>
                {
                    p.BrandIds    = p.Brands.Select(b => b.Id).ToList();
                    p.BpSizeIds   = p.Sizes.Select(b => b.Id).ToList();
                    p.ItemTypeIds = p.ItemType.Select(b => b.Id).ToList();
                });
                if (bp.Commiteds == null)
                {
                    return (bp, null);
                }

                return (bp, null);
            }
            catch (Exception ex)
            {
                mess.Errors = ex.Message;
                mess.Status = 900;
                return (null, mess);
            }
        }

        public async Task UpdateCrd3(int id, List<CRD3> model)
        {
            var bp = await _context.BP.Include(x => x.CRD3).FirstOrDefaultAsync();

            if (bp == null)
            {
                throw new KeyNotFoundException($"Customer with id {id} not found");
            }

            bp.CRD3 = model;

            await _context.SaveChangesAsync();
        }

        public async Task<(BP, Mess)> UpdateBPAsync(int id, BusinessPartnerView model, string cardType)
        {
            Mess mess = new Mess();
            try
            {
                var item = await _context.BP
                    // .AsNoTracking()
                    .Include(p => p.CRD1)
                    .Include(p => p.CRD2)
                    .Include(p => p.CRD3)
                    .Include(p => p.CRD4)
                    .Include(p => p.CRD5)
                    .FirstOrDefaultAsync(p => p.Id == id);
                if (item == null)
                {
                    mess.Status = 400;
                    mess.Errors = "Không tìm thấy bản ghi để cập nhập";
                    return (null, mess);
                }

                string json              = model.item;
                var    format            = "dd/MM/yyyy";
                var    dateTimeConverter = new IsoDateTimeConverter { DateTimeFormat = format };
                BP     items             = JsonConvert.DeserializeObject<BP>(json, dateTimeConverter);
                if (id != items.Id)
                {
                    mess.Status = 400;
                    mess.Errors = "Không tìm thấy bản ghi để cập nhập";
                    return (null, mess);
                }

                _modelUpdater.UpdateModel(item, items, "CRD1", "CRD2", "CRD3", "CRD4", "CRD5", "NA5");
                if (items.CRD1 != null)
                {
                    foreach (var crd1 in items.CRD1.ToList())
                    {
                        var crd1s = item.CRD1
                            .FirstOrDefault(c => c.Id == crd1.Id && c.Id != 0);

                        if (crd1s != null)
                        {
                            if (string.IsNullOrEmpty(crd1.Status))
                            {
                            }
                            else if (crd1.Status.Equals("D"))
                            {
                                _context.CRD1.Remove(crd1s);
                                item.CRD1.Remove(crd1s);
                            }
                            else if (crd1.Status.Equals("U"))
                            {
                                var dtoCRD1    = crd1.GetType();
                                var entityCRD1 = crd1s.GetType();

                                foreach (var prop in dtoCRD1.GetProperties())
                                {
                                    var dtoValue = prop.GetValue(crd1);
                                    if (dtoValue != null)
                                    {
                                        var entityProp = entityCRD1.GetProperty(prop.Name);
                                        if (entityProp != null)
                                        {
                                            entityProp.SetValue(crd1s, dtoValue);
                                        }
                                    }
                                }
                            }
                        }
                        else
                            item.CRD1.Add(crd1);
                    }

                    if (model.images != null)
                    {
                        var uploadsFolder = Path.Combine(_webHostEnvironment.ContentRootPath, "uploads");
                        Directory.CreateDirectory(uploadsFolder);
                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.images.FileName);
                        var filePath = Path.Combine(uploadsFolder, fileName);
                        var request  = _httpContextAccessor.HttpContext?.Request;
                        var baseUrl  = $"{request.Scheme}://{request.Host}";
                        var imageUrl = $"{baseUrl}/uploads/{fileName}";
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await model.images.CopyToAsync(fileStream);
                        }

                        if (string.IsNullOrEmpty(item.Avatar))
                        {
                            item.Avatar = imageUrl;
                        }
                        else
                        {
                            var filePathDel = item.Avatar;
                            if (File.Exists(filePathDel))
                            {
                                File.Delete(filePathDel);
                            }

                            item.Avatar = imageUrl;
                        }
                    }
                }

                if (items.CRD2 != null)
                {
                    foreach (var crd2 in items.CRD2.ToList())
                    {
                        var crd2s = item.CRD2
                            .FirstOrDefault(c => c.Id == crd2.Id && c.Id != 0);

                        if (crd2s != null)
                        {
                            if (string.IsNullOrEmpty(crd2.Status))
                            {
                            }
                            else if (crd2.Status.Equals("D"))
                            {
                                _context.CRD2.Remove(crd2s);
                                item.CRD2.Remove(crd2s);
                            }
                            else if (crd2.Status.Equals("U"))
                            {
                                var dtoCRD1    = crd2.GetType();
                                var entityCRD1 = crd2s.GetType();

                                foreach (var prop in dtoCRD1.GetProperties())
                                {
                                    var dtoValue = prop.GetValue(crd2);
                                    if (dtoValue != null)
                                    {
                                        var entityProp = entityCRD1.GetProperty(prop.Name);
                                        if (entityProp != null)
                                        {
                                            entityProp.SetValue(crd2s, dtoValue);
                                        }
                                    }
                                }
                            }
                        }
                        else
                            item.CRD2.Add(crd2);
                    }
                }

                if (items.CRD3 != null)
                {
                    foreach (var crd2 in items.CRD3.ToList())
                    {
                        var crd2s = item.CRD3
                            .FirstOrDefault(c => c.Id == crd2.Id && c.Id != 0);

                        if (crd2s != null)
                        {
                            if (string.IsNullOrEmpty(crd2.Status))
                            {
                            }
                            else if (crd2.Status.Equals("D"))
                            {
                                _context.CRD3.Remove(crd2s);
                                item.CRD3.Remove(crd2s);
                            }
                            else if (crd2.Status.Equals("U"))
                            {
                                var dtoCRD1    = crd2.GetType();
                                var entityCRD1 = crd2s.GetType();

                                foreach (var prop in dtoCRD1.GetProperties())
                                {
                                    var dtoValue = prop.GetValue(crd2);
                                    if (dtoValue != null)
                                    {
                                        var entityProp = entityCRD1.GetProperty(prop.Name);
                                        if (entityProp != null)
                                        {
                                            entityProp.SetValue(crd2s, dtoValue);
                                        }
                                    }
                                }
                            }
                        }
                        else
                            item.CRD3.Add(crd2);
                    }
                }

                if (items.CRD4 != null)
                {
                    foreach (var crd2 in items.CRD4.ToList())
                    {
                        var crd2s = item.CRD4
                            .FirstOrDefault(c => c.Id == crd2.Id && c.Id != 0);

                        if (crd2s != null)
                        {
                            if (string.IsNullOrEmpty(crd2.Status))
                            {
                            }
                            else if (crd2.Status.Equals("D"))
                            {
                                _context.CRD4.Remove(crd2s);
                                item.CRD4.Remove(crd2s);
                            }
                            else if (crd2.Status.Equals("U"))
                            {
                                var dtoCRD1    = crd2.GetType();
                                var entityCRD1 = crd2s.GetType();

                                foreach (var prop in dtoCRD1.GetProperties())
                                {
                                    var dtoValue = prop.GetValue(crd2);
                                    if (dtoValue != null)
                                    {
                                        var entityProp = entityCRD1.GetProperty(prop.Name);
                                        if (entityProp != null)
                                        {
                                            entityProp.SetValue(crd2s, dtoValue);
                                        }
                                    }
                                }
                            }
                        }
                        else
                            item.CRD4.Add(crd2);
                    }
                }

                if (items.CRD5 != null)
                {
                    foreach (var crd2 in items.CRD5.ToList())
                    {
                        var crd2s = item.CRD5
                            .FirstOrDefault(c => c.Id == crd2.Id && c.Id != 0);

                        if (crd2s != null)
                        {
                            if (string.IsNullOrEmpty(crd2.Status))
                            {
                            }
                            else if (crd2.Status.Equals("D"))
                            {
                                _context.CRD5.Remove(crd2s);
                                item.CRD5.Remove(crd2s);
                            }
                            else if (crd2.Status.Equals("U"))
                            {
                                var dtoCRD1    = crd2.GetType();
                                var entityCRD1 = crd2s.GetType();

                                foreach (var prop in dtoCRD1.GetProperties())
                                {
                                    var dtoValue = prop.GetValue(crd2);
                                    if (dtoValue != null)
                                    {
                                        var entityProp = entityCRD1.GetProperty(prop.Name);
                                        if (entityProp != null)
                                        {
                                            entityProp.SetValue(crd2s, dtoValue);
                                        }
                                    }
                                }
                            }
                        }
                        else
                            item.CRD5.Add(crd2);
                    }
                }

                item.CardType = cardType;
                // _context.BP.Update(item);
                await _context.SaveChangesAsync();
                return (item, null);
            }
            catch (Exception ex)
            {
                mess.Status = 400;
                mess.Errors = ex.Message;
                return (null, mess);
            }
        }

        public async Task<(bool, Mess)> SyncBPAsync(List<BPView> bps)
        {
            Mess mes = new Mess();
            try
            {
                var t1s = bps.GroupBy(p => new
                    {
                        p.CardCode,
                        p.CardName,
                        p.FrgnName,
                        p.LicTradNum,
                        p.Email,
                        p.Phone,
                        p.Person,
                        p.CNTC,
                        p.CNBL
                    })
                    .SelectMany(g => g)
                    .ToList();

                foreach (var t1 in t1s)
                {
                    using (var transaction = _context.Database.BeginTransaction())
                    {
                        var bus = await _context.BP.Include(p => p.CRD3).Include(p => p.CRD4)
                            .FirstOrDefaultAsync(b => b.LicTradNum == t1.LicTradNum);
                        if (bus != null)
                        {
                            bus.CardCode = t1.CardCode;
                            bus.CardName = t1.CardName;
                            bus.FrgnName = t1.FrgnName;
                            bus.Phone    = t1.Phone;
                            bus.Person   = t1.Person;
                            if (bus.CRD3.Count > 0)
                            {
                                foreach (var c3 in bus.CRD3)
                                {
                                    if (c3.PaymentMethodCode == "PayCredit")
                                        c3.Balance = t1.CNTC;
                                    if (c3.PaymentMethodCode == "PayGuarantee")
                                        c3.Balance = t1.CNBL;
                                }
                            }
                            else
                            {
                                var pays = await _context.PaymentMethod
                                    .Where(p => p.PaymentMethodCode == "PayCredit" ||
                                               p.PaymentMethodCode  == "PayGuarantee").ToArrayAsync();
                                List<CRD3> listCRD3 = new List<CRD3>();
                                foreach (var pay in pays)
                                {
                                    CRD3 c3s = new CRD3();
                                    c3s.PaymentMethodID   = pay.Id;
                                    c3s.PaymentMethodCode = pay.PaymentMethodCode;
                                    c3s.PaymentMethodName = pay.PaymentMethodName;
                                    c3s.BPId              = bus.Id;
                                    if (pay.PaymentMethodCode == "PayCredit")
                                        c3s.Balance = t1.CNTC;
                                    if (pay.PaymentMethodCode == "PayGuarantee")
                                        c3s.Balance = t1.CNBL;
                                    listCRD3.Add(c3s);
                                }

                                bus.CRD3 = listCRD3;
                            }

                            if (bus.CRD4.Count > 0)
                            {
                                foreach (var c4 in bus.CRD4)
                                {
                                    var c4v = bps
                                        .Where(p => p.InvoiceCode == c4.InvoiceNumber &&
                                                   p.Payment      == c4.PaymentMethodCode).Select(p => p.PaidAmount)
                                        .FirstOrDefault();
                                    c4.PaidAmount = c4v;
                                }
                            }

                            _context.BP.Update(bus);
                            await _context.SaveChangesAsync();
                            await transaction.CommitAsync();
                        }
                        else
                        {
                            BP bpp = new BP();
                            bpp.CardCode   = t1.CardCode;
                            bpp.CardName   = t1.CardName;
                            bpp.FrgnName   = t1.FrgnName;
                            bpp.Phone      = t1.Phone;
                            bpp.Person     = t1.Person;
                            bpp.CardType   = "C";
                            bpp.LicTradNum = t1.LicTradNum;
                            bpp.Email      = t1.Email;
                            var pays = await _context.PaymentMethod
                                .Where(p => p.PaymentMethodCode == "PayCredit" || p.PaymentMethodCode == "PayGuarantee")
                                .ToArrayAsync();
                            List<CRD3> listCRD3 = new List<CRD3>();
                            foreach (var pay in pays)
                            {
                                CRD3 c3s = new CRD3();
                                c3s.PaymentMethodID   = pay.Id;
                                c3s.PaymentMethodCode = pay.PaymentMethodCode;
                                c3s.PaymentMethodName = pay.PaymentMethodName;
                                c3s.BPId              = bus.Id;
                                if (pay.PaymentMethodCode == "PayCredit")
                                    c3s.Balance = t1.CNTC;
                                if (pay.PaymentMethodCode == "PayGuarantee")
                                    c3s.Balance = t1.CNBL;
                                listCRD3.Add(c3s);
                            }

                            bpp.CRD3 = listCRD3;
                            _context.BP.Add(bpp);
                            await _context.SaveChangesAsync();
                            var defaultUser = new AppUser
                            {
                                CardId   = bpp.Id,
                                UserName = t1.Email!.Substring(0, t1.Email.IndexOf("@")),
                                FullName = t1.Email!.Substring(0, t1.Email.IndexOf("@")),
                                Email    = t1.Email,
                                UserType = "NPP"
                            };

                            string userPassword = t1.Email.Substring(0, t1.Email.IndexOf("@")) + "@A123456";
                            var    user         = await _userManager.FindByNameAsync(defaultUser.UserName);
                            if (user == null)
                            {
                                var checkUser = await _userManager.CreateAsync(defaultUser, userPassword);
                            }

                            await _context.SaveChangesAsync();
                            transaction.Commit();
                        }
                    }
                }

                return (true, null);
            }
            catch (Exception ex)
            {
                mes.Errors = ex.Message;
                mes.Status = 900;
                return (false, mes);
            }
        }

        public async Task<(BP?, Mess?)> AddClassify(int BpId, List<BpClassify> classify)
        {
            var             mess        = new Mess();
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var bp = await _context.BP.Include(b => b.Classify).FirstOrDefaultAsync(b => b.Id == BpId);
                if (bp == null)
                {
                    mess.Errors = $"BP with id: {BpId} does not exist";
                    mess.Status = 404;
                    await transaction.RollbackAsync();
                    return (null, mess);
                }

                foreach (var cl in classify)
                {
                    var brands = await _context.Brand.Where(b => cl.BrandIds.Contains(b.Id)).ToListAsync();
                    cl.Brands = brands;
                    var bpsizes = await _context.BPSize.Where(b => cl.BpSizeIds.Contains(b.Id)).ToListAsync();
                    cl.Sizes = bpsizes;
                    var itemTypes = await _context.ItemType.Where(b => cl.ItemTypeIds.Contains(b.Id)).ToListAsync();
                    cl.ItemType = itemTypes;
                }

                bp.Classify?.AddRange(classify);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return (bp, null);
            }
            catch (Exception e)
            {
                mess.Errors = e.Message;
                mess.Status = 500;
                await transaction.RollbackAsync();
                return (null, mess);
            }
        }

        public async Task<Mess?> UpdateClassify(int bpId, List<BpClassify> classifys)
        {
            var mess = new Mess();

            await using var trans = await _context.Database.BeginTransactionAsync();
            try
            {
                foreach (var classify in classifys)
                {
                    var cl = await _context.BpClassify.Where(c => c.Id == classify.Id && c.BpId == bpId)
                        .Include(bpClassify => bpClassify.Brands).Include(bpClassify => bpClassify.Sizes!)
                        .Include(bpClassify => bpClassify.ItemType!)
                        .FirstOrDefaultAsync();
                    if (cl == null)
                    {
                        mess.Errors = $"Classify with id: {classify.Id} does not exist";
                        mess.Status = 404;
                        await trans.RollbackAsync();
                        return mess;
                    }

                    classify.Id   = cl.Id;
                    classify.BpId = cl.BpId;

                    _mapper.Map(classify, cl);

                    var currentBrands = cl!.Brands!.ToList();
                    var newBrandIds   = classify!.Brands!.Select(p => p.Id).ToList();

                    cl.Brands = await _context.Brand.Where(x => classify.BrandIds.Contains(x.Id)).ToListAsync();

                    var newSizeIds = classify!.Sizes!.Select(p => p.Id).ToList();


                    cl.Sizes = await _context.BPSize.Where(x => classify.BpSizeIds.Contains(x.Id)).ToListAsync();


                    var ItemTypeid = classify!.ItemType!.Select(p => p.Id).ToList();


                    cl.ItemType = await _context.ItemType.Where(x => classify.ItemTypeIds.Contains(x.Id)).ToListAsync();
                    await _context.SaveChangesAsync();
                }

                await _context.SaveChangesAsync();
                await trans.CommitAsync();
                return null;
            }
            catch (Exception ex)
            {
                mess.Errors = ex.Message;
                mess.Status = 500;
                await trans.RollbackAsync();
                return mess;
            }
        }

        public async Task<(BP?, Mess?)> RemoveClassify(int bpId, List<int> classIds)
        {
            var             mess = new Mess();
            await using var tras = await _context.Database.BeginTransactionAsync();
            try
            {
                var bp = await _context.BP
                    .Include(b => b.Classify)
                    .ThenInclude(p => p.Brands)
                    .Include(b => b.Classify)
                    .ThenInclude(p => p.Sizes)
                    .Include(b => b.Classify)
                    .ThenInclude(p => p.ItemType)
                    .FirstOrDefaultAsync(b => b.Id == bpId);

                if (bp == null)
                {
                    mess.Errors = $"BP with id: {bpId} does not exist";
                    mess.Status = 404;
                    await tras.RollbackAsync();

                    return (null, mess);
                }

                bp.Classify.RemoveAll(p => classIds.Contains(p.Id));
                await _context.SaveChangesAsync();
                await tras.CommitAsync();

                return (bp, null);
            }
            catch (Exception ex)
            {
                mess.Errors = ex.Message;
                mess.Status = 500;
                await tras.CommitAsync();
                return (null, mess);
            }
        }

        public Task<(BP?, Mess?)> AddAddress(int bpId, CRD1 address)
            => _addressService.AddAsync(bpId, address);

        public Task<(BP?, Mess?)> UpdateAddress(int bpId, CRD1 address)
            => _addressService.UpdateAsync(bpId, address);

        public Task<(BP?, Mess?)> RemoveAddress(int bpId, List<int> addressIds)
            => _addressService.RemoveAsync(bpId, addressIds);


        // Whitelist các loại tệp được phép upload làm đính kèm hồ sơ khách hàng.
        // Bất kỳ extension không nằm trong danh sách sẽ bị từ chối.
        private static readonly HashSet<string> _allowedAttachmentExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".pdf", ".doc", ".docx", ".xls", ".xlsx",
            ".jpg", ".jpeg", ".png", ".gif", ".bmp",
            ".txt", ".csv"
        };

        // Giới hạn kích thước mỗi file để tránh DoS qua disk-fill.
        private const long _maxAttachmentSize = 20L * 1024 * 1024; // 20 MB

        public async Task<(BP?, Mess?)> AddFiles(int bpId, int userId, List<IFormFile> files, string[] notes)
        {
            var mess = new Mess();

            await using var trans = await _context.Database.BeginTransactionAsync();
            try
            {
                var bp            = await _context.BP.Include(p => p.CRD6).FirstOrDefaultAsync(p => p.Id == bpId);
                var uploadFiles   = new List<string>();
                var uploadsFolder = Path.Combine(_webHostEnvironment.ContentRootPath, "uploads");
                Directory.CreateDirectory(uploadsFolder);

                int fileCount = 0;
                foreach (var file in files)
                {
                    if (file.Length > 0)
                    {
                        // Validate size — từ chối file quá lớn trước khi đọc xuống đĩa.
                        if (file.Length > _maxAttachmentSize)
                        {
                            mess.Status = 400;
                            mess.Errors = $"File '{file.FileName}' vượt quá giới hạn {_maxAttachmentSize / (1024 * 1024)} MB.";
                            await trans.RollbackAsync();
                            return (null, mess);
                        }

                        // Validate extension — chỉ chấp nhận các loại trong whitelist.
                        var extension = Path.GetExtension(file.FileName);
                        if (string.IsNullOrEmpty(extension) || !_allowedAttachmentExtensions.Contains(extension))
                        {
                            mess.Status = 400;
                            mess.Errors = $"Định dạng tệp '{extension}' không được phép. Chỉ chấp nhận: {string.Join(", ", _allowedAttachmentExtensions)}.";
                            await trans.RollbackAsync();
                            return (null, mess);
                        }

                        var fileName = Guid.NewGuid().ToString() + extension;
                        var filePath = Path.Combine(uploadsFolder, fileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(fileStream);
                        }

                        uploadFiles.Add(filePath);
                        HttpRequest? request = _httpContextAccessor.HttpContext?.Request;
                        if (request != null)
                        {
                            var baseUrl = $"{request.Scheme}://{request.Host}";
                            var fileUrl = $"{baseUrl}/uploads/{fileName}";

                            var newCrd6 = new CRD6
                            {
                                FileName = file.FileName,
                                Note     = notes[fileCount],
                                FileUrl  = fileUrl,
                                AuthorId = userId,
                            };

                            bp.CRD6.Add(newCrd6);
                        }
                    }

                    fileCount++;
                }

                await _context.SaveChangesAsync();
                await trans.CommitAsync();
                return (bp, null);
            }
            catch (Exception ex)
            {
                mess.Errors = ex.Message;
                mess.Status = 500;
                await trans.RollbackAsync();
                return (null, mess);
            }
        }

        public async Task<(BP?, Mess?)> RemoveFile(int bpId, List<int> fileIds)
        {
            var             mess  = new Mess();
            await using var trans = await _context.Database.BeginTransactionAsync();

            try
            {
                var bp = await _context.BP.Include(p => p.CRD6).FirstOrDefaultAsync(p => p.Id == bpId);
                if (bp == null)
                {
                    mess.Errors = $"BP with id: {bpId} does not exist";
                    mess.Status = 404;
                    await trans.RollbackAsync();
                    return (null, mess);
                }

                bp.CRD6!.RemoveAll(p => fileIds.Contains(p.Id));
                await _context.SaveChangesAsync();
                await trans.CommitAsync();
                return (bp, null);
            }
            catch (Exception e)
            {
                mess.Errors = e.Message;
                mess.Status = 500;
                await trans.RollbackAsync();
                return (null, mess);
            }
        }

        public async Task<Mess?> UpdateFiles(int bpId, List<CRD6> files)
        {
            await using var trans = await _context.Database.BeginTransactionAsync();
            try
            {
                var bp = await _context.CRD6.Where(p => files.Select(x => x.Id).ToList().Contains(p.Id)).ToListAsync();

                var fileUpdateIdx = 0;
                foreach (var s in bp)
                {
                    if (s.Id == files[fileUpdateIdx].Id)
                    {
                        s.Note = files[fileUpdateIdx].Note;
                    }

                    fileUpdateIdx++;
                }

                await _context.SaveChangesAsync();
                await trans.CommitAsync();
                return null;
            }
            catch (Exception ex)
            {
                var mess = new Mess
                {
                    Errors = ex.Message,
                    Status = 500,
                };
                await trans.RollbackAsync();

                return mess;
            }
        }

        public async Task<(bool, Mess)> SyncBPAsync()
        {
            Mess ms = new Mess();
            try
            {
                var client = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Get,
                                                     _endpoints.Host + "/BPMasterData");
                request.Headers.Add("accept", "*/*");
                var response = await client.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    // Read the response content as a JSON string
                    var jsonString = await response.Content.ReadAsStringAsync();
                    jsonString = jsonString.Replace("\"OCRD\":", "");
                    int jsonLen = jsonString.Length;

                    string jsonStringF = jsonString.Substring(1);

                    jsonLen = jsonStringF.Length - 1;
                    string jsonStringL = jsonStringF.Substring(0, jsonLen);
                    if (jsonString.Length > 0)
                    {
                        var bps = JsonConvert.DeserializeObject<List<BPSyncView>>(jsonStringL);
                        var t1s = bps.GroupBy(p => new
                            {
                                p.CardCode,
                                p.CardName,
                                p.FrgnName,
                                p.LicTradNum,
                                p.Email,
                                p.Phone,
                                p.Person,
                                p.CNTC,
                                p.CNBL,
                                p.Times,
                                p.TimesBL
                            })
                            .SelectMany(g => g)
                            .ToList();
                        var LicTradNum = t1s.Select(e => e.LicTradNum).ToList();
                        var CardCode   = t1s.Select(e => e.CardCode).ToList();
                        foreach (var t1 in t1s)
                        {
                            using (var transaction = _context.Database.BeginTransaction())
                            {
                                var bus = await _context.BP.Include(p => p.CRD3)
                                    .FirstOrDefaultAsync(b => b.LicTradNum == t1.LicTradNum ||
                                                             b.CardCode    == t1.CardCode);
                                if (bus != null)
                                {
                                    bus.CardCode = t1.CardCode;
                                    bus.CardName = t1.CardName;
                                    bus.FrgnName = t1.FrgnName;
                                    bus.Phone    = t1.Phone;
                                    bus.Person   = t1.Person;
                                    bus.Email    = t1.Email;
                                    if (bus.CRD3.Count > 0)
                                    {
                                        foreach (var c3 in bus.CRD3)
                                        {
                                            if (c3.PaymentMethodCode == "PayCredit")
                                            {
                                                if (t1.CNTC != 0)
                                                    c3.Balance = t1.CNTC;
                                                if (t1.CNTC == 0)
                                                    c3.Balance = null;
                                                if (c3.Times != null)
                                                    c3.Times = t1.Times;
                                            }

                                            if (c3.PaymentMethodCode == "PayGuarantee")
                                            {
                                                if (t1.CNBL != 0)
                                                    c3.Balance = t1.CNBL;
                                                if (t1.CNBL == 0)
                                                    c3.Balance = null;
                                                if (c3.Times != null)
                                                    c3.Times = t1.TimesBL;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        var pays = await _context.PaymentMethod
                                            .Where(p => p.PaymentMethodCode == "PayCredit" ||
                                                       p.PaymentMethodCode  == "PayGuarantee").ToArrayAsync();
                                        List<CRD3> listCRD3 = new List<CRD3>();
                                        foreach (var pay in pays)
                                        {
                                            CRD3 c3s = new CRD3();
                                            c3s.PaymentMethodID   = pay.Id;
                                            c3s.PaymentMethodCode = pay.PaymentMethodCode;
                                            c3s.PaymentMethodName = pay.PaymentMethodName;
                                            c3s.BPId              = bus.Id;
                                            if (pay.PaymentMethodCode == "PayCredit")
                                            {
                                                if (t1.CNTC != 0)
                                                    c3s.Balance = t1.CNTC;
                                                if (t1.CNTC == 0)
                                                    c3s.Balance = null;
                                                c3s.Times = t1.Times;
                                            }

                                            if (pay.PaymentMethodCode == "PayGuarantee")
                                            {
                                                if (t1.CNBL != 0)
                                                    c3s.Balance = t1.CNBL;
                                                if (t1.CNBL == 0)
                                                    c3s.Balance = null;
                                                c3s.Times = t1.TimesBL;
                                            }

                                            listCRD3.Add(c3s);
                                        }

                                        bus.CRD3 = listCRD3;
                                    }

                                    _context.BP.Update(bus);
                                    await _context.SaveChangesAsync();
                                    await transaction.CommitAsync();
                                }
                                else
                                {
                                    BP bpp = new BP();
                                    bpp.CardCode   = t1.CardCode;
                                    bpp.CardName   = t1.CardName;
                                    bpp.FrgnName   = t1.FrgnName;
                                    bpp.Phone      = t1.Phone;
                                    bpp.Person     = t1.Person;
                                    bpp.CardType   = "C";
                                    bpp.LicTradNum = t1.LicTradNum;
                                    bpp.Status     = "A";
                                    bpp.Email      = t1.Email;
                                    var pays = await _context.PaymentMethod
                                        .Where(p => p.PaymentMethodCode == "PayCredit" ||
                                                   p.PaymentMethodCode  == "PayGuarantee").ToArrayAsync();
                                    List<CRD3> listCRD3 = new List<CRD3>();
                                    foreach (var pay in pays)
                                    {
                                        CRD3 c3s = new CRD3();
                                        c3s.PaymentMethodID   = pay.Id;
                                        c3s.PaymentMethodCode = pay.PaymentMethodCode;
                                        c3s.PaymentMethodName = pay.PaymentMethodName;
                                        //c3s.BPId = bus.Id;
                                        if (pay.PaymentMethodCode == "PayCredit")
                                        {
                                            if (t1.CNTC != 0)
                                                c3s.Balance = t1.CNTC;
                                            if (t1.CNTC == 0)
                                                c3s.Balance = null;
                                            c3s.Times = t1.Times;
                                        }

                                        if (pay.PaymentMethodCode == "PayGuarantee")
                                        {
                                            if (t1.CNBL != 0)
                                                c3s.Balance = t1.CNBL;
                                            if (t1.CNBL == 0)
                                                c3s.Balance = null;
                                            c3s.Times = t1.TimesBL;
                                        }

                                        listCRD3.Add(c3s);
                                    }

                                    bpp.CRD3 = listCRD3;
                                    _context.BP.Add(bpp);
                                    await _context.SaveChangesAsync();
                                    var defaultUser = new AppUser
                                    {
                                        CardId   = bpp.Id,
                                        UserName = t1.Email!.Substring(0, t1.Email.IndexOf("@")),
                                        FullName = t1.Email!.Substring(0, t1.Email.IndexOf("@")),
                                        Email    = t1.Email,
                                        UserType = "NPP"
                                    };
                                    await _context.SaveChangesAsync();
                                    transaction.Commit();
                                }
                            }
                        }
                    }
                }
                else
                {
                    ms.Status = (int)response.StatusCode;
                    ms.Errors = "Lỗi đồng bộ";
                    return (false, ms);
                }

                return (true, null);
            }
            catch (Exception ex)
            {
                ms.Errors = ex.ToString();
                ms.Status = 900;
                return (false, ms);
            }
        }

        private List<CRD3> BuildCRD3List(dynamic t1, List<PaymentMethod> pays)
        {
            return pays.Select(pay => new CRD3
            {
                PaymentMethodID   = pay.Id,
                PaymentMethodCode = pay.PaymentMethodCode,
                PaymentMethodName = pay.PaymentMethodName,
                Balance           = pay.PaymentMethodCode == "PayCredit" ? t1.CNTC : t1.CNBL,
                Times             = pay.PaymentMethodCode == "PayCredit" ? t1.Times : t1.TimesBL
            }).ToList();
        }

        public async Task<bool> SyncBPCRD4Async()
        {
            Mess ms = new Mess();
            try
            {
                var client = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Get,
                                                     _endpoints.Host + "/BPMasterData");
                request.Headers.Add("accept", "*/*");
                var response = await client.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    // Read the response content as a JSON string
                    var jsonString = await response.Content.ReadAsStringAsync();
                    jsonString = jsonString.Replace("\"OCRD\":", "");
                    int jsonLen = jsonString.Length;

                    string jsonStringF = jsonString.Substring(1);

                    jsonLen = jsonStringF.Length - 1;
                    string jsonStringL = jsonStringF.Substring(0, jsonLen);
                    if (jsonString.Length > 0)
                    {
                        var bps = JsonConvert.DeserializeObject<List<BPSyncView>>(jsonStringL);
                        var t1s = bps.GroupBy(p => new
                            {
                                p.CardCode,
                                p.CardName,
                                p.FrgnName,
                                p.LicTradNum,
                                p.Email,
                                p.Phone,
                                p.Person,
                                p.CNTC,
                                p.CNBL,
                                p.Times,
                                p.TimesBL
                            })
                            .SelectMany(g => g)
                            .ToList();

                        foreach (var t1 in t1s)
                        {
                            using (var transaction = _context.Database.BeginTransaction())
                            {
                                var bus = await _context.BP.Include(p => p.CRD3).Include(p => p.CRD4)
                                    .FirstOrDefaultAsync(b => b.LicTradNum == t1.LicTradNum ||
                                                             b.CardCode    == t1.CardCode);
                                if (bus != null)
                                {
                                    bus.CardCode = t1.CardCode;
                                    bus.CardName = t1.CardName;
                                    bus.FrgnName = t1.FrgnName;
                                    bus.Phone    = t1.Phone;
                                    bus.Person   = t1.Person;
                                    if (bus.CRD3.Count > 0)
                                    {
                                        foreach (var c3 in bus.CRD3)
                                        {
                                            if (c3.PaymentMethodCode == "PayCredit")
                                            {
                                                if (t1.CNTC != 0)
                                                    c3.Balance = t1.CNTC;
                                                if (t1.CNTC == 0)
                                                    c3.Balance = null;
                                                if (c3.Times != null)
                                                    c3.Times = t1.Times;
                                            }

                                            if (c3.PaymentMethodCode == "PayGuarantee")
                                            {
                                                if (t1.CNBL != 0)
                                                    c3.Balance = t1.CNBL;
                                                if (t1.CNBL == 0)
                                                    c3.Balance = null;
                                                if (c3.Times != null)
                                                    c3.Times = t1.TimesBL;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        var pays = await _context.PaymentMethod
                                            .Where(p => p.PaymentMethodCode == "PayCredit" ||
                                                       p.PaymentMethodCode  == "PayGuarantee").ToArrayAsync();
                                        List<CRD3> listCRD3 = new List<CRD3>();
                                        foreach (var pay in pays)
                                        {
                                            CRD3 c3s = new CRD3();
                                            c3s.PaymentMethodID   = pay.Id;
                                            c3s.PaymentMethodCode = pay.PaymentMethodCode;
                                            c3s.PaymentMethodName = pay.PaymentMethodName;
                                            c3s.BPId              = bus.Id;
                                            if (pay.PaymentMethodCode == "PayCredit")
                                            {
                                                if (t1.CNTC != 0)
                                                    c3s.Balance = t1.CNTC;
                                                if (t1.CNTC == 0)
                                                    c3s.Balance = null;
                                                c3s.Times = t1.Times;
                                            }

                                            if (pay.PaymentMethodCode == "PayGuarantee")
                                            {
                                                if (t1.CNBL != 0)
                                                    c3s.Balance = t1.CNBL;
                                                if (t1.CNBL == 0)
                                                    c3s.Balance = null;
                                                c3s.Times = t1.TimesBL;
                                            }

                                            listCRD3.Add(c3s);
                                        }

                                        bus.CRD3 = listCRD3;
                                    }

                                    _context.BP.Update(bus);
                                    await _context.SaveChangesAsync();
                                    await transaction.CommitAsync();
                                }
                                else
                                {
                                    BP bpp = new BP();
                                    bpp.CardCode   = t1.CardCode;
                                    bpp.CardName   = t1.CardName;
                                    bpp.FrgnName   = t1.FrgnName;
                                    bpp.Phone      = t1.Phone;
                                    bpp.Person     = t1.Person;
                                    bpp.CardType   = "C";
                                    bpp.LicTradNum = t1.LicTradNum;
                                    bpp.Status     = "A";
                                    bpp.Email      = t1.Email;
                                    var pays = await _context.PaymentMethod
                                        .Where(p => p.PaymentMethodCode == "PayCredit" ||
                                                   p.PaymentMethodCode  == "PayGuarantee").ToArrayAsync();
                                    List<CRD3> listCRD3 = new List<CRD3>();
                                    foreach (var pay in pays)
                                    {
                                        CRD3 c3s = new CRD3();
                                        c3s.PaymentMethodID   = pay.Id;
                                        c3s.PaymentMethodCode = pay.PaymentMethodCode;
                                        c3s.PaymentMethodName = pay.PaymentMethodName;
                                        //c3s.BPId = bus.Id;
                                        if (pay.PaymentMethodCode == "PayCredit")
                                        {
                                            if (t1.CNTC != 0)
                                                c3s.Balance = t1.CNTC;
                                            if (t1.CNTC == 0)
                                                c3s.Balance = null;
                                            c3s.Times = t1.Times;
                                        }

                                        if (pay.PaymentMethodCode == "PayGuarantee")
                                        {
                                            if (t1.CNBL != 0)
                                                c3s.Balance = t1.CNBL;
                                            if (t1.CNBL == 0)
                                                c3s.Balance = null;
                                            c3s.Times = t1.TimesBL;
                                        }

                                        listCRD3.Add(c3s);
                                    }

                                    bpp.CRD3 = listCRD3;
                                    _context.BP.Add(bpp);
                                    await _context.SaveChangesAsync();
                                    var defaultUser = new AppUser
                                    {
                                        CardId   = bpp.Id,
                                        UserName = t1.Email!.Substring(0, t1.Email.IndexOf("@")),
                                        FullName = t1.Email!.Substring(0, t1.Email.IndexOf("@")),
                                        Email    = t1.Email,
                                        UserType = "NPP"
                                    };
                                    await _context.SaveChangesAsync();
                                    transaction.Commit();
                                }
                            }
                        }
                    }
                }
                else
                {
                    ms.Status = (int)response.StatusCode;
                    ms.Errors = "Lỗi đồng bộ";
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                ms.Errors = ex.ToString();
                ms.Status = 900;
                return false;
            }
        }

        public async Task<bool> SyncTTDHAsync()
        {
            Mess ms = new Mess();
            try
            {
                var client = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Get,
                                                     _endpoints.Host + "/ApprovalOrder");
                request.Headers.Add("accept", "*/*");
                var response = await client.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    // Read the response content as a JSON string
                    var jsonString = await response.Content.ReadAsStringAsync();
                    jsonString = jsonString.Replace("\"OWDD\":", "");
                    int jsonLen = jsonString.Length;

                    string jsonStringF = jsonString.Substring(1);

                    jsonLen = jsonStringF.Length - 1;
                    string jsonStringL = jsonStringF.Substring(0, jsonLen);
                    if (jsonString.Length > 0)
                    {
                        var bps = JsonConvert.DeserializeObject<List<OWDDS>>(jsonStringL);
                        if (bps != null)
                        {
                            var checkbp = bps.Select(e => e.OrderCode).ToList();
                            var order = _context.ODOC.Where(e => e.Status == "DXN" && checkbp.Contains(e.InvoiceCode))
                                .ToList();
                            order.ForEach(o => o.Status = "DGH");
                            await _context.SaveChangesAsync();
                        }
                    }
                }
                else
                {
                    ms.Status = (int)response.StatusCode;
                    ms.Errors = "Lỗi đồng bộ";
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                ms.Errors = ex.ToString();
                ms.Status = 900;
                return false;
            }
        }

        public async Task<bool> SyncTTDHHAsync()
        {
            Mess ms = new Mess();
            try
            {
                var client = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Get,
                                                     _endpoints.Host + "/RejectOrder");
                request.Headers.Add("accept", "*/*");
                var response = await client.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    // Read the response content as a JSON string
                    var jsonString = await response.Content.ReadAsStringAsync();
                    jsonString = jsonString.Replace("\"OWDD\":", "");
                    int jsonLen = jsonString.Length;

                    string jsonStringF = jsonString.Substring(1);

                    jsonLen = jsonStringF.Length - 1;
                    string jsonStringL = jsonStringF.Substring(0, jsonLen);
                    if (jsonString.Length > 0)
                    {
                        var bps = JsonConvert.DeserializeObject<List<OWDDS>>(jsonStringL);
                        if (bps != null)
                        {
                            var checkbp = bps.Select(e => e.OrderCode).ToList();
                            var order = _context.ODOC.Where(e => e.Status == "DXN" && checkbp.Contains(e.InvoiceCode))
                                .ToList();
                            order.ForEach(o => o.Status = "DONG");
                            var crd4 = _context.CRD4.Where(e => checkbp.Contains(e.InvoiceNumber));
                            if (crd4 != null)
                                _context.CRD4.RemoveRange(crd4);

                            foreach (var doc in order)
                            {
                                await _pointService.OnDocumentStatusChangedAsync(doc.Id, doc.CardId ?? 0, doc.ObjType ?? 22, "DONG");
                            }

                            await _context.SaveChangesAsync();
                        }
                    }
                }
                else
                {
                    ms.Status = (int)response.StatusCode;
                    ms.Errors = "Lỗi đồng bộ";
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                ms.Errors = ex.ToString();
                ms.Status = 900;
                return false;
            }
        }

        public async Task<bool> SyncTTDH1Async(string Invocie)
        {
            Mess ms = new Mess();
            try
            {
                if (Invocie != null)
                {
                    var order = _context.ODOC
                        .Where(e => (e.Status == "DXN" || e.Status == "DGH") && Invocie == e.InvoiceCode &&
                                   e.ObjType                                            == 22).ToList();
                    order.ForEach(o => o.Status = "DONG");
                    foreach (var doc in order)
                    {
                        await _pointService.OnDocumentStatusChangedAsync(doc.Id, doc.CardId ?? 0, doc.ObjType ?? 22, "DONG");
                    }

                    var order1 = _context.ODOC
                        .Where(e => (e.Status == "DXN" || e.Status == "DGH") && Invocie == e.InvoiceCode &&
                                   e.ObjType                                            == 12).ToList();
                    order1.ForEach(o => o.Status = "DONG");
                    foreach (var doc in order1)
                    {
                        await _pointService.OnDocumentStatusChangedAsync(doc.Id, doc.CardId ?? 0, doc.ObjType ?? 22, "DONG");
                    }

                    await _context.SaveChangesAsync();
                }

                return true;
            }
            catch (Exception ex)
            {
                ms.Errors = ex.ToString();
                ms.Status = 900;
                return false;
            }
        }

        public async Task<bool> SyncIssueCancelAsync()
        {
            Mess ms = new Mess();
            try
            {
                List<OWDDS> bps    = null;
                List<OWDDS> bpsR   = null;
                var         client = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Get,
                                                     _endpoints.Host + "/RejectIssue");
                request.Headers.Add("accept", "*/*");
                var response = await client.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    // Read the response content as a JSON string
                    var jsonString = await response.Content.ReadAsStringAsync();
                    jsonString = jsonString.Replace("\"OWDD\":", "");
                    int jsonLen = jsonString.Length;

                    string jsonStringF = jsonString.Substring(1);

                    jsonLen = jsonStringF.Length - 1;
                    string jsonStringL = jsonStringF.Substring(0, jsonLen);
                    if (jsonString.Length > 0)
                    {
                        bps = JsonConvert.DeserializeObject<List<OWDDS>>(jsonStringL);
                    }
                }
                else
                {
                    ms.Status = (int)response.StatusCode;
                    ms.Errors = "Lỗi đồng bộ";
                    return false;
                }

                request = new HttpRequestMessage(HttpMethod.Get,
                                                 _endpoints.Host + "/ApprovalIssue");
                request.Headers.Add("accept", "*/*");
                response = await client.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    // Read the response content as a JSON string
                    var jsonString = await response.Content.ReadAsStringAsync();
                    jsonString = jsonString.Replace("\"OWDD\":", "");
                    int jsonLen = jsonString.Length;

                    string jsonStringF = jsonString.Substring(1);

                    jsonLen = jsonStringF.Length - 1;
                    string jsonStringL = jsonStringF.Substring(0, jsonLen);
                    if (jsonString.Length > 0)
                    {
                        bpsR = JsonConvert.DeserializeObject<List<OWDDS>>(jsonStringL);
                    }
                }
                else
                {
                    ms.Status = (int)response.StatusCode;
                    ms.Errors = "Lỗi đồng bộ";
                    return false;
                }

                if (bps != null)
                {
                    var approval = bpsR.Select(e => e.OrderCode).ToList();
                    var checkbp = bps.Where(e => !approval.Contains(e.OrderCode)).Select(e => e.OrderCode).Distinct()
                        .ToList();
                    var order = _context.ODOC.Where(e => e.Status == "DXN" && checkbp.Contains(e.InvoiceCode)).ToList();
                    order.ForEach(o => o.Status = "DONG");
                    await _context.SaveChangesAsync();
                }

                return true;
            }
            catch (Exception ex)
            {
                ms.Errors = ex.ToString();
                ms.Status = 900;
                return false;
            }
        }

        // SyncTCRD4Async, SyncCardCodeCRD4Async: đã xoá — replaced bởi
        // Service.Sync.BPSyncService delta sync (commit XXXXXXX).


        public async Task<(List<CRD3>?, Mess?)> CUDCRD3(int BpId, List<CRD3Dto> cRD3Dtos)
        {
            Mess m = new Mess();
            try
            {
                var dbList = _context.BP.FirstOrDefault(e => e.Id == BpId);
                if (dbList == null)
                {
                    m.Status = 400;
                    m.Errors = "Đã có lỗi xảy ra";
                    return (null, m);
                }

                var        crd3     = _context.CRD3.Where(e => e.BPId == BpId).ToList();
                List<CRD3> listCRD3 = new List<CRD3>();
                if (crd3.Count == 0)
                {
                    var payMethod = _context.PaymentMethod
                        .Where(e => e.PaymentMethodCode == "PayCredit" || e.PaymentMethodCode == "PayGuarantee")
                        .ToList();
                    foreach (var crd in payMethod)
                    {
                        listCRD3.Add(new CRD3
                        {
                            PaymentMethodID = crd.Id,
                            PaymentMethodCode = crd.PaymentMethodCode,
                            PaymentMethodName = crd.PaymentMethodName,
                            BPId = BpId,
                            Balance = cRD3Dtos.FirstOrDefault(e => e.PaymentMethodID == crd.Id).Balance ?? null,
                            Times = cRD3Dtos.FirstOrDefault(e => e.PaymentMethodID == crd.Id).Times ?? null
                        });
                    }

                    if (listCRD3.Count > 0)
                    {
                        _context.CRD3.AddRange(listCRD3);
                        await _context.SaveChangesAsync();
                    }

                    return (listCRD3, null);
                }
                else
                {
                    foreach (var r3 in crd3)
                    {
                        r3.Balance = cRD3Dtos.FirstOrDefault(e => e.PaymentMethodID == r3.PaymentMethodID)?.Balance ??
                            null;
                        r3.Times = cRD3Dtos.FirstOrDefault(e => e.PaymentMethodID == r3.PaymentMethodID)?.Times ?? null;
                    }

                    await _context.SaveChangesAsync();
                    return (crd3, null);
                }
            }
            catch (Exception ex)
            {
                m.Status = 400;
                m.Errors = "Đã có lỗi xảy ra";
                return (null, m);
            }
        }

        public async Task<bool> SyncCancelYCHGsync()
        {
            try
            {
                string  Db       = EncryptDecrypt.DecryptString(_configuration["SAPServiceLayer:CompanyDB"]);
                string  Username = EncryptDecrypt.DecryptString(_configuration["SAPServiceLayer:Username"]);
                string  Password = EncryptDecrypt.DecryptString(_configuration["SAPServiceLayer:Password"]);
                Cookies cookiess = null;
                ;
                var l = new LoginInfor
                {
                    CompanyDB = Db,
                    UserName  = Username,
                    Password  = Password
                };
                string url  = _configuration["SAPServiceLayer:Host"];
                string data = JsonConvert.SerializeObject(l);
                try
                {
                    if (cookiess == null || cookiess.SessionTime < DateTime.Now)
                    {
                        HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url + "Login");
                        httpWebRequest.ContentType = "application/json";
                        httpWebRequest.Method      = "POST";
                        httpWebRequest.KeepAlive   = true;
                        httpWebRequest.ServerCertificateValidationCallback += BackEndAPI.Service.Sync.Security.TlsBypass.AcceptSelfSignedInDev();
                        httpWebRequest.Headers.Add("B1S-WCFCompatible", "true");
                        httpWebRequest.Headers.Add("B1S-MetadataWithoutSession", "true");
                        httpWebRequest.Accept                         = "*/*";
                        httpWebRequest.ServicePoint.Expect100Continue = false;
                        httpWebRequest.Headers.Add("Accept-Encoding", "gzip, deflate, br");
                        httpWebRequest.AutomaticDecompression = DecompressionMethods.GZip;

                        using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                        {
                            streamWriter.Write(data);
                        }

                        try
                        {
                            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                            var cookie       = httpResponse.Headers.GetValues("Set-Cookie");
                            cookiess = new Cookies();
                            int    endIndex = cookie[1].ToString().IndexOf(";");
                            string routeid  = cookie[1].ToString().Substring(0, endIndex);
                            endIndex = cookie[0].ToString().IndexOf(";");
                            string sesion = cookie[0].ToString().Substring(0, endIndex + 1);
                            cookiess.ROUTEID     = routeid;
                            cookiess.B1SESSION   = sesion;
                            cookiess.SessionTime = DateTime.Now.AddMinutes(20);
                        }
                        catch (WebException ex)
                        {
                            return true;
                        }
                    }

                    //HttpWebRequest httpWebRequests;
                    string nextUrl = url + "InventoryGenEntries?$select=U_MDHPT&$filter= startswith(U_MDHPT , 'YCLH')";
                    int    i       = 0;
                    do
                    {
                        HttpWebRequest httpWebRequests;
                        if (i == 0)
                            httpWebRequests = (HttpWebRequest)WebRequest.Create(nextUrl);
                        else
                            httpWebRequests = (HttpWebRequest)WebRequest.Create(url + nextUrl);
                        i++;
                        httpWebRequests.ContentType = "application/json";
                        httpWebRequests.Method      = "GET";
                        httpWebRequests.KeepAlive   = true;
                        httpWebRequests.ServerCertificateValidationCallback += BackEndAPI.Service.Sync.Security.TlsBypass.AcceptSelfSignedInDev();
                        httpWebRequests.Headers.Add("B1S-WCFCompatible", "true");
                        httpWebRequests.Headers.Add("B1S-MetadataWithoutSession", "true");
                        httpWebRequests.Accept                         = "*/*";
                        httpWebRequests.ServicePoint.Expect100Continue = false;
                        httpWebRequests.Headers.Add("Accept-Encoding", "gzip, deflate, br");
                        httpWebRequests.Headers.Add("Cookie", cookiess.B1SESSION + cookiess.ROUTEID);
                        httpWebRequests.AutomaticDecompression =
                            DecompressionMethods.GZip | DecompressionMethods.Deflate;

                        try
                        {
                            using (var httpResponse = (HttpWebResponse)httpWebRequests.GetResponse())
                            using (var reader = new StreamReader(httpResponse.GetResponseStream()))
                            {
                                var responseText = reader.ReadToEnd();

                                var jsonObj = JObject.Parse(responseText);
                                var result  = jsonObj["value"].ToObject<List<CRD4InfoUpdate>>();

                                var MDHPTs = result.Select(e => e.U_MDHPT.Substring(0, 10)).ToArray();
                                var yclh = _context.ODOC.Where(e => MDHPTs.Contains(e.InvoiceCode) && e.Status == "DXN")
                                    .ToList();
                                foreach (var item in yclh)
                                {
                                    item.Status = "DONG";
                                }

                                await _context.SaveChangesAsync();
                                var nextLink = jsonObj["odata.nextLink"]?.ToString();
                                nextUrl = jsonObj["odata.nextLink"]?.ToString();
                            }
                        }
                        catch (WebException ex)
                        {
                            nextUrl = null;
                            // Có thể log lỗi nếu cần
                        }
                    } while (!string.IsNullOrEmpty(nextUrl));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return true;
                }

                return true;
            }
            catch
            {
                return true;
            }
        }
    }
}