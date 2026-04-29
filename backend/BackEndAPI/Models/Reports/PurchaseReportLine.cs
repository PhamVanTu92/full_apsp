namespace BackEndAPI.Models.Reports;

public class TopItemReport
{
    public string ItemName { get; set; }
    public double Quantity { get; set; }
}


public class PurchaseReport
{
    public double? PurchasedQuantity { get; set; }
    public double PromotionalQuantity { get; set; }
    public double TotalAccumulatedVolume { get; set; }
    public int TotalOrders { get; set; }
    public List<PurchaseReportLine> Lines { get; set; }
}
public class PurchaseReportLine
{
    public string? ItemCode { get; set; } 
    public string? ItemName { get; set; }
    public string Brand { get; set; } 
    public string Industry { get; set; }
    public string ItemType { get; set; }
    public string Packaging { get; set; }
    public double? PurchasedQuantity { get; set; }
    public double PromotionalQuantity { get; set; }
    public double TotalAccumulatedVolume { get; set; }
    public int TotalOrders { get; set; }
}

public class ItemPurchaseReport
{
    public double QuantityPurchased { get; set; } // Số lượng mua
    public double PromotionalQuantity { get; set; } // Số lượng khuyến mãi
    public double Discount { get; set; } // Giảm giá
    public double TaxTotal { get; set; } // Thuế suất
    public double TotalAmount { get; set; } // Tổng tiền
    public List<ItemPurchaseReportLine> Lines { get; set; }
}

public class ItemPurchaseReportLine
{
    public int OrderId { get; set; }
    public string? Currency { get; set; } // Giảm giá
    public string OrderCode { get; set; } // Mã đơn hàng
    public DateTime OrderDate { get; set; } // Ngày đặt hàng
    public double QuantityPurchased { get; set; } // Số lượng mua
    public double PromotionalQuantity { get; set; } // Số lượng khuyến mãi
    public double UnitPrice { get; set; } // Đơn giá (vnđ)
    public double Discount { get; set; } // Giảm giá
    public string? DiscountType { get; set; } // Giảm giá
    public string? PaymentType { get; set; } // Giảm giá
    public double TaxTotal { get; set; } // Thuế suất
    public double TotalAmount => QuantityPurchased * UnitPrice + TaxTotal - Discount; // Tổng tiền
    public bool IsIncludedInProduction { get; set; } // Được tính sản lượng
}