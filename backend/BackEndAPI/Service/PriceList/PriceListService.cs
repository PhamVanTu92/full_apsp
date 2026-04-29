using BackEndAPI.Data;
using BackEndAPI.Models.BPGroups;
using BackEndAPI.Models.Other;
using BackEndAPI.Models.PriceList;
using BackEndAPI.Service.PriceLists;
using Gridify;
using Microsoft.EntityFrameworkCore;


namespace BackEndAPI.Service.PriceLists
{
    public class PriceListService : IPriceListService
    {
        private readonly AppDbContext _context;

        public PriceListService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<(IEnumerable<PriceListDTO>, Mess,int)> GetAllAsync(GridifyQuery gridifyQuery)
        {
            Mess mess = new Mess();
            try
            {
                var query = _context.PriceLists.Include(p => p.PriceListLine).AsQueryable();
                var filteredQuery = query.ApplyFiltering(gridifyQuery);
                var total = await filteredQuery.CountAsync();
                var sortedQuery = filteredQuery
                     .ApplyOrdering(gridifyQuery)
                     .ApplyPaging(gridifyQuery);

                var items = await sortedQuery.Select(e => new PriceListDTO
                {
                    Id = e.Id,
                    PriceListName = e.PriceListName,
                    IsAllCustomer = e.IsAllCustomer,
                    IsRetail = e.IsRetail,
                    CustomerId = e.CustomerId,
                    CustomerName = e.Customer != null ? e.Customer.CardName : null,
                    CustomerGroupId = e.CustomerGroupId,
                    CustomerGroupName = e.CustomerGroup != null ? e.CustomerGroup.GroupName : null,
                    CreatedDate = e.CreatedDate,
                    EffectDate = e.EffectDate,
                    ExpriedDate = e.ExpriedDate,
                    IsActive = e.IsActive,
                    PriceListLine = e.PriceListLine
                 }).ToListAsync();
                return (items, null, total);
            }
            catch (Exception ex)
            {
                mess.Status = 900;
                mess.Errors = ex.Message;
                return (null, mess,0);
            }
        }

        public async Task<(PriceListDTO, Mess)> GetByIdAsync(int id)
        {
            Mess mess = new Mess();
            try
            {
                var items = await _context.PriceLists.Include(e=>e.CustomerGroup)
                .Include(p => p.Customer)
                .Include(p => p.PriceListLine)
                .Where(p => p.Id == id)
                .Select(e => new PriceListDTO
                {
                    Id = e.Id,
                    PriceListName = e.PriceListName,
                    IsAllCustomer = e.IsAllCustomer,
                    IsRetail = e.IsRetail,
                    CustomerId = e.CustomerId,
                    CustomerName = e.Customer != null ? e.Customer.CardName : null,
                    CustomerGroupId = e.CustomerGroupId,
                    CustomerGroupName = e.CustomerGroup != null ? e.CustomerGroup.GroupName : null,
                    CreatedDate = e.CreatedDate,
                    EffectDate = e.EffectDate,
                    ExpriedDate = e.ExpriedDate,
                    IsActive = e.IsActive,
                    PriceListLine = e.PriceListLine
                })
                .FirstOrDefaultAsync();
                return (items, null);
            }
            catch (Exception ex)
            {
                mess.Status = 900;
                mess.Errors = ex.Message;
                return (null, mess);
            }
        }

        public async Task<(PriceListDTO, Mess)> CreateAsync(PriceList priceList)
        {
            Mess mess = new Mess();
            try
            {
                if (priceList.CustomerGroupId != null)
                {
                    if (priceList.CustomerGroupId == 0)
                    {
                        priceList.CustomerGroupId = null;
                    }
                    else
                    {
                        var ocrg = _context.OCRG.FirstOrDefault(e => e.Id == priceList.CustomerGroupId);
                        if (ocrg == null)
                        {
                            mess.Status = 400;
                            mess.Errors = "ID Nhóm khách hàng " + priceList.CustomerGroupId + " không tồn tại";
                            return (null, mess);
                        }

                    }
                }
                if (priceList.CustomerId != null)
                {
                    if (priceList.CustomerId == 0)
                    {
                        priceList.CustomerId = null;
                    }
                    else
                    {
                        var ocrd = _context.BP.FirstOrDefault(e => e.Id == priceList.CustomerId);
                        if (ocrd == null)
                        {
                            mess.Status = 400;
                            mess.Errors = "ID Khách hàng " + priceList.CustomerId + " không tồn tại";
                            return (null, mess);
                        }
                    }
                }
                var Counts = priceList.PriceListLine?.Count();
                if(Counts <= 0)
                {
                    mess.Status = 400;
                    mess.Errors = "Không có bản ghi nào";
                    return (null, mess);
                }
                var listItemCodes = priceList.PriceListLine?.Select(e => e.ItemCode);
                var dbItemCodes = _context.Item.Select(i => i.ItemCode).ToList();

                var notExistItemCodes = listItemCodes
                    .Except(dbItemCodes)
                    .ToList();
                if(notExistItemCodes.Count > 0)
                {
                    mess.Status = 400;
                    mess.Errors = "Các mã hàng: "+ string.Join(",", notExistItemCodes) + "không tồn tại";
                    return (null, mess);
                }    
                _context.PriceLists.Add(priceList);
                await _context.SaveChangesAsync();
                var (priceLists,mes) = await GetByIdAsync(priceList.Id);
                return (priceLists, null);
            }
            catch (Exception ex)
            {
                mess.Status = 900;
                mess.Errors = ex.Message;
                return (null, mess);
            }
        }

