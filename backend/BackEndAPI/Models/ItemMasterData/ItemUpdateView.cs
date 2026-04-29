using System.ComponentModel.DataAnnotations;

namespace BackEndAPI.Models.ItemMasterData
{
    public class ItemUpdateView
    {
        public int Id { get; set; }
        public int? Series { get; set; }
        [MaxLength(50)]
        [Required(ErrorMessage = "Mã hàng trống")]
        public string? ItemCode { get; set; }
        [MaxLength(254)]
        [Required(ErrorMessage = "Tên hàng trống")]
        public string? ItemName { get; set; }
        [MaxLength(254)]
        public string? FrgnName { get; set; }
        public int? ItmsGrpCode { get; set; }
        public int? UgpEntry { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Giá phải lớn hơn 0")]
        public double? Price { get; set; }
        [MaxLength(50)]
        public string? CodeBars { get; set; }
        public ICollection<ITM1> ITM1 { get; set; }
    }
}
