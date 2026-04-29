using BackEndAPI.Models.BusinessPartners;
using BackEndAPI.Service.BusinessPartners.Address;
using BackEndAPI.Tests.Helpers;
using FluentAssertions;
using Xunit;

namespace BackEndAPI.Tests.Unit.Services;

/// <summary>
/// Test cho BPAddressService — service đầu tiên extract khỏi God class
/// BusinessPartnerService theo plan trong docs/REFACTOR_PLAN.md.
/// </summary>
public class BPAddressServiceTests
{
    private static BP CreateBpWithAddresses(int bpId, params CRD1[] addresses)
    {
        return new BP
        {
            Id = bpId,
            CardCode = $"C{bpId:D6}",
            CardName = $"Test BP {bpId}",
            CRD1 = addresses.ToList()
        };
    }

    [Fact]
    public async Task AddAsync_WhenBPExists_AddsAddressAndReturnsBp()
    {
        // Arrange
        using var db = TestDbContextFactory.Create();
        db.BP.Add(CreateBpWithAddresses(1));
        await db.SaveChangesAsync();
        var sut = new BPAddressService(db);

        var newAddress = new CRD1 { Address = "123 Test St", BPId = 1 };

        // Act
        var (bp, mess) = await sut.AddAsync(1, newAddress);

        // Assert
        mess.Should().BeNull();
        bp.Should().NotBeNull();
        bp!.CRD1.Should().HaveCount(1);
        bp.CRD1![0].Address.Should().Be("123 Test St");
    }

    [Fact]
    public async Task AddAsync_WhenBPNotFound_Returns404()
    {
        using var db = TestDbContextFactory.Create();
        var sut = new BPAddressService(db);

        var (bp, mess) = await sut.AddAsync(999, new CRD1 { Address = "x" });

        bp.Should().BeNull();
        mess.Should().NotBeNull();
        mess!.Status.Should().Be(404);
        mess.Errors.Should().Contain("999");
    }

    [Fact]
    public async Task RemoveAsync_RemovesOnlyTargetedAddresses()
    {
        using var db = TestDbContextFactory.Create();
        var addr1 = new CRD1 { Id = 1, Address = "addr-1", BPId = 1 };
        var addr2 = new CRD1 { Id = 2, Address = "addr-2", BPId = 1 };
        var addr3 = new CRD1 { Id = 3, Address = "addr-3", BPId = 1 };
        db.BP.Add(CreateBpWithAddresses(1, addr1, addr2, addr3));
        await db.SaveChangesAsync();
        var sut = new BPAddressService(db);

        var (bp, mess) = await sut.RemoveAsync(1, new List<int> { 1, 3 });

        mess.Should().BeNull();
        bp!.CRD1.Should().HaveCount(1);
        bp.CRD1![0].Id.Should().Be(2);
    }

    [Fact]
    public async Task RemoveAsync_WhenBPNotFound_Returns404()
    {
        using var db = TestDbContextFactory.Create();
        var sut = new BPAddressService(db);

        var (bp, mess) = await sut.RemoveAsync(999, new List<int> { 1 });

        bp.Should().BeNull();
        mess!.Status.Should().Be(404);
    }
}
