using BackEndAPI.Models.Account;
using BackEndAPI.Models.Other;
using BackEndAPI.Models.Promotion;
using Gridify;
using Microsoft.AspNetCore.Identity;

namespace BackEndAPI.Service.Account
{
    public interface IAccountService
    {
        Task<(AppUser, Mess)> RegisterUserAsync(AccountView model);
        Task<(AppUser, Mess)> UpdateUserAsync(int id, AccountUpdateView model);
        Task<(AppUser, Mess)> GetUserById(int id);
        Task<(AppUser, Mess)> DeActiveUser(int UserId, string Status);
        Task<(AppUser, Mess)> DeleteUser(int UserId);
        Task<(IEnumerable<AppUserClaim>, Mess)> GetUserClaim(string UserId);
        Task<(IEnumerable<AppUserRole>, Mess)> GetUserRole(string UserId);
        Task<(AppRole, Mess)> AddUpdateUserClaims(int UserId, List<AppRoleClaim> userClaims);

        Task<(IEnumerable<AppUser>, Mess, int total)> GetUserAsync(int skip, int limit, GridifyQuery q, string? search,
            string userType = "");

        Task<(string, AppUser, Mess)> AuthenticateUser(Login login);
        Task<string> GenerateJSONWebToken(AppUser userInfo);
        Task<(string, AppUser, Mess)> TwoFA(string otp, string email);
        Task<(bool, Mess)> ConfirmEmail(string userId, string token);
        Task<(AppUser, Mess)> ChangePassword(int userId, string oldPassword, string newPassword, int type =0);
        Task<(List<CreateUserResponse>?, Mess?)> CreateUserWithBp(List<AccountBP> bpIds);
        Task ResetPassword(ResetPasswordRequest rq);
        Task ForgotPassword(string email);
        Task<(List<AppSetting>, Mess)> GetAppSetting();
        Task<(List<AppSetting>, Mess)> UpdateAppSetting(List<AppSetting> app);
    }
}