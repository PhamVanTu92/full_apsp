using System.ComponentModel;
using System.Net.WebSockets;
using System.Reflection;
using BackEndAPI.Data;
using BackEndAPI.Models.Account;
using BackEndAPI.Models.Banks;
using BackEndAPI.Models.Other;
using BackEndAPI.Service;
using BackEndAPI.Service.Account;
using BackEndAPI.Service.Areas;
using BackEndAPI.Service.Document;
using BackEndAPI.Service.BPGroups;
using BackEndAPI.Service.Branchs;
using BackEndAPI.Service.BusinessPartners;
using BackEndAPI.Service.ItemGroups;
using BackEndAPI.Service.ItemMasterData;
using BackEndAPI.Service.Payments;
using BackEndAPI.Service.Privilege;
using BackEndAPI.Service.Promotions;
using BackEndAPI.Service.Unit;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.FileProviders;
using System.Text.Json.Serialization;
using Function.EncryptDecrypt;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using B1SLayer;
using Microsoft.OpenApi.Models;
using BackEndAPI.Models.TaxGroup;
using BackEndAPI.Service.GoldPriceLists;
using BackEndAPI.Models.ItemGroups;
using BackEndAPI.Service.Document;
using BackEndAPI.Service.Sync.Jobs;
using BackEndAPI.Service.Jobs;
using BackEndAPI.Service.Mail;
using Microsoft.AspNetCore.Mvc;
using BackEndAPI.Service.Approval;
using BackEndAPI.Service.ItemAreas;
using BackEndAPI.Models.BusinessPartners;
using BackEndAPI.Mapping;
using BackEndAPI.Middleware;
using BackEndAPI.Models;
using BackEndAPI.Models.ProductionCommitmentModel;
using BackEndAPI.Service.Approval_V2.ApprovalLevel;
using BackEndAPI.Service.Approval_V2.ApprovalSample;
using BackEndAPI.Service.Approval_V2.ApprovalWorkFlow.Engine;
using BackEndAPI.Service.Approval_V2.ApprovalWorkFlow.Service;
using BackEndAPI.Service.Committeds;
using BackEndAPI.Service.EventAggregator;
using BackEndAPI.Service.Notification;
using BackEndAPI.Service.Fees;
using BackEndAPI.Service.SaleForecast;
using BackEndAPI.Service.ProductionCommitment;
using Org.BouncyCastle.Asn1;
using BackEndAPI.Service.NotificationHub;
using BackEndAPI.Service.Brands;
using BackEndAPI.Service.Cart;
using BackEndAPI.Service.DebtReconciliation;
using BackEndAPI.Service.GenericeService;
using BackEndAPI.Service.Industrys;
using BackEndAPI.Service.LocationService;
using BackEndAPI.Service.OrganizationUnit;
using BackEndAPI.Service.Privile;
using BackEndAPI.Service.Report;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Quartz;
using BackEndAPI.Service.Articles;
using BackEndAPI.Service.PriceLists;
using BackEndAPI.Service.Test;
using BackEndAPI.Service.Zalo;
using BackEndAPI.Service.ConfirmationSystems;
using Serilog;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;

var builder = WebApplication.CreateBuilder(args);

// ── Serilog: structured logging với rolling file + console ──────────────────
builder.Host.UseSerilog((context, services, config) =>
{
    config
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Application", "BackEndAPI")
        .WriteTo.Console()
        .WriteTo.File(
            path: "Logs/log-.txt",
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 30,
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}");

    // Production: filter out EF sensitive data warnings ra log để tránh ghi
    // query parameters chứa PII (số điện thoại, email, password hash, etc.)
    if (!context.HostingEnvironment.IsDevelopment())
    {
        config.Filter.ByExcluding(e =>
            e.Properties.TryGetValue("EventId", out var eid) &&
            eid.ToString().Contains("SensitiveDataLoggingEnabledWarning"));
    }
});

PayOne._config = builder.Configuration.GetSection("PayoneConfig").Get<PayOneConfig>() ??
    throw new InvalidOperationException();
string merchantId         = PayOne._config.PayNow.Id;
string merchantAccessCode = PayOne._config.PayNow.AccessCode;
string merchantHashCode   = PayOne._config.PayNow.HashCode;

var    smtpSettings = builder.Configuration.GetSection("SmtpSettings").Get<SmtpSettings>();

