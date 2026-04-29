using AutoMapper;
using BackEndAPI.Models.Unit;
using BackEndAPI.Service.Privile;
using BackEndAPI.Service.Unit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace BackEndAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OUGPController : ControllerBase
    {
        private readonly IOUGPService _ougpService;
        private readonly IOUOMService _ouomService;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IMapper _mapper;
        public OUGPController(IOUGPService ougpService, IOUOMService ouomService, IHostingEnvironment hostingEnvironment, IMapper mapper)
        {
            _ougpService = ougpService;
            _ouomService = ouomService;
            _hostingEnvironment = hostingEnvironment;
            _mapper = mapper;
        }
        [PrivilegeRequirement("UnitGroup.Create")]
        [HttpPost]
        [Route("add")]
        public async Task<IActionResult> Create([FromBody] OUGP model)
        {
            int ErrorCode = 0;
            string ErrorMessage = "";
            if (model == null)
            {
                ErrorCode = 204;
                ErrorMessage = "Dữ liệu trống";
                return BadRequest(new { ErrorCode, ErrorMessage });
            }
            if (model.BaseUom <= 0)
            {
                ErrorCode = 204;
                ErrorMessage = "Đơn vị tính cơ sở trống";
                return BadRequest(new { ErrorCode, ErrorMessage });
            }
            if (model.BaseUom > 0)
            {
                var (item, mess) = await _ouomService.GetByIdAsync((int)model.BaseUom);
                if (item == null)
                {
                    ErrorCode = 204;
                    ErrorMessage = "Đơn vị tính cơ sở không tồn tại";
                    return BadRequest(new { ErrorCode, ErrorMessage });
                }
            }
            if (!ModelState.IsValid)
            {
                ErrorCode = 204;
                ErrorMessage = String.Join(Environment.NewLine, ModelState.Values.SelectMany(v => v.Errors)
                                                           .Select(v => v.ErrorMessage + " " + v.Exception));
                return BadRequest(new { ErrorCode, ErrorMessage });
            }
            var (items,message) = await _ougpService.AddAsync(model);
            if (message != null)
            {
                return BadRequest(new { message.Status, message.Errors });
            }
            return Ok(items);
        }
        [AllowAnonymous]
        [HttpPost]
        [Route("sync")]
        public async Task<IActionResult> Sync(List<OUGP> model)
        {
            int ErrorCode = 0;
            string ErrorMessage = "";
            if (model == null)
            {
                ErrorCode = 204;
                ErrorMessage = "Dữ liệu trống";
                return BadRequest(new { ErrorCode, ErrorMessage });
            }
            if (!ModelState.IsValid)
            {
                ErrorCode = 204;
                ErrorMessage = String.Join(Environment.NewLine, ModelState.Values.SelectMany(v => v.Errors)
                                                           .Select(v => v.ErrorMessage + " " + v.Exception));
                return BadRequest(new { ErrorCode, ErrorMessage });
            }
            var (items, message) = await _ougpService.SyncOUGPAsync(model);
            if (message != null)
            {
                return BadRequest(new { message.Status, message.Errors });
            }
            return Ok(items);
        }
        [PrivilegeRequirement("UnitGroup.View")]
        [HttpGet]
        public async Task<IActionResult> GetAllWithPagination(string? search,int skip = 0, int limit = 30)
        {
            var (items, mess, total) = await _ougpService.GetOUGPs(search,skip, limit);
            
            return Ok(new { items, total, skip, limit });
        }
        [PrivilegeRequirement("UnitGroup.View")]
        [HttpGet("{id}")]
        public async Task<IActionResult> getById(int id)
        {
            var (items,mess) = await _ougpService.GetOUGPByIdAsync(id);
            if (mess != null)
            {
                return BadRequest(new { mess.Status, mess.Errors });
            }
            return Ok(items);
        }
        [PrivilegeRequirement("UnitGroup.Edit")]
        [HttpPut("{id}")]
        public async Task<IActionResult> update(int id, [FromBody] OUGP model)
        {
            if (model == null)
                return BadRequest("data is null");
            var (items,mess) = await _ougpService.UpdateOUGPAsync(id, model);
            if (mess != null)
            {
                return BadRequest(new { mess.Status, mess.Errors });
            }
            return Ok(items);
        }
        [PrivilegeRequirement("UnitGroup.Delete")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            int ErrorCode = 400;
            string ErrorMessage = "Không tìm thấy đơn vị tính";
            var (bpGroup, mess) = await _ougpService.GetByIdAsync(id);
            if (bpGroup == null)
            {
                return BadRequest(new { ErrorCode, ErrorMessage });
            }
            var (check, mes) = await _ougpService.DeleteAsync(id);
            if (!check)
            {
                return BadRequest(new { mes.Status, mes.Errors });
            }
            return Ok("Thành công");
        }
    }
}
