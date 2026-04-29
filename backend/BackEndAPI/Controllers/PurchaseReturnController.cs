using AutoMapper;
using BackEndAPI.Models.Document;
using BackEndAPI.Service.Document;
using BackEndAPI.Service.Privile;
using Gridify;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace BackEndAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PurchaseReturnController : Controller
    {
        private readonly IDocumentService _arService;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IMapper _mapper;

        public PurchaseReturnController(IDocumentService arService, IHostingEnvironment hostingEnvironment,
            IMapper mapper)
        {
            _arService = arService;
            _hostingEnvironment = hostingEnvironment;
            _mapper = mapper;
        }
        [Authorize]
        [HttpGet("getOrder")]
        public async Task<IActionResult> getByOrderSearch([FromQuery]  GridifyQuery q)
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
            var (item, mess, total) = await _arService.GetOrderReturnAsync(int.Parse(userId), q);
            if (mess != null)
            {
                return BadRequest(new { mess.Status, mess.Errors });
            }

            return Ok(new { item , total});
        }
        [PrivilegeRequirement("PurchaseReturn.Create")]
        [HttpPost]
        [Route("add")]
        public async Task<IActionResult> Create([FromBody] OrderReturn doc)
        {
            ODOC model = new ODOC();
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
            model.ObjType = 16;
            model.UserId = intUserId;
            model.ObjType = doc.ObjType;
            model.DocDate = doc.DocDate;
            model.RefInvoiceCode = doc.RefInvoiceCode;
            model.DocType = "";
            model.CardId = doc.CardId;
            model.CardName = doc.CardName;
            model.CardCode = doc.CardCode;
            List<DOC1> doc1  = new List<DOC1>();
            foreach(var d in doc.ItemDetails.ToList())
            {
                DOC1 d1 = new DOC1();
                d1.Quantity = d.Quantity;
                d1.Price = d.Price ?? 0;
                d1.ItemId = d.ItemId;
                if ((d.Type ?? "") == "")
                    d1.Type = "I";
                else
                    d1.Type = d.Type;
                d1.ItemCode = d.ItemCode;
                d1.ItemName = d.ItemName;
                d1.UomName = d.UomName;
                d1.UomCode = d.UomCode;
                d1.OuomId = d.OuomId;
                d1.BaseId = d.BaseId;
                d1.BaseLineId = d.BaseLineId;
                doc1.Add(d1);
            }
            model.ItemDetail = doc1;
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var (itemGroup, mess) = await _arService.AddDocumentAsync(model, 16);
            if (mess != null)
            {
                return BadRequest(new { mess.Status, mess.Errors });
            }

            return Ok(model);
        }

        [PrivilegeRequirement("PurchaseReturn.View")]
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

            var (items, total, mess) = await _arService.GetAllDocumentAsync(skip, limit, 16, q, search, intUserId);
            if (mess != null)
            {
                return BadRequest(new { mess.Status, mess.Errors });
            }

            return Ok(new { items, total, skip, limit });
        }

        [PrivilegeRequirement("PurchaseReturn.Edit")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDocument(int id, [FromForm] OrderReturn model)
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
            var (items, mess) = await _arService.UpdateDocumentReturnAsync(id, model, 16);
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
            var mess = await _arService.UpdateStatus(id, status, AttachFile, 16, reasonForCancellation ?? "");
            if (mess != null)
            {
                return BadRequest(new { mess.Status, mess.Errors });
            }

            return Ok();
        }
        [PrivilegeRequirement("PurchaseReturn.View")]
        [HttpGet("{id}")]
        public async Task<IActionResult> getById(int id)
        {
            var (item, mess) = await _arService.GetDocumentByIdAsync(id, 16);
            if (mess != null)
            {
                return BadRequest(new { mess.Status, mess.Errors });
            }

            return Ok(new { item });
        }

        [PrivilegeRequirement("PurchaseReturn.View")]
        [HttpGet("search/{search}")]
        public async Task<IActionResult> getBySearch(string search)
        {
            var (item, mess) = await _arService.GetDocumentAsync(search, 16);
            if (mess != null)
            {
                return BadRequest(new { mess.Status, mess.Errors });
            }

            return Ok(new { item });
        }

        [HttpGet("/api/me/PurchaseReturn")]
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
                await _arService.GetAllDocumentByCardIdIdAsync(skip, limit, iCardId, 16, search, q);

            return Ok(new { items, total, skip, limit });
        }
    }
}
