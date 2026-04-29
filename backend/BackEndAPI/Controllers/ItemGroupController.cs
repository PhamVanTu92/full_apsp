using BackEndAPI.Models.BusinessPartners;
using BackEndAPI.Models.ItemGroups;
using BackEndAPI.Service.BusinessPartners;
using BackEndAPI.Service.ItemGroups;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using BackEndAPI.Service.Privile;
using Gridify;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace BackEndAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ItemGroupController : ControllerBase
    {
        private readonly IItemGroupService _itemgroupService;
        private readonly IHostingEnvironment _hostingEnvironment;
        public ItemGroupController(IItemGroupService itemgroupService, IHostingEnvironment hostingEnvironment)
        {
            _itemgroupService = itemgroupService;
            _hostingEnvironment = hostingEnvironment;
        }
        [AllowAnonymous]
        [HttpPost]
        [Route("add")]
        [PrivilegeRequirement("ItemGroup.Create")]
        public async Task<IActionResult> Create([FromBody] OIBT model)
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
            var itemGroup = await _itemgroupService.CreateItemAsync(model);
            return Ok(itemGroup);
        }
        [AllowAnonymous]
        [HttpPut("{id}")]
        [PrivilegeRequirement("ItemGroup.Update")]
        public async Task<IActionResult> update(int id, [FromBody] OIBT model)
        {
            if (model == null)
                return BadRequest("data is null");
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var itemGroup = await _itemgroupService.UpdateItemGroupAsync(id, model);
            if (itemGroup == null)
            {
                return BadRequest();
            }
            return Ok(itemGroup);
        }
        [AllowAnonymous]
        [HttpGet]
        [PrivilegeRequirement("ItemGroup.View")]
        public async Task<IActionResult> getAll([FromQuery] GridifyQuery query, [FromQuery] string? search)
        {
            var (itemGroup, total) = await _itemgroupService.GetItemGroupAsync(query, search);
            return Ok(new { itemGroup, total, query.Page, query.PageSize });
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var itemGroup = await _itemgroupService.GetItemGroupByIdAsync(id);
            if (itemGroup == null)
            {
                return NotFound();
            }
            return Ok(itemGroup);
        }
        [AllowAnonymous]
        [HttpGet("search/{search}")]
        [PrivilegeRequirement("ItemGroup.View")]
        public async Task<IActionResult> getAll(string search)
        {
            var itemGroup = await _itemgroupService.GetItemGroupAsync(search);
            if (itemGroup == null)
            {
                return NotFound();
            }
            return Ok(itemGroup);
        }
        [AllowAnonymous]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var (bpGroup,mess) = await _itemgroupService.GetByIdAsync(id);
            if (bpGroup == null)
            {
                return NotFound();
            }
            var (check, mes)  = await _itemgroupService.DeleteAsync(id);
            if (!check)
            {
                return BadRequest(new { mes.Status, mes.Errors });
            }
            return Ok("Thành công");
        }
        [HttpPost("{id}/conds")]
        public async Task<IActionResult> AddCond(int id, [FromBody] List<ConditionItemGroup> cond)
        {
            var (group, mess) = await _itemgroupService.AddCondToGroup(id, cond);
            if (mess != null) return BadRequest(mess);
            
            return Ok(group);
        }
        
        [HttpPut("{id}/conds")]
        [PrivilegeRequirement("ItemGroup.Update")]
        public async Task<IActionResult> UpdateCond(int id ,[FromBody] List<ConditionItemGroup> conds)
        {
            var mess = await _itemgroupService.UpdateGroupCondtion(id, conds);
            if (mess != null) return BadRequest(mess);
            
            return Ok();
        }
        
        [HttpDelete("{id}/conds")]
        [PrivilegeRequirement("ItemGroup.Update")]
        public async Task<IActionResult> RemoveCond(int id,[FromBody] List<int> condIds)
        {
            var (group,mess) = await _itemgroupService.RemoveCondFromGroup(id,condIds );
            if (mess != null) return BadRequest(mess);
            
            return Ok(group);
        }
    }
}
