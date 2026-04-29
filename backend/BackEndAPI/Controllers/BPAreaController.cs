using BackEndAPI.Models.BusinessPartners;
using BackEndAPI.Service.ItemAreas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace BackEndAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BPAreaController : Controller
    { 
        private readonly IBPAreaService _bpAreaService;
        private readonly IHostingEnvironment _hostingEnvironment;
        public BPAreaController(IBPAreaService bpAreaService, IHostingEnvironment hostingEnvironment)
        {
            _bpAreaService = bpAreaService;
            _hostingEnvironment = hostingEnvironment;
        }
        [AllowAnonymous]
        [HttpPost]
        [Route("add")]
        public async Task<IActionResult> Create([FromBody] BPArea model)
        {
            if (model == null)
            {
                int ErrorCode = 204;
                string ErrorMessage = "Dữ liệu trống";
                return BadRequest(new { ErrorCode, ErrorMessage });
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            else
            {
                var (check, mess) = await _bpAreaService.BPAreaExistsAsync(model.ParentId);
                if (!check)
                {
                    int ErrorCode = 400;
                    string ErrorMessage = "Nhóm hàng hóa cha không tồn tại.";
                    return BadRequest(new { ErrorCode, ErrorMessage });
                }

            }
            var itemArea = await _bpAreaService.CreateBPAreaAsync(model);
            return Ok(itemArea);
        }
        [AllowAnonymous]
        [HttpPut("{id}")]
        public async Task<IActionResult> update(int id, [FromBody] BPArea model)
        {
            if (model == null)
                return BadRequest("data is null");
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            else
            {
                var (check, mes) = await _bpAreaService.BPAreaExistsAsync(model.ParentId);
                if (!check)
                {
                    int ErrorCode = 400;
                    string ErrorMessage = "Nhóm hàng hóa cha không tồn tại.";
                    return BadRequest(new { ErrorCode, ErrorMessage });
                }
                if (model.ParentId == model.Id)
                {
                    int ErrorCode = 400;
                    string ErrorMessage = "Nhóm hàng hóa cha trùng với nhóm hàng hiện tại";
                    return BadRequest(new { ErrorCode, ErrorMessage });
                }

            }
            var (itemArea, mess) = await _bpAreaService.UpdateBPAreaAsync(id, model);
            if (itemArea == null)
            {
                int ErrorCode = mess.Status;
                string ErrorMessage = mess.Errors;
                return BadRequest(new { ErrorCode, ErrorMessage });
            }
            return Ok(itemArea);
        }
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> getAll(int skip = 0, int limit =30)
        {
            var (itemArea,mess) = await _bpAreaService.GetBPAreaAsync();
            if (mess != null)
            {
                return BadRequest(new {mess.Status,mess.Errors});
            }
            int total = itemArea.Count;
            var bp = itemArea.Skip(skip)
            .Take(limit)
            .ToList();
            return Ok(new { itemArea, total, skip, limit });
        }
        [AllowAnonymous]
        [HttpGet("search/{search}")]
        public async Task<IActionResult> getAll(string search)
        {
            var (itemArea, mess) = await _bpAreaService.GetBPAreaAsync(search);
            if (mess != null)
            {
                return BadRequest(new { mess.Status, mess.Errors });
            }
            return Ok(itemArea);
        }
        [AllowAnonymous]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var (check, mes) = await _bpAreaService.CanDeleteBPAreaAsync(id);
            if (!check)
            {
                int ErrorCode = 400;
                string ErrorMessage = "Không thể xóa Nhóm hàng hóa do đã tồn tại ở Khu vực khác";
                return BadRequest(new { ErrorCode, ErrorMessage });
            }

            var (bpGroup, mess) = await _bpAreaService.GetByIdAsync(id);
            if (mess != null)
            {
                return NotFound();
            }
            var (check1, mes1) = await _bpAreaService.DeleteBPAreaAsync(id);
            if (!check1)
            {
                return BadRequest(new { mes1.Status, mes1.Errors });
            }
            return Ok("Thành công");
        }
    }
}
