using AutoMapper;
using BackEndAPI.Models.Document;
using BackEndAPI.Models.Other;
using BackEndAPI.Service.Document;
using BackEndAPI.Service.Zalo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackEndAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ZaloController : Controller
    {
        private readonly IDocumentService _arService;
        private readonly ZaloService _zaloService;
        public ZaloController(IDocumentService arServicer, ZaloService zaloService)
        {
            _arService = arServicer;
            _zaloService = zaloService;
        }
        [Authorize]
        [HttpPost()]
        public async Task<IActionResult> SendZalo(int DocId, string TypeMess)
        {
            var mes = await _arService.SendZalo(DocId, TypeMess);
            if(mes!=null)
                return BadRequest(mes);
            return Ok();
        }
        [Authorize]
        [HttpGet()]
        public async Task<IActionResult> getZalo()
        {
            var (zalo,mes) = await _zaloService.GetZaloInfo();
            if (mes != null)
                return BadRequest(mes);
            return Ok(zalo);
        }
        [Authorize]
        [HttpPut()]
        public async Task<IActionResult> UpdateZalo([FromBody] ZaloAccess model)
        {
            var mes = await _zaloService.Update(model);
            if (mes != null)
                return BadRequest(mes);
            return Ok();
        }
    }
}
