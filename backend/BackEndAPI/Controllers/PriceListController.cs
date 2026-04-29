using AutoMapper;
using BackEndAPI.Models.ItemMasterData;
using BackEndAPI.Models.Other;
using BackEndAPI.Models.PriceList;
using BackEndAPI.Service;
using BackEndAPI.Service.ItemMasterData;
using BackEndAPI.Service.PriceLists;
using BackEndAPI.Service.Privile;
using BackEndAPI.Service.Unit;
using Gridify;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using NHibernate.Mapping.ByCode;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BackEndAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PriceListController : Controller
    {
        private readonly IItemService _itemService;
        private readonly IMapper _mapper;
        private readonly IPriceListService _service;
        public PriceListController(IItemService itemService,  IMapper mapper, IPriceListService service)
        {
            _itemService = itemService;
            _mapper = mapper;
            _service = service;
        }
        [PrivilegeRequirement("Pricelist.Create")]
        [HttpPost("import")]
        public async Task<IActionResult> PostImport(List<ItemImportView> view)
        {
            var (items, mess) = await _itemService.GetImportPricelist(view);
            if (mess != null)
            {
                return BadRequest(new { mess.Status, mess.Errors });
            }
            return Ok(items);
        }
        [PrivilegeRequirement("Pricelist.View")]
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery]  GridifyQuery gridifyQuery)
        {
            var (data, mess,total) = await _service.GetAllAsync(gridifyQuery);
            if (mess != null)
            {
                return BadRequest(new { mess.Status, mess.Errors });
            }
            return Ok(new {data, total });
        }
        [PrivilegeRequirement("Pricelist.View")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var (data,mess) = await _service.GetByIdAsync(id);
            if (mess != null)
            {
                return BadRequest(new { mess.Status, mess.Errors });
            }
            return Ok(data);
        }
        [PrivilegeRequirement("Pricelist.Create")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PriceList priceList)
        {
            var (created,mess) = await _service.CreateAsync(priceList);
            if (mess != null)
            {
                return BadRequest(new { mess.Status, mess.Errors });
            }
            return Ok(created);
        }
        [PrivilegeRequirement("Pricelist.Edit")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] PriceList priceList)
        {
            var (updated,mess) = await _service.UpdateAsync(id, priceList);
            if (mess != null)
            {
                return BadRequest(new { mess.Status, mess.Errors });
            }
            return Ok(updated);
        }
        [PrivilegeRequirement("Pricelist.Edit")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var (deleted,mess) = await _service.DeleteAsync(id);
            if (deleted)
            {
                return BadRequest(new { mess.Status, mess.Errors });
            }
            return Ok();
        }
    }
}