namespace BackEndAPI.Service.Promotions;

/// <summary>
/// Tập hợp các status code của ODOC liên quan đến tích/trừ/hoàn điểm.
/// Tránh hardcode magic strings rải rác.
/// </summary>
public static class PointStatuses
{
    /// <summary>"DHT" — đơn hàng (ObjType=22) hoàn thành → cộng điểm vào cycle.</summary>
    public const string OrderCompleted = "DHT";

    /// <summary>"DXN" — VPKM (ObjType=12) xác nhận → trừ điểm.</summary>
    public const string ExchangeConfirmed = "DXN";

    /// <summary>Status reverse: huỷ/đóng/huỷ-2 → hoàn điểm về trạng thái trước đó.</summary>
    public static readonly IReadOnlySet<string> ReverseStatuses =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "HUY", "DONG", "HUY2" };

    public static bool IsOrderCompleted(string? status)
        => string.Equals(status, OrderCompleted, StringComparison.OrdinalIgnoreCase);

    public static bool IsExchangeConfirmed(string? status)
        => string.Equals(status, ExchangeConfirmed, StringComparison.OrdinalIgnoreCase);

    public static bool IsReverse(string? status)
        => status != null && ReverseStatuses.Contains(status);
}

/// <summary>
/// ObjType cho CustomerPoint / ODOC.
/// </summary>
public static class PointObjTypes
{
    public const int Order = 22;
    public const int Vpkm = 12;
}
