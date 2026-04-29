using BackEndAPI.Data;
using BackEndAPI.Models.ItemGroups;
using BackEndAPI.Models.Other;
using BackEndAPI.Service.Brands;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1.Pkcs;
using static Org.BouncyCastle.Asn1.Cmp.Challenge;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace BackEndAPI.Service.Industrys
{
    public class IndustryService:Service<Industry>, IIndustryService
    {
        private readonly AppDbContext _context;
        private readonly IHostingEnvironment _webHostEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;
        Mess mes = new Mess();
        public IndustryService(AppDbContext context, IHostingEnvironment webHostEnvironment, IHttpContextAccessor httpContextAccessor) : base(context)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<(bool, Mess)> SyncIndustryAsync(List<Industry> models)
        {
            try
            {
                foreach (var model in models)
                {
                    var industry = await _context.Industry.FirstOrDefaultAsync(p => p.SapCode == model.SapCode);
                    if (industry != null)
                    {

                        if (industry.Code != model.Code || industry.Name != model.Name)
                        {
                            industry.Code = model.Code;
                            industry.Name = model.Name;
                            _context.Industry.Update(industry);
                            await _context.SaveChangesAsync();
                        }
                    }
                    industry = await _context.Industry.FirstOrDefaultAsync(p => p.SapCode == null && (p.Code == model.Code || p.Name == model.Name));
                    if (industry != null)
                    {
                        industry.SapCode = model.SapCode;
                        industry.Code = model.Code;
                        industry.Name = model.Name;
                        _context.Industry.Update(industry);
                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                        industry = await _context.Industry.FirstOrDefaultAsync(p => p.SapCode == model.SapCode && p.Name == model.Name);
                        if (industry == null)
                        {
                            Industry u1 = new Industry();
                            u1.Code = model.Code;
                            u1.Name = model.Name;
                            u1.SapCode = model.SapCode;
                            _context.Industry.Add(u1);
                            await _context.SaveChangesAsync();
                        }
                        else
                        {
                            industry.SapCode = model.SapCode;
                            industry.Code = model.Code;
                            industry.Name = model.Name;
                            _context.Industry.Update(industry);
                            await _context.SaveChangesAsync();
                        }
                    }
                }
                return (true, null);
            }
            catch (Exception ex)
            {
                mes.Status = 900;
                mes.Errors = ex.Message;
                return (false, mes);
            }
        }

        public async Task<(List<Industry>?, Mess?)> GetIndustrysAsync()
        {
            var mess = new Mess();

            try
            {
                var inds = await _context.Industry.Include(i => i.Brands).ToListAsync();
                return (inds, null);
            }
            catch (Exception ex)
            {
                mess.Errors = ex.Message;
                mess.Status = 500;
                return (null, mess);
            }
        }
        public async Task<(List<IndustryView>?, Mess?)> GetIndustryViewAsync()
        {
            var mess = new Mess();

            try
            {
                var query =
                 from spec in _context.ItemSpec
                 join ind in _context.Industry on spec.IndustryId equals ind.Id
                 join br in _context.Brand on spec.BrandId equals br.Id
                 join it in _context.ItemType on spec.ItemTypeId equals it.Id
                 select new
                 {
                     Industry = ind,
                     Brand = br,
                     ItemType = it
                 };
                var result = query
                .GroupBy(x => new { x.Industry.Id, x.Industry.Code, x.Industry.Name })
                .Select(indGroup => new IndustryView
                {
                    Id = indGroup.Key.Id,
                    Code = indGroup.Key.Code,
                    Name = indGroup.Key.Name,
                    Brands = indGroup
                        .GroupBy(b => new { b.Brand.Id, b.Brand.Code, b.Brand.Name })
                        .Select(brGroup => new BrandView
                        {
                            Id = brGroup.Key.Id,
                            Code = brGroup.Key.Code,
                            Name = brGroup.Key.Name,
                            ItemTypes = brGroup
                                .GroupBy(t => new { t.ItemType.Id, t.ItemType.Code, t.ItemType.Name })
                                .Select(tpGroup => new ItemTypeView
                                {
                                    Id = tpGroup.Key.Id,
                                    Code = tpGroup.Key.Code,
                                    Name = tpGroup.Key.Name
                                })
                                .ToList()
                        })
                }).ToList();

                return (result, null);
            }
            catch (Exception ex)
            {
                mess.Errors = ex.Message;
                mess.Status = 500;
                return (null, mess);
            }
        }

        public async Task<(List<IndustryDTO>?, Mess?)> GetIndustrysNewAsync(int? CardId)
        {
             var mess = new Mess();

            try
            {
                var BPId = _context.BpClassify.Where(e => e.BpId == CardId).Select(e => e.IndustryId).ToArray();
                var Brands = _context.BpClassify.Include(e => e.Brands).Where(e => e.BpId == CardId).SelectMany(e => e.Brands.Select(b => b.Id)).ToArray();
                var ItemType = _context.BpClassify.Include(e => e.ItemType).Where(e => e.BpId == CardId).SelectMany(e => e.ItemType.Select(b => b.Id)).ToArray();
                var inds = _context.ItemSpec.Where(e => BPId.Contains(e.IndustryId) || BPId.Count() == 0)
                .GroupBy(x => new { x.IndustryId, x.Industry })
                .Select(g => new IndustryDTO
                {
                    Id = g.Key.IndustryId,
                    Name = g.Key.Industry,
                    Brands = g.Where(e=> Brands.Contains(e.BrandId) || Brands.Count() == 0).GroupBy(b => new { b.BrandId, b.Brand })
                              .Select(bg => new BrandDTO
                              {
                                  Id = bg.Key.BrandId,
                                  Name = bg.Key.Brand,
                                  ItemTypes = bg.Where(e => ItemType.Contains(e.ItemTypeId) || ItemType.Count() == 0).GroupBy(it => new { it.ItemTypeId, it.ItemType })
                                    .Select(gIt => new ItemTypeDTO
                                    {
                                        Id = gIt.Key.ItemTypeId,
                                        Name = gIt.Key.ItemType
                                    })
                                    .ToList()
                              })
                              .ToList()
                })
                .OrderBy(x => x.Id)
                .ToList();
                return (inds, null);
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