// JWT validation chặt: phải có Key + Issuer + Audience.
// Production: Issuer/Audience phải khớp domain thật, không được dùng "localhost".
var jwtKeyEncrypted = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Jwt:Key chưa được cấu hình. Set qua User Secrets (dev) hoặc env var (prod).");
var jwtIssuer  = builder.Configuration["Jwt:Issuer"]
    ?? throw new InvalidOperationException("Jwt:Issuer chưa được cấu hình.");
var jwtAudience = builder.Configuration["Jwt:Audience"]
    ?? throw new InvalidOperationException("Jwt:Audience chưa được cấu hình.");

if (!builder.Environment.IsDevelopment())
{
    if (jwtIssuer.Contains("localhost", StringComparison.OrdinalIgnoreCase) ||
        jwtAudience.Contains("localhost", StringComparison.OrdinalIgnoreCase))
    {
        throw new InvalidOperationException(
            $"JWT Issuer/Audience không được chứa 'localhost' ở môi trường {builder.Environment.EnvironmentName}. " +
            "Cập nhật appsettings.{Env}.json hoặc env var Jwt__Issuer / Jwt__Audience.");
    }
}

string key = EncryptDecrypt.DecryptString(jwtKeyEncrypted);
if (System.Text.Encoding.UTF8.GetByteCount(key) < 32)
{
    throw new InvalidOperationException(
        $"JWT key sau decrypt chỉ {System.Text.Encoding.UTF8.GetByteCount(key)} byte — cần >= 32 byte (256-bit) để dùng HMAC-SHA256.");
}
// key = EncryptDecrypt.EncryptString("123456789987654321abcdef@!@#@@@@*****");
string c                = EncryptDecrypt.EncryptString("manager");
string c1               = EncryptDecrypt.EncryptString("1111");
string connectionString = EncryptDecrypt.DecryptString(builder.Configuration.GetConnectionString("DefaultConnection"))
    ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection chưa được cấu hình.");
// ⚠ KHÔNG log connectionString — chứa SQL password sau decrypt.
builder.Services.AddSingleton(smtpSettings);
builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));
builder.Services.Configure<Endpoints>(builder.Configuration.GetSection("Endpoints"));
builder.Services.Configure<ZaloTokenConfig>(
    builder.Configuration.GetSection("ZaloTokenConfig"));
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(connectionString);
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});
builder.Services.AddScoped(typeof(IGenericService<>), typeof(GenericService<>));
builder.Services.AddScoped<IOrganizationUnitService, OrganizationUnitService>();

#region ApprovalLevel2

builder.Services.AddScoped<IApprovalLevelService, ApprovalLevelService>();
builder.Services.AddScoped<IApprovalSampleService, ApprovalSampleService>();
builder.Services.AddScoped<IApprovalWorkFlowService, ApprovalWorkFlowService>();
builder.Services.AddScoped<IApprovalWorkFlowFactory, ApprovalWorkFlowFactory>();
builder.Services.AddScoped<IApprovalWorkFlowEngine, PurchaseOrderWorkFlow>();
builder.Services.AddScoped<IApprovalWorkFlowEngine, RequestPickUpItemsWorkFlow>();
builder.Services.AddScoped<IApprovalWorkFlowEngine, PurchaseReturnWorkFlow>();
builder.Services.AddScoped<VnPayService>();
builder.Services.AddScoped<IVnPayPaymentService, VnPayPaymentService>();

#endregion

builder.Services.AddScoped<IExchangePointService, ExchangePointService>();
builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));
builder.Services.AddIdentity<AppUser, AppRole>(options => { options.User.RequireUniqueEmail = true; })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();
builder.Services.AddSingleton<IEventAggregator, EventAggregator>();
builder.Services.AddScoped<LoggingSystemService>();
builder.Services.AddScoped<UserContextService>();

builder.Services.AddScoped<Approval>();
builder.Services.AddScoped<Notification>();
builder.Services.AddScoped<ReportService>();

builder.Services.AddHostedService<ApprovalInitializer>();
builder.Services.AddHostedService<NotificationInitializer>();

var configuration = builder.Configuration;


// SAP B1 Service Layer connection — credentials trong config bị mã hoá, decrypt khi đăng ký.
builder.Services.AddSingleton(_ => new SLConnection(
    new Uri(configuration["SAPServiceLayer:Host"]!),
    EncryptDecrypt.DecryptString(configuration["SAPServiceLayer:CompanyDB"]),
    EncryptDecrypt.DecryptString(configuration["SAPServiceLayer:Username"]),
    EncryptDecrypt.DecryptString(configuration["SAPServiceLayer:Password"])
));


