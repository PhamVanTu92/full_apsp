using AutoMapper;
using BackEndAPI.Models.BusinessPartners;
using BackEndAPI.Models.Other;
using BackEndAPI.Service.BusinessPartners;
using BackEndAPI.Service.Promotions;
using Gridify;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackEndAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentRuleController : Controller
    {
        private readonly IPaymentRuleService _iService;
        public PaymentRuleController(IPaymentRuleService iService)
        {
            _iService = iService;
        }
        //[Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> updatePaymentRule(int id, PaymentRuleView model)
        {
            if (model == null)
            {
                Mess mes = new Mess();
                mes.Status = 400;
                mes.Errors = "Dữ liệu trống";
                return BadRequest(new { mes.Status, mes.Errors });
            }
            var (items, mess) = await _iService.UpdateRuleAsync(id, model);
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
        //[Authorize]
        [HttpGet()]
        public async Task<IActionResult> getAll([FromQuery] GridifyQuery q)
        {

            var (items, mess,total) = await _iService.GetAllRuleAsync(q);
            if (mess != null)
            {
                return BadRequest(new { mess.Status, mess.Errors });
            }
             return Ok(new { items,total });
        }
    }
}
