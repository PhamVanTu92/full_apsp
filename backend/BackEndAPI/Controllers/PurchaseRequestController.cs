using System.Security.Claims;
using AutoMapper;
using BackEndAPI.Models.Document;
using BackEndAPI.Models.Other;
using BackEndAPI.Service.Document;
using BackEndAPI.Service.Privile;
using Gridify;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace BackEndAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PurchaseRequestController : Controller
    {
        private readonly IDocumentService _arService;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IMapper _mapper;

        public PurchaseRequestController(IDocumentService arService, IHostingEnvironment hostingEnvironment,
            IMapper mapper)
        {
            _arService = arService;
            _hostingEnvironment = hostingEnvironment;
            _mapper = mapper;
        }

        [PrivilegeRequirement("PurchaseRequest.Create")]
        [HttpPost]
        [Route("add")]
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

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var (itemGroup, mess) = await _arService.AddDocumentAsync(model, 1250000001);
            if (mess != null)
            {
                return BadRequest(new { mess.Status, mess.Errors });
            }

            return Ok();
        }

        [PrivilegeRequirement("PurchaseRequest.View")]
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
            
            var (items, total, mess) = await _arService.GetAllDocumentAsync(skip, limit, 1250000001, q, search, intUserId);
            if (mess != null)
            {
                return BadRequest(new { mess.Status, mess.Errors });
            }

            return Ok(new { items, total, skip, limit });
        }

        [PrivilegeRequirement("PurchaseRequest.Edit")]
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
            var (items, mess) = await _arService.UpdateDocumentAsync(id, model, 1250000001);
            if (mess != null)
            {
                return BadRequest(new { mess.Status, mess.Errors });
            }

            return Ok(items);
        }
        [Authorize]
        [HttpPut("{id}/change-status/{status}")]
        public async Task<IActionResult> UpdateStatus(int id, [FromRoute] string status, List<IFormFile> AttachFile, [FromQuery] string? reasonForCancellation)
        {
            var mess = await _arService.UpdateStatus(id, status, AttachFile, 1250000001, reasonForCancellation ?? "");
            if (mess != null)
            {
                return BadRequest(new { mess.Status, mess.Errors });
            }

            return Ok();
        }
        [PrivilegeRequirement("PurchaseRequest.View")]
        [HttpGet("{id}")]
        public async Task<IActionResult> getById(int id)
        {
            var (item, mess) = await _arService.GetDocumentByIdAsync(id, 1250000001);
            if (mess != null)
            {
                return BadRequest(new { mess.Status, mess.Errors });
            }

            return Ok(new { item });
        }

        [PrivilegeRequirement("PurchaseRequest.View")]
        [HttpGet("search/{search}")]
        public async Task<IActionResult> getBySearch(string search)
        {
            var (item, mess) = await _arService.GetDocumentAsync(search, 1250000001);
            if (mess != null)
            {
                return BadRequest(new { mess.Status, mess.Errors });
            }

            return Ok(new { item });
        }

        [HttpGet("/api/me/PurchaseRequest")]
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
                await _arService.GetAllDocumentByCardIdIdAsync(skip, limit, iCardId, 1250000001, search, q);

            return Ok(new { items, total, skip, limit });
        }

        [PrivilegeRequirement("PurchaseRequest.UpdateNote")]
        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateNote([FromRoute] int id, [FromBody] ODOC model)
        {
            var mess = await _arService.PathUpdate(id, model);
            if (mess != null)
            {
                return BadRequest(new { mess.Status, mess.Errors });
            }

            return Ok();
        }
        [PrivilegeRequirement("PurchaseRequest.Edit")]
        [HttpPost("{id}/AttDocuments")]
        public async Task<IActionResult> AddAttDocument(int id, [FromForm] List<IFormFile> files)
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

            var mess = await _arService.AddAttDocuments(id, intUserId, files);
            if (mess != null)
            {
                return BadRequest(new { mess.Status, mess.Errors });
            }

            return Ok();
        }

        [PrivilegeRequirement("PurchaseRequest.Edit")]
        [HttpDelete("{id}/AttDocuments")]
        public async Task<IActionResult> RemoveAttDocument(int id, [FromBody] List<int> fileIds)
        {
            var mess = await _arService.RemoveAttDocuments(id, fileIds);
            if (mess != null)
            {
                return BadRequest(new { mess.Status, mess.Errors });
            }

            return Ok();
        }

        [PrivilegeRequirement("PurchaseRequest.Edit")]
        [HttpPost("{id}/AttFiles")]
        public async Task<IActionResult> AddAttFile(int id, [FromForm] List<IFormFile> files)
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

            var mess = await _arService.AddAttFile(id, intUserId, files);
            if (mess != null)
            {
                return BadRequest(new { mess.Status, mess.Errors });
            }

            return Ok();
        }

        [PrivilegeRequirement("PurchaseRequest.Edit")]
        [HttpDelete("{id}/AttFiles")]
        public async Task<IActionResult> RemoveAttFile(int id, [FromBody] List<int> fileIds)
        {
            var mess = await _arService.RemoveAttFile(id, fileIds);
            if (mess != null)
            {
                return BadRequest(new { mess.Status, mess.Errors });
            }

            return Ok();
        }

        [HttpPut("{id:int}/addresses")]
        public async Task<IActionResult> UpdateAddress([FromRoute] int id, [FromBody] List<DOC12> address)
        {
            await _arService.UpdateDocAddress(id, address);
            return Ok();
        }
        [Authorize]
        [HttpPost("rating")]
        public async Task<IActionResult> CreateOrderRating([FromForm] CreateOrderRatingDto dto)
        {
            var result = await _arService.CreateOrderRatingAsync(dto);
            return Ok(result);
        }
    }
}