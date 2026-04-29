using BackEndAPI.Data;
using BackEndAPI.Models.Approval;
using BackEndAPI.Models.ARInvoice;
using BackEndAPI.Models.BPGroups;
using BackEndAPI.Models.BusinessPartners;
using BackEndAPI.Models.Document;
using BackEndAPI.Models.ItemMasterData;
using BackEndAPI.Models.Other;
using BackEndAPI.Service.BPGroups;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Org.BouncyCastle.Utilities;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using AutoMapper;
using Azure;
using B1SLayer;
using BackEndAPI.Models.Promotion;
using BackEndAPI.Models.SAP;
using BackEndAPI.Service.Committeds;
using BackEndAPI.Service.EventAggregator;
using BackEndAPI.Service.Notification;
using BackEndAPI.Service.Payments;
using BackEndAPI.Service.Privile;
using BackEndAPI.Service.Promotions;
using Gridify;
using Flurl.Http;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Net;
using Function.EncryptDecrypt;
using System.Text.Json.Nodes;
using Newtonsoft.Json;
using System.Security.Claims;
using BackEndAPI.Service.Approval;
using System.Xml;
using System.Collections.ObjectModel;
using System.Collections;
using BackEndAPI.Models.Committed;
using System.Linq;
using BackEndAPI.Models.ItemGroups;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Http;
using BackEndAPI.Controllers;
using BackEndAPI.Service.BusinessPartners;
using Microsoft.AspNetCore.Routing.Constraints;
using Function.Address;
using System.Text.RegularExpressions;
using System.Text.Json;
using NHibernate.Mapping;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Razor.TagHelpers;
using BackEndAPI.Service.Zalo;
using System.IO;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using NHibernate.Criterion;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NHibernate.Cfg.MappingSchema;
using Flurl.Util;
using NHibernate.Engine;


namespace BackEndAPI.Service.Document
{
    public class Cookies
    {
        public string B1SESSION { get; set; }
        public string ROUTEID { get; set; }
        public DateTime SessionTime { get; set; }
    }
    public class LoginInfor
    {
        public string CompanyDB { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
    }
    public class ItemTypeKTSL
    {
        public bool AddAccumulate { get; set; }
        public int ItemId { get; set; }
        public string ItemType { get; set; }
    }
    public class ItemUnit
    {
        public bool AddAccumulate { get; set; }
        public int UomId { get; set; }
    }
    public partial class DocumentService : Service<ODOC>, IDocumentService
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;
        private readonly IModelUpdater _modelUpdater;
        private readonly IEventAggregator _eventAggregator;
        private readonly IMapper _mapper;
        private readonly IPrivileService _privileService;
        private readonly SLConnection _slConnection;
        private readonly IPromotionService _promotionService;
        private readonly ICommittedService _committedService;
        private readonly IBusinessPartnerService _partnerService;
        private readonly IPointSetupService _pointService;
        private readonly LoggingSystemService _systemLog;
        private readonly HttpClient _http;
        private readonly string _baseUrl;
        private readonly string _db;
        private readonly string _user;
        private readonly string _pass;
        private readonly object _loginLock = new();
        private DateTime _sessionExpires = DateTime.MinValue;
        private readonly ZaloService _zService;
        public DocumentService(ZaloService zService,IPointSetupService pointService,AppDbContext context, IEventAggregator eventAggregator, IModelUpdater modelUpdater,
            IWebHostEnvironment webHostEnvironment, IHttpContextAccessor httpContextAccessor,
            IMapper mapper, SLConnection slConnection ,IPrivileService privileService, IPromotionService promotionService,
            ICommittedService committedService, IConfiguration configuration, IBusinessPartnerService partnerService, LoggingSystemService systemLog) : base(context)
        {
            _systemLog = systemLog;
            _committedService = committedService;
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _httpContextAccessor = httpContextAccessor;
            _modelUpdater = modelUpdater;
            _eventAggregator = eventAggregator;
            _mapper = mapper;
            _configuration = configuration;
            _slConnection = slConnection;
            _privileService = privileService;
            _promotionService = promotionService;
            _partnerService = partnerService;
            _pointService = pointService;
            _baseUrl = configuration["SAPServiceLayer:Host"]?.TrimEnd('/') + "/";
            _db = EncryptDecrypt.DecryptString(configuration["SAPServiceLayer:CompanyDB"]);
            _user = EncryptDecrypt.DecryptString(configuration["SAPServiceLayer:Username"]);
            _pass = EncryptDecrypt.DecryptString(configuration["SAPServiceLayer:Password"]);
            _zService = zService;
            var handler = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                UseCookies = true,
                CookieContainer = new CookieContainer()
            };

            // TLS cert bypass cho SAP B1 self-signed cert — chỉ kích hoạt khi
            // TlsBypass.IsEnabled (Development hoặc env var ALLOW_SELF_SIGNED_TLS=true).
            // Production: callback null → .NET validate cert nghiêm ngặt như mặc định.
            if (BackEndAPI.Service.Sync.Security.TlsBypass.IsEnabled)
            {
                handler.ServerCertificateCustomValidationCallback =
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
            }

            _http = new HttpClient(handler)
            {
                BaseAddress = new Uri(_baseUrl),
                Timeout = TimeSpan.FromSeconds(20)
            };

            _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _http.DefaultRequestHeaders.Add("B1S-WCFCompatible", "true");
            _http.DefaultRequestHeaders.Add("B1S-MetadataWithoutSession", "true");
            _http.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
        }
        public async Task<Cookies> EnsureLoginAsync(Cookies cookiess, string url, string data)
        {
            if (cookiess != null && cookiess.SessionTime > DateTime.Now)
                return cookiess; // session còn hiệu lực

            // Tạo request login
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url + "Login");
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";
            httpWebRequest.KeepAlive = true;
            httpWebRequest.ServerCertificateValidationCallback += BackEndAPI.Service.Sync.Security.TlsBypass.AcceptSelfSignedInDev();
            httpWebRequest.Headers.Add("B1S-WCFCompatible", "true");
            httpWebRequest.Headers.Add("B1S-MetadataWithoutSession", "true");
            httpWebRequest.Accept = "*/*";
            httpWebRequest.ServicePoint.Expect100Continue = false;
            httpWebRequest.Headers.Add("Accept-Encoding", "gzip, deflate, br");
            httpWebRequest.AutomaticDecompression = DecompressionMethods.GZip;

            // Ghi dữ liệu đăng nhập
            using (var streamWriter = new StreamWriter(await httpWebRequest.GetRequestStreamAsync()))
            {
                await streamWriter.WriteAsync(data);
            }

            try
            {
                using (var httpResponse = (HttpWebResponse)await httpWebRequest.GetResponseAsync())
                {
                    var cookieHeader = httpResponse.Headers.GetValues("Set-Cookie");
                    if (cookieHeader == null || cookieHeader.Length < 2)
                        throw new Exception("Không nhận được cookie hợp lệ từ SAP.");

                    var newCookie = new Cookies();

                    // Lấy ROUTEID
                    int endIndex = cookieHeader[1].IndexOf(";");
                    string routeid = cookieHeader[1].Substring(0, endIndex);

                    // Lấy B1SESSION
                    endIndex = cookieHeader[0].IndexOf(";");
                    string session = cookieHeader[0].Substring(0, endIndex + 1);

                    newCookie.ROUTEID = routeid;
                    newCookie.B1SESSION = session;
                    newCookie.SessionTime = DateTime.Now.AddMinutes(20);

                    return newCookie;
                }
            }
            catch (WebException ex)
            {
                // Nếu server trả lỗi HTTP (ví dụ 401), đọc nội dung lỗi để debug/log
                string errorMsg = "";
                using (var stream = ex.Response?.GetResponseStream())
                using (var reader = new StreamReader(stream ?? Stream.Null))
                    errorMsg = await reader.ReadToEndAsync();

                Console.WriteLine($"[SAP LOGIN ERROR] {ex.Message} - {errorMsg}");
                throw new Exception("Không thể đăng nhập SAP B1 Service Layer.", ex);
            }
        }
        public async Task<bool> SyncToSapAsync()
        {
            try
            {
                string Db = EncryptDecrypt.DecryptString(_configuration["SAPServiceLayer:CompanyDB"]);
                string Username = EncryptDecrypt.DecryptString(_configuration["SAPServiceLayer:Username"]);
                string Password = EncryptDecrypt.DecryptString(_configuration["SAPServiceLayer:Password"]);
                Cookies cookiess = null; ;
                var l = new LoginInfor
                {
                    CompanyDB = Db,
                    UserName = Username,
                    Password = Password
                };
                string url = _configuration["SAPServiceLayer:Host"];
                string data = JsonConvert.SerializeObject(l);
                var doc = _context.ODOC
                .Include(x => x.ItemDetail)
                .Include(x => x.Promotion)
                .Include(x => x.Address)
                .Include(x => x.BP)
                .ThenInclude(x => x.CRD3)
                .Include(x => x.PaymentInfo)
                .FirstOrDefault(x => x.Status == "DXN" && x.ObjType == 22 && x.IsSync == false);
                if (doc != null)
                {
                    if (cookiess == null || cookiess.SessionTime < DateTime.Now)
                    {

                        HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url + "Login");
                        httpWebRequest.ContentType = "application/json";
                        httpWebRequest.Method = "POST";
                        httpWebRequest.KeepAlive = true;
                        httpWebRequest.ServerCertificateValidationCallback += BackEndAPI.Service.Sync.Security.TlsBypass.AcceptSelfSignedInDev();
                        httpWebRequest.Headers.Add("B1S-WCFCompatible", "true");
                        httpWebRequest.Headers.Add("B1S-MetadataWithoutSession", "true");
                        httpWebRequest.Accept = "*/*";
                        httpWebRequest.ServicePoint.Expect100Continue = false;
                        httpWebRequest.Headers.Add("Accept-Encoding", "gzip, deflate, br");
                        httpWebRequest.AutomaticDecompression = DecompressionMethods.GZip;

                        using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                        { streamWriter.Write(data); }
                        try
                        {
                            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                            var cookie = httpResponse.Headers.GetValues("Set-Cookie");
                            cookiess = new Cookies();
                            int endIndex = cookie[1].ToString().IndexOf(";");
                            string routeid = cookie[1].ToString().Substring(0, endIndex);
                            endIndex = cookie[0].ToString().IndexOf(";");
                            string sesion = cookie[0].ToString().Substring(0, endIndex + 1);
                            cookiess.ROUTEID = routeid;
                            cookiess.B1SESSION = sesion;
                            cookiess.SessionTime = DateTime.Now.AddMinutes(20);
                        }
                        catch (WebException ex)
                        {
                        }
                    }
                    HttpWebRequest httpWebRequestsdraft = (HttpWebRequest)WebRequest.Create(url + "Drafts?$filter=U_MDHPT eq '" + doc.InvoiceCode + "'&$select = DocEntry,U_MDHPT");
                    httpWebRequestsdraft.ContentType = "application/json";
                    httpWebRequestsdraft.Method = "GET";
                    httpWebRequestsdraft.KeepAlive = true;
                    httpWebRequestsdraft.ServerCertificateValidationCallback += BackEndAPI.Service.Sync.Security.TlsBypass.AcceptSelfSignedInDev();
                    httpWebRequestsdraft.Headers.Add("B1S-WCFCompatible", "true");
                    httpWebRequestsdraft.Headers.Add("B1S-MetadataWithoutSession", "true");
                    httpWebRequestsdraft.Accept = "*/*";
                    httpWebRequestsdraft.ServicePoint.Expect100Continue = false;
                    httpWebRequestsdraft.Headers.Add("Accept-Encoding", "gzip, deflate, br");
                    httpWebRequestsdraft.Headers.Add("Cookie", cookiess.B1SESSION + cookiess.ROUTEID);
                    httpWebRequestsdraft.AutomaticDecompression = DecompressionMethods.GZip;
                    try
                    {
                        var httpResponse = (HttpWebResponse)httpWebRequestsdraft.GetResponse();
                        if (httpResponse.StatusCode == HttpStatusCode.OK)
                        {
                            using (var reader = new StreamReader(httpResponse.GetResponseStream()))
                            {
                                var responseText = reader.ReadToEnd();


                                var dataxxx = JsonConvert.DeserializeObject<DraftResponse>(responseText);

                                if (dataxxx?.Value?.Count > 0)
                                {
                                    var bp = dataxxx.Value[0];
                                    if (doc.InvoiceCode == bp.U_MDHPT)
                                    {
                                        doc.IsSync = true;
                                        doc.ItemDetail.ForEach(item =>
                                        {
                                            item.IsSync = true;
                                            item.DraftId = bp.DocEntry.ToString();
                                        });
                                        await _context.SaveChangesAsync();
                                        return true;
                                    }
                                }

                            }

                        }

                    }
                    catch (WebException exx)
                    {

                    }
                    var line1 = doc.ItemDetail.Where(x => x.PaymentMethodCode == "PayNow" && x.Quantity > 0).Select(x =>
                    new SapOrderLine()
                    {
                        Quantity = x.Quantity,
                        ItemCode = x.ItemCode,
                        LineTotal = x.PriceAfterDist * x.Quantity,
                        UnitPrice = x.Price,
                        VatGroup = "SVN5",
                        LineId = x.LineId,
                        U_LHH = "01",
                    }).ToList();
                    var line2 = doc.ItemDetail.Where(x => x.PaymentMethodCode == "PayCredit" && x.Quantity > 0).Select(x =>
                        new SapOrderLine()
                        {
                            Quantity = x.Quantity,
                            ItemCode = x.ItemCode,
                            LineTotal = x.PriceAfterDist * x.Quantity,
                            UnitPrice = x.Price,
                            LineId = x.LineId,
                            U_LHH = "01",
                            VatGroup = "SVN5"
                        }).ToList();
                    var line3 = doc.ItemDetail.Where(x => x.PaymentMethodCode == "PayGuarantee" && x.Quantity > 0).Select(x =>
                        new SapOrderLine()
                        {
                            Quantity = x.Quantity,
                            ItemCode = x.ItemCode,
                            LineTotal = x.PriceAfterDist * x.Quantity,
                            UnitPrice = x.Price,
                            LineId = x.LineId,
                            U_LHH = "01",
                            VatGroup = "SVN5"
                        }).ToList();
                    SapOrderBP sl = new SapOrderBP();
                    try
                    {
                        //cookiess = await EnsureLoginAsync(cookiess, url, data);
                        if (cookiess == null || cookiess.SessionTime < DateTime.Now)
                        {

                            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url + "Login");
                            httpWebRequest.ContentType = "application/json";
                            httpWebRequest.Method = "POST";
                            httpWebRequest.KeepAlive = true;
                            httpWebRequest.ServerCertificateValidationCallback += BackEndAPI.Service.Sync.Security.TlsBypass.AcceptSelfSignedInDev();
                            httpWebRequest.Headers.Add("B1S-WCFCompatible", "true");
                            httpWebRequest.Headers.Add("B1S-MetadataWithoutSession", "true");
                            httpWebRequest.Accept = "*/*";
                            httpWebRequest.ServicePoint.Expect100Continue = false;
                            httpWebRequest.Headers.Add("Accept-Encoding", "gzip, deflate, br");
                            httpWebRequest.AutomaticDecompression = DecompressionMethods.GZip;

                            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                            { streamWriter.Write(data); }
                            try
                            {
                                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                                var cookie = httpResponse.Headers.GetValues("Set-Cookie");
                                cookiess = new Cookies();
                                int endIndex = cookie[1].ToString().IndexOf(";");
                                string routeid = cookie[1].ToString().Substring(0, endIndex);
                                endIndex = cookie[0].ToString().IndexOf(";");
                                string sesion = cookie[0].ToString().Substring(0, endIndex + 1);
                                cookiess.ROUTEID = routeid;
                                cookiess.B1SESSION = sesion;
                                cookiess.SessionTime = DateTime.Now.AddMinutes(20);
                            }
                            catch (WebException ex)
                            {
                            }
                        }
                        HttpWebRequest httpWebRequests = (HttpWebRequest)WebRequest.Create(url + "BusinessPartners?$select=CardCode,U_DiachiHD,FederalTaxID,U_Khuvuc&$filter=CardCode eq '" + doc.BP.CardCode + "'");
                        httpWebRequests.ContentType = "application/json";
                        httpWebRequests.Method = "GET";
                        httpWebRequests.KeepAlive = true;
                        httpWebRequests.ServerCertificateValidationCallback += BackEndAPI.Service.Sync.Security.TlsBypass.AcceptSelfSignedInDev();
                        httpWebRequests.Headers.Add("B1S-WCFCompatible", "true");
                        httpWebRequests.Headers.Add("B1S-MetadataWithoutSession", "true");
                        httpWebRequests.Accept = "*/*";
                        httpWebRequests.ServicePoint.Expect100Continue = false;
                        httpWebRequests.Headers.Add("Accept-Encoding", "gzip, deflate, br");
                        httpWebRequests.Headers.Add("Cookie", cookiess.B1SESSION + cookiess.ROUTEID);
                        httpWebRequests.AutomaticDecompression = DecompressionMethods.GZip;
                        try
                        {
                            var httpResponse = (HttpWebResponse)httpWebRequests.GetResponse();
                            if (httpResponse.StatusCode == HttpStatusCode.OK)
                            {
                                using (var reader = new StreamReader(httpResponse.GetResponseStream()))
                                {
                                    var responseText = reader.ReadToEnd();


                                    var dataxxx = JsonConvert.DeserializeObject<BPResponse>(responseText);

                                    if (dataxxx?.Value?.Count > 0)
                                    {
                                        var bp = dataxxx.Value[0];
                                        sl.CardCode = bp.CardCode;
                                        sl.FederalTaxID = bp.FederalTaxID;
                                        sl.U_DiachiHD = bp.U_DiachiHD;
                                        sl.U_Khuvuc = bp.U_Khuvuc;
                                    }

                                }

                            }

                        }
                        catch (WebException ex)
                        {

                        }
                    }
                    catch (Exception ex)
                    {
                    }
                    string U_NNHG = "";
                    string U_BSX = "";
                    string U_CMND = "";
                    string U_Description_vn = "";
                    if (doc.Address !=null)
                    {
                        var add = doc.Address.FirstOrDefault(e => e.Type == "S");
                        if(add != null)
                        {
                            U_NNHG = add.Person ?? "";
                            U_BSX = add.VehiclePlate ?? "";
                            U_CMND = add.CCCD ?? "";
                            U_Description_vn = add.Address ?? "";
                        }    
                    }    
                    var order1 = new SapOrder
                    {
                        CardCode = doc.BP.CardCode,
                        DocDate = DateTime.Now.ToString("yyyy-MM-dd"),
                        DocDueDate = DateTime.Now.ToString("yyyy-MM-dd"),
                        DocObjectCode = "17",
                        DocCurrency = doc.Currency,
                        U_NVKD1 = "NBT2",
                        U_NVKD2 = "NBT2",
                        U_NVKD3 = "NBT2",
                        U_NVKD4 = "NBT2",
                        U_MDHPT = doc.InvoiceCode,
                        U_Area = sl.U_Khuvuc,
                        U_CoAdd = sl.U_DiachiHD,
                        U_CoTaxNo = sl.FederalTaxID,
                        U_NNHG = U_NNHG,
                        U_BSX = U_BSX,
                        U_CMND = U_CMND,
                        U_Description_vn = U_Description_vn
                    };
                    var timeTC = doc.BP.CRD3?.FirstOrDefault(p => p.PaymentMethodID == 2)?.Times ?? 0;
                    var timeBL = doc.BP.CRD3?.FirstOrDefault(p => p.PaymentMethodID == 3)?.Times ?? 0;
                    var order2 = new SapOrder
                    {
                        CardCode = doc.BP.CardCode,
                        DocDate = DateTime.Now.ToString("yyyy-MM-dd"),
                        DocDueDate = DateTime.Now.AddDays(timeTC).ToString("yyyy-MM-dd"),
                        DocObjectCode = "17",
                        DocCurrency = doc.Currency,
                        U_NVKD1 = "NBT2",
                        U_NVKD2 = "NBT2",
                        U_NVKD3 = "NBT2",
                        U_NVKD4 = "NBT2",
                        U_MDHPT = doc.InvoiceCode,
                        U_Area = sl.U_Khuvuc,
                        U_CoAdd = sl.U_DiachiHD,
                        U_CoTaxNo = sl.FederalTaxID,
                        U_NNHG = U_NNHG,
                        U_BSX = U_BSX,
                        U_CMND = U_CMND,
                        U_Description_vn = U_Description_vn
                    };
                    var order3 = new SapOrder
                    {
                        CardCode = doc.BP.CardCode,
                        DocDate = DateTime.Now.ToString("yyyy-MM-dd"),
                        DocDueDate = DateTime.Now.AddDays(timeTC).ToString("yyyy-MM-dd"),
                        DocObjectCode = "17",
                        DocCurrency = doc.Currency,
                        U_NVKD1 = "NBT2",
                        U_NVKD2 = "NBT2",
                        U_NVKD3 = "NBT2",
                        U_NVKD4 = "NBT2",
                        U_MDHPT = doc.InvoiceCode,
                        U_Area = sl.U_Khuvuc,
                        U_CoAdd = sl.U_DiachiHD,
                        U_CoTaxNo = sl.FederalTaxID,
                        U_NNHG = U_NNHG,
                        U_BSX = U_BSX,
                        U_CMND = U_CMND,
                        U_Description_vn = U_Description_vn
                    };
                    var itemKM = _context.Item.ToList();
                    if (line1.Count > 0)
                    {
                        var ids1 = line1.Select(x => x.LineId).ToList();

                        var pp = doc.Promotion.Where(p => ids1.Contains(p.LineId) && !p.ItemCode.IsNullOrEmpty() && (p.QuantityAdd ?? 0) > 0).ToList();
                        pp.ForEach(item =>
                        {
                            var price = itemKM.FirstOrDefault(e => e.ItemCode == item.ItemCode)?.Price ?? 0;
                            item.Price = price;
                        });
                        line1.AddRange(pp.Select(x => new SapOrderLine()
                        {
                            Quantity = x.QuantityAdd ?? 0,
                            ItemCode = x.ItemCode,
                            LineTotal = 0,
                            UnitPrice = x.Price,
                            VatGroup = "SVN4",
                            U_LHH = "02",
                            U_CTKM = x.PromotionCode
                        }));
                        if ((doc.BonusAmount ?? 0) != 0)
                            order1.Comments = "Thưởng thanh toán ngay " + doc.Bonus + "%: " + doc.BonusAmount;
                        order1.DocumentLines = line1;
                        order1.U_HTTT1 = "1";

                        try
                        {
                            //cookiess = await EnsureLoginAsync(cookiess, url, data);
                            if (cookiess == null || cookiess.SessionTime < DateTime.Now)
                            {

                                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url + "Login");
                                httpWebRequest.ContentType = "application/json";
                                httpWebRequest.Method = "POST";
                                httpWebRequest.KeepAlive = true;
                                httpWebRequest.ServerCertificateValidationCallback += BackEndAPI.Service.Sync.Security.TlsBypass.AcceptSelfSignedInDev();
                                httpWebRequest.Headers.Add("B1S-WCFCompatible", "true");
                                httpWebRequest.Headers.Add("B1S-MetadataWithoutSession", "true");
                                httpWebRequest.Accept = "*/*";
                                httpWebRequest.ServicePoint.Expect100Continue = false;
                                httpWebRequest.Headers.Add("Accept-Encoding", "gzip, deflate, br");
                                httpWebRequest.AutomaticDecompression = DecompressionMethods.GZip;

                                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                                { streamWriter.Write(data); }
                                try
                                {
                                    var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                                    var cookie = httpResponse.Headers.GetValues("Set-Cookie");
                                    cookiess = new Cookies();
                                    int endIndex = cookie[1].ToString().IndexOf(";");
                                    string routeid = cookie[1].ToString().Substring(0, endIndex);
                                    endIndex = cookie[0].ToString().IndexOf(";");
                                    string sesion = cookie[0].ToString().Substring(0, endIndex + 1);
                                    cookiess.ROUTEID = routeid;
                                    cookiess.B1SESSION = sesion;
                                    cookiess.SessionTime = DateTime.Now.AddMinutes(20);
                                }
                                catch (WebException ex)
                                {
                                }
                            }
                            HttpWebRequest httpWebRequests = (HttpWebRequest)WebRequest.Create(url + "Orders");
                            httpWebRequests.ContentType = "application/json";
                            httpWebRequests.Method = "POST";
                            httpWebRequests.KeepAlive = true;
                            httpWebRequests.ServerCertificateValidationCallback += BackEndAPI.Service.Sync.Security.TlsBypass.AcceptSelfSignedInDev();
                            httpWebRequests.Headers.Add("B1S-WCFCompatible", "true");
                            httpWebRequests.Headers.Add("B1S-MetadataWithoutSession", "true");
                            httpWebRequests.Accept = "*/*";
                            httpWebRequests.ServicePoint.Expect100Continue = false;
                            httpWebRequests.Headers.Add("Accept-Encoding", "gzip, deflate, br");
                            httpWebRequests.Headers.Add("Cookie", cookiess.B1SESSION + cookiess.ROUTEID);
                            httpWebRequests.AutomaticDecompression = DecompressionMethods.GZip;
                            var json = JsonConvert.SerializeObject(order1);
                            //var content = new StringContent(json, Encoding.UTF8, "application/json");
                            using (var streamWriter = new StreamWriter(httpWebRequests.GetRequestStream()))

                            { streamWriter.Write(json); }
                            try
                            {
                                var httpResponse = (HttpWebResponse)httpWebRequests.GetResponse();
                                doc.IsSync = true;
                                var listI = line1.Select(x => x.LineId).ToList();
                                doc.ItemDetail.Where(p => listI.Contains(p.LineId)).ToList().ForEach(p => { p.IsSync = true; });

                            }
                            catch (WebException ex)
                            {
                                var resp = ex.Response as HttpWebResponse;
                                if (resp?.StatusCode == HttpStatusCode.NotFound)
                                {
                                    var draft = resp?.Headers["Location"]?.ToString();
                                    if (!draft.IsNullOrEmpty())
                                    {
                                        var match = Regex.Match(draft ?? "", @"\((\d+)\)");
                                        var draftId = match.Success ? int.Parse(match.Groups[1].Value) : 0;
                                        HttpWebRequest httpWebRequestsdraft1 = (HttpWebRequest)WebRequest.Create(url + "Drafts?$filter=DocEntry eq "+ draftId + " and U_MDHPT eq '"+doc.InvoiceCode+ "'&$select = DocEntry ,U_MDHPT");
                                        httpWebRequestsdraft1.ContentType = "application/json";
                                        httpWebRequestsdraft1.Method = "GET";
                                        httpWebRequestsdraft1.KeepAlive = true;
                                        httpWebRequestsdraft1.ServerCertificateValidationCallback += BackEndAPI.Service.Sync.Security.TlsBypass.AcceptSelfSignedInDev();
                                        httpWebRequestsdraft1.Headers.Add("B1S-WCFCompatible", "true");
                                        httpWebRequestsdraft1.Headers.Add("B1S-MetadataWithoutSession", "true");
                                        httpWebRequestsdraft1.Accept = "*/*";
                                        httpWebRequestsdraft1.ServicePoint.Expect100Continue = false;
                                        httpWebRequestsdraft1.Headers.Add("Accept-Encoding", "gzip, deflate, br");
                                        httpWebRequestsdraft1.Headers.Add("Cookie", cookiess.B1SESSION + cookiess.ROUTEID);
                                        httpWebRequestsdraft1.AutomaticDecompression = DecompressionMethods.GZip;
                                        try
                                        {
                                            var httpResponse = (HttpWebResponse)httpWebRequestsdraft1.GetResponse();
                                            if (httpResponse.StatusCode == HttpStatusCode.OK)
                                            {
                                                using (var reader = new StreamReader(httpResponse.GetResponseStream()))
                                                {
                                                    var responseText = reader.ReadToEnd();


                                                    var dataxxx = JsonConvert.DeserializeObject<DraftResponse>(responseText);

                                                    if (dataxxx?.Value?.Count > 0)
                                                    {
                                                        var bp = dataxxx.Value[0];
                                                        if(doc.InvoiceCode == bp.U_MDHPT)
                                                        {
                                                            doc.IsSync = true;
                                                            var listI = line1.Select(x => x.LineId).ToList();
                                                            doc.ItemDetail.Where(p => listI.Contains(p.LineId)).ToList().ForEach(p => { p.IsSync = true; p.DraftId = draftId.ToString(); });
                                                        }    
                                                    }

                                                }

                                            }

                                        }
                                        catch (WebException exx)
                                        {

                                        }
                                        
                                    }
                                    else
                                    {
                                        var listI = line1.Select(x => x.LineId).ToList();
                                        string responseContent = string.Empty;
                                        using (var stream = resp.GetResponseStream())
                                        using (var reader = new StreamReader(stream))
                                        {
                                            responseContent = reader.ReadToEnd();
                                        }
                                        try
                                        {
                                            var jsonDoc = JsonDocument.Parse(responseContent);
                                            var root = jsonDoc.RootElement;
                                            doc.IsSync = false;
                                            if (root.TryGetProperty("error", out var error))
                                            {
                                                var code = error.GetProperty("code").GetInt32();
                                                var message = error.GetProperty("message").GetProperty("value").GetString();
                                                doc.ItemDetail.Where(p => listI.Contains(p.LineId)).ToList().ForEach(p => { p.Error = message; });
                                            }
                                            else
                                            {
                                                doc.ItemDetail.Where(p => listI.Contains(p.LineId)).ToList().ForEach(p => { p.Error = "Không thể phân tích phản hồi lỗi từ SAP."; });
                                            }
                                        }
                                        catch
                                        {
                                            doc.ItemDetail.Where(p => listI.Contains(p.LineId)).ToList().ForEach(p => { p.Error = "Không thể đọc phản hồi lỗi (body không phải JSON)."; });
                                        }
                                    }
                                }

                            }


                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }

                    if (line2.Count > 0)
                    {
                        var ids1 = line2.Select(x => x.LineId).ToList();
                        var pp = doc.Promotion.Where(p => ids1.Contains(p.LineId) && !p.ItemCode.IsNullOrEmpty() && (p.QuantityAdd ?? 0) > 0).ToList();
                        pp.ForEach(item =>
                        {
                            var price = itemKM.FirstOrDefault(e => e.ItemCode == item.ItemCode)?.Price ?? 0;
                            item.Price = price;
                        });
                        line2.AddRange(pp.Select(x => new SapOrderLine()
                        {
                            Quantity = x.QuantityAdd ?? 0,
                            ItemCode = x.ItemCode,
                            LineTotal = 0,
                            UnitPrice = x.Price,
                            VatGroup = "SVN4",
                            U_LHH = "02",
                            U_CTKM = x.PromotionCode
                        }));
                        order2.DocumentLines = line2;
                        order2.U_HTTT1 = "2";
                        //cookiess = await EnsureLoginAsync(cookiess, url, data);
                        if (cookiess == null || cookiess.SessionTime < DateTime.Now)
                        {
                            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url + "Login");
                            httpWebRequest.ContentType = "application/json";
                            httpWebRequest.Method = "POST";
                            httpWebRequest.KeepAlive = true;
                            httpWebRequest.ServerCertificateValidationCallback += BackEndAPI.Service.Sync.Security.TlsBypass.AcceptSelfSignedInDev();
                            httpWebRequest.Headers.Add("B1S-WCFCompatible", "true");
                            httpWebRequest.Headers.Add("B1S-MetadataWithoutSession", "true");
                            httpWebRequest.Accept = "*/*";
                            httpWebRequest.ServicePoint.Expect100Continue = false;
                            httpWebRequest.Headers.Add("Accept-Encoding", "gzip, deflate, br");
                            httpWebRequest.AutomaticDecompression = DecompressionMethods.GZip;

                            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                            { streamWriter.Write(data); }
                            try
                            {
                                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                                var cookie = httpResponse.Headers.GetValues("Set-Cookie");
                                cookiess = new Cookies();
                                int endIndex = cookie[1].ToString().IndexOf(";");
                                string routeid = cookie[1].ToString().Substring(0, endIndex);
                                endIndex = cookie[0].ToString().IndexOf(";");
                                string sesion = cookie[0].ToString().Substring(0, endIndex + 1);
                                cookiess.ROUTEID = routeid;
                                cookiess.B1SESSION = sesion;
                                cookiess.SessionTime = DateTime.Now.AddMinutes(20);
                            }
                            catch (WebException ex)
                            {
                            }
                        }
                        HttpWebRequest httpWebRequests = (HttpWebRequest)WebRequest.Create(url + "Orders");
                        httpWebRequests.ContentType = "application/json";
                        httpWebRequests.Method = "POST";
                        httpWebRequests.KeepAlive = true;
                        httpWebRequests.ServerCertificateValidationCallback += BackEndAPI.Service.Sync.Security.TlsBypass.AcceptSelfSignedInDev();
                        httpWebRequests.Headers.Add("B1S-WCFCompatible", "true");
                        httpWebRequests.Headers.Add("B1S-MetadataWithoutSession", "true");
                        httpWebRequests.Accept = "*/*";
                        httpWebRequests.ServicePoint.Expect100Continue = false;
                        httpWebRequests.Headers.Add("Accept-Encoding", "gzip, deflate, br");
                        httpWebRequests.Headers.Add("Cookie", cookiess.B1SESSION + cookiess.ROUTEID);
                        httpWebRequests.AutomaticDecompression = DecompressionMethods.GZip;
                        var json = JsonConvert.SerializeObject(order2);
                        //var content = new StringContent(json, Encoding.UTF8, "application/json");
                        using (var streamWriter = new StreamWriter(httpWebRequests.GetRequestStream()))

                        { streamWriter.Write(json); }
                        try
                        {
                            var httpResponse = (HttpWebResponse)httpWebRequests.GetResponse();
                            doc.IsSync = true;
                            var listI = line2.Select(x => x.LineId).ToList();
                            doc.ItemDetail.Where(p => listI.Contains(p.LineId)).ToList().ForEach(p => { p.IsSync = true; });
                        }
                        catch (WebException ex)
                        {
                            var resp = (HttpWebResponse)ex.Response;
                            if (resp?.StatusCode == HttpStatusCode.NotFound)
                            {
                                var draft = resp?.Headers["Location"]?.ToString();
                                if (!draft.IsNullOrEmpty())
                                {
                                    var match = Regex.Match(draft ?? "", @"\((\d+)\)");
                                    var draftId = match.Success ? int.Parse(match.Groups[1].Value) : 0;
                                    HttpWebRequest httpWebRequestsdraft2 = (HttpWebRequest)WebRequest.Create(url + "Drafts?$filter=DocEntry eq " + draftId + " and U_MDHPT eq '" + doc.InvoiceCode + "'&$select = DocEntry,U_MDHPT");
                                    httpWebRequestsdraft2.ContentType = "application/json";
                                    httpWebRequestsdraft2.Method = "GET";
                                    httpWebRequestsdraft2.KeepAlive = true;
                                    httpWebRequestsdraft2.ServerCertificateValidationCallback += BackEndAPI.Service.Sync.Security.TlsBypass.AcceptSelfSignedInDev();
                                    httpWebRequestsdraft2.Headers.Add("B1S-WCFCompatible", "true");
                                    httpWebRequestsdraft2.Headers.Add("B1S-MetadataWithoutSession", "true");
                                    httpWebRequestsdraft2.Accept = "*/*";
                                    httpWebRequestsdraft2.ServicePoint.Expect100Continue = false;
                                    httpWebRequestsdraft2.Headers.Add("Accept-Encoding", "gzip, deflate, br");
                                    httpWebRequestsdraft2.Headers.Add("Cookie", cookiess.B1SESSION + cookiess.ROUTEID);
                                    httpWebRequestsdraft2.AutomaticDecompression = DecompressionMethods.GZip;
                                    try
                                    {
                                        var httpResponse = (HttpWebResponse)httpWebRequestsdraft2.GetResponse();
                                        if (httpResponse.StatusCode == HttpStatusCode.OK)
                                        {
                                            using (var reader = new StreamReader(httpResponse.GetResponseStream()))
                                            {
                                                var responseText = reader.ReadToEnd();


                                                var dataxxx = JsonConvert.DeserializeObject<DraftResponse>(responseText);

                                                if (dataxxx?.Value?.Count > 0)
                                                {
                                                    var bp = dataxxx.Value[0];
                                                    if (doc.InvoiceCode == bp.U_MDHPT)
                                                    {
                                                        doc.IsSync = true;
                                                        var listI = line1.Select(x => x.LineId).ToList();
                                                        doc.ItemDetail.Where(p => listI.Contains(p.LineId)).ToList().ForEach(p => { p.IsSync = true; p.DraftId = draftId.ToString(); });
                                                    }
                                                }

                                            }

                                        }

                                    }
                                    catch (WebException exx)
                                    {

                                    }
                                }
                                else
                                {
                                    var listI = line1.Select(x => x.LineId).ToList();
                                    string responseContent = string.Empty;
                                    using (var stream = resp.GetResponseStream())
                                    using (var reader = new StreamReader(stream))
                                    {
                                        responseContent = reader.ReadToEnd();
                                    }
                                    try
                                    {
                                        var jsonDoc = JsonDocument.Parse(responseContent);
                                        var root = jsonDoc.RootElement;
                                        doc.IsSync = false;
                                        if (root.TryGetProperty("error", out var error))
                                        {
                                            var code = error.GetProperty("code").GetInt32();
                                            var message = error.GetProperty("message").GetProperty("value").GetString();
                                            doc.ItemDetail.Where(p => listI.Contains(p.LineId)).ToList().ForEach(p => { p.Error = message; });
                                        }
                                        else
                                        {
                                            doc.ItemDetail.Where(p => listI.Contains(p.LineId)).ToList().ForEach(p => { p.Error = "Không thể phân tích phản hồi lỗi từ SAP."; });
                                        }
                                    }
                                    catch
                                    {
                                        doc.ItemDetail.Where(p => listI.Contains(p.LineId)).ToList().ForEach(p => { p.Error = "Không thể đọc phản hồi lỗi (body không phải JSON)."; });
                                    }
                                }
                            }
                        }
                    }

                    if (line3.Count > 0)
                    {
                        var ids1 = line3.Select(x => x.LineId).ToList();
                        var pp = doc.Promotion.Where(p => ids1.Contains(p.LineId) && !p.ItemCode.IsNullOrEmpty() && (p.QuantityAdd ?? 0) > 0).ToList();
                        pp.ForEach(item =>
                        {
                            var price = itemKM.FirstOrDefault(e => e.ItemCode == item.ItemCode)?.Price ?? 0;
                            item.Price = price;
                        });
                        line3.AddRange(pp.Select(x => new SapOrderLine()
                        {
                            Quantity = x.QuantityAdd ?? 0,
                            ItemCode = x.ItemCode,
                            LineTotal = 0,
                            UnitPrice = x.Price,
                            VatGroup = "SVN4",
                            U_LHH = "02",
                            U_CTKM = x.PromotionCode
                        }));
                        order3.DocumentLines = line3;
                        order3.U_HTTT1 = "3";
                        //cookiess = await EnsureLoginAsync(cookiess, url, data);
                        if (cookiess == null || cookiess.SessionTime < DateTime.Now)
                        {
                            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url + "Login");
                            httpWebRequest.ContentType = "application/json";
                            httpWebRequest.Method = "POST";
                            httpWebRequest.KeepAlive = true;
                            httpWebRequest.ServerCertificateValidationCallback += BackEndAPI.Service.Sync.Security.TlsBypass.AcceptSelfSignedInDev();
                            httpWebRequest.Headers.Add("B1S-WCFCompatible", "true");
                            httpWebRequest.Headers.Add("B1S-MetadataWithoutSession", "true");
                            httpWebRequest.Accept = "*/*";
                            httpWebRequest.ServicePoint.Expect100Continue = false;
                            httpWebRequest.Headers.Add("Accept-Encoding", "gzip, deflate, br");
                            httpWebRequest.AutomaticDecompression = DecompressionMethods.GZip;

                            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                            { streamWriter.Write(data); }
                            try
                            {
                                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                                var cookie = httpResponse.Headers.GetValues("Set-Cookie");
                                cookiess = new Cookies();
                                int endIndex = cookie[1].ToString().IndexOf(";");
                                string routeid = cookie[1].ToString().Substring(0, endIndex);
                                endIndex = cookie[0].ToString().IndexOf(";");
                                string sesion = cookie[0].ToString().Substring(0, endIndex + 1);
                                cookiess.ROUTEID = routeid;
                                cookiess.B1SESSION = sesion;
                                cookiess.SessionTime = DateTime.Now.AddMinutes(20);
                            }
                            catch (WebException ex)
                            {
                            }
                        }
                        HttpWebRequest httpWebRequests = (HttpWebRequest)WebRequest.Create(url + "Orders");
                        httpWebRequests.ContentType = "application/json";
                        httpWebRequests.Method = "POST";
                        httpWebRequests.KeepAlive = true;
                        httpWebRequests.ServerCertificateValidationCallback += BackEndAPI.Service.Sync.Security.TlsBypass.AcceptSelfSignedInDev();
                        httpWebRequests.Headers.Add("B1S-WCFCompatible", "true");
                        httpWebRequests.Headers.Add("B1S-MetadataWithoutSession", "true");
                        httpWebRequests.Accept = "*/*";
                        httpWebRequests.ServicePoint.Expect100Continue = false;
                        httpWebRequests.Headers.Add("Accept-Encoding", "gzip, deflate, br");
                        httpWebRequests.Headers.Add("Cookie", cookiess.B1SESSION + cookiess.ROUTEID);
                        httpWebRequests.AutomaticDecompression = DecompressionMethods.GZip;
                        var json = JsonConvert.SerializeObject(order3);
                        //var content = new StringContent(json, Encoding.UTF8, "application/json");
                        using (var streamWriter = new StreamWriter(httpWebRequests.GetRequestStream()))

                        { streamWriter.Write(json); }
                        try
                        {
                            var httpResponse = (HttpWebResponse)httpWebRequests.GetResponse();
                            doc.IsSync = true;
                            var listI = line3.Select(x => x.LineId).ToList();
                            doc.ItemDetail.Where(p => listI.Contains(p.LineId)).ToList().ForEach(p => { p.IsSync = true; });
                        }
                        catch (WebException ex)
                        {
                            var resp = (HttpWebResponse)ex.Response;
                            if (resp?.StatusCode == HttpStatusCode.NotFound)
                            {
                                var draft = resp?.Headers["Location"]?.ToString();
                                if (!draft.IsNullOrEmpty())
                                {
                                    var match = Regex.Match(draft ?? "", @"\((\d+)\)");
                                    var draftId = match.Success ? int.Parse(match.Groups[1].Value) : 0;
                                    HttpWebRequest httpWebRequestsdraft3 = (HttpWebRequest)WebRequest.Create(url + "Drafts?$filter=DocEntry eq " + draftId + " and U_MDHPT eq '" + doc.InvoiceCode + "'&$select = DocEntry,U_MDHPT");
                                    httpWebRequestsdraft3.ContentType = "application/json";
                                    httpWebRequestsdraft3.Method = "GET";
                                    httpWebRequestsdraft3.KeepAlive = true;
                                    httpWebRequestsdraft3.ServerCertificateValidationCallback += BackEndAPI.Service.Sync.Security.TlsBypass.AcceptSelfSignedInDev();
                                    httpWebRequestsdraft3.Headers.Add("B1S-WCFCompatible", "true");
                                    httpWebRequestsdraft3.Headers.Add("B1S-MetadataWithoutSession", "true");
                                    httpWebRequestsdraft3.Accept = "*/*";
                                    httpWebRequestsdraft3.ServicePoint.Expect100Continue = false;
                                    httpWebRequestsdraft3.Headers.Add("Accept-Encoding", "gzip, deflate, br");
                                    httpWebRequestsdraft3.Headers.Add("Cookie", cookiess.B1SESSION + cookiess.ROUTEID);
                                    httpWebRequestsdraft3.AutomaticDecompression = DecompressionMethods.GZip;
                                    try
                                    {
                                        var httpResponse = (HttpWebResponse)httpWebRequestsdraft3.GetResponse();
                                        if (httpResponse.StatusCode == HttpStatusCode.OK)
                                        {
                                            using (var reader = new StreamReader(httpResponse.GetResponseStream()))
                                            {
                                                var responseText = reader.ReadToEnd();


                                                var dataxxx = JsonConvert.DeserializeObject<DraftResponse>(responseText);

                                                if (dataxxx?.Value?.Count > 0)
                                                {
                                                    var bp = dataxxx.Value[0];
                                                    if (doc.InvoiceCode == bp.U_MDHPT)
                                                    {
                                                        doc.IsSync = true;
                                                        var listI = line1.Select(x => x.LineId).ToList();
                                                        doc.ItemDetail.Where(p => listI.Contains(p.LineId)).ToList().ForEach(p => { p.IsSync = true; p.DraftId = draftId.ToString(); });
                                                    }
                                                }

                                            }

                                        }

                                    }
                                    catch (WebException exx)
                                    {

                                    }
                                    //doc.IsSync = true;
                                    //var listI = line1.Select(x => x.LineId).ToList();
                                    //doc.ItemDetail.Where(p => listI.Contains(p.LineId)).ToList().ForEach(p => { p.IsSync = true; p.DraftId = draftId.ToString(); });
                                }
                                else
                                {
                                    var listI = line1.Select(x => x.LineId).ToList();
                                    string responseContent = string.Empty;
                                    using (var stream = resp.GetResponseStream())
                                    using (var reader = new StreamReader(stream))
                                    {
                                        responseContent = reader.ReadToEnd();
                                    }
                                    try
                                    {
                                        var jsonDoc = JsonDocument.Parse(responseContent);
                                        var root = jsonDoc.RootElement;
                                        doc.IsSync = false;
                                        if (root.TryGetProperty("error", out var error))
                                        {
                                            var code = error.GetProperty("code").GetInt32();
                                            var message = error.GetProperty("message").GetProperty("value").GetString();
                                            doc.ItemDetail.Where(p => listI.Contains(p.LineId)).ToList().ForEach(p => { p.Error = message; });
                                        }
                                        else
                                        {
                                            doc.ItemDetail.Where(p => listI.Contains(p.LineId)).ToList().ForEach(p => { p.Error = "Không thể phân tích phản hồi lỗi từ SAP."; });
                                        }
                                    }
                                    catch
                                    {
                                        doc.ItemDetail.Where(p => listI.Contains(p.LineId)).ToList().ForEach(p => { p.Error = "Không thể đọc phản hồi lỗi (body không phải JSON)."; });
                                    }
                                }
                            }
                        }
                    }
                    _context.ODOC.Update(doc);
                    await _context.SaveChangesAsync();
                }
                return true;
            }
            catch
            {
                return true;
            }
        }

        public async Task<bool> SyncToSapDraftAsync()
        {
            try
            {
                string Db = EncryptDecrypt.DecryptString(_configuration["SAPServiceLayer:CompanyDB"]);
                string Username = EncryptDecrypt.DecryptString(_configuration["SAPServiceLayer:Username"]);
                string Password = EncryptDecrypt.DecryptString(_configuration["SAPServiceLayer:Password"]);
                Cookies cookiess = null; ;
                var l = new LoginInfor
                {
                    CompanyDB = Db,
                    UserName = Username,
                    Password = Password
                };
                string url = _configuration["SAPServiceLayer:Host"];
                string data = JsonConvert.SerializeObject(l);
                var doc = _context.ODOC
                .Include(x => x.ItemDetail)
                .FirstOrDefault(x => x.Status == "DXN" && x.ObjType == 22 && x.IsCheck == false && x.IsSync == true);
                if (doc != null)
                {
                    if (cookiess == null || cookiess.SessionTime < DateTime.Now)
                    {

                        HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url + "Login");
                        httpWebRequest.ContentType = "application/json";
                        httpWebRequest.Method = "POST";
                        httpWebRequest.KeepAlive = true;
                        httpWebRequest.ServerCertificateValidationCallback += BackEndAPI.Service.Sync.Security.TlsBypass.AcceptSelfSignedInDev();
                        httpWebRequest.Headers.Add("B1S-WCFCompatible", "true");
                        httpWebRequest.Headers.Add("B1S-MetadataWithoutSession", "true");
                        httpWebRequest.Accept = "*/*";
                        httpWebRequest.ServicePoint.Expect100Continue = false;
                        httpWebRequest.Headers.Add("Accept-Encoding", "gzip, deflate, br");
                        httpWebRequest.AutomaticDecompression = DecompressionMethods.GZip;

                        using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                        { streamWriter.Write(data); }
                        try
                        {
                            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                            var cookie = httpResponse.Headers.GetValues("Set-Cookie");
                            cookiess = new Cookies();
                            int endIndex = cookie[1].ToString().IndexOf(";");
                            string routeid = cookie[1].ToString().Substring(0, endIndex);
                            endIndex = cookie[0].ToString().IndexOf(";");
                            string sesion = cookie[0].ToString().Substring(0, endIndex + 1);
                            cookiess.ROUTEID = routeid;
                            cookiess.B1SESSION = sesion;
                            cookiess.SessionTime = DateTime.Now.AddMinutes(20);
                        }
                        catch (WebException ex)
                        {
                        }
                    }
                    HttpWebRequest httpWebRequestsdraft = (HttpWebRequest)WebRequest.Create(url + "Drafts?$filter=U_MDHPT eq '"+doc.InvoiceCode+"'&$select = DocEntry,U_MDHPT");
                    httpWebRequestsdraft.ContentType = "application/json";
                    httpWebRequestsdraft.Method = "GET";
                    httpWebRequestsdraft.KeepAlive = true;
                    httpWebRequestsdraft.ServerCertificateValidationCallback += BackEndAPI.Service.Sync.Security.TlsBypass.AcceptSelfSignedInDev();
                    httpWebRequestsdraft.Headers.Add("B1S-WCFCompatible", "true");
                    httpWebRequestsdraft.Headers.Add("B1S-MetadataWithoutSession", "true");
                    httpWebRequestsdraft.Accept = "*/*";
                    httpWebRequestsdraft.ServicePoint.Expect100Continue = false;
                    httpWebRequestsdraft.Headers.Add("Accept-Encoding", "gzip, deflate, br");
                    httpWebRequestsdraft.Headers.Add("Cookie", cookiess.B1SESSION + cookiess.ROUTEID);
                    httpWebRequestsdraft.AutomaticDecompression = DecompressionMethods.GZip;
                    try
                    {
                        var httpResponse = (HttpWebResponse)httpWebRequestsdraft.GetResponse();
                        if (httpResponse.StatusCode == HttpStatusCode.OK)
                        {
                            using (var reader = new StreamReader(httpResponse.GetResponseStream()))
                            {
                                var responseText = reader.ReadToEnd();


                                var dataxxx = JsonConvert.DeserializeObject<DraftResponse>(responseText);

                                if (dataxxx?.Value?.Count == 0)
                                {
                                    doc.IsSync = false;
                                    doc.IsCheck = false;
                                    doc.ItemDetail.ForEach(item =>
                                    {
                                        item.IsSync = false;
                                        item.DraftId = null;
                                    });
                                    _context.ODOC.Update(doc);

                                    await _context.SaveChangesAsync();
                                    return true;
                                }
                                else
                                {
                                    var bp = dataxxx.Value[0];
                                    if (doc.InvoiceCode == bp.U_MDHPT && (doc.ItemDetail?.FirstOrDefault()?.DraftId ?? "" )== bp.DocEntry.ToString())
                                    {
                                        doc.IsCheck = true;
                                        _context.ODOC.Update(doc);
                                        await _context.SaveChangesAsync();
                                        return true;
                                    }
                                    else
                                    {
                                        doc.IsSync = true;
                                        doc.IsCheck = true;
                                        doc.ItemDetail.ForEach(item =>
                                        {
                                            item.IsSync = true;
                                            item.DraftId = bp.DocEntry.ToString();
                                        });
                                        _context.ODOC.Update(doc);
                                        await _context.SaveChangesAsync();
                                        return true;
                                    }
                                }

                            }

                        }

                    }
                    catch (WebException exx)
                    {
                        return true;
                    }
                    
                }
                return true;
            }
            catch
            {
                return true;
            }
        }
        public async Task<bool> SyncVPKMToSapAsync()
        {
            try
            {
                string Db = EncryptDecrypt.DecryptString(_configuration["SAPServiceLayer:CompanyDB"]);
                string Username = EncryptDecrypt.DecryptString(_configuration["SAPServiceLayer:Username"]);
                string Password = EncryptDecrypt.DecryptString(_configuration["SAPServiceLayer:Password"]);
                Cookies cookiess = null; ;
                var l = new LoginInfor
                {
                    CompanyDB = Db,
                    UserName = Username,
                    Password = Password
                };
                string url = _configuration["SAPServiceLayer:Host"];
                string data = JsonConvert.SerializeObject(l);
                var doc = _context.ODOC
                .Include(x => x.ItemDetail.Where(p => p.IsSync == false))
                .Include(x => x.Promotion)
                .Include(x => x.BP)
                .ThenInclude(x => x.CRD3)
                .Include(x => x.PaymentInfo)
                .FirstOrDefault(x => x.Status == "DXN" && x.ObjType == 12 && x.IsSync == false);
                if (doc != null)
                { 
                    SapOrderBP sl = new SapOrderBP();
                    try
                    {
                        if (cookiess == null || cookiess.SessionTime < DateTime.Now)
                        {

                            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url + "Login");
                            httpWebRequest.ContentType = "application/json";
                            httpWebRequest.Method = "POST";
                            httpWebRequest.KeepAlive = true;
                            httpWebRequest.ServerCertificateValidationCallback += BackEndAPI.Service.Sync.Security.TlsBypass.AcceptSelfSignedInDev();
                            httpWebRequest.Headers.Add("B1S-WCFCompatible", "true");
                            httpWebRequest.Headers.Add("B1S-MetadataWithoutSession", "true");
                            httpWebRequest.Accept = "*/*";
                            httpWebRequest.ServicePoint.Expect100Continue = false;
                            httpWebRequest.Headers.Add("Accept-Encoding", "gzip, deflate, br");
                            httpWebRequest.AutomaticDecompression = DecompressionMethods.GZip;

                            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                            { streamWriter.Write(data); }
                            try
                            {
                                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                                var cookie = httpResponse.Headers.GetValues("Set-Cookie");
                                cookiess = new Cookies();
                                int endIndex = cookie[1].ToString().IndexOf(";");
                                string routeid = cookie[1].ToString().Substring(0, endIndex);
                                endIndex = cookie[0].ToString().IndexOf(";");
                                string sesion = cookie[0].ToString().Substring(0, endIndex + 1);
                                cookiess.ROUTEID = routeid;
                                cookiess.B1SESSION = sesion;
                                cookiess.SessionTime = DateTime.Now.AddMinutes(20);
                            }
                            catch (WebException ex)
                            {
                            }
                        }
                        HttpWebRequest httpWebRequests = (HttpWebRequest)WebRequest.Create(url + "BusinessPartners?$select=CardCode,U_DiachiHD,FederalTaxID,U_Khuvuc&$filter=CardCode eq '" + doc.BP.CardCode + "'");
                        httpWebRequests.ContentType = "application/json";
                        httpWebRequests.Method = "GET";
                        httpWebRequests.KeepAlive = true;
                        httpWebRequests.ServerCertificateValidationCallback += BackEndAPI.Service.Sync.Security.TlsBypass.AcceptSelfSignedInDev();
                        httpWebRequests.Headers.Add("B1S-WCFCompatible", "true");
                        httpWebRequests.Headers.Add("B1S-MetadataWithoutSession", "true");
                        httpWebRequests.Accept = "*/*";
                        httpWebRequests.ServicePoint.Expect100Continue = false;
                        httpWebRequests.Headers.Add("Accept-Encoding", "gzip, deflate, br");
                        httpWebRequests.Headers.Add("Cookie", cookiess.B1SESSION + cookiess.ROUTEID);
                        httpWebRequests.AutomaticDecompression = DecompressionMethods.GZip;
                        try
                        {
                            var httpResponse = (HttpWebResponse)httpWebRequests.GetResponse();
                            if (httpResponse.StatusCode == HttpStatusCode.OK)
                            {
                                using (var reader = new StreamReader(httpResponse.GetResponseStream()))
                                {
                                    var responseText = reader.ReadToEnd();


                                    var dataxxx = JsonConvert.DeserializeObject<BPResponse>(responseText);

                                    if (dataxxx?.Value?.Count > 0)
                                    {
                                        var bp = dataxxx.Value[0];
                                        sl.CardCode = bp.CardCode;
                                        sl.FederalTaxID = bp.FederalTaxID;
                                        sl.U_DiachiHD = bp.U_DiachiHD;
                                        sl.U_Khuvuc = bp.U_Khuvuc;
                                    }

                                }

                            }

                        }
                        catch (WebException ex)
                        {

                        }
                    }
                    catch (Exception ex)
                    {
                    }
                    var order1 = new SapOrder
                    {
                        CardCode = doc.BP.CardCode,
                        DocDate = DateTime.Now.ToString("yyyy-MM-dd"),
                        DocDueDate = DateTime.Now.ToString("yyyy-MM-dd"),
                        DocObjectCode = "17",
                        DocCurrency = doc.Currency,
                        U_NVKD1 = "NBT2",
                        U_NVKD2 = "NBT2",
                        U_NVKD3 = "NBT2",
                        U_NVKD4 = "NBT2",
                        U_MDHPT = doc.InvoiceCode,
                        U_Area = sl.U_Khuvuc,
                        U_CoAdd = sl.U_DiachiHD,
                        U_CoTaxNo = sl.FederalTaxID,
                    };
                    var itemKM = _context.Item.Where(e => doc.ItemDetail.Select(e=>e.ItemCode).ToArray().Contains(e.ItemCode)).ToList();
                    var line1 = doc.ItemDetail.Where(x =>  x.Quantity > 0).Select(x =>
                    new SapOrderLine()
                    {
                        Quantity = x.Quantity,
                        ItemCode = x.ItemCode,
                        LineTotal = 0,
                        UnitPrice = itemKM.FirstOrDefault(e=>e.ItemCode ==x.ItemCode)?.Price ?? 0,
                        VatGroup = "SVN4",
                        LineId = x.LineId,
                        U_LHH = "01",
                    }).ToList();

                    
                    if (line1.Count > 0)
                    { 
                        order1.DocumentLines = line1; 

                        try
                        {
                            if (cookiess == null || cookiess.SessionTime < DateTime.Now)
                            {

                                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url + "Login");
                                httpWebRequest.ContentType = "application/json";
                                httpWebRequest.Method = "POST";
                                httpWebRequest.KeepAlive = true;
                                httpWebRequest.ServerCertificateValidationCallback += BackEndAPI.Service.Sync.Security.TlsBypass.AcceptSelfSignedInDev();
                                httpWebRequest.Headers.Add("B1S-WCFCompatible", "true");
                                httpWebRequest.Headers.Add("B1S-MetadataWithoutSession", "true");
                                httpWebRequest.Accept = "*/*";
                                httpWebRequest.ServicePoint.Expect100Continue = false;
                                httpWebRequest.Headers.Add("Accept-Encoding", "gzip, deflate, br");
                                httpWebRequest.AutomaticDecompression = DecompressionMethods.GZip;

                                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                                { streamWriter.Write(data); }
                                try
                                {
                                    var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                                    var cookie = httpResponse.Headers.GetValues("Set-Cookie");
                                    cookiess = new Cookies();
                                    int endIndex = cookie[1].ToString().IndexOf(";");
                                    string routeid = cookie[1].ToString().Substring(0, endIndex);
                                    endIndex = cookie[0].ToString().IndexOf(";");
                                    string sesion = cookie[0].ToString().Substring(0, endIndex + 1);
                                    cookiess.ROUTEID = routeid;
                                    cookiess.B1SESSION = sesion;
                                    cookiess.SessionTime = DateTime.Now.AddMinutes(20);
                                }
                                catch (WebException ex)
                                {
                                }
                            }
                            HttpWebRequest httpWebRequests = (HttpWebRequest)WebRequest.Create(url + "Orders");
                            httpWebRequests.ContentType = "application/json";
                            httpWebRequests.Method = "POST";
                            httpWebRequests.KeepAlive = true;
                            httpWebRequests.ServerCertificateValidationCallback += BackEndAPI.Service.Sync.Security.TlsBypass.AcceptSelfSignedInDev();
                            httpWebRequests.Headers.Add("B1S-WCFCompatible", "true");
                            httpWebRequests.Headers.Add("B1S-MetadataWithoutSession", "true");
                            httpWebRequests.Accept = "*/*";
                            httpWebRequests.ServicePoint.Expect100Continue = false;
                            httpWebRequests.Headers.Add("Accept-Encoding", "gzip, deflate, br");
                            httpWebRequests.Headers.Add("Cookie", cookiess.B1SESSION + cookiess.ROUTEID);
                            httpWebRequests.AutomaticDecompression = DecompressionMethods.GZip;
                            var json = JsonConvert.SerializeObject(order1);
                            //var content = new StringContent(json, Encoding.UTF8, "application/json");
                            using (var streamWriter = new StreamWriter(httpWebRequests.GetRequestStream()))

                            { streamWriter.Write(json); }
                            try
                            {
                                var httpResponse = (HttpWebResponse)httpWebRequests.GetResponse();
                                doc.IsSync = true;
                                var listI = line1.Select(x => x.LineId).ToList();
                                doc.ItemDetail.Where(p => listI.Contains(p.LineId)).ToList().ForEach(p => { p.IsSync = true; });
                            }
                            catch (WebException ex)
                            {
                                var resp = ex.Response as HttpWebResponse;
                                if (resp?.StatusCode == HttpStatusCode.NotFound)
                                {
                                    var draft = resp?.Headers["Location"]?.ToString();
                                    if (!draft.IsNullOrEmpty())
                                    {
                                        var match = Regex.Match(draft ?? "", @"\((\d+)\)");
                                        var draftId = match.Success ? int.Parse(match.Groups[1].Value) : 0;
                                        doc.IsSync = true;
                                        var listI = line1.Select(x => x.LineId).ToList();
                                        doc.ItemDetail.Where(p => listI.Contains(p.LineId)).ToList().ForEach(p => { p.IsSync = true; p.DraftId = draftId.ToString(); });
                                    }
                                    else
                                    {
                                        var listI = line1.Select(x => x.LineId).ToList();
                                        string responseContent = string.Empty;
                                        using (var stream = resp.GetResponseStream())
                                        using (var reader = new StreamReader(stream))
                                        {
                                            responseContent = reader.ReadToEnd();
                                        }
                                        try
                                        {
                                            var jsonDoc = JsonDocument.Parse(responseContent);
                                            var root = jsonDoc.RootElement;
                                            doc.IsSync = false;
                                            if (root.TryGetProperty("error", out var error))
                                            {
                                                var code = error.GetProperty("code").GetInt32();
                                                var message = error.GetProperty("message").GetProperty("value").GetString();
                                                doc.ItemDetail.Where(p => listI.Contains(p.LineId)).ToList().ForEach(p => { p.Error = message; });
                                            }
                                            else
                                            {
                                                doc.ItemDetail.Where(p => listI.Contains(p.LineId)).ToList().ForEach(p => { p.Error = "Không thể phân tích phản hồi lỗi từ SAP."; });
                                            }
                                        }
                                        catch
                                        {
                                            doc.ItemDetail.Where(p => listI.Contains(p.LineId)).ToList().ForEach(p => { p.Error = "Không thể đọc phản hồi lỗi (body không phải JSON)."; });
                                        }
                                    }
                                }

                            }


                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }
 
                    _context.ODOC.Update(doc);
                    await _context.SaveChangesAsync();
                }
                return true;
            }
            catch
            {
                return true;
            }
        }
        public async Task<(ODOC, Mess)> AddDocumentAsync(ODOC model, int ObjType)
        {
            Mess mess = new Mess();
            model.Status = "DXL";
            model.ObjType = ObjType;
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    string notiType = "";
                    string notiTitle = "";
                    var message = "";
                    var codes = "";
                    if (ObjType == 198)
                    {
                        notiType = "fc";
                        message = "Khách hàng {0} đã tạo một kế hoạch mua hàng mới";
                        notiTitle = "Kế hoạch mua hàng mới {0}";
                        codes = await GenerateDocument("FC", "", 10, model);
                    }
                    else if (ObjType == 1250000001)
                    {
                        message = "Khách hàng {0} đã tạo một yêu cầu lấy hàng mới";
                        notiType = "order_request";
                        notiTitle = "Yêu cầu lấy hàng mới {0}";
                        codes = await GenerateDocument("YCLH", "", 10, model);
                    }
                    else if (ObjType == 22)
                    {
                        notiType = "order";
                        notiTitle = "Đơn hàng mới {0}";
                        message = "Khách hàng {0} đã đặt một đơn hàng mới trên hệ thống";
                        codes = await GenerateDocument("DH", "", 10, model);
                    }
                    else if (ObjType == 12)
                    {
                        notiType = "exchange";
                        notiTitle = "Yêu cầu đổi vật phẩm khuyến mại {0}";
                        message = "Khách hàng {0} đã tạo một Yêu cầu đổi vật phẩm khuyến mại mới trên hệ thống";
                        codes = await GenerateDocument("YCVP", "", 10, model);
                    }
                    else if (ObjType == 16)
                    {

                        if ((model.RefInvoiceCode ?? "") != "")
                        {
                            var returnPO = await _context.ODOC.AsNoTracking().FirstOrDefaultAsync(b => b.InvoiceCode == model.RefInvoiceCode && b.Status == "DHT");
                            if(returnPO == null)
                            {
                                mess.Status = 400;
                                mess.Errors = "Đơn hàng không tồn tại, hoặc chưa hoàn thành";
                                transaction.Rollback();
                                return (null, mess);
                            }
                            var detail = model.ItemDetail.Where(e => (e.Type ?? "") == "" || e.Type == "I").ToList();
                            if (returnPO == null)
                            {
                                mess.Status = 400;
                                mess.Errors = "Không có mặt hàng trả lại";
                                transaction.Rollback();
                                return (null, mess);
                            }
                            foreach (var item in model.ItemDetail.Where(e=>(e.Type ?? "") == "" || e.Type == "I").ToList())
                            {
                                var exists = await _context.DOC1.AnyAsync(sd => sd.FatherId == item.BaseId && sd.Id == item.BaseLineId && sd.FatherId == returnPO.Id);
                                if (!exists)
                                {
                                    mess.Status = 400;
                                    mess.Errors = $"Mặt hàng không thuộc đơn hàng {model.RefInvoiceCode}";
                                    transaction.Rollback();
                                    return (null, mess);
                                }
                            }
                        }
                        notiType = "return";
                        notiTitle = "Yêu cầu trả lại hàng {0}";
                        message = "Khách hàng {0} đã tạo một Yêu cầu trả lại hàng mới trên hệ thống";
                        codes = await GenerateDocument("RTQ", "", 10, model);
                    }
                    if (model.UserId != null && model.DocType.IsNullOrEmpty())
                    {
                        var checkChangeDiscount =
                            await _privileService.UserHasPrivilegeAsync(model.UserId ?? 0, "Order.ChangeDiscount");
                        var (dis, _) = await _promotionService.GetPromotionAsync(new PromotionParam()
                        {
                            CardId = model.CardId.ToString(),
                            OrderDate = DateTime.Now,
                            PromotionParamLine = model.ItemDetail.Select(p => new PromotionParamLine()
                            {
                                ItemId = p.ItemId ?? 0,
                                PayMethod = p.PaymentMethodCode,
                                Quantity = p.Quantity,
                            }).ToList(),
                        });

                        if (dis != null)
                        {
                            foreach (var ss in dis?.PromotionOrderLine[0].PromotionOrderLineSub)
                            {
                                if (ss.Discount != model.ItemDetail.Where(v => v.ItemId == ss.ItemId)
                                        .Select(v => v.Discount).FirstOrDefault())
                                {
                                    if (!checkChangeDiscount)
                                    {
                                        await transaction.RollbackAsync();
                                        mess.Errors = "Bạn không có quyền sửa giảm giá";
                                        mess.Status = 403;
                                        return (null, mess);
                                    }
                                }
                            }
                        }
                    }
                    model.ItemDetail.ForEach(e => e.OpenQty = e.Quantity);
                    if (model.ItemDetail.Count == 0 || model.ItemDetail == null)
                    {
                        mess.Status = 400;
                        mess.Errors = "Chi tiết phiếu trống";
                        transaction.Rollback();
                        return (null, mess);
                    }
                    if(ObjType != 12 && ObjType != 16)
                    {
                        if (model.Address == null)
                        {
                            mess.Status = 400;
                            mess.Errors = "Địa chỉ trống";
                            transaction.Rollback();
                            return (null, mess);
                        }

                        if (model?.PaymentMethod?.Count == 0 || model?.PaymentMethod == null)
                        {
                            mess.Status = 400;
                            mess.Errors = "Phương thức thanh toán trống";
                            transaction.Rollback();
                            return (null, mess);
                        }
                    } 
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
                    
                    CommittedTracking b = null;
                    Committed a = null;
                    PaymentInfo paymentInfo = null;
                    if (ObjType == 22)
                    {
                        List<DOC1DTO> listItms = new List<DOC1DTO>();
                        foreach(var i in model.ItemDetail)
                        {
                            i.Id = 0;
                            DOC1DTO itms = new DOC1DTO();
                            _modelUpdater.UpdateModel(itms, i, "NA1", "NA1", "NA1", "NA3", "NA4", "NA5");
                            listItms.Add(itms);
                        }
                        List<DOC2DTO> listdoc2 = new List<DOC2DTO>();
                        foreach (var i in model.Promotion)
                        {
                            i.Id = 0;
                            DOC2DTO itms = new DOC2DTO();
                            _modelUpdater.UpdateModel(itms, i, "Id", "NA1", "NA1", "NA3", "NA4", "NA5");
                            listdoc2.Add(itms);
                        }
                        if(model.DocType.IsNullOrEmpty())
                        {
                            (paymentInfo, a, b) = await GetPaymentInfo1(new PriceDocCheck
                            {
                                CardId = bp.Id,

                                ItemDetail = listItms,
                                Promotion = listdoc2,
                            });
                        }
                        else
                        {
                            (paymentInfo, a, b) = await GetPaymentInfoNET(new PriceDocCheck
                            {
                                CardId = bp.Id,
                                PayNowAmount = model.BonusAmount ?? 0,
                                ItemDetail = listItms,
                                Promotion = listdoc2,
                            });
                        }
                        model.PaymentInfo = paymentInfo;

                        model.DocDate = DateTime.UtcNow;
                        model.DueDate = DateTime.UtcNow;

                        model.ItemDetail.ForEach(p => p.ItemVolume = _context
                            .Item
                            .Include(i => i.Packing)
                            .Where(z => z.Id == p.ItemId).Select(i => i.Packing!.Volumn).FirstOrDefault());

                        
                    }

                    var lineIdx = 0;
                    foreach (var item in model.ItemDetail)
                    {
                        item.LineId = lineIdx;
                        lineIdx++;
                    }
                    
                    model.ObjType = ObjType;
                    model.InvoiceCode = codes;
                    if (model.ItemDetail.Exists(e=>e.PaymentMethodCode != "PayNow") && model.ObjType == 22)
                    {
                        var itemdetail = model.ItemDetail.Where(e => e.PaymentMethodCode != "PayNow").Distinct().ToList();
                        foreach (var item in itemdetail.Select(e=>e.PaymentMethodCode).Distinct())
                        {
                            var pmmethod =await  _context.PaymentMethod.FirstOrDefaultAsync(e => e.PaymentMethodCode == item);
                            var crd4 = new Models.BusinessPartners.CRD4
                            {
                                InvoiceNumber = model.InvoiceCode,
                                InvoiceDate = model.DocDate,
                                DueDate = model.DueDate,
                                InvoiceTotal = itemdetail.Where(e => e.PaymentMethodCode == item).Sum(e=>e.Quantity*e.PriceAfterDist + e.VATAmount),
                                PaymentMethodID = pmmethod.Id,
                                PaymentMethodCode = pmmethod.PaymentMethodCode,
                                PaymentMethodName = pmmethod.PaymentMethodName,
                                AmountOverdue = model.Total,
                                BPId = model.CardId ?? 0,
                            };
                                var crd3 = await _context.CRD3.Where(p =>
                                        p.BPId == model.CardId &&
                                        p.PaymentMethodID == model.PaymentMethod[0].PaymentMethodID)
                                    .FirstOrDefaultAsync();
                                if (crd3 != null)
                                {
                                    crd3.Balance += itemdetail.Where(e => e.PaymentMethodCode == item).Sum(e => e.Quantity * e.PriceAfterDist + e.VATAmount) ?? 0;
                                }

                                _context.CRD4.Add(crd4);
                        }
                        
                    }
                    var checkUserApsp = await _context.Users.FirstOrDefaultAsync(e => e.Id == model.UserId);
                    
                    if (checkUserApsp != null && model.ObjType == 22)
                    {
                        model.Status = "CXN";
                        if (model.PaymentInfo != null)
                        {
                            var debts = await _partnerService.GetCRD3Async(model.CardId ?? 0);
                            var debt1 = debts.FirstOrDefault(e => e.PaymentMethodCode == "PayCredit");
                            var debt2 = debts.FirstOrDefault(e => e.PaymentMethodCode == "PayGuarantee");
                            
                            if (model.PaymentInfo.TotalDebt != 0 && ( debt1.BalanceLimit) > (debt1.Balance ?? 0) && ((debt1.Balance ?? 0) !=0) )
                            {
                                model.Status = "DXL";
                            }
                            if (model.PaymentInfo.TotalDebtGuarantee != 0 && ( debt2.BalanceLimit) > (debt2.Balance ?? 0) && ((debt2.Balance ?? 0) != 0))
                            {
                                model.Status = "DXL";
                            }
                            
                            if (model.PaymentInfo.TotalPayNow != 0 && checkUserApsp.UserType != "APSP")
                            {
                                model.Status = "TTN";
                            }
                            if (model.PaymentMethod.Where(e => e.PaymentMethodCode == "PayVisa").Count() == model.PaymentMethod.Count())
                            {
                                model.Status = "TTN";
                            }
                        }
                    }
                    if (checkUserApsp == null && model.ObjType == 22)
                    {
                        if (model.PaymentMethod.Where(e => e.PaymentMethodCode == "PayVisa").Count() == model.PaymentMethod.Count())
                        {
                            model.Status = "TTN";
                        }
                    }    
                    if (checkUserApsp != null && model.ObjType == 1250000001)
                    {
                        model.Status = "CXN";
                        var debts = await _partnerService.GetCRD3Async(model.CardId ?? 0);
                        var debt1 = debts.FirstOrDefault(e => e.PaymentMethodCode == "PayCredit");
                        var debt2 = debts.FirstOrDefault(e => e.PaymentMethodCode == "PayGuarantee");

                        if ((debt1.BalanceLimit) > debt1.Balance)
                        {
                            model.Status = "DXL";
                        }
                        if ((debt2.BalanceLimit) > debt2.Balance)
                        {
                            model.Status = "DXL";
                        }

                    }
                    if (checkUserApsp != null && model.ObjType == 12)
                    {
                        model.Status = "CXN";
                    }
                    if (checkUserApsp != null && model.ObjType == 16)
                    {
                        model.Status = "DXL";
                    }
                    _context.ODOC.Add(model);

                    await _context.SaveChangesAsync();
                    if (ObjType == 22)
                    {
                        if (model.DocType.IsNullOrEmpty())
                        {
                            CalculatorPoint calPoint = new CalculatorPoint();
                            calPoint.CardId = model.CardId ?? 0;
                            calPoint.DocId = model.Id;
                            calPoint.CalculatorPointLine = model.ItemDetail
                            .Select(e => new CalculatorPointLine
                            {
                                ItemId = e.ItemId ?? 0,
                                Quantity = e.Quantity
                            })
                            .ToList();
                            await _pointService.CalculatePoints(calPoint);
                        }    
                        await _systemLog.SaveWithTransAsync("INFO", "Create", $"Thêm mới đơn hàng {model.InvoiceCode}", "Order", model.Id);
                    }
                    else if (ObjType == 16)
                    {

                        var updateDetails = model.ItemDetail.Where(e => (e.Type ?? "") == "" || e.Type == "I").Select(e => new { e.BaseLineId, e.Quantity }).ToList();
                        var lineID = updateDetails.Select(e => e.BaseLineId).ToArray();
                        var docDetail = _context.DOC1.Where(e => lineID.Contains(e.Id));
                        foreach (var d in docDetail)
                        {
                            d.OpenQty = d.Quantity - updateDetails.FirstOrDefault(e => e.BaseLineId == d.Id)?.Quantity;
                        }
                        _context.DOC1.UpdateRange(docDetail);
                        await _context.SaveChangesAsync();
                        await _systemLog.SaveWithTransAsync("INFO", "Create", $"Thêm mới yêu cầu trả hàng {model.InvoiceCode}", "Return", model.Id);
                    }    
                    else if(ObjType == 12)
                    {
                        CalculatorPoint calPoint = new CalculatorPoint();
                        calPoint.CardId = model.CardId ?? 0;
                        calPoint.DocId = model.Id;
                        calPoint.CalculatorPointLine = model.ItemDetail
                        .Select(e => new CalculatorPointLine
                        {
                            ItemId = e.ItemId ?? 0,
                            Quantity = e.Quantity,
                            Point = e.Price
                        })
                        .ToList();
                        var messRedeem =await _pointService.RedeemPoints(calPoint,"CP");
                        if (messRedeem != null)
                        {
                            mess.Status = messRedeem.Status;
                            mess.Errors = messRedeem.Errors;
                            transaction.Rollback();
                            return (null, mess);
                        }
                        await _systemLog.SaveWithTransAsync("INFO", "Create", $"Thêm mới yêu cầu đổi vật phẩm {model.InvoiceCode}", "vpqd", model.Id);
                    }
                    else
                    {
                        await _systemLog.SaveWithTransAsync("INFO", "Create", $"Thêm mới yêu cầu lấy hàng gửi {model.InvoiceCode}", "Request", model.Id);
                    }
                    

                    if(b !=null)
                    {
                        List<DOC16> listDOC16 = new List<DOC16>();
                        var listID = _context.CommittedTrackingLine.Where(e => e.IsCommitted == true).Select(e => e.Id);
                        foreach (var sub in b.CommittedTrackingLine.ToList())
                        {
                            if(sub.DocId == 0)
                            {
                                sub.DocId = model.Id;
                            }  
                            if(sub.IsCommitted == true && !listID.Contains(sub.Id))
                            {
                                var docCode = _context.ODOC.Where(e => e.Id == sub.DocId).Select(e => e.InvoiceCode).FirstOrDefault();
                                
                                DOC16 doc16 = new DOC16();
                                doc16.Type = sub.CommittedType;
                                doc16.FatherId = model.Id;
                                doc16.DocCode = docCode??"";
                                doc16.DocID = sub.DocId;
                                doc16.Volumn = sub.Volume;
                                doc16.Bonus = sub.BonusApplied;
                                listDOC16.Add(doc16);
                            } 
                        }   
                        if(listDOC16.Count > 0)
                        {
                            _context.DOC16.AddRange(listDOC16);
                            await _context.SaveChangesAsync();
                        }    
                        var (m, mes) = await _committedService.CreateCommitedTracking(b);
                        if (mes != null)
                        {
                            mess.Status = mes.Status;
                            mess.Errors = mes.Errors;
                            transaction.Rollback();
                            return (null, mess);
                        }
                    } 
                    if(a!=null)
                    {
                        foreach (var line in a.CommittedLine)
                        {
                            foreach (var sub in line.CommittedLineSub)
                            {
                                sub.ItemTypes = null;
                                sub.Brand = null;
                            }
                        }
                        _context.Committed.Update(a);
                        await _context.SaveChangesAsync();
                    }

                    if (model.ObjType == 22)
                    {
                        await LogProvicy(model);
                    }
                    
                    await transaction.CommitAsync();

                    if (model.UserId != bp.SaleId)
                    {
                        _eventAggregator.Publish(new Models.NotificationModels.Notification
                        {
                            Message = string.Format(message, model.CardCode),
                            Title = string.Format(notiTitle, model.InvoiceCode),
                            Type = "info",
                            Object = new Models.NotificationModels.NotificationObject
                            {
                                ObjId = model.Id,
                                ObjName = "-",
                                ObjType = notiType,
                            },
                            SendUsers = [bp.SaleId ?? 0],
                        });
                    }
                    return (null, null);
                }
                catch (Exception ex)
                {
                    mess.Status = 900;
                    mess.Errors = ex.Message;
                    transaction.Rollback();
                    return (null, mess);
                }
            }
        }
        public async Task<(Redeem, Mess)> CreateAsync(RedeemCreateDto dto)
        {
            Mess mess = new Mess();
            try
            {
                if (dto.Lines == null || !dto.Lines.Any())
                {
                    mess.Status = 400;
                    mess.Errors = "Danh sách sản phẩm đổi quà không được để trống.";
                    return (null, mess);
                }
                var odoc = new ODOC
                {
                    CardId = dto.CustomerId,
                    DocType = "YCVP",
                    CardCode = dto.CustomerCode,
                    CardName = dto.CustomerName,
                    DocDate = dto.RedeemDate,
                    Note = dto.Note,
                    ItemDetail = dto.Lines.Select(x => new DOC1
                    {
                        ItemId = x.ItemId,
                        ItemCode = x.ItemCode,
                        ItemName = x.ItemName,
                        Price = x.PointPerUnit,
                        Quantity = x.Quantity
                    }).ToList()
                };
                var (doc, message) = await AddDocumentAsync(odoc, 12);
                if (message != null)
                    return (null, message);
                var redeem = new Redeem
                {
                    Id = doc.Id,
                    InvoiceCode = doc.InvoiceCode,
                    CustomerId = doc.CardId ?? 0,
                    CustomerCode = doc.CardCode,
                    CustomerName = doc.CardName,
                    RedeemDate = doc.DocDate ?? DateTime.MinValue,
                    Note = doc.Note,
                    TotalPointUsed = (int)doc.ItemDetail.Sum(e => e.Quantity * e.Price),
                    Lines = doc.ItemDetail.Select(x => new RedeemLine
                    {
                        ItemId = x.ItemId ?? 0,
                        ItemCode = x.ItemCode,
                        ItemName = x.ItemName,
                        PointPerUnit = x.Price,
                        Quantity = x.Quantity
                    }).ToList()
                };
                return (redeem, null);
            }
            catch (Exception ex)
            {
                mess.Status = 900;
                mess.Errors = ex.Message;
                return (null, mess);
            }
        }

        public async Task<(PaymentInfo, Committed,CommittedTracking)> GetPaymentInfo(PriceDocCheck model)
        {
            double total = 0;
            double TotalPayNow = 0;
            double TotalDebt = 0;
            double TotalDebtGuarantee = 0;

            double TotalDiscount = 0;
            var bp = await _context.BP.Include(p => p.Groups).AsNoTracking().FirstOrDefaultAsync(b => b.Id == model.CardId);
            if (bp == null)
            {
                throw new KeyNotFoundException("Không tìm thấy nhà phân phối");
            }
            Committed commited1 = null;
            CommittedTracking commitedTracking = null;
            commited1 = await _context.Committed.AsNoTracking().AsSplitQuery()
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
                .FirstOrDefaultAsync(p => p.CommittedYear!.Value.Year == DateTime.Now.Year && p.DocStatus == "A" && p.CardId == model.CardId);

            var listPromotion = model.Promotion.Select(e => e.PromotionId).ToList();
            List<bool> cm = null;
            if (commited1 == null)
                goto track;
            commitedTracking = _context.CommittedTracking.AsNoTracking().AsSplitQuery()
                   .Include(p => p.CommittedTrackingLine).FirstOrDefault(e => e.CommittedId == commited1.Id);
            // Committed month, year
            cm = _context.Promotion.Where(e => e.IsOtherDist == false && listPromotion.Contains(e.Id)).Select(e => e.IsOtherDist).ToList();

            if (cm.Count >= 0)
            {
                var promotions = await _context.Promotion
                  .AsNoTracking()
                  .AsSplitQuery()
                  .Where(p => listPromotion.Contains(p.Id))
                  .Include(p => p.PromotionLine)
                  .ThenInclude(p => p.PromotionLineSub)
                  .ThenInclude(p => p.PromotionItemBuy)
                  .Include(p => p.PromotionLine)
                  .ThenInclude(p => p.PromotionLineSub)
                  .ThenInclude(p => p.PromotionUnit)
                  .Include(p => p.PromotionBrand)
                  .Include(p => p.PromotionIndustry)
                  .ToListAsync();
                var itemType = promotions.SelectMany(p => p.PromotionLine).Where(p => p.AddAccumulate == true)
                                .SelectMany(pl => pl.PromotionLineSub)
                                .SelectMany(pls => pls.PromotionItemBuy)
                                .Select(itemBuy => new
                                {
                                    itemBuy.ItemId,
                                    itemBuy.ItemType
                                }).ToList();
                var itemUnit = promotions.SelectMany(p => p.PromotionLine).Where(p => p.AddAccumulate == true)
                               .SelectMany(pl => pl.PromotionLineSub)
                               .SelectMany(pls => pls.PromotionUnit)
                               .Select(itemBuy => new
                               {
                                   itemBuy.UomId,
                               }).ToList();
                if (commited1 != null)
                {
                    if (commitedTracking == null)
                    {
                        commitedTracking = new CommittedTracking();
                        commitedTracking.CommittedId = commited1.Id;
                        List<CommittedTrackingLine> list = new List<CommittedTrackingLine>();
                        foreach (var cmt in commited1.CommittedLine)
                        {
                            var line = new CommittedTrackingLine();
                            line.OrderDate = DateTime.Now;
                            
                            line.DocId = 0;
                            int FlagM = 0;
                            if (cmt.CommittedType == "Q")
                            {

                                DateTime date = DateTime.Now;
                                if (new[] { 1, 2, 3 }.Contains(date.Month))
                                    line.CommittedType = "Q1";
                                if (new[] { 4, 5, 6 }.Contains(date.Month))
                                    line.CommittedType = "Q2";
                                if (new[] { 8, 9, 7 }.Contains(date.Month))
                                    line.CommittedType = "Q3";
                                if (new[] { 11, 12, 10 }.Contains(date.Month))
                                    line.CommittedType = "Q4";
                                FlagM++;
                            }
                            if (cmt.CommittedType == "Y")
                            {
                                line.CommittedType = "M" + DateTime.Now.Month;
                            }
                            foreach (var lineSub in cmt.CommittedLineSub.ToList())
                            {
                                line.CommittedLineSubId = lineSub.Id;
                                var lisr = lineSub.ItemTypes.Select(e => e.Id).ToList();
                                var itemTypeG = itemType.Where(e => e.ItemType == "G").Select(e => e.ItemId).ToList();
                                var brandId = promotions.SelectMany(p => p.PromotionBrand).Select(p => p.BrandId).ToList();
                                var industryId = promotions.SelectMany(p => p.PromotionIndustry).Select(p => p.IndustryId).ToList();
                                List<Item> listI = new List<Item>();
                                List<Item> listIs = new List<Item>();
                                var listIt = model.ItemDetail.Select(e => e.ItemId);
                                if (itemTypeG.Count > 0)
                                {
                                    listI = _context.Item.Include(e => e.Packing).Where(p => lisr.Contains(p.ItemTypeId ?? 0) && listIt.Contains(p.Id)
                                        && !itemTypeG.Contains(p.ItemTypeId ?? 0) && !brandId.Contains(p.BrandId ?? 0) && !industryId.Contains(p.IndustryId ?? 0)).ToList();
                                    listIs = _context.Item.Include(e => e.Packing).Where(p => lisr.Contains(p.ItemTypeId ?? 0) && listIt.Contains(p.Id)).ToList();
                                }
                                else
                                {
                                    var itemTypeI = itemType.Where(e => e.ItemType == "I").Select(e => e.ItemId).ToList();
                                    listI = _context.Item.Include(e => e.Packing).Where(p => lisr.Contains(p.ItemTypeId ?? 0) && listIt.Contains(p.Id)
                                    && !itemTypeI.Contains(p.Id) && !brandId.Contains(p.BrandId ?? 0) && !industryId.Contains(p.IndustryId ?? 0)).ToList();
                                    listIs = _context.Item.Include(e => e.Packing).Where(p => lisr.Contains(p.ItemTypeId ?? 0) && listIt.Contains(p.Id)).ToList();
                                }
                                //if (listI.Count == 0 && itemType.Count > 0)
                                //    continue;
                                var totalVolumn = from detail in model.ItemDetail
                                                  join item in listIs on detail.ItemId equals item.Id
                                                  where detail.ItemId == item.Id
                                                  select new
                                                  {
                                                      Volumn = detail.Quantity * item.Packing.Volumn,
                                                      Bonus = detail.Quantity * detail.Price
                                                  };
                                double totalVolumnT = totalVolumn.Sum(p => p.Volumn) ?? 0;
                                if (listI.Count == 0 && itemType.Count > 0)
                                {
                                    lineSub.Package = (lineSub.Package ?? 0) ;
                                    lineSub.AfterVolumn = 0;
                                }   
                                else
                                {
                                    lineSub.AfterVolumn = totalVolumnT;
                                    lineSub.Package = (lineSub.Package ?? 0) + totalVolumnT ; 
                                }   
                                lineSub.CurrentVolumn = 0;
                                double totalBonus = totalVolumn.Sum(p => p.Bonus);
                                
                                if (line.CommittedType == "M1")
                                {
                                    if (totalVolumnT < lineSub.Month1)
                                    {
                                        line.IsCommitted = false;
                                    }
                                    else
                                    {
                                        line.IsCommitted = true;
                                    }
                                    lineSub.TotalMonth1 = totalVolumnT;
                                    if (cm.Count <= 0)
                                    {
                                        if (totalVolumnT >= lineSub.Month1)
                                            lineSub.BonusMonth1 = (totalBonus * (lineSub.DiscountMonth ?? 0)) / 100;
                                        line.BonusApplied = lineSub.BonusMonth1;
                                    }
                                    else
                                    {
                                        lineSub.BonusMonth1 = 0;
                                        line.BonusApplied = 0;
                                    }
                                    line.Volume = lineSub.TotalMonth1;
                                    if (listI.Count == 0 && itemType.Count > 0)
                                    {
                                        lineSub.TotalMonth1 = lineSub.TotalMonth1 - totalVolumnT;
                                        if (cm.Count <= 0)
                                            if (totalVolumnT >= lineSub.Month1)
                                                lineSub.BonusMonth1 = lineSub.BonusMonth1 - ((totalBonus * (lineSub.DiscountMonth ?? 0)) / 100);
                                        line.IsCheck = false;
                                    }    
                                        
                                }
                                if (line.CommittedType == "M2")
                                {
                                    if (totalVolumnT < lineSub.Month2)
                                    {
                                        line.IsCommitted = false;
                                    }
                                    else
                                    {
                                        line.IsCommitted = true;
                                    }
                                    lineSub.TotalMonth2 = totalVolumnT;
                                    if (cm.Count <= 0)
                                    {
                                        if (totalVolumnT >= lineSub.Month2)
                                            lineSub.BonusMonth2 = (totalBonus * (lineSub.DiscountMonth ?? 0)) / 100;
                                        line.BonusApplied = lineSub.BonusMonth2;
                                    }
                                    else
                                    {
                                        lineSub.BonusMonth2 = 0;
                                        line.BonusApplied = 0;
                                    }
                                    line.Volume = lineSub.TotalMonth2;
                                    if (listI.Count == 0 && itemType.Count > 0)
                                    {
                                        lineSub.TotalMonth2 = lineSub.TotalMonth2 - totalVolumnT;
                                        if (cm.Count <= 0)
                                            if (totalVolumnT >= lineSub.Month2)
                                                lineSub.BonusMonth2 = lineSub.BonusMonth2 - ((totalBonus * (lineSub.DiscountMonth ?? 0)) / 100);
                                        line.IsCheck = false;
                                    }    
                                        
                                }
                                if (line.CommittedType == "M3")
                                {
                                    if (totalVolumnT < lineSub.Month3)
                                    {
                                        line.IsCommitted = false;
                                    }
                                    else
                                    {
                                        line.IsCommitted = true;
                                    }
                                    lineSub.TotalMonth3 = totalVolumnT;
                                    if (cm.Count <= 0)
                                    {
                                        if (totalVolumnT >= lineSub.Month3)
                                            lineSub.BonusMonth3 = (totalBonus * (lineSub.DiscountMonth ?? 0)) / 100;
                                        line.BonusApplied = lineSub.BonusMonth3;
                                    }
                                    else
                                    {
                                        lineSub.BonusMonth3 = 0;
                                        line.BonusApplied = 0;
                                    }
                                    line.Volume = lineSub.TotalMonth3;
                                    if (listI.Count == 0 && itemType.Count > 0)
                                    {
                                        lineSub.TotalMonth3 = lineSub.TotalMonth3 - totalVolumnT;
                                        if (cm.Count <= 0)
                                            if (totalVolumnT >= lineSub.Month3)
                                                lineSub.BonusMonth3 = lineSub.BonusMonth3 - ((totalBonus * (lineSub.DiscountMonth ?? 0)) / 100);
                                        line.IsCheck = false;
                                    }
                                }
                                if (line.CommittedType == "M4")
                                {
                                    if (totalVolumnT < lineSub.Month4)
                                    {
                                        line.IsCommitted = false;
                                    }
                                    else
                                    {
                                        line.IsCommitted = true;
                                    }
                                    lineSub.TotalMonth4 = totalVolumnT;
                                    if (cm.Count <= 0)
                                    {
                                        if (totalVolumnT >= lineSub.Month4)
                                            lineSub.BonusMonth4 = (totalBonus * (lineSub.DiscountMonth ?? 0)) / 100;
                                        line.BonusApplied = lineSub.BonusMonth4;
                                    }
                                    else
                                    {
                                        lineSub.BonusMonth4 = 0;
                                        line.BonusApplied = 0;
                                    }
                                    line.Volume = lineSub.TotalMonth4;
                                    if (listI.Count == 0 && itemType.Count > 0)
                                    {
                                        lineSub.TotalMonth4 = lineSub.TotalMonth4 - totalVolumnT;
                                        if (cm.Count <= 0)
                                            if (totalVolumnT >= lineSub.Month4)
                                                lineSub.BonusMonth4 = lineSub.BonusMonth4 - ((totalBonus * (lineSub.DiscountMonth ?? 0)) / 100);
                                        line.IsCheck = false;
                                    }
                                }
                                if (line.CommittedType == "M5")
                                {
                                    if (totalVolumnT < lineSub.Month5)
                                    {
                                        line.IsCommitted = false;
                                    }
                                    else
                                    {
                                        line.IsCommitted = true;
                                    }
                                    lineSub.TotalMonth5 = totalVolumnT;
                                    if (cm.Count <= 0)
                                    {
                                        if (totalVolumnT >= lineSub.Month5)
                                            lineSub.BonusMonth5 = (totalBonus * (lineSub.DiscountMonth ?? 0)) / 100;
                                        line.BonusApplied = lineSub.BonusMonth5;
                                    }
                                    else
                                    {
                                        lineSub.BonusMonth5 = 0;
                                        line.BonusApplied = 0;
                                    }
                                    line.Volume = lineSub.TotalMonth5;
                                    if (listI.Count == 0 && itemType.Count > 0)
                                    {
                                        lineSub.TotalMonth5 = lineSub.TotalMonth5 - totalVolumnT;
                                        if (cm.Count <= 0)
                                            if (totalVolumnT >= lineSub.Month5)
                                                lineSub.BonusMonth5 = lineSub.BonusMonth5 - ((totalBonus * (lineSub.DiscountMonth ?? 0)) / 100);
                                        line.IsCheck = false;
                                    }
                                }
                                if (line.CommittedType == "M6")
                                {
                                    if (totalVolumnT < lineSub.Month6)
                                    {
                                        line.IsCommitted = false;
                                    }
                                    else
                                    {
                                        line.IsCommitted = true;
                                    }
                                    lineSub.TotalMonth6 = totalVolumnT;
                                    if (cm.Count <= 0)
                                    {
                                        if (totalVolumnT >= lineSub.Month6)
                                            lineSub.BonusMonth6 = (totalBonus * (lineSub.DiscountMonth ?? 0)) / 100;
                                        line.BonusApplied = lineSub.BonusMonth6;
                                    }
                                    else
                                    {
                                        lineSub.BonusMonth6 = 0;
                                        line.BonusApplied = 0;
                                    }
                                    line.Volume = lineSub.TotalMonth6;
                                    if (listI.Count == 0 && itemType.Count > 0)
                                    {
                                        lineSub.TotalMonth6 = lineSub.TotalMonth6 - totalVolumnT;
                                        if (cm.Count <= 0)
                                            if (totalVolumnT >= lineSub.Month6)
                                                lineSub.BonusMonth6 = lineSub.BonusMonth6 - ((totalBonus * (lineSub.DiscountMonth ?? 0)) / 100);
                                        line.IsCheck = false;
                                    }
                                }
                                if (line.CommittedType == "M7")
                                {
                                    if (totalVolumnT < lineSub.Month7)
                                    {
                                        line.IsCommitted = false;
                                    }
                                    else
                                    {
                                        line.IsCommitted = true;
                                    }
                                    lineSub.TotalMonth7 = totalVolumnT;
                                    if (cm.Count <= 0)
                                    {
                                        if (totalVolumnT >= lineSub.Month7)
                                            lineSub.BonusMonth7 = (totalBonus * (lineSub.DiscountMonth ?? 0)) / 100;
                                        line.BonusApplied = lineSub.BonusMonth7;
                                    }
                                    else
                                    {
                                        lineSub.BonusMonth7 = 0;
                                        line.BonusApplied = 0;
                                    }
                                    line.Volume = lineSub.TotalMonth7;
                                    if (listI.Count == 0 && itemType.Count > 0)
                                    {
                                        lineSub.TotalMonth7 = lineSub.TotalMonth7 - totalVolumnT;
                                        if (cm.Count <= 0)
                                            if (totalVolumnT >= lineSub.Month7)
                                                lineSub.BonusMonth7 = lineSub.BonusMonth7 - ((totalBonus * (lineSub.DiscountMonth ?? 0)) / 100);
                                        line.IsCheck = false;
                                    }
                                }
                                if (line.CommittedType == "M8")
                                {
                                    if (totalVolumnT < lineSub.Month8)
                                    {
                                        line.IsCommitted = false;
                                    }
                                    else
                                    {
                                        line.IsCommitted = true;
                                    }
                                    lineSub.TotalMonth8 = totalVolumnT;
                                    if (cm.Count <= 0)
                                    {
                                        if (totalVolumnT >= lineSub.Month8)
                                            lineSub.BonusMonth8 = (totalBonus * (lineSub.DiscountMonth ?? 0)) / 100;
                                        line.BonusApplied = lineSub.BonusMonth8;
                                    }
                                    else
                                    {
                                        lineSub.BonusMonth8 = 0;
                                        line.BonusApplied = 0;
                                    }
                                    line.Volume = lineSub.TotalMonth8;
                                    if (listI.Count == 0 && itemType.Count > 0)
                                    {
                                        lineSub.TotalMonth8 = lineSub.TotalMonth8 - totalVolumnT;
                                        if (cm.Count <= 0)
                                            if (totalVolumnT >= lineSub.Month8)
                                                lineSub.BonusMonth8 = lineSub.BonusMonth8 - ((totalBonus * (lineSub.DiscountMonth ?? 0)) / 100);
                                        line.IsCheck = false;
                                    }
                                }
                                if (line.CommittedType == "M9")
                                {
                                    if (totalVolumnT < lineSub.Month9)
                                    {
                                        line.IsCommitted = false;
                                    }
                                    else
                                    {
                                        line.IsCommitted = true;
                                    }
                                    lineSub.TotalMonth9 = totalVolumnT;
                                    if (cm.Count <= 0)
                                    {
                                        if (totalVolumnT >= lineSub.Month9)
                                            lineSub.BonusMonth9 = (totalBonus * (lineSub.DiscountMonth ?? 0)) / 100;
                                        line.BonusApplied = lineSub.BonusMonth9;
                                    }
                                    else
                                    {
                                        lineSub.BonusMonth9 = 0;
                                        line.BonusApplied = 0;
                                    }
                                    line.Volume = lineSub.TotalMonth9;
                                    if (listI.Count == 0 && itemType.Count > 0)
                                    {
                                        lineSub.TotalMonth9 = lineSub.TotalMonth9 - totalVolumnT;
                                        if (cm.Count <= 0)
                                            if (totalVolumnT >= lineSub.Month9)
                                                lineSub.BonusMonth9 = lineSub.BonusMonth9 - ((totalBonus * (lineSub.DiscountMonth ?? 0)) / 100);
                                        line.IsCheck = false;
                                    }
                                }
                                if (line.CommittedType == "M10")
                                {
                                    if (totalVolumnT < lineSub.Month10)
                                    {
                                        line.IsCommitted = false;
                                    }
                                    else
                                    {
                                        line.IsCommitted = true;
                                    }
                                    lineSub.TotalMonth10 = totalVolumnT;
                                    if (cm.Count <= 0)
                                    {
                                        if (totalVolumnT >= lineSub.Month10)
                                            lineSub.BonusMonth10 = (totalBonus * (lineSub.DiscountMonth ?? 0)) / 100;
                                        line.BonusApplied = lineSub.BonusMonth10;
                                    }
                                    else
                                    {
                                        lineSub.BonusMonth10 = 0;
                                        line.BonusApplied = 0;
                                    }
                                    line.Volume = lineSub.TotalMonth10;
                                    if (listI.Count == 0 && itemType.Count > 0)
                                    {
                                        lineSub.TotalMonth10 = lineSub.TotalMonth10 - totalVolumnT;
                                        if (cm.Count <= 0)
                                            if (totalVolumnT >= lineSub.Month10)
                                                lineSub.BonusMonth10 = lineSub.BonusMonth10 - ((totalBonus * (lineSub.DiscountMonth ?? 0)) / 100);
                                        line.IsCheck = false;
                                    }
                                }
                                if (line.CommittedType == "M11")
                                {
                                    if (totalVolumnT < lineSub.Month11)
                                    {
                                        line.IsCommitted = false;
                                    }
                                    else
                                    {
                                        line.IsCommitted = true;
                                    }
                                    lineSub.TotalMonth11 = totalVolumnT;
                                    if (cm.Count <= 0)
                                    {
                                        if (totalVolumnT >= lineSub.Month11)
                                            lineSub.BonusMonth11 = (totalBonus * (lineSub.DiscountMonth ?? 0)) / 100;
                                        line.BonusApplied = lineSub.BonusMonth11;
                                    }
                                    else
                                    {
                                        lineSub.BonusMonth11 = 0;
                                        line.BonusApplied = 0;
                                    }
                                    line.Volume = lineSub.TotalMonth11;
                                    if (listI.Count == 0 && itemType.Count > 0)
                                    {
                                        lineSub.TotalMonth11 = lineSub.TotalMonth11 - totalVolumnT;
                                        if (cm.Count <= 0)
                                            if (totalVolumnT >= lineSub.Month11)
                                                lineSub.BonusMonth11 = lineSub.BonusMonth11 - ((totalBonus * (lineSub.DiscountMonth ?? 0)) / 100);
                                        line.IsCheck = false;
                                    }
                                }
                                if (line.CommittedType == "M12")
                                {
                                    if (totalVolumnT < lineSub.Month12)
                                    {
                                        line.IsCommitted = false;
                                    }
                                    else
                                    {
                                        line.IsCommitted = true;
                                    }
                                    lineSub.TotalMonth12 = totalVolumnT;
                                    if (cm.Count <= 0)
                                    {
                                        if (totalVolumnT >= lineSub.Month12)
                                            lineSub.BonusMonth12 = (totalBonus * (lineSub.DiscountMonth ?? 0)) / 100;
                                        line.BonusApplied = lineSub.BonusMonth12;
                                    }   
                                    else
                                    {
                                        lineSub.BonusMonth12 = 0;
                                        line.BonusApplied = 0;
                                    }    
                                        
                                    line.Volume = lineSub.TotalMonth12;
                                    if (listI.Count == 0 && itemType.Count > 0)
                                    {
                                        lineSub.TotalMonth12 = lineSub.TotalMonth12 - totalVolumnT;
                                        if (cm.Count <= 0)
                                            if (totalVolumnT >= lineSub.Month12)
                                                lineSub.BonusMonth12 = lineSub.BonusMonth12 - ((totalBonus * (lineSub.DiscountMonth ?? 0)) / 100);
                                        line.IsCheck = false;
                                    }
                                }
                                if (line.CommittedType == "Q1")
                                {
                                    if (totalVolumnT < lineSub.Quarter1)
                                    {
                                        line.IsCommitted = false;
                                    }
                                    else
                                    {
                                        line.IsCommitted = true;
                                    }
                                    lineSub.TotalQuarter1 = totalVolumnT;
                                    if (cm.Count <= 0)
                                    {
                                        if (totalVolumnT >= lineSub.Quarter1)
                                            lineSub.BonusQuarter1 = (totalBonus * (lineSub.Discount ?? 0)) / 100;
                                        line.BonusApplied = lineSub.BonusQuarter1;
                                    }
                                    else
                                    {
                                        lineSub.BonusQuarter1 = 0;
                                        line.BonusApplied = 0;
                                    }
                                    line.Volume = lineSub.TotalQuarter1;
                                    if (listI.Count == 0 && itemType.Count > 0)
                                    {
                                        lineSub.TotalQuarter1 = lineSub.TotalQuarter1 - totalVolumnT;
                                        if (cm.Count <= 0)
                                            if (totalVolumnT >= lineSub.Quarter1)
                                                lineSub.BonusQuarter1 = lineSub.BonusQuarter1 - ((totalBonus * (lineSub.Discount ?? 0)) / 100);
                                        line.IsCheck = false;
                                    }
                                }
                                if (line.CommittedType == "Q2")
                                {
                                    if (totalVolumnT < lineSub.Quarter2)
                                    {
                                        line.IsCommitted = false;

                                    }
                                    else
                                    {
                                        line.IsCommitted = true;
                                        
                                    }
                                    lineSub.TotalQuarter2 = totalVolumnT;
                                    if (cm.Count <= 0)
                                    {
                                        if (totalVolumnT >= lineSub.Quarter2)
                                            lineSub.BonusQuarter2 = (totalBonus * (lineSub.Discount ?? 0)) / 100;
                                        line.BonusApplied = lineSub.BonusQuarter2;
                                    }
                                    else
                                    {
                                        lineSub.BonusQuarter2 = 0;
                                        line.BonusApplied = 0;
                                    }
                                    line.Volume = lineSub.TotalQuarter2;
                                    if (listI.Count == 0 && itemType.Count > 0)
                                    {
                                        lineSub.TotalQuarter2 = lineSub.TotalQuarter2 - totalVolumnT;
                                        if (cm.Count <= 0)
                                            if (totalVolumnT >= lineSub.Quarter2)
                                                lineSub.BonusQuarter2 = lineSub.BonusQuarter2 - ((totalBonus * (lineSub.Discount ?? 0)) / 100);
                                        line.IsCheck = false;
                                    }

                                }
                                if (line.CommittedType == "Q3")
                                {
                                    if (totalVolumnT < lineSub.Quarter3)
                                    {
                                        line.IsCommitted = false;

                                    }
                                    else
                                    {
                                        line.IsCommitted = true;
                                        
                                    }
                                    lineSub.TotalQuarter3 = totalVolumnT;
                                    if (cm.Count <= 0)
                                    {
                                        if (totalVolumnT >= lineSub.Quarter3)
                                            lineSub.BonusQuarter3 = (totalBonus * (lineSub.Discount ?? 0)) / 100;
                                        line.BonusApplied = lineSub.BonusQuarter3;
                                    }
                                    else
                                    {
                                        lineSub.BonusQuarter3 = 0;
                                        line.BonusApplied = 0;
                                    }
                                    line.Volume = lineSub.TotalQuarter3;
                                    if (listI.Count == 0 && itemType.Count > 0)
                                    {

                                        lineSub.TotalQuarter3 = lineSub.TotalQuarter3 - totalVolumnT;
                                        if (cm.Count <= 0)
                                            if (totalVolumnT >= lineSub.Quarter3)
                                                lineSub.BonusQuarter3 = lineSub.BonusQuarter3 - ((totalBonus * (lineSub.Discount ?? 0)) / 100);
                                        line.IsCheck = false;
                                    }
                                }
                                if (line.CommittedType == "Q4")
                                {
                                    if (totalVolumnT < lineSub.Quarter4)
                                    {
                                        line.IsCommitted = false;

                                    }
                                    else
                                    {
                                        line.IsCommitted = true;
                                    }
                                    lineSub.TotalQuarter4 = totalVolumnT;
                                    if (cm.Count <= 0)
                                    {
                                        if (totalVolumnT > lineSub.Quarter4)
                                            lineSub.BonusQuarter4 = (totalBonus * (lineSub.Discount ?? 0)) / 100;
                                        line.BonusApplied = lineSub.BonusQuarter4;
                                    }
                                    else
                                    {
                                        lineSub.BonusQuarter4 = 0;
                                        line.BonusApplied = 0;
                                    }
                                    line.Volume = lineSub.TotalQuarter4;
                                    if (listI.Count == 0 && itemType.Count > 0)
                                    {
                                        lineSub.TotalQuarter4 = lineSub.TotalQuarter4 - totalVolumnT;
                                        if (cm.Count <= 0)
                                            if (totalVolumnT > lineSub.Quarter4)
                                                lineSub.BonusQuarter4 = lineSub.BonusQuarter4 - ((totalBonus * (lineSub.Discount ?? 0)) / 100);
                                        line.IsCheck = false;
                                    }

                                }
                                list.Add(line);
                                if (DateTime.Now.Month <= 3)
                                {
                                    line = new CommittedTrackingLine();
                                    line.OrderDate = DateTime.Now;
                                    line.CommittedLineSubId = lineSub.Id;
                                    line.DocId = 0;
                                    double totalM = 0;
                                        totalM = (lineSub.Month1 ?? 0) + (lineSub.Month2 ?? 0) + (lineSub.Month3 ?? 0);
                                    if (totalVolumnT >= totalM)
                                    {
                                        line.IsCommitted = true;
                                    }
                                    else
                                        line.IsCommitted = false;
                                    
                                    if (cm.Count <= 0)
                                    {
                                        if (totalVolumnT >= totalM)
                                            lineSub.ThreeMonthBonus = (totalBonus * (lineSub.ThreeMonthDiscount ?? 0)) / 100;
                                        line.BonusApplied = lineSub.ThreeMonthBonus ?? 0;
                                    }
                                    else
                                    {
                                        lineSub.ThreeMonthBonus = 0;
                                        line.BonusApplied = 0;
                                    }
                                    line.Volume = totalVolumnT;
                                    line.CommittedType = "3T";
                                    if (listI.Count == 0 && itemType.Count > 0)
                                    {
                                        if (cm.Count <= 0)
                                            if (totalVolumnT >= totalM)
                                                lineSub.ThreeMonthBonus = lineSub.ThreeMonthBonus - ((totalBonus * (lineSub.ThreeMonthDiscount ?? 0)) / 100);
                                        line.IsCheck = false;
                                    }
                                    list.Add(line);
                                }
                                if (DateTime.Now.Month <= 6)
                                {
                                    line = new CommittedTrackingLine();
                                    line.OrderDate = DateTime.Now;
                                    line.CommittedLineSubId = lineSub.Id;
                                    line.DocId = 0;
                                    double totalM = 0;
                                    if (FlagM > 0)
                                        totalM = (lineSub.Quarter1 ?? 0) + (lineSub.Quarter2 ??0);
                                    else
                                        totalM = (lineSub.Month1 ?? 0) + (lineSub.Month2 ?? 0)+ (lineSub.Month3 ?? 0) + (lineSub.Month4 ?? 0) + (lineSub.Month5 ?? 0) + (lineSub.Month6 ?? 0); ;
                                    if (totalVolumnT >= totalM)
                                    {
                                        line.IsCommitted = true;
                                    }
                                    else
                                        line.IsCommitted = false;
                                    if(cm.Count <= 0)
                                    {
                                        if (totalVolumnT >= totalM)
                                            lineSub.SixMonthBonus = (totalBonus * (lineSub.SixMonthDiscount ?? 0)) / 100;
                                        line.BonusApplied = lineSub.SixMonthBonus ?? 0;
                                    }
                                    else
                                    {
                                        lineSub.SixMonthBonus = 0;
                                        line.BonusApplied = 0;
                                    }
                                    line.Volume = totalVolumnT;
                                    line.CommittedType = "6T";
                                    if (listI.Count == 0 && itemType.Count > 0)
                                    {
                                        if (cm.Count <= 0)
                                            if (totalVolumnT >= totalM)
                                            lineSub.SixMonthBonus = lineSub.SixMonthBonus - ((totalBonus * (lineSub.SixMonthDiscount ?? 0)) / 100);
                                        line.IsCheck = false;
                                    }
                                    list.Add(line);
                                }
                                if (DateTime.Now.Month <= 9)
                                {
                                    line = new CommittedTrackingLine();
                                    line.OrderDate = DateTime.Now;
                                    line.CommittedLineSubId = lineSub.Id;
                                    line.DocId = 0;
                                    double totalM = 0;
                                    if (FlagM > 0)
                                        totalM = (lineSub.Quarter1 ?? 0) + (lineSub.Quarter2 ?? 0) + (lineSub.Quarter3 ?? 0);
                                    else
                                        totalM = (lineSub.Month1 ?? 0) + (lineSub.Month2 ?? 0) + (lineSub.Month3 ?? 0) + (lineSub.Month4 ?? 0) + (lineSub.Month5 ?? 0) + (lineSub.Month6 ?? 0)
                                            + (lineSub.Month7 ?? 0) + (lineSub.Month8 ?? 0) + (lineSub.Month9 ?? 0); 
                                    if (totalVolumnT >= totalM)
                                    {
                                        line.IsCommitted = true;
                                    }
                                    else
                                        line.IsCommitted = false;
                                    
                                    if (cm.Count <= 0)
                                    {
                                        if (totalVolumnT >= totalM)
                                            lineSub.NineMonthBonus = (totalBonus * (lineSub.NineMonthDiscount ?? 0)) / 100;
                                        line.BonusApplied = lineSub.NineMonthBonus ?? 0;
                                    }
                                    else
                                    {
                                        lineSub.NineMonthBonus = 0;
                                        line.BonusApplied = lineSub.NineMonthBonus ?? 0;
                                    }
                                    if (listI.Count == 0 && itemType.Count > 0)
                                    {
                                        if (cm.Count <= 0)
                                            if (totalVolumnT >= totalM)
                                                lineSub.NineMonthBonus = lineSub.NineMonthBonus - (totalBonus * (lineSub.NineMonthDiscount ?? 0)) / 100;
                                        line.IsCheck = false;
                                    }
                                    line.Volume = totalVolumnT;
                                    line.CommittedType = "9T";
                                    list.Add(line);
                                }
                                int checkover = 0, Flag = 0;
                                checkover = lineSub.CommittedLineSubSub.Count();
                                foreach (var over in lineSub.CommittedLineSubSub.ToList().OrderByDescending(e=>e.OutPut))
                                {
                                    if(Flag == 0)
                                    {
                                        line = new CommittedTrackingLine();
                                        line.OrderDate = DateTime.Now;
                                        line.CommittedLineSubId = lineSub.Id;
                                        line.DocId = 0;
                                        if (totalVolumnT >= over.OutPut)
                                        {
                                            line.IsCommitted = true;
                                            Flag++;
                                        }
                                        else
                                            line.IsCommitted = false;
                                        over.Total = totalVolumnT;
                                        if (cm.Count <= 0)
                                        {
                                            if (totalVolumnT >= over.OutPut)
                                                over.BonusTotal = (totalBonus * over.Discount) / 100;
                                            line.BonusApplied = over.BonusTotal;
                                        }
                                        else
                                        {
                                            line.BonusApplied = 0;
                                            over.BonusTotal = 0;
                                        }
                                        line.Volume = totalVolumnT;
                                        line.CommittedType = "OV" + checkover;
                                        if (listI.Count == 0 && itemType.Count > 0)
                                        {
                                            over.Total = over.Total - totalVolumnT;
                                            if (cm.Count <= 0)
                                                if (totalVolumnT >= over.OutPut)
                                                    over.BonusTotal = over.BonusTotal - (totalBonus * over.Discount) / 100;
                                            line.IsCheck = false;
                                        }
                                        if (Flag > 1)
                                        {
                                            if (line.IsCheck == true && over.BonusTotal > 0)
                                                over.BonusTotal = over.BonusTotal - (totalBonus * over.Discount) / 100;
                                            line.IsCommitted = false;
                                        }
                                        checkover--;
                                        list.Add(line);
                                    }
                                    
                                }
                                if(Flag == 0)
                                {
                                    line = new CommittedTrackingLine();
                                    line.OrderDate = DateTime.Now;
                                    line.CommittedLineSubId = lineSub.Id;
                                    line.DocId = 0;
                                    if (totalVolumnT >= lineSub.Total)
                                    {
                                        line.IsCommitted = true;
                                    }
                                    else
                                        line.IsCommitted = false;
                                    lineSub.Total12M = totalVolumnT;
                                    if (cm.Count <= 0)
                                    {
                                        if (totalVolumnT >= lineSub.Total)
                                            lineSub.YearBonus = (totalBonus * (lineSub.DiscountYear ?? 0)) / 100;
                                        line.BonusApplied = lineSub.YearBonus ?? 0;
                                    }
                                    else
                                    {
                                        lineSub.YearBonus = 0;
                                        line.BonusApplied = 0;
                                    }
                                    line.Volume = totalVolumnT;
                                    line.CommittedType = "12T";
                                    if (listI.Count == 0 && itemType.Count > 0)
                                    {
                                        lineSub.Total12M = lineSub.Total12M  - totalVolumnT;
                                        if (cm.Count <= 0)
                                            if (totalVolumnT >= lineSub.Total)
                                            lineSub.YearBonus = lineSub.YearBonus - (totalBonus * (lineSub.DiscountYear ?? 0)) / 100;
                                        line.IsCheck = false;
                                    }
                                    list.Add(line);
                                }
                            }
                            commitedTracking.CommittedTrackingLine = list;
                        }
                    }
                    else
                    {
                        List<CommittedTrackingLine> list = new List<CommittedTrackingLine>();
                        foreach (var cmt in commited1.CommittedLine)
                        {
                            var line = new CommittedTrackingLine();
                            line.OrderDate = DateTime.Now;
                            line.DocId = 0;
                            int FlagM = 0;
                            if (cmt.CommittedType == "Q")
                            {

                                DateTime date = DateTime.Now;
                                if (new[] { 1, 2, 3 }.Contains(date.Month))
                                    line.CommittedType = "Q1";
                                if (new[] { 4, 5, 6 }.Contains(date.Month))
                                    line.CommittedType = "Q2";
                                if (new[] { 8, 9, 7 }.Contains(date.Month))
                                    line.CommittedType = "Q3";
                                if (new[] { 11, 12, 10 }.Contains(date.Month))
                                    line.CommittedType = "Q4";
                                FlagM++;
                            }
                            if (cmt.CommittedType == "Y")
                            {
                                line.CommittedType = "M" + DateTime.Now.Month;
                            }
                            foreach (var lineSub in cmt.CommittedLineSub.ToList())
                            {
                                line.CommittedLineSubId = lineSub.Id;
                                var lisr = lineSub.ItemTypes.Select(e => e.Id).ToList();
                                var itemTypeG = itemType.Where(e => e.ItemType == "G").Select(e => e.ItemId).ToList();
                                var brandId = promotions.SelectMany(p => p.PromotionBrand).Select(p => p.BrandId).ToList();
                                var industryId = promotions.SelectMany(p => p.PromotionIndustry).Select(p => p.IndustryId).ToList();
                                List<Item> listI = new List<Item>();
                                List<Item> listIs = new List<Item>();
                                var listIt = model.ItemDetail.Select(e => e.ItemId);
                                if (itemTypeG.Count > 0)
                                {
                                    listI = _context.Item.Include(e => e.Packing).Where(p => lisr.Contains(p.ItemTypeId ?? 0) && listIt.Contains(p.Id)
                                        && !itemTypeG.Contains(p.ItemTypeId ?? 0) && !brandId.Contains(p.BrandId ?? 0) && !industryId.Contains(p.IndustryId ?? 0)).ToList();
                                    listIs = _context.Item.Include(e => e.Packing).Where(p => lisr.Contains(p.ItemTypeId ?? 0) && listIt.Contains(p.Id)).ToList();
                                }
                                else
                                {
                                    var itemTypeI = itemType.Where(e => e.ItemType == "I").Select(e => e.ItemId).ToList();
                                    listI = _context.Item.Include(e => e.Packing).Where(p => lisr.Contains(p.ItemTypeId ?? 0) && listIt.Contains(p.Id)
                                    && !itemTypeI.Contains(p.Id) && !brandId.Contains(p.BrandId ?? 0) && !industryId.Contains(p.IndustryId ?? 0)).ToList();
                                    listIs = _context.Item.Include(e => e.Packing).Where(p => lisr.Contains(p.ItemTypeId ?? 0) && listIt.Contains(p.Id)).ToList();
                                }
                                
                                var totalVolumn = from detail in model.ItemDetail
                                                  join item in listIs on detail.ItemId equals item.Id
                                                  where detail.ItemId == item.Id
                                                  select new
                                                  {
                                                      Volumn = detail.Quantity * item.Packing.Volumn,
                                                      Bonus = detail.Quantity * detail.Price
                                                  };
                                double totalVolumnT = totalVolumn.Sum(p => p.Volumn) ?? 0;
                                double totalBonus = totalVolumn.Sum(p => p.Bonus);
                                if (listI.Count == 0 && itemType.Count > 0)
                                {
                                    lineSub.CurrentVolumn = lineSub.Package ?? 0;
                                    lineSub.AfterVolumn = lineSub.Package ?? 0;
                                    lineSub.Package = lineSub.Package ?? 0 ;
                                }
                                else
                                {
                                    lineSub.CurrentVolumn = lineSub.Package ?? 0;
                                    lineSub.AfterVolumn = (lineSub.Package ?? 0) + totalVolumnT;
                                    lineSub.Package = (lineSub.Package ?? 0) + totalVolumnT;
                                }
                                
                                List<int> listDocl;
                                if (line.CommittedType == "M1")
                                {
                                    listDocl = commitedTracking.CommittedTrackingLine.Where(e=>e.OrderDate.Month == 1 && e.CommittedType == "M1"&& e.IsCommitted ==true).Select(e => e.DocId).ToList();
                                    var M = commitedTracking.CommittedTrackingLine.Where(e => !listDocl.Contains(e.DocId) && e.OrderDate.Month == DateTime.Now.Month).ToList();
                                    double totalVolumnOver = M.Sum(e => e.Volume);
                                    if (totalVolumnT + totalVolumnOver < lineSub.Month1)
                                    {
                                        line.IsCommitted = false;
                                    }
                                    else
                                    {
                                        line.IsCommitted = true;
                                        M.ForEach(e => e.IsCommitted = true);
                                        M.ForEach(e => e.BonusAddApplied = e.BonusApplied);
                                    }
                                    lineSub.TotalMonth1 = totalVolumnT;
                                    if (cm.Count <= 0)
                                    {
                                        if (totalVolumnT >= lineSub.Month1)
                                            lineSub.BonusMonth1 = (totalBonus * (lineSub.DiscountMonth ?? 0)) / 100;
                                        line.Volume = lineSub.TotalMonth1;
                                    }
                                    else
                                    {
                                        lineSub.BonusMonth1 = 0;
                                        line.Volume = 0;
                                    }
                                    if (listI.Count == 0 && itemType.Count > 0)
                                    {
                                        lineSub.TotalMonth1 = lineSub.TotalMonth2 - totalVolumnT;
                                        if (cm.Count <= 0)
                                            if (totalVolumnT >= lineSub.Month1)
                                                lineSub.BonusMonth1 = lineSub.BonusMonth1 - ((totalBonus * (lineSub.DiscountMonth ?? 0)) / 100);
                                        line.IsCheck = false;
                                    }

                                }
                                if (line.CommittedType == "M2")
                                {
                                    listDocl = commitedTracking.CommittedTrackingLine.Where(e => e.OrderDate.Month == 2 && e.CommittedType == "M2" && e.IsCommitted == true).Select(e => e.DocId).ToList();
                                    var M = commitedTracking.CommittedTrackingLine.Where(e => !listDocl.Contains(e.DocId) && e.OrderDate.Month == DateTime.Now.Month && e.IsCommitted == false).ToList();
                                    double totalVolumnOver = M.Sum(e => e.Volume);
                                    if (totalVolumnT + totalVolumnOver < lineSub.Month2)
                                    {
                                        line.IsCommitted = false;
                                    }
                                    else
                                    {
                                        line.IsCommitted = true;
                                        M.ForEach(e => e.IsCommitted = true);
                                        M.ForEach(e => e.BonusAddApplied = e.BonusApplied);
                                    }
                                    lineSub.TotalMonth2 = totalVolumnT;
                                    
                                    if (cm.Count <= 0)
                                    {
                                        if (totalVolumnT >= lineSub.Month2)
                                            lineSub.BonusMonth2 = (totalBonus * (lineSub.DiscountMonth ?? 0)) / 100;
                                        line.BonusApplied = lineSub.BonusMonth2;
                                    }
                                    else
                                    {
                                        lineSub.BonusMonth2 = 0;
                                        line.BonusApplied = 0;
                                    }
                                    line.Volume = lineSub.TotalMonth2;
                                    if (listI.Count == 0 && itemType.Count > 0)
                                    {
                                        lineSub.TotalMonth2 = lineSub.TotalMonth2 - totalVolumnT;
                                        if (cm.Count <= 0)
                                            if (totalVolumnT >= lineSub.Month2)
                                                lineSub.BonusMonth2 = lineSub.BonusMonth2 - ((totalBonus * (lineSub.DiscountMonth ?? 0)) / 100);
                                        line.IsCheck = false;
                                    }
                                }
                                if (line.CommittedType == "M3")
                                {
                                    listDocl = commitedTracking.CommittedTrackingLine.Where(e => e.OrderDate.Month == 3 && e.CommittedType == "M3" && e.IsCommitted == true).Select(e => e.DocId).ToList();
                                    var M = commitedTracking.CommittedTrackingLine.Where(e => !listDocl.Contains(e.DocId) && e.OrderDate.Month == DateTime.Now.Month && e.IsCommitted == false).ToList();
                                    double totalVolumnOver = M.Sum(e => e.Volume);
                                    if (totalVolumnT + totalVolumnOver < lineSub.Month3)
                                    {
                                        line.IsCommitted = false;
                                    }
                                    else
                                    {
                                        line.IsCommitted = true;
                                        M.ForEach(e => e.IsCommitted = true);
                                        M.ForEach(e => e.BonusAddApplied = e.BonusApplied);
                                    }
                                    lineSub.TotalMonth3 = totalVolumnT;
                                    if (cm.Count <= 0)
                                    {
                                        if (totalVolumnT >= lineSub.Month3)
                                            lineSub.BonusMonth3 = (totalBonus * (lineSub.DiscountMonth ?? 0)) / 100;
                                        line.BonusApplied = lineSub.BonusMonth3;
                                    }
                                    else
                                    {
                                        lineSub.BonusMonth3 = 0;
                                        line.BonusApplied = 0;
                                    }
                                    line.Volume = lineSub.TotalMonth3;
                                    if (listI.Count == 0 && itemType.Count > 0)
                                    {
                                        lineSub.TotalMonth3 = lineSub.TotalMonth3 - totalVolumnT;
                                        if (cm.Count <= 0)
                                            if (totalVolumnT >= lineSub.Month3)
                                                lineSub.BonusMonth3 = lineSub.BonusMonth3 - ((totalBonus * (lineSub.DiscountMonth ?? 0)) / 100);
                                        line.IsCheck = false;
                                    }
                                }
                                if (line.CommittedType == "M4")
                                {
                                    listDocl = commitedTracking.CommittedTrackingLine.Where(e => e.OrderDate.Month == 4 && e.CommittedType == "M4" && e.IsCommitted == true).Select(e => e.DocId).ToList();
                                    var M = commitedTracking.CommittedTrackingLine.Where(e => !listDocl.Contains(e.DocId) && e.OrderDate.Month == DateTime.Now.Month && e.IsCommitted == false).ToList();
                                    double totalVolumnOver = M.Sum(e => e.Volume);
                                    if (totalVolumnT + totalVolumnOver < lineSub.Month4)
                                    {
                                        line.IsCommitted = false;
                                    }
                                    else
                                    {
                                        line.IsCommitted = true;
                                        M.ForEach(e => e.IsCommitted = true);
                                    }
                                    lineSub.TotalMonth4 = totalVolumnT;
                                    if (cm.Count <= 0)
                                    {
                                        if (totalVolumnT >= lineSub.Month4)
                                            lineSub.BonusMonth4 = (totalBonus * (lineSub.DiscountMonth ?? 0)) / 100;
                                        line.BonusApplied = lineSub.BonusMonth4;
                                    }
                                    else
                                    {
                                        lineSub.BonusMonth4 = 0;
                                        line.BonusApplied = 0;
                                    }
                                    line.Volume = lineSub.TotalMonth4;
                                    if (listI.Count == 0 && itemType.Count > 0)
                                    {
                                        lineSub.TotalMonth4 = lineSub.TotalMonth4 - totalVolumnT;
                                        if (cm.Count <= 0)
                                            if (totalVolumnT >= lineSub.Month4)
                                                lineSub.BonusMonth4 = lineSub.BonusMonth4 - ((totalBonus * (lineSub.DiscountMonth ?? 0)) / 100);
                                        line.IsCheck = false;
                                    }
                                }
                                if (line.CommittedType == "M5")
                                {
                                    listDocl = commitedTracking.CommittedTrackingLine.Where(e => e.OrderDate.Month == 5 && e.CommittedType == "M5" && e.IsCommitted == true).Select(e => e.DocId).ToList();
                                    var M = commitedTracking.CommittedTrackingLine.Where(e => !listDocl.Contains(e.DocId) && e.OrderDate.Month == DateTime.Now.Month && e.IsCommitted == false).ToList();
                                    double totalVolumnOver = M.Sum(e => e.Volume);
                                    if (totalVolumnT + totalVolumnOver < lineSub.Month5)
                                    {
                                        line.IsCommitted = false;
                                    }
                                    else
                                    {
                                        line.IsCommitted = true;
                                        M.ForEach(e => e.IsCommitted = true);
                                    }
                                    lineSub.TotalMonth5 = totalVolumnT;
                                    if (cm.Count <= 0)
                                    {
                                        if (totalVolumnT >= lineSub.Month5)
                                            lineSub.BonusMonth5 = (totalBonus * (lineSub.DiscountMonth ?? 0)) / 100;
                                        line.BonusApplied = lineSub.BonusMonth5;
                                    }
                                    else
                                    {
                                        lineSub.BonusMonth5 = 0;
                                        line.BonusApplied = 0;
                                    }
                                    line.Volume = lineSub.TotalMonth5;
                                    if (listI.Count == 0 && itemType.Count > 0)
                                    {
                                        lineSub.TotalMonth5 = lineSub.TotalMonth5 - totalVolumnT;
                                        if (cm.Count <= 0)
                                            if (totalVolumnT >= lineSub.Month5)
                                                lineSub.BonusMonth5 = lineSub.BonusMonth5 - ((totalBonus * (lineSub.DiscountMonth ?? 0)) / 100);
                                        line.IsCheck = false;
                                    }
                                }
                                if (line.CommittedType == "M6")
                                {
                                    listDocl = commitedTracking.CommittedTrackingLine.Where(e => e.OrderDate.Month == 6 && e.CommittedType == "M6" && e.IsCommitted == true).Select(e => e.DocId).ToList();
                                    var M = commitedTracking.CommittedTrackingLine.Where(e => !listDocl.Contains(e.DocId) && e.OrderDate.Month == DateTime.Now.Month && e.IsCommitted == false).ToList();
                                    double totalVolumnOver = M.Sum(e => e.Volume);
                                    if (totalVolumnT + totalVolumnOver < lineSub.Month6)
                                    {
                                        line.IsCommitted = false;
                                    }
                                    else
                                    {
                                        line.IsCommitted = true;
                                        M.ForEach(e => e.IsCommitted = true);
                                    }
                                    lineSub.TotalMonth6 = totalVolumnT;
                                    if (cm.Count <= 0)
                                    {
                                        if (totalVolumnT >= lineSub.Month6)
                                            lineSub.BonusMonth6 = (totalBonus * (lineSub.DiscountMonth ?? 0)) / 100;
                                        line.BonusApplied = lineSub.BonusMonth6;
                                    }
                                    else
                                    {
                                        lineSub.BonusMonth6 = 0;
                                        line.BonusApplied = 0;
                                    }
                                    line.Volume = lineSub.TotalMonth6;
                                    if (listI.Count == 0 && itemType.Count > 0)
                                    {
                                        lineSub.TotalMonth6 = lineSub.TotalMonth6 - totalVolumnT;
                                        if (cm.Count <= 0)
                                            if (totalVolumnT >= lineSub.Month6)
                                                lineSub.BonusMonth6 = lineSub.BonusMonth6 - ((totalBonus * (lineSub.DiscountMonth ?? 0)) / 100);
                                        line.IsCheck = false;
                                    }
                                }
                                if (line.CommittedType == "M7")
                                {
                                    listDocl = commitedTracking.CommittedTrackingLine.Where(e => e.OrderDate.Month == 7 && e.CommittedType == "M7" && e.IsCommitted == true).Select(e => e.DocId).ToList();
                                    var M = commitedTracking.CommittedTrackingLine.Where(e => !listDocl.Contains(e.DocId) && e.OrderDate.Month == DateTime.Now.Month && e.IsCommitted == false).ToList();
                                    double totalVolumnOver = M.Sum(e => e.Volume);
                                    if (totalVolumnT + totalVolumnOver < lineSub.Month7)
                                    {
                                        line.IsCommitted = false;
                                    }
                                    else
                                    {
                                        line.IsCommitted = true;
                                        M.ForEach(e => e.IsCommitted = true);
                                    }
                                    lineSub.TotalMonth7 = totalVolumnT;
                                    if (cm.Count <= 0)
                                    {
                                        if (totalVolumnT >= lineSub.Month7)
                                            lineSub.BonusMonth7 = (totalBonus * (lineSub.DiscountMonth ?? 0)) / 100;
                                        line.BonusApplied = lineSub.BonusMonth7;
                                    }
                                    else
                                    {
                                        lineSub.BonusMonth7 = 0;
                                        line.BonusApplied = 0;
                                    }
                                    line.Volume = lineSub.TotalMonth7;
                                    if (listI.Count == 0 && itemType.Count > 0)
                                    {
                                        lineSub.TotalMonth7 = lineSub.TotalMonth7 - totalVolumnT;
                                        if (cm.Count <= 0)
                                            if (totalVolumnT >= lineSub.Month7)
                                                lineSub.BonusMonth7 = lineSub.BonusMonth7 - ((totalBonus * (lineSub.DiscountMonth ?? 0)) / 100);
                                        line.IsCheck = false;
                                    }
                                }
                                if (line.CommittedType == "M8")
                                {
                                    listDocl = commitedTracking.CommittedTrackingLine.Where(e => e.OrderDate.Month == 8 && e.CommittedType == "M8" && e.IsCommitted == true).Select(e => e.DocId).ToList();
                                    var M = commitedTracking.CommittedTrackingLine.Where(e => !listDocl.Contains(e.DocId) && e.OrderDate.Month == DateTime.Now.Month && e.IsCommitted == false).ToList();
                                    double totalVolumnOver = M.Sum(e => e.Volume);
                                    if (totalVolumnT + totalVolumnOver < lineSub.Month8)
                                    {
                                        line.IsCommitted = false;
                                    }
                                    else
                                    {
                                        line.IsCommitted = true;
                                        M.ForEach(e => e.IsCommitted = true);
                                    }
                                    lineSub.TotalMonth8 = totalVolumnT;
                                    if (cm.Count <= 0)
                                    {
                                        if (totalVolumnT >= lineSub.Month8)
                                            lineSub.BonusMonth8 = (totalBonus * (lineSub.DiscountMonth ?? 0)) / 100;
                                        line.BonusApplied = lineSub.BonusMonth8;
                                    }
                                    else
                                    {
                                        lineSub.BonusMonth8 = 0;
                                        line.BonusApplied = 0;
                                    }
                                    line.Volume = lineSub.TotalMonth8;
                                    if (listI.Count == 0 && itemType.Count > 0)
                                    {
                                        lineSub.TotalMonth8 = lineSub.TotalMonth8 - totalVolumnT;
                                        if (cm.Count <= 0)
                                            if (totalVolumnT >= lineSub.Month8)
                                                lineSub.BonusMonth8 = lineSub.BonusMonth8 - ((totalBonus * (lineSub.DiscountMonth ?? 0)) / 100);
                                        line.IsCheck = false;
                                    }
                                }
                                if (line.CommittedType == "M9")
                                {
                                    listDocl = commitedTracking.CommittedTrackingLine.Where(e => e.OrderDate.Month == 9 && e.CommittedType == "M9"  && e.IsCommitted == true).Select(e => e.DocId).ToList();
                                    var M = commitedTracking.CommittedTrackingLine.Where(e => !listDocl.Contains(e.DocId) && e.OrderDate.Month == DateTime.Now.Month && e.IsCommitted == false).ToList();
                                    double totalVolumnOver = M.Sum(e => e.Volume);
                                    if (totalVolumnT + totalVolumnOver < lineSub.Month9)
                                    {
                                        line.IsCommitted = false;
                                    }
                                    else
                                    {
                                        line.IsCommitted = true;
                                        M.ForEach(e => e.IsCommitted = true);
                                    }
                                    lineSub.TotalMonth9 = totalVolumnT;
                                    if (cm.Count <= 0)
                                    {
                                        if (totalVolumnT >= lineSub.Month9)
                                            lineSub.BonusMonth9 = (totalBonus * (lineSub.DiscountMonth ?? 0)) / 100;
                                        line.BonusApplied = lineSub.BonusMonth9;
                                    }
                                    else
                                    {
                                        lineSub.BonusMonth9 = 0;
                                        line.BonusApplied = 0;
                                    }
                                    line.Volume = lineSub.TotalMonth9;
                                    if (listI.Count == 0 && itemType.Count > 0)
                                    {
                                        lineSub.TotalMonth9 = lineSub.TotalMonth9 - totalVolumnT;
                                        if (cm.Count <= 0)
                                            if (totalVolumnT >= lineSub.Month9)
                                                lineSub.BonusMonth9 = lineSub.BonusMonth9 - ((totalBonus * (lineSub.DiscountMonth ?? 0)) / 100);
                                        line.IsCheck = false;
                                    }
                                }
                                if (line.CommittedType == "M10")
                                {
                                    listDocl = commitedTracking.CommittedTrackingLine.Where(e => e.OrderDate.Month == 10 && e.CommittedType == "M10" && e.IsCommitted == true).Select(e => e.DocId).ToList();
                                    var M = commitedTracking.CommittedTrackingLine.Where(e => !listDocl.Contains(e.DocId) && e.OrderDate.Month == DateTime.Now.Month && e.IsCommitted == false).ToList();
                                    double totalVolumnOver = M.Sum(e => e.Volume);
                                    if (totalVolumnT + totalVolumnOver < lineSub.Month10)
                                    {
                                        line.IsCommitted = false;
                                    }
                                    else
                                    {
                                        line.IsCommitted = true;
                                        M.ForEach(e => e.IsCommitted = true);
                                    }
                                    lineSub.TotalMonth10 = totalVolumnT;
                                    if (cm.Count <= 0)
                                    {
                                        if (totalVolumnT >= lineSub.Month10)
                                            lineSub.BonusMonth10 = (totalBonus * (lineSub.DiscountMonth ?? 0)) / 100;
                                        line.BonusApplied = lineSub.BonusMonth10;
                                    }
                                    else
                                    {
                                        lineSub.BonusMonth10 = 0;
                                        line.BonusApplied = 0;
                                    }
                                    line.Volume = lineSub.TotalMonth10;
                                    if (listI.Count == 0 && itemType.Count > 0)
                                    {
                                        lineSub.TotalMonth10 = lineSub.TotalMonth10 - totalVolumnT;
                                        if (cm.Count <= 0)
                                            if (totalVolumnT >= lineSub.Month10)
                                                lineSub.BonusMonth10 = lineSub.BonusMonth10 - ((totalBonus * (lineSub.DiscountMonth ?? 0)) / 100);
                                        line.IsCheck = false;
                                    }
                                }
                                if (line.CommittedType == "M11")
                                {
                                    listDocl = commitedTracking.CommittedTrackingLine.Where(e => e.OrderDate.Month == 11 && e.CommittedType == "M11" && e.IsCommitted == true).Select(e => e.DocId).ToList();
                                    var M = commitedTracking.CommittedTrackingLine.Where(e => !listDocl.Contains(e.DocId) && e.OrderDate.Month == DateTime.Now.Month && e.IsCommitted == false).ToList();
                                    double totalVolumnOver = M.Sum(e => e.Volume);
                                    if (totalVolumnT + totalVolumnOver < lineSub.Month11)
                                    {
                                        line.IsCommitted = false;
                                    }
                                    else
                                    {
                                        line.IsCommitted = true;
                                        M.ForEach(e => e.IsCommitted = true);
                                    }
                                    lineSub.TotalMonth11 = totalVolumnT;
                                    if (cm.Count <= 0)
                                    {
                                        if (totalVolumnT >= lineSub.Month11)
                                            lineSub.BonusMonth11 = (totalBonus * (lineSub.DiscountMonth ?? 0)) / 100;
                                        line.BonusApplied = lineSub.BonusMonth11;
                                    }
                                    else
                                    {
                                        lineSub.BonusMonth11 = 0;
                                        line.BonusApplied = 0;
                                    }
                                    line.Volume = lineSub.TotalMonth11;
                                    if (listI.Count == 0 && itemType.Count > 0)
                                    {
                                        lineSub.TotalMonth11 = lineSub.TotalMonth11 - totalVolumnT;
                                        if (cm.Count <= 0)
                                            if (totalVolumnT >= lineSub.Month11)
                                                lineSub.BonusMonth11 = lineSub.BonusMonth11 - ((totalBonus * (lineSub.DiscountMonth ?? 0)) / 100);
                                        line.IsCheck = false;
                                    }
                                }
                                if (line.CommittedType == "M12")
                                {
                                    listDocl = commitedTracking.CommittedTrackingLine.Where(e => e.OrderDate.Month == 12 && e.CommittedType == "M12" && e.IsCommitted == true).Select(e => e.DocId).ToList();
                                    var M = commitedTracking.CommittedTrackingLine.Where(e => !listDocl.Contains(e.DocId) && e.OrderDate.Month == DateTime.Now.Month && e.IsCommitted == false).ToList();
                                    double totalVolumnOver = M.Sum(e => e.Volume);
                                    if (totalVolumnT + totalVolumnOver < lineSub.Month12)
                                    {
                                        line.IsCommitted = false;
                                    }
                                    else
                                    {
                                        line.IsCommitted = true;
                                        M.ForEach(e => e.IsCommitted = true);
                                    }
                                    lineSub.TotalMonth12 = totalVolumnT;
                                    if (cm.Count <= 0)
                                    {
                                        if (totalVolumnT >= lineSub.Month12)
                                            lineSub.BonusMonth12 = (totalBonus * (lineSub.DiscountMonth ?? 0)) / 100;
                                        line.BonusApplied = lineSub.BonusMonth12;
                                    }
                                    else
                                    {
                                        lineSub.BonusMonth12 = 0;
                                        line.BonusApplied = 0;
                                    }
                                    line.Volume = lineSub.TotalMonth12;
                                    if (listI.Count == 0 && itemType.Count > 0)
                                    {
                                        lineSub.TotalMonth12 = lineSub.TotalMonth12 - totalVolumnT;
                                        if (cm.Count <= 0)
                                            if (totalVolumnT >= lineSub.Month12)
                                                lineSub.BonusMonth12 = lineSub.BonusMonth12 - ((totalBonus * (lineSub.DiscountMonth ?? 0)) / 100);
                                        line.IsCheck = false;
                                    }
                                }
                                if (line.CommittedType == "Q1")
                                {
                                    List<int> l = new List<int> { 1, 2, 3 };
                                    listDocl = commitedTracking.CommittedTrackingLine.Where(e => l.Contains(e.OrderDate.Month) && e.CommittedType == "Q1" && e.IsCommitted == true).Select(e => e.DocId).ToList();
                                    var M = commitedTracking.CommittedTrackingLine.Where(e => !listDocl.Contains(e.DocId) && l.Contains(e.OrderDate.Month) && e.CommittedType == "Q1" && e.IsCommitted == false).ToList();
                                    double totalVolumnOver = M.Sum(e => e.Volume);
                                    if (totalVolumnT + totalVolumnOver < lineSub.Quarter1)
                                    {
                                        line.IsCommitted = false;
                                    }
                                    else
                                    {
                                        line.IsCommitted = true;
                                        M.ForEach(e => e.IsCommitted = true);
                                    }
                                    lineSub.TotalQuarter1 = totalVolumnT;
                                    if (cm.Count <= 0)
                                    {
                                        if (totalVolumnT + totalVolumnOver >= lineSub.Quarter1)
                                            lineSub.BonusQuarter1 = (totalBonus * (lineSub.Discount ?? 0)) / 100;
                                        line.BonusApplied = lineSub.BonusQuarter1;
                                    }
                                    else
                                    {
                                        lineSub.BonusQuarter1 = 0;
                                        line.BonusApplied = 0;
                                    }
                                    line.Volume = lineSub.TotalQuarter1;
                                    if (listI.Count == 0 && itemType.Count > 0)
                                    {
                                        lineSub.TotalQuarter1 = lineSub.TotalQuarter1 - totalVolumnT;
                                        if (cm.Count <= 0)
                                            if (totalVolumnT + totalVolumnOver >= lineSub.Quarter1)
                                                lineSub.BonusQuarter1 = lineSub.BonusQuarter1 - ((totalBonus * (lineSub.Discount ?? 0)) / 100);
                                        line.IsCheck = false;
                                    }
                                }
                                if (line.CommittedType == "Q2")
                                {
                                    List<int> l = new List<int> { 4, 5, 6 };
                                    listDocl = commitedTracking.CommittedTrackingLine.Where(e => l.Contains(e.OrderDate.Month) && e.CommittedType == "Q2" && e.IsCommitted == true).Select(e => e.DocId).ToList();
                                    var M = commitedTracking.CommittedTrackingLine.Where(e => !listDocl.Contains(e.DocId) && l.Contains(e.OrderDate.Month) && e.CommittedType == "Q2" && e.IsCommitted == false).ToList();
                                    double totalVolumnOver = M.Sum(e => e.Volume);
                                    if (totalVolumnT + totalVolumnOver < lineSub.Quarter2)
                                    {
                                        line.IsCommitted = false;
                                    }
                                    else
                                    {
                                        line.IsCommitted = true;
                                        M.ForEach(e => e.IsCommitted = true);
                                    }
                                    lineSub.TotalQuarter2 = totalVolumnT;
                                    if (cm.Count <= 0)
                                    {
                                        if (totalVolumnT + totalVolumnOver >= lineSub.Quarter2)
                                            lineSub.BonusQuarter2 = (totalBonus * (lineSub.Discount ?? 0)) / 100;
                                        line.BonusApplied = lineSub.BonusQuarter2;
                                    }
                                    else
                                    {
                                        lineSub.BonusQuarter2 = 0;
                                        line.BonusApplied = 0;
                                    }
                                    line.Volume = lineSub.TotalQuarter2;
                                    if (listI.Count == 0 && itemType.Count > 0)
                                    {
                                        lineSub.TotalQuarter2 = lineSub.TotalQuarter2 - totalVolumnT;
                                        if (cm.Count <= 0)
                                            if (totalVolumnT + totalVolumnOver >= lineSub.Quarter2)
                                                lineSub.BonusQuarter2 = lineSub.BonusQuarter2 - ((totalBonus * (lineSub.Discount ?? 0)) / 100);
                                        line.IsCheck = false;
                                    }

                                }
                                if (line.CommittedType == "Q3")
                                {
                                    List<int> l = new List<int> { 7, 8, 9 };
                                    listDocl = commitedTracking.CommittedTrackingLine.Where(e => l.Contains(e.OrderDate.Month) && e.CommittedType == "Q3" && e.IsCommitted == true).Select(e => e.DocId).ToList();
                                    var M = commitedTracking.CommittedTrackingLine.Where(e => !listDocl.Contains(e.DocId) && l.Contains(e.OrderDate.Month) && e.CommittedType == "Q3" && e.IsCommitted == false).ToList();
                                    double totalVolumnOver = M.Sum(e => e.Volume);
                                    if (totalVolumnT + totalVolumnOver < lineSub.Quarter3)
                                    {
                                        line.IsCommitted = false;
                                    }
                                    else
                                    {
                                        line.IsCommitted = true;
                                        M.ForEach(e => e.IsCommitted = true);
                                    }
                                    lineSub.TotalQuarter3 = totalVolumnT;
                                    if (cm.Count <= 0)
                                    {
                                        if (totalVolumnT + totalVolumnOver >= lineSub.Quarter3)
                                            lineSub.BonusQuarter3 = (totalBonus * (lineSub.Discount ?? 0)) / 100;
                                        line.BonusApplied = lineSub.BonusQuarter3;
                                    }
                                    else
                                    {
                                        lineSub.BonusQuarter3 = 0;
                                        line.BonusApplied = 0;
                                    }
                                    line.Volume = lineSub.TotalQuarter3;
                                    if (listI.Count == 0 && itemType.Count > 0)
                                    {
                                        lineSub.TotalQuarter3 = lineSub.TotalQuarter3 - totalVolumnT;
                                        if (cm.Count <= 0)
                                            if (totalVolumnT + totalVolumnOver >= lineSub.Quarter3)
                                                lineSub.BonusQuarter3 = lineSub.BonusQuarter3 - ((totalBonus * (lineSub.Discount ?? 0)) / 100);
                                        line.IsCheck = false;
                                    }
                                }
                                if (line.CommittedType == "Q4")
                                {
                                    List<int> l = new List<int> { 10, 11, 12 };
                                    listDocl = commitedTracking.CommittedTrackingLine.Where(e => l.Contains(e.OrderDate.Month) && e.CommittedType == "Q4" && e.IsCommitted == true).Select(e => e.DocId).ToList();
                                    var M = commitedTracking.CommittedTrackingLine.Where(e => !listDocl.Contains(e.DocId) && l.Contains(e.OrderDate.Month) && e.CommittedType == "Q4" && e.IsCommitted == false).ToList();
                                    double totalVolumnOver = M.Sum(e => e.Volume);
                                    if (totalVolumnT + totalVolumnOver < lineSub.Quarter3)
                                    {
                                        line.IsCommitted = false;
                                    }
                                    else
                                    {
                                        line.IsCommitted = true;
                                        M.ForEach(e => e.IsCommitted = true);
                                    }
                                    lineSub.TotalQuarter4 = totalVolumnT;
                                    if (cm.Count <= 0)
                                    {
                                        if (totalVolumnT + totalVolumnOver >= lineSub.Quarter4)
                                            lineSub.BonusQuarter4 = (totalBonus * (lineSub.Discount ?? 0)) / 100;
                                        line.BonusApplied = lineSub.BonusQuarter4;
                                    }
                                    else
                                    {
                                        lineSub.BonusQuarter4 = 0;
                                        line.BonusApplied = 0;
                                    }
                                    line.Volume = lineSub.TotalQuarter4;
                                    if (listI.Count == 0 && itemType.Count > 0)
                                    {
                                        lineSub.TotalQuarter4 = lineSub.TotalQuarter4 - totalVolumnT;
                                        if (cm.Count <= 0)
                                            if (totalVolumnT + totalVolumnOver >= lineSub.Quarter4)
                                                lineSub.BonusQuarter4 = lineSub.BonusQuarter4 - ((totalBonus * (lineSub.Discount ?? 0)) / 100);
                                        line.IsCheck = false;
                                    }

                                }
                                list.Add(line);
                                if (DateTime.Now.Month <= 3)
                                {
                                    line = new CommittedTrackingLine();
                                    line.OrderDate = DateTime.Now;
                                    line.CommittedLineSubId = lineSub.Id;
                                    line.DocId = 0;
                                    double totalM = 0;
                                    totalM = (lineSub.Month1 ?? 0) + (lineSub.Month2 ?? 0) + (lineSub.Month3 ?? 0);
                                    List<int> l = new List<int> { 1, 2, 3 };
                                    listDocl = commitedTracking.CommittedTrackingLine.Where(e => l.Contains(e.OrderDate.Month) && e.CommittedType == "3T" && e.IsCommitted == true).Select(e => e.DocId).ToList();
                                    var M = commitedTracking.CommittedTrackingLine.Where(e => !listDocl.Contains(e.DocId) && l.Contains(e.OrderDate.Month) && e.CommittedType == "3T" && e.IsCommitted == false).ToList();
                                    double totalVolumnOver = M.Sum(e => e.Volume);
                                    if (totalVolumnT + totalVolumnOver < totalM)
                                    {
                                        line.IsCommitted = false;
                                    }
                                    else
                                    {
                                        line.IsCommitted = true;
                                        M.ForEach(e => e.IsCommitted = true);
                                    }
                                    if (cm.Count <= 0)
                                    {
                                        if (totalVolumnT + totalVolumnOver >= totalM)
                                            lineSub.ThreeMonthBonus = (totalBonus * (lineSub.ThreeMonthDiscount ?? 0)) / 100;
                                        line.BonusApplied = lineSub.ThreeMonthBonus ?? 0;
                                    }
                                    else
                                    {
                                        lineSub.ThreeMonthBonus = 0;
                                        line.BonusApplied = 0;
                                    }
                                    line.Volume = totalVolumnT;
                                    line.CommittedType = "3T";
                                    if (listI.Count == 0 && itemType.Count > 0)
                                    {
                                        if (cm.Count <= 0)
                                            if (totalVolumnT + totalVolumnOver >= totalM)
                                                lineSub.ThreeMonthBonus = (totalBonus * (lineSub.ThreeMonthDiscount ?? 0)) / 100;
                                        line.IsCheck = false;
                                    }
                                    line.CommittedType = "3T";
                                    list.Add(line);
                                }
                                if (DateTime.Now.Month <= 6)
                                {
                                    line = new CommittedTrackingLine();
                                    line.OrderDate = DateTime.Now;
                                    line.CommittedLineSubId = lineSub.Id;
                                    line.DocId = 0;
                                    double totalM = 0;
                                    if (FlagM > 0)
                                        totalM = (lineSub.Quarter1 ?? 0) + (lineSub.Quarter2 ?? 0);
                                    else
                                        totalM = (lineSub.Month1 ?? 0) + (lineSub.Month2 ?? 0) + (lineSub.Month3 ?? 0) + (lineSub.Month4 ?? 0) + (lineSub.Month5 ?? 0) + (lineSub.Month6 ?? 0); ;
                                    List<int> l = new List<int> { 1, 2, 3,4,5,6 };
                                    listDocl = commitedTracking.CommittedTrackingLine.Where(e => l.Contains(e.OrderDate.Month) && e.CommittedType == "6T" && e.IsCommitted == true).Select(e => e.DocId).ToList();
                                    var M = commitedTracking.CommittedTrackingLine.Where(e => !listDocl.Contains(e.DocId) && l.Contains(e.OrderDate.Month) && e.CommittedType == "6T" && e.IsCommitted == false).ToList();
                                    double totalVolumnOver = M.Sum(e => e.Volume);
                                    if (totalVolumnT + totalVolumnOver < totalM)
                                    {
                                        line.IsCommitted = false;
                                    }
                                    else
                                    {
                                        line.IsCommitted = true;
                                        M.ForEach(e => e.IsCommitted = true);
                                    }
                                    if (cm.Count <= 0)
                                    {
                                        if (totalVolumnT + totalVolumnOver >= totalM)
                                            lineSub.SixMonthBonus = (totalBonus * (lineSub.SixMonthDiscount ?? 0)) / 100;
                                        line.BonusApplied = lineSub.SixMonthBonus ?? 0;
                                    }
                                    else
                                    {
                                        lineSub.SixMonthBonus = 0;
                                        line.BonusApplied = 0;
                                    }
                                    line.Volume = totalVolumnT;
                                    line.CommittedType = "6T";
                                    if (listI.Count == 0 && itemType.Count > 0)
                                    {
                                        if (cm.Count <= 0)
                                            if (totalVolumnT + totalVolumnOver >= totalM)
                                                lineSub.SixMonthBonus = lineSub.SixMonthBonus - (totalBonus * (lineSub.SixMonthDiscount ?? 0)) / 100;
                                        line.IsCheck = false;
                                    }
                                    line.CommittedType = "6T";
                                    list.Add(line);
                                }
                                if (DateTime.Now.Month <= 9)
                                {
                                    line = new CommittedTrackingLine();
                                    line.OrderDate = DateTime.Now;
                                    line.CommittedLineSubId = lineSub.Id;
                                    line.DocId = 0;
                                    double totalM = 0;
                                    if (FlagM > 0)
                                        totalM = (lineSub.Quarter1 ?? 0) + (lineSub.Quarter2 ?? 0) + (lineSub.Quarter3 ?? 0);
                                    else
                                        totalM = (lineSub.Month1 ?? 0) + (lineSub.Month2 ?? 0) + (lineSub.Month3 ?? 0) + (lineSub.Month4 ?? 0) + (lineSub.Month5 ?? 0) + (lineSub.Month6 ?? 0)
                                            + (lineSub.Month7 ?? 0) + (lineSub.Month8 ?? 0) + (lineSub.Month9 ?? 0);
                                    List<int> l = new List<int> { 1, 2, 3, 4, 5, 6,7,8,9};
                                    listDocl = commitedTracking.CommittedTrackingLine.Where(e => l.Contains(e.OrderDate.Month) && e.CommittedType == "9T" && e.IsCommitted == true).Select(e => e.DocId).ToList();
                                    var M = commitedTracking.CommittedTrackingLine.Where(e => !listDocl.Contains(e.DocId) && l.Contains(e.OrderDate.Month) && e.CommittedType == "9T" && e.IsCommitted == false).ToList();
                                    double totalVolumnOver = M.Sum(e => e.Volume);
                                    if (totalVolumnT + totalVolumnOver < totalM)
                                    {
                                        line.IsCommitted = false;
                                    }
                                    else
                                    {
                                        line.IsCommitted = true;
                                        M.ForEach(e => e.IsCommitted = true);
                                    }
                                    if (cm.Count <= 0)
                                    {
                                        if (totalVolumnT + totalVolumnOver >= totalM)
                                            lineSub.NineMonthBonus = (totalBonus * (lineSub.NineMonthDiscount ?? 0)) / 100;
                                        line.BonusApplied = lineSub.NineMonthBonus ?? 0;
                                    }
                                    else
                                    {
                                        lineSub.NineMonthBonus = 0;
                                        line.BonusApplied = 0;
                                    }
                                    line.Volume = totalVolumnT;
                                    line.CommittedType = "9T";
                                    if (listI.Count == 0 && itemType.Count > 0)
                                    {
                                        if (cm.Count <= 0)
                                            if (totalVolumnT + totalVolumnOver >= totalM)
                                                lineSub.NineMonthBonus = lineSub.NineMonthBonus - (totalBonus * (lineSub.SixMonthDiscount ?? 0)) / 100;
                                        line.IsCheck = false;
                                    }
                                    list.Add(line);
                                }
                                int checkover = 0, Flag = 0;
                                checkover = lineSub.CommittedLineSubSub.Count();
                                foreach (var over in lineSub.CommittedLineSubSub.ToList().OrderByDescending(e => e.OutPut))
                                {
                                    if(Flag ==0)
                                    {
                                        line = new CommittedTrackingLine();
                                        line.OrderDate = DateTime.Now;
                                        line.CommittedLineSubId = lineSub.Id;
                                        line.DocId = 0;
                                        listDocl = commitedTracking.CommittedTrackingLine.Where(e => e.OrderDate.Month == 12 && e.CommittedType == "OV" + checkover && e.IsCommitted == true).Select(e => e.DocId).ToList();
                                        var M = commitedTracking.CommittedTrackingLine.Where(e => !listDocl.Contains(e.DocId) && e.CommittedType == "OV" + checkover && e.IsCommitted == false).ToList();
                                        double totalVolumnOver = M.Sum(e => e.Volume);
                                        if (totalVolumnT + totalVolumnOver < over.OutPut)
                                        {
                                            line.IsCommitted = false;
                                        }
                                        else
                                        {
                                            line.IsCommitted = true;
                                            M.ForEach(e => e.IsCommitted = true);
                                            Flag++;
                                        }
                                        over.BonusTotal = (totalBonus * over.Discount) / 100;
                                        over.Total = totalBonus;
                                        line.BonusApplied = over.BonusTotal;
                                        line.Volume = totalVolumnT;
                                        line.CommittedType = "OV" + checkover;
                                        if (listI.Count == 0 && itemType.Count > 0)
                                        {
                                            over.BonusTotal = over.BonusTotal - ((totalBonus * over.Discount) / 100);
                                            over.Total = over.Total - totalBonus;
                                            line.IsCheck = false;
                                        }
                                        if (Flag > 1)
                                        {
                                            if (line.IsCheck == true && over.BonusTotal > 0)
                                                over.BonusTotal = over.BonusTotal - (totalBonus * over.Discount) / 100;
                                            line.IsCommitted = false;
                                        }
                                        checkover--;
                                        list.Add(line);
                                    }
                                    
                                }
                                if (Flag == 0)
                                {
                                    line = new CommittedTrackingLine();
                                    line.OrderDate = DateTime.Now;
                                    line.CommittedLineSubId = lineSub.Id;
                                    line.DocId = 0;
                                    listDocl = commitedTracking.CommittedTrackingLine.Where(e => e.OrderDate.Month == 12 && e.CommittedType == "12T"&& e.IsCommitted == true).Select(e => e.DocId).ToList();
                                    var M = commitedTracking.CommittedTrackingLine.Where(e => !listDocl.Contains(e.DocId) && e.CommittedType == "12T"&& e.IsCommitted == false).ToList();
                                    double totalVolumnOver = M.Sum(e => e.Volume);
                                    if (totalVolumnT + totalVolumnOver >= lineSub.Total)
                                    {
                                        line.IsCommitted = true;
                                        M.ForEach(e => e.IsCommitted = true);
                                    }
                                    else
                                        line.IsCommitted = false;
                                    
                                    lineSub.Total12M = totalVolumnT;
                                    
                                    if (cm.Count <= 0)
                                    {
                                        if (totalVolumnT + totalVolumnOver >= lineSub.Total)
                                            lineSub.YearBonus = (totalBonus * (lineSub.DiscountYear ?? 0)) / 100;
                                        line.BonusApplied = lineSub.YearBonus ?? 0;
                                    }
                                    else
                                    {
                                        lineSub.YearBonus = 0;
                                        line.BonusApplied = 0;
                                    }
                                    line.Volume = totalVolumnT;
                                    line.CommittedType = "12T";
                                    if (listI.Count == 0 && itemType.Count > 0)
                                    {
                                        if (cm.Count <= 0)
                                            if (totalVolumnT + totalVolumnOver >= lineSub.Total)
                                                lineSub.YearBonus = lineSub.YearBonus - (totalBonus * (lineSub.DiscountYear ?? 0)) / 100;
                                        line.IsCheck = false;
                                    }    
                                        
                                    list.Add(line);
                                }
                            }
                            commitedTracking.CommittedTrackingLine = list;
                        }
                    }
                }
            }
            track:
            double bonusCommited = 0;
            if (commitedTracking != null && cm.Count == 0)
                bonusCommited = commitedTracking.CommittedTrackingLine.Where(e=>e.IsCommitted == true).Sum(e => e.BonusApplied);
            var bonusDis = 0.0;
            var BonusVolumn = 0.0;
            var PromotionTax = 0.0;
            if (bp.IsBusinessHouse == true)
            {
                var pay = _context.PaymentRule.FirstOrDefault(e => e.Name == "HỘ KINH DOANH");
                double.TryParse(pay.BonusPaynow.ToString(), out bonusDis);
                double.TryParse(pay.BonusVolumn.ToString(), out BonusVolumn);
                double.TryParse(pay.PromotionTax.ToString(), out PromotionTax);
            }
            else
            {
                var pay = _context.PaymentRule.FirstOrDefault(e => e.Name != "HỘ KINH DOANH");
                double.TryParse(pay.BonusPaynow.ToString(), out bonusDis);
                double.TryParse(pay.BonusVolumn.ToString(), out BonusVolumn);
                double.TryParse(pay.PromotionTax.ToString(), out PromotionTax);
            }

            double vatPromotion = 0;
            var promItem = await _context.Item.AsNoTracking()
                .Where(p => model.Promotion.Select(x => x.ItemCode).ToList().Contains(p.ItemCode))
                .Include(p => p.TaxGroups)
                .ToDictionaryAsync(i => i.ItemCode);

            if (bp.IsBusinessHouse == true)
            {
                bonusCommited *= BonusVolumn / 100;

                if (promItem.Count > 0)
                {
                    model.Promotion.ForEach(p =>
                    {
                        if (promItem.TryGetValue(p.ItemCode, out var prom))
                        {
                            p.Price = prom.Price ?? 0;
                            var tax = prom.TaxGroups?.Rate ?? 0;
                            if (p.QuantityAdd != null)
                                vatPromotion += (p.Price * p.QuantityAdd.Value * (1 + (double)tax / 100) * (PromotionTax / 100));
                        }
                    });
                }
            }

            var totalD = model.ItemDetail.Where(p => p.PaymentMethodCode == "PayNow")
                .Sum(p => (p.Price * (1 - p.Discount / 100) * p.Quantity));

            var totalBonusPaynow = 0.0;
            int checkCount = 0;
           
            checkCount++;
            model.ItemDetail.ForEach(
                p =>
                {
                    p.PriceAfterDist = p.Price * (1 - p.Discount / 100);
                    p.LineTotal = p.PriceAfterDist * p.Quantity;

                    if (p.PaymentMethodCode == "PayNow")
                    {
                        p.LineTotal *= (1 - bonusCommited / totalD);
                        totalBonusPaynow += p.Price * p.Quantity * bonusDis / 100 * (1 + p.VAT / 100);
                        TotalPayNow += p.LineTotal * (1 + p.VAT / 100);
                    }

                    p.VATAmount = p.LineTotal * p.VAT / 100;

                    if (p.PaymentMethodCode == "PayCredit")
                        TotalDebt += p.LineTotal * (1 + p.VAT / 100);

                    if (p.PaymentMethodCode == "PayGuarantee")
                        TotalDebtGuarantee += p.LineTotal * (1 + p.VAT / 100);
                });
            
            model.PaymentInfo = new PaymentInfo
            {
                VATAmount = model.ItemDetail.Sum(p => p.VATAmount ?? 0),

                PaynowVATAmount = model.ItemDetail.Where(x => x.PaymentMethodCode == "PayNow").Sum(p => p.VATAmount ?? 0),
                DebtVATAmount = model.ItemDetail.Where(x => x.PaymentMethodCode == "PayCredit").Sum(p => p.VATAmount ?? 0),
                DebtGuaranteeVATAmount = model.ItemDetail.Where(x => x.PaymentMethodCode == "PayGuarantee").Sum(p => p.VATAmount ?? 0),

                TotalBeforeVat = model.ItemDetail.Sum(x => x.Price * (1 - x.Discount / 100) * x.Quantity),

                TotalPayNowBeforeVat = model.ItemDetail.Where(x => x.PaymentMethodCode == "PayNow").Sum(x => x.Price * (1 - x.Discount / 100) * x.Quantity),
                TotalDebtBeforeVat = model.ItemDetail.Where(x => x.PaymentMethodCode == "PayCredit").Sum(x => x.Price * (1 - x.Discount / 100) * x.Quantity),
                TotalDebtGuaranteeBeforeVat = model.ItemDetail.Where(x => x.PaymentMethodCode == "PayGuarantee").Sum(x => x.Price * (1 - x.Discount / 100) * x.Quantity),

                BonusCommited = bonusCommited,
                TotalDebt = TotalDebt,
                TotalDebtGuarantee = TotalDebtGuarantee
            };
            // Pay now
            if (TotalPayNow != 0)
                model.PaymentInfo.TotalPayNow = TotalPayNow;
            var pm = _context.Promotion.Where(e => e.IsOtherPay == false && listPromotion.Contains(e.Id)).Select(e => e.IsOtherPay).ToList();
            if (pm.Count == 0)
            {
                model.PaymentInfo.BonusAmount = totalBonusPaynow - vatPromotion;
                model.PaymentInfo.BonusPercent = bonusDis / 100;
            }
            model.PaymentInfo.TotalAfterVat = model.PaymentInfo.TotalPayNow +
                                                  model.PaymentInfo.TotalDebtGuarantee +
                                                  model.PaymentInfo.TotalDebt;
            if (commitedTracking != null)
            {
                if(commitedTracking.CommittedTrackingLine != null)
                    foreach (var check in commitedTracking.CommittedTrackingLine.Where(e=>e.IsCheck == false || e.CommittedType.IsNullOrEmpty()).ToList())
                    {
                        commitedTracking.CommittedTrackingLine.Remove(check);
                    }
            }
            return (model.PaymentInfo, commited1, commitedTracking);
        }
        public async Task<(PaymentInfo, Committed, CommittedTracking)> GetPaymentInfo1(PriceDocCheck model)
        {
            double bonusCommited = 0;
            double total = 0;
            double TotalPayNow = 0;
            double TotalDebt = 0;
            double TotalDebtGuarantee = 0;
            List<int> ints = new List<int>();
            List<int> intsPay = new List<int>();
            List<int> intsPay1 = new List<int>();
            var totalBonusPaynow = 0.0;
            double vatPromotion = 0;
            var bonusDis = 0.0;
            Committed commited1 = null;
            CommittedTracking commitedTracking = null;
            double TotalDiscount = 0;
            var bp = await _context.BP.Include(p => p.Groups).AsNoTracking().FirstOrDefaultAsync(b => b.Id == model.CardId);
            if (bp == null)
            {
                throw new KeyNotFoundException("Không tìm thấy nhà phân phối");
            }
            if (model.DocType != null)
                goto track; 
           

            
            
            
            commited1 = await _context.Committed.AsNoTracking().AsSplitQuery()
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
                .FirstOrDefaultAsync(p => p.CommittedYear!.Value.Year == DateTime.Now.Year && p.DocStatus == "A" && p.CardId == model.CardId);
           
            model.Promotion.ForEach(p =>
            {

                var prob = _context.Promotion.FirstOrDefault(e => e.Id == p.PromotionId && ((e.HasException == false && e.IsOtherDist == false) || (e.HasException == true && e.IsOtherDistExc == false)));
                if(prob != null)
                {
                    if(p.ListLineId != null)
                    {
                        if (p.ListLineId.Length > 1)
                            ints.AddRange(p.ListLineId);
                        else
                            ints.Add(p.LineId);
                    } 
                    else
                    {
                        ints.Add(p.LineId);
                    }    
                    
                }
                if(p.HasException == true)
                {
                    var prob2 = _context.Promotion.FirstOrDefault(e => e.Id == p.PromotionId &&   e.IsOtherPayExc == true);
                    if (prob2 != null)
                    {
                        if (p.ListLineId != null)
                        {
                            if (p.ListLineId.Length > 1)
                                intsPay1.AddRange(p.ListLineId);
                            else
                                intsPay1.Add(p.LineId);
                        }
                        else
                        {
                            intsPay1.Add(p.LineId);
                        }
                    }
                    var prob21 = _context.Promotion.FirstOrDefault(e => e.Id == p.PromotionId && e.IsOtherPayExc == false);
                    if (prob21 != null)
                    {
                        if (p.ListLineId != null)
                        {
                            if (p.ListLineId.Length > 1)
                                intsPay.AddRange(p.ListLineId);
                            else
                                intsPay.Add(p.LineId);
                        }
                        else
                        {
                            intsPay.Add(p.LineId);
                        }
                    }
                } 
                else
                {
                    var prob2 = _context.Promotion.FirstOrDefault(e => e.Id == p.PromotionId && e.IsOtherPay == true);
                    if (prob2 != null)
                    {
                        if (p.ListLineId != null)
                        {
                            if (p.ListLineId.Length > 1)
                                intsPay1.AddRange(p.ListLineId);
                            else
                                intsPay1.Add(p.LineId);
                        }
                        else
                        {
                            intsPay1.Add(p.LineId);
                        }
                    }
                    var prob21 = _context.Promotion.FirstOrDefault(e => e.Id == p.PromotionId && e.IsOtherPay == false);
                    if (prob21 != null)
                    {
                        if (p.ListLineId != null)
                        {
                            if (p.ListLineId.Length > 1)
                                intsPay.AddRange(p.ListLineId);
                            else
                                intsPay.Add(p.LineId);
                        }
                        else
                        {
                            intsPay.Add(p.LineId);
                        }
                    }
                }    
                
            });
            var listPromotion = model.Promotion.Select(e => e.PromotionId).ToList();

            if (commited1 == null)
                goto track;
            commitedTracking = _context.CommittedTracking.AsNoTracking().AsSplitQuery()
                   .Include(p => p.CommittedTrackingLine).FirstOrDefault(e => e.CommittedId == commited1.Id);
            var promotions = await _context.Promotion.AsNoTracking().AsSplitQuery()
                  .Where(p => listPromotion.Contains(p.Id))
                  .Include(p => p.PromotionLine)
                  .ThenInclude(p => p.PromotionLineSub)
                  .ThenInclude(p => p.PromotionItemBuy)
                  .Include(p => p.PromotionLine)
                  .ThenInclude(p => p.PromotionLineSub)
                  .ThenInclude(p => p.PromotionUnit)
                  .Include(p => p.PromotionBrand)
                  .Include(p => p.PromotionIndustry)
                  .ToListAsync();
            if (promotions.Count > 0)
                foreach (var promotion in promotions)
                {
                    List<ItemTypeKTSL> itemTypeKTSL = promotion.PromotionLine
                    .SelectMany(pl =>
                        pl.PromotionLineSub.SelectMany(pls =>
                            pls.PromotionItemBuy.Select(itemBuy => new ItemTypeKTSL
                            {
                                AddAccumulate = pl.AddAccumulate,
                                ItemId = itemBuy.ItemId,
                                ItemType = itemBuy.ItemType
                            })
                        )
                    )
                    .ToList();
                    List<ItemUnit> itemUnit = promotion.PromotionLine
                    .SelectMany(pl =>
                        pl.PromotionLineSub.SelectMany(pls =>
                            pls.PromotionUnit.Select(itemBuy => new ItemUnit
                            {
                                AddAccumulate = pl.AddAccumulate,
                                UomId = itemBuy.UomId,
                            })
                        )
                    )
                    .ToList();
                    if (commitedTracking == null)
                        Commited(ref commitedTracking, commited1, promotions, promotion, model, itemTypeKTSL, itemUnit, ints);
                    else
                        CommitedTracking(ref commitedTracking, commited1, promotions, promotion, model, itemTypeKTSL, itemUnit, ints);  
                }
            else
            {
                if (commitedTracking == null)
                    Commited(ref commitedTracking, commited1, model);
                else
                    CommitedTracking(ref commitedTracking, commited1, model);
            }
            
            track:
            
            var listID = _context.CommittedTrackingLine.Where(e => e.IsCommitted == true).Select(e => e.Id);
            if (commitedTracking != null)
            {
                var checksss = commitedTracking.CommittedTrackingLine.Where(e => !listID.Contains(e.Id)).ToList();
                bonusCommited = checksss.Sum(e => e.BonusAddApplied ?? 0);
            }    
                
            
            var BonusVolumn = 0.0;
            var PromotionTax = 0.0;
            if (bp.IsBusinessHouse == true)
            {
                var pay = _context.PaymentRule.FirstOrDefault(e => e.Name == "HỘ KINH DOANH");
                double.TryParse(pay.BonusPaynow.ToString(), out bonusDis);
                double.TryParse(pay.BonusVolumn.ToString(), out BonusVolumn);
                double.TryParse(pay.PromotionTax.ToString(), out PromotionTax);
            }
            else
            {
                var pay = _context.PaymentRule.FirstOrDefault(e => e.Name != "HỘ KINH DOANH");
                double.TryParse(pay.BonusPaynow.ToString(), out bonusDis);
                double.TryParse(pay.BonusVolumn.ToString(), out BonusVolumn);
                double.TryParse(pay.PromotionTax.ToString(), out PromotionTax);
            }

           
            var promItem = await _context.Item.AsNoTracking()
                .Where(p => model.Promotion.Select(x => x.ItemCode).ToList().Contains(p.ItemCode))
                .Include(p => p.TaxGroups)
                .ToDictionaryAsync(i => i.ItemCode);

            if (bp.IsBusinessHouse == true)
            {
                bonusCommited *= BonusVolumn / 100;

                if (promItem.Count > 0)
                {
                    model.Promotion.ForEach(p =>
                    {
                        if (promItem.TryGetValue(p.ItemCode, out var prom))
                        {
                            p.Price = prom.Price ?? 0;
                            var tax = 0;// prom.TaxGroups?.Rate ?? 0;
                            if (p.QuantityAdd != null)
                                vatPromotion += (p.Price * p.QuantityAdd.Value * (1 + (double)tax / 100) * (PromotionTax / 100));
                        }
                    });
                }
            }

            var totalD = model.ItemDetail.Where(p => p.PaymentMethodCode == "PayNow")
                .Sum(p => p.DiscountType == "P" ? (p.Price * (1 - p.Discount / 100) * p.Quantity) : (p.Price - p.Discount) * p.Quantity);

            
            int checkCount = 0;
            
            checkCount++;
            model.ItemDetail.ForEach(
                p =>
                {
                    p.PriceAfterDist = p.DiscountType == "P" ? (p.Price * (1 - p.Discount / 100)) : (p.Price - p.Discount);
                    p.LineTotal = p.PriceAfterDist * p.Quantity;

                    if (p.PaymentMethodCode == "PayNow" && bp.IsInterCom ==false)
                    {
                        if (totalD != 0)
                        {
                            p.LineTotal *= (1 - bonusCommited / totalD);
                        }
                        if(intsPay1.Count > 0)
                        {
                            if (intsPay1.Contains(p.LineId) && !intsPay.Contains(p.LineId))
                                totalBonusPaynow += p.Price * p.Quantity * bonusDis / 100 * (1 + p.VAT / 100);
                        }
                        else
                        {
                            //if(!intsPay.Contains(p.LineId))
                            //    totalBonusPaynow += p.Price * p.Quantity * bonusDis / 100 * (1 + p.VAT / 100);
                        }

                        TotalPayNow += p.LineTotal * (1 + p.VAT / 100);
                    }
                    if (p.PaymentMethodCode == "PayNow" && bp.IsInterCom == true)
                    {
                        p.LineTotal *= (1 - bonusCommited / totalD);
                        if (intsPay1.Count > 0)
                        {
                            if (intsPay1.Contains(p.LineId) && !intsPay.Contains(p.LineId))
                                totalBonusPaynow += p.Price * p.Quantity * bonusDis / 100 * (1 + p.VAT / 100);
                        }
                        else
                        {
                            //if (!intsPay.Contains(p.LineId))
                            //    totalBonusPaynow += p.Price * p.Quantity * bonusDis / 100 * (1 + p.VAT / 100);
                        }

                        TotalPayNow += p.LineTotal * (1 + p.VAT / 100);
                    }
                    p.VATAmount = p.LineTotal * p.VAT / 100;

                    if (p.PaymentMethodCode == "PayCredit")
                        TotalDebt += p.LineTotal * (1 + p.VAT / 100);

                    if (p.PaymentMethodCode == "PayGuarantee")
                        TotalDebtGuarantee += p.LineTotal * (1 + p.VAT / 100);
                });
            model.PaymentInfo = new PaymentInfo
            {
                VATAmount = model.ItemDetail.Sum(p => p.VATAmount ?? 0),

                PaynowVATAmount = model.ItemDetail.Where(x => x.PaymentMethodCode == "PayNow").Sum(p => p.VATAmount ?? 0),
                DebtVATAmount = model.ItemDetail.Where(x => x.PaymentMethodCode == "PayCredit").Sum(p => p.VATAmount ?? 0),
                DebtGuaranteeVATAmount = model.ItemDetail.Where(x => x.PaymentMethodCode == "PayGuarantee").Sum(p => p.VATAmount ?? 0),

                TotalBeforeVat = model.ItemDetail.Sum(x => x.DiscountType == "P" ? (x.Price * (1 - x.Discount / 100) * x.Quantity) : (x.Price - x.Discount) * x.Quantity),

                TotalPayNowBeforeVat = model.ItemDetail.Where(x => x.PaymentMethodCode == "PayNow").Sum(x => x.DiscountType == "P" ? (x.Price * (1 - x.Discount / 100) * x.Quantity) : (x.Price - x.Discount) * x.Quantity),
                TotalDebtBeforeVat = model.ItemDetail.Where(x => x.PaymentMethodCode == "PayCredit").Sum(x => x.DiscountType == "P" ? (x.Price * (1 - x.Discount / 100) * x.Quantity) : (x.Price - x.Discount) * x.Quantity),
                TotalDebtGuaranteeBeforeVat = model.ItemDetail.Where(x => x.PaymentMethodCode == "PayGuarantee").Sum(x => x.DiscountType == "P" ? (x.Price * (1 - x.Discount / 100) * x.Quantity) : (x.Price - x.Discount) * x.Quantity),

                BonusCommited = bonusCommited,
                TotalDebt = TotalDebt,
                TotalDebtGuarantee = TotalDebtGuarantee
            };
            if (TotalPayNow != 0)
                model.PaymentInfo.TotalPayNow = TotalPayNow;
            
            if (totalBonusPaynow > 0)
            {
                if(model.PayNowAmount != null && model.PayNowAmount != 0)
                {
                    model.PaymentInfo.BonusAmount = (model.PayNowAmount ?? 0) - vatPromotion;
                }
                else
                {
                    model.PaymentInfo.BonusAmount = totalBonusPaynow - vatPromotion;
                }
                model.PaymentInfo.BonusPercent = bonusDis / 100;
            }

            //}
            if (model.PaymentInfo.TotalPayNow + model.PaymentInfo.TotalDebtGuarantee + model.PaymentInfo.TotalDebt != 0)
                model.PaymentInfo.TotalAfterVat = model.PaymentInfo.TotalPayNow +
                                                  model.PaymentInfo.TotalDebtGuarantee +
                                                  model.PaymentInfo.TotalDebt;
            else
                model.PaymentInfo.TotalAfterVat = model.PaymentInfo.TotalBeforeVat + model.PaymentInfo.VATAmount;
            if (commitedTracking != null)
            {
                if (commitedTracking.CommittedTrackingLine != null)
                    foreach (var check in commitedTracking.CommittedTrackingLine.Where(e => e.IsCheck == false || e.CommittedType.IsNullOrEmpty()).ToList())
                    {
                        commitedTracking.CommittedTrackingLine.Remove(check);
                    }
            }
            return (model.PaymentInfo, commited1, commitedTracking);
        }
        public async Task<(PaymentInfo, Committed, CommittedTracking)> GetPaymentInfoNET(PriceDocCheck model)
        {
            double total = 0;
            double TotalPayNow = 0;
            double TotalDebt = 0;
            double TotalDebtGuarantee = 0;

            double TotalDiscount = 0;
            var bp = await _context.BP.Include(p => p.Groups).AsNoTracking().FirstOrDefaultAsync(b => b.Id == model.CardId);
            if (bp == null)
            {
                throw new KeyNotFoundException("Không tìm thấy nhà phân phối");
            }
            Committed commited1 = null;
            CommittedTracking commitedTracking = null;
            //commited1 = await _context.Committed.AsNoTracking().AsSplitQuery()
            //    .Include(committed => committed.CommittedLine)
            //    .ThenInclude(committedLine => committedLine.CommittedLineSub)
            //    .ThenInclude(committedLineSub => committedLineSub.Brand)
            //    .Include(committed => committed.CommittedLine)
            //    .ThenInclude(committedLine => committedLine.CommittedLineSub)
            //    .ThenInclude(committedLineSub => committedLineSub.ItemTypes)
            //    .Include(committed => committed.CommittedLine)
            //    .ThenInclude(committedLine => committedLine.CommittedLineSub)
            //    .ThenInclude(committedLineSub => committedLineSub.Industry)
            //    .Include(committed => committed.CommittedLine)
            //    .ThenInclude(committedLine => committedLine.CommittedLineSub)
            //    .ThenInclude(c => c.CommittedLineSubSub)
            //    .FirstOrDefaultAsync(p => p.CommittedYear!.Value.Year == DateTime.Now.Year && p.DocStatus == "A" && p.CardId == model.CardId);
            List<int> ints = new List<int>();
            List<int> intsPay = new List<int>();
            List<int> intsPay1 = new List<int>();
            
            if (commited1 == null)
                goto track;
            //commitedTracking = _context.CommittedTracking.AsNoTracking().AsSplitQuery()
            //       .Include(p => p.CommittedTrackingLine).FirstOrDefault(e => e.CommittedId == commited1.Id);
           
            //    if (commitedTracking == null)
            //        Commited(ref commitedTracking, commited1, model);
            //    else
            //        CommitedTracking(ref commitedTracking, commited1, model);

        track:
            double bonusCommited = 0;
            var listID = _context.CommittedTrackingLine.Where(e => e.IsCommitted == true).Select(e => e.Id);
            if (commitedTracking != null)
            {
                var checksss = commitedTracking.CommittedTrackingLine.Where(e => !listID.Contains(e.Id)).ToList();
                bonusCommited = checksss.Sum(e => e.BonusAddApplied ?? 0);
            }

            var bonusDis = 0.0;
            var BonusVolumn = 0.0;
            var PromotionTax = 0.0;
            if (bp.IsBusinessHouse == true)
            {
                var pay = _context.PaymentRule.FirstOrDefault(e => e.Name == "HỘ KINH DOANH");
                double.TryParse(pay.BonusPaynow.ToString(), out bonusDis);
                double.TryParse(pay.BonusVolumn.ToString(), out BonusVolumn);
                double.TryParse(pay.PromotionTax.ToString(), out PromotionTax);
            }
            else
            {
                var pay = _context.PaymentRule.FirstOrDefault(e => e.Name != "HỘ KINH DOANH");
                double.TryParse(pay.BonusPaynow.ToString(), out bonusDis);
                double.TryParse(pay.BonusVolumn.ToString(), out BonusVolumn);
                double.TryParse(pay.PromotionTax.ToString(), out PromotionTax);
            }

            double vatPromotion = 0;
            var promItem = await _context.Item.AsNoTracking()
                .Where(p => model.Promotion.Select(x => x.ItemCode).ToList().Contains(p.ItemCode))
                .Include(p => p.TaxGroups)
                .ToDictionaryAsync(i => i.ItemCode);

            if (bp.IsBusinessHouse == true)
            {
                bonusCommited *= BonusVolumn / 100;

                if (promItem.Count > 0)
                {
                    model.Promotion.ForEach(p =>
                    {
                        if (promItem.TryGetValue(p.ItemCode, out var prom))
                        {
                            p.Price = prom.Price ?? 0;
                            var tax = 0;// prom.TaxGroups?.Rate ?? 0;
                            if (p.QuantityAdd != null)
                                vatPromotion += (p.Price * p.QuantityAdd.Value * (1 + (double)tax / 100) * (PromotionTax / 100));
                        }
                    });
                }
            }

            var totalD = model.ItemDetail.Where(p => p.PaymentMethodCode == "PayNow")
                .Sum(x => x.DiscountType == "P" ? (x.Price * (1 - x.Discount / 100) * x.Quantity) : (x.Price - x.Discount) * x.Quantity);

            var totalBonusPaynow = model.PayNowAmount;
            int checkCount = 0;

            checkCount++;
            model.ItemDetail.ForEach(
                p =>
                {
                    p.PriceAfterDist = p.DiscountType == "P" ? (p.Price * (1 - p.Discount / 100)) : (p.Price -p.Discount);
                    p.LineTotal = p.PriceAfterDist * p.Quantity;

                    if (p.PaymentMethodCode == "PayNow")
                        TotalPayNow += p.LineTotal * (1 + p.VAT / 100);
                    if (p.PaymentMethodCode == "PayCredit")
                        TotalDebt += p.LineTotal * (1 + p.VAT / 100);

                    if (p.PaymentMethodCode == "PayGuarantee")
                        TotalDebtGuarantee += p.LineTotal * (1 + p.VAT / 100);
                });

            model.PaymentInfo = new PaymentInfo
            {
                VATAmount = model.ItemDetail.Sum(p => p.VATAmount ?? 0),

                PaynowVATAmount = model.ItemDetail.Where(x => x.PaymentMethodCode == "PayNow").Sum(p => p.VATAmount ?? 0),
                DebtVATAmount = model.ItemDetail.Where(x => x.PaymentMethodCode == "PayCredit").Sum(p => p.VATAmount ?? 0),
                DebtGuaranteeVATAmount = model.ItemDetail.Where(x => x.PaymentMethodCode == "PayGuarantee").Sum(p => p.VATAmount ?? 0),

                TotalBeforeVat = model.ItemDetail.Sum(x => x.DiscountType == "P" ? (x.Price * (1 - x.Discount / 100) * x.Quantity) : (x.Price - x.Discount) * x.Quantity),

                TotalPayNowBeforeVat = model.ItemDetail.Where(x => x.PaymentMethodCode == "PayNow").Sum(x => x.DiscountType == "P" ? (x.Price * (1 - x.Discount / 100) * x.Quantity) : (x.Price - x.Discount) * x.Quantity),
                TotalDebtBeforeVat = model.ItemDetail.Where(x => x.PaymentMethodCode == "PayCredit").Sum(x => x.DiscountType == "P" ? (x.Price * (1 - x.Discount / 100) * x.Quantity) : (x.Price - x.Discount) * x.Quantity),
                TotalDebtGuaranteeBeforeVat = model.ItemDetail.Where(x => x.PaymentMethodCode == "PayGuarantee").Sum(x => x.DiscountType == "P" ? (x.Price * (1 - x.Discount / 100) * x.Quantity) : (x.Price - x.Discount) * x.Quantity),

                BonusCommited = bonusCommited,
                TotalDebt = TotalDebt,
                TotalDebtGuarantee = TotalDebtGuarantee
            };
            if (TotalPayNow != 0)
                model.PaymentInfo.TotalPayNow = TotalPayNow;
            if (totalBonusPaynow > 0)
            {
                model.PaymentInfo.BonusAmount = totalBonusPaynow - vatPromotion ?? 0;
                model.PaymentInfo.BonusPercent = bonusDis / 100;
            }

            //}
            model.PaymentInfo.TotalAfterVat = model.PaymentInfo.TotalPayNow +
                                                  model.PaymentInfo.TotalDebtGuarantee +
                                                  model.PaymentInfo.TotalDebt;
            if (commitedTracking != null)
            {
                if (commitedTracking.CommittedTrackingLine != null)
                    foreach (var check in commitedTracking.CommittedTrackingLine.Where(e => e.IsCheck == false || e.CommittedType.IsNullOrEmpty()).ToList())
                    {
                        commitedTracking.CommittedTrackingLine.Remove(check);
                    }
            }
            return (model.PaymentInfo, commited1, commitedTracking);
        }
        public void Commited(ref CommittedTracking commitedTracking, Committed commited1, List<Promotion> promotions, Promotion promotion, PriceDocCheck model, List<ItemTypeKTSL> itemTypeKTSL, List<ItemUnit> itemUnit, List<int> ints)
        {
            commitedTracking = new CommittedTracking();
            commitedTracking.CommittedId = commited1.Id;
            List<CommittedTrackingLine> list = new List<CommittedTrackingLine>();
            foreach (var cmt in commited1.CommittedLine)
            {
                var line = new CommittedTrackingLine();
                line.OrderDate = DateTime.Now;

                line.DocId = 0;
                int FlagM = 0;
                if (cmt.CommittedType == "Q")
                {

                    DateTime date = DateTime.Now;
                    if (new[] { 1, 2, 3 }.Contains(date.Month))
                        line.CommittedType = "Q1";
                    if (new[] { 4, 5, 6 }.Contains(date.Month))
                        line.CommittedType = "Q2";
                    if (new[] { 8, 9, 7 }.Contains(date.Month))
                        line.CommittedType = "Q3";
                    if (new[] { 11, 12, 10 }.Contains(date.Month))
                        line.CommittedType = "Q4";
                    FlagM++;
                }
                if (cmt.CommittedType == "Y")
                {
                    line.CommittedType = "M" + DateTime.Now.Month;
                }
                foreach (var lineSub in cmt.CommittedLineSub.ToList())
                {
                    line.CommittedLineSubId = lineSub.Id;
                    var lisr = lineSub.ItemTypes.Select(e => e.Id).ToList();
                    var listBrand = lineSub.Brand.Select(e => e.Id).ToList();
                    var brandId = promotions.SelectMany(p => p.PromotionBrand).Select(p => p.BrandId).ToList();
                    var industryId = promotions.SelectMany(p => p.PromotionIndustry).Select(p => p.IndustryId).ToList();
                    List<Item> listIgnore = new List<Item>();
                    List<Item> listI = new List<Item>();
                    List<Item> listIPro = new List<Item>();
                    var listItemss = model.ItemDetail.Where(e=>ints.Contains(e.LineId)).Select(e => e.ItemId);
                    var listIt = model.ItemDetail.Where(e=> listItemss.Contains(e.ItemId)).Select(e => e.ItemId);
                    var listIgPromotion = model.ItemDetail.Where(e => !listItemss.Contains(e.ItemId)).Select(e => e.ItemId);

                    var itemTypeI = itemTypeKTSL.Where(e => e.ItemType == "I" && e.AddAccumulate == true).Select(e => e.ItemId).ToList();
                    var itemTypeGI = itemTypeKTSL.Where(e => e.ItemType == "G" && e.AddAccumulate == true).Select(e => e.ItemId).ToList();
                    var unitI = itemUnit.Where(e => e.AddAccumulate == true).Select(e => e.UomId).ToList();
                    listIgnore = _context.Item.Include(e => e.Packing).Where(p => lisr.Contains(p.ItemTypeId ?? 0) && listIt.Contains(p.Id)
                                && p.IndustryId == lineSub.IndustryId && listBrand.Contains(p.BrandId ?? 0)
                                && brandId.Contains(p.BrandId ?? 0) && industryId.Contains(p.IndustryId ?? 0)
                                && ((itemTypeI.Count > 0 && itemTypeI.Contains(p.Id)) || (itemTypeGI.Count > 0 && itemTypeGI.Contains(p.ItemTypeId ?? 00)))
                                && (unitI.Count > 0 && unitI.Contains(p.PackingId ?? 0))
                                ).ToList();

                    listI = _context.Item.Include(e => e.Packing).Where(p => lisr.Contains(p.ItemTypeId ?? 0) && listIt.Contains(p.Id)
                               && brandId.Contains(p.BrandId ?? 0) && industryId.Contains(p.IndustryId ?? 0)
                               && !listIgnore.Select(e => e.Id).Contains(p.Id)
                               ).ToList();
                    listIPro = _context.Item.Include(e => e.Packing).Where(p => lisr.Contains(p.ItemTypeId ?? 0) && listIgPromotion.Contains(p.Id)
                                && p.IndustryId == lineSub.IndustryId && listBrand.Contains(p.BrandId ?? 0)).ToList();
                    var totalVolumn = from detail in model.ItemDetail
                                      join item in listI on detail.ItemId equals item.Id
                                      where detail.ItemId == item.Id
                                      select new
                                      {
                                          Volumn = detail.Quantity * item.Packing.Volumn,
                                          Bonus = detail.Quantity * detail.Price
                                      };
                    var totalVolumnIgnore = from detail in model.ItemDetail
                                            join item in listIgnore on detail.ItemId equals item.Id
                                            where detail.ItemId == item.Id
                                            select new
                                            {
                                                Volumn = detail.Quantity * item.Packing.Volumn,
                                                Bonus = detail.Quantity * detail.Price
                                            };
                    var totalVolumnPro = from detail in model.ItemDetail
                                            join item in listIPro on detail.ItemId equals item.Id
                                            where detail.ItemId == item.Id
                                            select new
                                            {
                                                Volumn = detail.Quantity * item.Packing.Volumn,
                                                Bonus = detail.Quantity * detail.Price
                                            };
                    double totalVolumnT = (totalVolumn.Sum(p => p.Volumn) ?? 0);
                    double totalVolumnTPro = (totalVolumnPro.Sum(p => p.Volumn) ?? 0);
                    double totalVolumnTIgnore = (totalVolumnIgnore.Sum(p => p.Volumn) ?? 0);
                    lineSub.CurrentVolumn = 0;
                    lineSub.Package = (lineSub.Package ?? 0) + totalVolumnT + totalVolumnTPro + totalVolumnTIgnore;
                    lineSub.AfterVolumn = (lineSub.Package ?? 0);
                    var isOtherDist = promotion.HasException == false ? promotion.IsOtherDist : promotion.IsOtherDistExc;
                    double totalBonus = totalVolumn.Sum(p => p.Bonus);
                    double totalBonusIgnore = totalVolumnIgnore.Sum(p => p.Bonus);
                    double totalBonusPro = totalVolumnPro.Sum(p => p.Bonus);
                    ProcessCommittedMonth(line, lineSub, isOtherDist, totalVolumnT, totalVolumnTIgnore, totalVolumnTPro, totalBonus, totalBonusIgnore, totalBonusPro);
                    ProcessCommittedQuater(line, lineSub, isOtherDist, totalVolumnT, totalVolumnTIgnore, totalVolumnTPro, totalBonus, totalBonusIgnore, totalBonusPro);
                    list.Add(line);
                    if (DateTime.Now.Month <= 3)
                    {
                        line = new CommittedTrackingLine();
                        line.OrderDate = DateTime.Now;
                        line.CommittedLineSubId = lineSub.Id;
                        line.DocId = 0;
                        double totalM = 0;
                        totalM = (lineSub.Month1 ?? 0) + (lineSub.Month2 ?? 0) + (lineSub.Month3 ?? 0);
                        double Addbonus = 0.0, bonus = 0.0;
                        var discount = (double)(lineSub.ThreeMonthDiscount ?? 0);
                        if (totalVolumnT + totalVolumnTIgnore + totalVolumnTPro >= totalM)
                        {
                           
                            if (isOtherDist)
                            {
                                Addbonus = ((totalBonus + totalBonusIgnore) * discount) / 100.0;
                                bonus = ((totalBonus) * discount) / 100.0;
                            }
                            else
                            {
                                Addbonus = 0;
                                bonus = 0;
                            }
                            Addbonus = Addbonus + ((totalBonusPro) * discount) / 100.0;
                            bonus = bonus + ((totalBonusPro) * discount) / 100.0;
                            lineSub.ThreeMonthBonus = bonus;
                        }
                        else
                            line.IsCommitted = false;
                        line.BonusAddApplied = Addbonus;
                        if (line.IsCommitted == false && isOtherDist)
                        {
                            bonus = ((totalBonus + totalBonusPro) * discount) / 100.0;
                            line.BonusApplied = bonus;
                        }
                        else
                            line.BonusApplied = bonus;
                        line.Volume = totalVolumnT + totalVolumnTPro;
                        line.CommittedType = "3T";
                        list.Add(line);
                    }
                    if (DateTime.Now.Month <= 6)
                    {
                        line = new CommittedTrackingLine();
                        line.OrderDate = DateTime.Now;
                        line.CommittedLineSubId = lineSub.Id;
                        line.DocId = 0;
                        double totalM = 0;
                        if (FlagM > 0)
                            totalM = (lineSub.Quarter1 ?? 0) + (lineSub.Quarter2 ?? 0);
                        else
                            totalM = (lineSub.Month1 ?? 0) + (lineSub.Month2 ?? 0) + (lineSub.Month3 ?? 0) + (lineSub.Month4 ?? 0) + (lineSub.Month5 ?? 0) + (lineSub.Month6 ?? 0); ;
                        
                        double Addbonus = 0.0, bonus = 0.0;
                        var discount = (double)(lineSub.SixMonthDiscount ?? 0);
                        if (totalVolumnT + totalVolumnTIgnore + totalVolumnTPro >= totalM)
                        {
                            
                            if (isOtherDist)
                            {
                                Addbonus = ((totalBonus + totalBonusIgnore) * discount) / 100.0;
                                bonus = ((totalBonus) * discount) / 100.0;
                            }
                            else
                            {
                                Addbonus = 0;
                                bonus = 0;
                            }
                            Addbonus = Addbonus + ((totalBonusPro) * discount) / 100.0;
                            bonus = bonus + ((totalBonusPro) * discount) / 100.0;
                            lineSub.SixMonthBonus = bonus;
                        }
                        else
                        {
                            line.IsCommitted = false;
                        } 
                        line.BonusAddApplied = Addbonus;
                        if (line.IsCommitted == false && isOtherDist)
                        {
                            bonus = ((totalBonus + totalBonusPro) * discount) / 100.0;
                            line.BonusApplied = bonus;
                        }
                        else
                            line.BonusApplied = bonus;
                        line.Volume = totalVolumnT + totalVolumnTPro;
                        line.CommittedType = "6T";
                        list.Add(line);
                    }
                    if (DateTime.Now.Month <= 9)
                    {
                        line = new CommittedTrackingLine();
                        line.OrderDate = DateTime.Now;
                        line.CommittedLineSubId = lineSub.Id;
                        line.DocId = 0;
                        double totalM = 0;
                        if (FlagM > 0)
                            totalM = (lineSub.Quarter1 ?? 0) + (lineSub.Quarter2 ?? 0) + (lineSub.Quarter3 ?? 0);
                        else
                            totalM = (lineSub.Month1 ?? 0) + (lineSub.Month2 ?? 0) + (lineSub.Month3 ?? 0) + (lineSub.Month4 ?? 0) + (lineSub.Month5 ?? 0) + (lineSub.Month6 ?? 0)
                                + (lineSub.Month7 ?? 0) + (lineSub.Month8 ?? 0) + (lineSub.Month9 ?? 0);
                        double Addbonus = 0.0, bonus = 0.0;
                        var discount = (double)(lineSub.NineMonthDiscount ?? 0);
                        if (totalVolumnT + totalVolumnTIgnore + totalVolumnTPro >= totalM)
                        {

                            if (isOtherDist)
                            {
                                Addbonus = ((totalBonus + totalBonusIgnore) * discount) / 100.0;
                                bonus = ((totalBonus) * discount) / 100.0;
                            }
                            else
                            {
                                Addbonus = 0;
                                bonus = 0;
                            }
                            Addbonus = Addbonus + ((totalBonusPro) * discount) / 100.0;
                            bonus = bonus + ((totalBonusPro) * discount) / 100.0;
                            lineSub.NineMonthBonus = bonus;
                        }
                        else
                        {
                            bonus = ((totalBonus + totalBonusPro) * discount) / 100.0;
                            line.IsCommitted = false;
                        }
                        
                        line.BonusAddApplied = Addbonus;
                        if (line.IsCommitted == false && isOtherDist)
                        {
                            bonus = ((totalBonus +totalBonusPro) * discount) / 100.0;
                            line.BonusApplied = bonus;
                        }
                        else
                            line.BonusApplied = bonus;
                        line.Volume = totalVolumnT + totalVolumnTPro;
                        line.CommittedType = "9T";
                        list.Add(line);
                    }
                    int checkover = 0, Flag = 0;
                    checkover = lineSub.CommittedLineSubSub.Count();
                    foreach (var over in lineSub.CommittedLineSubSub.ToList().OrderByDescending(e => e.OutPut))
                    {
                            line = new CommittedTrackingLine();
                            line.OrderDate = DateTime.Now;
                            line.CommittedLineSubId = lineSub.Id;
                            line.DocId = 0;
                            double Addbonus = 0.0, bonus = 0.0;
                            var discount = (double)(over.Discount);
                            if (totalVolumnT + totalVolumnTIgnore + totalVolumnTPro >= over.OutPut)
                            {
                                Flag++;
                                if (isOtherDist)
                                {
                                    Addbonus = ((totalBonus + totalBonusIgnore) * discount) / 100.0;
                                    bonus = ((totalBonus) * discount) / 100.0;
                                }
                                else
                                {
                                    Addbonus = 0;
                                    bonus = 0;
                                }
                                Addbonus = Addbonus + ((totalBonusPro) * discount) / 100.0;
                                bonus = bonus + ((totalBonusPro) * discount) / 100.0;
                                over.BonusTotal = bonus;
                            }
                            else
                            {
                                line.IsCommitted = false;
                            }
                            over.Total = totalVolumnT + totalVolumnTPro;
                            line.BonusAddApplied = Addbonus;
                            if (line.IsCommitted == false && isOtherDist)
                            {
                                bonus = ((totalBonus + totalBonusPro) * discount) / 100.0;
                                line.BonusApplied = bonus;
                            }
                            else
                                line.BonusApplied = bonus;
                            line.Volume = totalVolumnT + totalVolumnTPro;
                            line.CommittedType = "OV" + checkover;
                            checkover--;
                            list.Add(line);
                    }
                    if (Flag == 0)
                    {
                        line = new CommittedTrackingLine();
                        line.OrderDate = DateTime.Now;
                        line.CommittedLineSubId = lineSub.Id;
                        line.DocId = 0;
                        double Addbonus = 0.0, bonus = 0.0;
                        
                        var discount = (double)(lineSub.DiscountYear ?? 0);
                        if (totalVolumnT + totalVolumnTIgnore + totalVolumnTPro >= lineSub.Total)
                        {

                            if (isOtherDist)
                            {
                                Addbonus = ((totalBonus + totalBonusIgnore) * discount) / 100.0;
                                bonus = ((totalBonus) * discount) / 100.0;
                            }
                            else
                            {
                                Addbonus = 0;
                                bonus = 0;
                            }
                            Addbonus = Addbonus + ((totalBonusPro) * discount) / 100.0;
                            bonus = bonus + ((totalBonusPro) * discount) / 100.0;
                            lineSub.YearBonus = bonus;
                        }
                        else
                        {
                            line.IsCommitted = false;
                        }
                        lineSub.Total12M = totalVolumnT + totalVolumnTPro;
                        line.BonusAddApplied = Addbonus;
                        if (line.IsCommitted == false && isOtherDist)
                        {
                            bonus = ((totalBonus + totalBonusPro) * discount) / 100.0;
                            line.BonusApplied = bonus;
                        }
                        else
                            line.BonusApplied = bonus;
                        line.Volume = totalVolumnT+ totalVolumnTPro;
                        line.CommittedType = "12T";
                        list.Add(line);
                    }
                }
                commitedTracking.CommittedTrackingLine = list;
            }
        }
        public void CommitedTracking(ref CommittedTracking commitedTracking, Committed commited1, List<Promotion> promotions, Promotion promotion, PriceDocCheck model, List<ItemTypeKTSL> itemTypeKTSL, List<ItemUnit> itemUnit, List<int> ints)
        {
            List<CommittedTrackingLine> list = new List<CommittedTrackingLine>();
            foreach (var cmt in commited1.CommittedLine)
            {
                var line = new CommittedTrackingLine();
                line.OrderDate = DateTime.Now;

                line.DocId = 0;
                int FlagM = 0;
                if (cmt.CommittedType == "Q")
                {

                    DateTime date = DateTime.Now;
                    if (new[] { 1, 2, 3 }.Contains(date.Month))
                        line.CommittedType = "Q1";
                    if (new[] { 4, 5, 6 }.Contains(date.Month))
                        line.CommittedType = "Q2";
                    if (new[] { 8, 9, 7 }.Contains(date.Month))
                        line.CommittedType = "Q3";
                    if (new[] { 11, 12, 10 }.Contains(date.Month))
                        line.CommittedType = "Q4";
                    FlagM++;
                }
                if (cmt.CommittedType == "Y")
                {
                    line.CommittedType = "M" + DateTime.Now.Month;
                }
                foreach (var lineSub in cmt.CommittedLineSub.ToList())
                {
                    line.CommittedLineSubId = lineSub.Id;
                    var lisr = lineSub.ItemTypes.Select(e => e.Id).ToList();
                    var listBrand = lineSub.Brand.Select(e => e.Id).ToList();
                    var brandId = promotions.SelectMany(p => p.PromotionBrand).Select(p => p.BrandId).ToList();
                    var industryId = promotions.SelectMany(p => p.PromotionIndustry).Select(p => p.IndustryId).ToList();
                    List<Item> listIgnore = new List<Item>();
                    List<Item> listI = new List<Item>();
                    List<Item> listIPro = new List<Item>();
                    var listItemss = model.ItemDetail.Where(e => ints.Contains(e.LineId)).Select(e => e.ItemId);
                    var listIt = model.ItemDetail.Where(e => listItemss.Contains(e.ItemId)).Select(e => e.ItemId);
                    var listIgPromotion = model.ItemDetail.Where(e => !listItemss.Contains(e.ItemId)).Select(e => e.ItemId);

                    var itemTypeI = itemTypeKTSL.Where(e => e.ItemType == "I" && e.AddAccumulate == true).Select(e => e.ItemId).ToList();
                    var itemTypeGI = itemTypeKTSL.Where(e => e.ItemType == "G" && e.AddAccumulate == true).Select(e => e.ItemId).ToList();
                    var unitI = itemUnit.Where(e => e.AddAccumulate == true).Select(e => e.UomId).ToList();
                    listIgnore = _context.Item.Include(e => e.Packing).Where(p => lisr.Contains(p.ItemTypeId ?? 0) && listIt.Contains(p.Id)
                                && p.IndustryId == lineSub.IndustryId && listBrand.Contains(p.BrandId ?? 0)
                                && brandId.Contains(p.BrandId ?? 0) && industryId.Contains(p.IndustryId ?? 0)
                                && ((itemTypeI.Count > 0 && itemTypeI.Contains(p.Id)) || (itemTypeGI.Count > 0 && itemTypeGI.Contains(p.ItemTypeId ?? 00)))
                                && (unitI.Count > 0 && unitI.Contains(p.PackingId ?? 0))
                                ).ToList();

                    listI = _context.Item.Include(e => e.Packing).Where(p => lisr.Contains(p.ItemTypeId ?? 0) && listIt.Contains(p.Id)
                               && brandId.Contains(p.BrandId ?? 0) && industryId.Contains(p.IndustryId ?? 0)
                               && !listIgnore.Select(e => e.Id).Contains(p.Id)
                               ).ToList();
                    listIPro = _context.Item.Include(e => e.Packing).Where(p => lisr.Contains(p.ItemTypeId ?? 0) && listIgPromotion.Contains(p.Id)
                                && p.IndustryId == lineSub.IndustryId && listBrand.Contains(p.BrandId ?? 0)).ToList();
                    var totalVolumn = from detail in model.ItemDetail
                                      join item in listI on detail.ItemId equals item.Id
                                      where detail.ItemId == item.Id
                                      select new
                                      {
                                          Volumn = detail.Quantity * item.Packing.Volumn,
                                          Bonus = detail.Quantity * detail.Price
                                      };
                    var totalVolumnIgnore = from detail in model.ItemDetail
                                            join item in listIgnore on detail.ItemId equals item.Id
                                            where detail.ItemId == item.Id
                                            select new
                                            {
                                                Volumn = detail.Quantity * item.Packing.Volumn,
                                                Bonus = detail.Quantity * detail.Price
                                            };
                    var totalVolumnPro = from detail in model.ItemDetail
                                         join item in listIPro on detail.ItemId equals item.Id
                                         where detail.ItemId == item.Id
                                         select new
                                         {
                                             Volumn = detail.Quantity * item.Packing.Volumn,
                                             Bonus = detail.Quantity * detail.Price
                                         };
                    double totalVolumnT = (totalVolumn.Sum(p => p.Volumn) ?? 0);
                    double totalVolumnTPro = (totalVolumnPro.Sum(p => p.Volumn) ?? 0);
                    double totalVolumnTIgnore = (totalVolumnIgnore.Sum(p => p.Volumn) ?? 0);
                    lineSub.CurrentVolumn = (lineSub.Package ?? 0);
                    lineSub.Package = (lineSub.Package ?? 0) + totalVolumnT + totalVolumnTPro + totalVolumnTIgnore;
                    lineSub.AfterVolumn = lineSub.Package ?? 0;
                    var isOtherDist = promotion.HasException == false ? promotion.IsOtherDist : promotion.IsOtherDistExc;
                    double totalBonus = totalVolumn.Sum(p => p.Bonus);
                    double totalBonusIgnore = totalVolumnIgnore.Sum(p => p.Bonus);
                    double totalBonusPro = totalVolumnPro.Sum(p => p.Bonus);
                    List<int> listDocl;
                    double Addbonus = 0, bonus = 0;
                    ProcessMonth(commitedTracking, lineSub, line, isOtherDist, totalVolumnT, totalVolumnTIgnore, totalVolumnTPro, totalBonus, totalBonusIgnore, totalBonusPro);
                    ProcessQuarter(commitedTracking, lineSub, line, isOtherDist, totalVolumnT, totalVolumnTIgnore, totalVolumnTPro,totalBonus, totalBonusIgnore, totalBonusPro);
                    list.Add(line);
                    if (DateTime.Now.Month <= 3)
                    {
                        line = new CommittedTrackingLine();
                        line.OrderDate = DateTime.Now;
                        line.CommittedLineSubId = lineSub.Id;
                        line.DocId = 0;
                        double totalM = 0;
                        totalM = (lineSub.Month1 ?? 0) + (lineSub.Month2 ?? 0) + (lineSub.Month3 ?? 0);
                        List<int> l = new List<int> { 1, 2, 3 };
                        var M = commitedTracking.CommittedTrackingLine.Where(e => l.Contains(e.OrderDate.Month) && e.CommittedType == "3T").ToList();
                        double totalVolumnOver = M.Sum(e => e.Volume);
                        var discount = (double)(lineSub.ThreeMonthDiscount ?? 0);
                        Addbonus = 0; bonus = 0;
                        if (totalVolumnT + totalVolumnOver + totalVolumnTIgnore + totalVolumnTPro >= totalM)
                        {
                            if (isOtherDist)
                            {
                                Addbonus = ((totalBonus + totalBonusIgnore) * discount) / 100.0;
                                bonus = ((totalBonus) * discount) / 100.0;
                            }
                            else
                            {
                                Addbonus = 0;
                                bonus = 0;
                            }
                            Addbonus = Addbonus + ((totalBonusPro) * discount) / 100.0;
                            bonus = bonus + ((totalBonusPro) * discount) / 100.0;
                            lineSub.ThreeMonthBonus = bonus;
                            M.Where(e => e.IsCommitted == false).ToList().ForEach(e => e.BonusAddApplied = e.BonusApplied);
                            M.Where(e => e.IsCommitted == false).ToList().ForEach(e => e.IsCommitted = true);
                            
                        }
                        else
                            line.IsCommitted = false;
                        line.BonusAddApplied = Addbonus;
                        if (line.IsCommitted == false && isOtherDist)
                        {
                            bonus = ((totalBonus) * discount) / 100.0;
                            line.BonusApplied = bonus;
                        }
                        else
                            line.BonusApplied = bonus;
                        line.Volume = totalVolumnT + totalVolumnTPro;
                        line.CommittedType = "3T";
                        list.Add(line);
                    }
                    if (DateTime.Now.Month <= 6)
                    {
                        line = new CommittedTrackingLine();
                        line.OrderDate = DateTime.Now;
                        line.CommittedLineSubId = lineSub.Id;
                        line.DocId = 0;
                        double totalM = 0;
                        if (FlagM > 0)
                            totalM = (lineSub.Quarter1 ?? 0) + (lineSub.Quarter2 ?? 0);
                        else
                            totalM = (lineSub.Month1 ?? 0) + (lineSub.Month2 ?? 0) + (lineSub.Month3 ?? 0) + (lineSub.Month4 ?? 0) + (lineSub.Month5 ?? 0) + (lineSub.Month6 ?? 0); ;
                        List<int> l = new List<int> { 1, 2, 3, 4, 5, 6 };
                        var M = commitedTracking.CommittedTrackingLine.Where(e => l.Contains(e.OrderDate.Month) && e.CommittedType == "6T").ToList();
                        double totalVolumnOver = M.Sum(e => e.Volume);
                        var discount = (double)(lineSub.SixMonthDiscount ?? 0);
                        Addbonus = 0; bonus = 0;
                        if (totalVolumnT + totalVolumnOver + totalVolumnTIgnore + totalVolumnTPro >= totalM)
                        {
                            if (isOtherDist)
                            {
                                Addbonus = ((totalBonus + totalBonusIgnore) * discount) / 100.0;
                                bonus = ((totalBonus) * discount) / 100.0;
                            }
                            else
                            {
                                Addbonus = 0;
                                bonus = 0;
                            }
                            Addbonus = Addbonus + ((totalBonusPro) * discount) / 100.0;
                            bonus = bonus + ((totalBonusPro) * discount) / 100.0;
                            lineSub.SixMonthBonus = bonus;
                            M.Where(e => e.IsCommitted == false).ToList().ForEach(e => e.BonusAddApplied = e.BonusApplied);
                            M.Where(e => e.IsCommitted == false).ToList().ForEach(e => e.IsCommitted = true);
                            
                        }
                        else
                            line.IsCommitted = false;
                        line.BonusAddApplied = Addbonus;
                        if (line.IsCommitted == false && isOtherDist)
                        {
                            bonus = ((totalBonus) * discount) / 100.0;
                            line.BonusApplied = bonus;
                        }
                        else
                            line.BonusApplied = bonus;
                        line.Volume = totalVolumnT + totalVolumnTPro;
                        line.CommittedType = "6T";
                        list.Add(line);
                    }
                    if (DateTime.Now.Month <= 9)
                    {
                        line = new CommittedTrackingLine();
                        line.OrderDate = DateTime.Now;
                        line.CommittedLineSubId = lineSub.Id;
                        line.DocId = 0;
                        double totalM = 0;
                        if (FlagM > 0)
                            totalM = (lineSub.Quarter1 ?? 0) + (lineSub.Quarter2 ?? 0) + (lineSub.Quarter3 ?? 0);
                        else
                            totalM = (lineSub.Month1 ?? 0) + (lineSub.Month2 ?? 0) + (lineSub.Month3 ?? 0) + (lineSub.Month4 ?? 0) + (lineSub.Month5 ?? 0) + (lineSub.Month6 ?? 0)
                                + (lineSub.Month7 ?? 0) + (lineSub.Month8 ?? 0) + (lineSub.Month9 ?? 0);
                        List<int> l = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
                        //listDocl = commitedTracking.CommittedTrackingLine.Where(e => l.Contains(e.OrderDate.Month) && e.CommittedType == "9T" && e.IsCommitted == true).Select(e => e.DocId).ToList();
                        var M = commitedTracking.CommittedTrackingLine.Where(e =>   l.Contains(e.OrderDate.Month) && e.CommittedType == "9T" ).ToList();
                        double totalVolumnOver = M.Sum(e => e.Volume);
                        var discount = (double)(lineSub.NineMonthDiscount ?? 0);
                        Addbonus = 0; bonus = 0;
                        if (totalVolumnT + totalVolumnOver + totalVolumnTIgnore + totalVolumnTPro >= totalM)
                        {
                           
                            if (isOtherDist)
                            {
                                Addbonus = ((totalBonus + totalBonusIgnore) * discount) / 100.0;
                                bonus = ((totalBonus) * discount) / 100.0;
                            }
                            else
                            {
                                Addbonus = 0;
                                bonus = 0;
                            }
                            Addbonus = Addbonus + ((totalBonusPro) * discount) / 100.0;
                            bonus = bonus + ((totalBonusPro) * discount) / 100.0;
                            lineSub.NineMonthBonus = bonus;
                            M.Where(e => e.IsCommitted == false).ToList().ForEach(e => e.BonusAddApplied = e.BonusApplied);
                            M.Where(e => e.IsCommitted == false).ToList().ForEach(e => e.IsCommitted = true);
                            
                        }
                        else
                            line.IsCommitted = false;
                        line.BonusAddApplied = Addbonus;
                        if (line.IsCommitted == false && isOtherDist)
                        {
                            bonus = ((totalBonus) * discount) / 100.0;
                            line.BonusApplied = bonus;
                        }
                        else
                            line.BonusApplied = bonus;
                        line.Volume = totalVolumnT + totalVolumnTPro;
                        line.CommittedType = "9T";
                        list.Add(line);
                    }
                    int checkover = 0, Flag = 0, isCheck = 0;
                    checkover = lineSub.CommittedLineSubSub.Count();
                    isCheck = checkover;
                    foreach (var over in lineSub.CommittedLineSubSub.ToList().OrderByDescending(e => e.OutPut))
                    {
                        line = new CommittedTrackingLine();
                        line.OrderDate = DateTime.Now;
                        line.CommittedLineSubId = lineSub.Id;
                        line.DocId = 0;
                        var M = commitedTracking.CommittedTrackingLine.Where(e => e.CommittedType == "OV" + checkover).ToList();
                        double totalVolumnOver = M.Sum(e => e.Volume);
                        var discount = (double)(over.Discount);
                        Addbonus = 0; bonus = 0;
                        if (totalVolumnT + totalVolumnOver + totalVolumnTIgnore + totalVolumnTPro >= over.OutPut)
                        {
                            Flag++;

                            if (isOtherDist)
                            {
                                Addbonus = ((totalBonus + totalBonusIgnore) * discount) / 100.0;
                                bonus = ((totalBonus) * discount) / 100.0;
                            }
                            else
                            {
                                Addbonus = 0;
                                bonus = 0;
                            }
                            Addbonus = Addbonus + ((totalBonusPro) * discount) / 100.0;
                            bonus = bonus + ((totalBonusPro) * discount) / 100.0;
                            over.BonusTotal = bonus;
                            M.Where(e => e.IsCommitted == false).ToList().ForEach(e => e.BonusAddApplied = e.BonusApplied);
                            M.Where(e => e.IsCommitted == false).ToList().ForEach(e => e.IsCommitted = true);

                        }
                        else
                            line.IsCommitted = false;
                        over.Total = totalVolumnT + totalVolumnTPro;
                        line.BonusAddApplied = Addbonus;
                        if (line.IsCommitted == false && isOtherDist)
                        {
                            bonus = ((totalBonus) * discount) / 100.0;
                            line.BonusApplied = bonus;
                        }
                        else
                        {
                            isCheck--;
                            line.BonusApplied = bonus;
                        }

                        line.Volume = totalVolumnT + totalVolumnTPro;
                        line.CommittedType = "OV" + checkover;
                        checkover--;
                        list.Add(line);
                        if (line.IsCommitted == true)
                            break;
                    }
                    if (Flag == 0)
                    {
                        line = new CommittedTrackingLine();
                        line.OrderDate = DateTime.Now;
                        line.CommittedLineSubId = lineSub.Id;
                        line.DocId = 0;
                        var M = commitedTracking.CommittedTrackingLine.Where(e =>  e.CommittedType == "12T" ).ToList();
                        double totalVolumnOver = M.Sum(e => e.Volume);
                        var discount = (double)(lineSub.DiscountYear ?? 0);
                        Addbonus = 0; bonus = 0;
                        if (totalVolumnT + totalVolumnOver + totalVolumnTIgnore + totalVolumnTPro >= lineSub.Total)
                        {
                            
                            if (isOtherDist)
                            {
                                Addbonus = ((totalBonus + totalBonusIgnore) * discount) / 100.0;
                                bonus = ((totalBonus) * discount) / 100.0;
                            }
                            else
                            {
                                Addbonus = 0;
                                bonus = 0;
                            }
                            Addbonus = Addbonus + ((totalBonusPro) * discount) / 100.0;
                            bonus = bonus + ((totalBonusPro) * discount) / 100.0;
                            lineSub.YearBonus = bonus;
                            M.Where(e => e.IsCommitted == false).ToList().ForEach(e => e.BonusAddApplied = e.BonusApplied);
                            M.Where(e => e.IsCommitted == false).ToList().ForEach(e => e.IsCommitted = true);
                            
                        }
                        else
                            line.IsCommitted = false;
                        lineSub.Total12M = totalVolumnT + totalVolumnTPro;
                        line.BonusAddApplied = Addbonus;
                        if (line.IsCommitted == false && isOtherDist)
                        {
                            bonus = ((totalBonus) * discount) / 100.0;
                            line.BonusApplied = bonus;
                        }
                        else
                            line.BonusApplied = bonus;
                        line.Volume = totalVolumnT + totalVolumnTPro;
                        line.CommittedType = "12T";
                        list.Add(line);
                    }
                }
                if (commitedTracking.CommittedTrackingLine.Count > 0)
                    foreach (var li in list)
                    {
                        commitedTracking.CommittedTrackingLine.Add(li);
                    }
                else
                    commitedTracking.CommittedTrackingLine = list;
            }
        }
        public void Commited(ref CommittedTracking commitedTracking, Committed commited1, PriceDocCheck model)
        {
            commitedTracking = new CommittedTracking();
            commitedTracking.CommittedId = commited1.Id;
            List<CommittedTrackingLine> list = new List<CommittedTrackingLine>();
            foreach (var cmt in commited1.CommittedLine)
            {
                var line = new CommittedTrackingLine();
                line.OrderDate = DateTime.Now;

                line.DocId = 0;
                int FlagM = 0;
                if (cmt.CommittedType == "Q")
                {

                    DateTime date = DateTime.Now;
                    if (new[] { 1, 2, 3 }.Contains(date.Month))
                        line.CommittedType = "Q1";
                    if (new[] { 4, 5, 6 }.Contains(date.Month))
                        line.CommittedType = "Q2";
                    if (new[] { 8, 9, 7 }.Contains(date.Month))
                        line.CommittedType = "Q3";
                    if (new[] { 11, 12, 10 }.Contains(date.Month))
                        line.CommittedType = "Q4";
                    FlagM++;
                }
                if (cmt.CommittedType == "Y")
                {
                    line.CommittedType = "M" + DateTime.Now.Month;
                }
                foreach (var lineSub in cmt.CommittedLineSub.ToList())
                {
                    line.CommittedLineSubId = lineSub.Id;
                    var lisr = lineSub.ItemTypes.Select(e => e.Id).ToList();
                    List<Item> listI = new List<Item>();
                    var listIt = model.ItemDetail.Select(e => e.ItemId);

                    listI = _context.Item.Include(e => e.Packing).Where(p => lisr.Contains(p.ItemTypeId ?? 0) && listIt.Contains(p.Id)
                               ).ToList();
                    var totalVolumn = from detail in model.ItemDetail
                                      join item in listI on detail.ItemId equals item.Id
                                      where detail.ItemId == item.Id
                                      select new
                                      {
                                          Volumn = detail.Quantity * item.Packing.Volumn,
                                          Bonus = detail.Quantity * detail.Price
                                      };


                    double totalVolumnT = (totalVolumn.Sum(p => p.Volumn) ?? 0);
                    lineSub.CurrentVolumn = 0;
                    lineSub.Package = (lineSub.Package ?? 0) + totalVolumnT;
                    lineSub.AfterVolumn = lineSub.Package ?? 0;
                    double totalBonus = totalVolumn.Sum(p => p.Bonus);
                    ProcessCommittedMonth(line, lineSub, true, totalVolumnT, 0,0, totalBonus, 0,0);
                    ProcessCommittedQuater(line, lineSub, true, totalVolumnT, 0,0, totalBonus, 0,0);
                    list.Add(line);
                    double Addbonus = 0.0, bonus = 0.0, totalVolumnTIgnore = 0, totalBonusIgnore = 0;
                    bool isOtherDist = true;
                    if (DateTime.Now.Month <= 3)
                    {
                        line = new CommittedTrackingLine();
                        line.OrderDate = DateTime.Now;
                        line.CommittedLineSubId = lineSub.Id;
                        line.DocId = 0;
                        double totalM = 0;
                        totalM = (lineSub.Month1 ?? 0) + (lineSub.Month2 ?? 0) + (lineSub.Month3 ?? 0);
                        if (totalVolumnT >= totalM)
                        {
                            line.IsCommitted = true;
                        }
                        else
                            line.IsCommitted = false;
                        Addbonus = 0.0; bonus = 0.0;
                        var discount = (double)(lineSub.ThreeMonthDiscount ?? 0);
                        if (totalVolumnT + totalVolumnTIgnore >= totalM)
                        {
                            if (isOtherDist)
                            {
                                Addbonus = ((totalBonus + totalBonusIgnore) * discount) / 100.0;
                                bonus = ((totalBonus) * discount) / 100.0;
                                line.IsCommitted = true;
                            }
                            else
                            {
                                line.IsCommitted = false;
                                Addbonus = 0;
                                bonus = 0;
                            }
                        }
                        else
                            line.IsCommitted = false;    
                        lineSub.ThreeMonthBonus = bonus;
                        line.BonusAddApplied = Addbonus;
                        if (line.IsCommitted == false && isOtherDist)
                        {
                            bonus = ((totalBonus) * discount) / 100.0;
                            line.BonusApplied = bonus;
                        }
                        else
                            line.BonusApplied = bonus;
                        line.Volume = totalVolumnT;
                        line.CommittedType = "3T";
                        list.Add(line);
                    }
                    if (DateTime.Now.Month <= 6)
                    {
                        line = new CommittedTrackingLine();
                        line.OrderDate = DateTime.Now;
                        line.CommittedLineSubId = lineSub.Id;
                        line.DocId = 0;
                        double totalM = 0;
                        if (FlagM > 0)
                            totalM = (lineSub.Quarter1 ?? 0) + (lineSub.Quarter2 ?? 0);
                        else
                            totalM = (lineSub.Month1 ?? 0) + (lineSub.Month2 ?? 0) + (lineSub.Month3 ?? 0) + (lineSub.Month4 ?? 0) + (lineSub.Month5 ?? 0) + (lineSub.Month6 ?? 0); ;
                        if (totalVolumnT >= totalM)
                        {
                            line.IsCommitted = true;
                        }
                        else
                            line.IsCommitted = false;
                        Addbonus = 0.0; bonus = 0.0;
                        var discount = (double)(lineSub.SixMonthDiscount ?? 0);
                        if (isOtherDist && totalVolumnT + totalVolumnTIgnore >= totalM)
                        {
                            if (isOtherDist)
                            {
                                Addbonus = ((totalBonus + totalBonusIgnore) * discount) / 100.0;
                                bonus = ((totalBonus) * discount) / 100.0;
                                line.IsCommitted = true;
                            }
                            else
                            {
                                line.IsCommitted = false;
                                Addbonus = 0;
                                bonus = 0;
                            }
                        }
                        else
                            line.IsCommitted = false;
                        lineSub.SixMonthBonus = bonus;
                        line.BonusAddApplied = Addbonus;
                        if(line.IsCommitted == false && isOtherDist)
                        {
                            bonus = ((totalBonus) * discount) / 100.0;
                            line.BonusApplied = bonus;
                        }
                        else
                            line.BonusApplied = bonus;
                        line.Volume = totalVolumnT;
                        line.CommittedType = "6T";
                        list.Add(line);
                    }
                    if (DateTime.Now.Month <= 9)
                    {
                        line = new CommittedTrackingLine();
                        line.OrderDate = DateTime.Now;
                        line.CommittedLineSubId = lineSub.Id;
                        line.DocId = 0;
                        double totalM = 0;
                        if (FlagM > 0)
                            totalM = (lineSub.Quarter1 ?? 0) + (lineSub.Quarter2 ?? 0) + (lineSub.Quarter3 ?? 0);
                        else
                            totalM = (lineSub.Month1 ?? 0) + (lineSub.Month2 ?? 0) + (lineSub.Month3 ?? 0) + (lineSub.Month4 ?? 0) + (lineSub.Month5 ?? 0) + (lineSub.Month6 ?? 0)
                                + (lineSub.Month7 ?? 0) + (lineSub.Month8 ?? 0) + (lineSub.Month9 ?? 0);
                        if (totalVolumnT >= totalM)
                        {
                            line.IsCommitted = true;
                        }
                        else
                            line.IsCommitted = false;

                        Addbonus = 0.0; bonus = 0.0;
                        var discount = (double)(lineSub.NineMonthDiscount ?? 0);
                        if (isOtherDist && totalVolumnT + totalVolumnTIgnore >= totalM)
                        {
                            
                            if (isOtherDist)
                            {
                                Addbonus = ((totalBonus + totalBonusIgnore) * discount) / 100.0;
                                bonus = ((totalBonus) * discount) / 100.0;
                                line.IsCommitted = true;
                            }
                            else
                            {
                                line.IsCommitted = false;
                                Addbonus = 0;
                                bonus = 0;
                            }
                        }
                        else
                            line.IsCommitted = false;
                        lineSub.NineMonthBonus = bonus;
                        line.BonusAddApplied = Addbonus;
                        if (line.IsCommitted == false && isOtherDist)
                        {
                            bonus = ((totalBonus) * discount) / 100.0;
                            line.BonusApplied = bonus;
                        }
                        else
                            line.BonusApplied = bonus;
                        line.Volume = totalVolumnT;
                        line.CommittedType = "9T";
                        list.Add(line);
                    }
                    int checkover = 0, Flag = 0;
                    checkover = lineSub.CommittedLineSubSub.Count();
                    foreach (var over in lineSub.CommittedLineSubSub.ToList().OrderByDescending(e => e.OutPut))
                    {
                            line = new CommittedTrackingLine();
                            line.OrderDate = DateTime.Now;
                            line.CommittedLineSubId = lineSub.Id;
                            line.DocId = 0;
                            if (totalVolumnT >= over.OutPut)
                            {
                                line.IsCommitted = true;
                                Flag++;
                            }
                            else
                                line.IsCommitted = false;
                            Addbonus = 0.0; bonus = 0.0;
                            var discount = (double)(over.Discount);
                            if (isOtherDist && totalVolumnT + totalVolumnTIgnore >= over.OutPut)
                            {
                                if (isOtherDist)
                                {
                                    Addbonus = ((totalBonus + totalBonusIgnore) * discount) / 100.0;
                                    bonus = ((totalBonus) * discount) / 100.0;
                                    line.IsCommitted = true;
                                }
                                else
                                {
                                    line.IsCommitted = false;
                                    Addbonus = 0;
                                    bonus = 0;
                                }
                            }
                            else
                                line.IsCommitted = false;
                            over.BonusTotal = bonus;
                            line.BonusAddApplied = Addbonus;
                            if (line.IsCommitted == false && isOtherDist)
                            {
                                bonus = ((totalBonus) * discount) / 100.0;
                                line.BonusApplied = bonus;
                            }
                            else
                                line.BonusApplied = bonus;
                            line.Volume = totalVolumnT;
                            line.CommittedType = "OV" + checkover;
                            checkover--;
                            list.Add(line);      
                    }
                    if (Flag == 0)
                    {
                        line = new CommittedTrackingLine();
                        line.OrderDate = DateTime.Now;
                        line.CommittedLineSubId = lineSub.Id;
                        line.DocId = 0;
                        if (totalVolumnT >= lineSub.Total)
                        {
                            line.IsCommitted = true;
                        }
                        else
                            line.IsCommitted = false;
                        Addbonus = 0.0; bonus = 0.0;
                        var discount = (double)(lineSub.DiscountYear ?? 0);
                        if (isOtherDist && totalVolumnT + totalVolumnTIgnore >= lineSub.Total)
                        {
                            if (isOtherDist)
                            {
                                Addbonus = ((totalBonus + totalBonusIgnore) * discount) / 100.0;
                                bonus = ((totalBonus) * discount) / 100.0;
                                line.IsCommitted = true;
                            }
                            else
                            {
                                line.IsCommitted = false;
                                Addbonus = 0;
                                bonus = 0;
                            }
                        }
                        else
                            line.IsCommitted = false;
                        lineSub.Total12M = bonus;
                        line.BonusAddApplied = Addbonus;
                        if (line.IsCommitted == false && isOtherDist)
                        {
                            bonus = ((totalBonus) * discount) / 100.0;
                            line.BonusApplied = bonus;
                        }
                        else
                            line.BonusApplied = bonus;
                        line.Volume = totalVolumnT;
                        line.CommittedType = "12T";
                        list.Add(line);
                    }
                }
                commitedTracking.CommittedTrackingLine = list;
            }
        }
        public void CommitedTracking(ref CommittedTracking commitedTracking, Committed commited1, PriceDocCheck model)
        {
            List<CommittedTrackingLine> list = new List<CommittedTrackingLine>();
            foreach (var cmt in commited1.CommittedLine)
            {
                var line = new CommittedTrackingLine();
                line.OrderDate = DateTime.Now;

                line.DocId = 0;
                int FlagM = 0;
                if (cmt.CommittedType == "Q")
                {

                    DateTime date = DateTime.Now;
                    if (new[] { 1, 2, 3 }.Contains(date.Month))
                        line.CommittedType = "Q1";
                    if (new[] { 4, 5, 6 }.Contains(date.Month))
                        line.CommittedType = "Q2";
                    if (new[] { 8, 9, 7 }.Contains(date.Month))
                        line.CommittedType = "Q3";
                    if (new[] { 11, 12, 10 }.Contains(date.Month))
                        line.CommittedType = "Q4";
                    FlagM++;
                }
                if (cmt.CommittedType == "Y")
                {
                    line.CommittedType = "M" + DateTime.Now.Month;
                }
                foreach (var lineSub in cmt.CommittedLineSub.ToList())
                {
                    line.CommittedLineSubId = lineSub.Id;
                    var lisr = lineSub.ItemTypes.Select(e => e.Id).ToList();
                    List<Item> listI = new List<Item>();
                    var listIt = model.ItemDetail.Select(e => e.ItemId);

                    listI = _context.Item.Include(e => e.Packing).Where(p => lisr.Contains(p.ItemTypeId ?? 0) && listIt.Contains(p.Id)
                               ).ToList();
                    var totalVolumn = from detail in model.ItemDetail
                                      join item in listI on detail.ItemId equals item.Id
                                      where detail.ItemId == item.Id
                                      select new
                                      {
                                          Volumn = detail.Quantity * item.Packing.Volumn,
                                          Bonus = detail.Quantity * detail.Price
                                      };


                    double totalVolumnT = (totalVolumn.Sum(p => p.Volumn) ?? 0);
                    lineSub.CurrentVolumn = 0;
                    lineSub.Package = (lineSub.Package ?? 0) + totalVolumnT;
                    lineSub.AfterVolumn = lineSub.Package ?? 0;
                    double totalBonus = totalVolumn.Sum(p => p.Bonus);
                    ProcessMonth(commitedTracking, lineSub, line, true, totalVolumnT, 0, 0, totalBonus, 0, 0);
                    ProcessQuarter(commitedTracking, lineSub, line, true, totalVolumnT, 0, 0, totalBonus, 0, 0);
                    list.Add(line);
                    double Addbonus = 0.0, bonus = 0.0, totalVolumnTIgnore = 0, totalBonusIgnore = 0;
                    bool isOtherDist = true;
                    if (DateTime.Now.Month <= 3)
                    {
                        line = new CommittedTrackingLine();
                        line.OrderDate = DateTime.Now;
                        line.CommittedLineSubId = lineSub.Id;
                        line.DocId = 0;
                        double totalM = 0;
                        totalM = (lineSub.Month1 ?? 0) + (lineSub.Month2 ?? 0) + (lineSub.Month3 ?? 0);
                        var M = commitedTracking.CommittedTrackingLine.Where(e =>e.CommittedType == "3T" ).ToList();
                        double totalVolumnOver = M.Sum(e => e.Volume);
                        if (totalVolumnT + totalVolumnOver >= totalM)
                        {
                            line.IsCommitted = true;
                        }
                        else
                            line.IsCommitted = false;
                        Addbonus = 0.0; bonus = 0.0;
                        var discount = (double)(lineSub.ThreeMonthDiscount ?? 0);
                        if (totalVolumnT + totalVolumnTIgnore + totalVolumnOver >= totalM)
                        {
                            if (isOtherDist)
                            {
                                Addbonus = ((totalBonus + totalBonusIgnore) * discount) / 100.0;
                                bonus = ((totalBonus) * discount) / 100.0;
                                line.IsCommitted = true;
                                M.Where(e => e.IsCommitted == false).ToList().ForEach(e => e.BonusAddApplied = e.BonusApplied);
                                M.Where(e => e.IsCommitted == false).ToList().ForEach(e => e.IsCommitted = true);
                            }
                            else
                            {
                                line.IsCommitted = false;
                                Addbonus = 0;
                                bonus = 0;
                            }
                        }
                        else
                        {
                            line.IsCommitted = false;
                        }
                        lineSub.ThreeMonthBonus = bonus;
                        line.BonusAddApplied = Addbonus;
                        if (line.IsCommitted == false && isOtherDist)
                        {
                            bonus = ((totalBonus) * discount) / 100.0;
                            line.BonusApplied = bonus;
                        }
                        else
                            line.BonusApplied = bonus;
                        line.Volume = totalVolumnT;
                        line.CommittedType = "3T";
                        list.Add(line);
                    }
                    if (DateTime.Now.Month <= 6)
                    {
                        line = new CommittedTrackingLine();
                        line.OrderDate = DateTime.Now;
                        line.CommittedLineSubId = lineSub.Id;
                        line.DocId = 0;
                        double totalM = 0;
                        //var listDocl = commitedTracking.CommittedTrackingLine.Where(e => e.OrderDate.Month == 12 && e.CommittedType == "12T" && e.IsCommitted == true).Select(e => e.DocId).ToList();
                        var M = commitedTracking.CommittedTrackingLine.Where(e =>  e.CommittedType == "6T").ToList();
                        double totalVolumnOver = M.Sum(e => e.Volume);
                        if (FlagM > 0)
                            totalM = (lineSub.Quarter1 ?? 0) + (lineSub.Quarter2 ?? 0);
                        else
                            totalM = (lineSub.Month1 ?? 0) + (lineSub.Month2 ?? 0) + (lineSub.Month3 ?? 0) + (lineSub.Month4 ?? 0) + (lineSub.Month5 ?? 0) + (lineSub.Month6 ?? 0); ;
                        if (totalVolumnT + totalVolumnOver>= totalM)
                        {
                            line.IsCommitted = true;
                        }
                        else
                            line.IsCommitted = false;
                        Addbonus = 0.0; bonus = 0.0;
                        var discount = (double)(lineSub.SixMonthDiscount ?? 0);
                        if (isOtherDist && totalVolumnT + totalVolumnTIgnore + totalVolumnOver>= totalM)
                        {
                            
                            if (isOtherDist)
                            {
                                Addbonus = ((totalBonus + totalBonusIgnore) * discount) / 100.0;
                                bonus = ((totalBonus) * discount) / 100.0;
                                line.IsCommitted = true;
                                M.Where(e => e.IsCommitted == false).ToList().ForEach(e => e.BonusAddApplied = e.BonusApplied);
                                M.Where(e => e.IsCommitted == false).ToList().ForEach(e => e.IsCommitted = true);
                            }
                            else
                            {
                                line.IsCommitted = false;
                                Addbonus = 0;
                                bonus = 0;
                            }
                        }
                        else
                        {
                            line.IsCommitted = false;
                        }
                        lineSub.SixMonthBonus = bonus;
                        line.BonusAddApplied = Addbonus;
                        if (line.IsCommitted == false && isOtherDist)
                        {
                            bonus = ((totalBonus) * discount) / 100.0;
                            line.BonusApplied = bonus;
                        }
                        else
                            line.BonusApplied = bonus;
                        line.Volume = totalVolumnT;
                        line.CommittedType = "6T";
                        list.Add(line);
                    }
                    if (DateTime.Now.Month <= 9)
                    {
                        line = new CommittedTrackingLine();
                        line.OrderDate = DateTime.Now;
                        line.CommittedLineSubId = lineSub.Id;
                        line.DocId = 0;
                        double totalM = 0;
                        //var listDocl = commitedTracking.CommittedTrackingLine.Where(e => e.OrderDate.Month == 12 && e.CommittedType == "12T" && e.IsCommitted == true).Select(e => e.DocId).ToList();
                        var M = commitedTracking.CommittedTrackingLine.Where(e =>  e.CommittedType == "9T").ToList();
                        double totalVolumnOver = M.Sum(e => e.Volume);
                        if (FlagM > 0)
                            totalM = (lineSub.Quarter1 ?? 0) + (lineSub.Quarter2 ?? 0) + (lineSub.Quarter3 ?? 0);
                        else
                            totalM = (lineSub.Month1 ?? 0) + (lineSub.Month2 ?? 0) + (lineSub.Month3 ?? 0) + (lineSub.Month4 ?? 0) + (lineSub.Month5 ?? 0) + (lineSub.Month6 ?? 0)
                                + (lineSub.Month7 ?? 0) + (lineSub.Month8 ?? 0) + (lineSub.Month9 ?? 0);
                        if (totalVolumnT +totalVolumnOver>= totalM)
                        {
                            line.IsCommitted = true;
                        }
                        else
                            line.IsCommitted = false;

                        Addbonus = 0.0; bonus = 0.0;
                        var discount = (double)(lineSub.NineMonthDiscount ?? 0);
                        if (isOtherDist && totalVolumnT + totalVolumnTIgnore + totalVolumnOver >= totalM)
                        {
                            
                            if (isOtherDist)
                            {
                                Addbonus = ((totalBonus + totalBonusIgnore) * discount) / 100.0;
                                bonus = ((totalBonus) * discount) / 100.0;
                                line.IsCommitted = true;
                                M.Where(e => e.IsCommitted == false).ToList().ForEach(e => e.BonusAddApplied = e.BonusApplied);
                                M.Where(e => e.IsCommitted == false).ToList().ForEach(e => e.IsCommitted = true);
                                
                            }
                            else
                            {
                                line.IsCommitted = false;
                                Addbonus = 0;
                                bonus = 0;
                            }
                        }
                        else
                        {
                            line.IsCommitted = false;
                        }
                        lineSub.NineMonthBonus = bonus;
                        line.BonusAddApplied = Addbonus;
                        if (line.IsCommitted == false && isOtherDist)
                        {
                            bonus = ((totalBonus) * discount) / 100.0;
                            line.BonusApplied = bonus;
                        }
                        else
                            line.BonusApplied = bonus;
                        line.Volume = totalVolumnT;
                        line.CommittedType = "9T";
                        list.Add(line);
                    }
                    int checkover = 0, Flag = 0, isCheck = 0;
                    checkover = lineSub.CommittedLineSubSub.Count();
                    isCheck = checkover;
                    foreach (var over in lineSub.CommittedLineSubSub.ToList().OrderByDescending(e => e.OutPut))
                    {
                        line = new CommittedTrackingLine();
                        line.OrderDate = DateTime.Now;
                        line.CommittedLineSubId = lineSub.Id;
                        line.DocId = 0;
                        var M = commitedTracking.CommittedTrackingLine.Where(e => e.CommittedType == "OV" + checkover).ToList();
                        double totalVolumnOver = M.Sum(e => e.Volume);
                        if (totalVolumnT + totalVolumnOver >= over.OutPut)
                        {
                            line.IsCommitted = true;
                            Flag++;
                        }
                        else
                            line.IsCommitted = false;
                        Addbonus = 0.0; bonus = 0.0;
                        var discount = (double)(over.Discount);
                        if (isOtherDist && totalVolumnT + totalVolumnTIgnore + totalVolumnOver >= over.OutPut)
                        {

                            if (isOtherDist)
                            {
                                Addbonus = ((totalBonus + totalBonusIgnore) * discount) / 100.0;
                                bonus = ((totalBonus) * discount) / 100.0;
                                line.IsCommitted = true;
                                M.Where(e => e.IsCommitted == false).ToList().ForEach(e => e.BonusAddApplied = e.BonusApplied);
                                M.Where(e => e.IsCommitted == false).ToList().ForEach(e => e.IsCommitted = true);

                            }
                            else
                            {
                                line.IsCommitted = false;
                                Addbonus = 0;
                                bonus = 0;
                            }
                        }
                        else
                        {
                            line.IsCommitted = false;
                        }
                        over.BonusTotal = bonus;
                        line.BonusAddApplied = Addbonus;
                        if (line.IsCommitted == false && isOtherDist)
                        {
                            bonus = ((totalBonus) * discount) / 100.0;
                            line.BonusApplied = bonus;
                        }
                        else
                        {
                            isCheck--;
                            line.BonusApplied = bonus;
                        }

                        line.Volume = totalVolumnT;
                        line.CommittedType = "OV" + checkover;
                        checkover--;
                        list.Add(line);
                        if (line.IsCommitted == true)
                            break;
                    }
                    if (Flag == 0)
                    {
                        line = new CommittedTrackingLine();
                        line.OrderDate = DateTime.Now;
                        line.CommittedLineSubId = lineSub.Id;
                        line.DocId = 0;
                        //var listDocl = commitedTracking.CommittedTrackingLine.Where(e => e.OrderDate.Month == 12 && e.CommittedType == "12T" && e.IsCommitted == true).Select(e => e.DocId).ToList();
                        var M = commitedTracking.CommittedTrackingLine.Where(e =>  e.CommittedType == "12T" ).ToList();
                        double totalVolumnOver = M.Sum(e => e.Volume);
                        if (totalVolumnT + totalVolumnOver >= lineSub.Total)
                        {
                            line.IsCommitted = true;
                        }
                        else
                            line.IsCommitted = false;
                        Addbonus = 0.0; bonus = 0.0;
                        var discount = (double)(lineSub.DiscountYear ?? 0);
                        if (isOtherDist && totalVolumnT + totalVolumnTIgnore + totalVolumnOver>= lineSub.Total)
                        {
                            if (isOtherDist)
                            {
                                Addbonus = ((totalBonus + totalBonusIgnore) * discount) / 100.0;
                                bonus = ((totalBonus) * discount) / 100.0;
                                line.IsCommitted = true;
                                M.Where(e => e.IsCommitted == false).ToList().ForEach(e => e.BonusAddApplied = e.BonusApplied);
                                M.Where(e => e.IsCommitted == false).ToList().ForEach(e => e.IsCommitted = true);
                                
                            }
                            else
                            {
                                line.IsCommitted = false;
                                Addbonus = 0;
                                bonus = 0;
                            }
                        }
                        else
                        {
                            line.IsCommitted = false;
                        }
                        lineSub.Total12M = bonus;
                        line.BonusAddApplied = Addbonus;
                        if(line.IsCommitted == false && isOtherDist)
                        {
                            bonus = ((totalBonus) * discount) / 100.0;
                            line.BonusApplied = bonus;
                        }
                        else
                            line.BonusApplied = bonus;
                        line.Volume = totalVolumnT;
                        line.CommittedType = "12T";
                        list.Add(line);
                    }
                }
                if(commitedTracking.CommittedTrackingLine.Count > 0)
                    foreach(var li in list)
                    {
                        commitedTracking.CommittedTrackingLine.Add(li);
                    }    
                else
                    commitedTracking.CommittedTrackingLine = list;
            }
        }
        public void ProcessCommittedMonth(CommittedTrackingLine line, CommittedLineSub lineSub,bool isOtherDist,double totalVolumnT,double totalVolumnTIgnore, double totalVolumnTPro, double totalBonus,double totalBonusIgnore, double totalBonusPro)
        {
            if (line.CommittedType == null)
                return;
            if (!line.CommittedType.StartsWith("M") ||
                !int.TryParse(line.CommittedType.Substring(1), out var month) ||
                month < 1 || month > 12)
                return;
            var subType = typeof(CommittedLineSub);
            var propThreshold = subType.GetProperty($"Month{month}");
            var propTotal = subType.GetProperty($"TotalMonth{month}");
            var propBonus = subType.GetProperty($"BonusMonth{month}");

            if (propThreshold == null || propTotal == null || propBonus == null)
                return;
            var threshold = (double)(propThreshold.GetValue(lineSub) ?? 0.0);
            propTotal.SetValue(lineSub, totalVolumnT+ totalVolumnTPro);
            double Addbonus = 0.0, bonus = 0.0;
            var discount = (double)(lineSub.DiscountMonth ?? 0);
            if (totalVolumnT + totalVolumnTIgnore  + totalVolumnTPro  >= threshold)
            {
                if (isOtherDist)
                {
                    Addbonus = ((totalBonus + totalBonusIgnore) * discount) / 100.0;
                    bonus = ((totalBonus) * discount) / 100.0;
                }
                else
                {
                    Addbonus = 0;
                    bonus = 0;
                }
                Addbonus = Addbonus + ((totalBonusPro) * discount) / 100.0;
                bonus = bonus + ((totalBonusPro) * discount) / 100.0;
                propBonus.SetValue(lineSub, bonus);
            }
            else
                line.IsCommitted = false;
            line.BonusAddApplied = Addbonus;
            if (line.IsCommitted == false && isOtherDist)
            {
                bonus = ((totalBonusPro + totalBonus) * discount) / 100.0;
                line.BonusApplied = bonus;
            }
            else
                line.BonusApplied = bonus;
            line.Volume = totalVolumnT + totalVolumnTPro;
        }
        public void ProcessCommittedQuater(CommittedTrackingLine line, CommittedLineSub lineSub, bool isOtherDist, double totalVolumnT, double totalVolumnTIgnore, double totalVolumnTPro, double totalBonus, double totalBonusIgnore, double totalBonusPro)
        {
            if (line.CommittedType == null)
                return;
            if (!line.CommittedType.StartsWith("Q") ||
                !int.TryParse(line.CommittedType.Substring(1), out var quater) ||
                quater < 1 || quater > 4)
                return;
            var subType = typeof(CommittedLineSub);
            var propThreshold = subType.GetProperty($"Quarter{quater}");
            var propTotal = subType.GetProperty($"TotalQuarter{quater}");
            var propBonus = subType.GetProperty($"BonusQuarter{quater}");
            if (propThreshold == null || propTotal == null || propBonus == null)
                return;
            var threshold = (double)(propThreshold.GetValue(lineSub) ?? 0.0);
            propTotal.SetValue(lineSub, totalVolumnT + totalVolumnTPro);
            
            double Addbonus = 0.0, bonus = 0.0;
            var discount = (double)(lineSub.Discount ?? 0);
            if (totalVolumnT + totalVolumnTIgnore + totalVolumnTPro >= threshold)
            {

                if (isOtherDist)
                {
                    Addbonus = ((totalBonus + totalBonusIgnore) * discount) / 100.0;
                    bonus = ((totalBonus) * discount) / 100.0;
                }
                else
                {
                    Addbonus = 0;
                    bonus = 0;
                }
                Addbonus = Addbonus + ((totalBonusPro) * discount) / 100.0;
                bonus = bonus + ((totalBonusPro) * discount) / 100.0;
                propBonus.SetValue(lineSub, bonus);
            }
            else
            {
                line.IsCommitted = false;
            }
            line.BonusAddApplied = Addbonus;
            if(line.IsCommitted == false && isOtherDist)
            {
                bonus = ((totalBonus + totalBonusPro) * discount) / 100.0;
                line.BonusApplied = bonus;
            }
            else
                line.BonusApplied = bonus;
            line.Volume = totalVolumnT + totalVolumnTPro;
        }
        public void ProcessQuarter(CommittedTracking commitedTracking, CommittedLineSub lineSub, CommittedTrackingLine line,
        bool isOtherDist,double totalVolumnT,double totalVolumnTIgnore,double totalVolumnTPro,double totalBonus,double totalBonusIgnore,double totalBonusPro)
        {
            if (line.CommittedType == null)
                return;
            if (!line.CommittedType.StartsWith("Q") ||
                !int.TryParse(line.CommittedType.Substring(1), out var quater) ||
                quater < 1 || quater > 4)
                return;
            List<int> months = new List<int>();
            if (quater ==1)
                months = new List<int> { 1, 2, 3 };
            else if (quater == 2)
                months = new List<int> { 4, 5, 6 };
            else if (quater == 3)
                months = new List<int> { 7, 8, 9 };
            else
                months = new List<int> { 10, 11, 12 };
                var listDocl = commitedTracking.CommittedTrackingLine
                .Where(e => months.Contains(e.OrderDate.Month)&& e.CommittedType == line.CommittedType && e.IsCommitted)
                .Select(e => e.DocId)
                .ToList();

            var M = commitedTracking.CommittedTrackingLine
                .Where(e =>  months.Contains(e.OrderDate.Month) && e.CommittedType == line.CommittedType)
                .ToList();

            double totalVolumnOver = M.Sum(e => e.Volume);
            double totalVolumeAll = totalVolumnT + totalVolumnTIgnore + totalVolumnTPro+ totalVolumnOver;
            double discount = (double)(lineSub.Discount ?? 0);
            var subType = typeof(CommittedLineSub);
            var propThreshold = subType.GetProperty($"Quarter{quater}");
            var propTotal = subType.GetProperty($"TotalQuarter{quater}");
            var propBonus = subType.GetProperty($"BonusQuarter{quater}");
            if (propThreshold == null || propTotal == null || propBonus == null)
                return;

            double quarterTarget = (double)(propThreshold.GetValue(lineSub) ?? 0.0);

            double bonus = 0;
            double addBonus = 0;

            if (totalVolumeAll >= quarterTarget)
            {
                if (isOtherDist)
                {
                    addBonus = ((totalBonus + totalBonusIgnore) * discount) / 100.0;
                    bonus = (totalBonus * discount) / 100.0;
                    M.Where(e => e.IsCommitted == false).ToList().ForEach(e => e.BonusAddApplied = e.BonusApplied);
                    M.Where(e=>e.IsCommitted == false).ToList().ForEach(e => e.IsCommitted = true);
                    line.IsCommitted = true;
                }
                else
                {
                    line.IsCommitted = false;
                    bonus = 0;
                    addBonus = 0;
                }
                addBonus += (totalBonusPro * discount) / 100.0 ;
                bonus += (totalBonusPro * discount) / 100.0;
                propBonus.SetValue(lineSub, bonus);
            }
            else
            {
                line.IsCommitted = false;
            }
            propTotal.SetValue(lineSub, totalVolumnT + totalVolumnTPro);
            line.BonusAddApplied = addBonus;
            if (line.IsCommitted == false && isOtherDist)
            {
                bonus = ((totalBonus + totalBonusIgnore) * discount) / 100.0;
                line.BonusApplied = bonus;
            }
            else
                line.BonusApplied = bonus;
            line.Volume = totalVolumnT + totalVolumnTPro;
        }
        public void ProcessMonth(CommittedTracking commitedTracking, CommittedLineSub lineSub, CommittedTrackingLine line,
        bool isOtherDist, double totalVolumnT, double totalVolumnTIgnore, double totalVolumnTPro, double totalBonus, double totalBonusIgnore, double totalBonusPro)
        {
            if (line.CommittedType == null)
                return;
            if (!line.CommittedType.StartsWith("M") ||
               !int.TryParse(line.CommittedType.Substring(1), out var months) ||
               months < 1 || months > 12)
                return;
            var listDocl = commitedTracking.CommittedTrackingLine
                .Where(e => e.OrderDate.Month == months && e.CommittedType == line.CommittedType && e.IsCommitted)
                .Select(e => e.DocId)
                .ToList();

            var M = commitedTracking.CommittedTrackingLine
                .Where(e => months == e.OrderDate.Month &&e.CommittedType == line.CommittedType)
                .ToList();

            double totalVolumnOver = M.Sum(e => e.Volume);
            double totalVolumeAll = totalVolumnT + totalVolumnTIgnore + totalVolumnTPro + totalVolumnOver;
            double discount = (double)(lineSub.DiscountMonth ?? 0);
            var subType = typeof(CommittedLineSub);
            var propThreshold = subType.GetProperty($"Month{months}");
            var propTotal = subType.GetProperty($"TotalMonth{months}");
            var propBonus = subType.GetProperty($"BonusMonth{months}");
            if (propThreshold == null || propTotal == null || propBonus == null)
                return;

            double quarterTarget = (double)(propThreshold.GetValue(lineSub) ?? 0.0);

            double bonus = 0;
            double addBonus = 0;

            if (totalVolumeAll >= quarterTarget)
            {
                if (isOtherDist)
                {
                    addBonus = ((totalBonus + totalBonusIgnore) * discount) / 100.0;
                    bonus = (totalBonus * discount) / 100.0;
                    M.Where(e => e.IsCommitted == false).ToList().ForEach(e => e.BonusAddApplied = e.BonusApplied);
                    M.Where(e => e.IsCommitted == false).ToList().ForEach(e => e.IsCommitted = true);
                    line.IsCommitted = true;
                }
                else
                {
                    line.IsCommitted = false;
                    bonus = 0;
                    addBonus = 0;
                }

                addBonus += (totalBonusPro * discount) / 100.0 ;
                bonus += (totalBonusPro * discount) / 100.0 ;
                propBonus.SetValue(lineSub, bonus);
            }
            else
                line.IsCommitted = false;
            propTotal.SetValue(lineSub, totalVolumnT + totalVolumnTPro);
            line.BonusAddApplied = addBonus;
            if (line.IsCommitted == false && isOtherDist)
            {
                bonus = ((totalBonus + totalBonusIgnore) * discount) / 100.0;
                line.BonusApplied = bonus;
            }
            else
                line.BonusApplied = bonus;
            line.Volume = totalVolumnT + totalVolumnTPro;
        }


        public async Task<(bool, Mess)> DeleteAsync(int? id, int ObjType)
        {
            Mess mess = new Mess();
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var query = _context.ODOC.AsQueryable();
                    var entity = await query.ToArrayAsync();
                    if (entity == null)
                    {
                        mess.Status = 400;
                        mess.Errors = "Không tìm thấy bản ghi để xóa";
                        return (false, mess);
                    }

                    _context.RemoveRange(entity);
                    await _context.SaveChangesAsync();
                    transaction.Commit();
                    return (true, null);
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

        public async Task<(IEnumerable<ODOC>, int total, Mess)> GetAllDocumentAsync(int skip, int limit, int ObjType,
            GridifyQuery q, string? search, int? userId)
        {
            Mess mess = new Mess();
            try
            {
                var query = _context.ODOC.AsQueryable();
                if (!string.IsNullOrEmpty(search))
                {
                    search = search.Trim();
                    query = query.Where(d =>
                        (d.CardCode != null && d.CardCode.Contains(search)) ||
                        (d.InvoiceCode != null && d.InvoiceCode.Contains(search)) || d.CardName.Contains(search));
                }
                int cardId = 0;
                if (userId is not null)
                {
                    var usr = await _context.AppUser.AsSplitQuery().AsQueryable().AsNoTracking()
                        .Include(xx => xx.DirectStaff)
                        .Where(xx => xx.Id == userId)
                        .Include(xx => xx.Role)
                        .ThenInclude(xx => xx.RoleFillCustomerGroups)
                        .FirstOrDefaultAsync();
                    cardId = usr?.CardId ?? 0;
                    if (usr != null && usr.Role != null)
                    {
                        if (usr.Role.IsFillCustomerGroup)
                        {
                            query = query.Where(e => e.BP.Groups.Any(c =>
                                usr.Role.RoleFillCustomerGroups.Select(d => d.CustomerGroupId).ToList().Contains(c.Id)));
                        }
                        if (usr.Role.IsSaleRole)
                        {
                            var usrIds1 = await GetAllCustomerIdWithUserId(userId ?? 0);
                            //  query = query
                            // .Where(x => x.BP.SaleId != null && (x.BP.SaleId == usr.Id
                            //                                     || usr.DirectStaff.Select(d => d.Id)
                            //                                         .Contains(x.BP.SaleId ?? -1)));

                            // var paths = usr.DirectStaffPath.Split("/");
                            query = query.Where(e => usrIds1.Contains(e.BP.SaleId.Value));
                        }    
                    }
                }
                if (cardId == 0)
                {
                    var totalCount = query.Where(p => p.ObjType == ObjType).ApplyFiltering(q).Count();
                    var items = query.AsSplitQuery().Where(p => p.ObjType == ObjType).OrderBy(p => p.DocDate)
                        .Include(p => p.PaymentInfo)
                        .Include(p => p.ItemDetail)
                        .Include(p => p.Tracking)
                        .Include(p => p.Address)
                        .Include(p => p.PaymentMethod)
                        .Include(p => p.AttDocuments)
                        .ThenInclude(p => p.Author)
                        .Include(p => p.AttachFile)
                        .ThenInclude(p => p.Author)
                        .Include(p => p.Author)
                        .Include(p => p.Promotion)
                        .OrderByDescending(p => p.Id)
                        .ApplyFiltering(q)
                        .Skip(skip * limit)
                        .Take(limit)
                        .ToList();
                    return (items, totalCount, null);
                }
                else
                {
                    var totalCount = query.Where(p => p.ObjType == ObjType && p.CardId == cardId).ApplyFiltering(q).Count();
                    var items = query.AsSplitQuery().Where(p => p.ObjType == ObjType && p.CardId == cardId).OrderBy(p => p.DocDate)
                        .Include(p => p.PaymentInfo)
                        .Include(p => p.ItemDetail)
                        .Include(p => p.Tracking)
                        .Include(p => p.Address)
                        .Include(p => p.PaymentMethod)
                        .Include(p => p.AttDocuments)
                        .ThenInclude(p => p.Author)
                        .Include(p => p.AttachFile)
                        .ThenInclude(p => p.Author)
                        .Include(p => p.Author)
                        .Include(p => p.Promotion)
                        .OrderByDescending(p => p.Id)
                        .ApplyFiltering(q)
                        .Skip(skip * limit)
                        .Take(limit)
                        .ToList();
                    return (items, totalCount, null);
                }   
                
            }
            catch (Exception ex)
            {
                mess.Status = 900;
                mess.Errors = ex.Message;
                return (null, 0, mess);
            }
        }

        public async Task<(IEnumerable<ODOC>, int total, Mess)> GetAllDocumentAsync(int ObjType)
        {
            Mess mess = new Mess();
            try
            {
                var query = _context.Set<ODOC>().AsQueryable();
                var totalCount = await query.Where(p => p.ObjType == ObjType).CountAsync();
                var items = await query.Where(p => p.ObjType == ObjType)
                    .Include(p => p.AttDocuments)
                    .ThenInclude(p => p.Author)
                    .Include(p => p.ItemDetail)
                    .Include(p => p.Tracking)
                    .Include(p => p.Address)
                    .Include(p => p.PaymentMethod)
                    .Include(p => p.AttachFile)
                    .ThenInclude(p => p.Author)
                    .Include(p => p.Promotion)
                    .OrderByDescending(p => p.Id)
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

        public async Task<(IEnumerable<ODOC>, int total, Mess)> GetAllDocumentAsync(string Status, int ObjType)
        {
            Mess mess = new Mess();
            try
            {
                var query = _context.Set<ODOC>().AsQueryable();
                var totalCount = await query.Where(p => p.ObjType == ObjType && p.Status == Status).CountAsync();
                var items = await query.Where(p => p.ObjType == ObjType)
                    .Include(p => p.ItemDetail)
                    .Include(p => p.Tracking)
                    .Include(p => p.Address)
                    .Include(p => p.PaymentMethod)
                    .Include(p => p.AttachFile)
                    .OrderByDescending(p => p.Id)
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

        public async Task<(IEnumerable<ODOC>, Mess)> GetDocumentAsync(string search, int ObjType)
        {
            Mess mess = new Mess();
            try
            {
                var document = await _context.ODOC
                    .Include(p => p.ItemDetail)
                    .Include(p => p.Tracking)
                    .Include(p => p.Address)
                    .Include(p => p.PaymentMethod)
                    .Include(p => p.AttachFile)
                    .ThenInclude(p => p.Author)
                    .Where(p => (p.Id.ToString().Contains(search) ||
                                 p.CardCode.Contains(search) ||
                                 p.CardName.Contains(search) || p.InvoiceCode.Contains(search)) && p.ObjType == ObjType)
                    .OrderByDescending(p => p.Id)
                    .ToListAsync();
                return (document, null);
            }
            catch (Exception ex)
            {
                mess.Status = 900;
                mess.Errors = ex.Message;
                return (null, mess);
            }
        }

        public async Task<(ODOC, Mess)> GetDocumentByIdAsync(int id, int ObjType)
        {
            Mess mess = new Mess();
            try
            {
                var document = await _context.ODOC
                    .AsNoTracking()
                    .Include(p => p.ItemDetail)
                    .Include(p => p.Tracking)
                    .Include(p => p.AttDocuments)
                    .ThenInclude(p => p.Author)
                    .Include(p => p.Address)
                    .Include(p => p.PaymentMethod)
                    .Include(p => p.AttachFile)
                    .ThenInclude(p => p.Author)
                    .Include(p => p.Approval)
                    .Include(p => p.Author)
                    .Include(p => p.Promotion)
                    .Include(p => p.PaymentInfo)
                    .Include(p => p.Payments)
                    .Include(p=>p.Ratings)
                    .ThenInclude(p => p.Images)
                    .FirstOrDefaultAsync(p => p.Id == id && p.ObjType == ObjType);
                if(document != null)
                {
                    var history = _context.CustomerPointLine.Where(e => e.DocId == document.Id).ToList();
                    document.CustomerPointHistory = history;
                }  
                return (document, null);
            }
            catch (Exception ex)
            {
                mess.Status = 900;
                mess.Errors = ex.Message;
                return (null, mess);
            }
        }

        public async Task<(ODOC, Mess)> UpdateDocumentAsync(int id, DOCUpdate model, int ObjType)
        {
            Mess mess = new Mess();
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var item = await _context.ODOC
                        .AsNoTracking()
                        .Include(p => p.AttachFile)
                        .FirstOrDefaultAsync(p => p.Id == id);
                    if (item == null)
                    {
                        mess.Status = 400;
                        mess.Errors = "Dữ liệu trống";
                        return (null, mess);
                    }

                    string json = model.document;
                    ODOC items = JsonConvert.DeserializeObject<ODOC>(json);
                    if (id != items.Id)
                    {
                        mess.Status = 400;
                        mess.Errors = "Dữ liệu không khớp";
                        return (null, mess);
                    }

                    items.DocDate = item.DocDate;
                    items.DueDate = item.DueDate;

                    var dtoType = items.GetType();
                    var entityType = item.GetType();

                    foreach (var prop in dtoType.GetProperties())
                    {
                        var dtoValue = prop.GetValue(items);
                        if (dtoValue != null)
                        {
                            var entityProp = entityType.GetProperty(prop.Name);
                            if (prop.Name.Equals("CreatedDate") || prop.Name.Equals("AttachFile"))
                            {
                            }
                            else if (entityProp != null)
                            {
                                entityProp.SetValue(item, dtoValue);
                            }
                        }
                    }

                    var uploadsFolder = Path.Combine(_webHostEnvironment.ContentRootPath, "uploads");
                    Directory.CreateDirectory(uploadsFolder);
                    if (model.attachFile != null && model.attachFile.Count > 0)
                    {
                        foreach (var file in model.attachFile)
                        {
                            bool ok = false;
                            if (file != null)
                            {
                                foreach (var doc14 in items.AttachFile.ToList())
                                {
                                    if (doc14.Status != null)
                                    {
                                        if (doc14.FileName.Equals(file.FileName))
                                        {
                                            var fileName = file.FileName;
                                            var fileNameSaving = Guid.NewGuid().ToString() +
                                                                 Path.GetExtension(file.FileName);
                                            var filePath = Path.Combine(uploadsFolder, fileNameSaving);

                                            using (var fileStream = new FileStream(filePath, FileMode.Create))
                                            {
                                                await file.CopyToAsync(fileStream);
                                            }

                                            var request = _httpContextAccessor.HttpContext.Request;
                                            var baseUrl = $"{request.Scheme}://{request.Host}";
                                            var imageUrl = $"{baseUrl}/uploads/{fileNameSaving}";
                                            if (!string.IsNullOrEmpty(doc14.Status))
                                            {
                                                if (doc14.Status.Equals("U"))
                                                {
                                                    var image = item.AttachFile.FirstOrDefault(i => i.Id == doc14.Id);
                                                    image.AuthorId = model.UserId;
                                                    var filePathDel = image.FilePath;
                                                    if (File.Exists(filePathDel))
                                                    {
                                                        File.Delete(filePathDel);
                                                    }

                                                    if (image != null)
                                                    {
                                                        image.FileName = fileName;
                                                        image.FilePath = imageUrl;
                                                        image.Memo = doc14.Memo;
                                                        image.Note = doc14.Note;
                                                    }
                                                }
                                                else if (doc14.Status.Equals("A"))
                                                {
                                                    item.AttachFile.Add(new DOC14
                                                    {
                                                        FileName = fileName,
                                                        FilePath = imageUrl,
                                                        Memo = doc14.Memo,
                                                        Note = doc14.Note
                                                    });
                                                }
                                            }
                                        }

                                        ok = true;
                                    }
                                }
                            }

                            if (ok)
                            {
                                var bp = await _context.BP.AsNoTracking().FirstOrDefaultAsync(p => p.Id == item.CardId);
                                List<int> sendUsers = [item.UserId ?? 0];
                                if (item.UserId != bp.SaleId)
                                {
                                    sendUsers.Add(bp.SaleId ?? 0);
                                }
                                string message = item.ObjType == 22
                                    ? "Đơn hàng {0} đã được bổ sung chứng từ"
                                    : "Yêu cầu lấy hàng {0} đã được bổ sung chứng từ";
                                string objType = item.ObjType == 22 ? "order" : "order_request";
                                _eventAggregator.Publish(new Models.NotificationModels.Notification
                                {
                                    Message = string.Format(message, item.InvoiceCode),
                                    Title = "Bổ sung chứng từ",
                                    Type = "info",
                                    Object = new Models.NotificationModels.NotificationObject
                                    {
                                        ObjId = item.Id,
                                        ObjName = "-",
                                        ObjType = objType,
                                    },
                                    SendUsers = sendUsers,
                                });
                            }
                        }
                    }

                    if (model.attachFile == null)
                    {
                        int ok = 0;
                        foreach (var doc14 in items.AttachFile.ToList())
                        {
                            if (doc14.Status != null)
                            {
                                if (!string.IsNullOrEmpty(doc14.Status))
                                {
                                    if (doc14.Status.Equals("U"))
                                    {
                                        var image = item.AttachFile.FirstOrDefault(i => i.Id == doc14.Id);
                                        if (image != null)
                                        {
                                            image.FileName = doc14.FileName;
                                            image.Memo = doc14.Memo;
                                            image.Note = doc14.Note;
                                        }
                                    }
                                    else if (doc14.Status.Equals("A"))
                                    {
                                        item.AttachFile.Add(new DOC14
                                        {
                                            FileName = doc14.FileName,
                                            Memo = doc14.Memo,
                                            Note = doc14.Note
                                        });
                                    }

                                    ok++;
                                }
                            }
                        }

                        if (ok != 0)
                        {
                            string message = item.ObjType == 22
                                ? "Đơn hàng {0} của bạn cần bổ sung chứng từ"
                                : "Yêu cầu lấy hàng {0} của bạn cần bổ sung chứng từ";
                            string objType = item.ObjType == 22 ? "order" : "order_request";
                            var userId = await _context.AppUser.Where(p => p.CardId == item.CardId).Select(p => p.Id)
                                .ToListAsync();
                            _eventAggregator.Publish(new Models.NotificationModels.Notification
                            {
                                Message = string.Format(message, item.InvoiceCode),
                                Title = "Yêu cầu bổ sung chứng từ",
                                Type = "info",
                                Object = new Models.NotificationModels.NotificationObject
                                {
                                    ObjId = item.Id,
                                    ObjName = "-",
                                    ObjType = objType,
                                },
                                SendUsers = userId,
                            });
                        }
                    }

                    if (items.AttachFile != null)
                    {
                        foreach (var itm1 in items.AttachFile.ToList())
                        {
                            if (!string.IsNullOrEmpty(itm1.Status))
                            {
                                if (itm1.Status.Equals("D"))
                                {
                                    var image = item.AttachFile.FirstOrDefault(i => i.Id == itm1.Id);
                                    if (image != null)
                                    {
                                        _context.DOC14.Remove(image);
                                        item.AttachFile.Remove(image);
                                    }
                                }
                            }
                        }
                    }

                    _context.ODOC.Update(item);
                    await _context.SaveChangesAsync();
                    transaction.Commit();
                    return (item, null);
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("Cannot insert duplicate key"))
                    {
                        mess.Status = 800;
                        mess.Errors = "Mã hàng hóa đã tồn tại";
                        transaction.Rollback();
                        return (null, mess);
                    }

                    mess.Status = 900;
                    mess.Errors = ex.Message;
                    transaction.Rollback();
                    return (null, mess);
                }
            }
        }

        public async Task<(IEnumerable<ODOC>?, int total, Mess?)> GetAllDocumentByCardIdIdAsync(int skip, int limit,
            int cardId, int objType, string? search, GridifyQuery q)
        {
            Mess mess = new Mess();
            try
            {
                var config = new GridifyMapper<ODOC>()
                    .GenerateMappings()
                    .AddMap("docDate", p => p.DocDate!.Value.Date);
                var query = _context.Set<ODOC>().AsQueryable();
                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(p => p.InvoiceCode.ToLower().Contains(search.ToLower()));
                }

                var totalCount = await query.ApplyFiltering(q, config).Where(p => p.CardId == cardId).CountAsync();
                var items = await query.ApplyFiltering(q, config).Where(p => p.CardId == cardId).OrderBy(p => p.DocDate)
                    .Include(p => p.ItemDetail)
                    .Include(p => p.Tracking)
                    .Include(p => p.Address)
                    .Include(p => p.AttDocuments)
                    .ThenInclude(p => p.Author)
                    .Include(p => p.PaymentMethod)
                    .Include(p => p.AttachFile)
                    .Include(p => p.Approval)
                    .Include(p => p.Author)
                    .Include(p => p.Promotion)
                    .Where(p => p.ObjType == objType)
                    .OrderByDescending(p => p.Id)
                    .Skip(skip * limit)
                    .Take(limit)
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

        //public async Task<(ODOC, Mess)> AddPurchaseOrderAsync(ODOC model)
        //{
        //    Mess mess = new Mess();
        //    using (var transaction = _context.Database.BeginTransaction())
        //    {
        //        try
        //        {
        //            var codes = "";
        //            codes = await GenerateDocument("DH", "", 10, model);
        //            if (model.ItemDetail.Count == 0 || model.ItemDetail == null)
        //            {
        //                mess.Status = 400;
        //                mess.Errors = "Chi tiết phiếu trống";
        //                transaction.Rollback();
        //                return (null, mess);
        //            }

        //            if (model.Address == null)
        //            {
        //                mess.Status = 400;
        //                mess.Errors = "Địa chỉ trống";
        //                transaction.Rollback();
        //                return (null, mess);
        //            }

        //            if (model.PaymentMethod.Count == 0 || model.PaymentMethod == null)
        //            {
        //                mess.Status = 400;
        //                mess.Errors = "Phương thức thanh toán trống";
        //                transaction.Rollback();
        //                return (null, mess);
        //            }

        //            var ocrd = await _context.BP
        //                .Include(p => p.CRD3)
        //                .Include(p => p.CRD4)
        //                .FirstOrDefaultAsync(p => p.Id == model.CardId);

        //            var crd3 = ocrd.CRD3.ToList();
        //            var crd4 = ocrd.CRD4.ToList();
        //            bool Balance = false, BalanceOver = false;
        //            var paymentMethod = model.PaymentMethod.FirstOrDefault();
        //            if (paymentMethod.PaymentMethodName.ToString() != "Thanh toán ngay")
        //            {
        //                var r3 = crd3.Where(p => p.PaymentMethodID == paymentMethod.PaymentMethodID).FirstOrDefault();
        //                if (r3 != null)
        //                {
        //                    if (model.Total + r3.Balance > r3.BalanceLimit)
        //                    {
        //                        Balance = true;
        //                    }
        //                }

        //                var r4 = crd4.Where(p => p.AmountOverdue > 0).FirstOrDefault();
        //                if (r4 != null)
        //                {
        //                    BalanceOver = true;
        //                }
        //            }

        //            if (Balance == true || BalanceOver == true)
        //            {
        //                model.Status = "DXL";
        //                model.WddStatus = "-";
        //            }
        //            else
        //            {
        //                model.Status = "DXN";
        //                model.WddStatus = "-";
        //            }

        //            model.LimitRequire = Balance;
        //            model.LimitOverDue = BalanceOver;
        //            model.ObjType = 22;
        //            model.InvoiceCode = codes;
        //            model.DocDate = DateTime.Now;
        //            model.DueDate = DateTime.Now;
        //            Console.WriteLine(DateTime.Now.ToString());
        //            Console.WriteLine(DateTime.UtcNow.ToString());

        //            _context.ODOC.Add(model);
        //            await _context.SaveChangesAsync();
        //            transaction.Commit();
        //            return (model, null);
        //        }
        //        catch (Exception ex)
        //        {
        //            mess.Status = 900;
        //            mess.Errors = ex.Message;
        //            transaction.Rollback();
        //            return (null, mess);
        //        }
        //    }
        //}

        public async Task<(bool, Mess)> AddApprovalFirst(int id, int ObjType)
        {
            Mess mess = new Mess();
            try
            {
                List<OWDD> listowdd = new List<OWDD>();

                ODOC doc = await _context.ODOC
                    .Include(p => p.PaymentMethod)
                    .FirstOrDefaultAsync(x => x.Id == id);

                var ocrd = await _context.BP
                    .Include(p => p.CRD3)
                    .Include(p => p.CRD4)
                    .FirstOrDefaultAsync(p => p.Id == doc.CardId);
                if (ocrd == null)
                {
                    mess.Status = 400;
                    mess.Errors = "Khách hàng không tồn tại";
                    return (false, mess);
                }

                var crd3 = ocrd.CRD3.ToList();
                var crd4 = ocrd.CRD4.ToList();
                bool Balance, BalanceOver;
                var paymentMethod = doc.PaymentMethod.FirstOrDefault();
                if (paymentMethod.PaymentMethodName.ToString() != "Thanh toán ngay")
                {
                    var r3 = crd3.Where(p => p.PaymentMethodID == paymentMethod.PaymentMethodID).FirstOrDefault();
                    if (doc.Total + r3.Balance > r3.BalanceLimit)
                    {
                        Balance = true;
                    }

                    var r4 = crd4.Where(p => p.AmountOverdue > 0).FirstOrDefault();
                    if (r4 != null)
                    {
                        BalanceOver = true;
                    }
                }

                var owtm = _context.OWTM.Include(p => p.WTM2).ThenInclude(p => p.OWST).ThenInclude(p => p.WST1)
                    .Include(p => p.WTM3.Where(p => p.TransType == 22))
                    .Include(p => p.WTM4)
                    .Where(p => p.Active == true && p.WTM4.Other == doc.Other &&
                                p.WTM4.LitmitOver == doc.LimitOverDue && p.WTM4.Litmit == doc.LimitRequire);
                if (owtm != null)
                {
                    foreach (var o in owtm)
                    {
                        OWDD owdd = new OWDD();
                        owdd.Status = "W";
                        owdd.WtmId = o.Id;
                        owdd.DocId = doc.Id;
                        owdd.ObjType = (int)doc.ObjType;
                        owdd.DocDate = DateTime.Now;
                        var wmt2 = o.WTM2.FirstOrDefault(p => p.Sort == 1);
                        owdd.CurrStep = wmt2.Sort;
                        owdd.MaxRejReqr = wmt2.OWST.MaxRejReqr;
                        owdd.MaxReqr = wmt2.OWST.MaxReqr;
                        List<WDD1> listWDD1 = new List<WDD1>();
                        foreach (var st in wmt2.OWST.WST1.ToList())
                        {
                            WDD1 wdd1 = new WDD1();
                            wdd1.StepCode = st.Id;
                            wdd1.UserId = st.UserId;
                            wdd1.SortId = wmt2.Sort; 
                            wdd1.Status = "W";
                            wdd1.CreateDate = DateTime.Now;
                            listWDD1.Add(wdd1);
                        }

                        owdd.WDD1 = listWDD1;
                        listowdd.Add(owdd);
                    }
                }

                _context.OWDD.AddRange(listowdd);
                await _context.SaveChangesAsync();
                return (true, null);
            }
            catch (Exception ex)
            {
                mess.Status = 900;
                mess.Errors = ex.Message;
                return (false, mess);
            }
        }

        public async Task<Mess?> PathUpdate(int id, ODOC patchDoc)
        {
            var mess = new Mess();
            try
            {
                var doc = await _context.ODOC.FirstOrDefaultAsync(p => p.Id == id);
                if (doc == null)
                {
                    mess.Errors = "ODOC not found";
                    mess.Status = 404;
                    return mess;
                }

                // _mapper.Map(patchDoc, doc);
                doc.SellerNote = patchDoc.SellerNote;
                _context.Update(doc);
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

        public async Task<Mess?> AddAttDocuments(int id, int userId, List<IFormFile> files)
        {
            var mess = new Mess();

            await using var trans = await _context.Database.BeginTransactionAsync();
            try
            {
                var doc = await _context.ODOC.Include(p => p.AttDocuments).FirstOrDefaultAsync(p => p.Id == id);
                if (doc == null)
                {
                    mess.Errors = "ODOC not found";
                    mess.Status = 404;
                    return mess;
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
                        HttpRequest? request = _httpContextAccessor.HttpContext?.Request;
                        if (request != null)
                        {
                            var baseUrl = $"{request.Scheme}://{request.Host}";
                            var fileUrl = $"{baseUrl}/uploads/{fileName}";

                            var newAttDoc = new AttDocument
                            {
                                FatherId = doc.Id,
                                FileName = file.FileName,
                                AuthorId = userId,
                                FilePath = fileUrl,
                            };

                            doc.AttDocuments.Add(newAttDoc);
                        }
                    }
                }

                await _context.SaveChangesAsync();
                await trans.CommitAsync();
                return null;
            }
            catch (Exception ex)
            {
                await trans.RollbackAsync();
                mess.Errors = ex.Message;
                mess.Status = 500;
                return mess;
            }
        }

        public async Task<Mess?> RemoveAttDocuments(int id, List<int> fileIDs)
        {
            var mess = new Mess();
            await using var trans = await _context.Database.BeginTransactionAsync();
            try
            {
                var doc = await _context.ODOC.Include(p => p.AttDocuments).FirstOrDefaultAsync(p => p.Id == id);
                if (doc == null)
                {
                    mess.Errors = "ODOC not found";
                    mess.Status = 404;
                    await trans.RollbackAsync();
                    return mess;
                }

                doc!.AttDocuments!.RemoveAll(p => fileIDs.Contains(p.Id));
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

        public async Task<Mess?> AddAttFile(int id, int userId, List<IFormFile> files)
        {
            var mess = new Mess();

            await using var trans = await _context.Database.BeginTransactionAsync();
            try
            {
                var doc = await _context.ODOC.Include(p => p.AttDocuments).FirstOrDefaultAsync(p => p.Id == id);
                if (doc == null)
                {
                    mess.Errors = "ODOC not found";
                    mess.Status = 404;
                    return mess;
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
                        HttpRequest? request = _httpContextAccessor.HttpContext?.Request;
                        if (request != null)
                        {
                            var baseUrl = $"{request.Scheme}://{request.Host}";
                            var fileUrl = $"{baseUrl}/uploads/{fileName}";

                            var newAttDoc = new DOC14()
                            {
                                FatherId = doc.Id,
                                FileName = file.FileName,
                                AuthorId = userId,
                                FilePath = fileUrl,
                            };

                            doc.AttachFile.Add(newAttDoc);
                        }
                    }
                }

                await _context.SaveChangesAsync();
                await trans.CommitAsync();
                return null;
            }
            catch (Exception ex)
            {
                await trans.RollbackAsync();
                mess.Errors = ex.Message;
                mess.Status = 500;
                return mess;
            }
        }

        public async Task<Mess?> RemoveAttFile(int id, List<int> fileIDs)
        {
            var mess = new Mess();
            await using var trans = await _context.Database.BeginTransactionAsync();
            try
            {
                var doc = await _context.ODOC.Include(p => p.AttachFile).FirstOrDefaultAsync(p => p.Id == id);
                if (doc == null)
                {
                    mess.Errors = "ODOC not found";
                    mess.Status = 404;
                    await trans.RollbackAsync();
                    return mess;
                }

                doc!.AttachFile!.RemoveAll(p => fileIDs.Contains(p.Id));
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

        public async Task<bool> SyncIssueToSapAsync()
        {
            try
            {
                string Db = EncryptDecrypt.DecryptString(_configuration["SAPServiceLayer:CompanyDB"]);
                string Username = EncryptDecrypt.DecryptString(_configuration["SAPServiceLayer:Username"]);
                string Password = EncryptDecrypt.DecryptString(_configuration["SAPServiceLayer:Password"]);
                Cookies cookiess = null; ;
                var l = new LoginInfor
                {
                    CompanyDB = Db,
                    UserName = Username,
                    Password = Password
                };
                string url = _configuration["SAPServiceLayer:Host"];
                string data = JsonConvert.SerializeObject(l);
                var doc = _context.ODOC
                .Include(x => x.ItemDetail)
                .Include(x => x.Address)
                .Include(x => x.BP)
                .AsNoTracking().AsSplitQuery()
                .FirstOrDefault(x => x.Status == "DXN" && x.IsSync == false && x.ObjType == 1250000001);
                if (doc != null)
                {
                    if (cookiess == null || cookiess.SessionTime < DateTime.Now)
                    {

                        HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url + "Login");
                        httpWebRequest.ContentType = "application/json";
                        httpWebRequest.Method = "POST";
                        httpWebRequest.KeepAlive = true;
                        httpWebRequest.ServerCertificateValidationCallback += BackEndAPI.Service.Sync.Security.TlsBypass.AcceptSelfSignedInDev();
                        httpWebRequest.Headers.Add("B1S-WCFCompatible", "true");
                        httpWebRequest.Headers.Add("B1S-MetadataWithoutSession", "true");
                        httpWebRequest.Accept = "*/*";
                        httpWebRequest.ServicePoint.Expect100Continue = false;
                        httpWebRequest.Headers.Add("Accept-Encoding", "gzip, deflate, br");
                        httpWebRequest.AutomaticDecompression = DecompressionMethods.GZip;

                        using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                        { streamWriter.Write(data); }
                        try
                        {
                            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                            var cookie = httpResponse.Headers.GetValues("Set-Cookie");
                            cookiess = new Cookies();
                            int endIndex = cookie[1].ToString().IndexOf(";");
                            string routeid = cookie[1].ToString().Substring(0, endIndex);
                            endIndex = cookie[0].ToString().IndexOf(";");
                            string sesion = cookie[0].ToString().Substring(0, endIndex + 1);
                            cookiess.ROUTEID = routeid;
                            cookiess.B1SESSION = sesion;
                            cookiess.SessionTime = DateTime.Now.AddMinutes(20);
                        }
                        catch (WebException ex)
                        {
                        }
                    }
                    HttpWebRequest httpWebRequestsdraft = (HttpWebRequest)WebRequest.Create(url + "Drafts?$filter=U_MDHPT eq '" + doc.InvoiceCode + "'&$select = DocEntry,U_MDHPT");
                    httpWebRequestsdraft.ContentType = "application/json";
                    httpWebRequestsdraft.Method = "GET";
                    httpWebRequestsdraft.KeepAlive = true;
                    httpWebRequestsdraft.ServerCertificateValidationCallback += BackEndAPI.Service.Sync.Security.TlsBypass.AcceptSelfSignedInDev();
                    httpWebRequestsdraft.Headers.Add("B1S-WCFCompatible", "true");
                    httpWebRequestsdraft.Headers.Add("B1S-MetadataWithoutSession", "true");
                    httpWebRequestsdraft.Accept = "*/*";
                    httpWebRequestsdraft.ServicePoint.Expect100Continue = false;
                    httpWebRequestsdraft.Headers.Add("Accept-Encoding", "gzip, deflate, br");
                    httpWebRequestsdraft.Headers.Add("Cookie", cookiess.B1SESSION + cookiess.ROUTEID);
                    httpWebRequestsdraft.AutomaticDecompression = DecompressionMethods.GZip;
                    try
                    {
                        var httpResponse = (HttpWebResponse)httpWebRequestsdraft.GetResponse();
                        if (httpResponse.StatusCode == HttpStatusCode.OK)
                        {
                            using (var reader = new StreamReader(httpResponse.GetResponseStream()))
                            {
                                var responseText = reader.ReadToEnd();


                                var dataxxx = JsonConvert.DeserializeObject<DraftResponse>(responseText);

                                if (dataxxx?.Value?.Count > 0)
                                {
                                    var bp = dataxxx.Value[0];
                                    if (doc.InvoiceCode == bp.U_MDHPT)
                                    {
                                        doc.IsSync = true;
                                        doc.ItemDetail.ForEach(item =>
                                        {
                                            item.IsSync = true;
                                            item.DraftId = bp.DocEntry.ToString();
                                        });
                                        _context.ODOC.Update(doc);
                                        await _context.SaveChangesAsync();
                                        return true;
                                    }
                                }

                            }

                        }

                    }
                    catch (WebException exx)
                    {

                    }
                    var line1 = doc.ItemDetail.Where(e=>e.Quantity > 0).Select(x =>
                    new IssueLine()
                    {
                        Quantity = x.Quantity,
                        ItemCode = x.ItemCode,
                    }).ToList();

                    string U_NNHG = "";
                    string U_BSX = "";
                    string U_CMND = "";
                    string U_Description_vn = "";
                    if (doc.Address != null)
                    {
                        var add = doc.Address.FirstOrDefault(e => e.Type == "S");
                        if (add != null)
                        {
                            U_NNHG = add.Person ?? "";
                            U_BSX = add.VehiclePlate ?? "";
                            U_CMND = add.CCCD ?? "";
                            U_Description_vn = add.Address ?? "";
                        }
                    }

                    var issue = new Issue
                    {
                        U_BPCode = doc.BP.SapCardCode,
                        U_NPPH = doc.BP.CardName,
                        U_MDHPT = doc.InvoiceCode,
                        DocDate = DateTime.Now.ToString("yyyy-MM-dd"),
                        U_NNHG = U_NNHG,
                        U_BSX = U_BSX,
                        U_CMND = U_CMND,
                        U_Description_vn = U_Description_vn
                    };
                    if (line1.Count > 0)
                    {
                        issue.DocumentLines = line1;
                        try
                        {
                            if (cookiess == null || cookiess.SessionTime < DateTime.Now)
                            {

                                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url + "Login");
                                httpWebRequest.ContentType = "application/json";
                                httpWebRequest.Method = "POST";
                                httpWebRequest.KeepAlive = true;
                                httpWebRequest.ServerCertificateValidationCallback += BackEndAPI.Service.Sync.Security.TlsBypass.AcceptSelfSignedInDev();
                                httpWebRequest.Headers.Add("B1S-WCFCompatible", "true");
                                httpWebRequest.Headers.Add("B1S-MetadataWithoutSession", "true");
                                httpWebRequest.Accept = "*/*";
                                httpWebRequest.ServicePoint.Expect100Continue = false;
                                httpWebRequest.Headers.Add("Accept-Encoding", "gzip, deflate, br");
                                httpWebRequest.AutomaticDecompression = DecompressionMethods.GZip;

                                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                                { streamWriter.Write(data); }
                                try
                                {
                                    var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                                    var cookie = httpResponse.Headers.GetValues("Set-Cookie");
                                    cookiess = new Cookies();
                                    int endIndex = cookie[1].ToString().IndexOf(";");
                                    string routeid = cookie[1].ToString().Substring(0, endIndex);
                                    endIndex = cookie[0].ToString().IndexOf(";");
                                    string sesion = cookie[0].ToString().Substring(0, endIndex + 1);
                                    cookiess.ROUTEID = routeid;
                                    cookiess.B1SESSION = sesion;
                                    cookiess.SessionTime = DateTime.Now.AddMinutes(20);
                                }
                                catch (WebException ex)
                                {
                                    return true;
                                }
                            }
                            HttpWebRequest httpWebRequests = (HttpWebRequest)WebRequest.Create(url + "Drafts");
                            httpWebRequests.ContentType = "application/json";
                            httpWebRequests.Method = "POST";
                            httpWebRequests.KeepAlive = true;
                            httpWebRequests.ServerCertificateValidationCallback += BackEndAPI.Service.Sync.Security.TlsBypass.AcceptSelfSignedInDev();
                            httpWebRequests.Headers.Add("B1S-WCFCompatible", "true");
                            httpWebRequests.Headers.Add("B1S-MetadataWithoutSession", "true");
                            httpWebRequests.Accept = "*/*";
                            httpWebRequests.ServicePoint.Expect100Continue = false;
                            httpWebRequests.Headers.Add("Accept-Encoding", "gzip, deflate, br");
                            httpWebRequests.Headers.Add("Cookie", cookiess.B1SESSION + cookiess.ROUTEID);
                            httpWebRequests.AutomaticDecompression = DecompressionMethods.GZip;
                            var json = JsonConvert.SerializeObject(issue);
                            //var content = new StringContent(json, Encoding.UTF8, "application/json");
                            using (var streamWriter = new StreamWriter(httpWebRequests.GetRequestStream()))

                            { streamWriter.Write(json); }
                            try
                            {
                                var httpResponse = (HttpWebResponse)httpWebRequests.GetResponse();
                                doc.IsSync = true;
                            }
                            catch (WebException ex)
                            {
                                return true;
                            }


                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                            return true;
                        }
                    }
                    _context.ODOC.Update(doc);
                    await _context.SaveChangesAsync();
                }
                return true;
            }
            catch
            {
                return true;
            }
        }

        public async Task<Mess?> UpdateIssueStatus(int id, string status)
        {
            var mess = new Mess();
            var doc = await _context.ODOC
                .Include(x => x.ItemDetail).ThenInclude(doc1 => doc1.Item)
                .Include(x => x.BP)
                .Include(x => x.Promotion)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (doc.ObjType == 1250000001)
            {
                switch (status)
                {
                    case "DXN":
                        var wUser = await _context.AppUser
                            .Include(u => u.UserRoles)
                            .ThenInclude(ur => ur.Role)
                            .Where(u => u.UserRoles.Any(ur => ur.Role.Name == "customer_service"))
                            .ToListAsync();

                        _eventAggregator.Publish(new Models.NotificationModels.Notification
                        {
                            Message = $"Yêu cầu lấy hàng {doc.InvoiceCode} đã được xác nhận",
                            Title = $"Yêu cầu lấy hàng {doc.InvoiceCode} đã được xác nhận, cần bạn lên kế hoạch giao hàng",
                            Type = "info",
                            Object = new Models.NotificationModels.NotificationObject
                            {
                                ObjId = doc.Id,
                                ObjName = "-",
                                ObjType = "order",
                            },
                            SendUsers = wUser.Select(u => u.Id).ToList(),
                        });


                        break;
                    case "DGH":
                        var sUser = await _context.AppUser
                            .Include(u => u.UserRoles)
                            .ThenInclude(ur => ur.Role)
                            .Where(u => u.UserRoles.Any(ur => ur.Role.Name == "customer_service"))
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
                                ObjType = "order",
                            },
                            SendUsers = sendUser,
                        });
                        break;
                    case "DHT":
                        var cUser1 = await _context.AppUser
                            .Where(u => u.CardId == doc.CardId).Select(u => u.Id).FirstOrDefaultAsync();
                        _eventAggregator.Publish(new Models.NotificationModels.Notification
                        {
                            Title = $"Yêu cầu lấy hàng {doc.InvoiceCode} đã hoàn thành",
                            Message = "Yêu cầu lấy hàng đã giao đến bạn thành công, cảm ơn bạn đã tin tưởng chúng tôi",
                            Type = "info",
                            Object = new Models.NotificationModels.NotificationObject
                            {
                                ObjId = doc.Id,
                                ObjName = "-",
                                ObjType = "order",
                            },
                            SendUsers = new List<int> { cUser1 },
                        });
                        break;
                }
            }


            if (doc == null)
            {
                mess.Errors = "not found";
                mess.Status = 404;
                return mess;
            }

            doc.Status = status;
            await _context.SaveChangesAsync();
            return null;
        }

        public async Task<(ODOC, Mess)> AddDocumentORFSAsync(ORFS doc)
        {
            Mess mess = new Mess();
            ODOC model = new ODOC();
            var user = _httpContextAccessor.HttpContext?.User;
            var userId = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            _modelUpdater.UpdateModel(model, doc, "ItemDetail", "Address", "PaymentMethod", "NA3", "NA4", "NA5");
            model.UserId = int.Parse(userId);
            List<DOC1> listItemdetail = new List<DOC1>();

            foreach (var detail in doc.RFS1.ToList())
            {
                DOC1 itemdetail = new DOC1();
                _modelUpdater.UpdateModel(itemdetail, detail, "NA1", "NA1", "NA1", "NA3", "NA4", "NA5");
                listItemdetail.Add(itemdetail);
            }
            model.ItemDetail = listItemdetail;
            
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    string notiType = "";
                    string notiTitle = "";
                    var message = "";
                    var codes = "";

                    notiType = "approval";
                    message = "Yêu cầu cấp mẫu thử nghiệm cần phê duyệt";
                    notiTitle = "Yêu cầu cấp mẫu thử nghiệm {0}";
                    codes = await GenerateDocument("FC", "", 10, model);
                    if (model.ItemDetail.Count == 0 || model.ItemDetail == null)
                    {
                        mess.Status = 400;
                        mess.Errors = "Chi tiết phiếu trống";
                        transaction.Rollback();
                        return (null, mess);
                    }
                    model.ObjType = 50;
                    model.InvoiceCode = codes;
                    
                    _context.ODOC.Add(model);

                    await _context.SaveChangesAsync();

                    if (doc.Status == "NHAP")
                    {
                        await transaction.CommitAsync();
                        return (model, null);
                    }
                    
                    var apts = _context.WTM2
                            .Include(a => a.OWST)
                            .ThenInclude(b => b!.WST1)
                            .Include(d => d.OWTM)
                            .ThenInclude(d => d.RUsers)
                            .Include(d => d.OWTM)
                            .ThenInclude(d => d.WTM3)
                            .Where(c => c.Sort == 1 && c.OWTM.WTM3.Select(e=>e.TransType).ToList().Contains(50)).ToList();
                    List<Models.Approval.Approval> approval = new List<Models.Approval.Approval>();
                    foreach (var apt in apts)
                    {
                        var l = apt.OWTM.RUsers.Select(e => e.Id).ToList();
                        if(l.Contains((int)model.UserId))
                        {
                            var line = new List<ApprovalLine>();
                            if (apt is { OWST.WST1: not null })
                                line.AddRange
                                (
                                    apt.OWST.WST1.Select(item => new ApprovalLine { Status = "P", StepCode = 1, UserId = item.UserId, WstId = item.FatherId }
                                ));
                            Models.Approval.Approval app1 = new Models.Approval.Approval
                            {
                                ActorId =  model.UserId,
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
                    if(approval.Count > 0)
                    {
                        _context.Approval.AddRange(approval);
                        _context.SaveChanges();
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
                        _context.ODOC.Update(model);
                        // qua nhieu SaveChange
                        // await _context.SaveChangesAsync();
                    }   
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return (model, null);
                }
                catch (Exception ex)
                {
                    mess.Status = 900;
                    mess.Errors = ex.Message;
                    transaction.Rollback();
                    return (null, mess);
                }
            }
        }

        public async Task<(IEnumerable<ORFS>, int total, Mess)> GetAllDocumentORFSAsync(GridifyQuery q, string? search, int? userId)
        {
            Mess mess = new Mess();
            try
            {
                var config = new GridifyMapper<ODOC>()
                    .GenerateMappings()
                    .AddMap("docDate", p => p.DocDate!.Value.Date);
                var query = _context.Set<ODOC>().AsQueryable();
                if (!string.IsNullOrEmpty(search))
                {
                    search = search.Trim();
                    query = query.Where(d =>
                        ((d.CardCode != null && d.CardCode.Contains(search)) ||
                        (d.InvoiceCode != null && d.InvoiceCode.Contains(search)) || d.CardName.Contains(search)));
                }

                if (userId is not null)
                {
                    var usr = await _context.AppUser.AsSplitQuery().AsQueryable()
                        .Include(xx => xx.DirectStaff)
                        .Where(xx => xx.Id == userId)
                        .Include(xx => xx.Role)
                        .FirstOrDefaultAsync();
                    if (usr.Role != null)
                    {
                        if (usr.Role.NormalizedName == "NHÂN VIÊN KINH DOANH")
                        {
                            query = query
                            .Where(x => x.BP.SaleId != null && (x.BP.SaleId == usr.Id
                                                                || usr.DirectStaff.Select(d => d.Id)
                                                                    .Contains(x.BP.SaleId ?? -1)));
                        }    
                            
                    }
                }
                query = query.Where(p => p.ObjType == 50);
                var totalCount = await query.ApplyFiltering(q, config).CountAsync();
                var items = await query.OrderBy(p => p.DocDate)
                    .Include(p => p.ItemDetail)
                    .OrderByDescending(p => p.Id)
                    .ApplyFiltering(q, config)
                    .ApplyPaging(q.Page, q.PageSize)
                    .ToListAsync();
                List<ORFS> listItems = new List<ORFS>();
                foreach (var item in items)
                {
                    ORFS model = new ORFS();
                    _modelUpdater.UpdateModel(model, item, "ItemDetail", "Address", "PaymentMethod", "NA3", "NA4", "NA5");


                    Collection<RFS1> listItemdetail = new Collection<RFS1>();

                    foreach (var detail in item.ItemDetail.ToList())
                    {
                        RFS1 itemdetail = new RFS1();
                        _modelUpdater.UpdateModel(itemdetail, detail, "NA1", "NA1", "NA1", "NA3", "NA4", "NA5");
                        listItemdetail.Add(itemdetail);
                    }
                    model.RFS1 = listItemdetail;
                    
                    listItems.Add(model);
                }
                return (listItems, totalCount, null);
            }
            catch (Exception ex)
            {
                mess.Status = 900;
                mess.Errors = ex.Message;
                return (null, 0, mess);
            }
        }

        public async Task<(ORFS, Mess)> GetAllDocumentORFSAsync(int id)
        {
            Mess mess = new Mess();
            try
            {
                var orfs = _context.ODOC.Include(e=>e.ItemDetail).FirstOrDefault(p => p.Id == id && p.ObjType ==50);
                if (orfs == null)
                    return (null, null);
                ORFS model = new ORFS();
                _modelUpdater.UpdateModel(model, orfs, "ItemDetail", "Address", "PaymentMethod", "NA3", "NA4", "NA5");


                Collection<RFS1> listItemdetail = new Collection<RFS1>();

                foreach (var detail in orfs.ItemDetail.ToList())
                {
                    RFS1 itemdetail = new RFS1();
                    _modelUpdater.UpdateModel(itemdetail, detail, "NA1", "NA1", "NA1", "NA3", "NA4", "NA5");
                    listItemdetail.Add(itemdetail);
                }
                model.RFS1 = listItemdetail;

                return (model, null);
            }
            catch (Exception ex)
            {
                mess.Status = 900;
                mess.Errors = ex.Message;
                return (null,mess);
            }
        }

        public async Task<(ORFS, Mess)> UpdateDocumentORFSAsync(int id, ORFS doc)
        {
            Mess mess = new Mess();
            try
            {
                var model = _context.ODOC
                    .Include(x => x.ItemDetail)
                    .AsNoTracking().AsSplitQuery()
                    .FirstOrDefault(x => x.Id == id);
                model.Status = "HT";
                if (model == null)
                {
                    mess.Status = 900;
                    mess.Errors = "Chứng từ không tồn tại";
                    return (null, mess);
                }
                if (doc.Id != id)
                {
                    mess.Status = 900;
                    mess.Errors = "Chứng từ không tồn tại";
                    return (null, mess);
                }
                if (doc.RFS1.Count > 0)
                {
                    foreach (var detail in doc.RFS1.ToList())
                    {
                        var dt = model.ItemDetail.FirstOrDefault(x => x.Id == detail.Id);
                        dt.Result = detail.Result;
                    }
                }
                _context.Update(model);
                await _context.SaveChangesAsync();
                doc.Status = "HT";
                return (doc, null);
            }
            catch (Exception ex)
            {
                mess.Status = 900;
                mess.Errors = ex.Message;
                return (null, mess);
            }
        }

        public async Task<Mess?> SendZalo(int DocId, string TypeMess)
        {
            Mess ms = new Mess();
            try
            {
                var doc = _context.ODOC.FirstOrDefault(e => e.Id == DocId);
                
                    
                if (doc != null)
                {
                    string cust = "";
                    var CRD5 = _context.BP.Include(e => e.CRD5.Where(e => e.Default == "Y")).Where(e => e.Id == doc.CardId).Select(e => e.CRD5).FirstOrDefault();

                    if (CRD5?.FirstOrDefault()?.Person == "" || CRD5?.FirstOrDefault()?.Person == null)
                        cust = doc.CardName ?? "";
                    else
                        cust = CRD5?.FirstOrDefault()?.Person ?? "";
                    if (TypeMess == "Confirmed")
                    {
                        if(doc.ZaloConfirmed == true)
                        {
                            ms.Errors = "Đã gửi thông báo xác nhận đơn hàng";
                            ms.Status = 400;
                            return ms;
                        }

                        ZNSOrderConfirm confirm = new ZNSOrderConfirm();
                        confirm.phone = "84" + CRD5?.FirstOrDefault()?.Phone.Substring(1);
                        confirm.tracking_id = doc.InvoiceCode;
                        confirm.template_id = "509811";
                        confirm.template_data = new detail1
                        {
                            amount = (int)Math.Round(doc.Total ?? 0, MidpointRounding.AwayFromZero),
                            orderId = doc.InvoiceCode ?? "",
                            store = doc.CardName ?? "",
                            customer = cust ?? "",
                            date1 = DateTime.UtcNow.ToString("dd/MM/yyyy"),
                            date2 = DateTime.UtcNow.AddDays(3).ToString("dd/MM/yyyy"),
                        };
                        await _zService.SendOrderConfirm(confirm);

                    }
                    else if (TypeMess == "Completed")
                    {
                        if (doc.ZaloCompleted == true)
                        {
                            ms.Errors = "Đã gửi thông báo hoàn thành đơn hàng";
                            ms.Status = 400;
                            return ms;
                        }
                        if(doc.ZaloConfirmed == false)
                        {
                            ms.Errors = "Xác nhận đơn trước khi hoàn thành";
                            ms.Status = 400;
                            return ms;
                        }    
                        ZNSOrderCompleted completed = new ZNSOrderCompleted();
                        completed.phone = "84" + CRD5?.FirstOrDefault()?.Phone?.Substring(1);
                        completed.tracking_id = doc.InvoiceCode;
                        completed.template_id = "509815";
                        completed.template_data = new detail2
                        {
                            orderId = doc.InvoiceCode ?? "",
                            amount = (int)Math.Round(doc.Total ?? 0, MidpointRounding.AwayFromZero),
                            store = doc.CardName ?? "",
                            customer = cust ?? "",
                            date1 = DateTime.UtcNow.ToString("dd/MM/yyyy")
                        };
                        await _zService.SendOrderCompleted(completed);
                    }
                    else
                    {
                        ms.Errors = "Loại gửi thông báo không đúng";
                        ms.Status = 400;
                        return ms;
                    }
                   
                }
                else
                {
                    ms.Errors = "Không tìm thấy số chứng từ";
                    ms.Status = 400;
                    return ms;
                }
                return null;
            }catch(Exception ex)
            {
                ms.Errors = ex.Message;
                ms.Status = 900;
                return ms;
            }
        }

        public async Task<(Rating, Mess)> CreateOrderRatingAsync(CreateOrderRatingDto dto)
        {
            Mess mes = new Mess();
            try
            {
                var user = _httpContextAccessor.HttpContext?.User;
                var userId = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var order = _context.ODOC.FirstOrDefault(e=>e.Id == dto.OrderId);
                if (order == null)
                {
                    mes.Errors = "Không tìm thấy đơn hàng";
                    mes.Status = 900;
                    return (null, mes);
                }
                var rate = _context.Ratings.FirstOrDefault(e => e.Id == dto.OrderId);
                if (rate != null)
                {
                    mes.Errors = "Đơn hàng đã được đánh giá";
                    mes.Status = 900;
                    return (null, mes);
                }  
                var rating = new Rating
                {
                    UserId = int.Parse(userId),
                    DocId = dto.OrderId,
                    RatingType = RatingType.Order,
                    QualityScore = dto.QualityScore,
                    ServiceScore = dto.ServiceScore,
                    ShipScore = dto.ShipScore,
                    Comment = dto.Comment,
                    Images = new List<RatingImage>()
                };

                // Upload image
                if (dto.Images != null)
                {
                    foreach (var img in dto.Images)
                    {
                        var uploadsFolder = Path.Combine(_webHostEnvironment.ContentRootPath, "uploads");
                        Directory.CreateDirectory(uploadsFolder);
                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(img.FileName);
                        var filePath = Path.Combine(uploadsFolder, fileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await img.CopyToAsync(fileStream);
                        }

                        HttpRequest? request = _httpContextAccessor.HttpContext?.Request;
                        if (request != null)
                        {
                            var baseUrl = $"{request.Scheme}://{request.Host}";
                            var imageUrl = $"{baseUrl}/uploads/{fileName}";
                            rating.Images.Add(new RatingImage { ImageUrl = imageUrl});
                        }
                    }
                }

                await _context.AddAsync(rating);
                await _context.SaveChangesAsync();
                var wUser = await _context.AppUser
                                .Include(u => u.UserRoles)
                                .ThenInclude(ur => ur.Role)
                                .Where(u => u.UserRoles.Any(ur => ur.Role.Name == "Nhân viên CSKH"))
                                .ToListAsync();
                _eventAggregator.Publish(new Models.NotificationModels.Notification
                {
                    Message = $"Khách hàng {order.CardName} đã đánh giá đơn hàng {order.InvoiceCode}",
                    Title = $"Khách hàng {order.CardName} đã đánh giá đơn hàng {order.InvoiceCode}",
                    Type = "info",
                    Object = new Models.NotificationModels.NotificationObject
                    {
                        ObjId = order.Id,
                        ObjName = "-",
                        ObjType = "rating",
                    },
                    SendUsers = wUser.Select(u => u.Id).ToList(),
                });
                return (rating,null);

            }
            catch(Exception ex)
            {
                mes.Errors = ex.Message;
                mes.Status = 900;
                return (null, mes);
            }
        }

        public async Task<(Rating, Mess)> CreateGeneralRatingAsync(CreateGeneralRatingDto dto)
        {
            Mess mes = new Mess();
            try
            {
                var user = _httpContextAccessor.HttpContext?.User;
                var userId = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var rating = new Rating
                {
                    UserId = int.Parse(userId),
                    RatingType = RatingType.Order,
                    QualityScore = dto.QualityScore,
                    ServiceScore = dto.ServiceScore,
                    ShipScore = dto.ShipScore,
                    Comment = dto.Comment,
                    Images = new List<RatingImage>()
                };

                // Upload image
                if (dto.Images != null)
                {
                    foreach (var img in dto.Images)
                    {
                        var uploadsFolder = Path.Combine(_webHostEnvironment.ContentRootPath, "uploads");
                        Directory.CreateDirectory(uploadsFolder);
                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(img.FileName);
                        var filePath = Path.Combine(uploadsFolder, fileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await img.CopyToAsync(fileStream);
                        }

                        HttpRequest? request = _httpContextAccessor.HttpContext?.Request;
                        if (request != null)
                        {
                            var baseUrl = $"{request.Scheme}://{request.Host}";
                            var imageUrl = $"{baseUrl}/uploads/{fileName}";
                            rating.Images.Add(new RatingImage { ImageUrl = imageUrl });
                        }
                    }
                }

                await _context.AddAsync(rating);
                await _context.SaveChangesAsync();
                var wUser = await _context.AppUser
                                .Include(u => u.UserRoles)
                                .ThenInclude(ur => ur.Role)
                                .Where(u => u.UserRoles.Any(ur => ur.Role.Name == "Nhân viên CSKH"))
                                .ToListAsync();
                _eventAggregator.Publish(new Models.NotificationModels.Notification
                {
                    Message = $"Đã có một đánh giá từ phía khách hàng",
                    Title = $"Đã có một đánh giá từ phía khách hàng",
                    Type = "info",
                    Object = new Models.NotificationModels.NotificationObject
                    {
                        ObjId = rating.Id,
                        ObjName = "-",
                        ObjType = "rating",
                    },
                    SendUsers = wUser.Select(u => u.Id).ToList(),
                });
                return (rating, null);

            }
            catch (Exception ex)
            {
                mes.Errors = ex.Message;
                mes.Status = 900;
                return (null, mes);
            }
        }

        public async Task<(IEnumerable<RatingDto>, Mess, int)> GetGeneralRatingAsync(GridifyQuery q)
        {
            Mess mes = new Mess();
            try
            {
                var user = _httpContextAccessor.HttpContext?.User;
                var userId = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var cardId = _context.Users.FirstOrDefault(e=>e.Id == int.Parse(userId))?.CardId;
                if(cardId == null || cardId == 0)
                {
                    var rate = _context.Ratings.Where(e=>!e.DocId.HasValue)
                            .Join(_context.Users, r => r.UserId, u => u.Id, (r, u) => new { r, u })
                            .Join(_context.BP, ru => ru.u.CardId, c => c.Id, (ru, c) => new { ru.r, ru.u, c })
                            .Select(x => new RatingDto
                               {
                                Id = x.r.Id,
                                CardCode = x.c.CardCode,
                                CardName = x.c.CardName,
                                QualityScore = x.r.QualityScore,
                                ServiceScore = x.r.ServiceScore,
                                ShipScore = x.r.ShipScore,
                                Comment = x.r.Comment,
                                CreatedAt = x.r.CreatedAt,
                                Images = _context.RatingImages
                                .Where(i => i.RatingId == x.r.Id)
                                .Select(i => new RatingImage { ImageUrl = i.ImageUrl })
                                .ToList()
                            }).AsQueryable();
                    var total = await rate.ApplyFiltering(q).CountAsync();
                    var result = await rate.ApplyFiltering(q).ToListAsync();
                    return (result, null, total);
                }
                else
                {
                    var rate = _context.Ratings.Where(e=>e.UserId == int.Parse(userId) && !e.DocId.HasValue)
                            .Join(_context.Users, r => r.UserId, u => u.Id, (r, u) => new { r, u })
                            .Join(_context.BP, ru => ru.u.CardId, c => c.Id, (ru, c) => new { ru.r, ru.u, c })
                            .Select(x => new RatingDto
                            {
                                Id = x.r.Id,
                                CardCode = x.c.CardCode,
                                CardName = x.c.CardName,
                                QualityScore = x.r.QualityScore,
                                ServiceScore = x.r.ServiceScore,
                                ShipScore = x.r.ShipScore,
                                Comment = x.r.Comment,
                                CreatedAt = x.r.CreatedAt,
                                Images = _context.RatingImages
                                .Where(i => i.RatingId == x.r.Id)
                                .Select(i => new RatingImage { ImageUrl = i.ImageUrl })
                                .ToList()
                            }).AsQueryable();
                    var total = await rate.ApplyFiltering(q).CountAsync();
                    var result = await rate.ApplyFiltering(q).ToListAsync();
                    return (result, null, total);
                }
            }
            catch (Exception ex)
            {
                mes.Errors = ex.Message;
                mes.Status = 900;
                return (null, mes,0);
            }
        }

        public async Task<(RatingDto, Mess)> GetGeneralRatingAsync(int Id)
        {
            Mess mes = new Mess();
            try
            { 
                var rate = _context.Ratings.Where(e =>e.Id == Id && !e.DocId.HasValue)
                            .Join(_context.Users, r => r.UserId, u => u.Id, (r, u) => new { r, u })
                            .Join(_context.BP, ru => ru.u.CardId, c => c.Id, (ru, c) => new { ru.r, ru.u, c })
                            .Select(x => new RatingDto
                            {
                                Id = x.r.Id,
                                CardCode = x.c.CardCode,
                                CardName = x.c.CardName,
                                QualityScore = x.r.QualityScore,
                                ServiceScore = x.r.ServiceScore,
                                ShipScore = x.r.ShipScore,
                                Comment = x.r.Comment,
                                CreatedAt = x.r.CreatedAt,
                                Images = _context.RatingImages
                                .Where(i => i.RatingId == x.r.Id)
                                .Select(i => new RatingImage { ImageUrl = i.ImageUrl })
                                .ToList()
                            }).AsQueryable();
                var result = await rate.FirstOrDefaultAsync();
                return (result, null);
            }
            catch (Exception ex)
            {
                mes.Errors = ex.Message;
                mes.Status = 900;
                return (null, mes);
            }
        }

        public async Task<(IEnumerable<SyncInvoiceErrorDto>, Mess, int)> SyncInvoiceError(GridifyQuery q)
        {
            Mess mes = new Mess();
            try
            {
                var query = _context.ODOC.ApplyFiltering(q)
                .Where(t1 => t1.IsSync == false && t1.Status == "DXN"  && t1.ObjType != 50);
                var totalItems = await query.ApplyFiltering(q).CountAsync();
                var items = await query
                .Select(t1 => new SyncInvoiceErrorDto
                {
                    Id = t1.Id,
                    InvoiceCode = t1.InvoiceCode,
                    ObjType = t1.ObjType ?? 0,
                    CardCode = t1.CardCode,
                    CardId = t1.CardId ?? 0,
                    CardName = t1.CardName,
                    Error = string.Join(",", t1.ItemDetail.Select(d => d.Error ?? ""))
                })
                .ToListAsync();
                return (items, null, totalItems);
            }
            catch (Exception ex)
            {
                mes.Errors = ex.Message;
                mes.Status = 900;
                return (null, mes,0);
            }
        }

        public async Task<bool> SyncToSapAsync(int DocID)
        {
            try
            {
                string Db = EncryptDecrypt.DecryptString(_configuration["SAPServiceLayer:CompanyDB"]);
                string Username = EncryptDecrypt.DecryptString(_configuration["SAPServiceLayer:Username"]);
                string Password = EncryptDecrypt.DecryptString(_configuration["SAPServiceLayer:Password"]);
                Cookies cookiess = null; ;
                var l = new LoginInfor
                {
                    CompanyDB = Db,
                    UserName = Username,
                    Password = Password
                };
                string url = _configuration["SAPServiceLayer:Host"];
                string data = JsonConvert.SerializeObject(l);
                var doc = _context.ODOC
                .Include(x => x.ItemDetail)
                .Include(x => x.Promotion)
                .Include(x => x.BP)
                .ThenInclude(x => x.CRD3)
                .Include(x => x.PaymentInfo)
                .FirstOrDefault(x => x.Status == "DXN" && x.ObjType == 22 && x.IsSync == false && x.Id == DocID);
                if (doc != null)
                {
                    if (cookiess == null || cookiess.SessionTime < DateTime.Now)
                    {

                        HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url + "Login");
                        httpWebRequest.ContentType = "application/json";
                        httpWebRequest.Method = "POST";
                        httpWebRequest.KeepAlive = true;
                        httpWebRequest.ServerCertificateValidationCallback += BackEndAPI.Service.Sync.Security.TlsBypass.AcceptSelfSignedInDev();
                        httpWebRequest.Headers.Add("B1S-WCFCompatible", "true");
                        httpWebRequest.Headers.Add("B1S-MetadataWithoutSession", "true");
                        httpWebRequest.Accept = "*/*";
                        httpWebRequest.ServicePoint.Expect100Continue = false;
                        httpWebRequest.Headers.Add("Accept-Encoding", "gzip, deflate, br");
                        httpWebRequest.AutomaticDecompression = DecompressionMethods.GZip;

                        using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                        { streamWriter.Write(data); }
                        try
                        {
                            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                            var cookie = httpResponse.Headers.GetValues("Set-Cookie");
                            cookiess = new Cookies();
                            int endIndex = cookie[1].ToString().IndexOf(";");
                            string routeid = cookie[1].ToString().Substring(0, endIndex);
                            endIndex = cookie[0].ToString().IndexOf(";");
                            string sesion = cookie[0].ToString().Substring(0, endIndex + 1);
                            cookiess.ROUTEID = routeid;
                            cookiess.B1SESSION = sesion;
                            cookiess.SessionTime = DateTime.Now.AddMinutes(20);
                        }
                        catch (WebException ex)
                        {
                        }
                    }
                    HttpWebRequest httpWebRequestsdraft = (HttpWebRequest)WebRequest.Create(url + "Drafts?$filter=U_MDHPT eq '" + doc.InvoiceCode + "'&$select = DocEntry,U_MDHPT");
                    httpWebRequestsdraft.ContentType = "application/json";
                    httpWebRequestsdraft.Method = "GET";
                    httpWebRequestsdraft.KeepAlive = true;
                    httpWebRequestsdraft.ServerCertificateValidationCallback += BackEndAPI.Service.Sync.Security.TlsBypass.AcceptSelfSignedInDev();
                    httpWebRequestsdraft.Headers.Add("B1S-WCFCompatible", "true");
                    httpWebRequestsdraft.Headers.Add("B1S-MetadataWithoutSession", "true");
                    httpWebRequestsdraft.Accept = "*/*";
                    httpWebRequestsdraft.ServicePoint.Expect100Continue = false;
                    httpWebRequestsdraft.Headers.Add("Accept-Encoding", "gzip, deflate, br");
                    httpWebRequestsdraft.Headers.Add("Cookie", cookiess.B1SESSION + cookiess.ROUTEID);
                    httpWebRequestsdraft.AutomaticDecompression = DecompressionMethods.GZip;
                    try
                    {
                        var httpResponse = (HttpWebResponse)httpWebRequestsdraft.GetResponse();
                        if (httpResponse.StatusCode == HttpStatusCode.OK)
                        {
                            using (var reader = new StreamReader(httpResponse.GetResponseStream()))
                            {
                                var responseText = reader.ReadToEnd();


                                var dataxxx = JsonConvert.DeserializeObject<DraftResponse>(responseText);

                                if (dataxxx?.Value?.Count > 0)
                                {
                                    var bp = dataxxx.Value[0];
                                    if (doc.InvoiceCode == bp.U_MDHPT)
                                    {
                                        doc.IsSync = true;
                                        doc.ItemDetail.ForEach(item =>
                                        {
                                            item.IsSync = true;
                                            item.DraftId = bp.DocEntry.ToString();
                                        });
                                        await _context.SaveChangesAsync();
                                        return true;
                                    }
                                }

                            }

                        }

                    }
                    catch (WebException exx)
                    {

                    }
                    var line1 = doc.ItemDetail.Where(x => x.PaymentMethodCode == "PayNow" && x.Quantity > 0).Select(x =>
                    new SapOrderLine()
                    {
                        Quantity = x.Quantity,
                        ItemCode = x.ItemCode,
                        LineTotal = x.PriceAfterDist * x.Quantity,
                        UnitPrice = x.Price,
                        VatGroup = "SVN5",
                        LineId = x.LineId,
                        U_LHH = "01",
                    }).ToList();
                    var line2 = doc.ItemDetail.Where(x => x.PaymentMethodCode == "PayCredit" && x.Quantity > 0).Select(x =>
                        new SapOrderLine()
                        {
                            Quantity = x.Quantity,
                            ItemCode = x.ItemCode,
                            LineTotal = x.PriceAfterDist * x.Quantity,
                            UnitPrice = x.Price,
                            LineId = x.LineId,
                            U_LHH = "01",
                            VatGroup = "SVN5"
                        }).ToList();
                    var line3 = doc.ItemDetail.Where(x => x.PaymentMethodCode == "PayGuarantee" && x.Quantity > 0).Select(x =>
                        new SapOrderLine()
                        {
                            Quantity = x.Quantity,
                            ItemCode = x.ItemCode,
                            LineTotal = x.PriceAfterDist * x.Quantity,
                            UnitPrice = x.Price,
                            LineId = x.LineId,
                            U_LHH = "01",
                            VatGroup = "SVN5"
                        }).ToList();
                    SapOrderBP sl = new SapOrderBP();
                    try
                    {
                        //cookiess = await EnsureLoginAsync(cookiess, url, data);
                        if (cookiess == null || cookiess.SessionTime < DateTime.Now)
                        {

                            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url + "Login");
                            httpWebRequest.ContentType = "application/json";
                            httpWebRequest.Method = "POST";
                            httpWebRequest.KeepAlive = true;
                            httpWebRequest.ServerCertificateValidationCallback += BackEndAPI.Service.Sync.Security.TlsBypass.AcceptSelfSignedInDev();
                            httpWebRequest.Headers.Add("B1S-WCFCompatible", "true");
                            httpWebRequest.Headers.Add("B1S-MetadataWithoutSession", "true");
                            httpWebRequest.Accept = "*/*";
                            httpWebRequest.ServicePoint.Expect100Continue = false;
                            httpWebRequest.Headers.Add("Accept-Encoding", "gzip, deflate, br");
                            httpWebRequest.AutomaticDecompression = DecompressionMethods.GZip;

                            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                            { streamWriter.Write(data); }
                            try
                            {
                                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                                var cookie = httpResponse.Headers.GetValues("Set-Cookie");
                                cookiess = new Cookies();
                                int endIndex = cookie[1].ToString().IndexOf(";");
                                string routeid = cookie[1].ToString().Substring(0, endIndex);
                                endIndex = cookie[0].ToString().IndexOf(";");
                                string sesion = cookie[0].ToString().Substring(0, endIndex + 1);
                                cookiess.ROUTEID = routeid;
                                cookiess.B1SESSION = sesion;
                                cookiess.SessionTime = DateTime.Now.AddMinutes(20);
                            }
                            catch (WebException ex)
                            {
                            }
                        }
                        HttpWebRequest httpWebRequests = (HttpWebRequest)WebRequest.Create(url + "BusinessPartners?$select=CardCode,U_DiachiHD,FederalTaxID,U_Khuvuc&$filter=CardCode eq '" + doc.BP.CardCode + "'");
                        httpWebRequests.ContentType = "application/json";
                        httpWebRequests.Method = "GET";
                        httpWebRequests.KeepAlive = true;
                        httpWebRequests.ServerCertificateValidationCallback += BackEndAPI.Service.Sync.Security.TlsBypass.AcceptSelfSignedInDev();
                        httpWebRequests.Headers.Add("B1S-WCFCompatible", "true");
                        httpWebRequests.Headers.Add("B1S-MetadataWithoutSession", "true");
                        httpWebRequests.Accept = "*/*";
                        httpWebRequests.ServicePoint.Expect100Continue = false;
                        httpWebRequests.Headers.Add("Accept-Encoding", "gzip, deflate, br");
                        httpWebRequests.Headers.Add("Cookie", cookiess.B1SESSION + cookiess.ROUTEID);
                        httpWebRequests.AutomaticDecompression = DecompressionMethods.GZip;
                        try
                        {
                            var httpResponse = (HttpWebResponse)httpWebRequests.GetResponse();
                            if (httpResponse.StatusCode == HttpStatusCode.OK)
                            {
                                using (var reader = new StreamReader(httpResponse.GetResponseStream()))
                                {
                                    var responseText = reader.ReadToEnd();


                                    var dataxxx = JsonConvert.DeserializeObject<BPResponse>(responseText);

                                    if (dataxxx?.Value?.Count > 0)
                                    {
                                        var bp = dataxxx.Value[0];
                                        sl.CardCode = bp.CardCode;
                                        sl.FederalTaxID = bp.FederalTaxID;
                                        sl.U_DiachiHD = bp.U_DiachiHD;
                                        sl.U_Khuvuc = bp.U_Khuvuc;
                                    }

                                }

                            }

                        }
                        catch (WebException ex)
                        {

                        }
                    }
                    catch (Exception ex)
                    {
                    }
                    string U_NNHG = "";
                    string U_BSX = "";
                    string U_CMND = "";
                    string U_Description_vn = "";
                    if (doc.Address != null)
                    {
                        var add = doc.Address.FirstOrDefault(e => e.Type == "S");
                        if (add != null)
                        {
                            U_NNHG = add.Person ?? "";
                            U_BSX = add.VehiclePlate ?? "";
                            U_CMND = add.CCCD ?? "";
                            U_Description_vn = add.Address ?? "";
                        }
                    }
                    var order1 = new SapOrder
                    {
                        CardCode = doc.BP.CardCode,
                        DocDate = DateTime.Now.ToString("yyyy-MM-dd"),
                        DocDueDate = DateTime.Now.ToString("yyyy-MM-dd"),
                        DocObjectCode = "17",
                        DocCurrency = doc.Currency,
                        U_NVKD1 = "NBT2",
                        U_NVKD2 = "NBT2",
                        U_NVKD3 = "NBT2",
                        U_NVKD4 = "NBT2",
                        U_MDHPT = doc.InvoiceCode,
                        U_Area = sl.U_Khuvuc,
                        U_CoAdd = sl.U_DiachiHD,
                        U_CoTaxNo = sl.FederalTaxID,
                        U_NNHG = U_NNHG,
                        U_BSX = U_BSX,
                        U_CMND = U_CMND,
                        U_Description_vn = U_Description_vn
                    };
                    var timeTC = doc.BP.CRD3?.FirstOrDefault(p => p.PaymentMethodID == 2)?.Times ?? 0;
                    var timeBL = doc.BP.CRD3?.FirstOrDefault(p => p.PaymentMethodID == 3)?.Times ?? 0;
                    var order2 = new SapOrder
                    {
                        CardCode = doc.BP.CardCode,
                        DocDate = DateTime.Now.ToString("yyyy-MM-dd"),
                        DocDueDate = DateTime.Now.AddDays(timeTC).ToString("yyyy-MM-dd"),
                        DocObjectCode = "17",
                        DocCurrency = doc.Currency,
                        U_NVKD1 = "NBT2",
                        U_NVKD2 = "NBT2",
                        U_NVKD3 = "NBT2",
                        U_NVKD4 = "NBT2",
                        U_MDHPT = doc.InvoiceCode,
                        U_Area = sl.U_Khuvuc,
                        U_CoAdd = sl.U_DiachiHD,
                        U_CoTaxNo = sl.FederalTaxID,
                        U_NNHG = U_NNHG,
                        U_BSX = U_BSX,
                        U_CMND = U_CMND,
                        U_Description_vn = U_Description_vn
                    };
                    var order3 = new SapOrder
                    {
                        CardCode = doc.BP.CardCode,
                        DocDate = DateTime.Now.ToString("yyyy-MM-dd"),
                        DocDueDate = DateTime.Now.AddDays(timeTC).ToString("yyyy-MM-dd"),
                        DocObjectCode = "17",
                        DocCurrency = doc.Currency,
                        U_NVKD1 = "NBT2",
                        U_NVKD2 = "NBT2",
                        U_NVKD3 = "NBT2",
                        U_NVKD4 = "NBT2",
                        U_MDHPT = doc.InvoiceCode,
                        U_Area = sl.U_Khuvuc,
                        U_CoAdd = sl.U_DiachiHD,
                        U_CoTaxNo = sl.FederalTaxID,
                        U_NNHG = U_NNHG,
                        U_BSX = U_BSX,
                        U_CMND = U_CMND,
                        U_Description_vn = U_Description_vn
                    };
                    var itemKM = _context.Item.ToList();
                    if (line1.Count > 0)
                    {
                        var ids1 = line1.Select(x => x.LineId).ToList();

                        var pp = doc.Promotion.Where(p => ids1.Contains(p.LineId) && !p.ItemCode.IsNullOrEmpty() && (p.QuantityAdd ?? 0) > 0).ToList();
                        pp.ForEach(item =>
                        {
                            var price = itemKM.FirstOrDefault(e => e.ItemCode == item.ItemCode)?.Price ?? 0;
                            item.Price = price;
                        });
                        line1.AddRange(pp.Select(x => new SapOrderLine()
                        {
                            Quantity = x.QuantityAdd ?? 0,
                            ItemCode = x.ItemCode,
                            LineTotal = 0,
                            UnitPrice = x.Price,
                            VatGroup = "SVN4",
                            U_LHH = "02",
                            U_CTKM = x.PromotionCode
                        }));
                        if ((doc.BonusAmount ?? 0) != 0)
                            order1.Comments = "Thưởng thanh toán ngay " + doc.Bonus + "%: " + doc.BonusAmount;
                        order1.DocumentLines = line1;
                        order1.U_HTTT1 = "1";

                        try
                        {
                            //cookiess = await EnsureLoginAsync(cookiess, url, data);
                            if (cookiess == null || cookiess.SessionTime < DateTime.Now)
                            {

                                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url + "Login");
                                httpWebRequest.ContentType = "application/json";
                                httpWebRequest.Method = "POST";
                                httpWebRequest.KeepAlive = true;
                                httpWebRequest.ServerCertificateValidationCallback += BackEndAPI.Service.Sync.Security.TlsBypass.AcceptSelfSignedInDev();
                                httpWebRequest.Headers.Add("B1S-WCFCompatible", "true");
                                httpWebRequest.Headers.Add("B1S-MetadataWithoutSession", "true");
                                httpWebRequest.Accept = "*/*";
                                httpWebRequest.ServicePoint.Expect100Continue = false;
                                httpWebRequest.Headers.Add("Accept-Encoding", "gzip, deflate, br");
                                httpWebRequest.AutomaticDecompression = DecompressionMethods.GZip;

                                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                                { streamWriter.Write(data); }
                                try
                                {
                                    var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                                    var cookie = httpResponse.Headers.GetValues("Set-Cookie");
                                    cookiess = new Cookies();
                                    int endIndex = cookie[1].ToString().IndexOf(";");
                                    string routeid = cookie[1].ToString().Substring(0, endIndex);
                                    endIndex = cookie[0].ToString().IndexOf(";");
                                    string sesion = cookie[0].ToString().Substring(0, endIndex + 1);
                                    cookiess.ROUTEID = routeid;
                                    cookiess.B1SESSION = sesion;
                                    cookiess.SessionTime = DateTime.Now.AddMinutes(20);
                                }
                                catch (WebException ex)
                                {
                                }
                            }
                            HttpWebRequest httpWebRequests = (HttpWebRequest)WebRequest.Create(url + "Orders");
                            httpWebRequests.ContentType = "application/json";
                            httpWebRequests.Method = "POST";
                            httpWebRequests.KeepAlive = true;
                            httpWebRequests.ServerCertificateValidationCallback += BackEndAPI.Service.Sync.Security.TlsBypass.AcceptSelfSignedInDev();
                            httpWebRequests.Headers.Add("B1S-WCFCompatible", "true");
                            httpWebRequests.Headers.Add("B1S-MetadataWithoutSession", "true");
                            httpWebRequests.Accept = "*/*";
                            httpWebRequests.ServicePoint.Expect100Continue = false;
                            httpWebRequests.Headers.Add("Accept-Encoding", "gzip, deflate, br");
                            httpWebRequests.Headers.Add("Cookie", cookiess.B1SESSION + cookiess.ROUTEID);
                            httpWebRequests.AutomaticDecompression = DecompressionMethods.GZip;
                            var json = JsonConvert.SerializeObject(order1);
                            //var content = new StringContent(json, Encoding.UTF8, "application/json");
                            using (var streamWriter = new StreamWriter(httpWebRequests.GetRequestStream()))

                            { streamWriter.Write(json); }
                            try
                            {
                                var httpResponse = (HttpWebResponse)httpWebRequests.GetResponse();
                                doc.IsSync = true;
                                var listI = line1.Select(x => x.LineId).ToList();
                                doc.ItemDetail.Where(p => listI.Contains(p.LineId)).ToList().ForEach(p => { p.IsSync = true; });

                            }
                            catch (WebException ex)
                            {
                                var resp = ex.Response as HttpWebResponse;
                                if (resp?.StatusCode == HttpStatusCode.NotFound)
                                {
                                    var draft = resp?.Headers["Location"]?.ToString();
                                    if (!draft.IsNullOrEmpty())
                                    {
                                        var match = Regex.Match(draft ?? "", @"\((\d+)\)");
                                        var draftId = match.Success ? int.Parse(match.Groups[1].Value) : 0;
                                        HttpWebRequest httpWebRequestsdraft1 = (HttpWebRequest)WebRequest.Create(url + "Drafts?$filter=DocEntry eq " + draftId + " and U_MDHPT eq '" + doc.InvoiceCode + "'&$select = DocEntry ,U_MDHPT");
                                        httpWebRequestsdraft1.ContentType = "application/json";
                                        httpWebRequestsdraft1.Method = "GET";
                                        httpWebRequestsdraft1.KeepAlive = true;
                                        httpWebRequestsdraft1.ServerCertificateValidationCallback += BackEndAPI.Service.Sync.Security.TlsBypass.AcceptSelfSignedInDev();
                                        httpWebRequestsdraft1.Headers.Add("B1S-WCFCompatible", "true");
                                        httpWebRequestsdraft1.Headers.Add("B1S-MetadataWithoutSession", "true");
                                        httpWebRequestsdraft1.Accept = "*/*";
                                        httpWebRequestsdraft1.ServicePoint.Expect100Continue = false;
                                        httpWebRequestsdraft1.Headers.Add("Accept-Encoding", "gzip, deflate, br");
                                        httpWebRequestsdraft1.Headers.Add("Cookie", cookiess.B1SESSION + cookiess.ROUTEID);
                                        httpWebRequestsdraft1.AutomaticDecompression = DecompressionMethods.GZip;
                                        try
                                        {
                                            var httpResponse = (HttpWebResponse)httpWebRequestsdraft1.GetResponse();
                                            if (httpResponse.StatusCode == HttpStatusCode.OK)
                                            {
                                                using (var reader = new StreamReader(httpResponse.GetResponseStream()))
                                                {
                                                    var responseText = reader.ReadToEnd();


                                                    var dataxxx = JsonConvert.DeserializeObject<DraftResponse>(responseText);

                                                    if (dataxxx?.Value?.Count > 0)
                                                    {
                                                        var bp = dataxxx.Value[0];
                                                        if (doc.InvoiceCode == bp.U_MDHPT)
                                                        {
                                                            doc.IsSync = true;
                                                            var listI = line1.Select(x => x.LineId).ToList();
                                                            doc.ItemDetail.Where(p => listI.Contains(p.LineId)).ToList().ForEach(p => { p.IsSync = true; p.DraftId = draftId.ToString(); });
                                                        }
                                                    }

                                                }

                                            }

                                        }
                                        catch (WebException exx)
                                        {

                                        }

                                    }
                                    else
                                    {
                                        var listI = line1.Select(x => x.LineId).ToList();
                                        string responseContent = string.Empty;
                                        using (var stream = resp.GetResponseStream())
                                        using (var reader = new StreamReader(stream))
                                        {
                                            responseContent = reader.ReadToEnd();
                                        }
                                        try
                                        {
                                            var jsonDoc = JsonDocument.Parse(responseContent);
                                            var root = jsonDoc.RootElement;
                                            doc.IsSync = false;
                                            if (root.TryGetProperty("error", out var error))
                                            {
                                                var code = error.GetProperty("code").GetInt32();
                                                var message = error.GetProperty("message").GetProperty("value").GetString();
                                                doc.ItemDetail.Where(p => listI.Contains(p.LineId)).ToList().ForEach(p => { p.Error = message; });
                                            }
                                            else
                                            {
                                                doc.ItemDetail.Where(p => listI.Contains(p.LineId)).ToList().ForEach(p => { p.Error = "Không thể phân tích phản hồi lỗi từ SAP."; });
                                            }
                                        }
                                        catch
                                        {
                                            doc.ItemDetail.Where(p => listI.Contains(p.LineId)).ToList().ForEach(p => { p.Error = "Không thể đọc phản hồi lỗi (body không phải JSON)."; });
                                        }
                                    }
                                }

                            }


                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }

                    if (line2.Count > 0)
                    {
                        var ids1 = line2.Select(x => x.LineId).ToList();
                        var pp = doc.Promotion.Where(p => ids1.Contains(p.LineId) && !p.ItemCode.IsNullOrEmpty() && (p.QuantityAdd ?? 0) > 0).ToList();
                        pp.ForEach(item =>
                        {
                            var price = itemKM.FirstOrDefault(e => e.ItemCode == item.ItemCode)?.Price ?? 0;
                            item.Price = price;
                        });
                        line2.AddRange(pp.Select(x => new SapOrderLine()
                        {
                            Quantity = x.QuantityAdd ?? 0,
                            ItemCode = x.ItemCode,
                            LineTotal = 0,
                            UnitPrice = x.Price,
                            VatGroup = "SVN4",
                            U_LHH = "02",
                            U_CTKM = x.PromotionCode
                        }));
                        order2.DocumentLines = line2;
                        order2.U_HTTT1 = "2";
                        //cookiess = await EnsureLoginAsync(cookiess, url, data);
                        if (cookiess == null || cookiess.SessionTime < DateTime.Now)
                        {
                            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url + "Login");
                            httpWebRequest.ContentType = "application/json";
                            httpWebRequest.Method = "POST";
                            httpWebRequest.KeepAlive = true;
                            httpWebRequest.ServerCertificateValidationCallback += BackEndAPI.Service.Sync.Security.TlsBypass.AcceptSelfSignedInDev();
                            httpWebRequest.Headers.Add("B1S-WCFCompatible", "true");
                            httpWebRequest.Headers.Add("B1S-MetadataWithoutSession", "true");
                            httpWebRequest.Accept = "*/*";
                            httpWebRequest.ServicePoint.Expect100Continue = false;
                            httpWebRequest.Headers.Add("Accept-Encoding", "gzip, deflate, br");
                            httpWebRequest.AutomaticDecompression = DecompressionMethods.GZip;

                            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                            { streamWriter.Write(data); }
                            try
                            {
                                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                                var cookie = httpResponse.Headers.GetValues("Set-Cookie");
                                cookiess = new Cookies();
                                int endIndex = cookie[1].ToString().IndexOf(";");
                                string routeid = cookie[1].ToString().Substring(0, endIndex);
                                endIndex = cookie[0].ToString().IndexOf(";");
                                string sesion = cookie[0].ToString().Substring(0, endIndex + 1);
                                cookiess.ROUTEID = routeid;
                                cookiess.B1SESSION = sesion;
                                cookiess.SessionTime = DateTime.Now.AddMinutes(20);
                            }
                            catch (WebException ex)
                            {
                            }
                        }
                        HttpWebRequest httpWebRequests = (HttpWebRequest)WebRequest.Create(url + "Orders");
                        httpWebRequests.ContentType = "application/json";
                        httpWebRequests.Method = "POST";
                        httpWebRequests.KeepAlive = true;
                        httpWebRequests.ServerCertificateValidationCallback += BackEndAPI.Service.Sync.Security.TlsBypass.AcceptSelfSignedInDev();
                        httpWebRequests.Headers.Add("B1S-WCFCompatible", "true");
                        httpWebRequests.Headers.Add("B1S-MetadataWithoutSession", "true");
                        httpWebRequests.Accept = "*/*";
                        httpWebRequests.ServicePoint.Expect100Continue = false;
                        httpWebRequests.Headers.Add("Accept-Encoding", "gzip, deflate, br");
                        httpWebRequests.Headers.Add("Cookie", cookiess.B1SESSION + cookiess.ROUTEID);
                        httpWebRequests.AutomaticDecompression = DecompressionMethods.GZip;
                        var json = JsonConvert.SerializeObject(order2);
                        //var content = new StringContent(json, Encoding.UTF8, "application/json");
                        using (var streamWriter = new StreamWriter(httpWebRequests.GetRequestStream()))

                        { streamWriter.Write(json); }
                        try
                        {
                            var httpResponse = (HttpWebResponse)httpWebRequests.GetResponse();
                            doc.IsSync = true;
                            var listI = line2.Select(x => x.LineId).ToList();
                            doc.ItemDetail.Where(p => listI.Contains(p.LineId)).ToList().ForEach(p => { p.IsSync = true; });
                        }
                        catch (WebException ex)
                        {
                            var resp = (HttpWebResponse)ex.Response;
                            if (resp?.StatusCode == HttpStatusCode.NotFound)
                            {
                                var draft = resp?.Headers["Location"]?.ToString();
                                if (!draft.IsNullOrEmpty())
                                {
                                    var match = Regex.Match(draft ?? "", @"\((\d+)\)");
                                    var draftId = match.Success ? int.Parse(match.Groups[1].Value) : 0;
                                    HttpWebRequest httpWebRequestsdraft2 = (HttpWebRequest)WebRequest.Create(url + "Drafts?$filter=DocEntry eq " + draftId + " and U_MDHPT eq '" + doc.InvoiceCode + "'&$select = DocEntry,U_MDHPT");
                                    httpWebRequestsdraft2.ContentType = "application/json";
                                    httpWebRequestsdraft2.Method = "GET";
                                    httpWebRequestsdraft2.KeepAlive = true;
                                    httpWebRequestsdraft2.ServerCertificateValidationCallback += BackEndAPI.Service.Sync.Security.TlsBypass.AcceptSelfSignedInDev();
                                    httpWebRequestsdraft2.Headers.Add("B1S-WCFCompatible", "true");
                                    httpWebRequestsdraft2.Headers.Add("B1S-MetadataWithoutSession", "true");
                                    httpWebRequestsdraft2.Accept = "*/*";
                                    httpWebRequestsdraft2.ServicePoint.Expect100Continue = false;
                                    httpWebRequestsdraft2.Headers.Add("Accept-Encoding", "gzip, deflate, br");
                                    httpWebRequestsdraft2.Headers.Add("Cookie", cookiess.B1SESSION + cookiess.ROUTEID);
                                    httpWebRequestsdraft2.AutomaticDecompression = DecompressionMethods.GZip;
                                    try
                                    {
                                        var httpResponse = (HttpWebResponse)httpWebRequestsdraft2.GetResponse();
                                        if (httpResponse.StatusCode == HttpStatusCode.OK)
                                        {
                                            using (var reader = new StreamReader(httpResponse.GetResponseStream()))
                                            {
                                                var responseText = reader.ReadToEnd();


                                                var dataxxx = JsonConvert.DeserializeObject<DraftResponse>(responseText);

                                                if (dataxxx?.Value?.Count > 0)
                                                {
                                                    var bp = dataxxx.Value[0];
                                                    if (doc.InvoiceCode == bp.U_MDHPT)
                                                    {
                                                        doc.IsSync = true;
                                                        var listI = line1.Select(x => x.LineId).ToList();
                                                        doc.ItemDetail.Where(p => listI.Contains(p.LineId)).ToList().ForEach(p => { p.IsSync = true; p.DraftId = draftId.ToString(); });
                                                    }
                                                }

                                            }

                                        }

                                    }
                                    catch (WebException exx)
                                    {

                                    }
                                }
                                else
                                {
                                    var listI = line1.Select(x => x.LineId).ToList();
                                    string responseContent = string.Empty;
                                    using (var stream = resp.GetResponseStream())
                                    using (var reader = new StreamReader(stream))
                                    {
                                        responseContent = reader.ReadToEnd();
                                    }
                                    try
                                    {
                                        var jsonDoc = JsonDocument.Parse(responseContent);
                                        var root = jsonDoc.RootElement;
                                        doc.IsSync = false;
                                        if (root.TryGetProperty("error", out var error))
                                        {
                                            var code = error.GetProperty("code").GetInt32();
                                            var message = error.GetProperty("message").GetProperty("value").GetString();
                                            doc.ItemDetail.Where(p => listI.Contains(p.LineId)).ToList().ForEach(p => { p.Error = message; });
                                        }
                                        else
                                        {
                                            doc.ItemDetail.Where(p => listI.Contains(p.LineId)).ToList().ForEach(p => { p.Error = "Không thể phân tích phản hồi lỗi từ SAP."; });
                                        }
                                    }
                                    catch
                                    {
                                        doc.ItemDetail.Where(p => listI.Contains(p.LineId)).ToList().ForEach(p => { p.Error = "Không thể đọc phản hồi lỗi (body không phải JSON)."; });
                                    }
                                }
                            }
                        }
                    }

                    if (line3.Count > 0)
                    {
                        var ids1 = line3.Select(x => x.LineId).ToList();
                        var pp = doc.Promotion.Where(p => ids1.Contains(p.LineId) && !p.ItemCode.IsNullOrEmpty() && (p.QuantityAdd ?? 0) > 0).ToList();
                        pp.ForEach(item =>
                        {
                            var price = itemKM.FirstOrDefault(e => e.ItemCode == item.ItemCode)?.Price ?? 0;
                            item.Price = price;
                        });
                        line3.AddRange(pp.Select(x => new SapOrderLine()
                        {
                            Quantity = x.QuantityAdd ?? 0,
                            ItemCode = x.ItemCode,
                            LineTotal = 0,
                            UnitPrice = x.Price,
                            VatGroup = "SVN4",
                            U_LHH = "02",
                            U_CTKM = x.PromotionCode
                        }));
                        order3.DocumentLines = line3;
                        order3.U_HTTT1 = "3";
                        //cookiess = await EnsureLoginAsync(cookiess, url, data);
                        if (cookiess == null || cookiess.SessionTime < DateTime.Now)
                        {
                            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url + "Login");
                            httpWebRequest.ContentType = "application/json";
                            httpWebRequest.Method = "POST";
                            httpWebRequest.KeepAlive = true;
                            httpWebRequest.ServerCertificateValidationCallback += BackEndAPI.Service.Sync.Security.TlsBypass.AcceptSelfSignedInDev();
                            httpWebRequest.Headers.Add("B1S-WCFCompatible", "true");
                            httpWebRequest.Headers.Add("B1S-MetadataWithoutSession", "true");
                            httpWebRequest.Accept = "*/*";
                            httpWebRequest.ServicePoint.Expect100Continue = false;
                            httpWebRequest.Headers.Add("Accept-Encoding", "gzip, deflate, br");
                            httpWebRequest.AutomaticDecompression = DecompressionMethods.GZip;

                            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                            { streamWriter.Write(data); }
                            try
                            {
                                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                                var cookie = httpResponse.Headers.GetValues("Set-Cookie");
                                cookiess = new Cookies();
                                int endIndex = cookie[1].ToString().IndexOf(";");
                                string routeid = cookie[1].ToString().Substring(0, endIndex);
                                endIndex = cookie[0].ToString().IndexOf(";");
                                string sesion = cookie[0].ToString().Substring(0, endIndex + 1);
                                cookiess.ROUTEID = routeid;
                                cookiess.B1SESSION = sesion;
                                cookiess.SessionTime = DateTime.Now.AddMinutes(20);
                            }
                            catch (WebException ex)
                            {
                            }
                        }
                        HttpWebRequest httpWebRequests = (HttpWebRequest)WebRequest.Create(url + "Orders");
                        httpWebRequests.ContentType = "application/json";
                        httpWebRequests.Method = "POST";
                        httpWebRequests.KeepAlive = true;
                        httpWebRequests.ServerCertificateValidationCallback += BackEndAPI.Service.Sync.Security.TlsBypass.AcceptSelfSignedInDev();
                        httpWebRequests.Headers.Add("B1S-WCFCompatible", "true");
                        httpWebRequests.Headers.Add("B1S-MetadataWithoutSession", "true");
                        httpWebRequests.Accept = "*/*";
                        httpWebRequests.ServicePoint.Expect100Continue = false;
                        httpWebRequests.Headers.Add("Accept-Encoding", "gzip, deflate, br");
                        httpWebRequests.Headers.Add("Cookie", cookiess.B1SESSION + cookiess.ROUTEID);
                        httpWebRequests.AutomaticDecompression = DecompressionMethods.GZip;
                        var json = JsonConvert.SerializeObject(order3);
                        //var content = new StringContent(json, Encoding.UTF8, "application/json");
                        using (var streamWriter = new StreamWriter(httpWebRequests.GetRequestStream()))

                        { streamWriter.Write(json); }
                        try
                        {
                            var httpResponse = (HttpWebResponse)httpWebRequests.GetResponse();
                            doc.IsSync = true;
                            var listI = line3.Select(x => x.LineId).ToList();
                            doc.ItemDetail.Where(p => listI.Contains(p.LineId)).ToList().ForEach(p => { p.IsSync = true; });
                        }
                        catch (WebException ex)
                        {
                            var resp = (HttpWebResponse)ex.Response;
                            if (resp?.StatusCode == HttpStatusCode.NotFound)
                            {
                                var draft = resp?.Headers["Location"]?.ToString();
                                if (!draft.IsNullOrEmpty())
                                {
                                    var match = Regex.Match(draft ?? "", @"\((\d+)\)");
                                    var draftId = match.Success ? int.Parse(match.Groups[1].Value) : 0;
                                    HttpWebRequest httpWebRequestsdraft3 = (HttpWebRequest)WebRequest.Create(url + "Drafts?$filter=DocEntry eq " + draftId + " and U_MDHPT eq '" + doc.InvoiceCode + "'&$select = DocEntry,U_MDHPT");
                                    httpWebRequestsdraft3.ContentType = "application/json";
                                    httpWebRequestsdraft3.Method = "GET";
                                    httpWebRequestsdraft3.KeepAlive = true;
                                    httpWebRequestsdraft3.ServerCertificateValidationCallback += BackEndAPI.Service.Sync.Security.TlsBypass.AcceptSelfSignedInDev();
                                    httpWebRequestsdraft3.Headers.Add("B1S-WCFCompatible", "true");
                                    httpWebRequestsdraft3.Headers.Add("B1S-MetadataWithoutSession", "true");
                                    httpWebRequestsdraft3.Accept = "*/*";
                                    httpWebRequestsdraft3.ServicePoint.Expect100Continue = false;
                                    httpWebRequestsdraft3.Headers.Add("Accept-Encoding", "gzip, deflate, br");
                                    httpWebRequestsdraft3.Headers.Add("Cookie", cookiess.B1SESSION + cookiess.ROUTEID);
                                    httpWebRequestsdraft3.AutomaticDecompression = DecompressionMethods.GZip;
                                    try
                                    {
                                        var httpResponse = (HttpWebResponse)httpWebRequestsdraft3.GetResponse();
                                        if (httpResponse.StatusCode == HttpStatusCode.OK)
                                        {
                                            using (var reader = new StreamReader(httpResponse.GetResponseStream()))
                                            {
                                                var responseText = reader.ReadToEnd();


                                                var dataxxx = JsonConvert.DeserializeObject<DraftResponse>(responseText);

                                                if (dataxxx?.Value?.Count > 0)
                                                {
                                                    var bp = dataxxx.Value[0];
                                                    if (doc.InvoiceCode == bp.U_MDHPT)
                                                    {
                                                        doc.IsSync = true;
                                                        var listI = line1.Select(x => x.LineId).ToList();
                                                        doc.ItemDetail.Where(p => listI.Contains(p.LineId)).ToList().ForEach(p => { p.IsSync = true; p.DraftId = draftId.ToString(); });
                                                    }
                                                }

                                            }

                                        }

                                    }
                                    catch (WebException exx)
                                    {

                                    }
                                    //doc.IsSync = true;
                                    //var listI = line1.Select(x => x.LineId).ToList();
                                    //doc.ItemDetail.Where(p => listI.Contains(p.LineId)).ToList().ForEach(p => { p.IsSync = true; p.DraftId = draftId.ToString(); });
                                }
                                else
                                {
                                    var listI = line1.Select(x => x.LineId).ToList();
                                    string responseContent = string.Empty;
                                    using (var stream = resp.GetResponseStream())
                                    using (var reader = new StreamReader(stream))
                                    {
                                        responseContent = reader.ReadToEnd();
                                    }
                                    try
                                    {
                                        var jsonDoc = JsonDocument.Parse(responseContent);
                                        var root = jsonDoc.RootElement;
                                        doc.IsSync = false;
                                        if (root.TryGetProperty("error", out var error))
                                        {
                                            var code = error.GetProperty("code").GetInt32();
                                            var message = error.GetProperty("message").GetProperty("value").GetString();
                                            doc.ItemDetail.Where(p => listI.Contains(p.LineId)).ToList().ForEach(p => { p.Error = message; });
                                        }
                                        else
                                        {
                                            doc.ItemDetail.Where(p => listI.Contains(p.LineId)).ToList().ForEach(p => { p.Error = "Không thể phân tích phản hồi lỗi từ SAP."; });
                                        }
                                    }
                                    catch
                                    {
                                        doc.ItemDetail.Where(p => listI.Contains(p.LineId)).ToList().ForEach(p => { p.Error = "Không thể đọc phản hồi lỗi (body không phải JSON)."; });
                                    }
                                }
                            }
                        }
                    }
                    _context.ODOC.Update(doc);
                    await _context.SaveChangesAsync();
                }
                return true;
            }
            catch
            {
                return true;
            }
        }
        public async Task<bool> SyncVPKMToSapAsync(int DocID)
        {
            try
            {
                string Db = EncryptDecrypt.DecryptString(_configuration["SAPServiceLayer:CompanyDB"]);
                string Username = EncryptDecrypt.DecryptString(_configuration["SAPServiceLayer:Username"]);
                string Password = EncryptDecrypt.DecryptString(_configuration["SAPServiceLayer:Password"]);
                Cookies cookiess = null; ;
                var l = new LoginInfor
                {
                    CompanyDB = Db,
                    UserName = Username,
                    Password = Password
                };
                string url = _configuration["SAPServiceLayer:Host"];
                string data = JsonConvert.SerializeObject(l);
                var doc = _context.ODOC
                .Include(x => x.ItemDetail.Where(p => p.IsSync == false))
                .Include(x => x.Promotion)
                .Include(x => x.BP)
                .ThenInclude(x => x.CRD3)
                .Include(x => x.PaymentInfo)
                .FirstOrDefault(x => x.Status == "DXN" && x.ObjType == 12 && x.IsSync == false && x.Id == DocID);
                if (doc != null)
                {
                    SapOrderBP sl = new SapOrderBP();
                    try
                    {
                        if (cookiess == null || cookiess.SessionTime < DateTime.Now)
                        {

                            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url + "Login");
                            httpWebRequest.ContentType = "application/json";
                            httpWebRequest.Method = "POST";
                            httpWebRequest.KeepAlive = true;
                            httpWebRequest.ServerCertificateValidationCallback += BackEndAPI.Service.Sync.Security.TlsBypass.AcceptSelfSignedInDev();
                            httpWebRequest.Headers.Add("B1S-WCFCompatible", "true");
                            httpWebRequest.Headers.Add("B1S-MetadataWithoutSession", "true");
                            httpWebRequest.Accept = "*/*";
                            httpWebRequest.ServicePoint.Expect100Continue = false;
                            httpWebRequest.Headers.Add("Accept-Encoding", "gzip, deflate, br");
                            httpWebRequest.AutomaticDecompression = DecompressionMethods.GZip;

                            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                            { streamWriter.Write(data); }
                            try
                            {
                                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                                var cookie = httpResponse.Headers.GetValues("Set-Cookie");
                                cookiess = new Cookies();
                                int endIndex = cookie[1].ToString().IndexOf(";");
                                string routeid = cookie[1].ToString().Substring(0, endIndex);
                                endIndex = cookie[0].ToString().IndexOf(";");
                                string sesion = cookie[0].ToString().Substring(0, endIndex + 1);
                                cookiess.ROUTEID = routeid;
                                cookiess.B1SESSION = sesion;
                                cookiess.SessionTime = DateTime.Now.AddMinutes(20);
                            }
                            catch (WebException ex)
                            {
                            }
                        }
                        HttpWebRequest httpWebRequests = (HttpWebRequest)WebRequest.Create(url + "BusinessPartners?$select=CardCode,U_DiachiHD,FederalTaxID,U_Khuvuc&$filter=CardCode eq '" + doc.BP.CardCode + "'");
                        httpWebRequests.ContentType = "application/json";
                        httpWebRequests.Method = "GET";
                        httpWebRequests.KeepAlive = true;
                        httpWebRequests.ServerCertificateValidationCallback += BackEndAPI.Service.Sync.Security.TlsBypass.AcceptSelfSignedInDev();
                        httpWebRequests.Headers.Add("B1S-WCFCompatible", "true");
                        httpWebRequests.Headers.Add("B1S-MetadataWithoutSession", "true");
                        httpWebRequests.Accept = "*/*";
                        httpWebRequests.ServicePoint.Expect100Continue = false;
                        httpWebRequests.Headers.Add("Accept-Encoding", "gzip, deflate, br");
                        httpWebRequests.Headers.Add("Cookie", cookiess.B1SESSION + cookiess.ROUTEID);
                        httpWebRequests.AutomaticDecompression = DecompressionMethods.GZip;
                        try
                        {
                            var httpResponse = (HttpWebResponse)httpWebRequests.GetResponse();
                            if (httpResponse.StatusCode == HttpStatusCode.OK)
                            {
                                using (var reader = new StreamReader(httpResponse.GetResponseStream()))
                                {
                                    var responseText = reader.ReadToEnd();


                                    var dataxxx = JsonConvert.DeserializeObject<BPResponse>(responseText);

                                    if (dataxxx?.Value?.Count > 0)
                                    {
                                        var bp = dataxxx.Value[0];
                                        sl.CardCode = bp.CardCode;
                                        sl.FederalTaxID = bp.FederalTaxID;
                                        sl.U_DiachiHD = bp.U_DiachiHD;
                                        sl.U_Khuvuc = bp.U_Khuvuc;
                                    }

                                }

                            }

                        }
                        catch (WebException ex)
                        {

                        }
                    }
                    catch (Exception ex)
                    {
                    }
                    var order1 = new SapOrder
                    {
                        CardCode = doc.BP.CardCode,
                        DocDate = DateTime.Now.ToString("yyyy-MM-dd"),
                        DocDueDate = DateTime.Now.ToString("yyyy-MM-dd"),
                        DocObjectCode = "17",
                        DocCurrency = doc.Currency,
                        U_NVKD1 = "NBT2",
                        U_NVKD2 = "NBT2",
                        U_NVKD3 = "NBT2",
                        U_NVKD4 = "NBT2",
                        U_MDHPT = doc.InvoiceCode,
                        U_Area = sl.U_Khuvuc,
                        U_CoAdd = sl.U_DiachiHD,
                        U_CoTaxNo = sl.FederalTaxID,
                    };
                    var itemKM = _context.Item.Where(e => doc.ItemDetail.Select(e => e.ItemCode).ToArray().Contains(e.ItemCode)).ToList();
                    var line1 = doc.ItemDetail.Where(x => x.Quantity > 0).Select(x =>
                    new SapOrderLine()
                    {
                        Quantity = x.Quantity,
                        ItemCode = x.ItemCode,
                        LineTotal = 0,
                        UnitPrice = itemKM.FirstOrDefault(e => e.ItemCode == x.ItemCode)?.Price ?? 0,
                        VatGroup = "SVN4",
                        LineId = x.LineId,
                        U_LHH = "01",
                    }).ToList();


                    if (line1.Count > 0)
                    {
                        order1.DocumentLines = line1;

                        try
                        {
                            if (cookiess == null || cookiess.SessionTime < DateTime.Now)
                            {

                                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url + "Login");
                                httpWebRequest.ContentType = "application/json";
                                httpWebRequest.Method = "POST";
                                httpWebRequest.KeepAlive = true;
                                httpWebRequest.ServerCertificateValidationCallback += BackEndAPI.Service.Sync.Security.TlsBypass.AcceptSelfSignedInDev();
                                httpWebRequest.Headers.Add("B1S-WCFCompatible", "true");
                                httpWebRequest.Headers.Add("B1S-MetadataWithoutSession", "true");
                                httpWebRequest.Accept = "*/*";
                                httpWebRequest.ServicePoint.Expect100Continue = false;
                                httpWebRequest.Headers.Add("Accept-Encoding", "gzip, deflate, br");
                                httpWebRequest.AutomaticDecompression = DecompressionMethods.GZip;

                                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                                { streamWriter.Write(data); }
                                try
                                {
                                    var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                                    var cookie = httpResponse.Headers.GetValues("Set-Cookie");
                                    cookiess = new Cookies();
                                    int endIndex = cookie[1].ToString().IndexOf(";");
                                    string routeid = cookie[1].ToString().Substring(0, endIndex);
                                    endIndex = cookie[0].ToString().IndexOf(";");
                                    string sesion = cookie[0].ToString().Substring(0, endIndex + 1);
                                    cookiess.ROUTEID = routeid;
                                    cookiess.B1SESSION = sesion;
                                    cookiess.SessionTime = DateTime.Now.AddMinutes(20);
                                }
                                catch (WebException ex)
                                {
                                }
                            }
                            HttpWebRequest httpWebRequests = (HttpWebRequest)WebRequest.Create(url + "Orders");
                            httpWebRequests.ContentType = "application/json";
                            httpWebRequests.Method = "POST";
                            httpWebRequests.KeepAlive = true;
                            httpWebRequests.ServerCertificateValidationCallback += BackEndAPI.Service.Sync.Security.TlsBypass.AcceptSelfSignedInDev();
                            httpWebRequests.Headers.Add("B1S-WCFCompatible", "true");
                            httpWebRequests.Headers.Add("B1S-MetadataWithoutSession", "true");
                            httpWebRequests.Accept = "*/*";
                            httpWebRequests.ServicePoint.Expect100Continue = false;
                            httpWebRequests.Headers.Add("Accept-Encoding", "gzip, deflate, br");
                            httpWebRequests.Headers.Add("Cookie", cookiess.B1SESSION + cookiess.ROUTEID);
                            httpWebRequests.AutomaticDecompression = DecompressionMethods.GZip;
                            var json = JsonConvert.SerializeObject(order1);
                            //var content = new StringContent(json, Encoding.UTF8, "application/json");
                            using (var streamWriter = new StreamWriter(httpWebRequests.GetRequestStream()))

                            { streamWriter.Write(json); }
                            try
                            {
                                var httpResponse = (HttpWebResponse)httpWebRequests.GetResponse();
                                doc.IsSync = true;
                                var listI = line1.Select(x => x.LineId).ToList();
                                doc.ItemDetail.Where(p => listI.Contains(p.LineId)).ToList().ForEach(p => { p.IsSync = true; });
                            }
                            catch (WebException ex)
                            {
                                var resp = ex.Response as HttpWebResponse;
                                if (resp?.StatusCode == HttpStatusCode.NotFound)
                                {
                                    var draft = resp?.Headers["Location"]?.ToString();
                                    if (!draft.IsNullOrEmpty())
                                    {
                                        var match = Regex.Match(draft ?? "", @"\((\d+)\)");
                                        var draftId = match.Success ? int.Parse(match.Groups[1].Value) : 0;
                                        doc.IsSync = true;
                                        var listI = line1.Select(x => x.LineId).ToList();
                                        doc.ItemDetail.Where(p => listI.Contains(p.LineId)).ToList().ForEach(p => { p.IsSync = true; p.DraftId = draftId.ToString(); });
                                    }
                                    else
                                    {
                                        var listI = line1.Select(x => x.LineId).ToList();
                                        string responseContent = string.Empty;
                                        using (var stream = resp.GetResponseStream())
                                        using (var reader = new StreamReader(stream))
                                        {
                                            responseContent = reader.ReadToEnd();
                                        }
                                        try
                                        {
                                            var jsonDoc = JsonDocument.Parse(responseContent);
                                            var root = jsonDoc.RootElement;
                                            doc.IsSync = false;
                                            if (root.TryGetProperty("error", out var error))
                                            {
                                                var code = error.GetProperty("code").GetInt32();
                                                var message = error.GetProperty("message").GetProperty("value").GetString();
                                                doc.ItemDetail.Where(p => listI.Contains(p.LineId)).ToList().ForEach(p => { p.Error = message; });
                                            }
                                            else
                                            {
                                                doc.ItemDetail.Where(p => listI.Contains(p.LineId)).ToList().ForEach(p => { p.Error = "Không thể phân tích phản hồi lỗi từ SAP."; });
                                            }
                                        }
                                        catch
                                        {
                                            doc.ItemDetail.Where(p => listI.Contains(p.LineId)).ToList().ForEach(p => { p.Error = "Không thể đọc phản hồi lỗi (body không phải JSON)."; });
                                        }
                                    }
                                }

                            }


                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }

                    _context.ODOC.Update(doc);
                    await _context.SaveChangesAsync();
                }
                return true;
            }
            catch
            {
                return true;
            }
        }

        public async Task<bool> SyncIssueToSapAsync(int DocID)
        {
            try
            {
                string Db = EncryptDecrypt.DecryptString(_configuration["SAPServiceLayer:CompanyDB"]);
                string Username = EncryptDecrypt.DecryptString(_configuration["SAPServiceLayer:Username"]);
                string Password = EncryptDecrypt.DecryptString(_configuration["SAPServiceLayer:Password"]);
                Cookies cookiess = null; ;
                var l = new LoginInfor
                {
                    CompanyDB = Db,
                    UserName = Username,
                    Password = Password
                };
                string url = _configuration["SAPServiceLayer:Host"];
                string data = JsonConvert.SerializeObject(l);
                var doc = _context.ODOC
                .Include(x => x.ItemDetail)
                .Include(x => x.BP)
                .AsNoTracking().AsSplitQuery()
                .FirstOrDefault(x => x.Status == "DXN" && x.IsSync == false && x.ObjType == 1250000001 && x.Id == DocID);
                if (doc != null)
                {
                    if (cookiess == null || cookiess.SessionTime < DateTime.Now)
                    {

                        HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url + "Login");
                        httpWebRequest.ContentType = "application/json";
                        httpWebRequest.Method = "POST";
                        httpWebRequest.KeepAlive = true;
                        httpWebRequest.ServerCertificateValidationCallback += BackEndAPI.Service.Sync.Security.TlsBypass.AcceptSelfSignedInDev();
                        httpWebRequest.Headers.Add("B1S-WCFCompatible", "true");
                        httpWebRequest.Headers.Add("B1S-MetadataWithoutSession", "true");
                        httpWebRequest.Accept = "*/*";
                        httpWebRequest.ServicePoint.Expect100Continue = false;
                        httpWebRequest.Headers.Add("Accept-Encoding", "gzip, deflate, br");
                        httpWebRequest.AutomaticDecompression = DecompressionMethods.GZip;

                        using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                        { streamWriter.Write(data); }
                        try
                        {
                            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                            var cookie = httpResponse.Headers.GetValues("Set-Cookie");
                            cookiess = new Cookies();
                            int endIndex = cookie[1].ToString().IndexOf(";");
                            string routeid = cookie[1].ToString().Substring(0, endIndex);
                            endIndex = cookie[0].ToString().IndexOf(";");
                            string sesion = cookie[0].ToString().Substring(0, endIndex + 1);
                            cookiess.ROUTEID = routeid;
                            cookiess.B1SESSION = sesion;
                            cookiess.SessionTime = DateTime.Now.AddMinutes(20);
                        }
                        catch (WebException ex)
                        {
                        }
                    }
                    HttpWebRequest httpWebRequestsdraft = (HttpWebRequest)WebRequest.Create(url + "Drafts?$filter=U_MDHPT eq '" + doc.InvoiceCode + "'&$select = DocEntry,U_MDHPT");
                    httpWebRequestsdraft.ContentType = "application/json";
                    httpWebRequestsdraft.Method = "GET";
                    httpWebRequestsdraft.KeepAlive = true;
                    httpWebRequestsdraft.ServerCertificateValidationCallback += BackEndAPI.Service.Sync.Security.TlsBypass.AcceptSelfSignedInDev();
                    httpWebRequestsdraft.Headers.Add("B1S-WCFCompatible", "true");
                    httpWebRequestsdraft.Headers.Add("B1S-MetadataWithoutSession", "true");
                    httpWebRequestsdraft.Accept = "*/*";
                    httpWebRequestsdraft.ServicePoint.Expect100Continue = false;
                    httpWebRequestsdraft.Headers.Add("Accept-Encoding", "gzip, deflate, br");
                    httpWebRequestsdraft.Headers.Add("Cookie", cookiess.B1SESSION + cookiess.ROUTEID);
                    httpWebRequestsdraft.AutomaticDecompression = DecompressionMethods.GZip;
                    try
                    {
                        var httpResponse = (HttpWebResponse)httpWebRequestsdraft.GetResponse();
                        if (httpResponse.StatusCode == HttpStatusCode.OK)
                        {
                            using (var reader = new StreamReader(httpResponse.GetResponseStream()))
                            {
                                var responseText = reader.ReadToEnd();


                                var dataxxx = JsonConvert.DeserializeObject<DraftResponse>(responseText);

                                if (dataxxx?.Value?.Count > 0)
                                {
                                    var bp = dataxxx.Value[0];
                                    if (doc.InvoiceCode == bp.U_MDHPT)
                                    {
                                        doc.IsSync = true;
                                        doc.ItemDetail.ForEach(item =>
                                        {
                                            item.IsSync = true;
                                            item.DraftId = bp.DocEntry.ToString();
                                        });
                                        _context.ODOC.Update(doc);
                                        await _context.SaveChangesAsync();
                                        return true;
                                    }
                                }

                            }

                        }

                    }
                    catch (WebException exx)
                    {

                    }
                    var line1 = doc.ItemDetail.Where(e => e.Quantity > 0).Select(x =>
                    new IssueLine()
                    {
                        Quantity = x.Quantity,
                        ItemCode = x.ItemCode,
                    }).ToList();

                    var issue = new Issue
                    {
                        U_BPCode = doc.BP.SapCardCode,
                        U_NPPH = doc.BP.CardName,
                        U_MDHPT = doc.InvoiceCode,
                        DocDate = DateTime.Now.ToString("yyyy-MM-dd"),
                    };
                    if (line1.Count > 0)
                    {
                        issue.DocumentLines = line1;
                        try
                        {
                            if (cookiess == null || cookiess.SessionTime < DateTime.Now)
                            {

                                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url + "Login");
                                httpWebRequest.ContentType = "application/json";
                                httpWebRequest.Method = "POST";
                                httpWebRequest.KeepAlive = true;
                                httpWebRequest.ServerCertificateValidationCallback += BackEndAPI.Service.Sync.Security.TlsBypass.AcceptSelfSignedInDev();
                                httpWebRequest.Headers.Add("B1S-WCFCompatible", "true");
                                httpWebRequest.Headers.Add("B1S-MetadataWithoutSession", "true");
                                httpWebRequest.Accept = "*/*";
                                httpWebRequest.ServicePoint.Expect100Continue = false;
                                httpWebRequest.Headers.Add("Accept-Encoding", "gzip, deflate, br");
                                httpWebRequest.AutomaticDecompression = DecompressionMethods.GZip;

                                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                                { streamWriter.Write(data); }
                                try
                                {
                                    var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                                    var cookie = httpResponse.Headers.GetValues("Set-Cookie");
                                    cookiess = new Cookies();
                                    int endIndex = cookie[1].ToString().IndexOf(";");
                                    string routeid = cookie[1].ToString().Substring(0, endIndex);
                                    endIndex = cookie[0].ToString().IndexOf(";");
                                    string sesion = cookie[0].ToString().Substring(0, endIndex + 1);
                                    cookiess.ROUTEID = routeid;
                                    cookiess.B1SESSION = sesion;
                                    cookiess.SessionTime = DateTime.Now.AddMinutes(20);
                                }
                                catch (WebException ex)
                                {
                                    return true;
                                }
                            }
                            HttpWebRequest httpWebRequests = (HttpWebRequest)WebRequest.Create(url + "Drafts");
                            httpWebRequests.ContentType = "application/json";
                            httpWebRequests.Method = "POST";
                            httpWebRequests.KeepAlive = true;
                            httpWebRequests.ServerCertificateValidationCallback += BackEndAPI.Service.Sync.Security.TlsBypass.AcceptSelfSignedInDev();
                            httpWebRequests.Headers.Add("B1S-WCFCompatible", "true");
                            httpWebRequests.Headers.Add("B1S-MetadataWithoutSession", "true");
                            httpWebRequests.Accept = "*/*";
                            httpWebRequests.ServicePoint.Expect100Continue = false;
                            httpWebRequests.Headers.Add("Accept-Encoding", "gzip, deflate, br");
                            httpWebRequests.Headers.Add("Cookie", cookiess.B1SESSION + cookiess.ROUTEID);
                            httpWebRequests.AutomaticDecompression = DecompressionMethods.GZip;
                            var json = JsonConvert.SerializeObject(issue);
                            //var content = new StringContent(json, Encoding.UTF8, "application/json");
                            using (var streamWriter = new StreamWriter(httpWebRequests.GetRequestStream()))

                            { streamWriter.Write(json); }
                            try
                            {
                                var httpResponse = (HttpWebResponse)httpWebRequests.GetResponse();
                                doc.IsSync = true;
                            }
                            catch (WebException ex)
                            {
                                return true;
                            }


                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                            return true;
                        }
                    }
                    _context.ODOC.Update(doc);
                    await _context.SaveChangesAsync();
                }
                return true;
            }
            catch
            {
                return true;
            }
        }
        public async Task<Mess> SyncInvoiceError(int DocID, int ObjType)
        {
            Mess mes = new Mess();
            try
            {
                bool check = false;
                if (ObjType == 22)
                    check = await SyncToSapAsync(DocID);
                else if(ObjType == 12)
                    check = await SyncVPKMToSapAsync(DocID);
                else if (ObjType == 1250000001)
                    check = await SyncIssueToSapAsync(DocID);
                if (check == true)
                {
                    return null;
                }
                else
                {
                    mes.Errors = "Đồng bộ thất bại";
                    mes.Status = 900;
                    return (mes);
                }  
            }
            catch (Exception ex)
            {
                mes.Errors = ex.Message;
                mes.Status = 900;
                return (mes);
            }
        }

        public async Task<(IEnumerable<OrderReturnDto>, Mess, int)> GetOrderReturnAsync(int? userId, GridifyQuery q)
        {
            Mess mess = new Mess();
            try
            {
                var query = _context.ODOC.AsQueryable();
                int cardId = 0;
                if (userId is not null)
                {
                    var usr = await _context.AppUser.AsSplitQuery().AsQueryable().AsNoTracking()
                        .Include(xx => xx.DirectStaff)
                        .Where(xx => xx.Id == userId)
                        .Include(xx => xx.Role)
                        .ThenInclude(xx => xx.RoleFillCustomerGroups)
                        .FirstOrDefaultAsync();
                    cardId = usr?.CardId ?? 0;
                    if (usr != null && usr.Role != null)
                    {
                        if (usr.Role.IsFillCustomerGroup)
                        {
                            query = query.Where(e => e.BP.Groups.Any(c =>
                                usr.Role.RoleFillCustomerGroups.Select(d => d.CustomerGroupId).ToList().Contains(c.Id)));
                        }
                        if (usr.Role.IsSaleRole)
                        {
                            var usrIds1 = await GetAllCustomerIdWithUserId(userId ?? 0);
                            query = query.Where(e => usrIds1.Contains(e.BP.SaleId.Value));
                        }
                    }
                }
                if (cardId == 0)
                {
                    var totalCount = query.Where(p => p.ObjType == 22 && (p.DocType ?? "") != "DVP" && p.Status == "DHT" && p.ItemDetail.Any(e => e.OpenQty > 0)).ApplyFiltering(q).Count();
                    var items = query.AsSplitQuery().Where(p => p.ObjType == 22 && (p.DocType ?? "") != "DVP" && p.Status == "DHT" && p.ItemDetail.Any(e => e.OpenQty > 0)).OrderBy(p => p.DocDate)
                        .Select(o => new OrderReturnDto
                        {
                            Id = o.Id,
                            DocType = o.DocType,
                            InvoiceCode = o.InvoiceCode,
                            CardId = o.CardId,
                            Currency = o.Currency,
                            SapDocEntry = o.SapDocEntry,
                            CardCode = o.CardCode,
                            CardName = o.CardName,
                            DocDate = o.DocDate,
                            // Map danh sách chi tiết từ bảng DOC1
                            ItemDetails = o.ItemDetail.Where(e => e.OpenQty > 0).Select(l => new DOC1OrderReturnDto
                            {
                                Id = l.Id,
                                Type = l.Type,
                                ItemId = l.ItemId,
                                ItemCode = l.ItemCode,
                                ItemName = l.ItemName,
                                Quantity = l.OpenQty ?? 0,
                                Price = l.PriceAfterDist,
                                OuomId = l.OuomId,
                                UomCode = l.UomCode,
                                UomName = l.UomName
                            }).ToList()
                        }).OrderByDescending(p => p.Id)
                        .ApplyFiltering(q)
                        .ApplyPaging(q)
                        .ToList();
                    return (items, null, totalCount);
                }
                else
                {
                    var totalCount = query.Where(p => p.ObjType == 22 && (p.DocType ?? "") != "DVP" && p.CardId == cardId && p.Status == "DHT" && p.ItemDetail.Any(e=>e.OpenQty > 0)).ApplyFiltering(q).Count();
                    var items = query.AsSplitQuery().Where(p => p.ObjType == 22 && (p.DocType ?? "") != "DVP" && p.CardId == cardId && p.Status == "DHT" && p.ItemDetail.Any(e => e.OpenQty > 0)).OrderBy(p => p.DocDate)
                    .Select(o => new OrderReturnDto
                    {
                        Id = o.Id,
                        DocType = o.DocType,
                        InvoiceCode = o.InvoiceCode,
                        CardId = o.CardId,
                        Currency = o.Currency,
                        SapDocEntry = o.SapDocEntry,
                        CardCode = o.CardCode,
                        CardName = o.CardName,
                        DocDate = o.DocDate,
                        ItemDetails = o.ItemDetail.Where(e => e.OpenQty > 0).Select(l => new DOC1OrderReturnDto
                        {
                            Id = l.Id,
                            Type = l.Type,
                            ItemId = l.ItemId,
                            ItemCode = l.ItemCode,
                            ItemName = l.ItemName,
                            Quantity = l.OpenQty ?? 0,
                            Price = l.PriceAfterDist,
                            OuomId = l.OuomId,
                            UomCode = l.UomCode,
                            UomName = l.UomName
                        }).ToList()
                    }).OrderByDescending(p => p.Id)
                        .ApplyFiltering(q)
                        .ApplyPaging(q)
                        .ToList();
                    return (items, null, totalCount);
                }

            }
            catch (Exception ex)
            {
                mess.Status = 900;
                mess.Errors = ex.Message;
                return (null, mess, 0);
            }
        }

        public Task<(ODOC, Mess)> UpdateDocumentReturnAsync(int id, OrderReturn model, int ObjType)
        {
            throw new NotImplementedException();
        }
    }
}