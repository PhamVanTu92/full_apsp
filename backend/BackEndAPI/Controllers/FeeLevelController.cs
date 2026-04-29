using AutoMapper;
using BackEndAPI.Models.Fee;
using BackEndAPI.Models.Other;
using BackEndAPI.Service.Fees;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace BackEndAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FeeLevelController : Controller
    {
        private readonly IFeeLevelService _feeLevelService;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IMapper _mapper;
        public FeeLevelController(IFeeLevelService feeLevelService, IHostingEnvironment hostingEnvironment, IMapper mapper)
        {
            _feeLevelService = feeLevelService;
            _hostingEnvironment = hostingEnvironment;
            _mapper = mapper;
        }
        [AllowAnonymous]
        [HttpPost("add")]
        public async Task<IActionResult> CreateFeeLevel(IEnumerable<FeeLevel> model)
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
            var (items, mess) = await _feeLevelService.CreateFeeLevelAsync(model);
            if (mess != null)
            {
                return BadRequest(new { mess.Status, mess.Errors });
            }
            return Ok(items);
        }
        //[AllowAnonymous]
        //[HttpPut("{id}")]
        //public async Task<IActionResult> UpdateFeeLevel(int id, FeeLevel model)
        //{
        //    Mess mes = new Mess();
        //    if (model == null)
        //    {
        //        mes.Status = 400;
        //        mes.Errors = "Dữ liệu trống";
        //        return BadRequest(mes);
        //    }
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }
        //    var (items, mess) = await _feeLevelService.UpdateFeeLevelAsync(id, model);
        //    if (mess != null)
        //    {
        //        return BadRequest(new { mess.Status, mess.Errors });
        //    }
        //    return Ok(items);
        //}
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetAllWithPagination(int skip = 0, int limit = 30, string search  = null)
        {
            var (items, mess, total) = await _feeLevelService.GetAllFeeLevelAsync(skip, limit, search);
            if (mess != null)
                return BadRequest(new { mess.Status, mess.Errors });
            return Ok(new { items, total, skip, limit });

        }
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<IActionResult> getById(int id)
        {
            var (items, mess) = await _feeLevelService.GetFeeLevelByIdAsync(id);
            if (mess != null)
            {
                return BadRequest(new { mess.Status, mess.Errors });
            }
            return Ok(items);
        }
    }
}
