using BackEndAPI.Models.Promotion;

namespace BackEndAPI.Service.Promotions;

/// <summary>
/// DTO mô tả 1 line trong document để check tích điểm.
/// Pure data, không phụ thuộc EF entity.
/// </summary>
public class DocLineForPoint
{
    public int IndustryId { get; init; }
    public int BrandId { get; init; }
    public int PackingId { get; init; }
    public int ItemTypeId { get; init; }
    public int ItemId { get; init; }
    public string ItemCode { get; init; } = "";
    public double Quantity { get; init; }
}

/// <summary>
/// Pure function: kiểm tra 1 docLine có thoả 1 PointSetupLine không.
///
/// Business rule (chốt 2026-04-28):
///   • ItemType = "I" (Item Direct): khớp ItemCode chính xác → BỎ QUA Packing,
///     chỉ check Industry + Brand. Item Direct là quy tắc cụ thể, override packing.
///   • ItemType = "G" (Item Group): khớp ItemTypeId → check đầy đủ Industry + Brand + Packing.
///   • Industry/Brand/Packing rỗng → coi là "match all" (no filter).
///
/// Thiết kế độc lập với EF/DB → 100% testable bằng unit test.
/// </summary>
public static class PointRuleMatcher
{
    /// <summary>
    /// Trả về true nếu docLine khớp rule, false nếu không.
    /// Helper public dùng chung giữa CalculatePoints (lưu thật) và
    /// CalculatePointCheck (preview) để đảm bảo cùng logic.
    /// </summary>
    public static bool Matches(PointSetupLine rule, DocLineForPoint docLine)
    {
        bool matchIndustry = rule.Industry == null || !rule.Industry.Any()
            || rule.Industry.Any(i => i.IndustryId == docLine.IndustryId);

        bool matchBrand = rule.Brands == null || !rule.Brands.Any()
            || rule.Brands.Any(b => b.BrandId == docLine.BrandId);

        if (!matchIndustry || !matchBrand) return false;

        bool hasItemDirect = rule.ItemType?.Any(t => t.ItemType == "I") == true;
        bool hasItemGroup = rule.ItemType?.Any(t => t.ItemType == "G") == true;

        // Ưu tiên Item Direct: nếu rule có ItemType="I" và khớp ItemCode → match,
        // KHÔNG check Packing.
        if (hasItemDirect)
        {
            bool itemDirectHit = rule.ItemType!
                .Where(t => t.ItemType == "I")
                .Any(t => t.ItemCode == docLine.ItemCode);
            if (itemDirectHit) return true;
            // Fallthrough: Item Direct không match, nhưng có thể match qua Item Group bên dưới.
        }

        // Item Group: phải khớp ItemTypeId AND Packing (nếu có).
        if (hasItemGroup)
        {
            bool itemGroupHit = rule.ItemType!
                .Where(t => t.ItemType == "G")
                .Any(t => t.ItemId == docLine.ItemTypeId);

            bool matchPacking = rule.Packings == null || !rule.Packings.Any()
                || rule.Packings.Any(p => p.PackingId == docLine.PackingId);

            return itemGroupHit && matchPacking;
        }

        // Rule không có ItemType nào: chỉ cần Industry + Brand đã match.
        // (Hành vi backward-compat — rule cũ có thể không config ItemType.)
        return !(rule.ItemType?.Any() ?? false);
    }

    /// <summary>
    /// Tính điểm cộng cho 1 docLine (rule.Point × Quantity), trả 0 nếu không match.
    /// </summary>
    public static double CalculatePoints(PointSetupLine rule, DocLineForPoint docLine)
        => Matches(rule, docLine) ? rule.Point * docLine.Quantity : 0;
}
