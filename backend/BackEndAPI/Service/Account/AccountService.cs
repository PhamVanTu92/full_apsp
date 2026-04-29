using BackEndAPI.Data;
using BackEndAPI.Models.Account;
using BackEndAPI.Models.Other;
using Function.EncryptDecrypt;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Mail;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Transactions;
using BackEndAPI.Service.BusinessPartners;
using BackEndAPI.Service.Mail;
using Gridify;
using Org.BouncyCastle.Utilities;
using System.Threading.Tasks;
using BackEndAPI.Models;
using BackEndAPI.Models.BusinessPartners;

namespace BackEndAPI.Service.Account
{
    public class AccountBP
    {
        public int CardId { get; set; }
        public string Email { get; set; } = string.Empty;
    }

    public class AccountService : IAccountService
    {
        private readonly AppDbContext            _context;
        private readonly UserManager<AppUser>    _userManager;
        private readonly RoleManager<AppRole>    _roleManager;
        private readonly SignInManager<AppUser>  _signInManager;
        private readonly IModelUpdater           _modelUpdater;
        private readonly IConfiguration          _configuration;
        private readonly IBusinessPartnerService _businessPartnerService;
        public readonly  IMailService            _mailService;
        private readonly IHttpContextAccessor    _httpContextAccessor;
        private readonly LoggingSystemService    _systemLog;

        public AccountService(AppDbContext context,
            IModelUpdater modelUpdater,
            UserManager<AppUser> userManager,
            RoleManager<AppRole> roleManager,
            SignInManager<AppUser> signInManager,
            IConfiguration configuration,
            IBusinessPartnerService businessPartnerService,
            IMailService mailService,
            IHttpContextAccessor httpContextAccessor,
            LoggingSystemService systemLog
        )
        {
            _httpContextAccessor    = httpContextAccessor;
            _context                = context;
            _userManager            = userManager;
            _roleManager            = roleManager;
            _signInManager          = signInManager;
            _modelUpdater           = modelUpdater;
            _configuration          = configuration;
            _businessPartnerService = businessPartnerService;
            _mailService            = mailService;
            _systemLog              = systemLog;
        }

        public async Task<(AppUser, Mess)> GetUserById(int id)
        {
            Mess mess = new Mess();
            try
            {
                AppUser app = await _userManager.Users
                    .Include(p => p.Role)
                    .ThenInclude(p => p.RoleClaims)
                    .ThenInclude(p => p.Privilege)
                    .Include(p => p.PersonRole)
                    .ThenInclude(p => p.RoleClaims)
                    .ThenInclude(r => r.Privilege)
                    .Include(x => x.Claims)
                    .FirstOrDefaultAsync(p => p.Id == id && p.Status != "D");
                var (bp, _) = await _businessPartnerService.GetBPByIdAsync(app.CardId ?? 0);
                app.BPInfo  = bp;
                var customerPoint = (from c in _context.CustomerPointCycles
                        join ps in _context.PointSetups
                            on c.PoitnSetupId equals ps.Id
                        where app.CardId == c.CustomerId
                        //&& c.StartDate <= DateTime.UtcNow
                        //&& c.EndDate >= DateTime.UtcNow
                        //&& c.ExpiryDate >= DateTime.UtcNow
                        select new CustomerPointCycleDTO
                        {
                            Id             = c.Id,
                            CustomerId     = c.CustomerId,
                            PoitnSetupId   = c.PoitnSetupId,
                            StartDate      = c.StartDate,
                            EndDate        = c.EndDate,
                            ExpiryDate     = c.ExpiryDate,
                            EarnedPoint    = c.EarnedPoint,
                            RedeemedPoint  = c.RedeemedPoint,
                            RemainingPoint = c.RemainingPoint,
                            Name           = ps.Name
                        })
                    .ToList();
                app.CustomerPoints = customerPoint;
                return (app, null);
            }
            catch (Exception ex)
            {
                mess.Status = 900;
                mess.Errors = ex.Message;
                return (null, mess);
            }
        }

