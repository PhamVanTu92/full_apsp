using AutoMapper;
using BackEndAPI.Models.BusinessPartners;
using BackEndAPI.Models.Other;
using BackEndAPI.Service;
using BackEndAPI.Service.BusinessPartners;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace BackEndAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TimezoneController : Controller
    {
        private readonly IService<TimeZones> _iService;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IMapper _mapper;
        public TimezoneController(IService<TimeZones> iService, IHostingEnvironment hostingEnvironment, IMapper mapper)
        {
            _iService = iService;
            _hostingEnvironment = hostingEnvironment;
            _mapper = mapper;
        }
        [AllowAnonymous]
        [HttpGet("getall")]
        public async Task<IActionResult> geAll()
        {
            var (bp,mess) = await _iService.GetAllAsync();
            if (bp == null)
            {
                return BadRequest(new {mess.Status,mess.Errors});
            }
            return Ok(bp);
        }
    }
}
