namespace BackEndAPI.Service.Payments;

public class PayOneConfig
{
    
    public PayNowConfig? PayNow { get; set; } 
    public InstallmentConfig? Installment { get; set; }
    public string BaseUrl { get; set; } = string.Empty;
    public string UrlPrefix { get; set; } = string.Empty;
    public string Host { get; set; } = string.Empty;
    public string RedirectLink { get; set; } = string.Empty;
}

public class PayNowConfig
{
    public string Id { get; set; } = string.Empty;
    public string AccessCode { get; set; } = string.Empty;
    public string HashCode { get; set; } = string.Empty;
}

public class InstallmentConfig
{
    public string Id { get; set; } = string.Empty;
    public string AccessCode { get; set; } = string.Empty;
    public string HashCode { get; set; } = string.Empty;
}