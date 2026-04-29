using BackEndAPI.Data;
using BackEndAPI.Models.Fee;
using BackEndAPI.Models.ItemMasterData;
using BackEndAPI.Models.Other;
using BackEndAPI.Models.Promotion;
using BackEndAPI.Service.GoldPriceLists;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using BackEndAPI.Service.NotificationHub;
using Org.BouncyCastle.Asn1.Cmp;
using Org.BouncyCastle.Asn1.Crmf;
using System.Collections.Generic;
using System.Net.WebSockets;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;
using System.Linq;
using BackEndAPI.Models.Account;
using BackEndAPI.Service.EventAggregator;
using Microsoft.AspNetCore.Identity;
using Gridify;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;

namespace BackEndAPI.Service.Fees
{
    public class FeeService : Service<Fee>, IFeeService
    {
        
        private readonly AppDbContext _context;
        private readonly IHostingEnvironment _webHostEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IModelUpdater _modelUpdater;

        public FeeService(AppDbContext context,IHostingEnvironment webHostEnvironment, IModelUpdater modelUpdater,IHttpContextAccessor httpContextAccessor) : base(context)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _httpContextAccessor = httpContextAccessor;
            _modelUpdater = modelUpdater;
        }

        public Task<(bool, Mess)> ActiveFeeAsync(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<(Fee, Mess)> CreateFeeAsync(Fee model)
        {
            Mess mess = new Mess();
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    await _context.Fee.AddAsync(model);
                    await _context.SaveChangesAsync();
                    transaction.Commit();
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

        public async Task<(IEnumerable<Fee>, Mess, int)> GetAllFeeLAsync(string? search, GridifyQuery q)
        {
            Mess mess = new Mess();
            try
            {
                var query = _context.Set<Fee>().AsQueryable();
                if (!string.IsNullOrEmpty(search))
                {
                    search = search.Trim().ToLower();
                    query = query.Where(p => p.Name.ToLower().Contains(search.ToLower()) || p.Description.ToLower().Contains(search.ToLower()));
                }
                var totalCount = await query.ApplyFiltering(q).CountAsync();
                var items = await query
                    .ApplyFiltering(q)
                    .ApplyPaging(q)
                    .Include(e => e.FeeLine)
                    .ThenInclude(e => e.FeeLevel)
                    .Include(e => e.FeeLine)
                    .ThenInclude(e => e.OUGP)
                    .Include(e => e.appUser)
                    .ToListAsync();
                return (items, null, totalCount);
            }
            catch (Exception ex)

            {
                mess.Status = 900;
                mess.Errors = ex.Message;
                return (null, mess, 0);
            }
        }

        #region GetFeeByCustomerAysnc

        
        //public async Task<(IEnumerable<FeebyCustomer>, Mess)> GetFeebyCustomerAsync(int year, int period)
        //{
        //    Mess mess = new Mess();
        //    try
        //    {
        //        var feePeriod = await _context.FeePeriod
        //            .Include(e => e.FeePeriodLine)
        //            .FirstOrDefaultAsync(e => e.Year == year && e.Period == period);


        //        var fee =await _context.Fee
        //                    .Include(e=>e.FeeLine)
        //                    .ThenInclude (e=>e.FeeLevel)
        //                    .Include(e => e.FeeLine)
        //                    .ThenInclude(e => e.OUGP)
        //                    .FirstOrDefaultAsync();
        //        List<FeebyCustomer> list = new List<FeebyCustomer>();

        //        var feeCus = feePeriod.FeePeriodLine.Select(e => new { e.CardCode, e.CardName }).Distinct().ToList();
        //        foreach(var fc in feeCus)
        //        {
        //            FeebyCustomer feebyCustomer = new FeebyCustomer();
        //            feebyCustomer.CardCode = fc.CardCode;
        //            feebyCustomer.CardName = fc.CardName;
        //            var feeLine = feePeriod.FeePeriodLine.Where(e => e.CardCode == fc.CardCode).ToList();
        //            List<FeebyCustomerLine> listLine = new List<FeebyCustomerLine>();
        //            foreach (var fl in feeLine)
        //            {
        //                bool check = false;
        //                double Day = fl.Day;
        //                int count = 0;
        //                //foreach (var f in fee.FeeLine.Where(e=>e.OUGP.UgpName == fl.UgpName).OrderBy(e=>e.FeeLevel.FromDays))
        //                var f = fee.FeeLine.Where(e => e.OUGP.UgpName == fl.UgpName).OrderBy(e => e.FeeLevel.FromDays).ToList();
        //                int flag = f.Count();
        //                for (int i =0;i < f.Count();i++)
        //                {
        //                    FeebyCustomerLine line = new FeebyCustomerLine();
        //                    line.ItemCode = fl.ItemCode;
        //                    line.ItemName = fl.ItemName;
        //                    line.UgpName = fl.UgpName;
        //                    line.UgpCode = fl.UgpCode;
        //                    line.BatchNum = fl.BatchNum;
        //                    line.Day = fl.Day;
        //                    line.Quantity = fl.Quantity;
        //                    line.ReceiptDate = fl.ReceiptDate;
        //                    line.IssueDate = fl.IssueDate;
        //                    double FromDay = f[i].FeeLevel.FromDays;
        //                    double ToDay = 0;
        //                    try
        //                    {
        //                        if ((int)f[i].FeeLevel.ToDays == 0)
        //                            ToDay = f[i + 1].FeeLevel.FromDays - 1;
        //                        else
        //                            ToDay = (double)f[i].FeeLevel.ToDays;
        //                    }
        //                    catch
        //                    {

        //                    }
        //                    if(count == 0)
        //                    {
        //                        if (Day <= FromDay)
        //                            break;
        //                        else
        //                        {
        //                            if (ToDay >= Day)
        //                            {
        //                                line.DayToFee = (int)Day - (int)FromDay;
        //                                Day = 0;
        //                            }    

        //                            else
        //                            {
        //                                line.DayToFee = (int)ToDay;
        //                                Day = Day - ToDay;
        //                            }    
        //                            count++;
        //                            line.LineTotal = line.DayToFee * (double)f[i].FeePrice + (line.DayToFee * (double)f[i].FeePrice * (double)f[i].FeeWAT / 100.00);
        //                            line.LineVAT = line.DayToFee * (double)f[i].FeePrice * (double)f[i].FeeWAT / 100.00;
        //                            line.Price = (double)f[i].FeePrice;
        //                            line.FeeLevelId = f[i].FeeLevel.Id;
        //                            line.FeeLevelName = f[i].FeeLevel.Name;
        //                            listLine.Add(line);
        //                        }    
        //                    }
        //                    else
        //                    {
        //                        count++;
        //                        if (Day == 0)
        //                            break;   
        //                        if (Day <= FromDay)
        //                        {

        //                            line.DayToFee = (int)Day;
        //                            line.LineTotal = line.DayToFee * (double)f[i].FeePrice + (line.DayToFee * (double)f[i].FeePrice * (double)f[i].FeeWAT / 100.00);
        //                            line.LineVAT = line.DayToFee * (double)f[i].FeePrice * (double)f[i].FeeWAT / 100.00;
        //                            line.Price = (double)f[i].FeePrice;
        //                            line.FeeLevelId = f[i].FeeLevel.Id;
        //                            line.FeeLevelName = f[i].FeeLevel.Name;
        //                            listLine.Add(line);
        //                            Day = Day - FromDay;
        //                            break;
        //                        }  
        //                        else
        //                        {
        //                            if (ToDay >= Day)
        //                            {
        //                                line.DayToFee = (int)Day;
        //                                Day = 0;
        //                            }

        //                            else
        //                            {
        //                                line.DayToFee = (int)ToDay;
        //                                Day = Day - ToDay;
        //                            }
        //                            count++;
        //                            line.LineTotal = line.DayToFee * (double)f[i].FeePrice + (line.DayToFee * (double)f[i].FeePrice * (double)f[i].FeeWAT / 100.00);
        //                            line.LineVAT = line.DayToFee * (double)f[i].FeePrice * (double)f[i].FeeWAT / 100.00;
        //                            line.Price = (double)f[i].FeePrice;
        //                            line.FeeLevelId = f[i].FeeLevel.Id;
        //                            line.FeeLevelName = f[i].FeeLevel.Name;
        //                            listLine.Add(line);
        //                        }
        //                    }

        //                }
        //            }
        //            feebyCustomer.FeebyCustomerLine = listLine;
        //            feebyCustomer.Total = listLine.Sum(e => e.LineTotal);
        //            feebyCustomer.Vat = listLine.Sum(e => e.LineVAT);
        //            list.Add(feebyCustomer);
        //        }


        //        return (list.Where(e=>e.Total > 0), null);
        //    }
        //    catch (Exception ex)

        //    {
        //        mess.Status = 900;
        //        mess.Errors = ex.Message;
        //        return (null, mess);
        //    }
        //}

        #endregion

        public async Task<(Fee, Mess)> GetFeeByIdAsync(int id)
        {
            Mess mess = new Mess();
            try
            {
                var item = await _context.Fee
                    .AsNoTracking()
                    .Include(e => e.appUser)
                    .Include(e => e.FeeLine)
                    .ThenInclude(e => e.FeeLevel)
                    .Include(e => e.FeeLine)
                    .ThenInclude(e => e.OUGP)
                    .FirstOrDefaultAsync(p => p.Id == id);
                return (item, null);
            }
            catch (Exception ex)
            {
                mess.Status = 900;
                mess.Errors = ex.Message;
                return (null, mess);
            }
        }

        public async Task<(IEnumerable<FeeLine>, Mess)> GetFeeLineAsync()
        {
            Mess mess = new Mess();
            try
            {
                List<FeeLine> items = new List<FeeLine>();
                var ougp = await _context.OUGP.ToListAsync();
                var feeLevel = await _context.FeeLevel.ToListAsync();
                if (ougp.Count > 0)
                {
                    foreach (var line in ougp)
                    {
                        foreach (var feeL in feeLevel)
                        {
                            FeeLine feeLine = new FeeLine();
                            feeLine.UgpId = line.Id;
                            feeLine.OUGP = line;
                            feeLine.FeeLevelId = feeL.Id;
                            feeLine.FeeLevel = feeL;
                            feeLine.FeeWAT = 0;
                            feeLine.FeePrice = 0;
                            items.Add(feeLine);
                        }
                    }
                }

                return (items, null);
            }
            catch (Exception ex)
            {
                mess.Status = 900;
                mess.Errors = ex.Message;
                return (null, mess);
            }
        }

        public async Task<(IEnumerable<FeePeriod>, Mess)> GetFeePeriodAsync(int year, int period)
        {
            Mess mess = new Mess();
            try
            {
                var items = await _context.FeePeriod
                    .Include(e => e.FeePeriodLine)
                    .ToListAsync();
                return (items, null);
            }
            catch (Exception ex)

            {
                mess.Status = 900;
                mess.Errors = ex.Message;
                return (null, mess);
            }
        }

        public async Task<(Fee, Mess)> UpdateFeeAsync(int id, Fee model)
        {
            Mess mess = new Mess();
            using (var transaction = _context.Database.BeginTransaction())
            {
                model.appUser = null;
                try
                {
                    var fee = await _context.Fee.Include(e => e.FeeLine).FirstOrDefaultAsync(p => p.Id == id);
                    if (fee == null)
                    {
                        mess.Status = 400;
                        mess.Errors = "Không tồn tại bản ghi để cập nhập";
                        transaction.Rollback();
                        return (null, mess);
                    }

                    _modelUpdater.UpdateModel(fee, model, "FeeLine", "CRD2", "CRD3", "CRD4", "CRD5", "NA5");
                    if (model.FeeLine != null)
                    {
                        foreach (var fe in model.FeeLine.ToList())
                        {
                            var fes = fee.FeeLine
                                .FirstOrDefault(c => c.Id == fe.Id && c.Id != 0);

                            if (fes != null)
                            {
                                if (string.IsNullOrEmpty(fe.Status))
                                {
                                }
                                else if (fe.Status.Equals("D"))
                                {
                                    _context.FeeLine.Remove(fes);
                                    fee.FeeLine.Remove(fes);
                                }
                                else if (fe.Status.Equals("U"))
                                {
                                    var dtoFeeLine = fe.GetType();
                                    var entityFeeLine = fes.GetType();

                                    foreach (var prop in dtoFeeLine.GetProperties())
                                    {
                                        var dtoValue = prop.GetValue(fe);
                                        if (dtoValue != null)
                                        {
                                            var entityProp = entityFeeLine.GetProperty(prop.Name);
                                            if (entityProp != null)
                                            {
                                                entityProp.SetValue(fes, dtoValue);
                                            }
                                        }
                                    }
                                }
                            }
                            else
                                fee.FeeLine.Add(fe);
                        }
                    }

                    _context.Fee.Update(fee);
                    await _context.SaveChangesAsync();
                    transaction.Commit();
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
    }

    public class FeebyCustomerService : Service<FeebyCustomer>, IFeebyCustomerService
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<NotificationHubs> _hubContext;
        private readonly IHostingEnvironment _webHostEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IEventAggregator _eventAggregator;
        Endpoints _endpoints;
        private readonly IConfiguration _configuration;
        public FeebyCustomerService(AppDbContext context, IEventAggregator eventAggregator,
            IHubContext<NotificationHubs> hubContext, IHostingEnvironment webHostEnvironment,
            IHttpContextAccessor httpContextAccessor, IOptions<Endpoints> options, IConfiguration configuration) : base(context)
        {
            _hubContext = hubContext;
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _httpContextAccessor = httpContextAccessor;
            _eventAggregator = eventAggregator;
            _endpoints = options.Value;
            _configuration = configuration;
        }

        public async Task<(IEnumerable<FeebyCustomer>, Mess, int)> GetAllFeeByCustomerAsync(int year, int period, string? search, GridifyQuery gq, int skip = 0, int limit = 30)
        {
            Mess mess = new Mess();
            try
            {
                _ = await GetFeebyCustomerAsync(year, period);
                var config = new GridifyMapper<FeebyCustomer>()
                                .GenerateMappings()
                                .AddMap("totalWithVat", p => p.Total + p.Vat);
                // Tạo câu querry
                // Luôn tìm kiếm theo Year và Period
                var query = _context.Set<FeebyCustomer>().AsQueryable()
                            .Where(e=>e.Year == year && e.Period == period);

                // Thêm điều kiện tìm kiếm theo mã chứng từ, tên chứng từ, mã khách hàng, tên khách hàng
                if (!string.IsNullOrEmpty(search))
                {
                    query = query
                        .Where(e => e.CardCode.Contains(search) || 
                                    e.CardName.Contains(search) || 
                                    e.FeebyCustomerCode.Contains(search) || 
                                    e.FeebyCustomerName.Contains(search));
                }
                // Số lượng bản ghi trả về
                var totalCount = await query.ApplyFiltering(gq, config).CountAsync();
                // Nội dung bản ghi trả về luôn bao gồm phân trang
                var items = await query
                                .ApplyFiltering(gq, config)
                                .Skip(skip * limit)
                                .Take(limit)
                                .ToListAsync();

                return (items, null, totalCount);
            }
            catch (Exception ex)
            {
                mess.Status = 900;
                mess.Errors = ex.Message;
                return (null, mess, 0);
            }
        }

        public async Task<(IEnumerable<FeebyCustomer>, Mess, int)> GetAllFeeByCustomerAsync(string userId, int year,
            int period, int skip = 0, int limit = 30)
        {
            Mess mess = new Mess();
            try
            {
                var user = _context.AppUser.FirstOrDefault(p => p.Id == int.Parse(userId));
                var customer = _context.BP.FirstOrDefault(id => id.Id == (int)user.CardId);
                var query = _context.Set<FeebyCustomer>().AsQueryable().Where(e =>
                    e.Year == year && e.Period == period && e.CardCode == customer.CardCode);
                var querys = _context.Set<FeebyCustomer>().AsQueryable().Where(e =>
                    e.Year == year && e.Period == period && e.CardCode == customer.CardCode);
                var totalCount = await query.CountAsync();
                var items = await querys.Skip(skip * limit)
                    .Take(limit)
                    .ToListAsync();
                return (items, null, totalCount);
            }
            catch (Exception ex)

            {
                mess.Status = 900;
                mess.Errors = ex.Message;
                return (null, mess, 0);
            }
        }

        public async Task<(IEnumerable<FeebyCustomer>, Mess, int)> GetAllFeeByCustomerAsync(string userId, int skip = 0,
            int limit = 30)
        {
            Mess mess = new Mess();
            try
            {
                var user = _context.AppUser.FirstOrDefault(p => p.Id == int.Parse(userId));
                var customer = _context.BP.FirstOrDefault(id => id.Id == (int)user.CardId);
                var query = _context.Set<FeebyCustomer>().AsQueryable().Where(e => e.CardCode == customer.CardCode);
                var querys = _context.Set<FeebyCustomer>().AsQueryable().Where(e => e.CardCode == customer.CardCode);
                var totalCount = await query.CountAsync();
                var items = await querys.OrderByDescending(e => e.Year).ThenByDescending(p => p.Period)
                    .Skip(skip * limit)
                    .Take(limit)
                    .ToListAsync();
                return (items, null, totalCount);
            }
            catch (Exception ex)

            {
                mess.Status = 900;
                mess.Errors = ex.Message;
                return (null, mess, 0);
            }
        }

        public async Task<(IEnumerable<FeebyCustomer>, Mess, int)> GetAllFeeByCustomerAsync(int skip = 0,
            int limit = 30)
        {
            Mess mess = new Mess();
            try
            {
                var query = _context.Set<FeebyCustomer>().AsQueryable();
                var querys = _context.Set<FeebyCustomer>().AsQueryable();
                var totalCount = await query.CountAsync();
                var items = await querys.OrderByDescending(e => e.Year).ThenByDescending(p => p.Period)
                    .Skip(skip * limit)
                    .Take(limit)
                    .ToListAsync();
                return (items, null, totalCount);
            }
            catch (Exception ex)
            {
                mess.Status = 900;
                mess.Errors = ex.Message;
                return (null, mess, 0);
            }
        }

        public async Task<Mess> GetFeebyCustomerAsync(int year, int period)
        {
            Mess mess = new Mess();
            try
            {
                var feeByCus =
                    await _context.FeebyCustomer.FirstOrDefaultAsync(e => e.Year == year && e.Period == period);
                if (feeByCus != null)
                {
                    return null;
                }
                else
                {
                    try
                    {
                        var feePeriodsss =
                            await _context.FeePeriod.FirstOrDefaultAsync(e => e.Year == year && e.Period == period);
                        if (feePeriodsss == null)
                        {
                            var client = new HttpClient();
                            var request = new HttpRequestMessage(HttpMethod.Get,
                                _endpoints.Host + "/Fee?year=" + year + "&period=" + period);
                            request.Headers.Add("accept", "*/*");
                            var response = await client.SendAsync(request);
                            if (response.IsSuccessStatusCode)
                            {
                                // Read the response content as a JSON string
                                var jsonString = await response.Content.ReadAsStringAsync();
                                jsonString = jsonString.Replace("\"FeePeriod\":", "");
                                int jsonLen = jsonString.Length;

                                string jsonStringF = jsonString.Substring(2);

                                jsonLen = jsonStringF.Length - 2;
                                string jsonStringL = jsonStringF.Substring(0, jsonLen);
                                if(jsonStringL.Length > 0)
                                {
                                    var feePeriods = JsonConvert.DeserializeObject<FeePeriod>(jsonStringL);
                                    _context.FeePeriod.Add(feePeriods);
                                    await _context.SaveChangesAsync();
                                } 
                            }
                            else
                            {
                                mess.Status = (int)response.StatusCode;
                                mess.Errors = "Lỗi đồng bộ";
                                return mess;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        mess.Status = 900;
                        mess.Errors = ex.Message;
                        return mess;
                    }

                    var feePeriod = await _context.FeePeriod
                        .Include(e => e.FeePeriodLine)
                        .FirstOrDefaultAsync(e => e.Year == year && e.Period == period);


                    var fee = await _context.Fee
                        .Include(e => e.FeeLine)
                        .ThenInclude(e => e.FeeLevel)
                        .Include(e => e.FeeLine)
                        .ThenInclude(e => e.OUGP)
                        .FirstOrDefaultAsync(p=>p.Status == true && p.FromDate.Year <= year && p.FromDate.Month <= period && p.ToDate.Year >= year && p.ToDate.Month >= period);
                    List<FeebyCustomer> list = new List<FeebyCustomer>();

                    var feeCus = feePeriod.FeePeriodLine.Select(e => new { e.CardCode, e.CardName }).Distinct()
                        .ToList();
                    foreach (var fc in feeCus)
                    {
                        FeebyCustomer feebyCustomer = new FeebyCustomer();
                        feebyCustomer.CardCode = fc.CardCode;
                        feebyCustomer.CardName = fc.CardName;
                        feebyCustomer.Year = year;
                        feebyCustomer.Period = period;
                        var codes = await GenerateByCode("BBTP" + year + "0" + period, "", 15, feebyCustomer);
                        feebyCustomer.FeebyCustomerCode = codes;
                        feebyCustomer.FeebyCustomerName = "Biên bản tính phí lưu kho " + feebyCustomer.CardName +
                                                          " Quý " + period + "- " + year;
                        var feeLine = feePeriod.FeePeriodLine.Where(e => e.CardCode == fc.CardCode).ToList();
                        List<FeebyCustomerLine> listLine = new List<FeebyCustomerLine>();
                        foreach (var fl in feeLine)
                        {
                            bool check = false;
                            double Day = fl.Day;
                            int count = 0;
                            var f = fee.FeeLine.Where(e => e.OUGP.UgpName == fl.UgpName && e.FeePrice > 0)
                                .OrderByDescending(e => e.FeeLevel.FromDays).ToList();
                            int flag = f.Count();
                            for (int i = 0; i < f.Count(); i++)
                            {
                                FeebyCustomerLine line = new FeebyCustomerLine();
                                line.ItemCode = fl.ItemCode;
                                line.ItemName = fl.ItemName;
                                line.UgpName = fl.UgpName;
                                line.UgpCode = fl.UgpCode;
                                line.BatchNum = fl.BatchNum;
                                line.Day = fl.Day;
                                line.Quantity = fl.Quantity;
                                line.ReceiptDate = fl.ReceiptDate;
                                line.IssueDate = fl.IssueDate;
                                double FromDay = f[i].FeeLevel.FromDays;
                                if (Day - f[i].FeeLevel.FromDays < 0)
                                {
                                }
                                else
                                {
                                    line.DayToFee = ((int)Day - f[i].FeeLevel.FromDays) +  1;
                                    line.LineTotal = line.DayToFee * fl.Quantity * (double)f[i].FeePrice +
                                                     (line.DayToFee * fl.Quantity * (double)f[i].FeePrice *
                                                         (double)f[i].FeeWAT / 100.00);
                                    line.LineVAT = line.DayToFee * fl.Quantity * (double)f[i].FeePrice *
                                        (double)f[i].FeeWAT / 100.00;
                                    line.Price = (double)f[i].FeePrice;
                                    line.FeeLevelId = f[i].FeeLevel.Id;
                                    line.FeeLevelName = f[i].FeeLevel.Name;
                                    if (line.LineTotal > 0)
                                        listLine.Add(line);
                                    Day = Day - ((Day - f[i].FeeLevel.FromDays) + 1);
                                }
                            }
                        }

                        feebyCustomer.FeebyCustomerLine = listLine;
                        feebyCustomer.Total = listLine.Sum(e => e.LineTotal);
                        feebyCustomer.Vat = listLine.Sum(e => e.LineVAT);
                        list.Add(feebyCustomer);
                    }

                    _context.FeebyCustomer.AddRange(list.Where(e => e.Total > 0));
                    await _context.SaveChangesAsync();
                    return null;
                }
            }
            catch (Exception ex)

            {
                mess.Status = 900;
                mess.Errors = ex.Message;
                return mess;
            }
        }

        public async Task<(FeebyCustomer, Mess)> GetFeeByCustomerAsync(int id)
        {
            Mess mess = new Mess();
            try
            {
                var item = await _context.FeebyCustomer
                    .AsNoTracking()
                    .Include(e => e.FeebyCustomerLine)
                    .FirstOrDefaultAsync(p => p.Id == id);
                await _hubContext.Clients.All.SendAsync("ReceiveNotification", "message");
                return (item, null);
            }
            catch (Exception ex)
            {
                mess.Status = 900;
                mess.Errors = ex.Message;
                return (null, mess);
            }
        }

        public async Task<(FeebyCustomer, Mess)> GetFeeByCustomerAsync(int id, string userId)
        {
            Mess mess = new Mess();
            try
            {
                var user = _context.AppUser.FirstOrDefault(p => p.Id == int.Parse(userId));
                var customer = _context.BP.FirstOrDefault(id => id.Id == (int)user.CardId);
                var item = await _context.FeebyCustomer
                    .AsNoTracking()
                    .Include(e => e.FeebyCustomerLine)
                    .FirstOrDefaultAsync(p => p.Id == id && p.CardCode == customer.CardCode);
                return (item, null);
            }
            catch (Exception ex)
            {
                mess.Status = 900;
                mess.Errors = ex.Message;
                return (null, mess);
            }
        }

        public async Task<(IEnumerable<FeebyCustomer>, Mess)> UpdateConfirmStatus(List<int> model)
        {
            Mess mess = new Mess();
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var items = await _context.FeebyCustomer
                        .AsNoTracking()
                        .Where(e => model.Contains(e.Id)).ToListAsync();
                    if (items.Count < model.Count)
                    {
                        mess.Status = 400;
                        mess.Errors = "Không tồn tại để cập nhập dữ liệu";
                        transaction.Rollback();
                        return (null, mess);
                    }

                    foreach (var item in items)
                    {
                        if (item.ConfirmStatus != "NC")
                        {
                            mess.Status = 400;
                            mess.Errors = "Chứng từ đã được cập nhập trạng thái khác";
                            transaction.Rollback();
                            return (null, mess);
                        }
                        else
                        {
                            item.ConfirmStatus = "CF";
                            item.ConfirmedDate = DateTime.Now;
                            _context.FeebyCustomer.Update(item);
                            var sendUser = await _context.AppUser.AsNoTracking().Where(p => p.UserType == "APSP")
                                .Select(p => p.Id).ToListAsync();
                            _eventAggregator.Publish(new Models.NotificationModels.Notification
                            {
                                Message =
                                    $"Khánh hàng {item.CardCode} đã xác nhận Biên bản tính phí lưu kho {item.FeebyCustomerCode}",
                                Title = $"Biên bản tính phí lưu kho {item.FeebyCustomerCode} đã được xác nhận",
                                Type = "info",
                                Object = new Models.NotificationModels.NotificationObject
                                {
                                    ObjType = "feebycustomer",
                                    ObjId = item.Id,
                                    ObjName = "Fee by Customer",
                                },
                                SendUsers = sendUser
                            });
                        }
                    }

                    await _context.SaveChangesAsync();
                    transaction.Commit();
                    return (items, null);
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

        public async Task<(IEnumerable<FeebyCustomer>, Mess)> UpdatePayStatus(List<int> model)
        {
            Mess mess = new Mess();
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var items = await _context.FeebyCustomer
                        .AsNoTracking()
                        .Where(e => model.Contains(e.Id)).ToListAsync();
                    if (items.Count < model.Count)
                    {
                        mess.Status = 400;
                        mess.Errors = "Không tồn tại để cập nhập dữ liệu";
                        transaction.Rollback();
                        return (null, mess);
                    }

                    foreach (var item in items)
                    {
                        if (item.PayStatus != "NP")
                        {
                            mess.Status = 400;
                            mess.Errors = "Chứng từ đã được cập nhập trạng thái khác";
                            transaction.Rollback();
                            return (null, mess);
                        }
                        else
                        {
                            item.PayStatus = "PD";
                            item.PayedDate = DateTime.Now;
                            _context.FeebyCustomer.Update(item);
                        }
                    }

                    await _context.SaveChangesAsync();
                    transaction.Commit();
                    return (items, null);
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

        public async Task<(IEnumerable<FeebyCustomer>, Mess)> UpdateStatus(List<int> model)
        {
            var mess = new Mess();
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var items = await _context.FeebyCustomer
                    .AsNoTracking()
                    .Where(e => model.Contains(e.Id)).ToListAsync();
                if (items.Count < model.Count)
                {
                    mess.Status = 400;
                    mess.Errors = "Không tồn tại để cập nhập dữ liệu";
                    transaction.Rollback();
                    return (null, mess);
                }

                foreach (var item in items)
                {
                    if (item.Status != "NS")
                    {
                        mess.Status = 400;
                        mess.Errors = "Chứng từ đã được cập nhập trạng thái khác";
                        transaction.Rollback();
                        return (null, mess);
                    }
                    else
                    {
                        item.Status = "SD";
                        item.SendedmDate = DateTime.Now;
                        _context.FeebyCustomer.Update(item);
                        var sendUser = await (from a in _context.AppUser
                            join b in _context.BP on a.CardId equals b.Id
                            where b.CardCode == item.CardCode
                            select a.Id).ToListAsync();
                        _eventAggregator.Publish(new Models.NotificationModels.Notification
                        {
                            Message = $"Bạn có Biên bản tính phí lưu kho mã {item.FeebyCustomerCode} cần xác nhận",
                            Title = "Biên bản tính phí lưu kho mới",
                            Type = "info",
                            Object = new Models.NotificationModels.NotificationObject
                            {
                                ObjType = "feebycustomer",
                                ObjId = item.Id,
                                ObjName = "Fee by Customer"
                            },
                            SendUsers = sendUser
                        });
                    }
                }

                await _context.SaveChangesAsync();
                transaction.Commit();
                return (items, null);
            }
            catch (Exception ex)
            {
                mess.Status = 900;
                mess.Errors = ex.Message;
                transaction.Rollback();
                return (null, mess);
            }
        }

        public async Task<Mess?> UpdateNoteFeeByCus(int id, string note)
        {
            var mess = new Mess();
            try
            {
                var fee = await _context.FeebyCustomer.FirstOrDefaultAsync(p => p.Id == id);
                if (fee == null)
                {
                    mess.Errors = "FeeByCus not found";
                    mess.Status = 404;
                    return mess;
                }

                fee.Note = note;
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
    }

    public class FeeLevelService : Service<FeeLevel>, IFeeLevelService
    {
        private readonly AppDbContext _context;
        private readonly IHostingEnvironment _webHostEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public FeeLevelService(AppDbContext context, IHostingEnvironment webHostEnvironment,
            IHttpContextAccessor httpContextAccessor) : base(context)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<(IEnumerable<FeeLevel>, Mess)> CreateFeeLevelAsync(IEnumerable<FeeLevel> model)
        {
            Mess mess = new Mess();
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    foreach (var fe in model)
                    {
                        if (fe.Status == null)
                        {
                        }
                        else if (fe.Status == "A")
                        {
                            await _context.FeeLevel.AddAsync(fe);
                            await _context.SaveChangesAsync();
                            List<FeeLine> feeLines = new List<FeeLine>();
                            var ougp = await _context.OUGP.ToListAsync();
                            if (ougp.Count > 0)
                            {
                                var fee = await _context.Fee.ToListAsync();
                                foreach (var f in fee)
                                {
                                    foreach (var line in ougp)
                                    {
                                        FeeLine feeLine = new FeeLine();
                                        feeLine.FeeId = f.Id;
                                        feeLine.UgpId = line.Id;
                                        feeLine.FeeLevelId = fe.Id;
                                        feeLine.FeeWAT = 0;
                                        feeLine.FeePrice = 0;
                                        feeLines.Add(feeLine);
                                    }
                                }

                                if (feeLines.Count > 0)
                                    await _context.FeeLine.AddRangeAsync(feeLines);
                            }

                            await _context.SaveChangesAsync();
                        }
                        else if (fe.Status == "D")
                        {
                            var feeLevel = await _context.FeeLevel.FirstOrDefaultAsync(x => x.Id == fe.Id);
                            if (feeLevel == null)
                            {
                                mess.Status = 400;
                                mess.Errors = "Mức tính phí không tồn tại";
                                transaction.Rollback();
                            }
                            _context.FeeLevel.Remove(feeLevel);
                            var feeL = await _context.FeeLine.Where(e => e.FeeLevelId == fe.Id).ToListAsync();
                            _context.FeeLine.RemoveRange(feeL);
                            await _context.SaveChangesAsync();
                        }
                        else if (fe.Status == "U")
                        {
                            var feeLevel = await _context.FeeLevel.FirstOrDefaultAsync(p => p.Id == fe.Id);
                            if (feeLevel == null)
                            {
                                mess.Status = 400;
                                mess.Errors = "Không tồn tại bản ghi để cập nhập";
                                transaction.Rollback();
                                return (null, mess);
                            }
                            feeLevel.Name = fe.Name;
                            feeLevel.ToDays = fe.ToDays;
                            feeLevel.FromDays = fe.FromDays;
                            _context.FeeLevel.Update(feeLevel);
                            await _context.SaveChangesAsync();
                        }
                    }

                    transaction.Commit();
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

        public async Task<(bool, Mess)> DeleteFeeLevelAsync(int id)
        {
            Mess mess = new Mess();
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var feeLevel = await _context.FeeLevel.FirstOrDefaultAsync(x => x.Id == id);
                    if (feeLevel == null)
                    {
                        mess.Status = 400;
                        mess.Errors = "Mức tính phí không tồn tại";
                        transaction.Rollback();
                    }

                    // var feeleelCheck =
                    //     await _context.FeeLine.FirstOrDefaultAsync(x => x.FeeLevelId == id && x.FeePrice > 0);
                    // if (feeleelCheck != null)
                    // {
                    //     mess.Status = 400;
                    //     mess.Errors = "Mức tính phí đã được sử dụng";
                    //     transaction.Rollback();
                    //     return (false, mess);
                    // }

                    _context.FeeLevel.Remove(feeLevel);
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

        public async Task<(IEnumerable<FeeLevel>, Mess, int)> GetAllFeeLevelAsync(int skip = 0, int limit = 30,
            string search = null)
        {
            Mess mess = new Mess();
            try
            {
                var query = _context.Set<FeeLevel>().AsQueryable();
                var querys = _context.Set<FeeLevel>().AsQueryable();
                if (!search.IsNullOrEmpty())
                {
                    query = query.Where(e => e.Name.Contains(search));
                    querys = querys.Where(e => e.Name.Contains(search));
                }

                var totalCount = await query.CountAsync();
                var items = await querys.Skip(skip * limit)
                    .Take(limit)
                    .ToListAsync();
                return (items, null, totalCount);
            }
            catch (Exception ex)

            {
                mess.Status = 900;
                mess.Errors = ex.Message;
                return (null, mess, 0);
            }
        }

        public async Task<(FeeLevel, Mess)> GetFeeLevelByIdAsync(int id)
        {
            Mess mess = new Mess();
            try
            {
                var item = await _context.FeeLevel
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == id);
                return (item, null);
            }
            catch (Exception ex)
            {
                mess.Status = 900;
                mess.Errors = ex.Message;
                return (null, mess);
            }
        }

        public async Task<(FeeLevel, Mess)> UpdateFeeLevelAsync(int id, FeeLevel model)
        {
            Mess mess = new Mess();
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var feeLevel = await _context.FeeLevel.FirstOrDefaultAsync(p => p.Id == id);
                    if (feeLevel == null)
                    {
                        mess.Status = 400;
                        mess.Errors = "Không tồn tại bản ghi để cập nhập";
                        transaction.Rollback();
                        return (null, mess);
                    }

                    // var feeleelCheck =
                    //     await _context.FeeLine.FirstOrDefaultAsync(x => x.FeeLevelId == id && x.FeePrice > 0);
                    // if (feeleelCheck != null)
                    // {
                    //     mess.Status = 400;
                    //     mess.Errors = "Mức tính phí đã được sử dụng";
                    //     transaction.Rollback();
                    //     return (null, mess);
                    // }

                    var item = await _context.Fee
                        .AsNoTracking()
                        .Include(e => e.FeeLine)
                        .ThenInclude(e => e.FeeLevel.Id == id)
                        .FirstOrDefaultAsync();

                    _context.FeeLevel.Update(feeLevel);
                    await _context.SaveChangesAsync();
                    transaction.Commit();
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
        
    }
}