        public async Task<(PriceListDTO,Mess)> UpdateAsync(int id, PriceList priceList)
        {
            Mess mess = new Mess();
            try { 
                var existing = await _context.PriceLists
                    .Include(p => p.PriceListLine)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (existing == null)
                {
                    mess.Status = 400;
                    mess.Errors = "Không có bản ghi để cập nhập";
                    return (null, mess);
                }
                if(priceList.CustomerGroupId != null)
                {
                    if (priceList.CustomerGroupId == 0)
                    {
                        priceList.CustomerGroupId = null;
                    }
                    else
                    {
                        var ocrg = _context.OCRG.FirstOrDefault(e => e.Id == priceList.CustomerGroupId);
                        if (ocrg == null)
                        {
                            mess.Status = 400;
                            mess.Errors = "ID Nhóm khách hàng " + priceList.CustomerGroupId + " không tồn tại";
                            return (null, mess);
                        }

                    }
                }    
                if(priceList.CustomerId != null)
                {
                    if (priceList.CustomerId == 0)
                    {
                        priceList.CustomerId = null;
                    }
                    else
                    {
                        var ocrd = _context.BP.FirstOrDefault(e => e.Id == priceList.CustomerId);
                        if (ocrd == null)
                        {
                            mess.Status = 400;
                            mess.Errors = "ID Khách hàng " + priceList.CustomerId + " không tồn tại";
                            return (null, mess);
                        }
                    }
                }    
                
                var Counts = priceList.PriceListLine?.Count();
                if (Counts <= 0)
                {
                    mess.Status = 400;
                    mess.Errors = "Không có bản ghi nào";
                    return (null, mess);
                }
                var listItemCodes = priceList.PriceListLine?.Select(e => e.ItemCode);
                var dbItemCodes = _context.Item.Select(i => i.ItemCode).ToList();

                var notExistItemCodes = listItemCodes
                    .Except(dbItemCodes)
                    .ToList();
                if (notExistItemCodes.Count > 0)
                {
                    mess.Status = 400;
                    mess.Errors = "Các mã hàng: " + string.Join(",", notExistItemCodes) + "không tồn tại";
                    return (null, mess);
                }
                // Update master
                existing.PriceListName = priceList.PriceListName;
                existing.IsAllCustomer = priceList.IsAllCustomer;
                existing.IsRetail = priceList.IsRetail;
                existing.CustomerId = priceList.CustomerId;
                existing.CustomerGroupId = priceList.CustomerGroupId;
                existing.EffectDate = priceList.EffectDate;
                existing.ExpriedDate = priceList.ExpriedDate;
                existing.IsActive = priceList.IsActive;

                // Update lines (replace all for simplicity)
                _context.PriceListLines.RemoveRange(existing.PriceListLine ?? new List<PriceListLine>());
                existing.PriceListLine = priceList.PriceListLine;

                await _context.SaveChangesAsync();
                var (priceLists, mes) = await GetByIdAsync(priceList.Id);
                return (priceLists, null);
                return (priceLists, null);
            }
            catch (Exception ex)
            {
                mess.Status = 900;
                mess.Errors = ex.Message;
                return (null, mess);
            }
        }

        public async Task<(bool,Mess)> DeleteAsync(int id)
        {
            Mess mess = new Mess();
            try
            {
                var existing = await _context.PriceLists
                .Include(p => p.PriceListLine)
                .FirstOrDefaultAsync(p => p.Id == id);

                if (existing == null)
                {
                    mess.Status = 400;
                    mess.Errors = "Không có bản ghi để xóa";
                    return (false, mess);
                }    

                _context.PriceListLines.RemoveRange(existing.PriceListLine ?? new List<PriceListLine>());
                _context.PriceLists.Remove(existing);
                await _context.SaveChangesAsync();
                return (true,null);
            }catch (Exception ex) 
            {
                mess.Status = 900;
                mess.Errors = ex.Message;
                return (false, mess);
            }
        }
    }
}