builder.Services.AddScoped<ILocaltion, Localtion>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IItemService, ItemService>();
builder.Services.AddScoped<IBusinessPartnerService, BusinessPartnerService>();
builder.Services.AddScoped<DashboardService>();
builder.Services.AddScoped<IItemGroupService, ItemGroupService>();
builder.Services.AddScoped<IBPGroupService, BPGroupService>();
builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddScoped<IOUOMService, OUOMService>();
builder.Services.AddScoped<IOUGPService, OUGPService>();
builder.Services.AddScoped<IPromotionService, PromotionService>();
builder.Services.AddScoped<PromotionCalculatorService>();
builder.Services.AddScoped<BackEndAPI.Service.Sync.IBPSyncService, BackEndAPI.Service.Sync.BPSyncService>();
builder.Services.AddScoped<BackEndAPI.Service.Sync.IInternalProxySyncService, BackEndAPI.Service.Sync.InternalProxySyncService>();
builder.Services.AddScoped<BackEndAPI.Service.Sync.IDocumentPushSyncService, BackEndAPI.Service.Sync.DocumentPushSyncService>();
builder.Services.AddScoped<BackEndAPI.Service.Sync.Diagnostics.ISapPushDiagnosticService, BackEndAPI.Service.Sync.Diagnostics.SapPushDiagnosticService>();
builder.Services.AddScoped<BackEndAPI.Service.BusinessPartners.Address.IBPAddressService, BackEndAPI.Service.BusinessPartners.Address.BPAddressService>();
builder.Services.AddScoped<BackEndAPI.Service.Sap.ISapClient, BackEndAPI.Service.Sap.SapClient>();

// Reference data cache (Brand, Industry, ItemGroup, Region, ...) — server cache + ETag + SignalR push
builder.Services.AddMemoryCache(opt => opt.SizeLimit = 1024);
builder.Services.AddSingleton<BackEndAPI.Service.Cache.IReferenceDataCache, BackEndAPI.Service.Cache.ReferenceDataCache>();
builder.Services.AddScoped<BackEndAPI.Service.Auth.IRefreshTokenService, BackEndAPI.Service.Auth.RefreshTokenService>();
builder.Services.AddScoped<IVoucherService, VoucherService>();
builder.Services.AddScoped<IVoucherLineService, VoucherLineService>();
builder.Services.AddScoped<ICouponService, CouponService>();
builder.Services.AddScoped<ICouponLineService, CouponLineService>();
builder.Services.AddScoped<IService<PaymentMethod>, Service<PaymentMethod>>();
builder.Services.AddScoped<IService<PersonInfor>, Service<PersonInfor>>();
builder.Services.AddScoped<IBrandService, BrandService>();
builder.Services.AddScoped<IService<AreaLocation>, Service<AreaLocation>>();
builder.Services.AddScoped<IPackingService, PackingService>();
builder.Services.AddScoped<IItemTypeService, ItemTypeService>();
builder.Services.AddScoped<IService<BPSize>, Service<BPSize>>();
builder.Services.AddScoped<IIndustryService, IndustryService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IService<TaxGroups>, Service<TaxGroups>>();
builder.Services.AddScoped<IService<TimeZones>, Service<TimeZones>>();
builder.Services.AddScoped<IAreaService, AreaService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IBranchAddressService, BranchAddressService>();
builder.Services.AddScoped<IModelUpdater, ModelUpdater>();
builder.Services.AddScoped<IPrivilegesServicecs, PrivilegesService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IFeeService, FeeService>();
builder.Services.AddTransient<IMailService, MailService>();
builder.Services.AddScoped<IApprovalStageService, ApprovalStageService>();
builder.Services.AddScoped<IApprovalTemplateService, ApprovalTemplateService>();
builder.Services.AddScoped<IFeeService, FeeService>();
builder.Services.AddScoped<IPointSetupService, PointSetupService>();
builder.Services.AddScoped<IFeebyCustomerService, FeebyCustomerService>();
builder.Services.AddScoped<IFeeLevelService, FeeLevelService>();
builder.Services.AddScoped<IUserGroupService, UserGroupService>();
builder.Services.AddScoped<IProductionCommitmentService, ProductionCommitmentService>();
builder.Services.AddScoped<ICommittedService, CommittedService>();
builder.Services.AddScoped<IArticleService, ArticleService>();
builder.Services.AddScoped<ISaleForecast, SaleForecast>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IPaymentRuleService, PaymentRuleService>();
builder.Services.AddScoped<IPriceListService, PriceListService>();
builder.Services.AddScoped<IDebtReconcilicationService, DebtReconcilicationService>();
builder.Services.AddScoped<IConfirmationDocumentService, ConfirmationDocumentService>();
builder.Services.AddSingleton<WebSocketService>();
builder.Services.AddTransient<WebSocketHandler>();
builder.Services.AddHttpClient<ZaloService>();
builder.Services.AddHostedService<ZaloService>();
builder.Services.AddScoped<ZaloService>();

