using AutoMapper;
using Azure.Core;
using BackEndAPI.Data;
using BackEndAPI.Models.BPGroups;
using BackEndAPI.Models.Fee;
using BackEndAPI.Models.ItemGroups;
using BackEndAPI.Models.ItemMasterData;
using BackEndAPI.Models.Other;
using BackEndAPI.Models.Promotion;
using BackEndAPI.Models.Unit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Writers;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using B1SLayer;
using Flurl.Util;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static System.Net.Mime.MediaTypeNames;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;
using Gridify;
using BackEndAPI.Models.Document;
using NHibernate;
using Flurl;
using System.Security.Claims;

namespace BackEndAPI.Service.ItemMasterData
{
    public class ItemService : Service<Item>, IItemService
    {
        Endpoints _endpoints;
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _context;
        private readonly IHostingEnvironment _webHostEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly SLConnection _slConnection;
        private readonly LoggingSystemService _systemLog;
        

        public ItemService(AppDbContext context, IHostingEnvironment webHostEnvironment,
            IHttpContextAccessor httpContextAccessor
            , IOptions<Endpoints> options, IConfiguration configuration, SLConnection slConnection, LoggingSystemService systemLog) : base(context)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _httpContextAccessor = httpContextAccessor;
            _endpoints = options.Value;
            _slConnection = slConnection;
            _configuration = configuration;
            _systemLog = systemLog;
        }
        

