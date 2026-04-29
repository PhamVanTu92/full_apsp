using System.ComponentModel.DataAnnotations.Schema;
using BackEndAPI.Models.Account;

namespace BackEndAPI.Models;

public class OrganizationUnit
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty; // Tên phòng ban, bộ phận
    public int? ParentId { get; set; }               // Đơn vị cha (nếu có)
    public OrganizationUnit? Parent { get; set; }    // Điều hướng quan hệ
    public List<OrganizationUnit> Children { get; set; } = new();

    public string Code { get; set; } = string.Empty; // Mã đơn vị
    public string? Description { get; set; }         // Mô tả
    public int Level { get; set; }                   // Cấp bậc trong công ty (VD: 1 = Ban giám đốc, 2 = Phòng ban,...)
    public bool IsActive { get; set; } = true;       // Trạng thái hoạt động

    // Các thông tin bổ sung
    public int? ManagerUserId { get; set; } // Người quản lý đơn vị
    public AppUser? ManagerUser { get; set; }
    [NotMapped] public int EmployeesCount => Employees.Count();
    public List<AppUser> Employees { get; set; } = new();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

public class OrganizationUnitDto
{
    public string Name { get; set; } = string.Empty; // Tên phòng ban, bộ phận
    public int? ParentId { get; set; }               // Đơn vị cha (nếu có)
    public string Code { get; set; } = string.Empty; // Mã đơn vị
    public string? Description { get; set; }         // Mô tả
    public int Level { get; set; }                   // Cấp bậc trong công ty (VD: 1 = Ban giám đốc, 2 = Phòng ban,...)
    public bool IsActive { get; set; } = true;       // Trạng thái hoạt động
    public int? ManagerUserId { get; set; }          // Người quản lý đơn vị

    public List<int> EmployeesIds { get; set; } = [];
}