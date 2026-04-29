using AutoMapper;
using BackEndAPI.Models.ItemMasterData;
using BackEndAPI.Models.Other;
using BackEndAPI.Service;
using BackEndAPI.Service.Areas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace BackEndAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AreaController : Controller
    {
        private readonly IAreaService _iService;
        private readonly IHostingEnvironment _hostingEnvironment;
        public AreaController(IAreaService iService, IHostingEnvironment hostingEnvironment)
        {
            _iService = iService;
            _hostingEnvironment = hostingEnvironment;
        }
        [AllowAnonymous]
        [HttpGet("search/{search}")]
        public async Task<IActionResult> getBySearch(string search)
        {
            var (item, mess) = await _iService.GetFLocation(search);
            if (mess == null)
            {
                return Ok(item);
            }
            else
                return BadRequest(mess);
        }
        [AllowAnonymous]
        [HttpGet("search/{id}/{search}")]
        public async Task<IActionResult> getBySearch(int id, string search)
        {
            var (item, mess) = await _iService.GetFArea(id, search);
            if (mess == null)
            {
                return Ok(item);
            }
            else
                return BadRequest(mess);
        }
        [AllowAnonymous]
        [HttpGet("searchArea/{search}")]
        public async Task<IActionResult> getByArea(string search)
        {
            var (item, mess) = await _iService.GetALocation(search);
            if (mess == null)
            {
                return Ok(item);
            }
            else
                return BadRequest(mess);
        }
        [AllowAnonymous]
        [HttpGet("searchArea/{id}/{search}")]
        public async Task<IActionResult> getByArea(int id, string search)
        {
            var (item, mess) = await _iService.GetAArea(id, search);
            if (mess == null)
            {
                return Ok(item);
            }
            else
                return BadRequest(mess);
        }
        [AllowAnonymous]
        [HttpGet("searchNewArea/{search}")]
        public async Task<IActionResult> getByNewArea(string search)
        {
            var (item, mess) = await _iService.GetNewArea(search);
            if (mess == null)
            {
                return Ok(item);
            }
            else
                return BadRequest(mess);
        }
        [AllowAnonymous]
        [HttpGet("searchNewArea/{id}/{search}")]
        public async Task<IActionResult> getByNewArea(int id, string search)
        {
            var (item, mess) = await _iService.GetNewArea(id, search);
            if (mess == null)
            {
                return Ok(item);
            }
            else
                return BadRequest(mess);
        }
    }
}
