using System.Linq.Expressions;
using System.Security.Claims;
using AutoMapper;
using BackEndAPI.Data;
using BackEndAPI.Dtos;
using BackEndAPI.Models.Approval_V2;
using BackEndAPI.Models.BPGroups;
using BackEndAPI.Models.BusinessPartners;
using BackEndAPI.Models.Other;
using BackEndAPI.Service.Approval_V2.ApprovalWorkFlow.Engine;
using BackEndAPI.Service.Approval_V2.ApprovalWorkFlow.Service;
using Gridify;
using Microsoft.EntityFrameworkCore;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace BackEndAPI.Service.BPGroups
{
    public class BPGroupService : Service<OCRG>, IBPGroupService
    {
        private readonly AppDbContext             _context;
        private readonly IHostingEnvironment      _webHostEnvironment;
        private readonly IHttpContextAccessor     _httpContextAccessor;
        private readonly IMapper                  _mapper;
        private readonly LoggingSystemService     _systemLog;
        private readonly IApprovalWorkFlowService _approvalWorkFlowService;

        public BPGroupService(AppDbContext context, IHostingEnvironment webHostEnvironment,
            IHttpContextAccessor httpContextAccessor, IMapper mapper, LoggingSystemService systemLog,
            IApprovalWorkFlowService approvalWorkFlowService) : base(context)
        {
            _context                 = context;
            _webHostEnvironment      = webHostEnvironment;
            _httpContextAccessor     = httpContextAccessor;
            _mapper                  = mapper;
            _systemLog               = systemLog;
            _approvalWorkFlowService = approvalWorkFlowService;
        }

        public async Task<bool> BPGroupExistsAsync(int? parentId, int? currentId = null)
        {
            if (parentId == null) return true;
            if (parentId == currentId) return false;
            return await _context.OCRG.AnyAsync(pg => pg.Id == parentId);
        }

        public async Task<(OCRG?, Mess?)> CreateGroup(OCRG model)
        {
            var             mess        = new Mess();
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                model.ConditionCusGroups.ForEach(c => c.Values.ForEach(v => c.CondValues.Add(new ConditionCusGroupValue
                {
                    Value = v,
                })));

                var (bp, _) = await GetCustomerInCond(model);
                model.Customers.AddRange(bp);

                _context.OCRG.Add(model);
                await _context.SaveChangesAsync();
                await _systemLog.SaveWithTransAsync("INFO", "Create", $"Tạo nhóm khách hàng mới {model.GroupName}",
                    "CustomerGroup", model.Id);


                await transaction.CommitAsync();

                // #region Test
                //
                // var user   = _httpContextAccessor.HttpContext?.User;
                // var userId = int.Parse(user?.FindFirst(ClaimTypes.NameIdentifier)!.Value!);
                //
                // var res = await _approvalWorkFlowService.CheckApprovalAsync(model.Id, DocumentEnum.PurchaseOrder);
                //
                // await _approvalWorkFlowService.CreateAsync(new List<IdAndTypeDocDto>
                // {
                //     new IdAndTypeDocDto
                //     {
                //         Id   = model.Id,
                //         Type = DocumentEnum.PurchaseOrder
                //     }
                // }, userId, res);
                //
                // #endregion

                return (model, null);
            }
            catch (Exception ex)
            {
                mess.Errors = ex.Message;
                mess.Status = 500;
                await transaction.RollbackAsync();
                return (null, mess);
            }
        }

        public Task<bool> DeleteBPGroupAsync(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<(OCRG?, Mess?)> GetBPGroupById(int id)
        {
            var mess = new Mess();
            try
            {
                var bpGroup = await _context.OCRG
                    .Include(p => p.Customers)
                    .Include(p => p.ConditionCusGroups)
                    .ThenInclude(p => p.CondValues)
                    .FirstOrDefaultAsync(pg => pg.Id == id);
                bpGroup?.ConditionCusGroups.ForEach(c => c.Values = c.CondValues.Select(cd => cd.Value).ToList());

                if (!bpGroup!.IsSelected)
                {
                    var (bp, _)       = await GetCustomerInCond(bpGroup);
                    bpGroup.Customers = bp ?? [];
                }

                return (bpGroup, null);
            }
            catch (Exception ex)
            {
                mess.Errors = ex.Message;
                mess.Status = 500;
                return (null, mess);
            }
        }

        public async Task<(List<OCRG>, int)> GetBPGroupAsync(string groupType, int skip, int limit, string search,
            GridifyQuery q)
        {
            var query = _context.OCRG.AsNoTracking().AsQueryable().ApplyFiltering(q);
            if (!string.IsNullOrEmpty(search))
            {
                search = search.Trim().ToLower();
                query = query.Where
                (
                    p => p.Customers.Any(x => x.CardName.ToLower().Contains(search))
                        || p.GroupName.ToLower().Contains(search)
                );
            }

            var totalCount = await query.CountAsync();
            var bpGroup = await query
                .Include(p => p.Customers)
                .Include(p => p.ConditionCusGroups)
                .ThenInclude(p => p.CondValues)
                .Where(p => p.GroupType.Equals(groupType))
                .OrderByDescending(p => p.Id)
                .Skip(skip * limit)
                .Take(limit)
                .ToListAsync();

            bpGroup.ForEach(p =>
                p.ConditionCusGroups.ForEach(c => c.Values = c.CondValues.Select(cd => cd.Value).ToList()));
            return (bpGroup, totalCount);
        }

        public async Task<List<OCRG>> GetBPGroupAsync(string search, string groupType)
        {
            var bpGroup = await _context.OCRG
                .Include(p => p.Customers)
                .Include(p => p.ConditionCusGroups)
                .ThenInclude(p => p.CondValues)
                .Where(p => p.GroupName.Contains(search) & p.GroupType.Equals(groupType))
                .OrderByDescending(p => p.Id)
                .ToListAsync();
            bpGroup.ForEach(p =>
                p.ConditionCusGroups.ForEach(c => c.Values = c.CondValues.Select(cd => cd.Value).ToList()));
            return bpGroup;
        }

        public Task<OCRG> UpdateBPGroupAsync(int id, OCRG model)
        {
            throw new NotImplementedException();
        }

        private List<OCRG> BuildHierarchy(List<OCRG> source)
        {
            var lookup     = source.ToLookup(x => x.ParentId);
            var rootGroups = lookup.Contains(null) ? lookup[null].ToList() : source;

            foreach (var group in rootGroups)
            {
                AddChildGroups(group, lookup);
            }

            return rootGroups;
        }

        private void AddChildGroups(OCRG parent, ILookup<int?, OCRG> lookup)
        {
            parent.Child = lookup[parent.Id].ToList();

            foreach (var child in parent.Child)
            {
                AddChildGroups(child, lookup);
            }
        }

        public async Task<bool> CanDeleteBPGroupAsync(int id)
        {
            return !await _context.OCRG.AnyAsync(pg => pg.ParentId == id);
        }

        public async Task<(OCRG?, Mess?)> AddCustomerToGroup(int groupId, List<int> customerIds)
        {
            var mess = new Mess();

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var group = await _context.OCRG.FirstOrDefaultAsync(pg => pg.Id == groupId);
                if (group == null)
                {
                    mess.Status = 404;
                    mess.Errors = "Group not found";
                    await transaction.RollbackAsync();
                    return (null, mess);
                }

                group.Customers.AddRange(await _context.BP.Where(b => customerIds.Contains(b.Id)).ToListAsync());

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

        public async Task<(OCRG?, Mess?)> RemoveCustomerFromGroup(int groupId, List<int> customerIds)
        {
            var mess = new Mess();

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var group = await _context.OCRG.Include(p => p.Customers).FirstOrDefaultAsync(pg => pg.Id == groupId);
                if (group == null)
                {
                    mess.Status = 404;
                    mess.Errors = "Group not found";
                    await transaction.RollbackAsync();
                    return (null, mess);
                }

                group.Customers.RemoveAll(p => customerIds.Contains(p.Id));

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

        public async Task<(OCRG?, Mess?)> AddCondToGroup(int groupId, List<ConditionCusGroup> conds)
        {
            var mess = new Mess();

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var group = await _context.OCRG.FirstOrDefaultAsync(pg => pg.Id == groupId);
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
                    cond.Values.ForEach(b => cond.CondValues.Add(new ConditionCusGroupValue
                    {
                        Value = b,
                    }));
                }

                group.ConditionCusGroups.AddRange(conds);

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

        public async Task<(OCRG?, Mess?)> RemoveCondFromGroup(int groupId, List<int> condIds)
        {
            var mess = new Mess();

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var group = await _context.OCRG
                    .Include(p => p.ConditionCusGroups)
                    .ThenInclude(c => c.CondValues)
                    .FirstOrDefaultAsync(pg => pg.Id == groupId);
                if (group == null)
                {
                    mess.Status = 404;
                    mess.Errors = "Group not found";
                    await transaction.RollbackAsync();
                    return (null, mess);
                }

                group.ConditionCusGroups.RemoveAll(p => condIds.Contains(p.Id));

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

        public async Task<Mess?> UpdateGroupCondtion(int groupId, List<ConditionCusGroup> conds)
        {
            var             mess        = new Mess();
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                foreach (var cond in conds)
                {
                    var oldCond = _context.ConditionCusGroups
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
                    var rmValue  = oldValue.Except(cond.Values).ToList();
                    var addValue = cond.Values.Except(oldValue).ToList();

                    oldCond.CondValues.RemoveAll(pg => rmValue.Contains(pg.Value));
                    oldCond.CondValues.AddRange(addValue.Select(p => new ConditionCusGroupValue
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

        public async Task<(List<BP>?, Mess?)> GetCustomerInCond(OCRG bpGroup)
        {
            var mess = new Mess();
            try
            {
                var predicate = LinqKit.PredicateBuilder.New<BP>();

                List<Expression<Func<BP, bool>>> conditions =
                    new List<Expression<Func<BP, bool>>>();

                foreach (var cond in bpGroup.ConditionCusGroups)
                {
                    var                        values = cond.Values;
                    Expression<Func<BP, bool>> whereClause;
                    switch (cond.TypeCondition)
                    {
                        case "region":
                            whereClause = cond.IsEqual
                                ? (Expression<Func<BP, bool>>)(p =>
                                    p.Classify!.Any(x => values.Contains(x.RegionId.ToString())))
                                : (Expression<Func<BP, bool>>)(p =>
                                    !p.Classify!.Any(x => values.Contains(x.RegionId.ToString())));
                            break;
                        case "size":
                            whereClause = cond.IsEqual
                                ? (Expression<Func<BP, bool>>)(p =>
                                    p.Classify!.Any(x => x.Sizes!.Any(y => values.Contains(y.Id.ToString())))
                                )
                                : (Expression<Func<BP, bool>>)(p =>
                                    !p.Classify!.Any(x => x.Sizes!.Any(y => values.Contains(y.Id.ToString()))));
                            break;
                        case "area":
                            whereClause = cond.IsEqual
                                ? (Expression<Func<BP, bool>>)(p =>
                                    p.Classify!.Any(x => values.Contains(x.AreaId.ToString())))
                                : (Expression<Func<BP, bool>>)(p =>
                                    !p.Classify!.Any(x => values.Contains(x.AreaId.ToString())));
                            break;
                        case "industry":
                            whereClause = cond.IsEqual
                                ? (Expression<Func<BP, bool>>)(p =>
                                    p.Classify!.Any(x => values.Contains(x.IndustryId.ToString())))
                                : (Expression<Func<BP, bool>>)(p =>
                                    !p.Classify!.Any(x => values.Contains(x.IndustryId.ToString())));
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

                var bpList = await _context.BP.Include(p => p.Classify)
                    .ThenInclude(p => p.Sizes)
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
    }
}