using BackEndAPI.Data;
using BackEndAPI.Models.BusinessPartners;
using BackEndAPI.Models.Other;
using Microsoft.EntityFrameworkCore;

namespace BackEndAPI.Service.BusinessPartners.Address;

/// <summary>
/// Implementation extracted từ BusinessPartnerService.
/// Logic giữ nguyên 100% — chỉ thay đổi vị trí file để tách responsibility.
/// </summary>
public class BPAddressService : IBPAddressService
{
    private readonly AppDbContext _context;

    public BPAddressService(AppDbContext context) => _context = context;

    public async Task<(BP?, Mess?)> AddAsync(int bpId, CRD1 address)
    {
        var mess = new Mess();
        await using var trans = await _context.Database.BeginTransactionAsync();
        try
        {
            var bp = await _context.BP.Include(bp => bp.CRD1!).FirstOrDefaultAsync(b => b.Id == bpId);
            if (bp == null)
            {
                mess.Errors = $"BP with id: {bpId} does not exist";
                mess.Status = 404;
                await trans.RollbackAsync();
                return (null, mess);
            }

            bp!.CRD1!.Add(address);

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

    public async Task<(BP?, Mess?)> UpdateAsync(int bpId, CRD1 address)
    {
        var mess = new Mess();
        await using var trans = await _context.Database.BeginTransactionAsync();
        try
        {
            var bp = await _context.BP.AsNoTracking().Include(bp => bp.CRD1!)
                .FirstOrDefaultAsync(b => b.Id == bpId);
            if (bp == null)
            {
                mess.Errors = $"BP with id: {bpId} does not exist";
                mess.Status = 404;
                await trans.RollbackAsync();
                return (null, mess);
            }

            if (!bp!.CRD1!.Exists(p => p.Id == address.Id))
            {
                mess.Errors = $"Address with id: {address.Id} does not exist";
                mess.Status = 404;
                await trans.RollbackAsync();
                return (null, mess);
            }

            address.BPId = bp.Id;
            _context.CRD1.Update(address);

            if (address.Default == "Y" && bp!.CRD1!.FirstOrDefault(p => p.Id == address.Id)!.Default != "Y")
            {
                await _context.Database.ExecuteSqlAsync(
                    $"UPDATE CRD1 SET [Default] = 'N' WHERE BPId = {bp.Id} AND Id != {address.Id} AND [Type] = {address.Type}");
            }

            await _context.SaveChangesAsync();
            await trans.CommitAsync();

            int updateIdx = bp!.CRD1!.FindIndex(p => p.Id == address.Id);
            bp!.CRD1![updateIdx] = address;
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

    public async Task<(BP?, Mess?)> RemoveAsync(int bpId, List<int> addressIds)
    {
        var mess = new Mess();
        await using var trans = await _context.Database.BeginTransactionAsync();
        try
        {
            var bp = await _context.BP.Include(bp => bp.CRD1!).FirstOrDefaultAsync(b => b.Id == bpId);
            if (bp == null)
            {
                mess.Errors = $"BP with id: {bpId} does not exist";
                mess.Status = 404;
                await trans.RollbackAsync();
                return (null, mess);
            }

            bp!.CRD1!.RemoveAll(p => addressIds.Contains(p.Id));
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
}
