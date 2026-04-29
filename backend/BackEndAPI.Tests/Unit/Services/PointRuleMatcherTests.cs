using BackEndAPI.Models.Promotion;
using BackEndAPI.Service.Promotions;
using FluentAssertions;
using Xunit;

namespace BackEndAPI.Tests.Unit.Services;

/// <summary>
/// Test pure function PointRuleMatcher — đảm bảo logic business chốt 2026-04-28:
///   • ItemType="I" (Item Direct) → bỏ qua Packing, chỉ cần Industry + Brand match
///   • ItemType="G" (Item Group)  → check đầy đủ Industry + Brand + ItemTypeId + Packing
///   • Industry/Brand/Packing rỗng = match all
/// </summary>
public class PointRuleMatcherTests
{
    private static PointSetupLine Rule(
        int point = 10,
        int[]? industryIds = null,
        int[]? brandIds = null,
        int[]? packingIds = null,
        (string Type, string ItemCode, int ItemId)[]? items = null)
    {
        return new PointSetupLine
        {
            Point = point,
            Industry = (industryIds ?? Array.Empty<int>())
                .Select(id => new PointSetupIndustry { IndustryId = id }).ToList(),
            Brands = (brandIds ?? Array.Empty<int>())
                .Select(id => new PointSetupBrand { BrandId = id }).ToList(),
            Packings = (packingIds ?? Array.Empty<int>())
                .Select(id => new PointSetupPacking { PackingId = id }).ToList(),
            ItemType = (items ?? Array.Empty<(string, string, int)>())
                .Select(x => new PointSetupItemType { ItemType = x.Type, ItemCode = x.ItemCode, ItemId = x.ItemId }).ToList()
        };
    }

    private static DocLineForPoint Line(
        int industryId = 100, int brandId = 200, int packingId = 300, int itemTypeId = 400,
        int itemId = 1, string itemCode = "ITEM-A", double qty = 1)
        => new DocLineForPoint
        {
            IndustryId = industryId, BrandId = brandId, PackingId = packingId,
            ItemTypeId = itemTypeId, ItemId = itemId, ItemCode = itemCode, Quantity = qty
        };

    // ── Item Direct (Type=I) — bỏ qua Packing ───────────────────────────

    [Fact]
    public void ItemDirect_KhopItemCode_BoQuaPacking_KhongBatBuocPacking()
    {
        // Rule: chỉ định ITEM-A trực tiếp + yêu cầu Packing=999
        // Doc: ITEM-A nhưng Packing=300 (KHÔNG khớp packing)
        // → vẫn match vì Item Direct override Packing
        var rule = Rule(packingIds: new[] { 999 },
            items: new[] { ("I", "ITEM-A", 0) });
        var doc = Line(packingId: 300, itemCode: "ITEM-A");

        PointRuleMatcher.Matches(rule, doc).Should().BeTrue();
    }

    [Fact]
    public void ItemDirect_KhongKhopItemCode_KhongMatch()
    {
        var rule = Rule(items: new[] { ("I", "ITEM-A", 0) });
        var doc = Line(itemCode: "ITEM-X");

        PointRuleMatcher.Matches(rule, doc).Should().BeFalse();
    }

    [Fact]
    public void ItemDirect_KhopItem_NhungKhongKhopIndustry_KhongMatch()
    {
        // Industry vẫn được check kể cả Item Direct
        var rule = Rule(industryIds: new[] { 999 },
            items: new[] { ("I", "ITEM-A", 0) });
        var doc = Line(industryId: 100, itemCode: "ITEM-A");

        PointRuleMatcher.Matches(rule, doc).Should().BeFalse();
    }

    [Fact]
    public void ItemDirect_KhopItem_NhungKhongKhopBrand_KhongMatch()
    {
        var rule = Rule(brandIds: new[] { 999 },
            items: new[] { ("I", "ITEM-A", 0) });
        var doc = Line(brandId: 200, itemCode: "ITEM-A");

        PointRuleMatcher.Matches(rule, doc).Should().BeFalse();
    }

    // ── Item Group (Type=G) — check đầy đủ ──────────────────────────────

