
using BackEndAPI.Data;
using BackEndAPI.Models.BPGroups;
using BackEndAPI.Models.Other;
using Microsoft.EntityFrameworkCore;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace BackEndAPI.Service.Privilege
{
    public class PrivilegesService : Service<Privileges>, IPrivilegesServicecs
    {
        private readonly AppDbContext _context;
        private readonly IHostingEnvironment _webHostEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public PrivilegesService(AppDbContext context, IHostingEnvironment webHostEnvironment, IHttpContextAccessor httpContextAccessor) : base(context)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<(List<Privileges>,Mess)> GetPrivilegesAsync()
        {
            Mess  mess = new Mess();
            try
            {
                var privileges = await _context.Privileges.AsNoTracking().ToListAsync();
                return (BuildHierarchy(privileges), null);
            }
            catch (Exception ex)
            {
                mess.Status = 900;
                mess.Errors = ex.Message;
                return (null, mess);
            }
        }
        private List<Privileges> BuildHierarchy(List<Privileges> source)
        {
            var lookup = source.ToLookup(x => x.ParentId);
            var rootGroups = lookup.Contains(null) ? lookup[null].ToList() : source;

            foreach (var group in rootGroups)
            {
                AddChildGroups(group, lookup);
            }

            return rootGroups;
        }

        private void AddChildGroups(Privileges parent, ILookup<int?, Privileges> lookup)
        {
            parent.Children = lookup[parent.Id].ToList();

            foreach (var child in parent.Children)
            {
                AddChildGroups(child, lookup);
            }
        }
    }
}
