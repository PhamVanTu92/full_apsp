using BackEndAPI.Models.Account;
using BackEndAPI.Service.Account;
using BackEndAPI.Service.Privile;
using BackEndAPI.Service.Privilege;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackEndAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoleController : Controller
    {
        private readonly IRoleService _iService;
        public RoleController(IRoleService iService)
        {
            _iService = iService;
        }
        [PrivilegeRequirement("Role.View")]
        [HttpGet("getRole")]
        public async Task<IActionResult> GetRole()
        {
            var (item, mess) = await _iService.GetRole();
            if (mess != null)
            {
                return BadRequest(new { mess.Status, mess.Errors });
            }
            return Ok(item);
        }
        [PrivilegeRequirement("Role.View")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetRoleId(int id)
        {
            var (item, mess) = await _iService.GetRoleClaim(id);
            if (mess != null)
            {
                return BadRequest(new { mess.Status, mess.Errors });
            }
            return Ok(item);
        }
        [PrivilegeRequirement("Role.Create")]
        [HttpPost("add")]
        public async Task<IActionResult> AddRole(AppRole appRole)
        {
             var (item, mess) = await _iService.AddRole(appRole);
            if (mess != null)
            {
                return BadRequest(new { mess.Status, mess.Errors });
            }
            return Ok(item);
        }
        [PrivilegeRequirement("Role.Edit")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRole(int id, AppRole appRole)
        {
            var (item, mess) = await _iService.UpdateRole(id, appRole);
            if (mess != null)
            {
                return BadRequest(new { mess.Status, mess.Errors });
            }
            return Ok(item);
        }
        [PrivilegeRequirement("Role.Delete")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRole(int id)
        {
            var (check, mess) = await _iService.DeleteRole(id);
            if (check == false)
            {
                return BadRequest(new { mess.Status, mess.Errors });
            }
            return Ok("Thành công");
        }
    }
}
