using AutoMapper;
using BackEndAPI.Models.ItemMasterData;
using BackEndAPI.Models.Promotion;
using BackEndAPI.Service.Promotions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace BackEndAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CouponController : Controller
    {
        //private readonly ICouponService _couponService;
        //private readonly ICouponLineService _couponLineService;
        //private readonly IHostingEnvironment _hostingEnvironment;
        //private readonly IMapper _mapper;
        //public CouponController(ICouponService couponService, ICouponLineService couponLineService, IHostingEnvironment hostingEnvironment, IMapper mapper)
        //{
        //    _couponService = couponService;
        //    _couponLineService = couponLineService;
        //    _hostingEnvironment = hostingEnvironment;
        //    _mapper = mapper;
        //}
        //[AllowAnonymous]
        //[HttpPost]
        //[Route("add")]
        //public async Task<IActionResult> Create([FromBody] Coupon model)
        //{
        //    int ErrorCode = 0;
        //    string ErrorMessage = "";
        //    if (model == null)
        //    {
        //        ErrorCode = 204;
        //        ErrorMessage = "Dữ liệu trống";
        //        return BadRequest(new { ErrorCode, ErrorMessage });
        //    }
        //    if (!ModelState.IsValid)
        //    {
        //        ErrorCode = 204;
        //        ErrorMessage = String.Join(Environment.NewLine, ModelState.Values.SelectMany(v => v.Errors)
        //                                                   .Select(v => v.ErrorMessage + " " + v.Exception));
        //        return BadRequest(new { ErrorCode, ErrorMessage });
        //    }
        //    //var ouom = await _couponService.AddAsync(model);
        //    var coupon = await _couponService.AddCouponAsync(model);
        //    return Ok(coupon);
        //}
        //[AllowAnonymous]
        //[HttpPost]
        //[Route("addCoupon")]
        //public async Task<IActionResult> CreateCoupon([FromBody] CouponCodeRule model)
        //{
        //    int ErrorCode = 0;
        //    string ErrorMessage = "";
        //    if (model == null)
        //    {
        //        ErrorCode = 204;
        //        ErrorMessage = "Dữ liệu trống";
        //        return BadRequest(new { ErrorCode, ErrorMessage });
        //    }
        //    if (!ModelState.IsValid)
        //    {
        //        ErrorCode = 204;
        //        ErrorMessage = String.Join(Environment.NewLine, ModelState.Values.SelectMany(v => v.Errors)
        //                                                   .Select(v => v.ErrorMessage + " " + v.Exception));
        //        return BadRequest(new { ErrorCode, ErrorMessage });
        //    }
        //    var ouom = await _couponLineService.CreateCouponLineAsync(model);
        //    return Ok(ouom);
        //}
        //[AllowAnonymous]
        //[HttpPut]
        //[Route("updateCoupon")]
        //public async Task<IActionResult> UpdateCoupon(int id, string status, [FromBody] List<CouponLineView> model)
        //{
        //    int ErrorCode = 0;
        //    string ErrorMessage = "";
        //    if (model == null)
        //    {
        //        ErrorCode = 204;
        //        ErrorMessage = "Dữ liệu trống";
        //        return BadRequest(new { ErrorCode, ErrorMessage });
        //    }
        //    if (!ModelState.IsValid)
        //    {
        //        ErrorCode = 204;
        //        ErrorMessage = String.Join(Environment.NewLine, ModelState.Values.SelectMany(v => v.Errors)
        //                                                   .Select(v => v.ErrorMessage + " " + v.Exception));
        //        return BadRequest(new { ErrorCode, ErrorMessage });
        //    }
        //    var (ouom,mes) = await _couponLineService.UpdateCouponLineAysnc(id, status, model);
        //    if(ouom == null)
        //    {
        //        return BadRequest(new { mes.Status, mes.Errors });
        //    }    
        //    return Ok(ouom);
        //}
        //[AllowAnonymous]
        //[HttpGet]
        //public async Task<IActionResult> GetAllWithPagination(int skip = 0, int limit = 30)
        //{
        //    var (item, total, mess) = await _couponService.GetAllWithPaginationAsync(skip, limit, "CouponLine", "CouponItem");
        //    if(mess != null)
        //    {
        //        return BadRequest(new { mess.Status, mess.Errors });
        //    }
        //    if (item == null)
        //        return Ok(new { item });
        //    var items = _mapper.Map<List<CouponDTO>>(item);
        //    return Ok(new { items, total, skip, limit });
        //}
        //[AllowAnonymous]
        //[HttpGet]
        //[Route("getCouponLine")]
        //public async Task<IActionResult> GetAllCouponLineWithPagination(int skip = 0, int limit = 30, int id = 0, string status = "", string couponCode = "")
        //{
        //    var (items, total) = await _couponLineService.GetCouponLineAsync(skip, limit,id, status, couponCode);
        //    return Ok(new { items, total, skip, limit });
        //}
        //[AllowAnonymous]
        //[HttpGet("{id}")]
        //public async Task<IActionResult> getById(int id)
        //{
        //    var item = await _couponService.GetCouponByIdAsync(id);
        //    if (item == null)
        //    {
        //        return NotFound();
        //    }
        //    return Ok(item);
        //}
        //[AllowAnonymous]
        //[HttpPut("{id}")]
        //public async Task<IActionResult> update(int id, [FromBody] Coupon model)
        //{
        //    if (model == null)
        //        return BadRequest("data is null");
        //    var itemGroup = await _couponService.UpdateCouponAsync(id, model);
        //    if (itemGroup == null)
        //    {
        //        int ErrorCode = 400;
        //        string ErrorMessage = "Coupon không đúng";
        //        return BadRequest(new { ErrorCode, ErrorMessage });
        //    }
        //    return Ok(itemGroup);
        //}
        //[AllowAnonymous]
        //[HttpDelete("{id}")]
        //public async Task<IActionResult> Delete(int id)
        //{
        //    int ErrorCode = 400;
        //    string ErrorMessage = "Coupon không đúng";
        //    var (bpGroup,mess) = await _couponService.GetByIdAsync(id);
        //    if (bpGroup == null)
        //    {
        //        return BadRequest(new { mess.Status, mess.Errors });
        //    }
        //    var (check, mes) = await _couponService.DeleteAsync(id);
        //    if (!check)
        //    {
        //        return BadRequest(new { mes.Status, mes.Errors });
        //    }
        //    return Ok("Thành công");
        //}
    }
}
