using BackEndAPI.Models.Other;
using BackEndAPI.Models.Promotion;
using BackEndAPI.Service.Privile;
using BackEndAPI.Service.Promotions;
using Gridify;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace BackEndAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PointSetupController : Controller
    {
        private readonly IPointSetupService _service;

        public PointSetupController(IPointSetupService service)
        {
            _service = service;
        }
        [PrivilegeRequirement("SetupPointsPurchase.View")]
        [HttpGet("{id}")]
        public async Task<ActionResult<PointSetupViewDto>> GetById(int id)
        {
            Mess mes = new Mess();
            var (items, mess) = await _service.GetByIdAsync(id);
            if (mess != null)
            {
                mes.Status = 400;
                mes.Errors = mess.Errors;
                return BadRequest(mes);
            }
            return Ok(new { items });
        }
        [PrivilegeRequirement("SetupPointsPurchase.View")]
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] GridifyQuery query)
        {

            Mess mes = new Mess();
            var (items, total,mess) = await _service.GetAllAsync(query);
            if (mess != null)
            {
                mes.Status = 400;
                mes.Errors = mess.Errors;
                return BadRequest(mes);
            }
            return Ok(new { items , total});
        }
        [PrivilegeRequirement("SetupPointsPurchase.Create")]
        [HttpPost]
        public async Task<ActionResult<PointSetupViewDto>> Create(PointSetupCreateDto dto)
        {
            Mess mes = new Mess();
            var (items, mess) = await _service.CreateAsync(dto);
            if (mess != null)
            {
                mes.Status = 400;
                mes.Errors = mess.Errors;
                return BadRequest(mes);
            }
            return Ok(new { items});
        }
        [PrivilegeRequirement("SetupPointsPurchase.Edit")]
        [HttpPut("{id}")]
        public async Task<ActionResult> Update( PointSetupUpdateDto dto)
        {
            Mess mes = new Mess();
            var (items, mess) = await _service.UpdateAsync(dto);
            if (mess != null)
            {
                mes.Status = 400;
                mes.Errors = mess.Errors;
                return BadRequest(mes);
            }
            return Ok(new { items });
        }
        [HttpPost("getPoints")]
        public async Task<IActionResult> GetAllPoints([FromBody] CalculatorPoint cal)
        {

            Mess mes = new Mess();
            var items = await _service.CalculatePointCheck(cal);
            return Ok(new { items });
        }
    }
}
