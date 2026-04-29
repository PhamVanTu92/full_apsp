using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.X509;

namespace BackEndAPI.Models.SAP;
public class Issue
{
    public string DocDate { get; set; }
    public string DocObjectCode { get; set; } = "60";
    public string U_BPCode { get; set; }
    public string U_NPPH { get; set; }
    public string U_XK { get; set; } = "1";
    public string U_MDHPT { get; set; }
    public string U_NNHG { get; set; }
    public string U_BSX { get; set; }
    public string U_CMND { get; set; }
    public string U_Description_vn { get; set; }
    public List<IssueLine> DocumentLines { get; set; } = new();
}
public class IssueLine
{
    public string ItemCode { get; set; } // Mã sản phẩm
    public double Quantity { get; set; } // Số lượng
    public string WarehouseCode { get; set; } = "GOI";
}
public class SapOrderBP
{
    public string? CardCode { get; set; }
    public string? FederalTaxID { get; set; }
    public string? U_DiachiHD { get; set; }
    public string? U_Khuvuc { get; set; }
}
public class BPResponse
{
    public List<SapOrderBP> Value { get; set; }
}
public class DraftResponse
{
    public List<DraftDH> Value { get; set; }
}
public class DraftDH
{
    public int DocEntry { get; set; }
   public string U_MDHPT { get; set; }
}
public class SapOrder
{
    public double TotalDiscount { get; set; }
    public int DocEntry { get; set; }
    public string CardCode { get; set; } // Mã khách hàng
    public string U_MDHPT { get; set; }
    public string DocDate { get; set; } // Ngày tạo đơn
    public string DocDueDate { get; set; } // Ngày đến hạn
    public string DocCurrency { get; set; } // Loại tiền tệ (VD: "USD", "VND")
    public string DocType { get; set; } = "dDocument_Items"; // Kiểu chứng từ
    public string DocObjectCode { get; set; } = "17";
    public string Comments { get; set; }
    public string U_BPCode { get; set; }
    public string U_NVKD1 { get; set; }
    public string U_NVKD2 { get; set; }
    public string U_NVKD3 { get; set; }
    public string U_NVKD4 { get; set; }
    public string U_CoAdd { get; set; }
    public string U_CoTaxNo { get; set; }
    public string U_CodeInv { get; set; } = "1";
    public string U_InvSerial{ get; set; } = "C"+DateTime.Now.Year.ToString().Substring(2)+"TAP";
    public string U_DeclareStat { get; set; } = "0";
    public string U_Type { get; set; } = "01";
    public string U_DHB { get; set; } = "01";
    public string U_Area { get; set; }
    public string U_NNHG { get; set; }
    public string U_BSX { get; set; }
    public string U_CMND { get; set; }
    public string U_Description_vn { get; set; }
    public List<SapOrderLine> DocumentLines { get; set; } = new();
    public SapPayment PaymentGroup { get; set; } // Thông tin thanh toán

    public SapAddress BillTo { get; set; } // Địa chỉ thanh toán
    public SapAddress ShipTo { get; set; } // Địa chỉ giao hàng
    
    public  string  U_HTTT1 { get; set; }
    
    public  string U_KV { get; set; }

    public double DiscountPercent { get; set; } // Chiết khấu đơn hàng
    public double VatPercent { get; set; } // VAT%
}

public class SapOrderLine
{
    public string ItemCode { get; set; } // Mã sản phẩm
    public string ItemDescription { get; set; } // Mô tả sản phẩm
    public double Quantity { get; set; } // Số lượng
    public double UnitPrice { get; set; } // Giá đơn vị
    public double LineTotal { get; set; } // Chiết khấu %
    public string U_LHH { get;set; }
    public string U_CTKM { get;set; }
    public string VatGroup { get; set; }
    
    [JsonIgnore]
    public int LineId { get; set; }
    // public string WarehouseCode { get; set; }
    [JsonProperty("U_BPCode")]
    public  string U_BPCode { get;set; }
}

public class SapPayment
{
    public string PaymentGroupCode { get; set; } // Mã phương thức thanh toán
    public double PaidAmount { get; set; }     // Số tiền đã thanh toán
}

public class SapAddress
{
    public string Address { get; set; }     // Địa chỉ
    public string City { get; set; }        // Thành phố
    public string Country { get; set; }     // Quốc gia
    public string ZipCode { get; set; }     // Mã bưu chính
}
