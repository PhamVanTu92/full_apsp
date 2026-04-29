using BackEndAPI.Data;
using BackEndAPI.Models.BusinessPartners;
using BackEndAPI.Models.ItemGroups;
using BackEndAPI.Models.Other;
using Microsoft.EntityFrameworkCore;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace BackEndAPI.Service.ItemAreas
{
    public class BPAreaService : Service<BPArea>, IBPAreaService
    {
        private readonly AppDbContext _context;
        private readonly IHostingEnvironment _webHostEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public BPAreaService(AppDbContext context, IHostingEnvironment webHostEnvironment, IHttpContextAccessor httpContextAccessor) : base(context)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<(BPArea, Mess)> CreateBPAreaAsync(BPArea model)
        {
            Mess mess = new Mess();
            try
            {
                _context.BPArea.Add(model);
                await _context.SaveChangesAsync();
                return (model,null);
            }
            catch (Exception ex)
            {
                mess.Status = 900;
                mess.Errors = ex.Message;
                return (null, mess);
            }
        }

        public Task<(bool,Mess)> DeleteBPAreaAsync(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<(List<BPArea>,Mess)> GetBPAreaAsync()
        {
            Mess mess = new Mess();
            try
            {
                var itemGroup = await _context.BPArea.ToListAsync();
                return (BuildHierarchy(itemGroup), null);
            }
            catch (Exception ex)
            {
                mess.Status = 900;
                mess.Errors = ex.Message;
                return (null, mess);
            }
        }

        public Task<(BPArea, Mess)> UpdateBPAreaAsync(int id, BPArea model)
        {
            throw new NotImplementedException();
        }
        private List<BPArea> BuildHierarchy(List<BPArea> source)
        {
            var lookup = source.ToLookup(x => x.ParentId);
            var rootGroups = lookup.Contains(null) ? lookup[null].ToList() : source;

            foreach (var group in rootGroups)
            {
                AddChildGroups(group, lookup);
            }

            return rootGroups;
        }

        private void AddChildGroups(BPArea parent, ILookup<int?, BPArea> lookup)
        {
            parent.Child = lookup[parent.Id].ToList();

            foreach (var child in parent.Child)
            {
                AddChildGroups(child, lookup);
            }
        }

        public async Task<(List<BPArea>,Mess)> GetBPAreaAsync(string search)
        {
            Mess mess = new Mess();
            try
            {
                var itemGroup = await _context.BPArea.Where(p => p.BPAreaName.Contains(search))
                .ToListAsync();
                return (BuildHierarchy(itemGroup),null);
            }
            catch (Exception ex)
            {
                mess.Status = 900;
                mess.Errors = ex.Message;
                return (null, mess);
            }
        }

        public async Task<(bool,Mess)> BPAreaExistsAsync(int? parentId, int? currentId = null)
        {
            Mess mess = new Mess();
            try
            {
                if (parentId == null) return (true, null);
                if (parentId == currentId) return (false, null);

                return (await _context.BPArea.AnyAsync(pg => pg.Id == parentId), null);
            }
            catch (Exception ex)
            {
                mess.Status = 900;
                mess.Errors = ex.Message;
                return (false, mess);
            }
            
        }

        public async Task<(bool, Mess)> CanDeleteBPAreaAsync(int id)
        {
            Mess mess = new Mess();
            try
            {
                return (!await _context.BPArea.AnyAsync(pg => pg.ParentId == id),null);
            }
            catch (Exception ex)
            {
                mess.Status = 900;
                mess.Errors = ex.Message;
                return (false, mess);
            }
            
        }

        
    }
}
