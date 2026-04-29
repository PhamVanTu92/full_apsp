using AutoMapper;
using BackEndAPI.Middleware;
using BackEndAPI.Models.BusinessPartners;
using BackEndAPI.Models.Other;
using BackEndAPI.Service;
using BackEndAPI.Service.Cache;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace BackEndAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BPSizeController : Controller
    {
        private readonly IService<BPSize> _iService;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IMapper _mapper;
        private readonly IReferenceDataCache _cache;

        public BPSizeController(IService<BPSize> iService, IHostingEnvironment hostingEnvironment, IMapper mapper,
            IReferenceDataCache cache)
        {
            _iService = iService;
            _hostingEnvironment = hostingEnvironment;
            _mapper = mapper;
            _cache = cache;
        }
        [AllowAnonymous]
        [HttpPost("add")]
        public async Task<IActionResult> CreateGoldBrandInfor(BPSize model)
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
            await _cache.InvalidateAsync(ReferenceEntities.BPSize);
            return Ok(items);
        }
        [AllowAnonymous]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateGoldBrandInfor(int id, BPSize model)
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
            await _cache.InvalidateAsync(ReferenceEntities.BPSize);
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
            if (items == null)
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
            if (items == null)
            {
                return Ok(new { items });
            }
            return Ok(items);
        }
        [AllowAnonymous]
        [HttpGet("getall")]
        [CacheableReferenceData(ReferenceEntities.BPSize)]
        public async Task<IActionResult> getall()
        {
            var items = await _cache.GetOrSetAsync(ReferenceEntities.BPSize, async () =>
            {
                var (data, mess) = await _iService.GetAllAsync();
                if (mess != null) throw new Exception(mess.Errors);
                return data ?? Enumerable.Empty<BPSize>();
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
            if (items == null)
            {
                return Ok(new { items });
            }
            return Ok(new { items, total, skip, limit });
        }
    }
}
