using BackEndAPI.Data;
using BackEndAPI.Models.Other;
using BackEndAPI.Models.ProductionCommitmentModel;
using Gridify;
using Microsoft.EntityFrameworkCore;

namespace BackEndAPI.Service.ProductionCommitment;

public class ProductionCommitmentService(AppDbContext context) : IProductionCommitmentService
{
    private readonly AppDbContext _context = context;

    public async Task<(Models.ProductionCommitmentModel.ProductionCommitment?, Mess?)> Create(
        Models.ProductionCommitmentModel.ProductionCommitment productionCommitment)
    {
        var mess = new Mess();
        await using var trans = await _context.Database.BeginTransactionAsync();
        try
        {
            foreach (var cmm in  productionCommitment?.CommitmentLines!)
            {
                int vCount = 0;
                if (cmm.Type == "Y") vCount = 12;
                else if (cmm.Type == "Q") vCount = 4;
                else if (cmm.Type == "M") vCount = 1;
                foreach (var item in cmm.Items!)
                {
                    if (vCount != item.ListValueCommitments?.Count)
                    {
                        mess.Errors = "The production commitment lines have different number of items";
                        mess.Status = 400;
                        await trans.RollbackAsync();
                        return (null, mess);
                    }
                    List<CommitmentItemAttribute> attributes = new List<CommitmentItemAttribute>();
                    foreach (var v in item.ListValueCommitments)
                    {
                       attributes.Add(new CommitmentItemAttribute
                       {
                           Name = "ok",
                            ValueCommitment = v
                       }); 
                    }
                    
                    item.CommitmentItemAttribute = attributes;
                }
            }

            _context.ProductionCommitment.Add(productionCommitment);
            await _context.SaveChangesAsync();
            await trans.CommitAsync();
            return (productionCommitment, null);
        }
        catch (Exception ex)
        {
            mess.Errors = ex.Message;
            mess.Status = 500;
            await trans.RollbackAsync();
            return (null, mess);
        }
    }

    public async Task<(List<Models.ProductionCommitmentModel.ProductionCommitment>?, Mess?, int)>
        GetProductionCommitments(int skip, int limit, GridifyQuery q,string? search)
    {
        var mess = new Mess();
        try
        {
            var query = _context.ProductionCommitment.AsQueryable();
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p => p.CommitmentName.ToLower().Contains(search.ToLower()));
            }

            var count = await query.ApplyFiltering(q).CountAsync();
            
            var data  = await query
                .Include(p => p.CommitmentLines)!
                .ThenInclude(p => p.Items)!
                .ThenInclude(p => p.CommitmentItemAttribute)!
                .Include(p => p.CommitmentLines)!
                .ThenInclude(p => p.Items)!
                .ThenInclude(p => p.DiscountCommitments)
                .ApplyFiltering(q).Skip(skip).Take(limit).ToListAsync();
            foreach (var item in from d in data from cmm in d.CommitmentLines! from item in cmm.Items! select item)
            {
                item.ListValueCommitments = item.CommitmentItemAttribute?.Select(p => p.ValueCommitment).ToList();
            }

            return (data, null, count);

        }
        catch (Exception ex)
        {
            mess.Errors = ex.Message;
            mess.Status = 500;
            return (null, mess, 0);
        }
    }
}