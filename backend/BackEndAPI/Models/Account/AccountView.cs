using BackEndAPI.Models.BusinessPartners;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackEndAPI.Models.Account
{
    public class AccountUpdateView
    {
        [Required] public int Id { get; set; }
        [EmailAddress] public string Email { get; set; }
        [PersonalData] [MaxLength(254)] public string FullName { get; set; }
        [MaxLength(1000)] public string? Address { get; set; }
        public int LocationId { get; set; }
        [MaxLength(254)] public string? LocationName { get; set; }
        public int AreaId { get; set; }
        [MaxLength(254)] public string? AreaName { get; set; }
        [MaxLength(25)] public string? Phone { get; set; }

        [PersonalData] [MaxLength(254)] public string? Note { get; set; }
        [PersonalData] public DateTime? DOB { get; set; }
        [MaxLength(10)] public string? IsAllBranch { get; set; }
        [MaxLength(10)] public string? IsInforOther { get; set; }
        [MaxLength(10)] public string? IsAllGeneral { get; set; }
        [DataType(DataType.Password)] public string? Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Mật khẩu và mật khẩu nhập lại không khớp")]
        public string? ConfirmPassword { get; set; }

        public int? RoleId { get; set; }
        public int? BranchId { get; set; }
        [MaxLength(254)] public string? BranchName { get; set; }
        [NotMapped] public ICollection<CustomerPointCycleDTO>? CustomerPoints { get; set; }
    }

    public class AccountView
    {
        [Required] [EmailAddress] public string Email { get; set; }
        [Required] public string UserName { get; set; }
        [PersonalData] [MaxLength(254)] public string FullName { get; set; }
        [MaxLength(1000)] public string? Address { get; set; }
        public int LocationId { get; set; }
        [MaxLength(254)] public string? LocationName { get; set; }
        public int AreaId { get; set; }
        [MaxLength(254)] public string? AreaName { get; set; }
        [MaxLength(25)] public string? Phone { get; set; }

        [PersonalData] [MaxLength(254)] public string? Note { get; set; }
        [PersonalData] public DateTime DOB { get; set; }
        [MaxLength(10)] public string? IsAllBranch { get; set; }
        [MaxLength(10)] public string? IsInforOther { get; set; }
        [MaxLength(10)] public string? IsAllGeneral { get; set; }
        [DataType(DataType.Password)] public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Mật khẩu và mật khẩu nhập lại không khớp")]
        public string ConfirmPassword { get; set; }
                            
        public string? UserType { get; set; }
        
        public int? CardId { get; set; }

        [Required] public int RoleId { get; set; }
        public int? BranchId { get; set; }
        [MaxLength(254)] public string? BranchName { get; set; }
    }

    public class AccountRegister
    {
        [Required] [EmailAddress] public string? Email { get; set; }
        [Required] public string? UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        [Required]
        [Display(Name = "Confirm Password")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Mật khẩu và mật khẩu nhập lại không khớp")]
        public string? ConfirmPassword { get; set; }
    }

    public class ChangePassword
    {
        [Required]
        [DataType(DataType.Password)]
        public string? OldPassword { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string? NewPassword { get; set; }
    }

    public class UserClaimView
    {
        [Required] public string ClaimType { get; set; }
        [Required] public string ClaimValue { get; set; }
    }

    public class Login
    {
        [Required(ErrorMessage = "Người dùng trống")]
        public string? UserName { get; set; }

        [Required(ErrorMessage = "Mật khẩu trống")]
        [DataType(DataType.Password)]
        public string? Password { get; set; }
    }
}