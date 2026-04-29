using AutoMapper;
using BackEndAPI.Models.ARInvoice;
using BackEndAPI.Models.ItemGroups;
using BackEndAPI.Models.Unit;
using BackEndAPI.Service.Document;
using BackEndAPI.Service.Unit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace BackEndAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OUOMController : ControllerBase
    {
        private readonly IOUOMService _ouomService;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IMapper _mapper;
        public OUOMController(IOUOMService ouomService, IHostingEnvironment hostingEnvironment, IMapper mapper)
        {
            _ouomService = ouomService;
            _hostingEnvironment = hostingEnvironment;
            _mapper = mapper;
        }
        [AllowAnonymous]
        [HttpPost]
        [Route("add")]
        public async Task<IActionResult> Create([FromBody] OUOM model)
        {
            int ErrorCode = 0;
            string ErrorMessage = "";
            if (model == null)
            {
                ErrorCode = 204;
                ErrorMessage = "Dữ liệu trống";
                return BadRequest(new { ErrorCode, ErrorMessage });
            }
            if (!ModelState.IsValid)
            {
                ErrorCode = 204;
                ErrorMessage = String.Join(Environment.NewLine, ModelState.Values.SelectMany(v => v.Errors)
                                                           .Select(v => v.ErrorMessage + " " + v.Exception));
                return BadRequest(new { ErrorCode, ErrorMessage });
            }
            var ouom = await _ouomService.AddAsync(model);
            return Ok(ouom);
        }
        [AllowAnonymous]
        [HttpPost]
        [Route("sync")]
        public async Task<IActionResult> Sync(List<Packing> model)
        {
            int ErrorCode = 0;
            string ErrorMessage = "";
            if (model == null)
            {
                ErrorCode = 204;
                ErrorMessage = "Dữ liệu trống";
                return BadRequest(new { ErrorCode, ErrorMessage });
            }
            if (!ModelState.IsValid)
            {
                ErrorCode = 204;
                ErrorMessage = String.Join(Environment.NewLine, ModelState.Values.SelectMany(v => v.Errors)
                                                           .Select(v => v.ErrorMessage + " " + v.Exception));
                return BadRequest(new { ErrorCode, ErrorMessage });
            }
            var ouom = await _ouomService.SyncOUOMAsync(model);
            return Ok(ouom);
        }
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetAllWithPagination(int skip = 0, int limit = 30)
        {
            var (items, total) = await _ouomService.GetAllWithPaginationAsync(skip, limit);
            return Ok(new { items, total, skip, limit });
        }
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<IActionResult> getById(int id)
        {
            var (item, mess) = await _ouomService.GetByIdAsync(id);
            if (item == null)
            {
                return NotFound();
            }
            return Ok(item);
        }
        [AllowAnonymous]
        [HttpPut("{id}")]
        public async Task<IActionResult> update(int id, [FromBody] OUOM model)
        {
            if (model == null)
                return BadRequest("data is null");
            var (itemGroup,mess) = await _ouomService.UpdateAsync(id, model);
            if (itemGroup == null)
            {
                int ErrorCode = mess.Status;
                string ErrorMessage = mess.Errors;
                return BadRequest(new { ErrorCode, ErrorMessage });
            }
            return Ok(itemGroup);
        }
        [AllowAnonymous]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var (check, mes) = await _ouomService.DeleteOUOMAsync(id);
            if (!check)
            {
                return BadRequest(new { mes.Status, mes.Errors });
            }
            return Ok("Thành công");
        }
    }
}