    [Fact]
    public void ItemGroup_KhopGroupId_KhopPacking_Match()
    {
        var rule = Rule(packingIds: new[] { 300 },
            items: new[] { ("G", "", 400) });
        var doc = Line(itemTypeId: 400, packingId: 300);

        PointRuleMatcher.Matches(rule, doc).Should().BeTrue();
    }

    [Fact]
    public void ItemGroup_KhopGroupId_KhongKhopPacking_KhongMatch()
    {
        // Item Group BẮT BUỘC khớp Packing
        var rule = Rule(packingIds: new[] { 300 },
            items: new[] { ("G", "", 400) });
        var doc = Line(itemTypeId: 400, packingId: 999);

        PointRuleMatcher.Matches(rule, doc).Should().BeFalse();
    }

    [Fact]
    public void ItemGroup_KhongKhopGroupId_KhongMatch()
    {
        var rule = Rule(items: new[] { ("G", "", 400) });
        var doc = Line(itemTypeId: 999);

        PointRuleMatcher.Matches(rule, doc).Should().BeFalse();
    }

    [Fact]
    public void ItemGroup_RuleKhongCoPacking_KhongCanCheck()
    {
        // Packing rỗng = match all
        var rule = Rule(items: new[] { ("G", "", 400) });
        var doc = Line(itemTypeId: 400, packingId: 12345);

        PointRuleMatcher.Matches(rule, doc).Should().BeTrue();
    }

    // ── Mixed: rule có cả I và G ────────────────────────────────────────

    [Fact]
    public void MixedItemDirectAndGroup_ItemDirectHit_BoQuaPacking()
    {
        var rule = Rule(packingIds: new[] { 999 },
            items: new[] { ("I", "ITEM-A", 0), ("G", "", 400) });
        var doc = Line(itemCode: "ITEM-A", itemTypeId: 999, packingId: 300);

        PointRuleMatcher.Matches(rule, doc).Should().BeTrue("Item Direct match → bỏ qua Packing");
    }

    [Fact]
    public void MixedItemDirectAndGroup_ItemDirectMiss_FallthroughGroupCheckPacking()
    {
        // ITEM-X không trong Item Direct list, nhưng ItemTypeId=400 + Packing=300 khớp Group
        var rule = Rule(packingIds: new[] { 300 },
            items: new[] { ("I", "ITEM-A", 0), ("G", "", 400) });
        var doc = Line(itemCode: "ITEM-X", itemTypeId: 400, packingId: 300);

        PointRuleMatcher.Matches(rule, doc).Should().BeTrue("Fallthrough sang Group check");
    }

    [Fact]
    public void MixedItemDirectAndGroup_ItemDirectMiss_GroupKhongKhopPacking_KhongMatch()
    {
        var rule = Rule(packingIds: new[] { 300 },
            items: new[] { ("I", "ITEM-A", 0), ("G", "", 400) });
        var doc = Line(itemCode: "ITEM-X", itemTypeId: 400, packingId: 999);

        PointRuleMatcher.Matches(rule, doc).Should().BeFalse();
    }

    // ── No filter rules ─────────────────────────────────────────────────

    [Fact]
    public void RuleKhongCoItemType_ChiCanIndustryBrandMatch()
    {
        // Backwards-compat: rule cũ có thể không config ItemType
        var rule = Rule();
        var doc = Line();

        PointRuleMatcher.Matches(rule, doc).Should().BeTrue();
    }

    [Fact]
    public void CalculatePoints_TraDungSoDiem()
    {
        var rule = Rule(point: 15, items: new[] { ("I", "ITEM-A", 0) });
        var doc = Line(itemCode: "ITEM-A", qty: 4);

        PointRuleMatcher.CalculatePoints(rule, doc).Should().Be(60);
    }

    [Fact]
    public void CalculatePoints_KhongMatch_TraVe0()
    {
        var rule = Rule(point: 15, items: new[] { ("I", "ITEM-A", 0) });
        var doc = Line(itemCode: "ITEM-X", qty: 4);

        PointRuleMatcher.CalculatePoints(rule, doc).Should().Be(0);
    }
}
