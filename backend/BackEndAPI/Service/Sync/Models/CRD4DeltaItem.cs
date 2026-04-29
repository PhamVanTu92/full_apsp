namespace BackEndAPI.Service.Sync.Models;

/// <summary>
/// DTO cho delta sync invoice từ SAP B1.
/// Khác với CRD4InfoUpdate cũ: thêm CardCode (để group BP) và UpdateDate (để delta).
/// </summary>
public class CRD4DeltaItem
{
    public int DocEntry { get; set; }
    public string CardCode { get; set; } = string.Empty;
    public string? U_MDHPT { get; set; }
    public DateTime DocDate { get; set; }
    public DateTime DocDueDate { get; set; }
    public double DocTotal { get; set; }
    public double PaidToDate { get; set; }
    public string? U_HTTT1 { get; set; }
    public DateTime UpdateDate { get; set; }
}

public class SyncResult
{
    public int TotalFromSap { get; set; }
    public int Updated { get; set; }
    public int Inserted { get; set; }
    public int Skipped { get; set; }
    public DateTime NewCheckpoint { get; set; }
}
