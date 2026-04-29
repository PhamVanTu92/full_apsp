using AutoMapper;
using BackEndAPI.Models.ItemGroups;
using BackEndAPI.Models.Other;
using BackEndAPI.Service;
using BackEndAPI.Service.Brands;
using BackEndAPI.Service.Privile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Pqc.Crypto.Lms;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace BackEndAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ItemTypeController : Controller
    {
        private readonly IItemTypeService _iService;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IMapper _mapper;

        public ItemTypeController(IItemTypeService iService, IHostingEnvironment hostingEnvironment, IMapper mapper)
        {
            _iService = iService;
            _hostingEnvironment = hostingEnvironment;
            _mapper = mapper;
        }

        [PrivilegeRequirement("ItemType.Create")]
        [HttpPost("add")]
        public async Task<IActionResult> CreateBrand(ItemType model)
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

            return Ok(items);
        }

        [AllowAnonymous]
        [HttpPost("sync")]
        public async Task<IActionResult> SyncBrand(List<ItemType> model)
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

            var (items, mess) = await _iService.SyncItemTypeAsync(model);
            if (mess != null)
            {
                return BadRequest(new { mess.Status, mess.Errors });
            }

            return Ok(items);
        }

        [PrivilegeRequirement("ItemType.Edit")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBrand(int id, ItemType model)
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

            return Ok(items);
        }

        [PrivilegeRequirement("ItemType.View")]
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

            return Ok(new { items });
        }

        [PrivilegeRequirement("ItemType.View")]
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

            return Ok(new { items });
        }

        [PrivilegeRequirement("ItemType.View")]
        [HttpGet("getall")]
        public async Task<IActionResult> getall()
        {
            var (items, mess) = await _iService.GetAllAsync();
            if (mess != null)
            {
                return BadRequest(new { mess.Status, mess.Errors });
            }

            if (items == null || items.Count() == 0)
            {
                return Ok(new { items });
            }

            return Ok(new { items });
        }

        [AllowAnonymous]
        [HttpGet("bypass")]
        public async Task<IActionResult> getallbypass()
        {
            var (items, mess) = await _iService.GetAllAsync();
            if (mess != null)
            {
                return BadRequest(new { mess.Status, mess.Errors });
            }

            if (items == null || items.Count() == 0)
            {
                return Ok(new { items });
            }

            return Ok(new { items });
        }

        [PrivilegeRequirement("ItemType.View")]
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