using AutoMapper;
using BackEndAPI.Models.Other;
using BackEndAPI.Service;
using BackEndAPI.Service.Privilege;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace BackEndAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PrivilegesController : Controller
    {
        private readonly IPrivilegesServicecs _iService;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IMapper _mapper;
        public PrivilegesController(IPrivilegesServicecs iService, IHostingEnvironment hostingEnvironment, IMapper mapper)
        {
            _iService = iService;
            _hostingEnvironment = hostingEnvironment;
            _mapper = mapper;
        }
        [AllowAnonymous]
        [HttpGet("getall")]
        public async Task<IActionResult> getall()
        {
            var (privileges, mess) = await _iService.GetPrivilegesAsync();
            if (privileges == null)
            {
                return BadRequest(new { mess.Status, mess.Errors });
            }
            return Ok(privileges);
        }
    }
}
