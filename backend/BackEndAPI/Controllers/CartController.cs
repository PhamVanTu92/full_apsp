using System.Security.Claims;
using BackEndAPI.Models.Cart;
using BackEndAPI.Service.Cart;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic.CompilerServices;

namespace BackEndAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CartController : ControllerBase
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    }

    // POST: api/Cart
    [HttpPost]
    public async Task<IActionResult> SaveCart([FromBody] Cart cartDto)
    {
        try
        {
            await _cartService.SaveCartAsync(cartDto);
            return Ok("Cart saved successfully.");
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // POST: api/Cart
    [HttpPost("me")]
    public async Task<IActionResult> SaveMeCart([FromBody] Cart cartDto)
    {
        try
        {
            var claims = User.Identity as ClaimsIdentity;
            if (claims == null)
            {
                return Unauthorized();
            }

            var userId = claims.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var ok = Int32.TryParse(userId, out int intUserId);
            if (!ok || intUserId == 0)
            {
                return Unauthorized();
            }

            cartDto.UserId = intUserId;

            await _cartService.SaveCartAsync(cartDto);
            return Ok("Cart saved successfully.");
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Lay gio hang theo userId
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    [HttpGet("{userId}")]
    public async Task<IActionResult> GetCart(int userId)
    {
        var cart = await _cartService.GetCartByUserIdAsync(userId);

        if (cart == null) return NotFound("Cart not found.");

        return Ok(cart);
    }

    /// <summary>
    /// Lay thong tin gio hang
    /// </summary>
    /// <returns></returns>
    [HttpGet("me")]
    public async Task<IActionResult> GetMeCart()
    {
        var claims = User.Identity as ClaimsIdentity;
        if (claims == null)
        {
            return Unauthorized();
        }

        var userId = claims.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var ok = Int32.TryParse(userId, out int intUserId);
        if (!ok || intUserId == 0)
        {
            return Unauthorized();
        }

        var cart = await _cartService.GetCartByUserIdAsync(intUserId);

        if (cart == null) return NotFound("Cart not found.");

        return Ok(cart);
    }

    /// <summary>
    /// Xoa 1 san pham khoi ro hang
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("me/bulk")]
    public async Task<IActionResult> RemoveBulkItem(List<int> id)
    {
        var claims = User.Identity as ClaimsIdentity;
        if (claims == null)
        {
            return Unauthorized();
        }

        var userId = claims.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var ok = Int32.TryParse(userId, out int intUserId);
        if (!ok || intUserId == 0)
        {
            return Unauthorized();
        }

        var cart = await _cartService.RemoveBulkItemFormCart(intUserId, id);

        if (cart == null) return NotFound("Cart not found.");

        return Ok(cart);
    }

    /// <summary>
    /// Xoa 1 san pham khoi ro hang
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("me/{id}")]
    public async Task<IActionResult> RemoveItem(int id)
    {
        var claims = User.Identity as ClaimsIdentity;
        if (claims == null)
        {
            return Unauthorized();
        }

        var userId = claims.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var ok = Int32.TryParse(userId, out int intUserId);
        if (!ok || intUserId == 0)
        {
            return Unauthorized();
        }

        var cart = await _cartService.RemoveItemFormCart(intUserId, id);

        if (cart == null) return NotFound("Cart not found.");

        return Ok(cart);
    }

    [HttpPut("me/update-payments/{paymentMethodCode}")]
    public async Task<IActionResult> UpdateItemPayment([FromRoute] string paymentMethodCode, [FromBody] List<int> itemIds)
    {
        var claims = User.Identity as ClaimsIdentity;
        if (claims == null)
        {
            return Unauthorized();
        }

        var userId = claims.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var ok = Int32.TryParse(userId, out int intUserId);
        if (!ok || intUserId == 0)
        {
            return Unauthorized();
        }

        var cart = await _cartService.UpdatePaymentMethodInCart(intUserId, itemIds, paymentMethodCode);

        if (cart == null) return NotFound("Cart not found.");

        return Ok(cart);
    }

    /// <summary>
    /// Cap nhat so luong cua mot san pham trong gio hang
    /// </summary>
    /// <param name="cartItems"></param>
    /// <returns></returns>
    [HttpPut("me")]
    public async Task<IActionResult> UpdateQuantityItem([FromBody] CartItem cartItems)
    {
        var claims = User.Identity as ClaimsIdentity;
        if (claims == null)
        {
            return Unauthorized();
        }

        var userId = claims.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var ok = Int32.TryParse(userId, out int intUserId);
        if (!ok || intUserId == 0)
        {
            return Unauthorized();
        }

        var cart = await _cartService.UpdateQuantityItemInCart(intUserId, cartItems.ItemId ?? 0, cartItems.Quantity,
            cartItems.PaymentMethodCode);

        if (cart == null) return NotFound("Cart not found.");

        return Ok(cart);
    }

    /// <summary>
    /// Them san pham vao gio hang
    /// </summary>
    /// <param name="cartItems"></param>
    /// <returns></returns>
    [HttpPost("me/items")]
    public async Task<IActionResult> AddItemToCart([FromBody] List<CartItem> cartItems)
    {
        var claims = User.Identity as ClaimsIdentity;
        if (claims == null)
        {
            return Unauthorized();
        }

        var userId = claims.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var ok = Int32.TryParse(userId, out int intUserId);
        if (!ok || intUserId == 0)
        {
            return Unauthorized();
        }

        var cart = await _cartService.AddItemToCart(intUserId, cartItems);

        if (cart == null) return NotFound("Cart not found.");

        return Ok(cart);
    }
}