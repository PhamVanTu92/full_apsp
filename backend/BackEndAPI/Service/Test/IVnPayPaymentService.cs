using BackEndAPI.Models.Other;

namespace BackEndAPI.Service.Test;

public class CreatePaymentDto
{
    public int DocId { get; set; }
    public required string PaymentType { get; set; }
}

public class VnPayRespons
{
    public string? Message { get; set; }
    public string? RspCode { get; set; }
}

public interface IVnPayPaymentService
{
    Task<(string?, Mess?)>          PaymentAsync(CreatePaymentDto dto);
    Task<(string?, Mess?, object?)> CallBackAsync(IQueryCollection query);

    Task<VnPayRespons> IpnAsync(IQueryCollection query);
}