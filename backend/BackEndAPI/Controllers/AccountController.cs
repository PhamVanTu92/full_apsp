using BackEndAPI.Models.Account;
using BackEndAPI.Models.Banks;
using BackEndAPI.Models.Other;
using BackEndAPI.Service.Account;
using BackEndAPI.Service.Areas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Text.Json.Serialization;
using System.Text.Json;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;
using BackEndAPI.Service.Privilege;
using Newtonsoft.Json.Linq;
using System.Data;
using Microsoft.AspNetCore.Identity;
using BackEndAPI.Models.ItemMasterData;
using System.Security.Claims;
using BackEndAPI.Service.Privile;
using Gridify;

namespace BackEndAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService      _iService;
        private readonly IPrivilegesServicecs _iPriService;
        private readonly IHostingEnvironment  _hostingEnvironment;
        private readonly Service.Auth.IRefreshTokenService _refreshTokens;
        private readonly Microsoft.AspNetCore.Identity.UserManager<Models.Account.AppUser> _userManager;

        public AccountController(IAccountService iService, IPrivilegesServicecs iPriService,
            IHostingEnvironment hostingEnvironment,
            Service.Auth.IRefreshTokenService refreshTokens,
            Microsoft.AspNetCore.Identity.UserManager<Models.Account.AppUser> userManager)
        {
            _iPriService        = iPriService;
            _iService           = iService;
            _hostingEnvironment = hostingEnvironment;
            _refreshTokens      = refreshTokens;
            _userManager        = userManager;
        }

        private string? CallerIp =>
            HttpContext?.Connection?.RemoteIpAddress?.ToString();

        [HttpPut("me/change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePassword changePassword)
        {
            var claims = User.Identity as ClaimsIdentity;
            if (claims == null)
            {
                return Unauthorized();
            }

            var userId = claims.FindFirst("userId")?.Value;
            var ok     = Int32.TryParse(userId, out int cardId);
            if (!ok || cardId == 0)
            {
                return Unauthorized();
            }

            var (u, mess) =
                await _iService.ChangePassword(cardId, changePassword.OldPassword, changePassword.NewPassword, 1);

            if (mess != null)
            {
                return BadRequest(mess);
            }

            return Ok(u);
        }

        [HttpPut("{id}/change-password")]
        [PrivilegeRequirement("User.ChangePassword")]
        public async Task<IActionResult> ChangePasswordId(int id, [FromBody] ChangePassword changePassword)
        {
            var (u, mess) = await _iService.ChangePassword(id, "", changePassword.NewPassword);

            if (mess != null)
            {
                return BadRequest(mess);
            }

            return Ok(u);
        }

        [AllowAnonymous]
        [EnableRateLimiting("login")]
        [HttpPost("Login")]
        public async Task<IActionResult> LoginUser(Login model, CancellationToken ct)
        {
            Mess mes = new Mess();
            if (model == null)
            {
                mes.Status = 400;
                mes.Errors = "Dữ liệu trống";
                return BadRequest(mes);
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var (token, appUser, mess) = await _iService.AuthenticateUser(model);
            if (token == null)
            {
                return BadRequest(new { mess.Status, mess.Errors });
            }

            // Phát hành refresh token nếu user authenticated thành công.
            // 2FA flow: appUser=null khi cần OTP — chưa cấp refresh, đợi /verify-otp.
            string? refreshToken = null;
            DateTime? refreshExpiresAt = null;
            if (appUser != null)
            {
                var (rt, exp) = await _refreshTokens.CreateAsync(appUser.Id, CallerIp, ct);
                refreshToken = rt;
                refreshExpiresAt = exp;
            }

            return Ok(new
            {
                token,
                refreshToken,
                refreshExpiresAt,
                appUser
            });
        }

        /// <summary>
        /// Đổi refresh token lấy access token mới (rotation: refresh cũ revoke, cấp refresh mới).
        /// Client nên gọi endpoint này khi access token gần hết hạn (vd: 1-2 phút trước khi expire).
        /// </summary>
        [AllowAnonymous]
        [EnableRateLimiting("login")]
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest req, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(req?.RefreshToken))
                return BadRequest(new { error = "RefreshToken required" });

            try
            {
                var (userId, newRefresh, expiresAt) = await _refreshTokens.RotateAsync(req.RefreshToken, CallerIp, ct);

                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null || user.Status == "D")
                {
                    // User bị disable giữa chừng — revoke luôn refresh vừa cấp
                    await _refreshTokens.RevokeAsync(newRefresh, CallerIp, "User disabled", ct);
                    return Unauthorized(new { error = "User no longer active" });
                }

                var newAccessToken = await _iService.GenerateJSONWebToken(user);

                return Ok(new
                {
                    token = newAccessToken,
                    refreshToken = newRefresh,
                    refreshExpiresAt = expiresAt
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Logout: revoke refresh token. Access token vẫn còn hiệu lực đến khi expire
        /// (JWT stateless không thể revoke ngay) — TTL ngắn 30 phút mitigate.
        /// </summary>
        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest req, CancellationToken ct)
        {
            if (!string.IsNullOrWhiteSpace(req?.RefreshToken))
            {
                await _refreshTokens.RevokeAsync(req.RefreshToken, CallerIp, "Logout", ct);
            }
            return NoContent();
        }

        /// <summary>
        /// Logout-all-devices: revoke toàn bộ refresh token của user hiện tại.
        /// </summary>
        [Authorize]
        [HttpPost("logout-all")]
        public async Task<IActionResult> LogoutAll(CancellationToken ct)
        {
            var userIdStr = (User.Identity as ClaimsIdentity)?.FindFirst("userId")?.Value;
            if (!int.TryParse(userIdStr, out var userId))
                return Unauthorized();

            await _refreshTokens.RevokeAllForUserAsync(userId, CallerIp, "Logout all devices", ct);
            return NoContent();
        }

        public class RefreshTokenRequest
        {
            public string? RefreshToken { get; set; }
        }

        [AllowAnonymous]
        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp(string otp, string email)
        {
            var (token, appUser, mess) = await _iService.TwoFA(otp, email);
            if (token == null)
            {
                return BadRequest(new { mess.Status, mess.Errors });
            }

            if (appUser == null)
                return Ok(new { token = token });
            return Ok(new { token = token, appUser = appUser });
        }

        [Authorize]
        [HttpPost("Register")]
        public async Task<IActionResult> RegisterUser(AccountView model)
        {
            Mess mes = new Mess();
            if (model == null)
            {
                mes.Status = 400;
                mes.Errors = "Dữ liệu trống";
                return BadRequest(mes);
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var (user, mess) = await _iService.RegisterUserAsync(model);
            if (user == null)
            {
                return BadRequest(new { mess.Status, mess.Errors });
            }

            return Ok(user);
        }

        //[Authorize(Policy = "User_Update")]
        [PrivilegeRequirement("User.Edit")]
        [HttpPost("UserClaim/addupdate")]
        public async Task<IActionResult> AddUserClaim(int UserId, [FromBody] List<AppRoleClaim> userClaims)
        {
            Mess mes = new Mess();
            if (userClaims == null)
            {
                mes.Status = 400;
                mes.Errors = "Dữ liệu trống";
                return BadRequest(mes);
            }

            var (userClaim, mess) = await _iService.AddUpdateUserClaims(UserId, userClaims);
            if (userClaim == null)
            {
                return BadRequest(new { mess.Status, mess.Errors });
            }

            return Ok(userClaim);
        }

        [HttpGet("{id}")]
        [PrivilegeRequirement("User.View")]
        //[Authorize(Policy = "User_Read")]
        public async Task<IActionResult> getById(int id)
        {
            var (item, mess) = await _iService.GetUserById(id);
            if (mess != null)
            {
                return BadRequest(new { mess.Status, mess.Errors });
            }


            //string json =
            //    JsonConvert.SerializeObject(item);
            return Ok(new { item });
        }

        [HttpPost("DeActive")]
        //[Authorize(Policy = "User_Update")]
        [PrivilegeRequirement("User.Edit")]
        [PrivilegeRequirement("User.ActiveDeActive")]
        public async Task<IActionResult> DeActiveUser(int id, string Status)
        {
            var (item, mes) = await _iService.DeActiveUser(id, Status);
            if (item == null)
            {
                return BadRequest(new { mes.Status, mes.Errors });
            }

            return Ok("Thành công");
        }

        [HttpPost("register-businesspartner")]
        [Authorize]
        //[PrivilegeRequirement("User.Edit")]
        public async Task<IActionResult> RegisterBusinessPartner([FromBody] List<AccountBP> bpIds)
        {
            var (item, mes) = await _iService.CreateUserWithBp(bpIds);
            if (item == null)
            {
                return BadRequest(new { mes.Status, mes.Errors });
            }

            return Ok(item);
        }

        [HttpDelete("{id}")]
        [AllowAnonymous]
        [PrivilegeRequirement("User.Delete")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var (item, mes) = await _iService.DeleteUser(id);
            if (item == null)
            {
                return BadRequest(new { mes.Status, mes.Errors });
            }

            return Ok("Thành công");
        }

        [PrivilegeRequirement("User.View")]
        //[Authorize(Policy = "User_Read")]
        [HttpGet("userClaim/{UserId}")]
        public async Task<IActionResult> getUserClaimId(string UserId)
        {
            var (item, mess) = await _iService.GetUserClaim(UserId);
            if (mess != null)
            {
                return BadRequest(new { mess.Status, mess.Errors });
            }

            return Ok(item);
        }

        [PrivilegeRequirement("User.View")]
        [HttpGet("getall")]
        public async Task<IActionResult> getAll([FromQuery] GridifyQuery q, [FromQuery] string? search,
            string? userType, int skip = 0, int limit = 30)
        {
            var (item, mess, total) = await _iService.GetUserAsync(skip, limit, q, search, userType);
            if (mess != null)
            {
                return BadRequest(new { mess.Status, mess.Errors });
            }


            return Ok(new { item, total, skip, limit });
        }

        [AllowAnonymous]
        [HttpGet("distributors")]
        public async Task<IActionResult> GetDistributorsAccount([FromQuery] GridifyQuery q, [FromQuery] string? search,
            int skip = 0, int limit = 30)
        {
            var (item, mess, total) = await _iService.GetUserAsync(skip, limit, q, search, "NPP");
            if (mess != null)
            {
                return BadRequest(new { mess.Status, mess.Errors });
            }

            var (privileges, mes) = await _iPriService.GetPrivilegesAsync();
            if (privileges == null)
            {
                return BadRequest(new { mes.Status, mes.Errors });
            }

            return Ok(new { item, total, skip, limit, privileges });
        }

        [AllowAnonymous]
        [HttpGet("system")]
        public async Task<IActionResult> GetSystemAccount([FromQuery] GridifyQuery q, [FromQuery] string? search,
            int skip = 0, int limit = 30)
        {
            var (item, mess, total) = await _iService.GetUserAsync(skip, limit, q, search, "APSP");
            if (mess != null)
            {
                return BadRequest(new { mess.Status, mess.Errors });
            }

            var (privileges, mes) = await _iPriService.GetPrivilegesAsync();
            if (privileges == null)
            {
                return BadRequest(new { mes.Status, mes.Errors });
            }

            return Ok(new { item, total, skip, limit, privileges });
        }

        [HttpPut("{id}")]
        //[Authorize(Policy = "User_Update")]
        [PrivilegeRequirement("User.Edit")]
        public async Task<IActionResult> UpdateUser(int id, AccountUpdateView model)
        {
            var (item, mes) = await _iService.UpdateUserAsync(id, model);
            if (item == null)
            {
                return BadRequest(new { mes.Status, mes.Errors });
            }

            return Ok(model);
        }

        [HttpPut("me")]
        //[Authorize(Policy = "User_Update")]
        public async Task<IActionResult> UpdateCurrentUser(AccountUpdateView model)
        {
            var claims = User.Identity as ClaimsIdentity;
            if (claims == null)
            {
                return Unauthorized();
            }

            var userId = claims.FindFirst("userId")?.Value;
            var ok     = Int32.TryParse(userId, out int cardId);
            if (!ok || cardId == 0)
            {
                return Unauthorized();
            }

            var (item, mes) = await _iService.UpdateUserAsync(cardId, model);
            if (item == null)
            {
                return BadRequest(new { mes.Status, mes.Errors });
            }

            return Ok(model);
        }

        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            var (check, mes) = await _iService.ConfirmEmail(userId, token);

            if (check == false)
            {
                return BadRequest(new { mes.Status, mes.Errors });
            }

            return Ok("Kích hoạt thành công");
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetMe()
        {
            var userClaims = User.Claims.ToList();
            var userId     = userClaims.FirstOrDefault(c => c.Type == "userId")?.Value;

            if (userId == null)
            {
                return Unauthorized();
            }

            var ok = int.TryParse(userId.ToString(), out var iUserId);

            var (user, mess) = await _iService.GetUserById(iUserId);

            if (mess != null)
            {
                return BadRequest(new { mess.Status, mess.Errors });
            }


            return Ok(new { user });
        }

        [AllowAnonymous]
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            await _iService.ForgotPassword(request.Email);

            return Ok("Email đặt lại mật khẩu đã được gửi.");
        }

        [AllowAnonymous]
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequest request)
        {
            await _iService.ResetPassword(request);

            return Ok();
        }
    }
}