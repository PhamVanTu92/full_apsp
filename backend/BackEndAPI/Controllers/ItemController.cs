using AutoMapper;
using BackEndAPI.Models.ItemMasterData;
using BackEndAPI.Models.Other;
using BackEndAPI.Service.ItemMasterData;
using BackEndAPI.Service.Unit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using System.Collections.Generic;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;
using System.Security.Claims;
using BackEndAPI.Data;
using BackEndAPI.Service.Privile;
using Microsoft.EntityFrameworkCore;
using Gridify;

namespace BackEndAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ItemController : ControllerBase
    {
        private readonly IItemService _itemService;
        private readonly IOUOMService _ouomService;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IMapper _mapper;

        public ItemController(IItemService itemService, IOUOMService ouomService,
            IHostingEnvironment hostingEnvironment, IMapper mapper)
        {
            _itemService = itemService;
            _ouomService = ouomService;
            _hostingEnvironment = hostingEnvironment;
            _mapper = mapper;
        }

        [AllowAnonymous]
        [HttpGet("bypass")]
        public async Task<IActionResult> GetAllWithPaginationForClient(int? cardId, [FromQuery] GridifyQuery gridifyQuery)
        {
            var search = HttpContext.Request.Query["search"];
            var brand = HttpContext.Request.Query["brand"];
            var industry = HttpContext.Request.Query["industry"];
            var packing = HttpContext.Request.Query["packing"];
            var itemType = HttpContext.Request.Query["itemType"];
            string typeDoc = HttpContext.Request.Query["typeDoc"];
            ItemQuery query = new ItemQuery();
            query.search = search;
            query.brand = brand;
            query.itemType = itemType;
            query.packing = packing;
            query.industry = industry;
            query.typeDoc = typeDoc;
            var claims = User.Claims.ToList();

            var cardIdz = claims.FirstOrDefault(c => c.Type == "cardId")?.Value;
            int id = 0;
            query.cardId = cardId;
            if (!string.IsNullOrEmpty(cardIdz))
            {
                Int32.TryParse(cardIdz.ToString(), out id);
                query.cardId = id;
            }

            if (!string.IsNullOrEmpty(typeDoc))
            {
                var (items1, mess1, total1) = await _itemService.GetAllItemsAsync(gridifyQuery, query, query.cardId.Value);
                if (mess1 != null)
                    return BadRequest(new { mess1.Status, mess1.Errors });
                return Ok(new { items = items1, total = total1});
            }

            var (items, mess, total) = await _itemService.GetAllItemsAsync(gridifyQuery, query);
            if (mess != null)
                return BadRequest(new { mess.Status, mess.Errors });
            return Ok(new { items, total});
        }

        //[Authorize]
        [PrivilegeRequirement("Item.Create")]
        [HttpPost]
        [Route("add")]
        public async Task<IActionResult> Create([FromForm] ItemView model)
        {
            if (model == null)
                return BadRequest("data is null");
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var (item, mess) = await _itemService.CreateItemAsync(model);
            if (mess != null)
            {
                return BadRequest(new { mess.Status, mess.Errors });
            }

            var (items, mes) = await _itemService.GetItemByIdAsync(item.Id);
            if (mes != null)
            {
                return BadRequest(new { mes.Status, mes.Errors });
            }

            return Ok(items);
        }

        [PrivilegeRequirement("Item.Edit")]
        [HttpPut("{id}")]
        public async Task<IActionResult> update(int id, [FromForm] ItemView model)
        {
            if (model == null)
                return BadRequest("data is null");
            var (item, mess) = await _itemService.UpdateItemAsync(id, model);
            if (mess != null)
            {
                return BadRequest(new { mess.Status, mess.Errors });
            }

            var (items, mes) = await _itemService.GetItemByIdAsync(item.Id);
            if (mes != null)
            {
                return BadRequest(new { mes.Status, mes.Errors });
            }

            //var itemDTO = _mapper.Map<ItemDTO>(items);
            return Ok(items);
        }

        [PrivilegeRequirement("Item.View")]
        [HttpGet("{id}")]
        public async Task<IActionResult> getById(int id)
        {
            var (items, mess) = await _itemService.GetItemByIdAsync(id);
            if (mess != null)
            {
                return BadRequest(new { mess.Status, mess.Errors });
            }

            //var itemDTO = _mapper.Map<ItemDTO>(item);
            return Ok(items);
        }
        [AllowAnonymous]
        [HttpGet("ItemPromotions")]
        public async Task<IActionResult> getByItemPromotions([FromQuery] GridifyQuery gridifyQuery, string? search, int? cardId)
        { 
            var (items, mess, total) = await _itemService.GetItemPromotions(gridifyQuery, cardId, search);
            if (mess != null)
            {
                return BadRequest(new { mess.Status, mess.Errors });
            }
            return Ok(new { items, total }); 
        }
        [AllowAnonymous]
        [HttpGet("ItemPromotion")]
        public async Task<IActionResult> getByItemPromotion([FromQuery] GridifyQuery gridifyQuery, string? search,int? cardId)
        {

          
            var (items, mess, total) = await _itemService.GetItemPromotion(gridifyQuery, cardId, search);
            if (mess != null)
            {
                return BadRequest(new { mess.Status, mess.Errors });
            }
            return Ok(new { items, total });
            return Ok();
        }
        [AllowAnonymous]
        [HttpGet("{id:int}/bypass")]
        public async Task<IActionResult> getByIdbyPass(int id)
        {
            var (items, mess) = await _itemService.GetItemByIdAsync(id);
            if (mess != null)
            {
                return BadRequest(new { mess.Status, mess.Errors });
            }

            //var itemDTO = _mapper.Map<ItemDTO>(item);
            return Ok(items);
        }

        [PrivilegeRequirement("Item.Delete")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var (check, mes) = await _itemService.DeleteItemAsync(id);
            if (!check)
            {
                return BadRequest(new { mes.Status, mes.Errors });
            }

            return Ok("Thành công");
        }

        [AllowAnonymous]
        [HttpPost("sap-sync")]
        public async Task<IActionResult> SyncFromSap(List<ItemSync> list)
        {
            await _itemService.SyncFromSap(list);
            return Ok();
        }
        [AllowAnonymous]
        [HttpPost("sync")]
        public async Task<IActionResult> Sync()
        {
            await _itemService.SyncFromSap();
            return Ok();
        }
        [PrivilegeRequirement("Item.View")]
        [HttpGet]
        public async Task<IActionResult> GetAllWithPagination(int? cardId, [FromQuery] GridifyQuery gridifyQuery)
        {
            var search = HttpContext.Request.Query["search"];
            var brand = HttpContext.Request.Query["brand"];
            var industry = HttpContext.Request.Query["industry"];
            var packing = HttpContext.Request.Query["packing"];
            var itemType = HttpContext.Request.Query["itemType"];
            string typeDoc = HttpContext.Request.Query["typeDoc"];
            ItemQuery query = new ItemQuery();
            query.search = search;
            query.brand = brand;
            query.itemType = itemType;
            query.packing = packing;
            query.industry = industry;
            query.typeDoc = typeDoc;
            var claims = User.Claims.ToList();

            var cardIdz = claims.FirstOrDefault(c => c.Type == "cardId")?.Value;
            int id = 0;
            query.cardId = cardId;
            if (!string.IsNullOrEmpty(cardIdz))
            {
                Int32.TryParse(cardIdz.ToString(), out id);
                query.cardId = id;
            }

            if (!string.IsNullOrEmpty(typeDoc))
            {
                var (items1, mess1, total1) = await _itemService.GetAllItemsAsync(gridifyQuery, query, query.cardId.Value);
                if (mess1 != null)
                    return BadRequest(new { mess1.Status, mess1.Errors });
                return Ok(new { items = items1, total = total1});
            }

            var (items, mess, total) = await _itemService.GetAllItemsAsync(gridifyQuery, query);
            if (mess != null)
                return BadRequest(new { mess.Status, mess.Errors });
            return Ok(new { items, total});
        }
        [PrivilegeRequirement("Item.View")]
        [HttpGet]
        [Route("ychg")]
        public async Task<IActionResult> GetAllWithPaginationIG(int? cardId, [FromQuery] GridifyQuery gridifyQuery)
        {
            var search = HttpContext.Request.Query["search"];
            var brand = HttpContext.Request.Query["brand"];
            var industry = HttpContext.Request.Query["industry"];
            var packing = HttpContext.Request.Query["packing"];
            var itemType = HttpContext.Request.Query["itemType"];
            string typeDoc = HttpContext.Request.Query["typeDoc"];
            ItemQuery query = new ItemQuery();
            query.search = search;
            query.brand = brand;
            query.itemType = itemType;
            query.packing = packing;
            query.industry = industry;
            query.typeDoc = typeDoc;
            var claims = User.Claims.ToList();

            var cardIdz = claims.FirstOrDefault(c => c.Type == "cardId")?.Value;
            int id = 0;
            query.cardId = cardId;
            if (!string.IsNullOrEmpty(cardIdz))
            {
                Int32.TryParse(cardIdz.ToString(), out id);
                query.cardId = id;
            }

            if (!string.IsNullOrEmpty(typeDoc))
            {
                var (items1, mess1, total1) = await _itemService.GetAllItemsIgAsync(gridifyQuery, query, query.cardId.Value);
                if (mess1 != null)
                    return BadRequest(new { mess1.Status, mess1.Errors });
                return Ok(new { items = items1, total = total1 });
            }

            var (items, mess, total) = await _itemService.GetAllItemsAsync(gridifyQuery, query);
            if (mess != null)
                return BadRequest(new { mess.Status, mess.Errors });
            return Ok(new { items, total });
        }

        // [PrivilegeRequirement("Item.View")]
        [HttpGet]
        [Route("hierarchy")]
        public async Task<IActionResult> gethierarchy(int? cardId)
        {
            var claims = User.Claims.ToList();

            int cardIdz;
            int.TryParse(claims.FirstOrDefault(c => c.Type == "cardId")?.Value, out cardIdz);
            var (items, mess) = await _itemService.GetHierarchySpecAsync(cardIdz == 0 ? cardId : cardIdz);
            if (mess != null)
                return BadRequest(new { mess.Status, mess.Errors });
            return Ok(new { items });
        }

        [PrivilegeRequirement("Item.Edit")]
        [HttpPut("Onhand/{itemCode}")]
        public async Task<IActionResult> update(string itemCode, ItemOnhand model)
        {
            var (item, mess) = await _itemService.UpdateItemAsync(itemCode, model);
            if (mess != null)
            {
                return BadRequest(new { mess.Status, mess.Errors });
            }

            return Ok("Thành công");
        }
    }
}

public class ProductController(AppDbContext dbContext) : Controller
{
    [HttpGet("api/product-groups")]
    public async Task<IActionResult> GetProductGroups()
    {
        var productGroup = await dbContext.ProductGroup.ToListAsync();

        return Ok(productGroup);
    }

    [HttpGet("api/product-applications")]
    public async Task<IActionResult> GetProductApplications()
    {
        var productGroup = await dbContext.ProductApplications.ToListAsync();

        return Ok(productGroup);
    }

    [HttpGet("api/product-quality-levles")]
    public async Task<IActionResult> GetProductQua()
    {
        var productGroup = await dbContext.ProductQualityLevel.ToListAsync();

        return Ok(productGroup);
    }
}