builder.Services.AddScoped<IBPAreaService, BPAreaService>();
builder.Services
    .AddScoped<BackEndAPI.Service.StorageFee.IStorageFeeService, BackEndAPI.Service.StorageFee.StorageFeeService>();
builder.Services.AddAutoMapper(typeof(Mapper));
builder.Services.AddControllers(options =>
    {
        // Auto-wrap mọi response thành ApiResponse<T> cho đồng nhất format.
        // Code legacy không cần sửa — filter tự bọc Ok(dto) → ApiResponse<T>.Ok(dto).
        options.Filters.Add<BackEndAPI.Middleware.ApiResponseFilter>();
    })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.Converters.Add(new ConvertDateTimeToUTC());
        //options.JsonSerializerOptions.PropertyNamingPolicy = null;
    });
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(option =>
{
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    option.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
    option.SwaggerDoc("v1",
                      new OpenApiInfo { Title = "Demo API", Version = "v1" }
    );
    option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In           = ParameterLocation.Header,
        Description  = "Please enter a valid token",
        Name         = "Authorization",
        Type         = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme       = "Bearer"
    });
    option.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id   = "Bearer"
                }
            },
            new string[] { }
        }
    });
});

builder.Services.Configure<IdentityOptions>(options =>
{
    // Password settings.
    options.Password.RequireDigit           = true;
    options.Password.RequireLowercase       = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase       = true;
    options.Password.RequiredLength         = 6;
    options.Password.RequiredUniqueChars    = 1;

    // Lockout settings.
    options.Lockout.DefaultLockoutTimeSpan  = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers      = true;

    // User settings.
    options.User.AllowedUserNameCharacters =
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail            = true;
    options.SignIn.RequireConfirmedEmail       = true;
    options.SignIn.RequireConfirmedPhoneNumber = false;
});


builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme             = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(o =>
{
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidIssuer   = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey
            (Encoding.UTF8.GetBytes(key)),
        ValidateIssuer           = true,
        ValidateAudience         = true,
        ValidateLifetime         = true,
        ValidateIssuerSigningKey = true,
        ClockSkew                = TimeSpan.FromMinutes(2)
    };
});
builder.Services.AddAuthorization(options => { });
// CORS — Development cho phép all origin, Production chỉ whitelist từ Cors:AllowedOrigins
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            policy.SetIsOriginAllowed(_ => true)
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        }
        else
        {
            var allowed = builder.Configuration
                .GetSection("Cors:AllowedOrigins")
                .Get<string[]>() ?? Array.Empty<string>();

            if (allowed.Length == 0)
                throw new InvalidOperationException(
                    "Cors:AllowedOrigins phải được cấu hình ở môi trường non-Development.");

            policy.WithOrigins(allowed)
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        }
    });
});
builder.Services.AddScoped<IAuthorizationHandler, PrivilegeAuthorizationHandler>();
builder.Services.AddScoped<IPrivileService, PrivileService>();
builder.Services.AddSignalR();
builder.Services.AddControllers()
.AddJsonOptions(o =>
{
    o.JsonSerializerOptions.ReferenceHandler =
        System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});
