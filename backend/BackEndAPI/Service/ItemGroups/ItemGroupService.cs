using BackEndAPI.Data;
using System.Linq.Expressions;
using AutoMapper;
using BackEndAPI.Models.ItemGroups;
using BackEndAPI.Models.ItemMasterData;
using BackEndAPI.Models.Other;
using Gridify;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace BackEndAPI.Service.ItemGroups
{
    public class ItemGroupService : Service<OIBT>, IItemGroupService
    {
        private readonly AppDbContext _context;
        private readonly IHostingEnvironment _webHostEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMapper _mapper;

        public ItemGroupService(AppDbContext context, IHostingEnvironment webHostEnvironment, IMapper mapper,
            IHttpContextAccessor httpContextAccessor) : base(context)
        {
            _context = context;
            _mapper = mapper;
            _webHostEnvironment = webHostEnvironment;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<OIBT> CreateItemAsync(OIBT model)
        {
            var mess = new Mess();
            try
            {
                model.ConditionItemGroups.ForEach(c => c.Values.ForEach(v =>
                    c.CondValues.Add(new ConditionItemGroupValue()
                    {
                        Value = v,
                    })));

                if (!model.IsSelected)
                {
                    var (bp, _) = await GetItemInCond(model);
                    model.Items.AddRange(bp);
                }
                else
                {
                    model.Items = await _context.Item.Where(x => model.ItemIds.Contains(x.Id)).ToListAsync();
                }

                _context.OIBT.Add(model);
                await _context.SaveChangesAsync();

                return (model);
            }
            catch (Exception ex)
            {
                mess.Errors = ex.Message;
                mess.Status = 500;
                return null;
            }
        }

        public Task<bool> DeleteItemGroupAsync(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<(List<OIBT>, int)> GetItemGroupAsync(GridifyQuery gridifyQuery, string? search)
        {
            
            var query = _context.OIBT.AsQueryable().ApplyFiltering(gridifyQuery);
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(x => x.ItmsGrpName == search || 
                                         x.Description.Contains(search));
            }

            var total = await query.CountAsync();
            
            var itemGroup = await _context.OIBT
                .Include(p => p.ConditionItemGroups)
                .ThenInclude(p => p.CondValues)
                .Include(x => x.Items)
                .ApplyOrdering(gridifyQuery)
                .ApplyPaging(gridifyQuery)
                .ToListAsync();
            
            return (itemGroup, total);
        }
        
        public async Task<OIBT?> GetItemGroupByIdAsync(int id)
        {
            var itemGroup = await _context.OIBT
                .Include(p => p.ConditionItemGroups)
                .ThenInclude(p => p.CondValues)
                .Include(x => x.Items)
                .ThenInclude(x => x.ITM1)
                .FirstOrDefaultAsync(x => x.Id == id);
            return itemGroup;
        }

        public async Task<OIBT> UpdateItemGroupAsync(int id, OIBT model)
        {
            var itemGroup = await _context.OIBT
                .Include(p => p.ConditionItemGroups)
                .ThenInclude(p => p.CondValues)
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (itemGroup == null)
            {
                throw new KeyNotFoundException("ItemGroup not found");
            }
            
            model.ConditionItemGroups.ForEach(c =>
            {
                c.CondValues = [];
                c.Values.ForEach(v =>
                    c.CondValues.Add(new ConditionItemGroupValue()
                    {
                        Value = v,
                    }));
            });


            itemGroup.Items = await _context.Item.Where(x => model.ItemIds.Contains(x.Id)).ToListAsync();
            _mapper.Map(model, itemGroup);
            if (!model.IsSelected)
            {
                var (bp, _) = await GetItemInCond(model);
                itemGroup.Items.AddRange(bp);
            }
            else
            {
                itemGroup.Items = await _context.Item.Where(x => model.ItemIds.Contains(x.Id)).ToListAsync();
            }

            await _context.SaveChangesAsync();

            return itemGroup;
        }

        public async Task<List<OIBT>> GetItemGroupAsync(string search)
        {
            var itemGroup = await _context.OIBT.Where(p => p.ItmsGrpName.Contains(search))
                .Include(p => p.ConditionItemGroups)
                .ThenInclude(p => p.CondValues)
                .ToListAsync();
            return itemGroup;
        }

        public async Task<bool> ItemGroupExistsAsync(int? parentId, int? currentId = null)
        {
            if (parentId == null) return true;
            if (parentId == currentId) return false;
            return await _context.OIBT.AnyAsync(pg => pg.Id == parentId);
        }

        public async Task<(List<Item>?, Mess?)> GetItemInCond(OIBT bpGroup)
        {
            var mess = new Mess();
            try
            {
                var predicate = LinqKit.PredicateBuilder.New<Item>();

                List<Expression<Func<Item, bool>>> conditions =
                    new List<Expression<Func<Item, bool>>>();

                foreach (var cond in bpGroup.ConditionItemGroups)
                {
                    var values = cond.CondValues.Select(x => x.Value).ToList();
                    Expression<Func<Item, bool>> whereClause;
                    switch (cond.TypeCondition)
                    {
                        case "industry":
                            whereClause = cond.IsEqual
                                ? (Expression<Func<Item, bool>>)(x =>
                                    values.Contains(x.IndustryId.ToString()!))
                                : (Expression<Func<Item, bool>>)(p =>
                                    !values.Contains(p.IndustryId.ToString()!));
                            break;
                        case "brand":
                            whereClause = cond.IsEqual
                                ? (Expression<Func<Item, bool>>)(p =>
                                    values.Contains(p.BrandId.ToString()!))
                                : (Expression<Func<Item, bool>>)(p =>
                                    !values.Contains(p.BrandId.ToString()!));
                            break;
                        case "item_type":
                            whereClause = cond.IsEqual
                                ? (Expression<Func<Item, bool>>)(p =>
                                    values.Contains(p.ItemTypeId.ToString()!))
                                : (Expression<Func<Item, bool>>)(p =>
                                    !values.Contains(p.ItemTypeId.ToString()!));
                            break;
                        case "product_applications":
                            whereClause = cond.IsEqual
                                ? (Expression<Func<Item, bool>>)(p =>
                                    values.Contains(p.ProductApplicationsCode!))
                                : (Expression<Func<Item, bool>>)(p =>
                                    !values.Contains(p.ProductApplicationsCode!));
                            break;
                        case "product_group":
                            whereClause = cond.IsEqual
                                ? (Expression<Func<Item, bool>>)(p =>
                                    values.Contains(p.ProductGroupCode!))
                                : (Expression<Func<Item, bool>>)(p =>
                                    !values.Contains(p.ProductGroupCode!));
                            break;
                        case "packing":
                            whereClause = cond.IsEqual
                                ? (Expression<Func<Item, bool>>)(p =>
                                    values.Contains(p.PackingId.ToString()!))
                                : (Expression<Func<Item, bool>>)(p =>
                                    !values.Contains(p.PackingId.ToString()!));
                            break;
                        default:
                            continue;
                    }

                    conditions.Add(whereClause);
                }

                foreach (var cond in conditions)
                {
                    predicate = bpGroup.IsOneOfThem ? predicate.Or(cond) : predicate.And(cond);
                }

                var bpList = await _context.Item
                    .Where(predicate).ToListAsync();
                return (bpList, null);
            }
            catch (Exception ex)
            {
                mess.Errors = ex.Message;
                mess.Status = 500;

                return (null, mess);
            }
        }

        public async Task<(OIBT?, Mess?)> AddCondToGroup(int groupId, List<ConditionItemGroup> conds)
        {
            var mess = new Mess();

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var group = await _context.OIBT.FirstOrDefaultAsync(pg => pg.Id == groupId);
                if (group == null)
                {
                    mess.Status = 404;
                    mess.Errors = "Group not found";
                    await transaction.RollbackAsync();
                    return (null, mess);
                }

                if (group.IsSelected)
                {
                    mess.Status = 400;
                    mess.Errors = "Group is not selected";
                    await transaction.RollbackAsync();
                    return (null, mess);
                }

                foreach (var cond in conds)
                {
                    cond.Values.ForEach(b => cond.CondValues.Add(new ConditionItemGroupValue()
                    {
                        Value = b,
                    }));
                }

                group.ConditionItemGroups.AddRange(conds);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return (group, null);
            }
            catch (Exception ex)
            {
                mess.Errors = ex.Message;
                mess.Status = 500;
                await transaction.RollbackAsync();
                return (null, mess);
            }
        }

        public async Task<(OIBT?, Mess?)> RemoveCondFromGroup(int groupId, List<int> condIds)
        {
            var mess = new Mess();

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var group = await _context.OIBT
                    .Include(p => p.ConditionItemGroups)
                    .ThenInclude(c => c.CondValues)
                    .FirstOrDefaultAsync(pg => pg.Id == groupId);
                if (group == null)
                {
                    mess.Status = 404;
                    mess.Errors = "Group not found";
                    await transaction.RollbackAsync();
                    return (null, mess);
                }

                group.ConditionItemGroups.RemoveAll(p => condIds.Contains(p.Id));

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return (group, null);
            }
            catch (Exception ex)
            {
                mess.Errors = ex.Message;
                mess.Status = 500;
                await transaction.RollbackAsync();
                return (null, mess);
            }
        }

        public async Task<Mess?> UpdateGroupCondtion(int groupId, List<ConditionItemGroup> conds)
        {
            var mess = new Mess();
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                foreach (var cond in conds)
                {
                    var oldCond = _context.ConditionItemGroup
                        .Include(pg => pg.CondValues)
                        .FirstOrDefault(pg => pg.Id == cond.Id && pg.GroupId == groupId);
                    if (oldCond == null)
                    {
                        mess.Errors = "Condition not found";
                        mess.Status = 404;
                        await transaction.RollbackAsync();
                        return mess;
                    }

                    _mapper.Map(cond, oldCond);

                    var oldValue = oldCond.CondValues.Select(pg => pg.Value).ToList();
                    var rmValue = oldValue.Except(cond.Values).ToList();
                    var addValue = cond.Values.Except(oldValue).ToList();

                    oldCond.CondValues.RemoveAll(pg => rmValue.Contains(pg.Value));
                    oldCond.CondValues.AddRange(addValue.Select(p => new ConditionItemGroupValue()
                        {
                            Value = p
                        }).ToList()
                    );
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return null;
            }
            catch (Exception ex)
            {
                mess.Errors = ex.Message;
                mess.Status = 500;
                await transaction.RollbackAsync();
                return mess;
            }
        }
    }
}