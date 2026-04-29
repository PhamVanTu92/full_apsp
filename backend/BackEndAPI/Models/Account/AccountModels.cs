using BackEndAPI.Models.ARInvoice;
using BackEndAPI.Models.BusinessPartners;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using BackEndAPI.Models.BPGroups;
using BackEndAPI.Models.Other;

namespace BackEndAPI.Models.Account
{
    public class AppUser : IdentityUser<int>
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ICollection<AppUserClaim>? Claims { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ICollection<AppUserLogin>? Logins { get; set; }

        public bool IsSupperUser { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ICollection<AppUserToken>? Tokens { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]

        public int? RoleId { get; set; }

        public int? PersonRoleId { get; set; }
        public AppRole? PersonRole { get; set; }
        public AppRole? Role { get; set; }

        public ICollection<AppUserRole>? UserRoles { get; set; }

        [PersonalData] [MaxLength(254)] public string FullName { get; set; }

        [MaxLength(25)] public string? Phone { get; set; }

        [MaxLength(25)] public string? UserType { get; set; } = "APSP";

        [PersonalData] [MaxLength(254)] public string? Note { get; set; }

        [PersonalData] public DateTime? DOB { get; set; }

        [JsonIgnore] public override string? PasswordHash { get; set; }

        [JsonIgnore] public override bool LockoutEnabled { get; set; }

        [JsonIgnore] public override int AccessFailedCount { get; set; }

        public override bool EmailConfirmed { get; set; }

        [JsonIgnore] public override string? SecurityStamp { get; set; }

        [JsonIgnore] public override string? ConcurrencyStamp { get; set; } = Guid.NewGuid().ToString();

        [JsonIgnore] public override bool PhoneNumberConfirmed { get; set; }

        [JsonIgnore] public override bool TwoFactorEnabled { get; set; }

        [JsonIgnore] public override DateTimeOffset? LockoutEnd { get; set; }

        public string? Status { get; set; } = "A";
        public int? CardId { get; set; }
        public BP? BPInfo { get; set; }
        public ICollection<UserGroup>? UserGroup { get; set; }

        [JsonIgnore] public ICollection<Approval.Approval>? ApprovalInfo { get; set; }

        [JsonIgnore] public ICollection<Approval.ApprovalLine>? ApprovalLine { get; set; }

        public DateTime? LastLogin { get; set; }

        public int? OrganizationId { get; set; }

        [NotMapped] public ICollection<CustomerPointCycleDTO>? CustomerPoints { get; set; }
        public List<AppUser> DirectStaff { get; set; } = [];

        [JsonIgnore] public string? OtpCode { get; set; }

        [JsonIgnore] public DateTime? OtpExpiredAt { get; set; }
    }

    public class CreateUserResponse
    {
        public string? UserName { get; set; }
        public int BPId { get; set; }
        public string BPCardCode { get; set; }
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class UserGroup
    {
        [Key] public int Id { get; set; }

        public string Name { get; set; }
        public bool IsActive { get; set; }
        public ICollection<AppUser>? ListUsers { get; set; }
        public string? Description { get; set; }

        public DateTime? CreatedAt { get; init; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
    }

    public class AppRole : IdentityRole<int>
    {
        [JsonIgnore] public virtual ICollection<AppUserRole>? UserRoles { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public virtual List<AppRoleClaim> RoleClaims { get; set; } = new List<AppRoleClaim>();

        public bool PartialChecked { get; set; }

        public bool IsPersonRole { get; set; }

        [NotMapped] public List<int> PrivilegeIds { get; set; } = new List<int>();

        public bool IsSaleRole { get; set; }
        public bool IsFillCustomerGroup { get; set; }

        public bool IsActive { get; set; } = true;

        [NotMapped] public int CountUserInRole { get; set; }

        public string? Notes { get; set; }

        public List<RoleFillCustomerGroup> RoleFillCustomerGroups { get; set; } = [];
    }

    public class RoleFillCustomerGroup
    {
        public int CustomerGroupId { get; set; }
        [JsonIgnore] public OCRG? Ocrg { get; init; }
        public int RoleId { get; set; }

        [NotMapped] public string? GroupName => Ocrg?.GroupName;
    }

    public class AppUserRole : IdentityUserRole<int>
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public virtual AppUser User { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public AppRole Role { get; set; }
    }

    public class AppUserClaim : IdentityUserClaim<int>
    {
        [JsonIgnore] public virtual AppUser User { get; set; }

        public int RoleClaimId { get; set; }
        public bool Checked { get; set; }
        public bool PartialChecked { get; set; }
    }

    public class AppUserClaimView
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string ClaimType { get; set; }
        public string ClaimValue { get; set; }
        public int RoleClaimId { get; set; }
        public bool Checked { get; set; }
        public bool PartialChecked { get; set; }
    }

    public class AppUserLogin : IdentityUserLogin<int>
    {
        [JsonIgnore] public virtual AppUser User { get; set; }
    }

    public class AppRoleClaim : IdentityRoleClaim<int>
    {
        [JsonIgnore] public virtual AppRole? Role { get; set; }

        [JsonIgnore] public Privileges? Privilege { get; set; }

        [NotMapped] public List<AppRoleClaim> Children { get; set; } = new List<AppRoleClaim>();

        public int PrivilegesId { get; set; }
        public bool PartialChecked { get; set; }

        [NotMapped] public string PrivilegeCode => Privilege?.Code ?? "";

        [NotMapped] public string PrivilegeName => Privilege?.Name ?? "";

        [NotMapped] public int NumberOrder => Privilege?.NumberOrder ?? 0;

        public bool Checked { get; set; }
    }

    public class AppUserToken : IdentityUserToken<int>
    {
        [JsonIgnore] public virtual AppUser User { get; set; }
    }

    public class UserBranch
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int BranchId { get; set; }

        [MaxLength(254)] public string? BranchName { get; set; }

        [JsonIgnore] public virtual AppUser User { get; set; }
    }

    public class UserBranchView
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int BranchId { get; set; }

        [MaxLength(254)] public string? BranchName { get; set; }

        public string? Status { get; set; }
    }

    public class ResetPasswordRequest
    {
        public string Email { get; set; }
        public string Token { get; set; }
        public string NewPassword { get; set; }
    }

    public class ForgotPasswordRequest
    {
        public string Email { get; set; }
    }

    public class AppSetting
    {
        public int Id { get; set; }
        public string UserType { get; set; }
        public bool Is2FARequired { get; set; } = false;
        public bool IsSessionTimeRequired { get; set; } = false;
        public string TwoFactorType { get; set; } = "Email";
        public int Timeout { get; set; } = 5; // minutes

        public int SessionTime { get; set; } = 30;
    }
}