namespace BackEndAPI.Models.Reports;

public class DebtReport
{
    public double DebtAge { get; set; }
    public double PayNow { get; set; }
    public double UnsecuredDebt { get; set; }
    public double GuaranteedDebt { get; set; }
    public double TotalDebt { get; set; }
}