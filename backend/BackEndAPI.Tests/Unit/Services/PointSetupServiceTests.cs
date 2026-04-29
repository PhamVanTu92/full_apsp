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
/// Test các business rules tích/trừ/hoàn điểm:
///   • Order ObjType=22:  DHT cộng cycle, HUY/DONG/HUY2 hoàn
///   • VPKM ObjType=12:   HUY/DONG/HUY2 hoàn lại điểm về cycle gốc
///   • RedeemPoints chỉ qua cycle còn hiệu lực (ExpiryDate >= today)
///   • PointSetup.UpdateAsync chặn sửa FromDate/ToDate khi đã có tích điểm
/// </summary>
public class PointSetupServiceTests
{
    private static PointSetupService Create(out Data.AppDbContext db)
    {
        db = TestDbContextFactory.Create();
        var http = new Mock<IHttpContextAccessor>();
        return new PointSetupService(db, http.Object, NullLogger<PointSetupService>.Instance);
    }

    private static PointSetup SeedSetup(Data.AppDbContext db, int id = 1, bool active = true)
    {
        var setup = new PointSetup
        {
            Id = id,
            Name = $"Setup {id}",
            FromDate = DateTime.UtcNow.AddDays(-30),
            ToDate = DateTime.UtcNow.AddDays(30),
            ExtendedToDate = DateTime.UtcNow.AddDays(60),
            IsActive = active,
            IsAllCustomer = true,
            PointSetupCustomer = new List<PointSetupCustomer>(),
            PointSetupLine = new List<PointSetupLine>()
        };
        db.PointSetups.Add(setup);
        db.SaveChanges();
        return setup;
    }

    private static CustomerPoint SeedPendingOrder(Data.AppDbContext db,
        int docId, int customerId, int setupId, double points)
    {
        var cp = new CustomerPoint
        {
            CustomerId = customerId,
            DocId = docId,
            DocType = PointObjTypes.Order,
            AddPoint = true,
            TotalPointChange = points,
            Details = new List<CustomerPointLine>
            {
                new CustomerPointLine
                {
                    CustomerId = customerId,
                    DocId = docId,
                    DocType = PointObjTypes.Order,
                    PoitnSetupId = setupId,
                    PointChange = points,
                    DocDate = DateTime.UtcNow,
                    ItemCode = "ITM001",
                    ItemName = "Item Test",
                    Note = "Pending"
                }
            }
        };
        db.CustomerPoint.Add(cp);
        db.SaveChanges();
        return cp;
    }

    // ── Order ObjType=22 ────────────────────────────────────────────────

    [Fact]
    public async Task OrderDHT_CongCycle()
    {
        var sut = Create(out var db);
        var setup = SeedSetup(db);
        SeedPendingOrder(db, docId: 100, customerId: 1, setupId: setup.Id, points: 50);

        await sut.OnDocumentStatusChangedAsync(100, 1, PointObjTypes.Order, "DHT");

        var cycle = db.CustomerPointCycles.Single();
        cycle.EarnedPoint.Should().Be(50);
        cycle.RemainingPoint.Should().Be(50);
    }

    [Fact]
    public async Task OrderDHT_GoiHaiLan_KhongDoubleAdd()
    {
        var sut = Create(out var db);
        var setup = SeedSetup(db);
        SeedPendingOrder(db, 100, 1, setup.Id, 50);

        await sut.OnDocumentStatusChangedAsync(100, 1, PointObjTypes.Order, "DHT");
        await sut.OnDocumentStatusChangedAsync(100, 1, PointObjTypes.Order, "DHT");

        db.CustomerPointCycles.Single().EarnedPoint.Should().Be(50, "idempotent guard phải skip lần 2");
    }

    [Fact]
    public async Task OrderHUY_DaConfirmed_HoanDiem()
    {
        var sut = Create(out var db);
        var setup = SeedSetup(db);
        SeedPendingOrder(db, 100, 1, setup.Id, 50);
        await sut.OnDocumentStatusChangedAsync(100, 1, PointObjTypes.Order, "DHT"); // confirm trước

        await sut.OnDocumentStatusChangedAsync(100, 1, PointObjTypes.Order, "HUY");

        db.CustomerPointCycles.Single().RemainingPoint.Should().Be(0);
        db.CustomerPointCycles.Single().EarnedPoint.Should().Be(0);
        db.CustomerPoint.Should().Contain(c => c.AddPoint == false, "phải có reverse history");
    }

