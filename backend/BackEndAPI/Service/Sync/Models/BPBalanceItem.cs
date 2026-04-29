namespace BackEndAPI.Service.Sync.Models;

/// <summary>
/// DTO cho BP "current account balance" từ SAP.
/// </summary>
public class BPBalanceItem
{
    public string CardCode { get; set; } = string.Empty;
    public double CurrentAccountBalance { get; set; }
    public DateTime UpdateDate { get; set; }
}
