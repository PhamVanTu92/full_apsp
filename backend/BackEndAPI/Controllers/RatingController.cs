using AutoMapper;
using BackEndAPI.Models.Other;
using BackEndAPI.Service.Document;
using Gridify;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackEndAPI.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class RatingController : Controller
    {
        private readonly IDocumentService _arService;
        public RatingController(IDocumentService arService)
        {
            _arService = arService;
        }
        [Authorize]
        [HttpPost()]
        public async Task<IActionResult> CreateGeneralRating([FromForm] CreateGeneralRatingDto dto)
        {
            var (result,mess) = await _arService.CreateGeneralRatingAsync(dto);
            if (mess != null)
            {
                return BadRequest(new { mess.Status, mess.Errors });
            }
            return Ok(result);
        }
        [Authorize]
        [HttpGet()]
        public async Task<IActionResult> CreateGeneralRating([FromQuery] GridifyQuery q)
        {
            var (result,mess,total) = await _arService.GetGeneralRatingAsync(q);
            if (mess != null)
            {
                return BadRequest(new { mess.Status, mess.Errors });
            }
            return Ok(new { result, total});
        }
        [Authorize]
        [HttpGet("{Id}")]
        public async Task<IActionResult> CreateGeneralRating(int Id)
        {
            var (result, mess) = await _arService.GetGeneralRatingAsync(Id);
            if (mess != null)
            {
                return BadRequest(new { mess.Status, mess.Errors });
            }
            return Ok(result);
        }
    }
}
