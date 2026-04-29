using AutoMapper;
using BackEndAPI.Models.Document;
using BackEndAPI.Service.Document;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace BackEndAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ARCreditMemoController : Controller
    {
        //private readonly IDocumentService _arService;
        //private readonly IHostingEnvironment _hostingEnvironment;
        //private readonly IMapper _mapper;
        //public ARCreditMemoController(IDocumentService arService, IHostingEnvironment hostingEnvironment, IMapper mapper)
        //{
        //    _arService = arService;
        //    _hostingEnvironment = hostingEnvironment;
        //    _mapper = mapper;
        //}
        //[AllowAnonymous]
        //[HttpPost]
        //[Route("add")]
        //public async Task<IActionResult> Create([FromBody] ODOC model)
        //{
        //    if (model == null)
        //    {
        //        int ErrorCode = 204;
        //        string ErrorMessage = "Dữ liệu trống";
        //        return BadRequest(new { ErrorCode, ErrorMessage });
        //    }
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }
        //    var (itemGroup, mess) = await _arService.AddDocumentAsync(model, 14);
        //    if (mess != null)
        //    {
        //        return BadRequest(new { mess.Status, mess.Errors });
        //    }
        //    return Ok(itemGroup);
        //}
        //[AllowAnonymous]
        //[HttpGet]
        //public async Task<IActionResult> GetAllWithPagination(int skip = 0, int limit = 30)
        //{
        //    var (items, total, mess) = await _arService.GetAllDocumentAsync(skip, limit, 14);
        //    if (mess != null)
        //    {
        //        return BadRequest(new { mess.Status, mess.Errors });
        //    }
        //    return Ok(new { items, total, skip, limit });
        //}
        //[AllowAnonymous]
        //[HttpGet("{id}")]
        //public async Task<IActionResult> getById(int id)
        //{
        //    var (item, mess) = await _arService.GetDocumentByIdAsync(id, 14);
        //    if (mess != null)
        //    {
        //        return BadRequest(new { mess.Status, mess.Errors });
        //    }
        //    return Ok(new { item });
        //}
        //[AllowAnonymous]
        //[HttpGet("search/{search}")]
        //public async Task<IActionResult> getBySearch(string search)
        //{
        //    var (item, mess) = await _arService.GetDocumentAsync(search, 14);
        //    if (mess != null)
        //    {
        //        return BadRequest(new { mess.Status, mess.Errors });
        //    }
        //    return Ok(new { item });
        //}
    }
}
