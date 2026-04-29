using AutoMapper;
using BackEndAPI.Models.Document;
using BackEndAPI.Service.Document;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using BackEndAPI.Service.Privile;
using Gridify;
using Microsoft.AspNetCore.JsonPatch;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;
using BackEndAPI.Models.Other;

namespace BackEndAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PurchaseOrderController : Controller
    {
        private readonly IDocumentService _arService;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IMapper _mapper;

        private readonly Service.Sync.IDocumentPushSyncService _pushSync;

        public PurchaseOrderController(IDocumentService arService, IHostingEnvironment hostingEnvironment,
            IMapper mapper,
            Service.Sync.IDocumentPushSyncService pushSync)
        {
            _arService = arService;
            _hostingEnvironment = hostingEnvironment;
            _mapper = mapper;
            _pushSync = pushSync;
        }

        // Cũ chỉ push 1 doc/call. Giờ drain toàn bộ pending (Drafts + Docs + Issues).
        [AllowAnonymous]
        [HttpPost("sync-sap")]
        public async Task<IActionResult> SyncToSap(CancellationToken ct)
        {
            var result = await _pushSync.PushPendingBatchAsync(ct);
            return Ok(result);
        }

        // Cũ chỉ push 1 issue/call. PushPendingBatchAsync cover luôn issues.
        [AllowAnonymous]
        [HttpPost("sync-sapyc")]
        public async Task<IActionResult> SyncToSapyc(CancellationToken ct)
        {
            var result = await _pushSync.PushPendingBatchAsync(ct);
            return Ok(result);
        }
        [PrivilegeRequirement("Order.Create")]
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

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            model.UserId = intUserId;
            var (itemGroup, mess) = await _arService.AddDocumentAsync(model, 22);
            if (mess != null)
            {
                return BadRequest(new { mess.Status, mess.Errors });
            }

            return Ok();
        }

        [PrivilegeRequirement("Order.View")]
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
            //string serverCondition = "(DocType == null OR DocType == '')";

            //if (!string.IsNullOrWhiteSpace(q.Filter))
            //    q.Filter = $"{q.Filter} AND {serverCondition}";
            //else
            //    q.Filter = serverCondition;
            var (items, total, mess) = await _arService.GetAllDocumentAsync(skip, limit, 22, q, search, intUserId);
            
            if (mess != null)
            {
                return BadRequest(new { mess.Status, mess.Errors });
            }

            return Ok(new { items, total, skip, limit });
        }

        [PrivilegeRequirement("Order.UploadDocuments")]
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
            var (items, mess) = await _arService.UpdateDocumentAsync(id, model, 22);
            if (mess != null)
            {
                return BadRequest(new { mess.Status, mess.Errors });
            }

            return Ok(items);
        }

        [PrivilegeRequirement("Order.View")]
        [HttpGet("{id}")]
        public async Task<IActionResult> getById(int id)
        {
            var (item, mess) = await _arService.GetDocumentByIdAsync(id, 22);
            if (mess != null)
            {
                return BadRequest(new { mess.Status, mess.Errors });
            }

            return Ok(new { item });
        }
        
        // [PrivilegeRequirement("Order.View")]
        [HttpPost("{id}/send-payment")]
        public async Task<IActionResult> SendPaymentRequest([FromRoute] int id)
        {
            await _arService.SendPaymentRequest(id);
            return NoContent();
        }
        
        [HttpPost("{id}/confirm-payment")]
        public async Task<IActionResult> PaymentSuccess([FromRoute] int id)
        {
            await _arService.ConfirmPayment(id);
            return NoContent();
        }

        [PrivilegeRequirement("Order.View")]
        [HttpGet("search/{search}")]
        public async Task<IActionResult> getBySearch(string search)
        {
            var (item, mess) = await _arService.GetDocumentAsync(search, 22);
            if (mess != null)
            {
                return BadRequest(new { mess.Status, mess.Errors });
            }

            return Ok(new { item });
        }

        [PrivilegeRequirement("Order.Complete")]
        [HttpPut("{id}/change-status/{status}")]
        public async Task<IActionResult> UpdateStatus(int id, [FromRoute] string status, List<IFormFile> AttachFile, [FromQuery] string? reasonForCancellation)
        {
            var mess = await _arService.UpdateStatus(id, status, AttachFile, 22, reasonForCancellation ?? "");
            if (mess != null)
            {
                return BadRequest(new { mess.Status, mess.Errors });
            }

            return Ok();
        }
        
        [HttpPost("price-check")]
        public async Task<IActionResult> CheckPriceDodc([FromBody] PriceDocCheck check)
        {
            var (result, result1,rs2) = await _arService.GetPaymentInfo1(check);
            return Ok(new {item =result, items =result1, items2 = rs2 });
        }

        [HttpGet("/api/me/PurchaseOrder")]
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
                await _arService.GetAllDocumentByCardIdIdAsync(skip, limit, iCardId, 22, search, q);

            return Ok(new { items, total, skip, limit });
        }

        [PrivilegeRequirement("Order.UpdateNote")]
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

        [PrivilegeRequirement("Order.Edit")]
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

        [PrivilegeRequirement("Order.Edit")]
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
        
        [PrivilegeRequirement("Order.Edit")]
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

        [PrivilegeRequirement("Order.Edit")]
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
        [AllowAnonymous]
        [HttpPost("synDraft")]
        public async Task<IActionResult> synVPKM(CancellationToken ct)
        {
            var result = await _pushSync.PushPendingBatchAsync(ct);
            return Ok(result);
        }
        [Authorize]
        [HttpPost("rating")]
        public async Task<IActionResult> CreateOrderRating([FromForm] CreateOrderRatingDto dto)
        {
            var result = await _arService.CreateOrderRatingAsync(dto);
            return Ok(result);
        }
        [Authorize]
        [HttpPost("ReportSync")]
        public async Task<IActionResult> ReportSync([FromQuery] GridifyQuery q)
        {
            var (result,mess,total) = await _arService.SyncInvoiceError(q);
            if (mess != null)
            {
                return BadRequest(new { mess.Status, mess.Errors });
            }
            return Ok(new { result, total });
        }
        [Authorize]
        [HttpPost("ReSync")]
        public async Task<IActionResult> ReSync(int DocId, int ObjType)
        {
            var mess = await _arService.SyncInvoiceError(DocId, ObjType);
            if (mess != null)
            {
                return BadRequest(new { mess.Status, mess.Errors });
            }
            return Ok();
        }
    }
}