using AutoMapper;
using BackEndAPI.Models.Promotion;
using BackEndAPI.Service.Promotions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace BackEndAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VoucherController : Controller
    {
        
        private readonly IVoucherService _voucherService;
        private readonly IVoucherLineService _voucherLineService;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IMapper _mapper;
        public VoucherController(IVoucherService voucherService, IVoucherLineService voucherLineService, IHostingEnvironment hostingEnvironment, IMapper mapper)
        {
            _voucherService = voucherService;
            _voucherLineService = voucherLineService;
            _hostingEnvironment = hostingEnvironment;
            _mapper = mapper;
        }
        [AllowAnonymous]
        [HttpPost]
        [Route("add")]
        public async Task<IActionResult> Create([FromBody] Voucher model)
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
            var ouom = await _voucherService.AddVoucherAsync(model);
            return Ok(ouom);
        }
        [AllowAnonymous]
        [HttpPost]
        [Route("addVoucher")]
        public async Task<IActionResult> CreateVoucher([FromBody] VoucherCodeRule model)
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
            var (ouom, mes )= await _voucherLineService.CreateVoucherLineAsync(model);
            if (ouom == null)
            {
                return BadRequest(new { mes.Status, mes.Errors });
            }
            return Ok(ouom);
        }
        [AllowAnonymous]
        [HttpPost]
        [Route("addAVoucher")]
        public async Task<IActionResult> CreateAVoucher(int id,[FromBody] List<VoucherLineAdd> model)
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
            var (ouom, mes) = await _voucherLineService.CreateAVoucherLineAsync(id,model);
            if (ouom == null)
            {
                return BadRequest(new { mes.Status, mes.Errors });
            }
            return Ok(ouom);
        }
        [AllowAnonymous]
        [HttpPut]
        [Route("updateVoucher/{id}/{status}")]
        public async Task<IActionResult> UpdateVoucher(int id, string status, [FromBody] VoucherListToRelease model)
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
            var (ouom, mes) = await _voucherLineService.UpdateVoucherLineAysnc(id, status, model);
            if (ouom == null)
            {
                return BadRequest(new { mes.Status, mes.Errors });
            }
            return Ok(ouom);
        }
        [AllowAnonymous]
        [HttpPut]
        [Route("cancelVoucher/{id}/{status}")]
        public async Task<IActionResult> CancelVoucher(int id, string status, [FromBody] VoucherListToCancel model)
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
            var (ouom, mes) = await _voucherLineService.CancelVoucherLineAysnc(id, status, model);
            if (mes != null)
            {
                return BadRequest(new { mes.Status, mes.Errors });
            }
            return Ok(ouom);
        }
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetAllWithPagination(int skip = 0, int limit = 30)
        {
            var (items, total) = await _voucherService.GetVoucherAsync(skip, limit);
            return Ok(new { items, total, skip, limit });
        }
        [AllowAnonymous]
        [HttpGet]
        [Route("getVoucherLine")]
        public async Task<IActionResult> GetAllVoucherLineWithPagination(int skip = 0, int limit = 30, int id = 0, string status = "", string voucherCode = "")
        {
            var (items, total) = await _voucherLineService.GetVoucherLineAsync(skip, limit, id, status, voucherCode);
            return Ok(new { items, total, skip, limit });
        }

        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<IActionResult> getById(int id)
        {
            var item = await _voucherService.GetVoucherByIdAsync(id);
            if (item == null)
            {
                return NotFound();
            }
            return Ok(item);
        }
        [AllowAnonymous]
        [HttpPut("{id}")]
        public async Task<IActionResult> update(int id, [FromBody] Voucher model)
        {
            if (model == null)
                return BadRequest("data is null");
            var itemGroup = await _voucherService.UpdateVoucherAsync(id, model);
            if (itemGroup == null)
            {
                int ErrorCode = 400;
                string ErrorMessage = "Không Voucher";
                return BadRequest(new { ErrorCode, ErrorMessage });
            }
            return Ok(itemGroup);
        }
        [AllowAnonymous]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            int ErrorCode = 400;
            string ErrorMessage = "Không Voucher";
            var (bpGroup, mess) = await _voucherService.GetByIdAsync(id);
            if (bpGroup == null)
            {
                return BadRequest(new { ErrorCode, ErrorMessage });
            }
            var (check, mes) = await _voucherService.DeleteAsync(id);
            if (!check)
            {
                return BadRequest(new { mes.Status, mes.Errors });
            }
            return Ok("Thành công");
        }
    }
}
