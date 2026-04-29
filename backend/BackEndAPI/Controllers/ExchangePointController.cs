using BackEndAPI.Models.Other;
using BackEndAPI.Models.Promotion;
using BackEndAPI.Service.Account;
using BackEndAPI.Service.Privile;
using BackEndAPI.Service.Privilege;
using BackEndAPI.Service.Promotions;
using Gridify;
using Microsoft.AspNetCore.Mvc;

namespace BackEndAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExchangePointController : Controller
    {
        private readonly IExchangePointService _iService;

        public ExchangePointController(IExchangePointService iService)
        {
            _iService = iService;
        }
        [PrivilegeRequirement("SetupPointsVPKM.View")]
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] GridifyQuery query)
        {
            Mess mes = new Mess();
            var (items,total,mess) = await _iService.GetExchangePointAsync(query);
            if (mess != null)
            {
                mes.Status = 400;
                mes.Errors = mess.Errors;
                return BadRequest(mes);
            }
            return Ok(new { items, total });
        }

        [PrivilegeRequirement("SetupPointsVPKM.View")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            Mess mes = new Mess();
            var (items, mess) = await _iService.GetExchangePointByIdAsync(id);
            if (mess != null)
            {
                mes.Status = 400;
                mes.Errors = mess.Errors;
                return BadRequest(mes);
            }
            return Ok(new { items });
        }

        [PrivilegeRequirement("SetupPointsVPKM.Create")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ExchangePointCreateDto dto)
        {
            Mess mes = new Mess();
            var (items, mess) = await _iService.AddExchangePointAsync(dto);
            if (mess != null)
            {
                mes.Status = 400;
                mes.Errors = mess.Errors;
                return BadRequest(mes);
            }
            return Ok(new { items });
        }

        [PrivilegeRequirement("SetupPointsVPKM.Edit")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update([FromBody] ExchangePointUpdateDto dto)
        {
            Mess mes = new Mess();
            var (items, mess) = await _iService.UpdateExchangePointAsync(dto);
            if (mess != null)
            {
                mes.Status = 400;
                mes.Errors = mess.Errors;
                return BadRequest(mes);
            }
            return Ok(new { items });
        }
    }
}
