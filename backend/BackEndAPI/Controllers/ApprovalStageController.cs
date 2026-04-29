using BackEndAPI.Models.Approval;
using BackEndAPI.Models.BusinessPartners;
using BackEndAPI.Models.Other;
using BackEndAPI.Service.Account;
using BackEndAPI.Service.Approval;
using BackEndAPI.Service.Privile;
using BackEndAPI.Service.Privilege;
using Gridify;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace BackEndAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ApprovalStageController : Controller
    {
        private readonly IApprovalStageService _iService;
        private readonly IHostingEnvironment _hostingEnvironment;
        public ApprovalStageController(IApprovalStageService iService, IHostingEnvironment hostingEnvironment)
        {
            _iService = iService;
            _hostingEnvironment = hostingEnvironment;
        }
        [AllowAnonymous]
        [HttpPost]
        [Route("add")]
        [PrivilegeRequirement("ApprovalStage.Create")]
        public async Task<IActionResult> Create(OWST model)
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
            var (items, mess) = await _iService.CreateOWSTAsync(model);
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
        [HttpPut("{id}")]
        [PrivilegeRequirement("ApprovalStage.Update")]
        public async Task<IActionResult> update(int id, OWST model)
        {
            if (model == null)
            {
                Mess mes = new Mess();
                mes.Status = 400;
                mes.Errors = "Dữ liệu trống";
                return BadRequest(new { mes.Status, mes.Errors });
            }
            var (items, mess) = await _iService.UpdateOWSTAsync(id, model);
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
        [PrivilegeRequirement("ApprovalStage.View")]
        public async Task<IActionResult> getById(int id)
        {
            var (items, mess) = await _iService.GetOWSTByIdAsync(id);
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
        [HttpGet]
        [PrivilegeRequirement("ApprovalStage.View")]
        public async Task<IActionResult> GetAllWithPagination([FromQuery] GridifyQuery q, int skip = 0, int limit = 30)
        {
            var search = HttpContext.Request.Query["search"];
            var (items, mess, total) = await _iService.GetAllOWSTAsync(skip, limit, search, q);
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

        [AllowAnonymous]
        [HttpDelete("{id}")]
        [PrivilegeRequirement("ApprovalStage.Delete")]
        public async Task<IActionResult> Delete(int id)
        {
            var (check, mes) = await _iService.DeleteOWSTAsync(id);
            if (!check)
            {
                return BadRequest(new { mes.Status, mes.Errors });
            }
            return Ok("Thành công");
        }
    }
}
