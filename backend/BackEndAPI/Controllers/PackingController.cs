using AutoMapper;
using BackEndAPI.Middleware;
using BackEndAPI.Models.ItemGroups;
using BackEndAPI.Models.Other;
using BackEndAPI.Service;
using BackEndAPI.Service.Cache;
using BackEndAPI.Service.Privile;
using BackEndAPI.Service.Unit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace BackEndAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PackingController : Controller
    {
        private readonly IPackingService _packingService;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IMapper _mapper;
        private readonly IReferenceDataCache _cache;

        public PackingController(IPackingService packingService, IHostingEnvironment hostingEnvironment, IMapper mapper,
            IReferenceDataCache cache)
        {
            _packingService = packingService;
            _hostingEnvironment = hostingEnvironment;
            _mapper = mapper;
            _cache = cache;
        }
        //[PrivilegeRequirement("Packing.Create")]
        [HttpPost("add")]
        public async Task<IActionResult> CreatePacking(Packing model)
        {
            Mess mes = new Mess();
            if (model == null)
            {
                mes.Status = 400;
                mes.Errors = "Dữ liệu trống";
                return BadRequest(mes);
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var (items, mess) = await _packingService.AddAsync(model);
            if (mess != null)
            {
                return BadRequest(new { mess.Status, mess.Errors });
            }
            await _cache.InvalidateAsync(ReferenceEntities.Packing);
            return Ok(items);
        }
        [PrivilegeRequirement("Packing.Sync")]
        [HttpPost("sync")]
        public async Task<IActionResult> SyncPacking(List<Packing> model)
        {
            Mess mes = new Mess();
            if (model == null)
            {
                mes.Status = 400;
                mes.Errors = "Dữ liệu trống";
                return BadRequest(mes);
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var (items, mess) = await _packingService.SyncPackingAsync(model);
            if (mess != null)
            {
                return BadRequest(new { mess.Status, mess.Errors });
            }
            await _cache.InvalidateAsync(ReferenceEntities.Packing);
            return Ok(items);
        }
        [PrivilegeRequirement("Packing.Edit")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBrand(int id, Packing model)
        {
            Mess mes = new Mess();
            if (model == null)
            {
                mes.Status = 400;
                mes.Errors = "Dữ liệu trống";
                return BadRequest(mes);
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var (items, mess) = await _packingService.UpdateAsync(id, model);
            if (mess != null)
            {
                return BadRequest(new { mess.Status, mess.Errors });
            }
            await _cache.InvalidateAsync(ReferenceEntities.Packing);
            return Ok(items);
        }
        [PrivilegeRequirement("Packing.View")]
        [HttpGet("search/{search}")]

        public async Task<IActionResult> getBySearch(string search)
        {
            var (items, mess) = await _packingService.GetAsync(null, "Name", search);
            if (mess != null)
            {
                return BadRequest(new { mess.Status, mess.Errors });
            }
            if (items == null || items.Count() == 0)
            {
                return Ok(new { items });
            }
            return Ok(items);
        }
        [PrivilegeRequirement("Packing.View")]
        [HttpGet("{id}")]
        public async Task<IActionResult> getbyid(int id)
        {
            var (items, mess) = await _packingService.GetByIdAsync(id);
            if (mess != null)
            {
                return BadRequest(new { mess.Status, mess.Errors });
            }
            if (items == null )
            {
                return Ok(new { items });
            }
            return Ok(items);
        }
        [PrivilegeRequirement("Packing.View")]
        [HttpGet("getall")]
        [CacheableReferenceData(ReferenceEntities.Packing)]
        public async Task<IActionResult> getall()
        {
            var items = await _cache.GetOrSetAsync(ReferenceEntities.Packing, async () =>
            {
                var (data, mess) = await _packingService.GetAllAsync();
                if (mess != null) throw new Exception(mess.Errors);
                return data ?? Enumerable.Empty<Packing>();
            });
            return Ok(items);
        }
        [PrivilegeRequirement("Packing.View")]
        [HttpGet("GetPagination")]
        public async Task<IActionResult> getallPage(int skip = 0, int limit = 30)
        {
            var (items, total, mess) = await _packingService.GetAllWithPaginationAsync(skip, limit, null);
            if (mess != null)
            {
                return BadRequest(new { mess.Status, mess.Errors });
            }
            if (items == null || items.Count() == 0)
            {
                return Ok(new { items });
            }
            return Ok(new { items, total, skip, limit });
        }
    }
}