builder.Services.AddQuartz(q =>
{
    /*var jobDebitKey = new JobKey("SyncDebJob");
    q.AddJob<SyncDebJob>(opts => opts.WithIdentity(jobDebitKey));

    q.AddTrigger(opts => opts
        .ForJob(jobDebitKey)
        .WithIdentity("SyncDebJob-trigger")
        .WithCronSchedule("0 0/2 * * * ?")
    );

    var jobVPKMKey = new JobKey("SyncVPKMJob");
    q.AddJob<SyncVPKMJob>(opts => opts.WithIdentity(jobVPKMKey));

    q.AddTrigger(opts => opts
        .ForJob(jobVPKMKey)
        .WithIdentity("SyncVPKMJob-trigger")
        .WithCronSchedule("0/58 * * * * ?")
    );


    var jobKey = new JobKey("SyncJob");
    q.AddJob<SyncJob>(opts => opts.WithIdentity(jobKey));

    q.AddTrigger(opts => opts
        .ForJob(jobKey)
        .WithIdentity("SyncJob-trigger")
        .WithCronSchedule("0 0/2 * * * ?")
    );

    var jobBPKey = new JobKey("SyncBPJob");
    q.AddJob<SyncBPJob>(opts => opts.WithIdentity(jobBPKey));

    q.AddTrigger(opts => opts
        .ForJob(jobBPKey)
        .WithIdentity("SyncBPJob-trigger")
        .WithCronSchedule("0 0/10 * * * ?")
    );
    var jobDOCKey = new JobKey("SyncDOCJob");
    q.AddJob<SyncDOCJob>(opts => opts.WithIdentity(jobDOCKey));

    q.AddTrigger(opts => opts
        .ForJob(jobDOCKey)
        .WithIdentity("SyncDOCJob-trigger")
        .WithCronSchedule("0 0/2 * * * ?")
    );
    var jobBPCRD4Key = new JobKey("SyncBPCRD4Job");
    q.AddJob<SyncBPCRD4Job>(opts => opts.WithIdentity(jobBPCRD4Key));
    // Delta sync — chỉ 1 HTTP call SAP / chu kỳ → an toàn ở 30s
    q.AddTrigger(opts => opts
        .ForJob(jobBPCRD4Key)
        .WithIdentity("SyncBPCRD4Job-trigger")
        .WithCronSchedule("0/30 * * * * ?")
    );*/
});

// Quartz background service — graceful shutdown:
//   • WaitForJobsToComplete=true: chờ job đang chạy hoàn tất trước khi tắt
//   • CancellationToken được pass xuống job qua SyncJobBase → job tôn trọng nó
//   • Bị bound bởi HostOptions.ShutdownTimeout (config bên dưới)
builder.Services.AddQuartzHostedService(q =>
{
    q.WaitForJobsToComplete = true;
    q.AwaitApplicationStarted = true;  // chờ app start xong mới fire job đầu
});

// Graceful shutdown timeout cho toàn host (mặc định .NET là 30s).
// Tăng lên 45s để job đang chạy có thời gian respond cancellation token,
// commit transaction DB và flush log Serilog.
//
// ⚠ K8s deployment phải set terminationGracePeriodSeconds >= 60 để app có
// đủ slot 45s shutdown + buffer trước khi K8s gửi SIGKILL.
builder.Services.Configure<HostOptions>(options =>
{
    options.ShutdownTimeout = TimeSpan.FromSeconds(45);
});

// ── Health checks ───────────────────────────────────────────────────────────
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>("database", tags: new[] { "ready" })
    .AddCheck<BackEndAPI.Service.Sync.Health.SapHealthCheck>(
        "sap", tags: new[] { "ready", "sap" });

// ── OpenTelemetry: distributed tracing + metrics ────────────────────────────
// Trace: từng HTTP request được gán traceId, propagate qua HttpClient/EF →
//        có thể trace 1 request đi qua API → DB → SAP với cùng traceId.
// Metrics: ASP.NET / HTTP / runtime metrics expose qua /metrics (Prometheus).
builder.Services.AddOpenTelemetry()
    .ConfigureResource(r => r.AddService(serviceName: "APSP-Backend",
                                         serviceVersion: typeof(Program).Assembly.GetName().Version?.ToString() ?? "0.0.0"))
    .WithTracing(t => t
        .AddAspNetCoreInstrumentation(o =>
        {
            // Loại trừ health check khỏi trace để giảm noise
            o.Filter = ctx => !ctx.Request.Path.StartsWithSegments("/api/health")
                           && !ctx.Request.Path.StartsWithSegments("/metrics");
        })
        .AddHttpClientInstrumentation()
        .AddEntityFrameworkCoreInstrumentation(o => o.SetDbStatementForText = builder.Environment.IsDevelopment())
        .AddSource("APSP.*")  // custom Activity sources nếu cần thêm
    )
    .WithMetrics(m => m
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddRuntimeInstrumentation()
        .AddPrometheusExporter()
    );

