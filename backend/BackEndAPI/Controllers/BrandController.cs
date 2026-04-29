using AutoMapper;
using BackEndAPI.Middleware;
using BackEndAPI.Models.ItemGroups;
using BackEndAPI.Models.Other;
using BackEndAPI.Service;
using BackEndAPI.Service.Brands;
using BackEndAPI.Service.Cache;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace BackEndAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BrandController : Controller
    {
        private readonly IBrandService _iService;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IMapper _mapper;
        private readonly IReferenceDataCache _cache;

        public BrandController(IBrandService iService, IHostingEnvironment hostingEnvironment, IMapper mapper,
            IReferenceDataCache cache)
        {
            _iService = iService;
            _hostingEnvironment = hostingEnvironment;
            _mapper = mapper;
            _cache = cache;
        }
        [Authorize]
        [HttpPost("add")]
        public async Task<IActionResult> CreateBrand(Brand model)
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
            var (items, mess) = await _iService.AddAsync(model);
            if (mess != null)
            {
                return BadRequest(new { mess.Status, mess.Errors });
            }
            // Invalidate cache + push SignalR event "ReferenceDataChanged" → client reload
            await _cache.InvalidateAsync(ReferenceEntities.Brand);
            if (items == null)
                return Ok(new { items });
            return Ok(items);
        }
        [AllowAnonymous]
        [HttpPost("sync")]
        public async Task<IActionResult> SyncBrand(List<Brand> model)
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
            var (items, mess) = await _iService.SyncBrandAsync(model);
            if (mess != null)
            {
                return BadRequest(new { mess.Status, mess.Errors });
            }
            await _cache.InvalidateAsync(ReferenceEntities.Brand);
            if (items == null)
                return Ok(new { items });
            return Ok(items);
        }
        [AllowAnonymous]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBrand(int id, Brand model)
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
            var (items, mess) = await _iService.UpdateAsync(id, model);
            if (mess != null)
            {
                return BadRequest(new { mess.Status, mess.Errors });
            }
            await _cache.InvalidateAsync(ReferenceEntities.Brand);
            return Ok(items);
        }
        [AllowAnonymous]
        [HttpGet("search/{search}")]

        public async Task<IActionResult> getBySearch(string search)
        {
            var (items, mess) = await _iService.GetAsync(null, "Name", search);
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
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<IActionResult> getbyid(int id)
        {
            var (items, mess) = await _iService.GetByIdAsync(id);
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
        /// <summary>
        /// List toàn bộ brand. Endpoint reference data — cache server-side + ETag.
        /// Client cần lưu ETag và gửi If-None-Match cho lần sau → có thể nhận 304 Not Modified.
        /// SignalR sẽ push event "ReferenceDataChanged" khi admin update brand → client invalidate.
        /// </summary>
        [AllowAnonymous]
        [HttpGet("getall")]
        [CacheableReferenceData(ReferenceEntities.Brand)]
        public async Task<IActionResult> getall()
        {
            var items = await _cache.GetOrSetAsync(ReferenceEntities.Brand, async () =>
            {
                var (data, mess) = await _iService.GetAllAsync();
                if (mess != null) throw new Exception(mess.Errors);
                return data ?? new List<Brand>();
            });

            return Ok(items);
        }
        [AllowAnonymous]
        [HttpGet("GetPagination")]
        public async Task<IActionResult> getallPage(int skip = 0, int limit = 30)
        {
            var (items, total, mess) = await _iService.GetAllWithPaginationAsync(skip, limit, null);
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
