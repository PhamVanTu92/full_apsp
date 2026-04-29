namespace BackEndAPI.Models.Reports;

public class OrderStateReport
{
    public int Pending { get; set; }
    public double TotalPending { get; set; }
    public int Complete { get; set; }
    public double TotalComplete { get; set; }
    public int OnDelivery { get; set; }
    public double TotalOnDelivery { get; set; }
    public int Confirmed { get; set; }
    public double TotalConfirmed { get; set; }
    public int Processing { get; set; }
    public double TotalProcessing { get; set; }
    public int Cancelled { get; set; }
    public double TotalCancelled { get; set; }
    public int Closed { get; set; }
    public double TotalClosed { get; set; }
    public int InPaying { get; set; }
    public double TotalInPaying { get; set; }
    public int Paid{ get; set; }
    public double TotalPaid { get; set; }
    public int Delivied { get; set; }
    public double TotalDelivied { get; set; }
}

