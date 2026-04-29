using AutoMapper;
using BackEndAPI.Models.Document;
using BackEndAPI.Models.Other;
using BackEndAPI.Models.Promotion;
using BackEndAPI.Service.Cart;
using BackEndAPI.Service.Document;
using BackEndAPI.Service.ItemMasterData;
using BackEndAPI.Service.Privile;
using BackEndAPI.Service.Promotions;
using BackEndAPI.Service.Unit;
using Gridify;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BackEndAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RedeemController : ControllerBase
    {
        private readonly IPointSetupService _service;
        private readonly IDocumentService _docservice;
        public RedeemController(IPointSetupService service, IDocumentService docservice)
        {
            _service = service;
            _docservice = docservice;
        }
        [PrivilegeRequirement("OrderExchange.Create")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ODOC model)
        {
            var claims = User.Identity as ClaimsIdentity;
            if (claims == null)
            {
                return Unauthorized();
            }

            var userId = claims.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var ok = Int32.TryParse(userId, out int intUserId);
            if (!ok)
            {
                return Unauthorized();
            }

            if (model == null)
            {
                int ErrorCode = 204;
                string ErrorMessage = "Dữ liệu trống";
                return BadRequest(new { ErrorCode, ErrorMessage });
            }

            model.UserId = intUserId;
            model.DocType = "DVP";
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var (itemGroup, mess) = await _docservice.AddDocumentAsync(model, 12);
            if (mess != null)
            {
                return BadRequest(new { mess.Status, mess.Errors });
            }

            return Ok(itemGroup);
        }
        [PrivilegeRequirement("OrderExchange.View")]
        [HttpGet]
        public async Task<IActionResult> GetAllWithPagination([FromQuery] GridifyQuery q, string? search, int skip = 0,
            int limit = 30)
        {
            var claims = User.Identity as ClaimsIdentity;
            if (claims == null)
            {
                return Unauthorized();
            }

            var userId = claims.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var ok = Int32.TryParse(userId, out int intUserId);
            string serverCondition = "DocType=DVP";

            if (string.IsNullOrWhiteSpace(q.Filter))
            {
                q.Filter = serverCondition;
            }
            else
            {
                q.Filter = $"{q.Filter},{serverCondition}";
            }
            var (items, total, mess) = await _docservice.GetAllDocumentAsync(skip, limit, 12, q, search, intUserId);
            if (mess != null)
            {
                return BadRequest(new { mess.Status, mess.Errors });
            }

            return Ok(new { items, total, skip, limit });
        }
        [PrivilegeRequirement("OrderExchange.Edit")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDocument(int id, [FromForm] DOCUpdate model)
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
            model.UserId = intUserId;
            var (items, mess) = await _docservice.UpdateDocumentAsync(id, model, 12);
            if (mess != null)
            {
                return BadRequest(new { mess.Status, mess.Errors });
            }

            return Ok(items);
        }
        [PrivilegeRequirement("OrderExchange.Complete")]
        [HttpPut("{id}/change-status/{status}")]
        public async Task<IActionResult> UpdateStatus(int id, [FromRoute] string status, List<IFormFile> AttachFile)
        {
            var mess = await _docservice.UpdateStatus(id, status, AttachFile, 12);
            if (mess != null)
            {
                return BadRequest(new { mess.Status, mess.Errors });
            }

            return Ok();
        }
        [PrivilegeRequirement("OrderExchange.View")]
        [HttpGet("{id}")]
        public async Task<IActionResult> getById(int id)
        {
            var (item, mess) = await _docservice.GetDocumentByIdAsync(id, 12);
            if (mess != null)
            {
                return BadRequest(new { mess.Status, mess.Errors });
            }

            return Ok(new { item });
        }

        [PrivilegeRequirement("OrderExchange.View")]
        [HttpGet("search/{search}")]
        public async Task<IActionResult> getBySearch(string search)
        {
            var (item, mess) = await _docservice.GetDocumentAsync(search, 12);
            if (mess != null)
            {
                return BadRequest(new { mess.Status, mess.Errors });
            }

            return Ok(new { item });
        }
        [Authorize]
        [HttpGet("/api/me/exhange")]
        public async Task<IActionResult> GetByCardIdWithAuthJwt(string? search, [FromQuery] GridifyQuery q,
            int skip = 0, int limit = 30)
        {
            var claims = User.Claims.ToList();

            var cardId = claims.FirstOrDefault(c => c.Type == "cardId")?.Value;
            if (cardId == null)
            {
                return Unauthorized();
            }

            var ok = Int32.TryParse(cardId.ToString(), out int iCardId);

            var (items, total, mess) =
                await _docservice.GetAllDocumentByCardIdIdAsync(skip, limit, iCardId, 12, search, q);

            return Ok(new { items, total, skip, limit });
        }
        [Authorize]
        [HttpGet("report")]
        public async Task<IActionResult> getReport(string? cardId, DateTime fromDate, DateTime toDate,int skip = 1, int limit = 30)
        {

            var (items, total, mess) = await _service.GetReportPoint(cardId, fromDate, toDate, skip, limit);

            return Ok(new { items, total, skip, limit });
        }
    }
}