        public static string GenerateRandomPassword(int length = 12)
        {
            // Đảm bảo độ dài mật khẩu hợp lệ (ít nhất 8 ký tự)
            if (length < 8)
            {
                throw new ArgumentException("Password length must be at least 8 characters.");
            }

            // Các ký tự cần thiết cho mật khẩu
            string lowerCase    = "abcdefghijklmnopqrstuvwxyz";     // Chữ cái thường
            string upperCase    = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";     // Chữ cái hoa
            string digits       = "0123456789";                     // Số
            string specialChars = "!@#$%^&*()-_=+[]{}|;:'\",.<>?/"; // Ký tự đặc biệt

            // Tạo chuỗi chứa tất cả ký tự có thể sử dụng
            string allChars = lowerCase + upperCase + digits + specialChars;

            // Dùng StringBuilder để xây dựng mật khẩu
            StringBuilder password = new StringBuilder();

            // Đảm bảo có ít nhất một ký tự thuộc mỗi loại — sử dụng RandomNumberGenerator
            // (cryptographically secure) thay vì System.Random để tránh tấn công đoán seed.
            password.Append(lowerCase[RandomNumberGenerator.GetInt32(lowerCase.Length)]);
            password.Append(upperCase[RandomNumberGenerator.GetInt32(upperCase.Length)]);
            password.Append(digits[RandomNumberGenerator.GetInt32(digits.Length)]);
            password.Append(specialChars[RandomNumberGenerator.GetInt32(specialChars.Length)]);

            // Tạo các ký tự còn lại trong mật khẩu (nếu cần)
            for (int i = password.Length; i < length; i++)
            {
                password.Append(allChars[RandomNumberGenerator.GetInt32(allChars.Length)]);
            }

            // Shuffle để vị trí của 4 ký tự "bắt buộc" không cố định ở 4 vị trí đầu
            // (giảm bớt thông tin attacker biết trước về cấu trúc password).
            var chars = password.ToString().ToCharArray();
            for (int i = chars.Length - 1; i > 0; i--)
            {
                int j = RandomNumberGenerator.GetInt32(i + 1);
                (chars[i], chars[j]) = (chars[j], chars[i]);
            }
            return new string(chars);
        }


        public async Task<(List<CreateUserResponse>?, Mess?)> CreateUserWithBp(List<AccountBP> bpIds)
        {
            var mess = new Mess();
            try
            {
                var listEmail = bpIds.Select(b => b.Email).ToList();
                var listId    = bpIds.Select(b => b.CardId).ToList();
                var bpList = await _context.BP
                    .Include(b => b.UserInfo)
                    .Where(b => listId.Contains(b.Id)).ToListAsync();
                var listResp = new List<CreateUserResponse>();

                foreach (var bp in bpList)
                {
                    if (bp.PortalRegistrationStatus == "Y")
                    {
                        if (bp.UserInfo.LastLogin != null)
                        {
                            listResp.Add(new CreateUserResponse()
                            {
                                ErrorMessage = "Nhà phân phối này đã có tài khoản",
                                BPId         = bp.Id,
                                BPCardCode   = bp.CardCode ?? "",
                                IsSuccess    = false
                            });
                            continue;
                        }
                    }

                    var         Email = bpIds.Where(b => b.CardId == bp.Id).Select(b => b.Email).FirstOrDefault();
                    AccountView newUser;
                    if (bp.UserInfo == null)
                    {
                        newUser = new AccountView()
                        {
                            Password = GenerateRandomPassword(12),
                            Email    = Email,
                            Address  = bp.Address,
                            UserName = bp.CardCode ?? "",
                            FullName = bp.CardName ?? "",
                            Phone    = bp.Phone,
                            UserType = "NPP",
                            CardId   = bp.Id
                        };

                        var (us, rees) = await RegisterUserAsync(newUser);
                        if (rees != null)
                        {
                            bp.PortalRegistrationStatus = "N";
                            listResp.Add(new CreateUserResponse()
                            {
                                ErrorMessage = rees.Errors,
                                BPId         = bp.Id,
                                BPCardCode   = bp.CardCode ?? "",
                                IsSuccess    = false
                            });
                            continue;
                        }
                        else
                            bp.PortalRegistrationStatus = "Y";
                    }
                    else
                    {
                        newUser = new AccountView()
                        {
                            Password = GenerateRandomPassword(12),
                            Email    = Email,
                            UserName = bp.CardCode ?? "",
                            FullName = bp.CardName ?? "",
                            Phone    = bp.Phone,
                        };

                        var xUser = new AppUser();
                        bp.UserInfo.Email        = Email;
                        bp.UserInfo.PasswordHash = _userManager.PasswordHasher.HashPassword(xUser, newUser.Password);
                        var check = await _mailService.SendMailBy(new InfoLoginMail()
                        {
                            Password     = newUser.Password,
                            Username     = newUser.UserName,
                            ToEmail      = newUser.Email,
                            Name         = newUser.FullName,
                            CustomerName = newUser.FullName,
                        }, 0);
                        bp.UserInfo.EmailConfirmed = check;
                    }

                    listResp.Add(new CreateUserResponse()
                    {
                        BPId       = bp.Id,
                        BPCardCode = bp.CardCode ?? "",
                        UserName   = newUser.UserName,
                        IsSuccess  = true
                    });


                    bp.PortalRegistrationStatus = "P";
                    await _context.SaveChangesAsync();
                }

                return (listResp, mess);
            }
            catch (Exception ex)
            {
                mess.Errors = ex.Message;
                mess.Status = 500;

                return (null, mess);
            }
        }

