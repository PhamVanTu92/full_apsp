using AutoMapper;
using BackEndAPI.Models.ItemMasterData;
using BackEndAPI.Models.Other;
using BackEndAPI.Models.Promotion;
using BackEndAPI.Models.Unit;
using BackEndAPI.Service.Promotions;
using BackEndAPI.Service.Unit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Security.Claims;
using BackEndAPI.Service.Privile;
using Gridify;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace BackEndAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PromotionController : ControllerBase
    {
        private readonly IPromotionService _promotionService;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IMapper _mapper;
        public PromotionController(IPromotionService promotionService, IHostingEnvironment hostingEnvironment, IMapper mapper)
        {
            _promotionService = promotionService;
            _hostingEnvironment = hostingEnvironment;
            _mapper = mapper;
        }
        [PrivilegeRequirement("PromotionProgram.Create")]
        [HttpPost]
        [Route("add")]
        public async Task<IActionResult> Create([FromBody] Promotion model)
        {
            Mess mes = new Mess();
            if (model == null)
            {
                mes.Status = 204;
                mes.Errors = "Dữ liệu trống";
                return BadRequest(new { mes.Status, mes.Errors });
            }
            if (!ModelState.IsValid)
            {
                mes.Status = 204;
                mes.Errors = String.Join(Environment.NewLine, ModelState.Values.SelectMany(v => v.Errors)
                                                           .Select(v => v.ErrorMessage + " " + v.Exception));
                return BadRequest(new { mes.Status, mes.Errors });
            }
            var (ouom,mess )= await _promotionService.AddPromotionAsync(model);
            if (ouom == null)
            {
                return BadRequest(new { mess.Status, mess.Errors });
            }
            return Ok(ouom);
        }
        [PrivilegeRequirement("PromotionProgram.View")]
        [HttpGet]
        public async Task<IActionResult> GetAllWithPagination([FromQuery] string? search,[FromQuery] GridifyQuery q, int skip = 0, int limit = 30)
        {
            var claims = User.Identity as ClaimsIdentity;
            if (claims == null)
            {
                return Unauthorized();
            }

            var userId = claims.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var ok = Int32.TryParse(userId, out int intUserId);
            if (!ok || intUserId == 0)
            {
                return Unauthorized();
            }
            
            var (items, total,mess) = await _promotionService.GetPromotionAsync(skip, limit, search,q, intUserId);
            if(mess != null)
            {
                return BadRequest(new { mess.Status, mess.Errors });
            }
            return Ok(new { items, total, skip, limit });
        }
        [PrivilegeRequirement("PromotionProgram.View")]
        [HttpGet("{id}")]
        public async Task<IActionResult> getById(int id)
        {
            var (item, mess) = await _promotionService.GetPromotionByIdAsync(id);
            if (mess != null)
            {
                return BadRequest(new { mess.Status, mess.Errors });
            }
            return Ok(item);
        }
        [PrivilegeRequirement("PromotionProgram.View")]
        [HttpGet("search/{search}")]
        public async Task<IActionResult> getSearch(string search, int skip, int limit)
        {
            var (items,total, mess) = await _promotionService.GetSearchPromotionAsync(skip, limit,search);
            if (mess != null)
            {
                return BadRequest(new { mess.Status, mess.Errors });
            }
            return Ok(new { items, total, skip, limit });
        }
        [PrivilegeRequirement("PromotionProgram.Edit")]
        [HttpPut("{id}")]
        public async Task<IActionResult> update(int id, [FromBody] Promotion model)
        {
            if (model == null)
                return BadRequest("data is null");
            var (itemGroup,mess) = await _promotionService.UpdatePromotionAsync(id, model);
            if (itemGroup == null)
            {
                return BadRequest(new { mess.Status, mess.Errors });
            }
            return Ok(itemGroup);
        }
        [PrivilegeRequirement("PromotionProgram.Delete")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            int ErrorCode = 400;
            string ErrorMessage = "Không chương trình khuyến mại";
            var (bpGroup, mess) = await _promotionService.GetByIdAsync(id);
            if (bpGroup == null)
            {
                return BadRequest(new { ErrorCode, ErrorMessage });
            }
            var (check, mes) = await _promotionService.DeleteAsync(id);
            if (!check)
            {
                return BadRequest(new { mes.Status, mes.Errors });
            }
            return Ok("Thành công");
        }

        [HttpPost("getPromotion")]
        public async Task<IActionResult> getPromotion([FromBody] PromotionParam promotionParam)
        {

            var (items, mess) = await _promotionService.GetPromotionAsync(promotionParam);
            if (mess != null)
            {
                return BadRequest(new { mess.Status, mess.Errors });
            }
            return Ok(new { items });
        }
    }
}
