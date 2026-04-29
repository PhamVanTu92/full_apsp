using AutoMapper;
using BackEndAPI.Middleware;
using BackEndAPI.Models.Banks;
using BackEndAPI.Models.Other;
using BackEndAPI.Service;
using BackEndAPI.Service.Cache;
using BackEndAPI.Service.Privile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace BackEndAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentMethodController : Controller
    {
        private readonly IService<PaymentMethod> _iService;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IMapper _mapper;
        private readonly IReferenceDataCache _cache;

        public PaymentMethodController(IService<PaymentMethod> iService, IHostingEnvironment hostingEnvironment, IMapper mapper,
            IReferenceDataCache cache)
        {
            _iService = iService;
            _hostingEnvironment = hostingEnvironment;
            _mapper = mapper;
            _cache = cache;
        }

        [AllowAnonymous]
        [HttpPost("add")]
        [PrivilegeRequirement("Payment")]
        public async Task<IActionResult> CreatePaymentMethod(PaymentMethod model)
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
            var (bank, mess) = await _iService.AddAsync(model);
            if (bank == null)
            {
                return BadRequest(new { mess.Status, mess.Errors });
            }
            await _cache.InvalidateAsync(ReferenceEntities.PaymentMethod);
            return Ok(bank);
        }

        [AllowAnonymous]
        [HttpPut("{id}")]
        [PrivilegeRequirement("Payment")]
        public async Task<IActionResult> UpdatePaymentMethod(int id, PaymentMethod model)
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
            var (bank, mess) = await _iService.UpdateAsync(id, model);
            if (bank == null)
            {
                return BadRequest(new { mess.Status, mess.Errors });
            }
            await _cache.InvalidateAsync(ReferenceEntities.PaymentMethod);
            return Ok(bank);
        }

        [AllowAnonymous]
        [HttpGet("search/{search}")]
        [PrivilegeRequirement("Payment")]
        public async Task<IActionResult> getBySearch(string search)
        {
            var (items, mess) = await _iService.GetAsync(null, "PaymentMethodCode", search, "PaymentMethodName", search);
            if (items == null)
            {
                return BadRequest(new { mess.Status, mess.Errors });
            }
            return Ok(items);
        }

        [AllowAnonymous]
        [HttpGet("getall")]
        [PrivilegeRequirement("Payment")]
        [CacheableReferenceData(ReferenceEntities.PaymentMethod)]
        public async Task<IActionResult> getall()
        {
            var items = await _cache.GetOrSetAsync(ReferenceEntities.PaymentMethod, async () =>
            {
                var (data, mess) = await _iService.GetAllAsync();
                if (data == null) throw new Exception(mess?.Errors ?? "Lỗi tải payment method");
                return data;
            });
            return Ok(items);
        }
    }
}
