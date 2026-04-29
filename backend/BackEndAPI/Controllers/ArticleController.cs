using BackEndAPI.Models.BusinessPartners;
using BackEndAPI.Models.Other;
using BackEndAPI.Service.Areas;
using BackEndAPI.Service.Articles;
using BackEndAPI.Service.Privile;
using Gridify;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace BackEndAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ArticleController : Controller
    {
        private readonly IArticleService _iService;
        public ArticleController(IArticleService iService)
        {
            _iService = iService;
        }
        [Authorize]
        [HttpGet()]
        [PrivilegeRequirement("Article")]
        public async Task<IActionResult> GetAllWithPagination([FromQuery] GridifyQuery q)
        {
            var (item,count, mess) = await _iService.GetAllAsync(q);
            if (mess == null)
            {
                return Ok(new { item, total = count });
            }
            else
                return BadRequest(mess);
        }
        [Authorize]
        [HttpPost()]
        [PrivilegeRequirement("Article")]
        public async Task<IActionResult> CreateArticle([FromForm] ArticleView model)
        {
            var (item, mess) = await _iService.CreateAsync(model);
            if (mess == null)
            {
                return Ok(new { item });
            }
            else
                return BadRequest(mess);
        }
    }
}