// ── Rate limiting ───────────────────────────────────────────────────────────
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Login: 5 requests / phút / IP — chống brute force
    options.AddPolicy("login", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));

    // Read-heavy endpoints: 60 req / phút / IP
    // (cao hơn login vì user thường list/scroll nhiều)
    options.AddPolicy("read", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 60,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 10
            }));

    // Write/mutating endpoints: 20 req / phút / user (theo userId từ JWT)
    options.AddPolicy("write", httpContext =>
    {
        var userId = httpContext.User?.FindFirst("userId")?.Value
                  ?? httpContext.Connection.RemoteIpAddress?.ToString()
                  ?? "anonymous";
        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: userId,
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 20,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            });
    });

    // Sync admin endpoints (force-trigger SAP sync) — chỉ 2 req / phút / IP
    options.AddPolicy("admin-sync", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 2,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));

    // Global fallback: 100 req / phút / IP — bảo vệ chung
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));

    // Skip rate limit cho health check + metrics (monitoring scrape liên tục)
    options.OnRejected = (context, _) =>
    {
        context.HttpContext.Response.Headers["Retry-After"] = "60";
        return ValueTask.CompletedTask;
    };
});

var app = builder.Build();

// Log mọi request HTTP (status code, duration, route)
app.UseSerilogRequestLogging();

// Cảnh báo nếu TLS bypass đang bật — KHÔNG được bật trong production thật
if (BackEndAPI.Service.Sync.Security.TlsBypass.IsEnabled)
{
    app.Logger.LogWarning(
        "⚠️  TLS_BYPASS_ENABLED: SAP cert validation đang bị bypass. " +
        "Acceptable cho dev/staging với self-signed cert. " +
        "TUYỆT ĐỐI không deploy production với cờ này. " +
        "Tắt bằng cách: ASPNETCORE_ENVIRONMENT=Production và unset ALLOW_SELF_SIGNED_TLS.");
}
app.UseRateLimiter();
app.UseCors("AllowAll");
app.UseWebSockets();
app.Map("/api/notifications", async context =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        var handler = context.RequestServices.GetRequiredService<WebSocketHandler>();
        await handler.Handle(context);
    }
    else
    {
        context.Response.StatusCode = 400;
    }
});

app.MapHub<NotificationHubs>("/api/notificationHubs");
app.MapHub<BackEndAPI.Hubs.ReferenceDataHub>("/api/hubs/reference-data");

// ETag được handle trong CacheableReferenceData attribute (action filter) —
// stable theo cache version, không bị ảnh hưởng bởi traceId trong response.
app.UseSwagger();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseSwaggerUI();
app.UseStaticFiles(new StaticFileOptions
{
    // OnPrepareResponse = ctx =>
    // {
    //     // var isDownload = ctx.Context.Request.Query.ContainsKey("download");
    //     // ctx.Context.Response.Headers.Append("Content-Disposition",  "inline");
    // },
    //FileProvider = new PhysicalFileProvider(
    //    Path.Combine(builder.Environment.WebRootPath, "uploads")),
    //RequestPath = "/uploads"
    //OnPrepareResponse = ctx => { ctx.Context.Response.Headers.Append("Content-Disposition", "attachment"); },
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "uploads")),
    RequestPath = "/uploads"
});
app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();


app.MapControllers();

// ── Health endpoints ────────────────────────────────────────────────────────
// Liveness: chỉ check process còn alive — không chạy bất kỳ dependency check nào
app.MapHealthChecks("/api/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => false,
    ResponseWriter = HealthChecks.UI.Client.UIResponseWriter.WriteHealthCheckUIResponse
}).DisableRateLimiting();

// Readiness: process + dependencies (DB, SAP) sẵn sàng phục vụ traffic.
// JSON response chi tiết per-check để Prometheus / Grafana parse được.
app.MapHealthChecks("/api/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = HealthChecks.UI.Client.UIResponseWriter.WriteHealthCheckUIResponse
}).DisableRateLimiting();

// Prometheus metrics endpoint — scrape /metrics để monitor:
//   • aspnetcore_request_duration_seconds (HTTP latency)
//   • http_client_request_duration_seconds (outbound SAP / proxy calls)
//   • dotnet_gc_collections_total, dotnet_gc_heap_size_bytes (GC pressure)
//   • db_client_operation_duration_seconds (EF Core query duration)
app.UseOpenTelemetryPrometheusScrapingEndpoint();
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var userManager = services.GetRequiredService<UserManager<AppUser>>();
    //var roleManager = services.GetRequiredService<RoleManager<AppRole>>();
    var context = services.GetRequiredService<AppDbContext>();
    await SeedData.AddPaymentRule(context);
    await SeedData.AppSetting(context);
}

app.Run();