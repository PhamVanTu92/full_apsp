using BackEndAPI.Models.BPGroups;
using BackEndAPI.Service.BPGroups;
using BackEndAPI.Service.Privile;
using Gridify;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace BackEndAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomerGroupController : ControllerBase
    {
        private readonly IBPGroupService _bpgroupService;
        private readonly IHostingEnvironment _hostingEnvironment;
        public CustomerGroupController(IBPGroupService bpgroupService, IHostingEnvironment hostingEnvironment)
        {
            _bpgroupService = bpgroupService;
            _hostingEnvironment = hostingEnvironment;
        }
        [PrivilegeRequirement("CustomerGroup.Create")]
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
            model.GroupType = "C";
            var (bpGroup ,mess)= await _bpgroupService.CreateGroup(model);
            if (mess != null)
            {
                return BadRequest(new { mess.Status, mess.Errors });
            }
            return Ok(bpGroup);
        }
        
        [PrivilegeRequirement("CustomerGroup.Edit")]
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

            model.ConditionCusGroups = null;
            model.GroupType = "C";
            var (bpGroup, mess) = await _bpgroupService.UpdateAsync(id, model);
            if (bpGroup == null)
            {
                int ErrorCode = mess.Status;
                string ErrorMessage = mess.Errors;
                return BadRequest(new { ErrorCode, ErrorMessage });
            }
            return Ok(bpGroup);
            
        }
        
        [PrivilegeRequirement("CustomerGroup.View")]
        [HttpGet]
        public async Task<IActionResult> getAll([FromQuery] GridifyQuery q,string? search ,int skip = 0, int limit = 30)
        {
            var (bpGroup,total) = await _bpgroupService.GetBPGroupAsync("C", skip, limit, search, q);
            if (bpGroup == null)
            {
                return NotFound();
            }
            return Ok(new { bpGroup, total, skip, limit });
        }
        
        [PrivilegeRequirement("CustomerGroup.View")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var (bpGroup, mess) = await _bpgroupService.GetBPGroupById(id);
            if (bpGroup == null)
            {
                return NotFound();
            }

            return Ok(bpGroup);
        }
        
        [PrivilegeRequirement("CustomerGroup.View")]
        [HttpGet("search/{search}")]
        public async Task<IActionResult> getAll(string search)
        {
            var bpGroup = await _bpgroupService.GetBPGroupAsync(search,"C");
            if (bpGroup == null)
            {
                return NotFound();
            }
            return Ok(bpGroup);
        }
        
        
        [PrivilegeRequirement("CustomerGroup.Delete")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (!await _bpgroupService.CanDeleteBPGroupAsync(id))
            {
                int ErrorCode = 400;
                string ErrorMessage = "Không thể xóa Nhóm khách hàng do đã tồn tại ở Nhóm khách hàng khác.";
                return BadRequest(new { ErrorCode, ErrorMessage });
            }

            var (bpGroup,mess) = await _bpgroupService.GetByIdAsync(id);
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
        
        
        [PrivilegeRequirement("CustomerGroup.Edit")]
        [HttpPost("{id}/customers")]
        public async Task<IActionResult> AddCustomer(int id, [FromBody] List<int> userIds)
        {
            var (group,mess) = await _bpgroupService.AddCustomerToGroup(id, userIds);
            if (mess != null) return BadRequest(mess);
            
            return Ok(group);
        }
        
        
        [PrivilegeRequirement("CustomerGroup.Delete")]
        [HttpDelete("{id}/customers")]
        public async Task<IActionResult> RemoveCustomer(int id, [FromBody] List<int> userIds)
        {
            var (group, mess) = await _bpgroupService.RemoveCustomerFromGroup(id, userIds);
            if (mess != null) return BadRequest(mess); 
            return Ok(group);
        }
        
        [PrivilegeRequirement("CustomerGroup.Edit")]
        [HttpPost("{id}/conds")]
        public async Task<IActionResult> AddCond(int id, [FromBody] List<ConditionCusGroup> cond)
        {
            var (group, mess) = await _bpgroupService.AddCondToGroup(id, cond);
            if (mess != null) return BadRequest(mess);
            
            return Ok(group);
        }
        
        [PrivilegeRequirement("CustomerGroup.Edit")]
        [HttpPut("{id}/conds")]
        public async Task<IActionResult> UpdateCond(int id ,[FromBody] List<ConditionCusGroup> conds)
        {
            var mess = await _bpgroupService.UpdateGroupCondtion(id, conds);
            if (mess != null) return BadRequest(mess);
            
            return Ok();
        }
        
        [PrivilegeRequirement("CustomerGroup.Edit")]
        [HttpDelete("{id}/conds")]
        public async Task<IActionResult> RemoveCond(int id,[FromBody] List<int> condIds)
        {
            var (group,mess) = await _bpgroupService.RemoveCondFromGroup(id,condIds );
            if (mess != null) return BadRequest(mess);
            
            return Ok(group);
        }
        
        [HttpPost("GetCusInConds")]
        public async Task<IActionResult> GetCusInConds([FromBody] OCRG cusInConds)
        {
            var (cus,mess) = await _bpgroupService.GetCustomerInCond(cusInConds );
            if (mess != null) return BadRequest(mess);
            return Ok(cus);
        }
        
    }
}
