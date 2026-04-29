using BackEndAPI.Data;
using BackEndAPI.Models.Other;
using Microsoft.EntityFrameworkCore;
using Function.Address;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;
using Microsoft.Extensions.Logging.Abstractions;

namespace BackEndAPI.Service.Areas
{
    public class AreaService:Service<AreaService>,IAreaService
    {
        private readonly AppDbContext _context;
        private readonly IHostingEnvironment _webHostEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public AreaService(AppDbContext context, IHostingEnvironment webHostEnvironment, IHttpContextAccessor httpContextAccessor) : base(context)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<(IEnumerable<Location>, Mess)> GetAArea(int id, string search)
        {
            //Mess mess = new Mess();
            //try
            //{

            //    var locations = LocationCollection.getArea(id, search).ToList();
            //    return (locations, null);
            //}
            //catch (Exception ex)
            //{
            //    mess.Status = 900;
            //    mess.Errors = ex.Message;
            //    return (null, mess);
            //}
            Mess mess = new Mess();
            try
            {

                var locations = LocationCollection.getLocationNew(id, search).ToList();
                return (locations, null);
            }
            catch (Exception ex)
            {
                mess.Status = 900;
                mess.Errors = ex.Message;
                return (null, mess);
            }
        }

        public async Task<(IEnumerable<Location>, Mess)> GetALocation(string search)
        {
            //Mess mess = new Mess();
            //try
            //{

            //    var locations = LocationCollection.getArea(search).ToList();
            //    return (locations, null);
            //}
            //catch (Exception ex)
            //{
            //    mess.Status = 900;
            //    mess.Errors = ex.Message;
            //    return (null, mess);
            //}
            Mess mess = new Mess();
            try
            {

                var locations = LocationCollection.getLocationNew(search).ToList();
                return (locations, null);
            }
            catch (Exception ex)
            {
                mess.Status = 900;
                mess.Errors = ex.Message;
                return (null, mess);
            }
        }

        public async Task<(IEnumerable<Location>, Mess)> GetNewArea(string search)
        {
            Mess mess = new Mess();
            try
            {

                var locations = LocationCollection.getLocationNew(search).ToList();
                return (locations, null);
            }
            catch (Exception ex)
            {
                mess.Status = 900;
                mess.Errors = ex.Message;
                return (null, mess);
            }
        }

        public async Task<(IEnumerable<Location>, Mess)> GetFArea(int id, string search)
        {
            //Mess mess = new Mess();
            //try
            //{

            //    var locations = LocationCollection.getLocation(id,search).ToList();
            //    return (locations, null);
            //}
            //catch (Exception ex)
            //{
            //    mess.Status = 900;
            //    mess.Errors = ex.Message;
            //    return (null, mess);
            //}
            Mess mess = new Mess();
            try
            {

                var locations = LocationCollection.getLocationNew(id, search).ToList();
                return (locations, null);
            }
            catch (Exception ex)
            {
                mess.Status = 900;
                mess.Errors = ex.Message;
                return (null, mess);
            }
        }
        public async Task<(IEnumerable<Location>, Mess)> GetFLocation(string search)
        {
            //Mess mess = new Mess();
            //try
            //{

            //    var locations = LocationCollection.getLocation(search).ToList();
            //    return (locations, null);
            //}
            //catch (Exception ex)
            //{
            //    mess.Status = 900;
            //    mess.Errors = ex.Message;
            //    return (null, mess);
            //}
            Mess mess = new Mess();
            try
            {

                var locations = LocationCollection.getLocationNew(search).ToList();
                return (locations, null);
            }
            catch (Exception ex)
            {
                mess.Status = 900;
                mess.Errors = ex.Message;
                return (null, mess);
            }
        }

        public async Task<(IEnumerable<Location>, Mess)> GetNewArea(int id, string search)
        {
            Mess mess = new Mess();
            try
            {

                var locations = LocationCollection.getLocationNew(id, search).ToList();
                return (locations, null);
            }
            catch (Exception ex)
            {
                mess.Status = 900;
                mess.Errors = ex.Message;
                return (null, mess);
            }
        }
    }
}
