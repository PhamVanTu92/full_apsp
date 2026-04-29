using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using BackEndAPI.Models.Account;
using BackEndAPI.Models.ItemMasterData;

namespace BackEndAPI.Models.Cart;

// Model: Cart
public class Cart
{
    public int Id { get; set; }
    public int UserId { get; set; } // Mã định danh người dùng
    [JsonIgnore] public AppUser? User { get; set; }
    public List<CartItem> Items { get; set; } = new List<CartItem>();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

// Model: CartItem
public class CartItem
{
    public int Id { get; set; }
    public int CartId { get; set; } // Foreign key đến Cart
    public int? ItemId { get; set; } // Mã sản phẩm
    public Item? Item { get; set; }
    public int Quantity { get; set; } // Số lượng sản phẩm
    [MaxLength(100)]
    public string? PaymentMethodCode { get; set; }
    [NotMapped]
    public double Price => Item?.Price ?? 0;
}