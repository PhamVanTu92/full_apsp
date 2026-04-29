using BackEndAPI.Data;
using BackEndAPI.Models.Cart;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.EntityFrameworkCore;

namespace BackEndAPI.Service.Cart;

public class CartService : ICartService
{
    private readonly AppDbContext _context;

    public CartService(AppDbContext context)
    {
        _context = context;
    }

    public async Task SaveCartAsync(Models.Cart.Cart cartDto)
    {
        // Tìm giỏ hàng hiện tại của người dùng
        var cart = await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == cartDto.UserId);

        if (cart == null)
        {
            _context.Carts.Add(cartDto);
        }
        else
        {
            cart.Items.Clear();
            cart.Items.AddRange(cartDto.Items);
        }

        await _context.SaveChangesAsync();
    }

    public async Task<Models.Cart.Cart> AddItemToCart(int userId, List<CartItem> cartItem)
    {
        var cart = await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart == null)
        {
            _context.Carts.Add(new Models.Cart.Cart()
            {
                UserId = userId,
                Items = cartItem,
            });
        }
        else
        {
            foreach (var item in cartItem)
            {
                var its = cart.Items.FirstOrDefault(it => it.ItemId == item.ItemId);
                if (its == null)
                {
                    cart.Items.Add(item);
                    continue;
                }

                its.Quantity += item.Quantity;
            }
        }


        await _context.SaveChangesAsync();
        return cart;
    }

    public async Task<Models.Cart.Cart> UpdateQuantityItemInCart(int userId, int itemId, int newQuantity,
        string? paymentMethodCode)
    {
        var cart = await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart == null)
        {
            throw new NullReferenceException("Cart");
        }

        cart.Items.FirstOrDefault(x => x.ItemId == itemId).Quantity = newQuantity;
        if (paymentMethodCode != null)
        {
            cart.Items.FirstOrDefault(x => x.ItemId == itemId).PaymentMethodCode = paymentMethodCode;
        }

        await _context.SaveChangesAsync();

        return cart;
    }

    public async Task<Models.Cart.Cart> UpdatePaymentMethodInCart(int userId, List<int> itemIds,
        string? paymentMethodCode)
    {
        var cart = await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == userId);
        if (cart == null)
        {
            throw new NullReferenceException("Cart");
        }

        cart.Items.Where(x => itemIds.Contains(x.Id)).ToList()
            .ForEach(x => { x.PaymentMethodCode = paymentMethodCode; });

        await _context.SaveChangesAsync();
        return cart;
    }

    public async Task<Models.Cart.Cart> RemoveItemFormCart(int userId, int itemId)
    {
        var cart = await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == userId);
        if (cart == null)
        {
            throw new KeyNotFoundException();
        }

        cart.Items.RemoveAll(x => x.ItemId == itemId);

        await _context.SaveChangesAsync();

        return cart;
    }

    public async Task<Models.Cart.Cart> RemoveBulkItemFormCart(int userId, List<int> itemId)
    {
        var cart = await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == userId);
        if (cart == null)
        {
            throw new KeyNotFoundException();
        }

        cart.Items.RemoveAll(x => itemId.Contains(x.Id));

        await _context.SaveChangesAsync();

        return cart;
    }

    public async Task<Models.Cart.Cart> GetCartByUserIdAsync(int userId)
    {
        var cart = await _context.Carts
            .AsNoTracking()
            .Include(c => c.Items)
            .ThenInclude(c => c.Item)
            .ThenInclude(c => c.Packing)
            .Include(c => c.Items)
            .ThenInclude(c => c.Item)
            .ThenInclude(c => c.ITM1)
            .Include(c => c.Items)
            .ThenInclude(c => c.Item)
            .ThenInclude(c => c.TaxGroups)
            .Include(c => c.Items)
            .ThenInclude(c => c.Item)
            .ThenInclude(c => c.Packing)
            .Include(c => c.Items)
            .ThenInclude(c => c.Item)
            .ThenInclude(c => c.OUGP)
            .ThenInclude(c => c.OUOM)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart == null) return null;

        return cart;
    }
}