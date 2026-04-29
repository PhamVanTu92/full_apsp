using BackEndAPI.Middleware;
using BackEndAPI.Models.Account;
using BackEndAPI.Service.Account;
using BackEndAPI.Service.Cache;
using BackEndAPI.Service.Privile;
using BackEndAPI.Service.Privilege;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BackEndAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AppSettingController : Controller
    {
        private readonly IAccountService _iService;
        private readonly IReferenceDataCache _cache;

        public AppSettingController(IAccountService iService, IReferenceDataCache cache)
        {
            _iService = iService;
            _cache = cache;
        }

        [AllowAnonymous]
        [HttpPut()]
        [PrivilegeRequirement("Author")]
        public async Task<IActionResult> Update(List<AppSetting> app)
        {
            var (item, mess) = await _iService.UpdateAppSetting(app);
            if (mess != null)
            {
                return BadRequest(mess);
            }
            await _cache.InvalidateAsync(ReferenceEntities.AppSetting);
            return Ok(new { item });
        }

        [HttpGet()]
        [AllowAnonymous]
        [PrivilegeRequirement("Author")]
        [CacheableReferenceData(ReferenceEntities.AppSetting)]
        public async Task<IActionResult> getById()
        {
            var item = await _cache.GetOrSetAsync(ReferenceEntities.AppSetting, async () =>
            {
                var (data, mess) = await _iService.GetAppSetting();
                if (mess != null) throw new Exception(mess.Errors);
                return data;
            });
            return Ok(new { item });
        }
    }
}