    [Fact]
    public async Task OrderHUY_ChuaConfirmed_XoaPendingKhongTouchCycle()
    {
        var sut = Create(out var db);
        var setup = SeedSetup(db);
        SeedPendingOrder(db, 100, 1, setup.Id, 50);
        // KHÔNG gọi DHT trước

        await sut.OnDocumentStatusChangedAsync(100, 1, PointObjTypes.Order, "HUY");

        db.CustomerPointCycles.Should().BeEmpty("không tạo cycle khi pending bị huỷ");
        db.CustomerPoint.Should().BeEmpty("pending phải xoá");
    }

    [Theory]
    [InlineData("HUY")]
    [InlineData("DONG")]
    [InlineData("HUY2")]
    public async Task OrderReverseStatuses_AllTriggerRefund(string reverseStatus)
    {
        var sut = Create(out var db);
        var setup = SeedSetup(db);
        SeedPendingOrder(db, 100, 1, setup.Id, 50);
        await sut.OnDocumentStatusChangedAsync(100, 1, PointObjTypes.Order, "DHT");

        await sut.OnDocumentStatusChangedAsync(100, 1, PointObjTypes.Order, reverseStatus);

        db.CustomerPointCycles.Single().RemainingPoint.Should().Be(0);
    }

    // ── PointSetup Update guards ────────────────────────────────────────

    [Fact]
    public async Task UpdatePointSetup_KhongCoTichDiem_ChoSuaToanBo()
    {
        var sut = Create(out var db);
        var setup = SeedSetup(db);

        var dto = new PointSetupUpdateDto
        {
            Id = setup.Id,
            Name = "Updated",
            FromDate = setup.FromDate.AddDays(5),
            ToDate = setup.ToDate.AddDays(5),
            ExtendedToDate = setup.ExtendedToDate,
            IsActive = true,
            IsAllCustomer = true,
            Customers = new List<PointSetupCustomerUpdateDto>(),
            Lines = new List<PointSetupLineUpdateDto>()
        };

        var (result, mess) = await sut.UpdateAsync(dto);

        mess.Should().BeNull();
        result.Should().NotBeNull();
        result!.Name.Should().Be("Updated");
    }

    [Fact]
    public async Task UpdatePointSetup_DaCoTichDiem_KhongChoSuaFromDate()
    {
        var sut = Create(out var db);
        var setup = SeedSetup(db);
        // Tạo 1 record CustomerPointLine để mark "đã có tích điểm"
        db.CustomerPointLine.Add(new CustomerPointLine
        {
            CustomerId = 1,
            DocId = 100,
            DocType = PointObjTypes.Order,
            PoitnSetupId = setup.Id,
            PointChange = 10,
            DocDate = DateTime.UtcNow,
            ItemCode = "ITM",
            ItemName = "Item",
            Note = ""
        });
        db.SaveChanges();

        var dto = new PointSetupUpdateDto
        {
            Id = setup.Id,
            Name = setup.Name,
            FromDate = setup.FromDate.AddDays(5), // ← cố sửa FromDate
            ToDate = setup.ToDate,
            ExtendedToDate = setup.ExtendedToDate,
            IsActive = true,
            IsAllCustomer = setup.IsAllCustomer,
            Customers = new List<PointSetupCustomerUpdateDto>(),
            Lines = new List<PointSetupLineUpdateDto>()
        };

        var (result, mess) = await sut.UpdateAsync(dto);

        result.Should().BeNull();
        mess.Should().NotBeNull();
        mess!.Status.Should().Be(400);
        mess.Errors.Should().Contain("FromDate");
    }

    [Fact]
    public async Task UpdatePointSetup_ChiCoCycle_KhongCoLine_VanBlock()
    {
        // Edge case: CustomerPointLine có thể đã bị xoá (vd: clean up old data) nhưng
        // CustomerPointCycle vẫn còn balance. Vẫn phải coi là "đã phát sinh tích điểm".
        var sut = Create(out var db);
        var setup = SeedSetup(db);
        db.CustomerPointCycles.Add(new CustomerPointCycle
        {
            CustomerId = 1,
            PoitnSetupId = setup.Id,
            StartDate = setup.FromDate,
            EndDate = setup.ToDate,
            ExpiryDate = setup.ExtendedToDate ?? setup.ToDate,
            EarnedPoint = 100,
            RemainingPoint = 100,
            Status = 0
        });
        db.SaveChanges();

        var dto = new PointSetupUpdateDto
        {
            Id = setup.Id,
            Name = setup.Name,
            FromDate = setup.FromDate.AddDays(5), // ← cố sửa
            ToDate = setup.ToDate,
            ExtendedToDate = setup.ExtendedToDate,
            IsActive = setup.IsActive,
            IsAllCustomer = setup.IsAllCustomer,
            NotifyBeforeDays = setup.NotifyBeforeDays,
            Note = setup.Note,
            Customers = new List<PointSetupCustomerUpdateDto>(),
            Lines = new List<PointSetupLineUpdateDto>()
        };

        var (result, mess) = await sut.UpdateAsync(dto);

        result.Should().BeNull();
        mess!.Status.Should().Be(400);
        mess.Errors.Should().Contain("FromDate");
    }