        public async Task<(Item, Mess)> CreateItemAsync(ItemView model)
        {
            string json = model.item;
            Mess mess = new Mess();
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    Item item = JsonConvert.DeserializeObject<Item>(json);
                    var oitm = await _context.Item.FirstOrDefaultAsync(p => p.ItemCode == item.ItemCode);
                    if (oitm != null)
                    {
                        mess.Status = 400;
                        mess.Errors = "Mã hàng hóa đã tồn tại";
                        transaction.Rollback();
                        return (null, mess);
                    }

                    if (model.images != null && model.images.Count > 0)
                    {
                        var uploadsFolder = Path.Combine(_webHostEnvironment.ContentRootPath, "uploads");
                        Directory.CreateDirectory(uploadsFolder);
                        foreach (var file in model.images)
                        {
                            if (file != null)
                            {
                                foreach (var itm1 in item.ITM1)
                                {
                                    if (itm1.FileName.Equals(file.FileName))
                                    {
                                        var fileName = file.FileName;
                                        var fileNameSaving =
                                            Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                                        var filePath = Path.Combine(uploadsFolder, fileNameSaving);

                                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                                        {
                                            await file.CopyToAsync(fileStream);
                                        }

                                        var request = _httpContextAccessor.HttpContext.Request;
                                        var baseUrl = $"{request.Scheme}://{request.Host}";
                                        var imageUrl = $"{baseUrl}/uploads/{fileNameSaving}";
                                        itm1.FilePath = imageUrl;
                                    }
                                }
                            }
                        }
                    }

                    _context.Item.Add(item);
                    await _systemLog.SaveAsync("INFO", "Create", $"Tạo mới sản phẩm {item.ItemCode}",
                        "Product", item.Id);
                    await _context.SaveChangesAsync();
                    transaction.Commit();
                    return (item, null);
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

        public async Task<(Item, Mess)> UpdateItemAsync(int id, ItemView model)
        {
            Mess mess = new Mess();
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var item = await _context.Item
                        .AsNoTracking()
                        .Include(p => p.ITM1)
                        .FirstOrDefaultAsync(p => p.Id == id);
                    if (item == null)
                    {
                        mess.Status = 400;
                        mess.Errors = "Dữ liệu trống";
                        return (null, mess);
                    }

                    string json = model.item;
                    Item items = JsonConvert.DeserializeObject<Item>(json);
                    var item1 = await _context.Item
                        .AsNoTracking()
                        .Include(p => p.ITM1)
                        .FirstOrDefaultAsync(p => p.Id != id && p.ItemCode == items.ItemCode);
                    if (item1 != null)
                    {
                        mess.Status = 800;
                        mess.Errors = "Mã hàng hóa đã tồn tại";
                        return (null, mess);
                    }

                    if (id != items.Id)
                    {
                        mess.Status = 400;
                        mess.Errors = "Dữ liệu không khớp";
                        return (null, mess);
                    }

                    var dtoType = items.GetType();
                    var entityType = item.GetType();

                    foreach (var prop in dtoType.GetProperties())
                    {
                        var dtoValue = prop.GetValue(items);
                        if (dtoValue != null)
                        {
                            var entityProp = entityType.GetProperty(prop.Name);
                            if (prop.Name.Equals("CreatedDate") || prop.Name.Equals("ITM1"))
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
                    if (model.images != null && model.images.Count > 0)
                    {
                        foreach (var file in model.images)
                        {
                            if (file != null)
                            {
                                foreach (var itm1 in items.ITM1.ToList())
                                {
                                    if (itm1.Status != null)
                                    {
                                        if (itm1.FileName.Equals(file.FileName))
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
                                            if (!string.IsNullOrEmpty(itm1.Status))
                                            {
                                                if (itm1.Status.Equals("U"))
                                                {
                                                    var image = item.ITM1.FirstOrDefault(i => i.Id == itm1.Id);
                                                    var filePathDel = image.FilePath;
                                                    if (File.Exists(filePathDel))
                                                    {
                                                        File.Delete(filePathDel);
                                                    }

                                                    if (image != null)
                                                    {
                                                        image.FileName = fileName;
                                                        image.FilePath = imageUrl;
                                                        image.Note = itm1.Note;
                                                    }
                                                }
                                                else if (itm1.Status.Equals("A"))
                                                {
                                                    item.ITM1.Add(new ITM1
                                                    {
                                                        FileName = fileName,
                                                        FilePath = imageUrl,
                                                        Note = itm1.Note
                                                    });
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (items.ITM1 != null)
                    {
                        foreach (var itm1 in items.ITM1.ToList())
                        {
                            if (!string.IsNullOrEmpty(itm1.Status))
                            {
                                if (itm1.Status.Equals("D"))
                                {
                                    var image = item.ITM1.FirstOrDefault(i => i.Id == itm1.Id);
                                    if (image != null)
                                    {
                                        _context.ITM1.Remove(image);
                                        item.ITM1.Remove(image);
                                    }
                                }
                            }
                        }
                    }

                    _context.Item.Update(item);
                    await _systemLog.SaveAsync("INFO", "Update", $"Cập nhật sản phẩm {item.ItemCode}",
                        "Product", item.Id);
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

        public async Task<(bool, Mess)> DeleteItemAsync(int id)
        {
            Mess mess = new Mess();
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var item = await _context.Item
                        .AsNoTracking()
                        .FirstOrDefaultAsync(p => p.Id == id);
                    if (item == null)
                    {
                        mess.Status = 400;
                        mess.Errors = "Dữ liệu trống";
                        transaction.Rollback();
                        return (false, mess);
                    }

                    var doc = await _context.DOC1
                        .AsNoTracking()
                        .FirstOrDefaultAsync(p => p.ItemId == id);
                    if (doc != null)
                    {
                        mess.Status = 400;
                        mess.Errors = "Hàng hóa đã được sử dụng";
                        transaction.Rollback();
                        return (false, mess);
                    }

                    _context.Item.Remove(item);
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

        public async Task<(Item, Mess)> GetItemByIdAsync(int id)
        {
            Mess mess = new Mess();
            try
            {
                var user = _httpContextAccessor.HttpContext?.User;
                var userId = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var pricelistEntity = _context.PriceLists
                 .Include(e => e.PriceListLine)
                 .Where(e => e.IsActive == true
                          && e.IsRetail == true
                          && e.EffectDate <= DateTime.UtcNow
                          && e.ExpriedDate >= DateTime.UtcNow)
                 .OrderByDescending(e => e.CreatedDate)
                 .FirstOrDefault();

                List<ItemPrice> pricelist = pricelistEntity?.PriceListLine
                    ?.Select(line => new ItemPrice
                    {
                        ItemCode = line.ItemCode,
                        Price = line.Price,
                        Currency = line.Currency
                    })
                    .ToList();
                string[] itemCode = null;
                if (pricelist != null)
                    itemCode = pricelist.Select(line => line.ItemCode).ToArray();
                var query =  _context.Item
                    .AsNoTracking()
                    .Include(p => p.ITM1)
                    .Include(p => p.Brand)
                    .Include(p => p.Industry)
                    .Include(p => p.Packing)
                    .Include(p => p.ItemType)
                    .Include(p => p.TaxGroups)
                    .Include(P => P.ProductApplications)
                    .Include(P => P.ProductGroup)
                    .Include(P => P.ProductQualityLevel)
                    .Include(P => P.OUGP)
                    .ThenInclude(P => P.UGP1)
                    .ThenInclude(p => p.OUOM);
                Item item;
                    item = query.FirstOrDefault(p => p.Id == id);
                if (userId == null)
                {
                    var priceInfo = pricelist.FirstOrDefault(p => p.ItemCode == item?.ItemCode);
                    if (priceInfo != null)
                    {
                        item.Price = priceInfo.Price;
                        item.Currency = priceInfo.Currency;
                    }
                }
                return (item, null);
            }
            catch (Exception ex)
            {
                mess.Status = 900;
                mess.Errors = ex.Message;
                return (null, mess);
            }
        }

        public async Task<(IEnumerable<Item>, Mess)> GetItemAsync(string search)
        {
            Mess mess = new Mess();
            try
            {
                var item = await _context.Item
                    .Include(p => p.ITM1)
                    .Include(p => p.Brand)
                    .Include(p => p.Industry)
                    .Include(p => p.Packing)
                    .Include(p => p.ItemType)
                    .Include(p => p.TaxGroups)
                    .Include(P => P.OUGP)
                    .ThenInclude(P => P.UGP1)
                    .Where(p => p.ItemCode.Contains(search) ||
                                p.ItemName.Contains(search))
                    .ToListAsync();
                return (item, null);
            }
            catch (Exception ex)
            {
                mess.Status = 900;
                mess.Errors = ex.Message;
                return (null, mess);
            }
        }


        public async Task SyncFromSap(List<ItemSync> list)
        {
            var listItem = list.Select(x => x.ItemCode).ToList();
            var items = await _context.Item.Where(p=>listItem.Contains(p.ItemCode)).ToListAsync();

            foreach (var item in items)
            {
                try
                {
                    var itemDT = list.Where(p => p.ItemCode == item.ItemCode).FirstOrDefault();
                    item.ProductApplicationsCode = itemDT.U_UD;
                    item.ProductQualityLevelCode = itemDT.U_CCL;
                    item.ProductGroupCode = itemDT.U_NH;
                    item.Price = itemDT.Price;
                    await _context.SaveChangesAsync();
                }
                catch
                {
                    continue;
                }
            }
        }
        public async Task<Mess> SyncFromSap()
        {
            Mess ms = new Mess();
            try
            {
                ms = await CCL();
                if (ms != null)
                    return ms;
                ms = await UD();
                if (ms != null)
                    return ms;
                ms = await TH();
                if (ms != null)
                    return ms;
                ms = await NH();
                if (ms != null)
                    return ms;
                ms = await NGHANG();
                if (ms != null)
                    return ms;
                if (ms != null)
                    return ms;
                var client = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Get,
                    _endpoints.Host + "/ItemMasterData");
                request.Headers.Add("accept", "*/*");
                var response = await client.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    // Read the response content as a JSON string
                    var jsonString = await response.Content.ReadAsStringAsync();
                    jsonString = jsonString.Replace("\"OITM\":", "");
                    int jsonLen = jsonString.Length;

                    string jsonStringF = jsonString.Substring(1);

                    jsonLen = jsonStringF.Length - 1;
                    string jsonStringL = jsonStringF.Substring(0, jsonLen);
                    if (jsonString.Length > 0)
                    {
                        var itemMaster = JsonConvert.DeserializeObject<List<ItemMaster>>(jsonStringL);
                        if (itemMaster != null)
                        {
                            var taxGroup = _context.TaxGroups.ToList();
                            int taxId = 0;
                            var listItems = itemMaster.Select(x => x.ItemCode).ToList();
                            var items = _context.Item.Where(p => listItems.Contains(p.ItemCode)).ToList();
                            var brands = _context.Brand.ToList();
                            var Industry = _context.Industry.ToList();
                            var listItemType = _context.ItemType.ToList();
                            items.ForEach(item =>
                            {
                                var vat = itemMaster.FirstOrDefault(e => e.ItemCode == item.ItemCode).Vat;
                                item.ItemName = itemMaster.FirstOrDefault(e => e.ItemCode == item.ItemCode).ItemName;
                                if(itemMaster.FirstOrDefault(e => e.ItemCode == item.ItemCode)?.Price == 0)
                                    item.IsActive = false;
                                else
                                    item.Price = itemMaster.FirstOrDefault(e => e.ItemCode == item.ItemCode).Price;
                                item.Currency = itemMaster.FirstOrDefault(e => e.ItemCode == item.ItemCode).Currency;
                                if (itemMaster.FirstOrDefault(e => e.ItemCode == item.ItemCode).validFor == "Y")
                                    item.IsActive = true;
                                else
                                {
                                    item.IsActive = false;
                                    item.Price = 0;
                                }
                                if (!itemMaster.FirstOrDefault(e => e.ItemCode == item.ItemCode).NH.IsNullOrEmpty())
                                {
                                    var check = listItemType.FirstOrDefault(p => p.Code.ToUpper().Trim() == itemMaster.FirstOrDefault(e => e.ItemCode == item.ItemCode).NH.ToUpper().Trim());
                                    if (check != null)
                                    {
                                        var id = check.Id;
                                        if (id != null)
                                            item.ItemTypeId = id;
                                    }
                                    taxId = taxGroup.FirstOrDefault(e => e.Rate == vat).Id;
                                    item.TaxGroupsId = taxId;
                                }
                                else
                                {
                                    taxId = taxGroup.FirstOrDefault(e => e.Rate == 0).Id;
                                    item.TaxGroupsId = taxId;
                                    item.ItemTypeId = listItemType.FirstOrDefault(p => p.Code.ToUpper().Trim() == "VPKM").Id;
                                }
                                    
                                if (!itemMaster.FirstOrDefault(e => e.ItemCode == item.ItemCode).NH.IsNullOrEmpty())
                                    item.ProductGroupCode = itemMaster.FirstOrDefault(e => e.ItemCode == item.ItemCode).NH;
                                if (!itemMaster.FirstOrDefault(e => e.ItemCode == item.ItemCode).CCL.IsNullOrEmpty())
                                    item.ProductQualityLevelCode = itemMaster.FirstOrDefault(e => e.ItemCode == item.ItemCode).CCL;
                                if (!itemMaster.FirstOrDefault(e => e.ItemCode == item.ItemCode).UD.IsNullOrEmpty())
                                    item.ProductApplicationsCode = itemMaster.FirstOrDefault(e => e.ItemCode == item.ItemCode).UD;
                                if (!itemMaster.FirstOrDefault(e => e.ItemCode == item.ItemCode).Industry.IsNullOrEmpty())
                                {
                                    string industry = itemMaster.FirstOrDefault(e => e.ItemCode == item.ItemCode).Industry;
                                    item.IndustryId = Industry.FirstOrDefault(e => e.Code == industry).Id;
                                }
                                else
                                    item.IndustryId = null;
                                if (!itemMaster.FirstOrDefault(e => e.ItemCode == item.ItemCode).Brand.IsNullOrEmpty())
                                {
                                    string bras = itemMaster.FirstOrDefault(e => e.ItemCode == item.ItemCode).Brand;
                                    item.BrandId = brands.FirstOrDefault(e => e.Code == bras).Id;
                                }
                                else
                                    item.BrandId = null;
                            });
                            var listItem = _context.Item.Select(e => e.ItemCode).ToList();
                            var listPacking = _context.Packing.ToList();
                            var listOUGP = _context.OUGP.ToList();
                            List<Item> newList = new List<Item>();
                            foreach (var item in itemMaster.Where(e => !listItem.Contains(e.ItemCode)).ToList())
                            {
                                if (!listItem.Contains(item.ItemCode))
                                {
                                    var packingid = listPacking.FirstOrDefault(p => p.Name.ToUpper().Trim() == item.UoM.ToUpper().Trim()).Id;
                                    var ugpid = listOUGP.FirstOrDefault(p => p.UgpName.ToUpper().Trim() == item.UoM.ToUpper().Trim()).Id;
                                    int? ItemTypeId = null;
                                    
                                    if (item.ItemType == "")
                                    {
                                        taxId = taxGroup.FirstOrDefault(e => e.Rate == 0).Id;
                                        ItemTypeId = listItemType.FirstOrDefault(p => p.Code.ToUpper().Trim() == "VPKM").Id;
                                    }
                                        
                                    else
                                    {
                                        var check = listItemType.FirstOrDefault(p => p.Code.ToUpper().Trim() == itemMaster.FirstOrDefault(e => e.ItemCode == item.ItemCode).NH.ToUpper().Trim());
                                        if (check != null)
                                        {
                                            var id = check.Id;
                                            if (id != null)
                                                ItemTypeId = id;
                                        }
                                        taxId = taxGroup.FirstOrDefault(e => e.Rate == item.Vat).Id;
                                    }
                                    bool Active = true;
                                    if (item.validFor == "N")
                                        Active = false;
                                    
                                    Item newItem = new Item
                                    {
                                        ItemCode = item.ItemCode,
                                        ItemName = item.ItemName,
                                        Price = item.Price,
                                        PackingId = packingid,
                                        ItemTypeId = ItemTypeId,
                                        TaxGroupsId = taxId,
                                        ProductGroupCode = null,
                                        ProductApplicationsCode = null,
                                        ProductQualityLevelCode = null,
                                        UgpEntry = ugpid,
                                        IsActive = Active

                                    };
                                    if (!item.Industry.IsNullOrEmpty())
                                    {
                                        string industry = itemMaster.FirstOrDefault(e => e.ItemCode == item.ItemCode).Industry;
                                        newItem.IndustryId = Industry.FirstOrDefault(e => e.Code == industry).Id;
                                    }
                                    else
                                        newItem.IndustryId = null;
                                    if (!item.Brand.IsNullOrEmpty())
                                    {
                                        string bras = itemMaster.FirstOrDefault(e => e.ItemCode == item.ItemCode).Brand;
                                        newItem.BrandId = brands.FirstOrDefault(e => e.Code == bras).Id;
                                    }
                                    else
                                        newItem.BrandId = null;
                                    if (!item.NH.IsNullOrEmpty())
                                        newItem.ProductGroupCode = item.NH;
                                    if (!item.CCL.IsNullOrEmpty())
                                        newItem.ProductQualityLevelCode = item.CCL;
                                    if (!item.UD.IsNullOrEmpty())
                                        newItem.ProductApplicationsCode = item.UD;
                                    else
                                        newItem.ProductGroupCode = null;
                                    newList.Add(newItem);
                                }
                            }
                            if (newList.Count > 0)
                            {
                                _context.Item.AddRange(newList);
                            }    
                                
                            await _context.SaveChangesAsync();
                        }
                    }
                }
                else
                {
                    ms.Status = (int)response.StatusCode;
                    ms.Errors = "Lỗi đồng bộ";
                    return ms;
                }
                string query = "INSERT into ItemSpec(BrandId,Brand,IndustryId,Industry,ItemTypeId,ItemType,PackingId,Packing) " +
                    " SELECT distinct BrandId, T2.Name Brand, IndustryId, T12.Name Industry, ItemTypeId, T22.Name ItemType, PackingId , T32.Name Packing " +
                    " from OITM T1 left join Brand T2 on T1.BrandId = T2.Id left join Industry T12 on T1.IndustryId = T12.Id " +
                    " left join ItemType T22 on T1.ItemTypeId = T22.Id left join Packing T32 on T1.PackingId = T32.Id " +
                    " where BrandId is not null and not exists (select 1 from ItemSpec where BrandId = T1.BrandId and IndustryId = T1.IndustryId and ItemTypeId = T1.ItemTypeId and PackingId = T1.PackingId)";
                try
                {
                    _context.Database.ExecuteSqlRaw(query);
                }
                catch (Exception ex)
                {
                    ms.Errors = ex.Message;
                    ms.Status = 900;
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
        public async Task<Mess> CCL()
        {
            Mess ms = new Mess();
            try
            {
                var client = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Get,
                    _endpoints.Host + "/CCL");
                request.Headers.Add("accept", "*/*");
                var response = await client.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    // Read the response content as a JSON string
                    var jsonString = await response.Content.ReadAsStringAsync();
                    jsonString = jsonString.Replace("\"CCL\":", "");
                    int jsonLen = jsonString.Length;

                    string jsonStringF = jsonString.Substring(1);

                    jsonLen = jsonStringF.Length - 1;
                    string jsonStringL = jsonStringF.Substring(0, jsonLen);
                    if (jsonString.Length > 0)
                    {
                        var itemMaster = JsonConvert.DeserializeObject<List<CCL>>(jsonStringL);
                        if (itemMaster != null)
                        {
                            var listItems = itemMaster.Select(x => x.Code).ToList();
                            var items = _context.ProductQualityLevel.Where(p => listItems.Contains(p.Code)).ToList();
                            items.ForEach(item =>
                            {
                                item.Name = itemMaster.FirstOrDefault(e => e.Code == item.Code).Name;
                            });
                            var listItem = _context.ProductQualityLevel.Select(e => e.Code).ToList();
                            List<ProductQualityLevel> newList = new List<ProductQualityLevel>();
                            foreach (var item in itemMaster.Where(e=>!listItem.Contains(e.Code)).ToList())
                            {
                                if (!listItem.Contains(item.Code))
                                {
                                    ProductQualityLevel newItem = new ProductQualityLevel
                                    {
                                        Code = item.Code,
                                        Name = item.Name,
                                    };
                                    newList.Add(newItem);
                                }
                            }
                            if (newList.Count > 0)
                                _context.ProductQualityLevel.AddRange(newList);
                            await _context.SaveChangesAsync();
                        }
                    }
                }
                else
                {
                    ms.Status = (int)response.StatusCode;
                    ms.Errors = "Lỗi đồng bộ";
                    return ms;
                }
                return null;
            }
            catch (Exception ex)
            {
                ms.Errors = ex.Message;
                ms.Status = 900;
                return ms;
            }
        }
        public async Task<Mess> TH()
        {
            Mess ms = new Mess();
            try
            {
                var client = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Get,
                    _endpoints.Host + "/TH");
                request.Headers.Add("accept", "*/*");
                var response = await client.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    // Read the response content as a JSON string
                    var jsonString = await response.Content.ReadAsStringAsync();
                    jsonString = jsonString.Replace("\"CCL\":", "");
                    int jsonLen = jsonString.Length;

                    string jsonStringF = jsonString.Substring(1);

                    jsonLen = jsonStringF.Length - 1;
                    string jsonStringL = jsonStringF.Substring(0, jsonLen);
                    if (jsonString.Length > 0)
                    {
                        var itemMaster = JsonConvert.DeserializeObject<List<CCL>>(jsonStringL);
                        if (itemMaster != null)
                        {
                            var listItems = itemMaster.Select(x => x.Code).ToList();
                            var items = _context.Brand.Where(p => listItems.Contains(p.Code)).ToList();
                            items.ForEach(item =>
                            {
                                item.Name = itemMaster.FirstOrDefault(e => e.Code == item.Code).Name;
                            });
                            var listItem = _context.Brand.Select(e => e.Code).ToList();
                            List<Brand> newList = new List<Brand>();
                            foreach (var item in itemMaster.Where(e => !listItem.Contains(e.Code)).ToList())
                            {
                                if (!listItem.Contains(item.Code))
                                {
                                    Brand newItem = new Brand
                                    {
                                        Code = item.Code,
                                        Name = item.Name,
                                    };
                                    newList.Add(newItem);
                                }
                            }
                            if (newList.Count > 0)
                                _context.Brand.AddRange(newList);
                            await _context.SaveChangesAsync();
                        }
                    }
                }
                else
                {
                    ms.Status = (int)response.StatusCode;
                    ms.Errors = "Lỗi đồng bộ";
                    return ms;
                }
                return null;
            }
            catch (Exception ex)
            {
                ms.Errors = ex.Message;
                ms.Status = 900;
                return ms;
            }
        }
        public async Task<Mess> NGHANG()
        {
            Mess ms = new Mess();
            try
            {
                var client = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Get,
                    _endpoints.Host + "/NGHANG");
                request.Headers.Add("accept", "*/*");
                var response = await client.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    // Read the response content as a JSON string
                    var jsonString = await response.Content.ReadAsStringAsync();
                    jsonString = jsonString.Replace("\"CCL\":", "");
                    int jsonLen = jsonString.Length;

                    string jsonStringF = jsonString.Substring(1);

                    jsonLen = jsonStringF.Length - 1;
                    string jsonStringL = jsonStringF.Substring(0, jsonLen);
                    if (jsonString.Length > 0)
                    {
                        var itemMaster = JsonConvert.DeserializeObject<List<CCL>>(jsonStringL);
                        if (itemMaster != null)
                        {
                            var listItems = itemMaster.Select(x => x.Code).ToList();
                            var items = _context.Industry.Where(p => listItems.Contains(p.Code)).ToList();
                            items.ForEach(item =>
                            {
                                item.Name = itemMaster.FirstOrDefault(e => e.Code == item.Code).Name;
                            });
                            var listItem = _context.Industry.Select(e => e.Code).ToList();
                            List<Industry> newList = new List<Industry>();
                            foreach (var item in itemMaster.Where(e => !listItem.Contains(e.Code)).ToList())
                            {
                                if (!listItem.Contains(item.Code))
                                {
                                    Industry newItem = new Industry
                                    {
                                        Code = item.Code,
                                        Name = item.Name,
                                    };
                                    newList.Add(newItem);
                                }
                            }
                            if (newList.Count > 0)
                                _context.Industry.AddRange(newList);
                            await _context.SaveChangesAsync();
                        }
                    }
                }
                else
                {
                    ms.Status = (int)response.StatusCode;
                    ms.Errors = "Lỗi đồng bộ";
                    return ms;
                }
                return null;
            }
            catch (Exception ex)
            {
                ms.Errors = ex.Message;
                ms.Status = 900;
                return ms;
            }
        }
        public async Task<Mess> UD()
        {
            Mess ms = new Mess();
            try
            {
                var client = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Get,
                    _endpoints.Host + "/UD");
                request.Headers.Add("accept", "*/*");
                var response = await client.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    // Read the response content as a JSON string
                    var jsonString = await response.Content.ReadAsStringAsync();
                    jsonString = jsonString.Replace("\"CCL\":", "");
                    int jsonLen = jsonString.Length;

                    string jsonStringF = jsonString.Substring(1);

                    jsonLen = jsonStringF.Length - 1;
                    string jsonStringL = jsonStringF.Substring(0, jsonLen);
                    if (jsonString.Length > 0)
                    {
                        var itemMaster = JsonConvert.DeserializeObject<List<CCL>>(jsonStringL);
                        if (itemMaster != null)
                        {
                            var listItems = itemMaster.Select(x => x.Code).ToList();
                            var items = _context.ProductApplications.Where(p => listItems.Contains(p.Code)).ToList();
                            items.ForEach(item =>
                            {
                                item.Name = itemMaster.FirstOrDefault(e => e.Code == item.Code).Name;
                            });
                            var listItem = _context.ProductApplications.Select(e => e.Code).ToList();
                            List<ProductApplications> newList = new List<ProductApplications>();
                            foreach (var item in itemMaster.Where(e => !listItem.Contains(e.Code)).ToList())
                            {
                                if (!listItem.Contains(item.Code))
                                {
                                    ProductApplications newItem = new ProductApplications
                                    {
                                        Code = item.Code,
                                        Name = item.Name,
                                    };
                                    newList.Add(newItem);
                                }
                            }
                            if (newList.Count > 0)
                                _context.ProductApplications.AddRange(newList);
                            await _context.SaveChangesAsync();
                        }
                    }
                }
                else
                {
                    ms.Status = (int)response.StatusCode;
                    ms.Errors = "Lỗi đồng bộ";
                    return ms;
                }
                return null;
            }
            catch (Exception ex)
            {
                ms.Errors = ex.Message;
                ms.Status = 900;
                return ms;
            }
        }
        public async Task<Mess> NH()
        {
            Mess ms = new Mess();
            try
            {
                var client = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Get,
                    _endpoints.Host + "/NH");
                request.Headers.Add("accept", "*/*");
                var response = await client.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    // Read the response content as a JSON string
                    var jsonString = await response.Content.ReadAsStringAsync();
                    jsonString = jsonString.Replace("\"CCL\":", "");
                    int jsonLen = jsonString.Length;

                    string jsonStringF = jsonString.Substring(1);

                    jsonLen = jsonStringF.Length - 1;
                    string jsonStringL = jsonStringF.Substring(0, jsonLen);
                    if (jsonString.Length > 0)
                    {
                        var itemMaster = JsonConvert.DeserializeObject<List<CCL>>(jsonStringL);
                        if (itemMaster != null)
                        {
                            var listItems = itemMaster.Select(x => x.Code).ToList();
                            var items = _context.ProductGroup.Where(p => listItems.Contains(p.Code)).ToList();
                            items.ForEach(item =>
                            {
                                item.Name = itemMaster.FirstOrDefault(e => e.Code == item.Code).Name;
                            });
                            var listItem = _context.ProductGroup.Select(e => e.Code).ToList();
                            List<ProductGroup> newList = new List<ProductGroup>();
                            foreach (var item in itemMaster.Where(e => !listItem.Contains(e.Code)).ToList())
                            {
                                if (!listItem.Contains(item.Code))
                                {
                                    ProductGroup newItem = new ProductGroup
                                    {
                                        Code = item.Code,
                                        Name = item.Name,
                                    };
                                    newList.Add(newItem);
                                }
                            }
                            if (newList.Count > 0)
                                _context.ProductGroup.AddRange(newList);


                            var items1 = _context.ItemType.Where(p => listItems.Contains(p.Code)).ToList();
                            items1.ForEach(item =>
                            {
                                item.Name = itemMaster.FirstOrDefault(e => e.Code == item.Code).Name;
                            });
                            var listItem1 = _context.ItemType.Select(e => e.Code).ToList();
                            List<ItemType> newList1 = new List<ItemType>();
                            foreach (var item in itemMaster.Where(e => !listItem1.Contains(e.Code)).ToList())
                            {
                                if (!listItem1.Contains(item.Code))
                                {
                                    ItemType newItem1 = new ItemType
                                    {
                                        Code = item.Code,
                                        Name = item.Name,
                                    };
                                    newList1.Add(newItem1);
                                }
                            }
                            if (newList1.Count > 0)
                                _context.ItemType.AddRange(newList1);

                            await _context.SaveChangesAsync();
                        }
                    }
                }
                else
                {
                    ms.Status = (int)response.StatusCode;
                    ms.Errors = "Lỗi đồng bộ";
                    return ms;
                }
                return null;
            }
            catch (Exception ex)
            {
                ms.Errors = ex.Message;
                ms.Status = 900;
                return ms;
            }
        }
        public async Task<(IEnumerable<Item>, Mess)> GetItemAsync(int brand, int industry, int itemType, int packing,
            string search)
        {
            Mess mess = new Mess();
            try
            {
                var item = await _context.Item
                    .Include(p => p.ITM1)
                    .Include(p => p.Brand)
                    .Include(p => p.Industry)
                    .Include(p => p.Packing)
                    .Include(p => p.ItemType)
                    .Include(p => p.TaxGroups)
                    .Include(P => P.OUGP)
                    .ThenInclude(P => P.UGP1)
                    .Where(p => (p.ItemCode.Contains(search) || p.ItemName.Contains(search)) &&
                                (brand == 0 || p.BrandId == brand)
                                && (industry == 0 || p.IndustryId == industry) &&
                                (itemType == 0 || p.ItemTypeId == itemType)
                                && (packing == 0 || p.PackingId == packing)
                    )
                    .ToListAsync();
                return (item, null);
            }
            catch (Exception ex)
            {
                mess.Status = 900;
                mess.Errors = ex.Message;
                return (null, mess);
            }
        }

        public async Task<(IEnumerable<Item>, Mess)> GetItemAsync(int brand, int industry, int itemType, int packing,
            string CardCode, string search)
        {
            Mess mess = new Mess();
            try
            {
                string a = ",1,9,82,";
                var item = await _context.Item
                    .Include(p => p.ITM1)
                    .Include(p => p.Brand)
                    .Include(p => p.Industry)
                    .Include(p => p.Packing)
                    .Include(p => p.ItemType)
                    .Include(p => p.TaxGroups)
                    .Include(P => P.OUGP)
                    .ThenInclude(P => P.UGP1)
                    .ThenInclude(UGP1 => UGP1.OUOM)
                    .Include(P => P.OUGP)
                    .ThenInclude(P => P.OUOM)
                    .Where(p => (p.ItemCode.Contains(search) || p.ItemName.Contains(search) || search == null) &&
                                (brand == 0 || p.BrandId == brand)
                                && (industry == 0 || p.IndustryId == industry) &&
                                (itemType == 0 || p.ItemTypeId == itemType)
                                && (packing == 0 || p.PackingId == packing) &&
                                a.Contains("," + p.IndustryId.ToString() + ",")
                    )
                    .ToListAsync();
                return (item, null);
            }
            catch (Exception ex)
            {
                mess.Status = 900;
                mess.Errors = ex.Message;
                return (null, mess);
            }
        }

        public async Task<(IEnumerable<Item>, Mess, int total)> GetAllItemsAsync(GridifyQuery gridifyQuery)
        {
            Mess mess = new Mess();
            try
            {
                var query =  _context.Set<Item>()
                     .Include(p => p.ITM1)
                    .Include(p => p.Brand)
                    .Include(p => p.Industry)
                    .Include(p => p.Packing)
                    .Include(p => p.ItemType)
                    .Include(p => p.TaxGroups)
                    .Include(P => P.OUGP)
                    .ThenInclude(P => P.UGP1)
                    .ThenInclude(p => p.OUOM)
                    .Include(P => P.OUGP)
                    .ThenInclude(P => P.OUOM)
                    .AsQueryable();
                var filteredQuery = query.ApplyFiltering(gridifyQuery);
                var total = await filteredQuery.CountAsync();
                var sortedQuery = filteredQuery
                     .ApplyOrdering(gridifyQuery)
                     .ApplyPaging(gridifyQuery);

                var items = await sortedQuery.ToListAsync();
                return (items, null, total);
            }
            catch (Exception ex)

            {
                mess.Status = 900;
                mess.Errors = ex.Message;
                return (null, mess, 0);
            }
        }

        public async Task<(IEnumerable<ItemListView>, Mess)> GetItemOrBarCode1Async(string search, int branchId)
        {
            Mess mess = new Mess();
            try
            {
                var items = await _context.ItemListViews.FromSqlRaw("usp_getItemandBarCode {0},{1}", search, branchId)
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

        public Task<(IEnumerable<Item>, Mess)> GetItemOrBarCodeAsync(string search, int branchId)
        {
            throw new NotImplementedException();
        }

        public async Task<(bool, Mess)> UpdateItemAsync(string itemCode, ItemOnhand model)
        {
            Mess mess = new Mess();
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var item = await _context.Item
                        .AsNoTracking()
                        .Include(p => p.ITM1)
                        .FirstOrDefaultAsync(p => p.ItemCode == itemCode);
                    if (item == null)
                    {
                        mess.Status = 400;
                        mess.Errors = "Dữ liệu trống";
                        return (false, mess);
                    }

                    if (itemCode != model.ItemCode)
                    {
                        mess.Status = 400;
                        mess.Errors = "Dữ liệu không khớp";
                        return (false, mess);
                    }

                    item.OnHand = model.Onhand;
                    item.OnOrder = model.OnOrder;
                    _context.Item.Update(item);
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

        public async Task<(IEnumerable<ItemSpecHierarchy>, Mess)> GetHierarchySpecAsync(int? CardId)
        {
            Mess mess = new Mess();
            try { 


                int[] BPId  = null;
                int[] Brands = null;
                int[] ItemType = null;
                if (CardId != null)
                {
                    BPId = _context.BpClassify.Where(e => e.BpId == CardId).Select(e => e.IndustryId).ToArray();
                    Brands = _context.BpClassify.Include(e=>e.Brands).Where(e => e.BpId == CardId).SelectMany(e => e.Brands.Select(b => b.Id)).ToArray();
                    ItemType = _context.BpClassify.Include(e => e.ItemType).Where(e => e.BpId == CardId).SelectMany(e => e.ItemType.Select(b => b.Id)).ToArray();
                }
                    
                List<ItemSpecHierarchy> listSpec = new List<ItemSpecHierarchy>();
                List<ItemSpec> itemSpec = new List<ItemSpec>();
                if(BPId != null)
                    itemSpec = await _context.ItemSpec.AsNoTracking().Where(e=> (BPId.Contains(e.IndustryId) || BPId.Count() == 0) && (Brands.Count() == 0 || Brands.Contains(e.BrandId)) && (ItemType.Count() == 0 || ItemType.Contains(e.ItemTypeId))).ToListAsync();
                else
                    itemSpec = await _context.ItemSpec.AsNoTracking().ToListAsync();
                var brand = itemSpec.DistinctBy(x => new { x.BrandId, x.Brand }).ToList();

                foreach (var b in brand)
                {
                    ItemSpecHierarchy spec = new ItemSpecHierarchy();
                    spec.BrandId = b.BrandId;
                    spec.BandName = b.Brand;
                    var industry = itemSpec.Where(p => p.BrandId == b.BrandId)
                        .Select(x => new { x.IndustryId, x.Industry }).Distinct().Select(n =>
                            new Nganhhang { IndustryId = n.IndustryId, IndustryName = n.Industry }).ToList();
                    List<Nganhhang> listn = new List<Nganhhang>();
                    foreach (var i in industry)
                    {
                        Nganhhang nganh = new Nganhhang();
                        nganh.IndustryId = i.IndustryId;
                        nganh.IndustryName = i.IndustryName;
                        ///var sp = itemSpec.DistinctBy(x => new { x.ItemTypeId, x.ItemType }).Where(x => x.BrandId == b.BrandId && x.IndustryId == i.IndustryId).ToList();
                        var sp = itemSpec.Where(x => x.BrandId == b.BrandId && x.IndustryId == i.IndustryId)
                            .Select(x => new { x.ItemTypeId, x.ItemType }).Distinct().Select(n =>
                                new LoaiSP { ItemTypeId = n.ItemTypeId, ItemTypeName = n.ItemType }).ToList();
                        List<LoaiSP> listsp = new List<LoaiSP>();
                        foreach (var s in sp)
                        {
                            LoaiSP lsp = new LoaiSP();
                            lsp.ItemTypeId = s.ItemTypeId;
                            lsp.ItemTypeName = s.ItemTypeName;
                            //var qcBB = itemSpec.DistinctBy(x => new { x.PackingId, x.Packing }).Where(x => x.BrandId == b.BrandId && x.IndustryId == i.IndustryId && x.ItemTypeId ==s.ItemTypeId).ToList();
                            var qcBB = itemSpec
                                .Where(x => x.BrandId == b.BrandId && x.IndustryId == i.IndustryId &&
                                            x.ItemTypeId == s.ItemTypeId).Select(x => new { x.PackingId, x.Packing })
                                .Distinct().Select(
                                    n => new QCBaobi { PackingId = n.PackingId, PackingName = n.Packing }).ToList();
                            List<QCBaobi> listpacking = new List<QCBaobi>();
                            foreach (var qc in qcBB)
                            {
                                QCBaobi bb = new QCBaobi();
                                bb.PackingId = qc.PackingId;
                                bb.PackingName = qc.PackingName;
                                listpacking.Add(bb);
                            }

                            lsp.Packing = listpacking;
                            listsp.Add(lsp);
                        }

                        nganh.ItemType = listsp;
                        listn.Add(nganh);
                    }

                    spec.Industry = listn;
                    listSpec.Add(spec);
                }

                return (listSpec, null);
            }
            catch (Exception ex)
            {
                mess.Status = 900;
                mess.Errors = ex.Message;
                return (null, mess);
            }
        }

        public async Task<(IEnumerable<Item>, Mess, int total)> GetAllItemsAsync(GridifyQuery gridifyQuery, ItemQuery queryP)
        {
            Mess mess = new Mess();
            try
            {
                var user = _httpContextAccessor.HttpContext?.User;
                var userId = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var pricelistEntity = _context.PriceLists
                 .Include(e => e.PriceListLine)
                 .Where(e => e.IsActive == true
                          && e.IsRetail == true
                          && e.EffectDate <= DateTime.UtcNow
                          && e.ExpriedDate >= DateTime.UtcNow)
                 .OrderByDescending(e => e.CreatedDate)
                 .FirstOrDefault();

                List<ItemPrice> pricelist = pricelistEntity?.PriceListLine
                    ?.Select(line => new ItemPrice
                    {
                        ItemCode = line.ItemCode,
                        Price = line.Price,
                        Currency = line.Currency
                    })
                    .ToList();
                var itemCode = pricelist?.Select(line => line.ItemCode).ToArray();
                List<string> itemstring = null;
                List<int> itemID = null;
                List<int> itemIgnore = null;
                string brand = null, industry = null;

                var query = _context.Set<Item>().AsQueryable();
                if (!queryP.search.IsNullOrEmpty())
                {
                    query = query.Where(e => e.ItemCode.Contains(queryP.search) || e.ItemName.Contains(queryP.search) ||
                                             e.Brand.Name.Contains(queryP.search)
                                             || e.ItemType.Name.Contains(queryP.search) ||
                                             e.Packing.Name.Contains(queryP.search) ||
                                             e.Industry.Name.Contains(queryP.search));
                }

                if (queryP.cardId != null)
                {
                    var ocrd = await _context.BP
                        .Where(x => x.Id == queryP.cardId)
                        .Include(x => x.Classify)
                        .ThenInclude(x => x.Brands)
                        .Include(x => x.Classify)
                        .ThenInclude(x => x.ItemType)
                        .Include(x => x.Classify)
                        .ThenInclude(x => x.Industry).Include(bp => bp.CRD2)
                        .AsNoTracking()
                        .FirstOrDefaultAsync();
                    if (ocrd != null)
                    {
                        var idusIds = ocrd.Classify.Select(x => x.Industry).Select(x => x.Id).ToList();
                        var braIds = ocrd.Classify.SelectMany(x => x.Brands).Select(x => x.Id).ToList();
                        var itemTypeid = ocrd.Classify.SelectMany(x => x.ItemType).Select(x => x.Id).ToList();
                        var result = ocrd.Classify
                        .Select(c => new
                        {
                            c.BpId,
                            c.IndustryId,
                            BrandId = c.Brands.Select(b => b.Id).ToList(),
                            ItemTypeId = c.ItemType.Select(i => i.Id).ToList()
                        }).ToList();
                        var itemIds = ocrd.CRD2.Select(p => p.TypeId).ToList();
                        List<int> totalQuery = new List<int>();
                        foreach (var r in result)
                        {
                            var tempQuery = _context.Item.Where(x => x.IndustryId == r.IndustryId && (r.ItemTypeId.Contains(x.ItemTypeId ?? 0) || r.ItemTypeId.Count == 0)  && (r.BrandId.Contains(x.BrandId ?? 0) || r.BrandId.Count ==0)).Select(e => e.Id).ToList();
                            if (tempQuery.Count > 0)
                                totalQuery.AddRange(tempQuery);
                        }
                        //var itms = _context.Item.Where((x => itemIds.Contains(x.Id) || itemIds.Count > 0) ).Select(e=>e.Id).ToList();
                        //var itmsIgn = _context.CRD2.Where(x => x.BPId != ocrd.Id).Select(e => e.TypeId).ToList();

                        var itmsIgn1 = _context.CRD2
                        .GroupBy(pc => pc.TypeId)
                        .Where(g => g.Count() == 1)
                        .Select(g => new {
                            TypeId = g.Key,
                            BPId = g.Select(x => x.BPId).FirstOrDefault()
                        })
                        .Where(x => x.BPId == ocrd.Id)
                        .Select(x => x.TypeId)
                        .ToList();
                        var itmsIgn =  _context.CRD2
                        .GroupBy(pc => pc.TypeId)
                        .Where(g => g.Count() == 1)
                        .Select(g => new {
                            TypeId = g.Key,
                            BPId = g.Select(x => x.BPId).FirstOrDefault()
                        })
                        .Where(x => x.BPId != ocrd.Id && (!itmsIgn1.Contains(x.TypeId)  || itmsIgn1.Count == 0))
                        .Select(x => x.TypeId)
                        .ToList();


                        if (itmsIgn1.Count > 0)
                            totalQuery.AddRange(itmsIgn1);
                        query = query.Where(x => !itmsIgn.Contains(x.Id) && (totalQuery.Contains(x.Id) || totalQuery.Count == 0));
                    }
                }

                if (!queryP.brand.IsNullOrEmpty())
                {
                    query = query.Where(e => queryP.brand.Contains("," + e.BrandId + ","));
                }

                if (brand != null)
                {
                    query = query.Where(e => brand.Contains("," + e.BrandId + ","));
                }

                if (industry != null)
                {
                    query = query.Where(e => industry.Contains("," + e.BrandId + ","));
                }

                if (itemIgnore != null)
                {
                    query = query.Where(e => !itemIgnore.Contains(e.Id));
                }

                if (!queryP.industry.IsNullOrEmpty())
                {
                    query = query.Where(e => queryP.industry.Contains("," + e.IndustryId + ","));
                }

                if (!queryP.itemType.IsNullOrEmpty())
                {
                    query = query.Where(e => queryP.itemType.Contains("," + e.ItemTypeId + ","));
                }

                if (!queryP.packing.IsNullOrEmpty())
                {
                    query = query.Where(e => queryP.packing.Contains("," + e.PackingId + ","));
                }
                if(userId != null)
                    query = query.Where(e => e.IsActive == true);
                else
                    if (itemCode.Count() > 0)

                        query = query.Where(e => e.IsActive== true && itemCode.Contains(e.ItemCode));
                    else
                        query = query.Where(e => e.IsActive == true);
                query = query.Include(p => p.ITM1)
                    .Include(p => p.Brand)
                    .Include(p => p.Industry)
                    .Include(p => p.Packing)
                    .Include(p => p.ItemType)
                    .Include(p => p.TaxGroups)
                    .Include(P => P.ProductApplications)
                    .Include(P => P.ProductGroup)
                    .Include(P => P.ProductQualityLevel)
                    .Include(P => P.OUGP)
                    .ThenInclude(P => P.UGP1)
                    .ThenInclude(UGP1 => UGP1.OUOM)
                    .Include(P => P.OUGP)
                    .ThenInclude(P => P.OUOM)
                     .AsQueryable();
                var filteredQuery = query.ApplyFiltering(gridifyQuery);
                var total = await filteredQuery.CountAsync();
                var sortedQuery = filteredQuery
                     .ApplyOrdering(gridifyQuery)
                     .ApplyPaging(gridifyQuery);
                
                var items = await sortedQuery.ToListAsync();
                if (userId == null)
                {
                    items.ForEach(e =>
                    {
                        var priceInfo = pricelist.FirstOrDefault(p => p.ItemCode == e.ItemCode);
                        if (priceInfo != null)
                        {
                            e.Price = priceInfo.Price;
                            e.Currency = priceInfo.Currency;
                        }
                    });
                } 
                return (items, null, total);
            }
            catch (Exception ex)

            {
                mess.Status = 900;
                mess.Errors = ex.Message;
                return (null, mess, 0);
            }
        }


        public async Task<(IEnumerable<Item>, Mess, int total)> GetAllItemsAsync(GridifyQuery gridifyQuery, ItemQuery queryP,
            int CardId)
        {
            Mess mess = new Mess();
            try
            {
                List<int> itemID = null;
                List<int> itemIgnore = null;
                string brand = null, industry = null;
                var ocrd = _context.BP.Where(e => e.Id == CardId).Include(e => e.CRD2).FirstOrDefault();
                if (ocrd == null)
                {
                    return (null, null, 0);
                }
                

                List<string> itemstring = null;
                if (ocrd.Brand != null)
                    brand = "," + ocrd.Brand + ",";
                if (ocrd.Industry != null)
                    industry = "," + ocrd.Industry + ",";
                if (ocrd.CRD2 != null)
                {
                    itemID = ocrd.CRD2.Select(e => e.TypeId).ToList();
                    itemIgnore = _context.CRD2.Where(e => e.BPId != queryP.cardId && !itemID.Contains(e.TypeId))
                        .Select(e => e.TypeId).ToList();
                }
                else
                {
                    itemIgnore = _context.CRD2.Select(e => e.TypeId).ToList();
                }

                if (!queryP.typeDoc.IsNullOrEmpty())
                {
                    var client = new HttpClient();
                    var request = new HttpRequestMessage(HttpMethod.Get,
                        _endpoints.Host + "/Goods?CardCode=" + ocrd.CardCode);
                    request.Headers.Add("accept", "*/*");
                    var response = await client.SendAsync(request);
                    if (response.IsSuccessStatusCode)
                    {
                        // Read the response content as a JSON string
                        var jsonString = await response.Content.ReadAsStringAsync();
                        if (jsonString != "null")
                        {
                            var pq = await _context.ODOC.AsNoTracking()
                                .Where(o => o.ObjType == 1250000001 && o.CardId == CardId && o.IsSync == false && o.Status != "HUY")
                                .Include(o => o.ItemDetail).ToListAsync();
                            var itms = pq.SelectMany(o => o.ItemDetail).ToList();
                
                            jsonString = jsonString.Replace("\"Goods\":", "");
                            int jsonLen = jsonString.Length - 2;
                            string jsonStringL = jsonString.Substring(1, jsonLen);
                            var goods = JsonConvert.DeserializeObject<List<Goods>>(jsonStringL);
                            itemstring = goods.Select(obj => obj.ItemCode).ToList();
                            var itemsm = _context.Item.Where(i => itemstring.Contains(i.ItemCode)).ToList();
                            foreach (var it in goods)
                            {
                                var ite = itemsm.FirstOrDefault(e => e.ItemCode == it.ItemCode);
                                if (ite != null)
                                {
                                    ite.OnHand = it.Quantity;
                                    ite.OnOrder = itms.Where(p => p.ItemCode == it.ItemCode).ToList().Sum(p => p.Quantity);
                                }
                            }
                
                            _context.Item.UpdateRange(itemsm);
                        }
                        else
                        {
                            return (null, null, 0);
                        }
                    }
                    else
                    {
                        mess.Status = (int)response.StatusCode;
                        mess.Errors = "Lỗi đồng bộ";
                        return (null, mess, 0);
                    }
                }

                var query = _context.Set<Item>().AsQueryable();
                var querys = _context.Set<Item>().AsQueryable();
                query = query.Where(e => e.IsActive == true);
                querys = querys.Where(e => e.IsActive == true);
                if (queryP.IsClient)
                {
                    query = query.Where(x => x.ItemTypeId != 16);
                    querys = querys.Where(x => x.ItemTypeId != 16);
                }
                if (!queryP.search.IsNullOrEmpty())
                {
                    query = query.Where(e => e.ItemCode.Contains(queryP.search) || e.ItemName.Contains(queryP.search) ||
                                             e.Brand.Name.Contains(queryP.search)
                                             || e.ItemType.Name.Contains(queryP.search) ||
                                             e.Packing.Name.Contains(queryP.search) ||
                                             e.Industry.Name.Contains(queryP.search));
                    querys = querys.Where(e => e.ItemCode.Contains(queryP.search) ||
                                               e.ItemName.Contains(queryP.search) ||
                                               e.Brand.Name.Contains(queryP.search)
                                               || e.ItemType.Name.Contains(queryP.search) ||
                                               e.Packing.Name.Contains(queryP.search) ||
                                               e.Industry.Name.Contains(queryP.search));
                }

                if (!queryP.brand.IsNullOrEmpty())
                {
                    query = query.Where(e => queryP.brand.Contains("," + e.BrandId + ","));
                    querys = querys.Where(e => queryP.brand.Contains("," + e.BrandId + ","));
                }

                if (!queryP.industry.IsNullOrEmpty())
                {
                    query = query.Where(e => queryP.industry.Contains("," + e.IndustryId + ","));
                    querys = querys.Where(e => queryP.industry.Contains("," + e.IndustryId + ","));
                }

                if (!queryP.itemType.IsNullOrEmpty())
                {
                    query = query.Where(e => queryP.itemType.Contains("," + e.ItemTypeId + ","));
                    querys = querys.Where(e => queryP.itemType.Contains("," + e.ItemTypeId + ","));
                }

                if (!queryP.packing.IsNullOrEmpty())
                {
                    query = query.Where(e => queryP.packing.Contains("," + e.PackingId + ","));
                    querys = querys.Where(e => queryP.packing.Contains("," + e.PackingId + ","));
                }

                if (brand != null)
                {
                    query = query.Where(e => brand.Contains("," + e.BrandId + ","));
                    querys = querys.Where(e => brand.Contains("," + e.BrandId + ","));
                }

                if (industry != null)
                {
                    query = query.Where(e => industry.Contains("," + e.BrandId + ","));
                    querys = querys.Where(e => industry.Contains("," + e.BrandId + ","));
                }

                if (itemIgnore != null)
                {
                    if(!queryP.typeDoc.IsNullOrEmpty())
                        if(!queryP.typeDoc.Equals("YCLHG"))
                        {
                            query = query.Where(e => !itemIgnore.Contains(e.Id));
                            querys = querys.Where(e => !itemIgnore.Contains(e.Id));
                        }
                }

                int totalCount = 0;
                List<Item> items = null;
                if (!queryP.typeDoc.IsNullOrEmpty())
                {
                    if (itemstring.Count > 0)
                    {
                          query = query.Where(i => itemstring.Contains(i.ItemCode))
                           .Include(p => p.ITM1)
                            .Include(p => p.Brand)
                            .Include(p => p.Industry)
                            .Include(p => p.Packing)
                            .Include(P => P.ProductApplications)
                            .Include(P => P.ProductGroup)
                            .Include(P => P.ProductQualityLevel)
                            .Include(p => p.ItemType)
                            .Include(p => p.TaxGroups)
                            .Include(P => P.OUGP)
                            .ThenInclude(P => P.UGP1)
                            .ThenInclude(UGP1 => UGP1.OUOM)
                            .Include(P => P.OUGP)
                            .ThenInclude(P => P.OUOM)
                         .AsQueryable();
                            var filteredQuery = query.ApplyFiltering(gridifyQuery);
                            var total = await filteredQuery.CountAsync();
                            var sortedQuery = filteredQuery
                                 .ApplyOrdering(gridifyQuery)
                                 .ApplyPaging(gridifyQuery);

                            items = await sortedQuery.ToListAsync();
                            return (items, null, total);
                    }
                    else
                    {
                        return (null, null, 0);
                    }
                }
                else
                {
                    query = query
                          .Include(p => p.ITM1)
                        .Include(p => p.Brand)
                        .Include(p => p.Industry)
                        .Include(p => p.Packing)
                        .Include(p => p.ItemType)
                        .Include(p => p.TaxGroups)
                        .Include(P => P.OUGP)
                        .ThenInclude(P => P.UGP1)
                        .ThenInclude(UGP1 => UGP1.OUOM)
                        .Include(P => P.OUGP)
                        .ThenInclude(P => P.OUOM)
                        .AsQueryable();
                    var filteredQuery = query.ApplyFiltering(gridifyQuery);
                    var total = await filteredQuery.CountAsync();
                    var sortedQuery = filteredQuery
                         .ApplyOrdering(gridifyQuery)
                         .ApplyPaging(gridifyQuery);

                    items = await sortedQuery.ToListAsync();
                    return (items, null, total);
                }
            }
            catch (Exception ex)

            {
                mess.Status = 900;
                mess.Errors = ex.Message;
                return (null, mess, 0);
            }
        }

        public async Task<(IEnumerable<Item>, Mess, int total)> GetAllItemsIgAsync(GridifyQuery gridifyQuery, ItemQuery queryP, int CardId)
        {
            Mess mess = new Mess();
            try
            {
                List<int> itemID = null;
                List<int> itemIgnore = null;
                string brand = null, industry = null;
                var ocrd = _context.BP.Where(e => e.Id == CardId).Include(e => e.CRD2).FirstOrDefault();
                if (ocrd == null)
                {
                    return (null, null, 0);
                }


                List<string> itemstring = null;
                if (ocrd.Brand != null)
                    brand = "," + ocrd.Brand + ",";
                if (ocrd.Industry != null)
                    industry = "," + ocrd.Industry + ",";
                //if (ocrd.CRD2 != null)
                //{
                //    itemID = ocrd.CRD2.Select(e => e.TypeId).ToList();
                //    itemIgnore = _context.CRD2.Where(e => e.BPId != queryP.cardId && !itemID.Contains(e.TypeId))
                //        .Select(e => e.TypeId).ToList();
                //}
                //else
                //{
                //    itemIgnore = _context.CRD2.Select(e => e.TypeId).ToList();
                //}

                if (!queryP.typeDoc.IsNullOrEmpty())
                {
                    var client = new HttpClient();
                    var request = new HttpRequestMessage(HttpMethod.Get,
                        _endpoints.Host + "/Goods?CardCode=" + ocrd.CardCode);
                    request.Headers.Add("accept", "*/*");
                    var response = await client.SendAsync(request);
                    if (response.IsSuccessStatusCode)
                    {
                        // Read the response content as a JSON string
                        var jsonString = await response.Content.ReadAsStringAsync();
                        if (jsonString != "null")
                        {
                            var pq = await _context.ODOC.AsNoTracking()
                                .Where(o => o.ObjType == 1250000001 && o.CardId == CardId && o.IsSync == false)
                                .Include(o => o.ItemDetail).ToListAsync();
                            var itms = pq.SelectMany(o => o.ItemDetail).ToList();

                            jsonString = jsonString.Replace("\"Goods\":", "");
                            int jsonLen = jsonString.Length - 2;
                            string jsonStringL = jsonString.Substring(1, jsonLen);
                            var goods = JsonConvert.DeserializeObject<List<Goods>>(jsonStringL);
                            itemstring = goods.Select(obj => obj.ItemCode).ToList();
                            var itemsm = _context.Item.Where(i => itemstring.Contains(i.ItemCode)).ToList();
                            foreach (var it in goods)
                            {
                                var ite = itemsm.FirstOrDefault(e => e.ItemCode == it.ItemCode);
                                if (ite != null)
                                {
                                    ite.OnHand = it.Quantity;
                                    ite.OnOrder = itms.Where(p => p.ItemCode == it.ItemCode).ToList().Sum(p => p.Quantity);
                                }
                            }

                            _context.Item.UpdateRange(itemsm);
                        }
                        else
                        {
                            return (null, null, 0);
                        }
                    }
                    else
                    {
                        mess.Status = (int)response.StatusCode;
                        mess.Errors = "Lỗi đồng bộ";
                        return (null, mess, 0);
                    }
                }

                var query = _context.Set<Item>().AsQueryable();
                var querys = _context.Set<Item>().AsQueryable();
                query = query.Where(e => e.IsActive == true);
                querys = querys.Where(e => e.IsActive == true);
                if (queryP.IsClient)
                {
                    query = query.Where(x => x.ItemTypeId != 16);
                    querys = querys.Where(x => x.ItemTypeId != 16);
                }
                if (!queryP.search.IsNullOrEmpty())
                {
                    query = query.Where(e => e.ItemCode.Contains(queryP.search) || e.ItemName.Contains(queryP.search) ||
                                             e.Brand.Name.Contains(queryP.search)
                                             || e.ItemType.Name.Contains(queryP.search) ||
                                             e.Packing.Name.Contains(queryP.search) ||
                                             e.Industry.Name.Contains(queryP.search));
                    querys = querys.Where(e => e.ItemCode.Contains(queryP.search) ||
                                               e.ItemName.Contains(queryP.search) ||
                                               e.Brand.Name.Contains(queryP.search)
                                               || e.ItemType.Name.Contains(queryP.search) ||
                                               e.Packing.Name.Contains(queryP.search) ||
                                               e.Industry.Name.Contains(queryP.search));
                }

                if (!queryP.brand.IsNullOrEmpty())
                {
                    query = query.Where(e => queryP.brand.Contains("," + e.BrandId + ","));
                    querys = querys.Where(e => queryP.brand.Contains("," + e.BrandId + ","));
                }

                if (!queryP.industry.IsNullOrEmpty())
                {
                    query = query.Where(e => queryP.industry.Contains("," + e.IndustryId + ","));
                    querys = querys.Where(e => queryP.industry.Contains("," + e.IndustryId + ","));
                }

                if (!queryP.itemType.IsNullOrEmpty())
                {
                    query = query.Where(e => queryP.itemType.Contains("," + e.ItemTypeId + ","));
                    querys = querys.Where(e => queryP.itemType.Contains("," + e.ItemTypeId + ","));
                }

                if (!queryP.packing.IsNullOrEmpty())
                {
                    query = query.Where(e => queryP.packing.Contains("," + e.PackingId + ","));
                    querys = querys.Where(e => queryP.packing.Contains("," + e.PackingId + ","));
                }

                if (brand != null)
                {
                    query = query.Where(e => brand.Contains("," + e.BrandId + ","));
                    querys = querys.Where(e => brand.Contains("," + e.BrandId + ","));
                }

                if (industry != null)
                {
                    query = query.Where(e => industry.Contains("," + e.BrandId + ","));
                    querys = querys.Where(e => industry.Contains("," + e.BrandId + ","));
                }

                //if (itemIgnore != null)
                //{
                //    query = query.Where(e => !itemIgnore.Contains(e.Id));
                //    querys = querys.Where(e => !itemIgnore.Contains(e.Id));
                //}

                int totalCount = 0;
                List<Item> items = null;
                if (!queryP.typeDoc.IsNullOrEmpty())
                {
                    if (itemstring.Count > 0)
                    {
                        query = query.Where(i => itemstring.Contains(i.ItemCode))
                         .Include(p => p.ITM1)
                          .Include(p => p.Brand)
                          .Include(p => p.Industry)
                          .Include(p => p.Packing)
                          .Include(P => P.ProductApplications)
                          .Include(P => P.ProductGroup)
                          .Include(P => P.ProductQualityLevel)
                          .Include(p => p.ItemType)
                          .Include(p => p.TaxGroups)
                          .Include(P => P.OUGP)
                          .ThenInclude(P => P.UGP1)
                          .ThenInclude(UGP1 => UGP1.OUOM)
                          .Include(P => P.OUGP)
                          .ThenInclude(P => P.OUOM)
                       .AsQueryable();
                        var filteredQuery = query.ApplyFiltering(gridifyQuery);
                        var total = await filteredQuery.CountAsync();
                        var sortedQuery = filteredQuery
                             .ApplyOrdering(gridifyQuery)
                             .ApplyPaging(gridifyQuery);

                        items = await sortedQuery.ToListAsync();
                        return (items, null, total);
                    }
                    else
                    {
                        return (null, null, 0);
                    }
                }
                else
                {
                    query = query
                          .Include(p => p.ITM1)
                        .Include(p => p.Brand)
                        .Include(p => p.Industry)
                        .Include(p => p.Packing)
                        .Include(p => p.ItemType)
                        .Include(p => p.TaxGroups)
                        .Include(P => P.OUGP)
                        .ThenInclude(P => P.UGP1)
                        .ThenInclude(UGP1 => UGP1.OUOM)
                        .Include(P => P.OUGP)
                        .ThenInclude(P => P.OUOM)
                        .AsQueryable();
                    var filteredQuery = query.ApplyFiltering(gridifyQuery);
                    var total = await filteredQuery.CountAsync();
                    var sortedQuery = filteredQuery
                         .ApplyOrdering(gridifyQuery)
                         .ApplyPaging(gridifyQuery);

                    items = await sortedQuery.ToListAsync();
                    return (items, null, total);
                }
            }
            catch (Exception ex)

            {
                mess.Status = 900;
                mess.Errors = ex.Message;
                return (null, mess, 0);
            }
        }

        public async Task<(IEnumerable<ItemPromotionView>, Mess, int total)> GetItemPromotion(GridifyQuery gridifyQuery, int? CardId, string? search)
        {
            Mess mess = new Mess();
            try
            {
                List<string> itemstring = null;
                List<int> itemID = null;
                List<int> itemIgnore = null;
                string brand = null, industry = null;
                var user = _httpContextAccessor.HttpContext?.User;
                var userId = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (CardId == null)
                    CardId = _context.Users.FirstOrDefault(e => e.Id == int.Parse(userId))?.CardId;
                var query = _context.Set<Item>().AsQueryable();
                if (!search.IsNullOrEmpty())
                {
                    query = query.Where(e => e.ItemCode.Contains(search) || e.ItemName.Contains(search));
                }
                List<ItemPoints> points = null;
                if (CardId != null)
                {
                    var ocrd = await _context.BP.AsNoTracking()
                        .Where(x => x.Id == CardId)
                        .Include(e => e.Groups)
                        .Include(x => x.Classify)
                        .ThenInclude(x => x.Brands)
                        .Include(x => x.Classify)
                        .ThenInclude(x => x.Industry).Include(bp => bp.CRD2)
                        .AsSplitQuery()
                        .AsNoTracking()
                        .FirstOrDefaultAsync();
                    points = _context.ExchangePoint
                   .Include(e => e.ExchangePointLine)
                   .Where(e => e.IsActive == true && e.IsAllCustomer == false && e.PointCustomer.Any(pc => pc.CustomerId == ocrd.Id && pc.Type == "C") && e.StartDate.Date <= DateTime.Now.Date && e.EndDate.Date >= DateTime.Now.Date)
                   .SelectMany(e => e.ExchangePointLine.Select(l => new ItemPoints { ItemCode = l.ItemCode, Points = l.Point }))
                   .GroupBy(x => x.ItemCode).Select(g => new ItemPoints { ItemCode = g.Key, Points = g.Max(x => x.Points) }).ToList();
                    if (points.Count == 0)
                    {

                        if (ocrd.Groups.Count > 0)
                        {
                            var ocrg = ocrd.Groups.Select(c => c.Id).ToArray();
                            points = _context.ExchangePoint
                           .Include(e => e.ExchangePointLine)
                           .AsSplitQuery()
                           .Where(e => e.IsActive == true && e.IsAllCustomer == false && e.PointCustomer.Any(pc => ocrg.Contains(pc.CustomerId) && pc.Type == "G") && e.StartDate.Date <= DateTime.UtcNow.Date && e.EndDate.Date >= DateTime.UtcNow.Date)
                           .SelectMany(e => e.ExchangePointLine.Select(l => new ItemPoints { ItemCode = l.ItemCode, Points = l.Point }))
                           .GroupBy(x => x.ItemCode).Select(g => new ItemPoints { ItemCode = g.Key, Points = g.Max(x => x.Points) }).ToList();
                        }
                    }
                    if (points.Count == 0)
                    {
                        points = _context.ExchangePoint
                           .Include(e => e.ExchangePointLine)
                           .AsSplitQuery()
                           .Where(e => e.IsActive == true && e.IsAllCustomer == true && e.StartDate.Date <= DateTime.UtcNow.Date && e.EndDate.Date >= DateTime.UtcNow.Date)
                           .SelectMany(e => e.ExchangePointLine.Select(l => new ItemPoints { ItemCode = l.ItemCode, Points = l.Point }))
                           .GroupBy(x => x.ItemCode).Select(g => new ItemPoints { ItemCode = g.Key, Points = g.Max(x => x.Points) }).ToList();
                    }
                    var ItemCode = points?.Where(e => !string.IsNullOrEmpty(e.ItemCode)).Select(e => e.ItemCode).Distinct().ToArray() ?? Array.Empty<string>();
                    if (ItemCode.Count() > 0)
                        query = query.Where(e => e.IsActive == true && ItemCode.Contains(e.ItemCode));
                    else
                        query = query.Where(e => e.IsActive == true && e.ItemCode == "1");

                    query = query.Include(p => p.ITM1)
                        .Include(p => p.Packing)
                        //.Where(e => e.ItemType.Code == "VPKM")
                        .AsQueryable();
                    var filteredQuery = query.ApplyFiltering(gridifyQuery);
                    var total = await filteredQuery.CountAsync();
                    var sortedQuery = filteredQuery
                        .ApplyOrdering(gridifyQuery)
                         .ApplyPaging(gridifyQuery);

                    var items = await sortedQuery.ToListAsync();
                    if (points != null)
                    {
                        var pointDict = points.ToDictionary(p => p.ItemCode, p => p.Points);

                        items.ForEach(item =>
                        {
                            if (pointDict.TryGetValue(item.ItemCode, out var point))
                                item.ExchangePoint = point;
                        });
                    }
                    var itemview = items.Select(e => new ItemPromotionView
                    {
                        Id = e.Id, 
                        ItemCode = e.ItemCode,
                        ItemName = e.ItemName,
                        ExchangePoint = e.ExchangePoint,
                        PackingId = e.PackingId,
                        Packing = new PackingPromotion
                        {
                            Code = e.Packing.Code,
                            Name = e.Packing.Name
                        },
                        ITM1 = e.ITM1.Select(img => new ItemPromotionImage
                        {
                            FileName = img.FileName,
                            FilePath = img.FilePath,
                            Note = img.Note
                        }).ToList()
                    }).ToList();
                    return (itemview, null, total);
                }else
                {
                    points = points = _context.ExchangePoint
                           .Include(e => e.ExchangePointLine)
                           .AsSplitQuery()
                           .Where(e => e.IsActive == true && e.IsAllCustomer == true && e.StartDate.Date <= DateTime.UtcNow.Date && e.EndDate.Date >= DateTime.UtcNow.Date)
                           .SelectMany(e => e.ExchangePointLine.Select(l => new ItemPoints { ItemCode = l.ItemCode, Points = l.Point }))
                           .GroupBy(x => x.ItemCode).Select(g => new ItemPoints { ItemCode = g.Key, Points = g.Max(x => x.Points) }).ToList();

                    var ItemCode = points?.Where(e => !string.IsNullOrEmpty(e.ItemCode)).Select(e => e.ItemCode).Distinct().ToArray() ?? Array.Empty<string>();
                    if (ItemCode.Count() > 0)
                        query = query.Where(e => e.IsActive == true && ItemCode.Contains(e.ItemCode));
                    else
                        query = query.Where(e => e.IsActive == true);

                    query = query.Include(p => p.ITM1)
                        .Include(p => p.Packing)
                        .Where(e => e.ItemType.Code == "VPKM")
                        .AsQueryable();
                    var filteredQuery = query.ApplyFiltering(gridifyQuery);
                    var total = await filteredQuery.CountAsync();
                    var sortedQuery = filteredQuery
                        .ApplyOrdering(gridifyQuery)
                         .ApplyPaging(gridifyQuery);

                    var items = await sortedQuery.ToListAsync();
                    if (points != null)
                    {
                        var pointDict = points.ToDictionary(p => p.ItemCode, p => p.Points);

                        items.ForEach(item =>
                        {
                            if (pointDict.TryGetValue(item.ItemCode, out var point))
                                item.ExchangePoint = point;
                        });
                    }
                    var itemview = items.Select(e => new ItemPromotionView
                    {
                        Id = e.Id,
                        ItemCode = e.ItemCode,
                        ItemName = e.ItemName,
                        ExchangePoint = e.ExchangePoint,
                        PackingId = e.PackingId,
                        Packing = new PackingPromotion
                        {
                            Code = e.Packing.Code,
                            Name = e.Packing.Name
                        },
                        ITM1 = e.ITM1.Select(img => new ItemPromotionImage
                        {
                            FileName = img.FileName,
                            FilePath = img.FilePath,
                            Note = img.Note
                        }).ToList()
                    }).ToList();
                    return (itemview, null, total);
                }    
                return (null, null, 0);
            }
            catch (Exception ex)

            {
                mess.Status = 900;
                mess.Errors = ex.Message;
                return (null, mess, 0);
            }
        }
        public async Task<(IEnumerable<ItemPromotionView>, Mess, int total)> GetItemPromotions(GridifyQuery gridifyQuery, int? CardId, string? search)
        {
            Mess mess = new Mess();
            try
            {
                List<string> itemstring = null;
                List<int> itemID = null;
                List<int> itemIgnore = null;
                string brand = null, industry = null;
                var user = _httpContextAccessor.HttpContext?.User;
                var userId = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (CardId == null)
                    CardId = _context.Users.FirstOrDefault(e => e.Id == int.Parse(userId))?.CardId;
                var query = _context.Set<Item>().AsQueryable();
                if (!search.IsNullOrEmpty())
                {
                    query = query.Where(e => e.ItemCode.Contains(search) || e.ItemName.Contains(search));
                }
                List<ItemPoints> points = null;
                if (CardId != null)
                {
                    query = query.Include(p => p.ITM1)
                        .Include(p => p.Packing)
                        .Where(e => e.ItemType.Code == "VPKM" && e.IsActive == true)
                        .AsQueryable();
                    var filteredQuery = query.ApplyFiltering(gridifyQuery);
                    var total = await filteredQuery.CountAsync();
                    var sortedQuery = filteredQuery
                        .ApplyOrdering(gridifyQuery)
                         .ApplyPaging(gridifyQuery);

                    var items = await sortedQuery.ToListAsync();
                    if (points != null)
                    {
                        var pointDict = points.ToDictionary(p => p.ItemCode, p => p.Points);

                        items.ForEach(item =>
                        {
                            if (pointDict.TryGetValue(item.ItemCode, out var point))
                                item.ExchangePoint = point;
                        });
                    }
                    var itemview = items.Select(e => new ItemPromotionView
                    {
                        Id = e.Id,
                        ItemCode = e.ItemCode,
                        ItemName = e.ItemName,
                        ExchangePoint = e.ExchangePoint,
                        PackingId = e.PackingId,
                        Packing = new PackingPromotion
                        {
                            Code = e.Packing.Code,
                            Name = e.Packing.Name
                        },
                        ITM1 = e.ITM1.Select(img => new ItemPromotionImage
                        {
                            FileName = img.FileName,
                            FilePath = img.FilePath,
                            Note = img.Note
                        }).ToList()
                    }).ToList();
                    return (itemview, null, total);
                }
                else
                {
                    points = points = _context.ExchangePoint
                           .Include(e => e.ExchangePointLine)
                           .AsSplitQuery()
                           .Where(e => e.IsActive == true && e.IsAllCustomer == true && e.StartDate.Date <= DateTime.UtcNow.Date && e.EndDate.Date >= DateTime.UtcNow.Date)
                           .SelectMany(e => e.ExchangePointLine.Select(l => new ItemPoints { ItemCode = l.ItemCode, Points = l.Point }))
                           .GroupBy(x => x.ItemCode).Select(g => new ItemPoints { ItemCode = g.Key, Points = g.Max(x => x.Points) }).ToList();

                    var ItemCode = points?.Where(e => !string.IsNullOrEmpty(e.ItemCode)).Select(e => e.ItemCode).Distinct().ToArray() ?? Array.Empty<string>();
                    if (ItemCode.Count() > 0)
                        query = query.Where(e => e.IsActive == true && ItemCode.Contains(e.ItemCode));
                    else
                        query = query.Where(e => e.IsActive == true);

                    query = query.Include(p => p.ITM1)
                        .Include(p => p.Packing)
                        .Where(e => e.ItemType.Code == "VPKM")
                        .AsQueryable();
                    var filteredQuery = query.ApplyFiltering(gridifyQuery);
                    var total = await filteredQuery.CountAsync();
                    var sortedQuery = filteredQuery
                        .ApplyOrdering(gridifyQuery)
                         .ApplyPaging(gridifyQuery);

                    var items = await sortedQuery.ToListAsync();
                    if (points != null)
                    {
                        var pointDict = points.ToDictionary(p => p.ItemCode, p => p.Points);

                        items.ForEach(item =>
                        {
                            if (pointDict.TryGetValue(item.ItemCode, out var point))
                                item.ExchangePoint = point;
                        });
                    }
                    var itemview = items.Select(e => new ItemPromotionView
                    {
                        Id = e.Id,
                        ItemCode = e.ItemCode,
                        ItemName = e.ItemName,
                        ExchangePoint = e.ExchangePoint,
                        PackingId = e.PackingId,
                        Packing = new PackingPromotion
                        {
                            Code = e.Packing.Code,
                            Name = e.Packing.Name
                        },
                        ITM1 = e.ITM1.Select(img => new ItemPromotionImage
                        {
                            FileName = img.FileName,
                            FilePath = img.FilePath,
                            Note = img.Note
                        }).ToList()
                    }).ToList();
                    return (itemview, null, total);
                }
                return (null, null, 0);
            }
            catch (Exception ex)

            {
                mess.Status = 900;
                mess.Errors = ex.Message;
                return (null, mess, 0);
            }
        }
        public async Task<(IEnumerable<ItemImport>, Mess)> GetImportPricelist(List<ItemImportView> view)
        {
            Mess mess = new Mess();
            try
            {
                var items = _context.Item.Include(e => e.Packing).Include(e=>e.ITM1)
                    .Where(e => view.Select(e => e.ItemCode).ToArray().Contains(e.ItemCode))
                    .Select(e => new ItemImport
                    {
                        ItemCode = e.ItemCode,
                        ItemName = e.ItemName,
                        PackageId = e.Packing.Id,
                        PackingName = e.Packing.Name,
                        Images = e.ITM1.FirstOrDefault().FilePath

                    })
                    .ToList();
                string keyword = "/uploads/";
                int index = 0;
                foreach (var item in items)
                {
                    item.Price = view.FirstOrDefault(e => e.ItemCode == item.ItemCode)?.Price ?? 0;
                    item.Currency = view.FirstOrDefault(e => e.ItemCode == item.ItemCode)?.Currency ?? "";
                    index = item.Images.IndexOf(keyword);

                    item.Images = item.Images.Substring(index);
                }
                return (items, null);
            }
            catch(Exception ex)
            {
                mess.Status = 900;
                mess.Errors = ex.Message;
                return (null, mess);
            }
        }
    }
}