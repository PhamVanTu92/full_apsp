using AutoMapper;
using BackEndAPI.Models.Document;
using BackEndAPI.Service.Document;
using Gridify;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using BackEndAPI.Service.Privile;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace BackEndAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RequestOfExampleController : Controller
    {
        private readonly IDocumentService _arService;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IMapper _mapper;
        public RequestOfExampleController(IDocumentService arService, IHostingEnvironment hostingEnvironment,
           IMapper mapper)
        {
            _arService = arService;
            _hostingEnvironment = hostingEnvironment;
            _mapper = mapper;
        }

        [Authorize]
        [HttpPost("Add")]
        [PrivilegeRequirement("Test.Create")]
        public async Task<IActionResult> SyncToSap([FromBody] ORFS model)
        {
            await _arService.AddDocumentORFSAsync(model);

            return Ok();
        }
        [Authorize]
        [HttpGet]
        [PrivilegeRequirement("Test.View")]
        public async Task<IActionResult> GetAllWithPagination([FromQuery] GridifyQuery q, string? search)
        {
            var claims = User.Identity as ClaimsIdentity;
            if (claims == null)
            {
                return Unauthorized();
            }

            var userId = claims.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var ok = Int32.TryParse(userId, out int intUserId);

            var (items, total, mess) = await _arService.GetAllDocumentORFSAsync(q, search, intUserId);

            if (mess != null)
            {
                return BadRequest(new { mess.Status, mess.Errors });
            }

            return Ok(new { items, total });
        }
        [Authorize]
        [HttpGet("{id}")]
        [PrivilegeRequirement("Test.View")]
        public async Task<IActionResult> get(int id)
        {
            var claims = User.Identity as ClaimsIdentity;
            if (claims == null)
            {
                return Unauthorized();
            }

            var userId = claims.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var ok = Int32.TryParse(userId, out int intUserId);

            var (items, mess) = await _arService.GetAllDocumentORFSAsync(id);

            if (mess != null)
            {
                return BadRequest(new { mess.Status, mess.Errors });
            }

            return Ok(new { items });
        }
        [Authorize]
        [HttpPut("{id}")]
        [PrivilegeRequirement("Test.Update")]
        public async Task<IActionResult> Put(int id, [FromBody] ORFS doc)
        {
            var claims = User.Identity as ClaimsIdentity;
            if (claims == null)
            {
                return Unauthorized();
            }
            var (items, mess) = await _arService.UpdateDocumentORFSAsync(id, doc);

            if (mess != null)
            {
                return BadRequest(new { mess.Status, mess.Errors });
            }

            return Ok(new { items });
        }
        
        /// <summary>
        /// Đẩy bản nháp lên chính thức 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost("{id}/official")]
        public async Task<IActionResult> RemoveDraft(int id)
        {
            await _arService.RemoveDraft(id);

            return Ok();
        }

    }
}
