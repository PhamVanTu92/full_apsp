using AutoMapper;
using BackEndAPI.Models.ConfirmationSystem;
using BackEndAPI.Models.Other;
using BackEndAPI.Service.Brands;
using BackEndAPI.Service.Cart;
using BackEndAPI.Service.ConfirmationSystems;
using Gridify;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace BackEndAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConfirmationController : Controller
    {

        private readonly IConfirmationDocumentService _iService;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IMapper _mapper;
        public ConfirmationController(IConfirmationDocumentService iService, IHostingEnvironment hostingEnvironment, IMapper mapper)
        {
            _iService = iService;
            _hostingEnvironment = hostingEnvironment;
            _mapper = mapper;
        }
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromForm] ConfirmationDocumentNew doc)
        {
            var claims = User.Identity as ClaimsIdentity;
            if (claims == null)
            {
                return Unauthorized();
            }

            var userId = claims.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var ok = int.TryParse(userId, out int intUserId);
            var (docs, mess) = await _iService.Create(doc, intUserId);
            if (mess != null)
            {
                return BadRequest(new { mess.Status, mess.Errors });
            }
            return Ok(docs);
        }
        [HttpGet("getall")]
        public async Task<IActionResult> GetList([FromQuery] GridifyQuery q)
        {
            var claims = User.Identity as ClaimsIdentity;
            if (claims == null)
            {
                return Unauthorized();
            }

            var userId = claims.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var ok = int.TryParse(userId, out int intUserId);
            var (items, total, mess) = await _iService.GetList(q, intUserId);

            if (mess != null)
            {
                return BadRequest(new { mess.Status, mess.Errors });
            }

            return Ok(new{items,total});
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDetail(int id)
        {
            var (document, mess) = await _iService.GetDetail(id);
            if(mess != null)
            {
                return BadRequest(new { mess.Status, mess.Errors });
            }

            return Ok(document);
        }

        // 4. Gửi cho khách hàng
        [HttpPost("send")]
        public async Task<IActionResult> Send([FromBody] ActionRequest request)
        {

            var claims = User.Identity as ClaimsIdentity;
            if (claims == null)
            {
                return Unauthorized();
            }

            var userId = claims.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var ok = int.TryParse(userId, out int intUserId);
            var (check, mess) = await _iService.Send(request, intUserId);
            if (mess != null)
            {
                return BadRequest(new { mess.Status, mess.Errors });
            }

            return Ok();
        }

        // 5. Khách hàng xác nhận (Approve)
        [HttpPost("approve")]
        public async Task<IActionResult> Approve([FromBody] ActionRequest request)
        {
            var claims = User.Identity as ClaimsIdentity;
            if (claims == null)
            {
                return Unauthorized();
            }

            var userId = claims.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var ok = int.TryParse(userId, out int intUserId);
            var (check, mess) = await _iService.Approve(request, intUserId);
            if (mess != null)
            {
                return BadRequest(new { mess.Status, mess.Errors });
            }

            return Ok();
        }

        // 6. Khách hàng từ chối (Reject)
        [HttpPost("reject")]
        public async Task<IActionResult> Reject([FromBody] ActionRequest request)
        {
            var claims = User.Identity as ClaimsIdentity;
            if (claims == null)
            {
                return Unauthorized();
            }

            var userId = claims.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var ok = int.TryParse(userId, out int intUserId);
            var (check, mess) = await _iService.Reject(request, intUserId);
            if (mess != null)
            {
                return BadRequest(new { mess.Status, mess.Errors });
            }

            return Ok();
        }
    }
}
