namespace BackEndAPI.Models;

public class VnpayLogging
{
    public int Id { get; set; }
    public string? OrderId { get; set; }
    public string? TxnRef { get; set; }
    public long? Amount { get; set; }
    public string? Status { get; set; }
    public long? PaymentTranId { get; set; }

    /// <summary>
    /// VNPAYQR, VNBANK, INTCARD
    /// </summary>
    public required string BankCode { get; set; }

    public string? Payload { get; set; }
    public DateTime CreatedDate { get; set; }
}