        public async Task<(AppUser, Mess)> RegisterUserAsync(AccountView model)
        {
            Mess mes = new Mess();
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    AppUser appUser = new AppUser();
                    _modelUpdater.UpdateModel(appUser, model, "NA1", "NA1", "NA2", "NA3", "NA3", "NA");
                    List<UserBranch> userBranchs = new List<UserBranch>();
                    if (AppDbContext.IsUniqueAsync<AppUser>(_context,
                                                            u => u.Email == model.Email && u.CardId != model.CardId))
                    {
                        mes.Status = 400;
                        mes.Errors = "Email đã tồn tại";

                        return (null, mes);
                    }

                    if (AppDbContext.IsUniqueAsync<AppUser>(_context,
                                                            u => u.Phone == model.Phone && u.CardId != model.CardId))
                    {
                        mes.Status = 400;
                        mes.Errors = "Số điện thoại đã tồn tại";

                        return (null, mes);
                    }

                    var result = await _userManager.CreateAsync(appUser, model.Password);

                    var ok = AppDbContext.IsUniqueAsync<AppUser>(_context,
                                                                 u => u.Email == model.Email &&
                                                                     u.CardId != model.CardId);

                    if (result.Succeeded)
                    {
                        if (model.RoleId > 0)
                        {
                            var role = await _context.AppRole.Include(x => x.RoleClaims)
                                .FirstOrDefaultAsync(x => x.Id == model.RoleId);
                            var roleClaims = role.RoleClaims;
                            if (role == null)
                            {
                                var appRole = new AppRole();
                                mes.Status = 400;
                                mes.Errors = "Vai trò không tồn tại";
                                transaction.Rollback();
                                return (null, mes);
                            }

                            var                privileges    = await _context.Privileges.ToListAsync();
                            List<AppUserClaim> listUserClaim = new List<AppUserClaim>();
                            AppRoleClaim       appRoleClaim  = new AppRoleClaim();
                            foreach (var privilege in privileges)
                            {
                                appRoleClaim = roleClaims
                                    .Where(x => x.ClaimType == privilege.Code && x.ClaimValue == privilege.Name)
                                    .FirstOrDefault();
                                if (appRoleClaim != null)
                                {
                                    AppUserClaim appUserClaim = new AppUserClaim();
                                    appUserClaim.UserId      = appUser.Id;
                                    appUserClaim.RoleClaimId = privilege.Id;
                                    appUserClaim.ClaimType   = privilege.Code;
                                    appUserClaim.ClaimValue  = privilege.Name;
                                    listUserClaim.Add(appUserClaim);
                                }
                            }


                            if (listUserClaim.Count > 0)
                                await _context.Set<AppUserClaim>().AddRangeAsync(listUserClaim);
                            AppUserRole appUserRole = new AppUserRole() { RoleId = model.RoleId, UserId = appUser.Id };
                            await _context.Set<AppUserRole>().AddAsync(appUserRole);
                            await _context.SaveChangesAsync();
                        }
                    }
                    else
                    {
                        List<IdentityError> errorList = result.Errors.ToList();

                        if (errorList[0].Description
                            .Equals("Passwords must have at least one non alphanumeric character."))
                        {
                            mes.Status = 500;
                            mes.Errors = "Mật khẩu phải có ít nhất một ký tự không phải chữ và số";
                        }
                        else //if(errorList[0].Description.Contains())
                        {
                            mes.Status = 500;
                            mes.Errors = errorList[0].Description;
                        }

                        transaction.Rollback();
                        return (null, mes);
                    }

                    var type                       = 0;
                    if (model.CardId == null) type = 1;

                    var check = await _mailService.SendMailBy(new InfoLoginMail()
                    {
                        Password     = model.Password,
                        Username     = model.UserName,
                        ToEmail      = model.Email,
                        Name         = model.FullName,
                        CustomerName = model.FullName,
                    }, type);
                    appUser.EmailConfirmed = check;
                    await _context.SaveChangesAsync();
                    transaction.Commit();
                    return (appUser, null);
                }
                catch (Exception ex)
                {
                    mes.Status = 900;
                    mes.Errors = ex.Message;
                    transaction.Rollback();
                    return (null, mes);
                }
            }
        }

        public async Task<(IEnumerable<AppUserClaim>, Mess)> GetUserClaim(string UserId)
        {
            Mess mes = new Mess();
            try
            {
                var user = await _userManager.FindByIdAsync(UserId);
                if (user == null)
                {
                    mes.Status = 400;
                    mes.Errors = "Không tồn tại người dùng";
                    return (null, mes);
                }
                else
                {
                    if (user.Status == "D")
                    {
                        mes.Status = 400;
                        mes.Errors = "Không tồn tại người dùng";
                        return (null, mes);
                    }
                }

                var appUserClaim = await _context.Set<AppUserClaim>().AsQueryable()
                    .Where(x => x.UserId == int.Parse(UserId)).ToListAsync();
                return (appUserClaim, null);
            }
            catch (Exception ex)
            {
                mes.Status = 900;
                mes.Errors = ex.Message;
                return (null, mes);
            }
        }

        public async Task<(AppRole, Mess)> AddUpdateUserClaims(int UserId,
            List<AppRoleClaim> userClaims)
        {
            Mess mes = new Mess();
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var user = await _context.AppUser.AsSplitQuery().AsQueryable()
                        .Include(u => u.PersonRole)
                        .ThenInclude(p => p.RoleClaims)
                        .Where(u => u.Id == UserId).FirstOrDefaultAsync();
                    if (user == null)
                    {
                        mes.Status = 400;
                        mes.Errors = "Không tồn tại người dùng";
                        return (null, mes);
                    }
                    else
                    {
                        if (user.Status == "D")
                        {
                            mes.Status = 400;
                            mes.Errors = "Không tồn tại người dùng";
                            return (null, mes);
                        }
                    }

                    if (user.PersonRole == null)
                    {
                        user.PersonRole              = new AppRole();
                        user.PersonRole.IsPersonRole = true;
                        user.PersonRole.RoleClaims   = new List<AppRoleClaim>();
                    }

                    user.PersonRole.RoleClaims.Clear();

                    await _context.SaveChangesAsync();

                    user.PersonRole.RoleClaims.AddRange(userClaims);


                    await _context.SaveChangesAsync();
                    transaction.Commit();
                    return (user.PersonRole, null);
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    mes.Status = 900;
                    mes.Errors = ex.Message;
                    return (null, mes);
                }
            }
        }

        public async Task<(IEnumerable<AppUser>, Mess, int total)> GetUserAsync(int skip, int limit, GridifyQuery q,
            string? search,
            string userType = "")
        {
            Mess mess = new Mess();
            try
            {
                var query = _context.Users.AsQueryable().ApplyFiltering(q);
                if (!string.IsNullOrEmpty(userType))
                {
                    query = query.Where(p => p.UserType == userType);
                }

                if (!string.IsNullOrEmpty(search))
                {
                    search = search?.ToLower().Trim();
                    query = query.Where(p => p.UserName.Contains(search) ||
                                            p.FullName.Contains(search)  ||
                                            p.Email.Contains(search));
                }

                var totalCount = await query.Where(x => x.Status != "D" && x.UserName != "Admin").CountAsync();
                var appUser = await query
                    .Where(x => x.Status != "D" && x.UserName != "Admin")
                    .ApplyOrdering(q)
                    .Skip(skip * limit)
                    .Take(limit)
                    .Include(x => x.UserRoles)
                    .ThenInclude(r => r.Role)
                    .Include(x => x.Claims)
                    .ToListAsync();
                return (appUser, null, totalCount);
            }
            catch (Exception ex)
            {
                mess.Status = 900;
                mess.Errors = ex.Message;
                return (null, mess, 0);
            }
        }

        public async Task<(IEnumerable<AppUserRole>, Mess)> GetUserRole(string UserId)
        {
            Mess mes = new Mess();
            try
            {
                var user = await _userManager.FindByIdAsync(UserId);
                if (user == null)
                {
                    mes.Status = 400;
                    mes.Errors = "Không tồn tại người dùng";
                    return (null, mes);
                }
                else
                {
                    if (user.Status == "D")
                    {
                        mes.Status = 400;
                        mes.Errors = "Không tồn tại người dùng";
                        return (null, mes);
                    }
                }

                var appUserRole = await _context.Set<AppUserRole>().AsQueryable()
                    .Where(x => x.UserId == int.Parse(UserId)).ToListAsync();
                return (appUserRole, null);
            }
            catch (Exception ex)
            {
                mes.Status = 900;
                mes.Errors = ex.Message;
                return (null, mes);
            }
        }

        public async Task<(IEnumerable<AppRole>, Mess)> GetRole()
        {
            Mess mes = new Mess();
            try
            {
                var appRole = await _context.Set<AppRole>().AsQueryable().ToListAsync();
                return (appRole, null);
            }
            catch (Exception ex)
            {
                mes.Status = 900;
                mes.Errors = ex.Message;
                return (null, mes);
            }
        }

        public async Task<(IEnumerable<AppRoleClaim>, Mess)> GetRoleClaim(int RoleId)
        {
            Mess mes = new Mess();
            try
            {
                var appRoleClaim = await _context.Set<AppRoleClaim>().AsQueryable().Where(r => r.RoleId == RoleId)
                    .ToListAsync();
                return (appRoleClaim, null);
            }
            catch (Exception ex)
            {
                mes.Status = 900;
                mes.Errors = ex.Message;
                return (null, mes);
            }
        }

        public async Task<(string, AppUser, Mess)> AuthenticateUser(Login login)
        {
            Mess mes = new Mess();
            try
            {
                var user = _context.AppUser.AsSplitQuery().AsQueryable().Include(p => p.UserRoles)
                    .ThenInclude(x => x.Role)
                    .Include(x => x.Role)
                    .ThenInclude(x => x.RoleClaims)
                    .ThenInclude(p => p.Privilege)
                    .Include(x => x.PersonRole)
                    .ThenInclude(x => x.RoleClaims)
                    .ThenInclude(p => p.Privilege)
                    .FirstOrDefault(x => x.UserName == login.UserName || x.Email == login.UserName);


                if (user == null)
                {
                    mes.Status = 400;
                    mes.Errors = "Người dùng hoặc mật khẩu không đúng";
                    await _systemLog.SaveWithTransAsync("WARN", "Login",
                                                        $"Người dùng {login.UserName} đăng nhập không thành công",
                                                        "User");
                    return (null, null, mes);
                }


                if (user.Status == "D")
                {
                    mes.Status = 401;
                    mes.Errors = "Tài khoản của bạn đã bị xóa";

                    return (null, null, mes);
                }

                if (user.Status == "I")
                {
                    mes.Status = 403;
                    mes.Errors = "Tài khoản này đã bị vô hiệu hóa";

                    return (null, null, mes);
                }

                var passwordValid = await _userManager.CheckPasswordAsync(user, login.Password);
                if (!passwordValid)
                {
                    mes.Status = 400;
                    mes.Errors = "Người dùng hoặc mật khẩu không đúng";
                    await _systemLog.SaveWithTransAsync("WARN", "Login",
                                                        $"Người dùng {login.UserName} đăng nhập không thành công",
                                                        "User");
                    return (null, null, mes);
                }

                string appCheck = "";
                if (user.UserType == "APSP")
                    appCheck = "APSP";
                else
                    appCheck = "Nhà phân phối";
                var app = _context.AppSetting.FirstOrDefault(e => e.UserType == appCheck);
                if (app.Is2FARequired == true)
                {
                    var otp = new Random().Next(100000, 999999).ToString();
                    user.OtpCode      = otp;
                    user.OtpExpiredAt = DateTime.UtcNow.AddMinutes(app.Timeout);
                    await _userManager.UpdateAsync(user);
                    await _context.SaveChangesAsync();
                    await _mailService.SendMailOTP(new OTPMail
                    {
                        ToEmail = user.Email,
                        OTP     = "Mã xác thực OTP:" + otp
                    });
                    return ("2FA", null, null);
                }

                string token = await GenerateJSONWebToken(user);

                if (user.CardId != null)
                {
                    var bp = await _context.BP.FirstOrDefaultAsync(x => x.Id == user.CardId);
                    if (user.LastLogin == null) bp.PortalRegistrationStatus = "Y";
                }

                user.LastLogin = DateTime.Now;
                await _systemLog.SaveAsync("INFO", "Login", $"Người dùng {user.UserName} đăng nhập", "User", user.Id);
                await _context.SaveChangesAsync();
                return (token, user, null);
            }
            catch (Exception ex)
            {
                mes.Status = 900;
                mes.Errors = ex.Message;
                return (null, null, mes);
            }
        }

        //}
        public async Task<string> GenerateJSONWebToken(AppUser userInfo)
        {
            string key         = EncryptDecrypt.DecryptString(_configuration["Jwt:Key"]);
            var    securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var    credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            string CardId = "";
            if (!string.IsNullOrEmpty(userInfo.CardId?.ToString()))
            {
                CardId = userInfo.CardId?.ToString();
            }

            //string roleName = userInfo.UserRoles.FirstOrDefault().Role.Name;
            var roles = await _userManager.GetRolesAsync(userInfo);
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, userInfo.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, userInfo.Email.ToString()),
                new Claim("cardId", CardId),
                new Claim("userId", userInfo.Id.ToString()),
                new Claim("userType", userInfo.UserType.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }


            // Access token TTL ngắn (30 phút) vì có refresh token gia hạn.
            // Trước đây: 120 phút. Sau khi thêm refresh: rút xuống để giảm impact
            // nếu access token leak (cookie XSS, log mistake).
            var token = new JwtSecurityToken(
                _configuration["Jwt:Issuer"],
                _configuration["Jwt:Audience"],
                claims,
                expires: DateTime.UtcNow.AddMinutes(30),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<(AppUser, Mess)> DeActiveUser(int UserId, string Status)
        {
            Mess mess = new Mess();
            try
            {
                AppUser app = await _userManager.Users
                    .FirstOrDefaultAsync(p => p.Id == UserId && p.Status != "D");

                if (app == null)
                {
                    mess.Status = 400;
                    mess.Errors = "Không tồn tại người dùng";
                    return (null, mess);
                }

                app.Status = Status;

                await _systemLog.SaveAsync("INFO", "DeActiveUser", $"Hủy kích hoạt tài khoản {app.UserName}", "User",
                                           UserId);
                await _context.SaveChangesAsync();
                return (app, null);
            }
            catch (Exception ex)
            {
                mess.Status = 900;
                mess.Errors = ex.Message;
                return (null, mess);
            }
        }


        public async Task<(AppUser, Mess)> ChangePassword(int userId, string oldPassword, string newPassword,
            int type = 0)
        {
            var mess = new Mess();
            try
            {
                AppUser appUser = await _userManager.Users.Include(x => x.UserRoles)
                    .FirstOrDefaultAsync(p => p.Id == userId && p.Status != "D");

                if (!string.IsNullOrEmpty(oldPassword) && type != 0)
                {
                    var has = await _userManager.CheckPasswordAsync(appUser, oldPassword);
                    if (!has)
                    {
                        mess.Errors = "Mật khẩu không khớp";
                        mess.Status = 404;
                        return (null, mess);
                    }
                }

                string token = await _userManager.GeneratePasswordResetTokenAsync(appUser);
                await _userManager.ResetPasswordAsync(appUser, token, newPassword);
                await _systemLog.SaveAsync("INFO", "ChangePassword", $"Người dùng {appUser.UserName} đã đổi mật khẩu",
                                           "User", userId);
                await _context.SaveChangesAsync();

                return (appUser, null);
            }
            catch (Exception ex)
            {
                mess.Errors = ex.Message;
                mess.Status = 500;
                return (null, mess);
            }
        }

        public async Task<(AppUser, Mess)> UpdateUserAsync(int id, AccountUpdateView model)
        {
            Mess mess = new Mess();
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    AppUser appUser = await _userManager.Users.Include(x => x.UserRoles)
                        .ThenInclude(r => r.Role)
                        .Include(u => u.Claims)
                        .FirstOrDefaultAsync(p => p.Id == id && p.Status != "D");
                    if (appUser == null)
                    {
                        mess.Status = 400;
                        mess.Errors = "Không tồn tại người dùng";
                        return (null, mess);
                    }

                    _modelUpdater.UpdateModel(appUser, model, "NA1", "NA1", "NA2", "NA3", "NA3", "NA");

                    if (!string.IsNullOrEmpty(model.Password))
                    {
                        string password = model.Password;
                        string token    = await _userManager.GeneratePasswordResetTokenAsync(appUser);
                        await _userManager.ResetPasswordAsync(appUser, token, password);
                    }

                    var result = await _userManager.UpdateAsync(appUser);


                    if (result.Succeeded)
                    {
                        await _systemLog.SaveWithTransAsync("INFO", "Update", $"Cập nhật người dùng {appUser.UserName}",
                                                            "User", id);
                        transaction.Commit();
                        return (appUser, null);
                    }
                    else
                    {
                        transaction.Rollback();
                        mess.Status = 400;
                        mess.Errors = "Cập nhập không thành công";
                        return (null, mess);
                    }
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    mess.Status = 900;
                    mess.Errors = ex.Message;
                    return (null, mess);
                }
            }
        }


        public async Task<(AppUser, Mess)> DeleteUser(int UserId)
        {
            Mess mess = new Mess();
            try
            {
                AppUser app = await _userManager.Users
                    .FirstOrDefaultAsync(p => p.Id == UserId && p.Status != "D");
                app.Status = "D";
                var result = await _userManager.UpdateAsync(app);
                if (result.Succeeded)
                {
                    await _systemLog.SaveAsync("INFO", "Delete", $"Xóa người dùng {app.UserName}", "User", UserId);
                    await _context.SaveChangesAsync();
                    return (app, null);
                }
                else
                {
                    mess.Status = 400;
                    mess.Errors = "Cập nhập không thành công";
                    return (null, mess);
                }
            }
            catch (Exception ex)
            {
                mess.Status = 900;
                mess.Errors = ex.Message;
                return (null, mess);
            }
        }

        public async Task ForgotPassword(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                throw new BadHttpRequestException("Email không tồn tại.");

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            await _mailService.SendMailBy(new InfoLoginMail
            {
                ToEmail      = email,
                Name         = user.FullName,
                Username     = user.UserName ?? "",
                Password     = "",
                CustomerName = user.FullName,
                ForgotLink   = $"reset-password?token={Uri.EscapeDataString(token)}&email={email}"
            }, user.CardId == null ? 3 : 2);
        }

        public async Task ResetPassword(ResetPasswordRequest rq)
        {
            var user = await _userManager.FindByEmailAsync(rq.Email);
            if (user == null)
                throw new BadHttpRequestException("Email không tồn tại.");

            var result = await _userManager.ResetPasswordAsync(user, rq.Token, rq.NewPassword);
            if (!result.Succeeded)
                throw new BadHttpRequestException(result.Errors.ToString() ?? "unknown");
            await _systemLog.SaveAsync("INFO", "Update", $"Đặt lại mật khẩu cho ngừi dùng {user.UserName}", "User",
                                       user.Id);
        }


        public async Task<(bool, Mess)> ConfirmEmail(string userId, string token)
        {
            Mess mes = new Mess();
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var user = await _userManager.FindByIdAsync(userId);
                    if (user == null)
                    {
                        mes.Errors = "Người dùng không tồn tại";
                        mes.Status = 400;
                        return (false, mes);
                    }

                    var result = await _userManager.ConfirmEmailAsync(user, token);
                    if (!result.Succeeded)
                    {
                        mes.Errors = "Token không tồn tại hoặc hết hạn";
                        mes.Status = 400;
                        return (false, mes);
                    }

                    user.EmailConfirmed = true;
                    await _userManager.UpdateAsync(user);
                    return (true, null);
                }
                catch (Exception ex)
                {
                    mes.Errors = ex.Message;
                    mes.Status = 400;
                    return (false, mes);
                }
            }
        }

        public async Task<(List<AppSetting>, Mess)> GetAppSetting()
        {
            Mess mess = new Mess();
            try
            {
                var app = await _context.AppSetting.AsQueryable().ToListAsync();
                return (app, null);
            }
            catch (Exception ex)
            {
                mess = new Mess
                {
                    Status = 900,
                    Errors = ex.Message
                };
                return (null, mess);
            }
        }

        public async Task<(List<AppSetting>, Mess)> UpdateAppSetting(List<AppSetting> app)
        {
            Mess mess = new Mess();
            try
            {
                var app1 = await _context.AppSetting.AsQueryable().ToListAsync();
                foreach (var item in app)
                {
                    var existingApp = app1.FirstOrDefault(a => a.UserType == item.UserType);
                    if (existingApp != null)
                    {
                        existingApp.Is2FARequired         = item.Is2FARequired;
                        existingApp.TwoFactorType         = item.TwoFactorType;
                        existingApp.Timeout               = item.Timeout;
                        existingApp.SessionTime           = item.SessionTime;
                        existingApp.IsSessionTimeRequired = item.IsSessionTimeRequired;
                    }
                    else
                    {
                        mess = new Mess
                        {
                            Status = 400,
                            Errors = "Không tồn tại dữ liệu"
                        };
                    }
                }

                await _systemLog.SaveAsync("INFO", "Update", $"Cập nhật lại chính sách phiên đăng nhập", "AppSetting");
                await _context.SaveChangesAsync();
                return (app, null);
            }
            catch (Exception ex)
            {
                mess = new Mess
                {
                    Status = 900,
                    Errors = ex.Message
                };
                return (null, mess);
            }
        }

        public async Task<(string, AppUser, Mess)> TwoFA(string otp, string email)
        {
            Mess mes = new Mess();
            try
            {
                var user = _context.AppUser.AsSplitQuery().AsQueryable().AsQueryable()
                    .Include(p => p.UserRoles)
                    .ThenInclude(x => x.Role)
                    .Include(x => x.Role)
                    .ThenInclude(x => x.RoleClaims)
                    .ThenInclude(p => p.Privilege)
                    .Include(x => x.PersonRole)
                    .ThenInclude(x => x.RoleClaims)
                    .ThenInclude(p => p.Privilege)
                    .FirstOrDefault(x => x.Email == email || x.UserName == email);


                if (user == null)
                {
                    mes.Status = 400;
                    mes.Errors = "Người dùng hoặc mật khẩu không đúng";
                    return (null, null, mes);
                }


                if (user.Status == "D")
                {
                    mes.Status = 401;
                    mes.Errors = "Tài khoản của bạn đã bị xóa";

                    return (null, null, mes);
                }

                if (user.Status == "I")
                {
                    mes.Status = 403;
                    mes.Errors = "Tài khoản này đã bị vô hiệu hóa";

                    return (null, null, mes);
                }

                if (user.OtpCode != otp)
                {
                    mes.Status = 400;
                    mes.Errors = "Mã OTP không đúng";

                    return (null, null, mes);
                }

                if (user.OtpExpiredAt < DateTime.UtcNow)
                {
                    mes.Status = 400;
                    mes.Errors = "Mã OTP đã hết hạn";

                    return (null, null, mes);
                }

                user.OtpExpiredAt = DateTime.UtcNow.AddMinutes(-1);
                string token = await GenerateJSONWebToken(user);

                if (user.CardId != null)
                {
                    var bp = await _context.BP.FirstOrDefaultAsync(x => x.Id == user.CardId);
                    if (user.LastLogin == null) bp.PortalRegistrationStatus = "Y";
                }

                user.LastLogin = DateTime.Now;
                _context.Update(user);
                await _context.SaveChangesAsync();
                return (token, user, null);
            }
            catch (Exception ex)
            {
                mes.Status = 900;
                mes.Errors = ex.Message;
                return (null, null, mes);
            }
        }
    }
}