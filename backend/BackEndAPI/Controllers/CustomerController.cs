using AutoMapper;
using BackEndAPI.Models.BusinessPartners;
using BackEndAPI.Models.ItemMasterData;
using BackEndAPI.Models.Other;
using BackEndAPI.Models.Promotion;
using BackEndAPI.Service.BusinessPartners;
using BackEndAPI.Service.ItemMasterData;
using BackEndAPI.Service.Privile;
using Gridify;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.Security.Claims;
using System.Text.Json.Serialization;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace BackEndAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomerController : ControllerBase
    {
        private readonly IBusinessPartnerService _bpService;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IMapper _mapper;

        private readonly Service.Sync.IBPSyncService _bpSyncService;
        private readonly Service.Sync.IInternalProxySyncService _proxySync;

        public CustomerController(IBusinessPartnerService bpService, IHostingEnvironment hostingEnvironment,
            IMapper mapper,
            Service.Sync.IBPSyncService bpSyncService,
            Service.Sync.IInternalProxySyncService proxySync)
        {
            _bpService = bpService;
            _hostingEnvironment = hostingEnvironment;
            _mapper = mapper;
            _bpSyncService = bpSyncService;
            _proxySync = proxySync;
        }

        [PrivilegeRequirement("Customer.Create")]
        [HttpPost]
        [Route("add")]
        public async Task<IActionResult> Create([FromForm] BusinessPartnerView model)
        {
            if (model == null)
            {
                Mess mes = new Mess();
                mes.Status = 400;
                mes.Errors = "Dữ liệu trống";
                return BadRequest(new { mes.Status, mes.Errors });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var (items, mess) = await _bpService.CreateBPAsync(model, "C");
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

        [AllowAnonymous]
        [HttpPost]
        [Route("sync")]
        public async Task<IActionResult> Sync(List<BPView> model)
        {
            if (model == null)
            {
                Mess mes = new Mess();
                mes.Status = 400;
                mes.Errors = "Dữ liệu trống";
                return BadRequest(new { mes.Status, mes.Errors });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var (items, mess) = await _bpService.SyncBPAsync(model);
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
        [AllowAnonymous]
        [HttpPost]
        [Route("syncbp")]
        public async Task<IActionResult> SyncBP()
        {


            var (items, mess) = await _bpService.SyncBPAsync();
            if (mess != null)
            {
                return BadRequest(new { mess.Status, mess.Errors });
            }
            return Ok();
        }
        [AllowAnonymous]
        [HttpPost]
        [Route("syncCrd4")]
        public async Task<IActionResult> SyncCRD4(CancellationToken ct)
        {
            var result = await _bpSyncService.SyncCRD4DeltaAsync(ct);
            return Ok(result);
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("syncCrd4CardCode")]
        public async Task<IActionResult> syncCrd4CardCode(CancellationToken ct)
        {
            var result = await _bpSyncService.SyncCardBalanceCRD4DeltaAsync(ct);
            return Ok(result);
        }
        [AllowAnonymous]
        [HttpPost]
        [Route("syncissue")]
        public async Task<IActionResult> SyncIssue()
        {
            var mess = await _bpService.SyncIssueCancelAsync();
            return Ok();
        }
        [PrivilegeRequirement("Customer.Edit")]
        [HttpPut("{id}")]
        public async Task<IActionResult> update(int id, [FromForm] BusinessPartnerView model)
        {
            if (model == null)
            {
                Mess mes = new Mess();
                mes.Status = 400;
                mes.Errors = "Dữ liệu trống";
                return BadRequest(new { mes.Status, mes.Errors });
            }

            var (items, mess) = await _bpService.UpdateBPAsync(id, model, "C");
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

        [HttpPut()]
        [Route("me")]
        public async Task<IActionResult> updateBP([FromForm] BusinessPartnerView model)
        {
            if (model == null)
            {
                Mess mes = new Mess();
                mes.Status = 400;
                mes.Errors = "Dữ liệu trống";
                return BadRequest(new { mes.Status, mes.Errors });
            }

            var claims = User.Claims.ToList();

            var cardId = claims.FirstOrDefault(c => c.Type == "cardId")?.Value;
            if (cardId == null)
            {
                return Unauthorized();
            }

            var ok = Int32.TryParse(cardId.ToString(), out int id);

            var (items, mess) = await _bpService.UpdateBPAsync(id, model, "C");
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

        [PrivilegeRequirement("Customer.View")]
        [HttpGet("{id}")]
        public async Task<IActionResult> getById([FromRoute] int id)
        {
            var (items, mess) = await _bpService.GetBPByIdAsync(id);
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

        //[AllowAnonymous]
        //[HttpDelete("{id}")]
        //public async Task<IActionResult> Delete(int id)
        //{
        //    var (check, mess) = await _bpService.DeleteAsync(id);
        //    if (!check)
        //    {
        //        return BadRequest(new {mess.Status,mess.Errors});
        //    }
        //    return Ok("Thành công");
        //}v
        [PrivilegeRequirement("Customer.View")]
        [HttpGet]
        public async Task<IActionResult> GetAllWithPagination([FromQuery] GridifyQuery q, int skip = 0, int limit = 30,
            string search = "")
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

            var (items, mess, total) = await _bpService.GetAllBPAsync(skip, limit, search, q, intUserId);
            if (mess != null)
            {
                return BadRequest(new { mess.Status, mess.Errors });
            }

            if (items == null)
            {
                return Ok(new { items });
            }

            return Ok(new { items, total, skip, limit });
        }
        [PrivilegeRequirement("Customer.View")]
        [HttpGet("customegroup")]
        public async Task<IActionResult> GetAllByCustomerGroup([FromQuery] GridifyQuery q, int skip = 0, int limit = 30,
            string search = "")
        {
            var (items, mess, total) = await _bpService.GetAllBPAsync(skip, limit, search, q);
            if (mess != null)
            {
                return BadRequest(new { mess.Status, mess.Errors });
            }

            if (items == null)
            {
                return Ok(new { items });
            }

            return Ok(new { items, total, skip, limit });
        }
        [PrivilegeRequirement("Customer.Delete")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var (check, mes) = await _bpService.DeleteBPAsync(id);
            if (!check)
            {
                return BadRequest(new { mes.Status, mes.Errors });
            }

            return Ok("Thành công");
        }


        [PrivilegeRequirement("Customer.Edit")]
        [HttpPost("{id}/classify")]
        public async Task<IActionResult> AddClassify(int id, [FromBody] List<BpClassify> classifies)
        {
            var (bp, mess) = await _bpService.AddClassify(id, classifies);
            if (mess != null)
            {
                return BadRequest(new { mess.Status, mess.Errors });
            }

            return Ok(bp);
        }

        [PrivilegeRequirement("Customer.Edit")]
        [HttpPut("{id}/classify")]
        public async Task<IActionResult> UpdateClassify(int id, [FromBody] List<BpClassify> classifies)
        {
            var mess = await _bpService.UpdateClassify(id, classifies);
            if (mess != null)
            {
                return BadRequest(new { mess.Status, mess.Errors });
            }

            return Ok();
        }
        [PrivilegeRequirement("Customer.Edit")]
        [HttpPut("{id}/crd3")]
        public async Task<IActionResult> UpdateCRD3(int id, [FromBody] List<CRD3Dto> crd3)
        {
            var (listCrd3 ,mess)= await _bpService.CUDCRD3(id, crd3);
            if (mess != null)
            {
                return BadRequest(new { mess.Status, mess.Errors });
            }

            return Ok(listCrd3);
        }
        [PrivilegeRequirement("Customer.Edit")]
        [HttpDelete("{id}/classify")]
        public async Task<IActionResult> RemoveClassify(int id, [FromBody] List<int> ids)
        {
            var (bp, mess) = await _bpService.RemoveClassify(id, ids);
            if (mess != null)
            {
                return BadRequest(new { mess.Status, mess.Errors });
            }

            return Ok(bp);
        }

        [PrivilegeRequirement("Customer.Edit")]
        [HttpPost("{id}/addresses")]
        public async Task<IActionResult> AddAddress(int id, [FromBody] CRD1 newAddress)
        {
            var (bp, mess) = await _bpService.AddAddress(id, newAddress);
            if (mess != null)
            {
                return BadRequest(new { mess.Status, mess.Errors });
            }

            return Ok(bp);
        }

        [PrivilegeRequirement("Customer.Edit")]
        [HttpPut("{id}/addresses")]
        public async Task<IActionResult> UpdateAddress(int id, [FromBody] CRD1 address)
        {
            var (bp, mess) = await _bpService.UpdateAddress(id, address);
            if (mess != null)
            {
                return BadRequest(new { mess.Status, mess.Errors });
            }

            return Ok(bp);
        }

        [PrivilegeRequirement("Customer.Edit")]
        [HttpDelete("{id}/addresses")]
        public async Task<IActionResult> RemoveAddress(int id, [FromBody] List<int> ids)
        {
            var (bp, mess) = await _bpService.RemoveAddress(id, ids);
            if (mess != null)
            {
                return BadRequest(new { mess.Status, mess.Errors });
            }

            return Ok(bp);
        }
        [HttpGet("{id}/current-commited")]
        public async Task<IActionResult> GetCurrentCommitd(int id)
        {
            var comm = await _bpService.GetCurrentCommited(id);

            return Ok(comm);
        }

        [HttpGet("{id}/debt")]
        public async Task<IActionResult> GetCrd3(int id)
        {
            var comm = await _bpService.GetCRD3Async(id);

            return Ok(comm);
        }

        [PrivilegeRequirement("Customer.Edit")]
        [HttpPost("{id}/files")]
        public async Task<IActionResult> AddFile(int id, [FromForm] List<IFormFile> files, [FromForm] string notes)
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

            string[] noteArr = JsonConvert.DeserializeObject<string[]>(notes);
            if (files.Count != noteArr!.Length)
            {
                return BadRequest("Note và file phai bang nhau");
            }

            var (bp, mess) = await _bpService.AddFiles(id, intUserId, files, noteArr);
            if (mess != null)
            {
                return BadRequest(new { mess.Status, mess.Errors });
            }

            return Ok(bp);
        }

        [PrivilegeRequirement("Customer.Edit")]
        [HttpDelete("{id}/files")]
        public async Task<IActionResult> RemoveFiles(int id, [FromBody] List<int> fileIds)
        {
            var (bp, mess) = await _bpService.RemoveFile(id, fileIds);
            if (mess != null)
            {
                return BadRequest(new { mess.Status, mess.Errors });
            }

            return Ok(bp);
        }

        [PrivilegeRequirement("Customer.Edit")]
        [HttpPut("{id}/files")]
        public async Task<IActionResult> UpdateFile(int id, [FromBody] List<CRD6> files)
        {
            var mess = await _bpService.UpdateFiles(id, files);
            if (mess != null)
            {
                return BadRequest(new { mess.Status, mess.Errors });
            }

            return Ok();
        }

        [PrivilegeRequirement("Customer.Edit")]
        [HttpPost("{id}/sale-staff/{staffId}")]
        public async Task<IActionResult> AddSaleStaff(int id, int staffId)
        {
            var (bp, mess) = await _bpService.ChangeSaleStaff(id, staffId);
            if (mess != null)
            {
                return BadRequest(new { mess.Status, mess.Errors });
            }

            return Ok(bp);
        }


        [HttpPost("ttdh")]
        public async Task<IActionResult> getPromotion(CancellationToken ct)
        {
            var result = await _proxySync.SyncApprovalOrderAsync(ct);
            return Ok(result);
        }

        // Trước đây gọi SyncTCRD4Async — bug, không liên quan ttdhh.
        // Đã sửa: giờ gọi SyncRejectOrderAsync (ttdhh = TTDH huỷ).
        [HttpPost("ttdhh")]
        public async Task<IActionResult> getPromotions(CancellationToken ct)
        {
            var result = await _proxySync.SyncRejectOrderAsync(ct);
            return Ok(result);
        }
        [HttpPost("points")]
        public async Task<IActionResult> Point(string invovic)
        {

            var mess = await _bpService.SyncTTDH1Async(invovic);
            return Ok(mess);
        }
        [HttpPost("ttdhHUY")]
        public async Task<IActionResult> SyncTTDHHAsync(CancellationToken ct)
        {
            var result = await _proxySync.SyncRejectOrderAsync(ct);
            return Ok(result);
        }
    }
}