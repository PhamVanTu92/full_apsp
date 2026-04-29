using AutoMapper;
using BackEndAPI.Data;
using BackEndAPI.Models.Other;
using Microsoft.EntityFrameworkCore;

namespace BackEndAPI.Service.StorageFee
{
    [method: Obsolete]
    public class StorageFeeService(
        IMapper mapper,
        AppDbContext context,
        IWebHostEnvironment webHostEnvironment,
        IHttpContextAccessor httpContextAccessor) : Service<Models.StorageFee.FeeMilestone>(context), IStorageFeeService
    {
        private readonly AppDbContext _context = context;
        private readonly IMapper _mapper = mapper;
        [Obsolete] private readonly IWebHostEnvironment _webHostEnvironment = webHostEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

        public async Task<(IEnumerable<Models.StorageFee.FeeMilestone>?, Mess?, int total)> GetFeeMilestonesAsync(
            int skip, int limit)
        {
            Mess mess = new Mess();
            try
            {
                var query = _context.Set<Models.StorageFee.FeeMilestone>().AsQueryable();
                var totalCount = await query.CountAsync();
                var feeMilestones = await query.Skip(skip * limit).Take(limit).ToListAsync();

                return (feeMilestones, null, totalCount);
            }
            catch (Exception ex)
            {
                mess.Errors = ex.Message;
                mess.Status = 500;
                return (null, mess, 0);
            }
        }

        public async Task<(List<Models.StorageFee.FeeMilestone>?, Mess?)> CreateFeeMilestoneAsync(
            List<Models.StorageFee.FeeMilestoneDto> feeDto)
        {
            List<Models.StorageFee.FeeMilestone> feeMilestones = new List<Models.StorageFee.FeeMilestone>();
            Mess mess = new Mess();
            foreach (var a in feeDto)
            {
                var fee = new Models.StorageFee.FeeMilestone();
                feeMilestones.Add(fee);
                try
                {
                    _mapper.Map(fee, feeDto);

                    _context.FeeMilestones.Add(fee);
                    await _context.SaveChangesAsync();
                    fee.isSuccess = true;
                }
                catch (Exception ex)
                {
                    fee.isSuccess = true;
                    fee.ErrorMessage = ex.Message;
                }
            }

            return (feeMilestones, null);
        }

        public async Task<(Models.StorageFee.FeeMilestone?, Mess?)> GetFeeMilestoneByIdAsync(int id)
        {
            Mess mess = new Mess();
            try
            {
                var result = await _context.FeeMilestones.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
                if (result == null)
                {
                    mess.Status = 404;
                    mess.Errors = "FeeMilestone Id not found.";
                    return (null, mess);
                }

                return (result, null);
            }
            catch (Exception ex)
            {
                mess.Errors = ex.Message;
                mess.Status = 500;

                return (null, mess);
            }
        }

        public async Task<Mess?> UpdateFeeMilestoneAsync(Models.StorageFee.FeeMilestoneDto feeDto)
        {
            Mess mess = new Mess();

            try
            {
                var fee = await _context.FeeMilestones.FindAsync(feeDto.Id);
                if (fee == null)
                {
                    mess.Errors = "FeeMilestone ID not found.";
                    mess.Status = 404;
                    return mess;
                }

                _mapper.Map(fee, feeDto);

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

        public async Task<Mess?> DeleteFeeMilestoneAsync(int id)
        {
            Mess mess = new Mess();
            using (var trans = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var fee = await _context.FeeMilestones.FindAsync(id);
                    if (fee == null)
                    {
                        mess.Errors = "FeeMilestone ID not found.";
                        mess.Status = 404;
                        await trans.RollbackAsync();
                        return mess;
                    }

                    var storageFeeLine = await _context.StorageFeeLines.FirstOrDefaultAsync(l => l.FeeId == id);
                    if (storageFeeLine != null)
                    {
                        mess.Status = 400;
                        mess.Errors = "used cannot be deleted";
                    }

                    _context.FeeMilestones.Remove(fee);
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
        }

        public async Task<(Models.StorageFee.StorageFee?, Mess?)> CreateStorageFeeAsync(
            Models.StorageFee.StorageFeeDto storageFeeDto)
        {
            Mess mess = new Mess();
            using (var trans = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var storageFee = _mapper.Map<Models.StorageFee.StorageFee>(storageFeeDto);
                    _context.StorageFees.Add(storageFee);
                    await _context.SaveChangesAsync();
                    await trans.CommitAsync();

                    return (storageFee, null);
                }
                catch (Exception ex)
                {
                    await trans.RollbackAsync();
                    mess.Errors = ex.Message;
                    mess.Status = 500;

                    return (null, mess);
                }
            }
        }

        public async Task<Mess?> UpdateStorageFeeAsync(Models.StorageFee.StorageFeeDto storageFee)
        {
            Mess mess = new Mess();
            using (var trans = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var stor = await _context.StorageFees.FindAsync(storageFee.Id);
                    if (stor == null)
                    {
                        await trans.RollbackAsync();
                        mess.Errors = "Storage Fee not found";
                        mess.Status = 404;
                        return mess;
                    }

                    stor.UgpId = storageFee.UgpId;

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
        }

        public async Task<Mess?> AddLinesToStorageFeeAsync(int id, List<Models.StorageFee.StorageFeeLineDto> list)
        {
            var mess = new Mess();
            Task<bool> ok = await ExecuteInTransactionAsync(async (context) =>
            {
                try
                {
                    var storage = await context.StorageFees.FindAsync(id);

                    if (storage == null)
                    {
                        mess.Errors = "StorageFee ID not found.";
                        mess.Status = 404;
                        return Task.FromResult(false);
                    }


                    foreach (var item in list)
                    {
                        var _item = _mapper.Map<Models.StorageFee.StorageFeeLine>(item);
                        _item.StorageFeeId = id;

                        await _context.StorageFeeLines.AddAsync(_item);
                    }

                    await context.SaveChangesAsync();

                    return Task.FromResult(true);
                }
                catch (Exception ex)
                {
                    mess.Errors = ex.Message;
                    mess.Status = 500;
                    return Task.FromResult(false);
                }
            });

            if (await ok == false) return mess;

            return null;
        }

        public async Task<Mess?> RemoveLinesFromStorageFee(List<int> lineIds)
        {
            var mess = new Mess();
            Task<bool> ok = await ExecuteInTransactionAsync(async (context) =>
            {
                try
                {
                    if (lineIds == null || lineIds.Count == 0)
                    {
                        return Task.FromResult(true);
                    }

                    var linesFound = await context.StorageFeeLines.Where(p => p.Id == lineIds[0]).ToListAsync();

                    context.StorageFeeLines.RemoveRange(linesFound);
                    await context.SaveChangesAsync();

                    return Task.FromResult(true);
                }
                catch (Exception ex)
                {
                    mess.Errors = ex.Message;
                    mess.Status = 500;
                    return Task.FromResult(false);
                }
            });
            if (await ok == false) return mess;

            return null;
        }

        public async Task<Mess?> DeleteStorageFeeAsync(int id)
        {
            Mess mess = new Mess();
            using var trans = await _context.Database.BeginTransactionAsync();
            try
            {
                var stor = await _context.StorageFees.FindAsync(id);
                if (stor == null)
                {
                    await trans.RollbackAsync();
                    mess.Errors = "Storage Fee not found";
                    mess.Status = 404;
                    return mess;
                }

                _context.StorageFees.Remove(stor);
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

        public async Task<(IEnumerable<Models.StorageFee.StorageFee>?, Mess?, int total)> GetStorageFeeAync(int skip,
            int limit)
        {
            Mess mess = new Mess();
            try
            {
                var query = _context.Set<Models.StorageFee.StorageFee>().AsQueryable();
                var totalCount = await query.CountAsync();
                var storageFeeList = await query.Include(p => p.Lines).Skip(skip * limit).Take(limit).ToListAsync();

                return (storageFeeList, null, totalCount);
            }
            catch (Exception ex)
            {
                mess.Errors = ex.Message;
                mess.Status = 500;
                return (null, mess, 0);
            }
        }

        public async Task<(Models.StorageFee.StorageFee?, Mess?)> GetStorageFeeByIdAync(int id)
        {
            Mess mess = new Mess();
            try
            {
                var data = await _context.StorageFees.Include(p => p.Lines).ThenInclude(a => a.FeeMilestone)
                    .FirstOrDefaultAsync(p => p.Id == id);
                if (data == null)
                {
                    mess.Errors = "StorageFee Id  not found.";
                    mess.Status = 404;
                    return (null, mess);
                }

                return (data, null);
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