using AutoMapper;
using BackEndAPI.Data;
using BackEndAPI.Models.NotificationModels;
using BackEndAPI.Models.Other;
using BackEndAPI.Service.EventAggregator;
using Gridify;
using MailKit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Org.BouncyCastle.Tls.Crypto;

namespace BackEndAPI.Service.SaleForecast;

public class SaleForecast(AppDbContext context, IMapper mapper, IEventAggregator eventAggregator)
    : ISaleForecast
{
    private readonly AppDbContext _context = context;
    private readonly IEventAggregator _eventAggregator = eventAggregator;
    private readonly IMapper _mapper = mapper;


    public async Task<(Models.SaleForecastModel.SaleForecast?, Mess?)> CreateSaleForecast(
        Models.SaleForecastModel.SaleForecast saleForecast)
    {
        var mess = new Mess();
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var ids = saleForecast.SaleForecastItems.Select(p => p.ItemId).ToList();
            var checkUniqueItem = await _context.SaleForecasts.AsNoTracking()
                .Include(p => p.SaleForecastItems.Where(p => ids.Contains(p.Id)))
                .Where(p => p.CustomerId == saleForecast.CustomerId)
                .Where(s => s.StartDate >= saleForecast.StartDate && s.EndDate <= saleForecast.EndDate).CountAsync();
            if (checkUniqueItem != 0)
            {
                mess.Errors = "Trong khoảng thời gian này đã tồn tại sản phẩm";
                mess.Status = 400;
                await transaction.RollbackAsync();
                return (null, mess);
            }
            var maxCode = _context.SaleForecasts
                .Where(c => c.PlanCode != null && c.PlanCode.StartsWith("KHNH"))
                .OrderByDescending(c => c.PlanCode)
                .Select(c => c.PlanCode)
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

            saleForecast.PlanCode = $"KHNH{newNumber:00000}";

            _context.SaleForecasts.Add(saleForecast);
            var systemUser = await _context.AppUser.AsNoTracking().Where(p => p.UserType == "APSP")
                .Select(p => p.Id).ToListAsync();
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            var obj = new Models.NotificationModels.NotificationObject
            {
                ObjId = saleForecast.Id,
                ObjType = "sale_forecast",
                ObjName = "Sale Forecast",
                
            };
            _eventAggregator.Publish(new Models.NotificationModels.Notification
            {
                Message = $"Kế hoạch nhập hàng mới {saleForecast.PlanCode}",
                Title = $"Kế hoạch nhập hàng mới {saleForecast.PlanCode}",
                SendUsers = systemUser,
                Object = obj,
                Type = "info"
            });

            return (saleForecast, null);
        }
        catch (Exception ex)
        {
            mess.Errors = ex.Message;
            mess.Status = 500;
            await transaction.RollbackAsync();
            return (null, mess);
        }
    }

    public async Task<(List<Models.SaleForecastModel.SaleForecast>?, Mess?, int)> GetSaleForecast(int skip, int limit,
        string? search, GridifyQuery filter, int cardId)
    {
        var mess = new Mess();
        try
        {
            var query = _context.SaleForecasts.AsQueryable();
            if (!string.IsNullOrEmpty(search))
            {
                search = search.Trim();
                query = query.Where(p => p.PlanName!.ToLower().Contains(search.ToLower()) || p.Bp!.CardName!.ToLower().Contains(search.ToLower()) || p.Bp!.CardCode!.ToLower().Contains(search.ToLower()));
            }

            if (cardId > 0)
            {
                query = query.Where(p => p.CustomerId == cardId);
            }
            

            var total = await query.ApplyFiltering(filter).CountAsync();
            var data = await query
                .AsNoTracking()
                .Include(p => p.Bp)
                .Include(p => p.Author)
                .Include(p => p.SaleForecastItems)
                .ThenInclude(p => p.Item)
                .ThenInclude(p => p!.OUGP)
                .Include(p => p.SaleForecastItems)
                .ThenInclude(p => p.Periods)
                .Include(p => p.SaleForecastItems)
                .ThenInclude(p => p.Uom)
                .ApplyFiltering(filter)
                .OrderByDescending(p => p.Id)
                .Skip(skip * limit)
                .Take(limit)
                .ToListAsync();

            return (data, null, total);
        }
        catch (Exception ex)
        {
            mess.Errors = ex.Message;
            mess.Status = 500;
            return (null, mess, 0);
        }
    }

    public async Task<(Models.SaleForecastModel.SaleForecast?, Mess?)> GetSaleForecastById(int planId)
    {
        var mess = new Mess();
        try
        {
            var plan = await _context.SaleForecasts
                .AsNoTracking()
                .Include(p => p.Bp)
                .Include(p => p.Author)
                .Include(p => p.SaleForecastItems)
                .ThenInclude(p => p.Item)
                .ThenInclude(p => p!.OUGP)
                .Include(p => p.SaleForecastItems)
                .ThenInclude(p => p.Periods)
                .Include(p => p.SaleForecastItems)
                .ThenInclude(p => p.Uom)
                .FirstOrDefaultAsync(p => p.Id == planId);
            if (plan == null)
            {
                mess.Errors = $"OrderPlan with Id {planId} does not exist";
                mess.Status = 404;
                return (null, mess);
            }

            return (plan, null);
        }
        catch (Exception ex)
        {
            mess.Errors = ex.Message;
            mess.Status = 500;
            return (null, mess);
        }
    }


    public async Task<(Models.SaleForecastModel.SaleForecast?, Mess?)> UpdateSaleForecast(int planId,
        Models.SaleForecastModel.SaleForecast saleForecast, int userId)
    {
        var mess = new Mess();
        await using var trans = await _context.Database.BeginTransactionAsync();
        try
        {
            var plan = await _context.SaleForecasts
                .Include(p => p.Author)
                .Include(s => s.SaleForecastItems)
                .ThenInclude(p => p.Periods)
                .FirstOrDefaultAsync(p => p.Id == planId);
            if (plan == null)
            {
                mess.Errors = $"OrderPlan with Id {planId} does not exist";
                mess.Status = 404;
                await trans.RollbackAsync();
                return (null, mess);
            }

            if (plan.Status == "R" || plan.Status == "A")
            {
                mess.Errors = $"Yêu cầu đã được xác nhận hoặc hủy";
                mess.Status = 400;
                await trans.RollbackAsync();
                return (null, mess);
            }

            if (plan.Author?.Id != userId && plan.Author?.UserType == "NPP")
            {
                mess.Errors = "User does not have an author";
                mess.Status = 400;
                await trans.RollbackAsync();
                return (null, mess);
            }


            _mapper.Map(saleForecast, plan);
            foreach (var item in saleForecast.SaleForecastItems)
            {
                if (item.Status == "A" || (item.Id == 0 && string.IsNullOrEmpty(item.Status)))
                {
                    plan.SaleForecastItems.Add(item);
                }
                else
                    switch (item.Status)
                    {
                        case "D":
                            var deleteItem = await _context.SaleForecastItems.FirstOrDefaultAsync(p => p.Id == item.Id);
                            if (deleteItem != null)
                                plan.SaleForecastItems.Remove(deleteItem);
                            break;
                        case "U" when item.Id != 0:
                            var planItem = plan.SaleForecastItems.FirstOrDefault(p => p.Id == item.Id);
                            if (planItem == null) break;
                            _mapper.Map(item, planItem);
                            foreach (var p in item!.Periods!)
                            {
                                _mapper.Map(p, planItem?.Periods?.FirstOrDefault(a => a.Id == p.Id));
                            }

                            break;
                    }
            }

            await _context.SaveChangesAsync();
            await trans.CommitAsync();

            return (plan, null);
        }
        catch (Exception ex)
        {
            mess.Errors = ex.Message;
            mess.Status = 500;
            await trans.RollbackAsync();
            return (null, mess);
        }
    }

    public async Task<(Models.SaleForecastModel.SaleForecast?, Mess?)> AddItemToSaleForecast(int id,
        ICollection<Models.SaleForecastModel.SaleForecastItem> orderPlanItems)
    {
        var mess = new Mess();
        try
        {
            var plan = await _context.SaleForecasts.Include(orderPlan => orderPlan.SaleForecastItems)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (plan == null)
            {
                mess.Errors = $"OrderPlan with Id {id} does not exist";
                mess.Status = 400;
                return (null, mess);
            }

            plan.SaleForecastItems.AddRange(orderPlanItems);

            await _context.SaveChangesAsync();
            return (plan, null);
        }
        catch (Exception ex)
        {
            mess.Errors = ex.Message;
            mess.Status = 500;
            return (null, mess);
        }
    }

    public async Task<(Models.SaleForecastModel.SaleForecast?, Mess?)> RemoveItemFromSaleForecast(int id,
        List<int> orderPlanIds)
    {
        var mess = new Mess();
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var plan = await _context.SaleForecasts.Include(orderPlan => orderPlan.SaleForecastItems)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (plan == null)
            {
                mess.Errors = $"OrderPlan with Id {id} does not exist";
                mess.Status = 400;
                await transaction.RollbackAsync();
                return (null, mess);
            }

            plan.SaleForecastItems.RemoveAll(p => orderPlanIds.Contains(p.Id));

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return (plan, null);
        }
        catch (Exception ex)
        {
            mess.Errors = ex.Message;
            mess.Status = 500;
            await transaction.RollbackAsync();
            return (null, mess);
        }
    }

    public async Task<(Models.SaleForecastModel.SaleForecast?, Mess?)> UpdateSaleForecast(int planId, int planItemId,
        Models.SaleForecastModel.SaleForecastItem saleForecastItem)
    {
        var mess = new Mess();
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var plan = await _context.SaleForecasts.FirstOrDefaultAsync(p => p.Id == planId);
            if (plan == null)
            {
                mess.Errors = $"OrderPlan with Id {planId} does not exist";
                mess.Status = 404;
                await transaction.RollbackAsync();
                return (null, mess);
            }

            var planItem = await _context.SaleForecastItems.FirstOrDefaultAsync(p => p.Id == planItemId);
            if (planItem == null)
            {
                mess.Errors = "OrderPlanItem with Id " + planItemId + " does not exist in OrderPlan";
                mess.Status = 404;
                await transaction.RollbackAsync();
                return (null, mess);
            }

            _mapper.Map(planItem, saleForecastItem);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return (plan, null);
        }
        catch (Exception ex)
        {
            mess.Errors = ex.Message;
            mess.Status = 500;
            await transaction.RollbackAsync();
            return (null, mess);
        }
    }

    public async Task<Mess?> UpdateStatusSaleForecast(int planId, string status, string userType)
    {
        var mess = new Mess();
        try
        {
            var plan = await _context.SaleForecasts.Include(p => p.Author).FirstOrDefaultAsync(p => p.Id == planId);
            if (plan == null)
            {
                mess.Errors = $"Kế hoạch nhập hàng với ID {planId} không tồn tại";
                mess.Status = 404;
                return mess;
            }

            if (plan.Status == "R" || plan.Status == "A")
            {
                mess.Errors = $"Yêu cầu đã được xác nhận hoặc hủy";
                mess.Status = 400;
                return mess;
            }

            plan.Status = status;
            var message = status == "R"
                ? $"Kế hoạch nhập hàng {plan.PlanCode} đã bị hủy"
                : $"Kế hoạch nhập hàng {plan.PlanCode} đã được xác nhận";
            var title = status == "A" ? $"Kế hoạch nhập hàng được xác nhận" : "Kế hoạch nhập hàng bị hủy";

            await _context.SaveChangesAsync();
            var queryUser = _context.AppUser.AsNoTracking().AsQueryable();
            if (userType == "NPP")
            {
                queryUser = queryUser.Where(p => p.UserType == "APSP");
            }
            else
            {
                queryUser = queryUser.Where(p => p.CardId == plan.CustomerId);
            }

            var userIds = await queryUser.Select(p => p.Id).ToListAsync();
            _eventAggregator.Publish(new Models.NotificationModels.Notification
            {
                Message = message,
                Title = title,
                Type = "info",
                SendUsers = userIds,
                Object = new NotificationObject
                {
                    ObjId = plan.Id,
                    ObjName = "Sale Forecast",
                    ObjType = "sale_forecast",
                }
            });
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