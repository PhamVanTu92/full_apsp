using BackEndAPI.Models.BusinessPartners;
using BackEndAPI.Models.Promotion;
using BackEndAPI.Service.Promotions;
using BackEndAPI.Tests.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace BackEndAPI.Tests.Unit.Services;

/// <summary>
/// Test luồng GetReportPoint qua single-customer mode (user có CardId).
/// Verify rằng báo cáo được TÁCH theo từng PointSetup (Setups[]).
/// </summary>
public class PointReportTests
{
    private static (PointSetupService sut, Data.AppDbContext db) Create(int? loggedInCardId = null)
    {
        var db = TestDbContextFactory.Create();
        var http = new Mock<IHttpContextAccessor>();

        if (loggedInCardId.HasValue)
        {
            // Seed user với CardId tương ứng
            db.Users.Add(new BackEndAPI.Models.Account.AppUser
            {
                Id = 9999,
                UserName = "tester",
                FullName = "Tester",
                CardId = loggedInCardId.Value
            });
            db.SaveChanges();

            var ctx = new DefaultHttpContext();
            ctx.User = new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity(
                new[] { new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "9999") }, "test"));
            http.Setup(h => h.HttpContext).Returns(ctx);
        }

        return (new PointSetupService(db, http.Object, NullLogger<PointSetupService>.Instance), db);
    }

    private static PointSetup SeedSetup(Data.AppDbContext db, int id, string name)
    {
        var s = new PointSetup
        {
            Id = id,
            Name = name,
            FromDate = DateTime.UtcNow.AddDays(-30),
            ToDate = DateTime.UtcNow.AddDays(30),
            ExtendedToDate = DateTime.UtcNow.AddDays(60),
            IsActive = true,
            IsAllCustomer = true,
            PointSetupCustomer = new List<PointSetupCustomer>(),
            PointSetupLine = new List<PointSetupLine>()
        };
        db.PointSetups.Add(s);
        db.SaveChanges();
        return s;
    }

    private static void SeedHistory(Data.AppDbContext db, int customerId, int setupId,
        int docId, int docType, double pointChange, string invoiceCode = "INV001")
    {
        // ODOC để map InvoiceCode + filter status
        if (!db.ODOC.Any(o => o.Id == docId))
        {
            db.ODOC.Add(new BackEndAPI.Models.Document.ODOC
            {
                Id = docId,
                CardId = customerId,
                InvoiceCode = invoiceCode,
                Status = "DHT",
                ObjType = docType
            });
        }
        db.CustomerPointLine.Add(new CustomerPointLine
        {
            CustomerId = customerId,
            DocId = docId,
            DocType = docType,
            PoitnSetupId = setupId,
            PointChange = pointChange,
            DocDate = DateTime.UtcNow.AddDays(-1),
            ItemCode = "ITM001",
            ItemName = "Test Item",
            Note = ""
        });
        db.SaveChanges();
    }

    private static void SeedCycle(Data.AppDbContext db, int customerId, int setupId,
        double earned, double redeemed, double remaining,
        DateTime? expiryDate = null)
    {
        db.CustomerPointCycles.Add(new CustomerPointCycle
        {
            CustomerId = customerId,
            PoitnSetupId = setupId,
            StartDate = DateTime.UtcNow.AddDays(-30),
            EndDate = DateTime.UtcNow.AddDays(30),
            ExpiryDate = expiryDate ?? DateTime.UtcNow.AddDays(60),
            EarnedPoint = earned,
            RedeemedPoint = redeemed,
            RemainingPoint = remaining,
            Status = 0
        });
        db.SaveChanges();
    }

    [Fact]
    public async Task GetReportPoint_TachTheoTungPointSetup()
    {
        var (sut, db) = Create(loggedInCardId: 1);
        db.BP.Add(new BP { Id = 1, CardCode = "C001", CardName = "Khách 1" });
        db.SaveChanges();

        var setup1 = SeedSetup(db, 10, "Setup A");
        var setup2 = SeedSetup(db, 20, "Setup B");

        SeedCycle(db, customerId: 1, setupId: 10, earned: 100, redeemed: 30, remaining: 70);
        SeedCycle(db, customerId: 1, setupId: 20, earned: 50, redeemed: 0, remaining: 50);

        SeedHistory(db, 1, 10, docId: 100, docType: 22, pointChange: 100, invoiceCode: "DH-001");
        SeedHistory(db, 1, 10, docId: 101, docType: 12, pointChange: -30, invoiceCode: "VPKM-001");
        SeedHistory(db, 1, 20, docId: 102, docType: 22, pointChange: 50, invoiceCode: "DH-002");

        var (reports, total, mess) = await sut.GetReportPoint(
            null, DateTime.UtcNow.AddDays(-7), DateTime.UtcNow.AddDays(1));

        mess.Should().BeNull();
        total.Should().Be(1, "single customer mode trả 1 report");

        var report = reports.Single();
        report.CardCode.Should().Be("C001");
        report.TotalPoint.Should().Be(150, "tổng tất cả cycle: 100+50");
        report.RedeemPoint.Should().Be(30);
        report.RemainingPoint.Should().Be(120);

        // ⭐ Tách theo từng setup
        report.Setups.Should().HaveCount(2);

        var groupA = report.Setups.Single(s => s.PointSetupId == 10);
        groupA.PointSetupName.Should().Be("Setup A");
        groupA.TotalPoint.Should().Be(100);
        groupA.RedeemPoint.Should().Be(30);
        groupA.RemainingPoint.Should().Be(70);
        groupA.IsActive.Should().BeTrue();
        groupA.Lines.Should().HaveCount(2, "1 đơn cộng + 1 đổi quà");

        var groupB = report.Setups.Single(s => s.PointSetupId == 20);
        groupB.TotalPoint.Should().Be(50);
        groupB.Lines.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetReportPoint_BackwardsCompat_ReportPointsLaTongHopMoiSetup()
    {
        var (sut, db) = Create(loggedInCardId: 1);
        db.BP.Add(new BP { Id = 1, CardCode = "C001", CardName = "Khách 1" });
        db.SaveChanges();
        SeedSetup(db, 10, "Setup A");
        SeedSetup(db, 20, "Setup B");
        SeedCycle(db, 1, 10, 100, 0, 100);
        SeedCycle(db, 1, 20, 50, 0, 50);
        SeedHistory(db, 1, 10, 100, 22, 100, "DH-A");
        SeedHistory(db, 1, 20, 200, 22, 50, "DH-B");

        var (reports, _, _) = await sut.GetReportPoint(
            null, DateTime.UtcNow.AddDays(-7), DateTime.UtcNow.AddDays(1));

        var report = reports.Single();
        report.ReportPoints.Should().HaveCount(2,
            "ReportPoints (legacy field) phải = SelectMany(Setups.Lines) cho client cũ");
    }

    [Fact]
    public async Task GetReportPoint_KhongCoData_TraReportRong()
    {
        var (sut, db) = Create(loggedInCardId: 1);
        db.BP.Add(new BP { Id = 1, CardCode = "C001", CardName = "Khách 1" });
        db.SaveChanges();

        var (reports, total, mess) = await sut.GetReportPoint(
            null, DateTime.UtcNow.AddDays(-7), DateTime.UtcNow.AddDays(1));

        mess.Should().BeNull();
        total.Should().Be(1);
        var report = reports.Single();
        report.Setups.Should().BeEmpty();
        report.TotalPoint.Should().Be(0);
    }

    [Fact]
    public async Task GetReportPoint_SetupHetHan_IsActiveLaFalse()
    {
        var (sut, db) = Create(loggedInCardId: 1);
        db.BP.Add(new BP { Id = 1, CardCode = "C001", CardName = "Khách 1" });
        db.SaveChanges();

        var expiredSetup = new PointSetup
        {
            Id = 30,
            Name = "Setup Expired",
            FromDate = DateTime.UtcNow.AddDays(-100),
            ToDate = DateTime.UtcNow.AddDays(-10),
            ExtendedToDate = DateTime.UtcNow.AddDays(-5),  // hết hạn 5 ngày trước
            IsActive = true,
            IsAllCustomer = true,
            PointSetupCustomer = new List<PointSetupCustomer>(),
            PointSetupLine = new List<PointSetupLine>()
        };
        db.PointSetups.Add(expiredSetup);
        db.SaveChanges();

        SeedCycle(db, 1, 30, 100, 0, 100, expiryDate: DateTime.UtcNow.AddDays(-5));
        SeedHistory(db, 1, 30, 300, 22, 100, "DH-Expired");

        var (reports, _, _) = await sut.GetReportPoint(
            null, DateTime.UtcNow.AddDays(-100), DateTime.UtcNow.AddDays(1));

        var report = reports.Single();
        report.Setups.Single().IsActive.Should().BeFalse("ExtendedToDate đã qua → không còn hiệu lực");
    }
}
