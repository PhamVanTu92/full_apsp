using AutoMapper;
using BackEndAPI.Models.ARInvoice;
using BackEndAPI.Models.BusinessPartners;
using BackEndAPI.Models.Document;
using BackEndAPI.Models.Promotion;
using BackEndAPI.Service;
using BackEndAPI.Service.Document;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace BackEndAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GoodReceiptPOController : ControllerBase
    {
        //private readonly IDocumentService _arService;
        //private readonly IHostingEnvironment _hostingEnvironment;
        //private readonly IMapper _mapper;
        //private readonly IModelUpdater _modelUpdater;
        //public GoodReceiptPOController(IDocumentService arService, IModelUpdater modelUpdater, IHostingEnvironment hostingEnvironment, IMapper mapper)
        //{
        //    _arService = arService;
        //    _hostingEnvironment = hostingEnvironment;
        //    _mapper = mapper;
        //    _modelUpdater = modelUpdater;
        //}
        ////[AllowAnonymous]
        //[AllowAnonymous]
        //[HttpPost]
        //[Route("add")]
        //public async Task<IActionResult> Create([FromBody] ODOC model)
        //{
        //    //Document document = new Document();
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
        //    var (itemGroup, mess) = await _arService.AddDocumentAsync(model, 20);
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
        //    var (items, total, mess) = await _arService.GetAllDocumentAsync(skip, limit, 20);
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
        //    var (item, mess) = await _arService.GetDocumentByIdAsync(id, 20);
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
        //    var (item, mess) = await _arService.GetDocumentAsync(search, 20);
        //    if (mess != null)
        //    {
        //        return BadRequest(new { mess.Status, mess.Errors });
        //    }
        //    return Ok(new { item });
        //}
    }
}
