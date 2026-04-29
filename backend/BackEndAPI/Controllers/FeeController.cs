using AutoMapper;
using BackEndAPI.Models.Fee;
using BackEndAPI.Models.ItemGroups;
using BackEndAPI.Models.ItemMasterData;
using BackEndAPI.Models.Other;
using BackEndAPI.Service;
using BackEndAPI.Service.Fees;
using BackEndAPI.Service.NotificationHub;
using BackEndAPI.Service.Privile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Pqc.Crypto.Lms;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;
using Gridify;

namespace BackEndAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FeeController : Controller
    {
        private readonly IFeeService _feeService;
        private readonly IFeebyCustomerService _feebyService;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IMapper _mapper;
        public FeeController(IFeeService feeService, IHubContext<NotificationHubs> hubContext, IFeebyCustomerService feebyService, IHostingEnvironment hostingEnvironment, IMapper mapper)
        {
            _feeService = feeService;
            _feebyService = feebyService;
            _hostingEnvironment = hostingEnvironment;
            _mapper = mapper;
        }
        
        [PrivilegeRequirement("Fee.Create")]
        [HttpPost("add")]
        public async Task<IActionResult> CreateFee(Fee model)
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
            var (items, mess) = await _feeService.CreateFeeAsync(model);
            if (mess != null)
            {
                return BadRequest(new { mess.Status, mess.Errors });
            }
            return Ok(items);
        }
        [PrivilegeRequirement("Fee.Edit")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateFee(int id, Fee model)
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
            var (items, mess) = await _feeService.UpdateFeeAsync(id, model);
            if (mess != null)
            {
                return BadRequest(new { mess.Status, mess.Errors });
            }
            return Ok(new { items });
        }
       
        [PrivilegeRequirement("Fee.ImportFile")]
        [HttpGet]
        public async Task<IActionResult> GetAllWithPagination(string? search, [FromQuery] GridifyQuery q)
        {
            var (items, mess, total) = await _feeService.GetAllFeeLAsync(search, q);
            if (mess != null)
                return BadRequest(new { mess.Status, mess.Errors });
            return Ok(new { items, total });

        }
        [PrivilegeRequirement("Fee.ImportFile")]
        [HttpGet("feeLine")]
        public async Task<IActionResult> GetAllFeeLine()
        {
            var (items, mess) = await _feeService.GetFeeLineAsync();
            if (mess != null)
                return BadRequest(new { mess.Status, mess.Errors });
            return Ok(new { items});

        }

        [PrivilegeRequirement("FeePeriod.ViewList")]
        [HttpGet("feePeriod/{year}/{period}")]
        public async Task<IActionResult> GetAllFeePeriod(int year, int period, string? search, [FromQuery] GridifyQuery gq, int skip = 0, int limit = 30)
        {
            var userClaims = User.Claims.ToList();
            var userId = userClaims.FirstOrDefault(c => c.Type == "userId")?.Value;
            var userType = userClaims.FirstOrDefault(c => c.Type == "userType")?.Value;
            if (userType == "NPP")
            {
                var (items, mess, total) = await _feebyService.GetAllFeeByCustomerAsync(year, period, search, gq, skip, limit);
                if (mess != null)
                {
                    return BadRequest(new { mess.Status, mess.Errors });
                }
                return Ok(new { items, skip = skip, limit = limit, total });
            }
            else
            {
                var (items, mess, total) = await _feebyService.GetAllFeeByCustomerAsync(year, period, search, gq, skip, limit);
                if (mess != null)
                {
                    return BadRequest(new { mess.Status, mess.Errors });
                }
                return Ok(new { items, skip = skip, limit = limit, total });
            }
        }

        [PrivilegeRequirement("FeePeriod.ViewList")]
        [Authorize]
        [HttpGet("feeByCus/{Id}")]
        public async Task<IActionResult> GetAllFeeByCus(int Id)
        {
            var userClaims = User.Claims.ToList();
            var userId = userClaims.FirstOrDefault(c => c.Type == "userId")?.Value;

            var userType = userClaims.FirstOrDefault(c => c.Type == "userType")?.Value;
            if(userType =="NPP")
            {
                var (items, mess) = await _feebyService.GetFeeByCustomerAsync(Id, userId);
                if (mess != null)
                    return BadRequest(new { mess.Status, mess.Errors });
                return Ok(new { items });
            }   
            else
            {
                var (items, mess) = await _feebyService.GetFeeByCustomerAsync(Id);
                if (mess != null)
                    return BadRequest(new { mess.Status, mess.Errors });
                return Ok(new { items });
            }    
                
            

        }
        [PrivilegeRequirement("FeePeriod.ViewList")]
        [HttpGet("feeByCus/pagination")]
        public async Task<IActionResult> GetAllWithPage(int skip = 0, int limit = 30)
        {
            var userClaims = User.Claims.ToList();
            var userId = userClaims.FirstOrDefault(c => c.Type == "userId")?.Value;
            var userType = userClaims.FirstOrDefault(c => c.Type == "userType")?.Value;
            if (userType == "NPP")
            {
                var (items, mess,total) = await _feebyService.GetAllFeeByCustomerAsync(userId,skip, limit);
                if (mess != null)
                    return BadRequest(new { mess.Status, mess.Errors });
                return Ok(new { items, total, skip, limit });
            }
            else
            {
                var (items, mess, total) = await _feebyService.GetAllFeeByCustomerAsync(skip, limit);
                if (mess != null)
                    return BadRequest(new { mess.Status, mess.Errors });
                return Ok(new { items, total, skip, limit });
            }

        }
        [PrivilegeRequirement("FeePeriod.ViewList")]
        [HttpGet("feeByCusMess/{Id}")]
        public async Task<IActionResult> GetAllFeeByCusMes(int Id)
        {
            var userClaims = User.Claims.ToList();
            var userId = userClaims.FirstOrDefault(c => c.Type == "userId")?.Value;
            var (items, mess) = await _feebyService.GetFeeByCustomerAsync(Id, userId);
            if (mess != null)
                return BadRequest(new { mess.Status, mess.Errors });
            return Ok(new { items });

        }

        #region MyRegion

        

        //[AllowAnonymous]
        //[HttpPut("feeByCus/status/{id}/{status}")]
        //public async Task<IActionResult> UpdateFeeStatus(int id, string status)
        //{
        //    Mess mes = new Mess();
        //    if (status == null)
        //    {
        //        mes.Status = 400;
        //        mes.Errors = "Dữ liệu trống";
        //        return BadRequest(mes);
        //    }
        //    if (status == "SD" || status == "CF" || status == "PD")
        //    {
        //        if (status == "SD")
        //        {
        //            var (items, ms) = await _feebyService.UpdateStatus(id);
        //            if (ms == null)
        //            {
        //                return BadRequest(new { ms.Status, ms.Errors });
        //            }
        //            return Ok(new { items });
        //        }    
        //        else if (status == "CF")
        //        {
        //            var (items, ms) = await _feebyService.UpdateConfirmStatus(id);
        //            if (ms == null)
        //            {
        //                return BadRequest(new { ms.Status, ms.Errors });
        //            }
        //            return Ok(new { items });
        //        }
        //        else 
        //        {
        //            var (items, ms) = await _feebyService.UpdatePayStatus(id);
        //            if (ms == null)
        //            {
        //                return BadRequest(new { ms.Status, ms.Errors });
        //            }
        //            return Ok(new { items });
        //        }
        //    }
        //    else
        //    {
        //        mes.Status = 400;
        //        mes.Errors = "Trạng thái không tồn tại";
        //        return BadRequest(mes);
        //    }

        //}
        #endregion
        [PrivilegeRequirement("Fee.ImportFile")]
        [HttpGet("{id}")]
        public async Task<IActionResult> getById(int id)
        {
            var (items, mess) = await _feeService.GetFeeByIdAsync(id);
            if (mess != null)
            {
                return BadRequest(new { mess.Status, mess.Errors });
            }
            return Ok(new { items });
        }
        [PrivilegeRequirement("FeePeriod.Edit")]
        [HttpPut("feeByCus/{status}")]
        public async Task<IActionResult> UpdateFeeCust(string status,List<int> t)
        {
            var userClaims = User.Claims.ToList();
            var userId = userClaims.FirstOrDefault(c => c.Type == "userId")?.Value;

            var userType = userClaims.FirstOrDefault(c => c.Type == "userType")?.Value;
            Mess mes = new Mess();
            if (t.Count < 0)
            {
                mes.Status = 400;
                mes.Errors = "Dữ liệu trống";
                return BadRequest(mes);
            }
            if(status =="SD")
            {
                if(userType == "NPP")
                {
                    mes.Status = 400;
                    mes.Errors = "Không có quyền thực hiện";
                    return BadRequest(mes);
                }    
                var (items, mess) = await _feebyService.UpdateStatus(t);
                if (mess != null)
                {
                    return BadRequest(new { mess.Status, mess.Errors });
                }
                return Ok(new { items });
            }  
            else if (status == "CF")
            {
                if (userType == "APSP")
                {
                    mes.Status = 400;
                    mes.Errors = "Không có quyền thực hiện";
                    return BadRequest(mes);
                }
                var (items, mess) = await _feebyService.UpdateConfirmStatus(t);
                if (mess != null)
                {
                    return BadRequest(new { mess.Status, mess.Errors });
                }
                return Ok(new { items });
            }
            else if (status == "PD")
            {
                if (userType == "NPP")
                {
                    mes.Status = 400;
                    mes.Errors = "Không có quyền thực hiện";
                    return BadRequest(mes);
                }
                var (items, mess) = await _feebyService.UpdatePayStatus(t);
                if (mess != null)
                {
                    return BadRequest(new { mess.Status, mess.Errors });
                }
                return Ok(new { items });
            }
            else
            {
                mes.Errors = "Trạng thái cập nhập không đúng";
                mes.Status = 400;
                return BadRequest(mes);
            }

        }

        [PrivilegeRequirement("FeePeriod.Edit")]
        [HttpPut("feeByCus/{id}/{note}")]
        public async Task<IActionResult> UpdateNoteFeeByCus(int id, string note)
        {
            var mess = await _feebyService.UpdateNoteFeeByCus(id, note);
            if (mess != null)
            {
                return BadRequest(mess);
            }

            return Ok();
        }
    }
}
