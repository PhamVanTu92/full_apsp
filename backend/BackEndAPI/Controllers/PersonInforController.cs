using AutoMapper;
using BackEndAPI.Models.Banks;
using BackEndAPI.Models.BusinessPartners;
using BackEndAPI.Models.Other;
using BackEndAPI.Service;
using BackEndAPI.Service.BusinessPartners;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace BackEndAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PersonInforController : Controller
    {
        //private readonly IService<PersonInfor> _iService;
        //private readonly IHostingEnvironment _hostingEnvironment;
        //private readonly IMapper _mapper;
        //public PersonInforController(IService<PersonInfor> iService, IHostingEnvironment hostingEnvironment, IMapper mapper)
        //{
        //    _iService = iService;
        //    _hostingEnvironment = hostingEnvironment;
        //    _mapper = mapper;
        //}
        //[AllowAnonymous]
        //[HttpPost("add")]
        //public async Task<IActionResult> CreatePersonInfor(PersonInfor model)
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
        //    var (bank, mess) = await _iService.AddAsync(model);
        //    if (bank == null)
        //    {
        //        return BadRequest(new { mess.Status, mess.Errors });
        //    }
        //    return Ok(bank);
        //}
        //[AllowAnonymous]
        //[HttpPut]
        //public async Task<IActionResult> UpdatePersonInfor(int id, PersonInfor model)
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
        //    var (person, mess) = await _iService.UpdateAsync(id, model);
        //    if (person == null)
        //    {
        //        return BadRequest(new { mess.Status, mess.Errors });
        //    }
        //    return Ok(person);
        //}
        //[AllowAnonymous]
        //[HttpGet("search/{search}")]
        //public async Task<IActionResult> getBySearch(string type, string search)
        //{
        //    var (person, mess) = await _iService.GetAsync(null, "PersonName", search);
        //    if (person == null)
        //    {
        //        return BadRequest(new { mess.Status, mess.Errors });
        //    }
        //    return Ok(person.Where(e=>e.PersonType ==type));
        //}
        //[AllowAnonymous]
        //[HttpGet("getall")]
        //public async Task<IActionResult> getall(string type)
        //{
        //    var (person, mess) = await _iService.GetAllAsync();
        //    if (person == null)
        //    {
        //        return BadRequest(new { mess.Status, mess.Errors });
        //    }
        //    return Ok(person.Where(e => e.PersonType == type));
        //}
    }
}
