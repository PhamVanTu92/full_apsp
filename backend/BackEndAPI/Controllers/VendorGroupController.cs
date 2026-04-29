using BackEndAPI.Models.BPGroups;
using BackEndAPI.Service.BPGroups;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace BackEndAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VendorGroupController : ControllerBase
    {
        private readonly IBPGroupService _bpgroupService;
        private readonly IHostingEnvironment _hostingEnvironment;
        public VendorGroupController(IBPGroupService bpgroupService, IHostingEnvironment hostingEnvironment)
        {
            _bpgroupService = bpgroupService;
            _hostingEnvironment = hostingEnvironment;
        }
        [AllowAnonymous]
        [HttpPost]
        [Route("add")]
        public async Task<IActionResult> Create([FromBody] OCRG model)
        {
            if (model == null)
                return BadRequest("data is null");
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            else
            {
                if (!await _bpgroupService.BPGroupExistsAsync(model.ParentId))
                {
                    int ErrorCode = 400;
                    string ErrorMessage = "Nhóm hàng hóa cha không tồn tại.";
                    return BadRequest(new { ErrorCode, ErrorMessage });
                }
            }
            model.GroupType = "V";
            var bpGroup = await _bpgroupService.AddAsync(model);
            return Ok(bpGroup);
        }
        [AllowAnonymous]
        [HttpPut("{id}")]
        public async Task<IActionResult> update(int id, [FromBody] OCRG model)
        {
            if (model == null)
                return BadRequest("data is null");
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            else
            {
                if (!await _bpgroupService.BPGroupExistsAsync(model.ParentId))
                {
                    int ErrorCode = 400;
                    string ErrorMessage = "Nhóm khách hàng cha không tồn tại.";
                    return BadRequest(new { ErrorCode, ErrorMessage });
                }
                if (model.ParentId == model.Id)
                {
                    int ErrorCode = 400;
                    string ErrorMessage = "Nhóm hàng hóa cha trùng với nhóm hàng hiện tại";
                    return BadRequest(new { ErrorCode, ErrorMessage });
                }

            }
            model.GroupType = "V";
            var (bpGroup,mess) = await _bpgroupService.UpdateAsync(id, model);
            if (bpGroup == null)
            {
                int ErrorCode = mess.Status;
                string ErrorMessage = mess.Errors;
                return BadRequest(new { ErrorCode, ErrorMessage });
            }
            return Ok(bpGroup);
        }
        // [AllowAnonymous]
        // [HttpGet]
        // public async Task<IActionResult> getAll(int skip, int limit)
        // {
        //     var bpGroup = await _bpgroupService.GetBPGroupAsync("V");
        //     if (bpGroup == null)
        //     {
        //         return NotFound();
        //     }
        //     int total = bpGroup.Count;
        //     var bp = bpGroup.Skip(skip)
        //     .Take(limit)
        //     .ToList();
        //     return Ok(new { bpGroup, total, skip, limit });
        // }
        [AllowAnonymous]
        [HttpGet("search/{search}")]
        public async Task<IActionResult> getAll(string search)
        {
            var bpGroup = await _bpgroupService.GetBPGroupAsync(search, "V");
            if (bpGroup == null)
            {
                return NotFound();
            }
            return Ok(bpGroup);
        }
        [AllowAnonymous]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (!await _bpgroupService.CanDeleteBPGroupAsync(id))
            {
                int ErrorCode = 400;
                string ErrorMessage = "Không thể xóa Nhóm khách hàng do đã tồn tại ở Nhóm khách hàng khác.";
                return BadRequest(new { ErrorCode, ErrorMessage });
            }

            var (bpGroup, mess) = await _bpgroupService.GetByIdAsync(id);
            if (bpGroup == null)
            {
                return NotFound();
            }
            var (check, mes) = await _bpgroupService.DeleteAsync(id);
            if (!check)
            {
                return BadRequest(new { mes.Status, mes.Errors });
            }
            return Ok("Thành công");
        }
    }
}
