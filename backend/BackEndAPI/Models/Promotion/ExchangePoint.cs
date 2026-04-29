using BackEndAPI.Models.ItemGroups;
using BackEndAPI.Models.ItemMasterData;
using CommandLine.Text;
using NHibernate.Criterion;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace BackEndAPI.Models.Promotion
{
    public class ExchangePoint
    {
        public int Id { get; set; }
        [MaxLength(254)] public string? Name { get; set; }
        public string? Note { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
        public bool IsAllCustomer { get; set; }
        public ICollection<PointCustomer>? PointCustomer { get; set; }
        public ICollection<ExchangePointLine>? ExchangePointLine { get; set; }
    }
    public class ExchangePointLine
    {
        public int Id { get; set; }
        public int FatherId { get; set; }
        [JsonIgnore] public ExchangePoint? ExchangePoint { get; set; }
        public int ItemId { get; set; }
        [MaxLength(50)] public string? ItemCode { get; set; }
        [MaxLength(254)] public string ItemName { get; set; }
        public int PackingId { get; set; }
        public Packing Packing { get; set; }
        public int Point {  get; set; }
    }
    public class PointCustomer
    {
        public int Id { get; set; }
        public int FatherId { get; set; }
        [JsonIgnore] public ExchangePoint? ExchangePoint { get; set; }
        [MaxLength(10)]
        public string Type { get; set; }
        public int CustomerId { get; set; }
        [MaxLength(50)] public string? CustomerCode { get; set; }
        [MaxLength(254)] public string CustomerName { get; set; }
        [NotMapped] public string? Status { get; set; }
    }
    public class ExchangePointReadDto
    {
        public int Id { get; set; }

        [SwaggerSchema(Description = "Tên chương trình đổi điểm")]
        public string? Name { get; set; }

        [SwaggerSchema(Description = "Ghi chú chương trình")]
        public string? Note { get; set; }

        [SwaggerSchema(Description = "Ngày bắt đầu")]
        public DateTime StartDate { get; set; }

        [SwaggerSchema(Description = "Ngày kết thúc")]
        public DateTime EndDate { get; set; }

        [SwaggerSchema(Description = "Trạng thái hoạt động")]
        public bool IsActive { get; set; }
        public bool IsAllCustomer { get; set; }
        [SwaggerSchema(Description = "Danh sách sản phẩm trong chương trình")]
        public List<ExchangePointLineReadDto>? Lines { get; set; }

        [SwaggerSchema(Description = "Danh sách khách hàng tham gia")]
        public List<PointCustomerReadDto>? Customers { get; set; }
    }

    public class ExchangePointLineReadDto
    {
        public int Id { get; set; }

        [SwaggerSchema(Description = "ID sản phẩm")]
        public int ItemId { get; set; }

        [SwaggerSchema(Description = "Mã sản phẩm")]
        public string? ItemCode { get; set; }

        [SwaggerSchema(Description = "Tên sản phẩm")]
        public string ItemName { get; set; }

        [SwaggerSchema(Description = "ID đơn vị đóng gói")]
        public int PackingId { get; set; }

        [SwaggerSchema(Description = "Tên đơn vị đóng gói")]
        public string? PackingName { get; set; }

        [SwaggerSchema(Description = "Điểm quy đổi")]
        public int Point { get; set; }
    }

    public class PointCustomerReadDto
    {
        public int Id { get; set; }

        [SwaggerSchema(Description = "Loại khách hàng (VIP/NORMAL/ALL)")]
        public string Type { get; set; }

        [SwaggerSchema(Description = "ID khách hàng")]
        public int CustomerId { get; set; }

        [SwaggerSchema(Description = "Mã khách hàng")]
        public string? CustomerCode { get; set; }

        [SwaggerSchema(Description = "Tên khách hàng")]
        public string CustomerName { get; set; }
    }
    /// <summary>
/// DTO tạo mới chương trình đổi điểm
/// </summary>
public class ExchangePointCreateDto
{
    /// <summary>Tên chương trình đổi điểm</summary>
    public string? Name { get; set; }

    /// <summary>Ghi chú nội bộ</summary>
    public string? Note { get; set; }

    /// <summary>Ngày bắt đầu áp dụng</summary>
    public DateTime StartDate { get; set; }

    /// <summary>Ngày kết thúc áp dụng</summary>
    public DateTime EndDate { get; set; }

    /// <summary>Có đang hoạt động hay không</summary>
    public bool IsActive { get; set; }
    public bool IsAllCustomer { get; set; }
    /// <summary>Danh sách sản phẩm được đổi điểm</summary>
    public List<ExchangePointLineCreateDto>? Lines { get; set; }

    /// <summary>Danh sách khách hàng áp dụng</summary>
    public List<PointCustomerCreateDto>? Customers { get; set; }
}
    public class ExchangePointLineCreateDto
    {
        [Required]
        [SwaggerSchema("ID của mặt hàng trong hệ thống")]
        public int ItemId { get; set; }

        [MaxLength(50)]
        [SwaggerSchema("Mã sản phẩm")]
        public string? ItemCode { get; set; }

        [Required]
        [MaxLength(254)]
        [SwaggerSchema("Tên sản phẩm")]
        public string ItemName { get; set; }

        [Required]
        [SwaggerSchema("ID đơn vị đóng gói (Packing)")]
        public int PackingId { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        [SwaggerSchema("Số điểm quy đổi")]
        public int Point { get; set; }
    }
    public class PointCustomerCreateDto
    {
        [Required]
        [MaxLength(10)]
        [SwaggerSchema("Khách hàng - C , Nhóm khách hàng G)")]
        public string Type { get; set; }

        [Required]
        [SwaggerSchema("ID khách hàng hoặc Nhóm khách hàng")]
        public int CustomerId { get; set; }

        [MaxLength(50)]
        [SwaggerSchema("Mã khách hàng (nếu có)")]
        public string? CustomerCode { get; set; }

        [Required]
        [MaxLength(254)]
        [SwaggerSchema("Tên khách hàng hoặc Tên nhóm khách hàng")]
        public string CustomerName { get; set; }
    }
    public class ExchangePointUpdateDto
    {
        [Required]
        [SwaggerSchema(Description = "ID chương trình cần cập nhật")]
        public int Id { get; set; }

        [Required]
        [MaxLength(254)]
        [SwaggerSchema(Description = "Tên chương trình đổi điểm")]
        public string? Name { get; set; }

        [SwaggerSchema(Description = "Ghi chú thêm cho chương trình")]
        public string? Note { get; set; }

        [Required]
        [SwaggerSchema(Description = "Ngày bắt đầu chương trình (yyyy-MM-dd)")]
        public DateTime StartDate { get; set; }

        [Required]
        [SwaggerSchema(Description = "Ngày kết thúc chương trình (yyyy-MM-dd)")]
        public DateTime EndDate { get; set; }

        [SwaggerSchema(Description = "Trạng thái hoạt động (true = đang chạy, false = ngưng)")]
        public bool IsActive { get; set; }
        public bool IsAllCustomer { get; set; }

        [SwaggerSchema(Description = "Danh sách mặt hàng cập nhật cho chương trình")]
        public List<ExchangePointLineUpdateDto>? Lines { get; set; }

        [SwaggerSchema(Description = "Danh sách khách hàng cập nhật cho chương trình")]
        public List<PointCustomerUpdateDto>? Customers { get; set; }
    }
    public class ExchangePointLineUpdateDto
    {
        [Required]
        [SwaggerSchema(Description = "ID chi tiết sản phẩm trong chương trình cần sửa")]
        public int Id { get; set; }

        [Required]
        [SwaggerSchema(Description = "ID mặt hàng")]
        public int ItemId { get; set; }

        [MaxLength(50)]
        [SwaggerSchema(Description = "Mã sản phẩm")]
        public string? ItemCode { get; set; }

        [Required]
        [MaxLength(254)]
        [SwaggerSchema(Description = "Tên sản phẩm")]
        public string ItemName { get; set; }

        [Required]
        [SwaggerSchema(Description = "ID đơn vị đóng gói (Packing)")]
        public int PackingId { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        [SwaggerSchema(Description = "Số điểm quy đổi cho sản phẩm")]
        public int Point { get; set; }
    }
    public class PointCustomerUpdateDto
    {
        [Required]
        [SwaggerSchema(Description = "ID khách hàng tham gia chương trình cần cập nhật")]
        public int Id { get; set; }

        [Required]
        [MaxLength(10)]
        [SwaggerSchema(Description = "Loại khách hàng - C , Nhóm khách hàng - G")]
        public string Type { get; set; }

        [Required]
        [SwaggerSchema(Description = "ID khách hàng hoặc nhóm khách hàng")]
        public int CustomerId { get; set; }

        [MaxLength(50)]
        [SwaggerSchema(Description = "Mã khách hàng")]
        public string? CustomerCode { get; set; }

        [Required]
        [MaxLength(254)]
        [SwaggerSchema(Description = "Tên khách hàng hoặc tên nhóm khách hàng")]
        public string CustomerName { get; set; }
    }
}
