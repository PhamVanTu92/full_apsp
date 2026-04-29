using BackEndAPI.Models.Approval;
using BackEndAPI.Models.Other;
using BackEndAPI.Service.Approval;
using BackEndAPI.Service.Privile;
using Gridify;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;


namespace BackEndAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ApprovalTemplateController:Controller
    {
   
        private readonly IApprovalTemplateService _iService;
        private readonly IHostingEnvironment  _hostingEnvironment;
        public ApprovalTemplateController(IApprovalTemplateService iService, IHostingEnvironment hostingEnvironment)
        {
            _iService = iService;
            _hostingEnvironment = hostingEnvironment;
        }

        [HttpPost]
        [Route("add")]
        [PrivilegeRequirement("ApprovalTemplate.Create")]
        public async Task<IActionResult> Create(OWTM model)
        {
            if (model == null)
            {
                Mess mes = new Mess();
                mes.Status = 400;
                mes.Errors = "Dữ liệu trống";
                return BadRequest(new { mes.Status, mes.Errors });
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var (items, mess) = await _iService.CreateOWTMAsync(model);
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
        [HttpPut("{id}")]
        [PrivilegeRequirement("ApprovalTemplate.Update")]
        public async Task<IActionResult> update(int id, OWTM model)
        {
            if (model == null)
            {
                Mess mes = new Mess();
                mes.Status = 400;
                mes.Errors = "Dữ liệu trống";
                return BadRequest(new { mes.Status, mes.Errors });
            }
            var (items, mess) = await _iService.UpdateOWTMAsync(id, model);
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
        [HttpGet("{id}")]
        [PrivilegeRequirement("ApprovalTemplate.View")]
        public async Task<IActionResult> getById(int id)
        {
            var (items, mess) = await _iService.GetOWTMByIdAsync(id);
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
        [HttpGet("getall")]
        [PrivilegeRequirement("ApprovalTemplate.View")]
        public async Task<IActionResult> GetAllWithPagination([FromQuery] GridifyQuery q,int skip = 0, int limit = 30)
        {
            var search = HttpContext.Request.Query["search"];
            var (items, mess, total) = await _iService.GetAllOWTMAsync(skip, limit, search, q);
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

        [HttpDelete("{id}")]
        [PrivilegeRequirement("ApprovalTemplate.Delete")]
        public async Task<IActionResult> Delete(int id)
        {
            var (check, mes) = await _iService.DeleteOWTMAsync(id);
            if (!check)
            {
                return BadRequest(new { mes.Status, mes.Errors });
            }
            return Ok("Thành công");
        }
    }
}

