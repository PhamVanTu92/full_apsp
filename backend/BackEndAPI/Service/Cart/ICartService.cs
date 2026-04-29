namespace BackEndAPI.Service.Cart;

public interface ICartService
{
    Task SaveCartAsync(Models.Cart.Cart cartDto);
    Task<Models.Cart.Cart> GetCartByUserIdAsync(int userId);
    Task<Models.Cart.Cart> AddItemToCart(int userId, List<Models.Cart.CartItem> cartItem);
    Task<Models.Cart.Cart> UpdateQuantityItemInCart(int userId, int itemId, int newQuantity, string? paymentMethodCode);
    Task<Models.Cart.Cart> RemoveItemFormCart(int userId, int itemId);
    Task<Models.Cart.Cart> RemoveBulkItemFormCart(int userId, List<int> itemId);
    Task<Models.Cart.Cart> UpdatePaymentMethodInCart(int userId, List<int> itemIds,
        string? paymentMethodCode);
}