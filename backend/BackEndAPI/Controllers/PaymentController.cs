using AutoMapper;
using BackEndAPI.Models.Banks;
using BackEndAPI.Models.ItemMasterData;
using BackEndAPI.Models.Other;
using BackEndAPI.Service;
using BackEndAPI.Service.Payments;
using BackEndAPI.Service.Privile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace BackEndAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : Controller
    {
        private readonly IPaymentService _iService;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IMapper _mapper;

        public PaymentController(IPaymentService iService, IHostingEnvironment hostingEnvironment, IMapper mapper)
        {
            _iService = iService;
            _hostingEnvironment = hostingEnvironment;
            _mapper = mapper;
        }

        [HttpPost]
        [PrivilegeRequirement("Payment")]
        public async Task<IActionResult> CreatePayment(PaymentDto model)
        {
            Mess mes = new Mess();
            if (model == null)
            {
                mes.Status = 400;
                mes.Errors = "Dữ liệu trống";
                return BadRequest(mes);
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var (bank, mess) = await _iService.AddPaymentAsync(model);
            if (bank == null)
            {
                return BadRequest(new { mess.Status, mess.Errors });
            }

            return Ok(bank);
        }

        [HttpGet("{id}")]
        [PrivilegeRequirement("Payment")]
        public async Task<IActionResult> getById(int id)
        {
            var item = await _iService.GetPaymentById(id);

            return Ok(item);
        }
        //[AllowAnonymous]
        //[HttpGet]
        //public async Task<IActionResult> GetAllWithPagination(int skip = 0, int limit = 30, int BranchId = 0)
        //{
        //    var (items, total,beginBalance, inComingPayment,outGoingPayment,EndBalance,mess) = await _iService.GetAllPaymentWithPaginationAsync(skip, limit, BranchId);
        //    if(mess != null)
        //    {
        //        return BadRequest(new { mess.Status, mess.Errors });
        //    }    
        //    return Ok(new { items, beginBalance, inComingPayment, outGoingPayment, EndBalance, total, skip, limit});
        //}
    }
}