    [Fact]
    public async Task UpdatePointSetup_DaCoTichDiem_ChoPhepSuaName()
    {
        var sut = Create(out var db);
        var setup = SeedSetup(db);
        db.CustomerPointLine.Add(new CustomerPointLine
        {
            CustomerId = 1, DocId = 100, DocType = PointObjTypes.Order,
            PoitnSetupId = setup.Id, PointChange = 10, DocDate = DateTime.UtcNow,
            ItemCode = "ITM", ItemName = "Item", Note = ""
        });
        db.SaveChanges();

        var dto = new PointSetupUpdateDto
        {
            Id = setup.Id,
            Name = "Tên mới",
            FromDate = setup.FromDate,
            ToDate = setup.ToDate,
            ExtendedToDate = setup.ExtendedToDate,
            IsActive = setup.IsActive,
            IsAllCustomer = setup.IsAllCustomer,
            NotifyBeforeDays = setup.NotifyBeforeDays,
            Note = setup.Note,
            Customers = new List<PointSetupCustomerUpdateDto>(),
            Lines = new List<PointSetupLineUpdateDto>()
        };

        var (result, mess) = await sut.UpdateAsync(dto);

        mess.Should().BeNull();
        result.Should().NotBeNull();
        result!.Name.Should().Be("Tên mới");
    }

    [Theory]
    [InlineData("IsActive")]
    [InlineData("Note")]
    [InlineData("NotifyBeforeDays")]
    public async Task UpdatePointSetup_DaCoTichDiem_BlockCacFieldKhongPhaiNameVaExtended(string fieldToChange)
    {
        var sut = Create(out var db);
        var setup = SeedSetup(db);
        db.CustomerPointLine.Add(new CustomerPointLine
        {
            CustomerId = 1, DocId = 100, DocType = PointObjTypes.Order,
            PoitnSetupId = setup.Id, PointChange = 10, DocDate = DateTime.UtcNow,
            ItemCode = "ITM", ItemName = "Item", Note = ""
        });
        db.SaveChanges();

        var dto = new PointSetupUpdateDto
        {
            Id = setup.Id,
            Name = setup.Name,
            FromDate = setup.FromDate,
            ToDate = setup.ToDate,
            ExtendedToDate = setup.ExtendedToDate,
            IsActive = fieldToChange == "IsActive" ? !setup.IsActive : setup.IsActive,
            IsAllCustomer = setup.IsAllCustomer,
            NotifyBeforeDays = fieldToChange == "NotifyBeforeDays" ? setup.NotifyBeforeDays + 1 : setup.NotifyBeforeDays,
            Note = fieldToChange == "Note" ? "đã sửa" : setup.Note,
            Customers = new List<PointSetupCustomerUpdateDto>(),
            Lines = new List<PointSetupLineUpdateDto>()
        };

        var (result, mess) = await sut.UpdateAsync(dto);

        result.Should().BeNull();
        mess.Should().NotBeNull();
        mess!.Status.Should().Be(400);
        mess.Errors.Should().Contain(fieldToChange);
    }

    [Fact]
    public async Task UpdatePointSetup_DaCoTichDiem_ChoPhepSuaExtendedToDate()
    {
        var sut = Create(out var db);
        var setup = SeedSetup(db);
        db.CustomerPointLine.Add(new CustomerPointLine
        {
            CustomerId = 1, DocId = 100, DocType = PointObjTypes.Order,
            PoitnSetupId = setup.Id, PointChange = 10, DocDate = DateTime.UtcNow,
            ItemCode = "ITM", ItemName = "Item", Note = ""
        });
        db.SaveChanges();

        var newExpiry = setup.ToDate.AddDays(90);
        var dto = new PointSetupUpdateDto
        {
            Id = setup.Id,
            Name = setup.Name,
            FromDate = setup.FromDate, // không đổi
            ToDate = setup.ToDate,     // không đổi
            ExtendedToDate = newExpiry, // ← gia hạn
            IsActive = true,
            IsAllCustomer = setup.IsAllCustomer,
            Customers = new List<PointSetupCustomerUpdateDto>(),
            Lines = new List<PointSetupLineUpdateDto>()
        };

        var (result, mess) = await sut.UpdateAsync(dto);

        mess.Should().BeNull();
        result.Should().NotBeNull();
        result!.ExtendedToDate.Should().Be(newExpiry);
    }
}
