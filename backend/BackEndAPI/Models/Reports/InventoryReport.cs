namespace BackEndAPI.Models.Reports;

public class InventoryReport
{
    public class Inventory
    {
        public string CardCode { get; set; } // Mã hàng hóa
        public string CardName { get; set; } // Tên hàng hóa
        public string PackagingSpecifications { get; set; } // Quy cách bao bì
        public string Brand { get; set; } // Thương hiệu
        public string Category { get; set; } // Ngành hàng
        public string ProductType { get; set; }
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public double BeginQty { get; set; }
        public double InQty { get; set; }
        public double OutQty { get; set; }
        public double EndQty { get; set; }
        public ICollection<InventoryDetail> InventoryDetail { get; set; }
    }
    public class InventoryDetail
    {
        public DateTime DocDate { get; set; }
        public string CardName { get; set; }
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public double InQty { get; set; }
        public double OutQty { get; set; }
    }
    public class BalanceBP
    {
        public string CardCode { get; set; }
        public string CardName { get; set; }
        public double? OBDebit { get; set; }
        public double? OBCredit { get; set; }
        public double? Debit { get; set; }
        public double? Credit { get; set; }
        public double? EBDebit { get; set; }
        public double? EBCredit { get; set; }
        public ICollection<BPBalance> Detail { get; set; }
    }
    public class BPBalance
    {
        public string? FCCurrency { get; set; }
        public double? Rate { get; set; }
        public string? RefDate { get; set; }
        public string? TaxDate { get; set; }
        public double? Debit { get; set; }
        public string? VoucherNo { get; set; }
        public double? Credit { get; set; }
        public string? LineMemo { get; set; }
    }
    public class Balance
    {
        public string? FCCurrency { get; set; }
        public double? Rate { get; set; }
        public double? OBDebit { get; set; }
        public double? OBCredit { get; set; }
        public string? RefDate { get; set; }
        public string? TaxDate { get; set; }
        public double? Debit { get; set; }
        public string? VoucherNo { get; set; }
        public double? Credit { get; set; }
        public string? LineMemo { get; set; }
        public string? CardCode { get; set; }
        public string? CardName { get; set; }
    }
    public class BCCN
    {
        public string CardCode { get; set; }
        public string CardName { get; set; }
        public double Credit { get; set; }
        public double TimeCredit { get; set; }
        public double TotalCredit { get; set; }
        public double OverCredit { get; set; }
        public double Guarantee { get; set; }
        public double TimeGuarantee { get; set; }
        public double TotalGuarantee { get; set; }
        public double OverGuarantee { get; set; }
        public ICollection<BCCNDetail> BCCNDetail { get; set; }
    }
    public class BCCNDetail
    {
        public string DocCode { get; set; }
        public string InvoiceCode { get; set; }
        public string DocDate { get; set; }
        public string DocDueDate { get; set; }
        public double DocTotal { get; set; }
        public double PaidToDate { get; set; }
        public string Types { get; set; }
        public ICollection<BCCNInComing> BCCNInComing { get; set; }
    }
    public class BCCNInComing
    {
        public string PaymentNumber { get; set; }
        public string DocDate { get; set; }
        public double AmountPaid { get; set; }
    }
    public class LinkInovie
    {
        public string? CardCode { get; set; }
        public string? DocDate { get; set; }
        public string? DocDueDate { get; set; }
        public double? DocTotal { get; set; }
        public string? InvoiceCode { get; set; }
        public string? LinkInvoice { get; set; }
    }
    public class ZaloReport
    {
        public int DocId { get; set; }
        public string InvoiceCode { get; set; }
        public int ObjType { get; set; }
        public string TypeMess { get; set; } = "Confirmed";
        public string ZaloMess { get; set; }
        public string TypeMess1 { get; set; } = "Completed";
        public string ZaloMess1 { get; set; }
    }
    public class ReportPayNow
    {
        public int CardId { get; set; }
        public string? CardCode { get; set; }
        public string? CardName { get; set; }
        public DateTime? DocDate { get; set; }
        public int DocId { get; set; }
        public string? InvocieCode { get; set; }
        public double? Value { get; set; }
        public double? ValueInvoice { get; set; }
        public double? Bonus { get; set; }
        public double? Total { get; set; }
    }
    public class ProCommitment
    {
        public string Industry { get; set; }
        public string Brand { get; set; }
        public string ProductType { get; set; }
        public ICollection<DiscountInfo> DiscountInfo { get; set; }
    }

    public class DiscountInfo
    {
        public string Indicator { get; set; }
        public double ActualProduction { get; set; } 
        public double DiscountAmount { get; set; }
        public double Percentage { get; set; }
        public double BonusAmount { get; set; }
    }
}