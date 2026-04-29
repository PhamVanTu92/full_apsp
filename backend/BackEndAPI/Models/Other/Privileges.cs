using BackEndAPI.Models.BPGroups;
using System.Text.Json.Serialization;

namespace BackEndAPI.Models.Other
{
    public class Privileges
    {
        public int Id { get; set; }
        public string Code { get; set; }
        [JsonPropertyName("privilegeName")] public string Name { get; set; }
        public int NumberOrder { get; set; }
        public int? ParentId { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ICollection<Privileges>? Children { get; set; }
    }

    public static class PrivilegesList
    {
        public static List<Privileges> GetPrivileges()
        {
            List<Privileges> listPrivileges = new List<Privileges>
            {
                new Privileges { Id = 1, Code = "System", Name = "Hệ thống", NumberOrder = 1 },
                new Privileges { Id = 2, Code = "Item", Name = "Hàng hóa", NumberOrder = 2 },
                new Privileges { Id = 3, Code = "Transaction", Name = "Giao dịch", NumberOrder = 3 },
                new Privileges { Id = 4, Code = "BusinessPartner", Name = "Đối tác", NumberOrder = 4 },
                new Privileges { Id = 5, Code = "Report", Name = "Báo cáo", NumberOrder = 5 },
                new Privileges { Id = 6, Code = "Cash", Name = "Sổ quỹ", NumberOrder = 6 },
                new Privileges { Id = 7, Code = "Promotion", Name = "Khuyến mại", NumberOrder = 7 },
                new Privileges { Id = 8, Code = "Voucher", Name = "Voucher", NumberOrder = 8 },
                new Privileges { Id = 9, Code = "Employee", Name = "Nhân viên", NumberOrder = 9 },
                new Privileges { Id = 10, Code = "Coupon", Name = "Coupon", NumberOrder = 10 },

                // id = 11
                new Privileges { Id = 11, Code = "Branch", Name = "Chi nhánh", NumberOrder = 4, ParentId = 1 },
                //id = 12
                new Privileges
                    { Id = 12, Code = "ExpensesOther", Name = "Chi phí nhập hàng", NumberOrder = 6, ParentId = 1 },
                //id = 13
                new Privileges { Id = 13, Code = "PosParameter", Name = "Thiết lập", NumberOrder = 1, ParentId = 1 },
                new Privileges { Id = 14, Code = "Surcharge", Name = "Thu khác", NumberOrder = 5, ParentId = 1 },
                new Privileges { Id = 15, Code = "PrintTemplate", Name = "Mẫu in", NumberOrder = 2, ParentId = 1 },
                new Privileges { Id = 16, Code = "User", Name = "Người dùng", NumberOrder = 4, ParentId = 1 },
                new Privileges
                    { Id = 17, Code = "SmsEmailTemplate", Name = "SMS / Email", NumberOrder = 7, ParentId = 1 },
                new Privileges
                    { Id = 18, Code = "AuditTrail", Name = "Lịch sử thao tác", NumberOrder = 8, ParentId = 1 },
                new Privileges { Id = 19, Code = "DashBoard", Name = "Tổng quan", NumberOrder = 9, ParentId = 1 },

                // Parent id = 11
                new Privileges { Id = 20, Code = "Branch_Read", Name = "Xem DS", NumberOrder = 1, ParentId = 11 },
                new Privileges { Id = 21, Code = "Branch_Create", Name = "Thêm mới", NumberOrder = 2, ParentId = 11 },
                new Privileges { Id = 22, Code = "Branch_Update", Name = "Cập nhật", NumberOrder = 3, ParentId = 11 },
                new Privileges { Id = 23, Code = "Branch_Delete", Name = "Xóa", NumberOrder = 4, ParentId = 11 },
                // Parent id = 12
                new Privileges
                    { Id = 24, Code = "ExpensesOther_Read", Name = "Xem DS", NumberOrder = 1, ParentId = 12 },
                new Privileges
                    { Id = 25, Code = "ExpensesOther_Create", Name = "Thêm mới", NumberOrder = 2, ParentId = 12 },
                new Privileges
                    { Id = 26, Code = "ExpensesOther_Update", Name = "Cập nhật", NumberOrder = 3, ParentId = 12 },
                new Privileges { Id = 27, Code = "ExpensesOther_Delete", Name = "Xóa", NumberOrder = 4, ParentId = 12 },
                //parent id = 13
                new Privileges
                    { Id = 28, Code = "PosParameter_Update", Name = "Cập nhật", NumberOrder = 1, ParentId = 13 },

                // Parent id = 14
                new Privileges { Id = 29, Code = "Surcharge_Read", Name = "Xem DS", NumberOrder = 1, ParentId = 14 },
                new Privileges
                    { Id = 30, Code = "Surcharge_Create", Name = "Thêm mới", NumberOrder = 2, ParentId = 14 },
                new Privileges
                    { Id = 31, Code = "Surcharge_Update", Name = "Cập nhật", NumberOrder = 3, ParentId = 14 },
                new Privileges { Id = 32, Code = "Surcharge_Delete", Name = "Xóa", NumberOrder = 4, ParentId = 14 },
                // Parent id = 15
                new Privileges
                    { Id = 33, Code = "PrintTemplate_Read", Name = "Xem DS", NumberOrder = 1, ParentId = 15 },
                new Privileges
                    { Id = 34, Code = "PrintTemplate_Update", Name = "Cập nhật", NumberOrder = 2, ParentId = 15 },
                new Privileges { Id = 35, Code = "PrintTemplate_Delete", Name = "Xóa", NumberOrder = 3, ParentId = 15 },
                // Parent id = 16
                new Privileges { Id = 36, Code = "User_Read", Name = "Xem DS", NumberOrder = 1, ParentId = 16 },
                new Privileges { Id = 37, Code = "User_Create", Name = "Thêm mới", NumberOrder = 2, ParentId = 16 },
                new Privileges { Id = 38, Code = "User_Update", Name = "Cập nhật", NumberOrder = 3, ParentId = 16 },
                new Privileges { Id = 39, Code = "User_Delete", Name = "Xóa", NumberOrder = 4, ParentId = 16 },
                new Privileges { Id = 40, Code = "User_Export", Name = "Xuất file", NumberOrder = 5, ParentId = 16 },

                // Parent id = 17
                new Privileges
                    { Id = 41, Code = "SmsEmailTemplate_Read", Name = "Xem DS", NumberOrder = 1, ParentId = 17 },
                new Privileges
                    { Id = 42, Code = "SmsEmailTemplate_Create", Name = "Thêm mới", NumberOrder = 2, ParentId = 17 },
                new Privileges
                    { Id = 43, Code = "SmsEmailTemplate_Update", Name = "Cập nhật", NumberOrder = 3, ParentId = 17 },
                new Privileges
                    { Id = 44, Code = "SmsEmailTemplate_Delete", Name = "Xóa", NumberOrder = 4, ParentId = 17 },
                new Privileges
                    { Id = 45, Code = "SmsEmailTemplate_SendSMS", Name = "Gửi SMS", NumberOrder = 5, ParentId = 17 },
                new Privileges
                {
                    Id = 46, Code = "SmsEmailTemplate_SendEmail", Name = "Gửi Email", NumberOrder = 6, ParentId = 17
                },
                new Privileges
                {
                    Id = 47, Code = "SmsEmailTemplate_SendZalo", Name = "Gửi tin nhắn Zalo", NumberOrder = 7,
                    ParentId = 17
                },
                // Parent id = 18
                new Privileges { Id = 48, Code = "AuditTrail_Read", Name = "Xem", NumberOrder = 1, ParentId = 18 },
                // Parent id = 19
                new Privileges { Id = 49, Code = "DashBoard_Read", Name = "Xem", NumberOrder = 1, ParentId = 19 },

                // Id = 50
                new Privileges { Id = 50, Code = "Manufacturing", Name = "Sản xuất", NumberOrder = 4, ParentId = 2 },
                new Privileges
                    { Id = 51, Code = "Manufacturing_Read", Name = "Xem DS", NumberOrder = 1, ParentId = 50 },
                new Privileges
                    { Id = 52, Code = "Manufacturing_Create", Name = "Thêm mới", NumberOrder = 2, ParentId = 50 },
                new Privileges
                    { Id = 53, Code = "Manufacturing_Update", Name = "Cập nhập", NumberOrder = 3, ParentId = 50 },
                new Privileges { Id = 54, Code = "Manufacturing_Delete", Name = "Xóa", NumberOrder = 4, ParentId = 50 },
                new Privileges
                    { Id = 55, Code = "Manufacturing_Export", Name = "Xuất file", NumberOrder = 5, ParentId = 50 },
                //ID = 56
                new Privileges { Id = 56, Code = "Product", Name = "Danh mục sản phẩm", NumberOrder = 1, ParentId = 2 },
                new Privileges { Id = 57, Code = "Product_Read", Name = "Xem DS", NumberOrder = 1, ParentId = 56 },
                new Privileges { Id = 58, Code = "Product_Create", Name = "Thêm mới", NumberOrder = 2, ParentId = 56 },
                new Privileges { Id = 59, Code = "Product_Update", Name = "Cập nhập", NumberOrder = 3, ParentId = 56 },
                new Privileges { Id = 60, Code = "Product_Delete", Name = "Xóa", NumberOrder = 4, ParentId = 56 },
                new Privileges
                    { Id = 61, Code = "Product_PurchasePrice", Name = "Giá nhập", NumberOrder = 5, ParentId = 56 },
                new Privileges { Id = 62, Code = "Product_Cost", Name = "Giá vốn", NumberOrder = 6, ParentId = 56 },
                new Privileges { Id = 63, Code = "Product_Import", Name = "Import", NumberOrder = 7, ParentId = 56 },
                new Privileges { Id = 64, Code = "Product_Export", Name = "Xuất file", NumberOrder = 8, ParentId = 56 },


                //ID = 65
                new Privileges { Id = 65, Code = "PriceBook", Name = "Thiết lập giá", NumberOrder = 2, ParentId = 2 },
                new Privileges { Id = 66, Code = "PriceBook_Read", Name = "Xem DS", NumberOrder = 1, ParentId = 65 },
                new Privileges
                    { Id = 67, Code = "PriceBook_Create", Name = "Thêm mới", NumberOrder = 2, ParentId = 65 },
                new Privileges
                    { Id = 68, Code = "PriceBook_Update", Name = "Cập nhập", NumberOrder = 3, ParentId = 65 },
                new Privileges { Id = 69, Code = "PriceBook_Delete", Name = "Xóa", NumberOrder = 4, ParentId = 65 },
                new Privileges { Id = 70, Code = "PriceBook_Import", Name = "Import", NumberOrder = 5, ParentId = 65 },
                new Privileges
                    { Id = 71, Code = "PriceBook_Export", Name = "Xuất file", NumberOrder = 6, ParentId = 65 },

                //ID = 72
                new Privileges { Id = 72, Code = "StockTake", Name = "Kiểm kho", NumberOrder = 3, ParentId = 2 },
                new Privileges { Id = 73, Code = "StockTake_Read", Name = "Xem DS", NumberOrder = 1, ParentId = 72 },
                new Privileges
                    { Id = 74, Code = "StockTake_Create", Name = "Thêm mới", NumberOrder = 2, ParentId = 72 },
                new Privileges
                    { Id = 75, Code = "StockTake_Update", Name = "Cập nhập", NumberOrder = 3, ParentId = 72 },
                new Privileges { Id = 76, Code = "StockTake_Delete", Name = "Xóa", NumberOrder = 4, ParentId = 72 },
                new Privileges { Id = 77, Code = "StockTake_Import", Name = "Import", NumberOrder = 5, ParentId = 72 },
                new Privileges
                    { Id = 78, Code = "StockTake_Export", Name = "Xuất file", NumberOrder = 6, ParentId = 72 },
                new Privileges
                    { Id = 79, Code = "StockTake_Inventory", Name = "Xem tồn kho", NumberOrder = 7, ParentId = 72 },
                new Privileges { Id = 80, Code = "StockTake_Clone", Name = "Sao chép", NumberOrder = 8, ParentId = 72 },
                new Privileges
                    { Id = 81, Code = "StockTake_Finish", Name = "Hoàn thành", NumberOrder = 9, ParentId = 72 },

                //ID = 82
                new Privileges { Id = 82, Code = "DamageItem", Name = "Xuất hủy", NumberOrder = 3, ParentId = 3 },
                new Privileges { Id = 83, Code = "DamageItem_Read", Name = "Xem DS", NumberOrder = 1, ParentId = 82 },
                new Privileges
                    { Id = 84, Code = "DamageItem_Create", Name = "Thêm mới", NumberOrder = 2, ParentId = 82 },
                new Privileges
                    { Id = 85, Code = "DamageItem_Update", Name = "Cập nhập", NumberOrder = 3, ParentId = 82 },
                new Privileges { Id = 86, Code = "DamageItem_Delete", Name = "Xóa", NumberOrder = 4, ParentId = 82 },
                new Privileges
                    { Id = 87, Code = "DamageItem_Clone", Name = "Sao chép", NumberOrder = 5, ParentId = 82 },
                new Privileges
                    { Id = 88, Code = "DamageItem_Export", Name = "Xuất file", NumberOrder = 6, ParentId = 82 },

                //ID = 89
                new Privileges { Id = 89, Code = "Customer", Name = "Khách hàng", NumberOrder = 1, ParentId = 4 },
                new Privileges { Id = 90, Code = "Customer_Read", Name = "Xem DS", NumberOrder = 1, ParentId = 89 },
                new Privileges { Id = 91, Code = "Customer_Create", Name = "Thêm mới", NumberOrder = 2, ParentId = 89 },
                new Privileges { Id = 92, Code = "Customer_Update", Name = "Cập nhập", NumberOrder = 3, ParentId = 89 },
                new Privileges { Id = 93, Code = "Customer_Delete", Name = "Xóa", NumberOrder = 4, ParentId = 89 },
                new Privileges
                    { Id = 94, Code = "Customer_ViewPhone", Name = "Điện thoại", NumberOrder = 5, ParentId = 89 },
                new Privileges { Id = 95, Code = "Customer_Import", Name = "Import", NumberOrder = 6, ParentId = 89 },
                new Privileges
                    { Id = 96, Code = "Customer_Export", Name = "Xuất file", NumberOrder = 5, ParentId = 89 },
                new Privileges
                {
                    Id = 97, Code = "Customer_UpdateGroup", Name = "Cập nhập nhóm khách hàng", NumberOrder = 6,
                    ParentId = 89
                },

                //ID = 98
                new Privileges
                    { Id = 98, Code = "CustomerPointAdjustment", Name = "Tích điểm KH", NumberOrder = 4, ParentId = 4 },
                new Privileges
                    { Id = 99, Code = "CustomerPointAdjustment_Read", Name = "Xem DS", NumberOrder = 1, ParentId = 98 },
                new Privileges
                {
                    Id = 100, Code = "CustomerPointAdjustment_Update", Name = "Cập nhập", NumberOrder = 2, ParentId = 98
                },

                //ID = 105
                new Privileges
                    { Id = 105, Code = "SupplierAdjustment", Name = "Công nợ NCC", NumberOrder = 6, ParentId = 4 },
                new Privileges
                    { Id = 106, Code = "SupplierAdjustment_Read", Name = "Xem DS", NumberOrder = 1, ParentId = 105 },
                new Privileges
                {
                    Id = 107, Code = "SupplierAdjustment_Create", Name = "Thêm mới", NumberOrder = 2, ParentId = 105
                },
                new Privileges
                {
                    Id = 108, Code = "SupplierAdjustment_Update", Name = "Cập nhập", NumberOrder = 3, ParentId = 105
                },
                new Privileges
                    { Id = 109, Code = "SupplierAdjustment_Delete", Name = "Xóa", NumberOrder = 4, ParentId = 105 },

                //ID = 110
                new Privileges
                {
                    Id = 110, Code = "DeliveryAdjustment", Name = "Công nợ đối tác GH", NumberOrder = 9, ParentId = 4
                },
                new Privileges
                    { Id = 111, Code = "DeliveryAdjustment_Read", Name = "Xem DS", NumberOrder = 1, ParentId = 110 },
                new Privileges
                {
                    Id = 112, Code = "DeliveryAdjustment_Create", Name = "Thêm mới", NumberOrder = 2, ParentId = 110
                },
                new Privileges
                {
                    Id = 113, Code = "DeliveryAdjustment_Update", Name = "Cập nhập", NumberOrder = 3, ParentId = 110
                },
                new Privileges
                    { Id = 114, Code = "DeliveryAdjustment_Delete", Name = "Xóa", NumberOrder = 4, ParentId = 110 },

                //ID = 115
                new Privileges { Id = 115, Code = "Supplier", Name = "Nhà cung cấp", NumberOrder = 5, ParentId = 4 },
                new Privileges { Id = 116, Code = "Supplier_Read", Name = "Xem DS", NumberOrder = 1, ParentId = 115 },
                new Privileges
                    { Id = 117, Code = "Supplier_Create", Name = "Thêm mới", NumberOrder = 2, ParentId = 115 },
                new Privileges
                    { Id = 118, Code = "Supplier_Update", Name = "Cập nhập", NumberOrder = 3, ParentId = 115 },
                new Privileges { Id = 119, Code = "Supplier_Delete", Name = "Xóa", NumberOrder = 4, ParentId = 115 },
                new Privileges
                    { Id = 120, Code = "Supplier_ViewPhone", Name = "Điện thoại", NumberOrder = 5, ParentId = 115 },
                new Privileges { Id = 121, Code = "Supplier_Import", Name = "Import", NumberOrder = 6, ParentId = 115 },
                new Privileges
                    { Id = 122, Code = "Supplier_Export", Name = "Xuất file", NumberOrder = 7, ParentId = 115 },
                new Privileges
                {
                    Id = 123, Code = "Supplier_UpdateGroup", Name = "Cập nhập nhóm NCC", NumberOrder = 8, ParentId = 115
                },


                //ID = 124
                new Privileges
                    { Id = 124, Code = "PartnerDelivery", Name = "Đối tác giao hàng", NumberOrder = 8, ParentId = 4 },
                new Privileges
                    { Id = 125, Code = "PartnerDelivery_Read", Name = "Xem DS", NumberOrder = 1, ParentId = 124 },
                new Privileges
                    { Id = 126, Code = "PartnerDelivery_Create", Name = "Thêm mới", NumberOrder = 2, ParentId = 124 },
                new Privileges
                    { Id = 127, Code = "PartnerDelivery_Update", Name = "Cập nhập", NumberOrder = 3, ParentId = 124 },
                new Privileges
                    { Id = 128, Code = "PartnerDelivery_Delete", Name = "Xóa", NumberOrder = 4, ParentId = 124 },
                new Privileges
                    { Id = 129, Code = "PartnerDelivery_Import", Name = "Import", NumberOrder = 5, ParentId = 124 },
                new Privileges
                    { Id = 130, Code = "PartnerDelivery_Export", Name = "Xuất file", NumberOrder = 6, ParentId = 124 },

                //ID = 131
                new Privileges
                {
                    Id = 131, Code = "PurchasePayment", Name = "Thanh toán NCC, ĐTGH", NumberOrder = 7, ParentId = 4
                },
                new Privileges
                    { Id = 132, Code = "PurchasePayment_Read", Name = "Xem DS", NumberOrder = 1, ParentId = 131 },
                new Privileges
                    { Id = 133, Code = "PurchasePayment_Create", Name = "Thêm mới", NumberOrder = 2, ParentId = 131 },
                new Privileges
                    { Id = 134, Code = "PurchasePayment_Update", Name = "Cập nhập", NumberOrder = 3, ParentId = 131 },

                //ID = 135
                new Privileges { Id = 135, Code = "Payment", Name = "Thanh toán", NumberOrder = 3, ParentId = 4 },
                new Privileges { Id = 136, Code = "Payment_Read", Name = "Xem DS", NumberOrder = 1, ParentId = 135 },
                new Privileges
                    { Id = 137, Code = "Payment_Create", Name = "Thêm mới", NumberOrder = 2, ParentId = 135 },
                new Privileges
                    { Id = 138, Code = "Payment_Update", Name = "Cập nhập", NumberOrder = 3, ParentId = 135 },
                //ID = 139
                new Privileges
                    { Id = 139, Code = "FinancialReport", Name = "Tài chính", NumberOrder = 9, ParentId = 5 },
                new Privileges
                {
                    Id = 140, Code = "FinancialReport_SalePerformanceReport", Name = "KQ HĐ Kinh Doanh",
                    NumberOrder = 1, ParentId = 139
                },

                //ID = 141
                new Privileges { Id = 141, Code = "UserReport", Name = "Nhân viên", NumberOrder = 7, ParentId = 5 },
                new Privileges
                {
                    Id = 142, Code = "UserReport_ByProfitReport", Name = "Lợi nhuận", NumberOrder = 1, ParentId = 141
                },
                new Privileges
                {
                    Id = 143, Code = "UserReport_ByUserReport", Name = "Hàng bán theo NV", NumberOrder = 2,
                    ParentId = 141
                },
                new Privileges
                    { Id = 144, Code = "UserReport_BySaleReport", Name = "Bán hàng", NumberOrder = 3, ParentId = 141 },

                //ID = 145
                new Privileges { Id = 145, Code = "SaleReport", Name = "Bán hàng", NumberOrder = 2, ParentId = 5 },
                new Privileges
                    { Id = 146, Code = "SaleReport_SaleByUser", Name = "Nhân viên", NumberOrder = 1, ParentId = 145 },
                new Privileges
                {
                    Id = 147, Code = "SaleReport_SaleProfitByInvoice", Name = "Lợi nhuận", NumberOrder = 2,
                    ParentId = 145
                },
                new Privileges
                {
                    Id = 148, Code = "SaleReport_SaleDiscountByInvoice", Name = "Giảm giá hóa đơn", NumberOrder = 3,
                    ParentId = 145
                },
                new Privileges
                    { Id = 149, Code = "SaleReport_SaleByRefund", Name = "Trả hàng", NumberOrder = 4, ParentId = 145 },
                new Privileges
                    { Id = 150, Code = "SaleReport_SaleByTime", Name = "Thời gian", NumberOrder = 5, ParentId = 145 },
                new Privileges
                {
                    Id = 151, Code = "SaleReport_BranchSaleReport", Name = "Chi nhánh", NumberOrder = 6, ParentId = 145
                },


                //ID = 152
                new Privileges { Id = 152, Code = "OrderReport", Name = "Đặt hàng", NumberOrder = 3, ParentId = 5 },
                new Privileges
                    { Id = 153, Code = "OrderReport_ByDocReport", Name = "Giao dịch", NumberOrder = 1, ParentId = 152 },
                new Privileges
                {
                    Id = 154, Code = "OrderReport_ByProductReport", Name = "Hàng hóa", NumberOrder = 2, ParentId = 152
                },

                //ID = 155
                new Privileges { Id = 155, Code = "EndOfDayReport", Name = "Cuối ngày", NumberOrder = 1, ParentId = 5 },
                new Privileges
                {
                    Id = 156, Code = "EndOfDayReport_EndOfDaySynthetic", Name = "Tổng hợp", NumberOrder = 1,
                    ParentId = 155
                },
                new Privileges
                {
                    Id = 157, Code = "EndOfDayReport_EndOfDayProduct", Name = "Hàng hóa", NumberOrder = 2,
                    ParentId = 155
                },
                new Privileges
                {
                    Id = 158, Code = "EndOfDayReport_EndOfDayCashFlow", Name = "Thu chi", NumberOrder = 3,
                    ParentId = 155
                },
                new Privileges
                {
                    Id = 159, Code = "EndOfDayReport_EndOfDayDocument", Name = "Bán hàng", NumberOrder = 4,
                    ParentId = 155
                },

                //ID = 160
                new Privileges
                    { Id = 160, Code = "SupplierReport", Name = "Nhà cung cấp", NumberOrder = 6, ParentId = 5 },
                new Privileges
                {
                    Id = 161, Code = "SupplierReport_BigByLiabilitiesReport", Name = "Công nợ", NumberOrder = 1,
                    ParentId = 160
                },
                new Privileges
                {
                    Id = 162, Code = "SupplierReport_SupplierInforReport", Name = "Hàng nhập theo NCC", NumberOrder = 2,
                    ParentId = 160
                },
                new Privileges
                {
                    Id = 163, Code = "SupplierReport_PurchaseOrderReport", Name = "Nhập hàng", NumberOrder = 3,
                    ParentId = 160
                },

                //ID = 164
                new Privileges
                    { Id = 164, Code = "SaleChannelReport", Name = "Kênh bán hàng", NumberOrder = 8, ParentId = 5 },
                new Privileges
                {
                    Id = 165, Code = "SaleChannelReport_ByProduct", Name = "Hàng bán theo kênh", NumberOrder = 1,
                    ParentId = 164
                },
                new Privileges
                {
                    Id = 166, Code = "SaleChannelReport_ByProfit", Name = "Lợi nhuận", NumberOrder = 2, ParentId = 164
                },
                new Privileges
                    { Id = 167, Code = "SaleChannelReport_BySale", Name = "Bán hàng", NumberOrder = 3, ParentId = 164 },


                //ID = 168
                new Privileges { Id = 168, Code = "ProductReport", Name = "Hàng hóa", NumberOrder = 4, ParentId = 5 },
                new Privileges
                {
                    Id = 169, Code = "ProductReport_ProducStockInOutStock", Name = "Giá trị kho", NumberOrder = 1,
                    ParentId = 168
                },
                new Privileges
                {
                    Id = 170, Code = "ProductReport_ProductByBatchExpire", Name = "Hạn sử dụng", NumberOrder = 2,
                    ParentId = 168
                },
                new Privileges
                {
                    Id = 171, Code = "ProductReport_ProductByCustomer", Name = "Khách theo bán hàng", NumberOrder = 3,
                    ParentId = 168
                },
                new Privileges
                {
                    Id = 172, Code = "ProductReport_ProductByDamageItem", Name = "Xuất hủy", NumberOrder = 4,
                    ParentId = 168
                },
                new Privileges
                {
                    Id = 173, Code = "ProductReport_ProductBySupplier", Name = "NCC theo hàng nhập", NumberOrder = 5,
                    ParentId = 168
                },
                new Privileges
                {
                    Id = 174, Code = "ProductReport_ProductByUser", Name = "Nhân viên theo hàng bán", NumberOrder = 6,
                    ParentId = 168
                },
                new Privileges
                {
                    Id = 175, Code = "ProductReport_ProducInOutStock", Name = "Xuất nhập tồn", NumberOrder = 7,
                    ParentId = 168
                },
                new Privileges
                {
                    Id = 176, Code = "ProductReport_ProducInOutStockDetail", Name = "Xuất nhập tồn chi tiết",
                    NumberOrder = 8, ParentId = 168
                },
                new Privileges
                {
                    Id = 177, Code = "ProductReport_ProductByProfit", Name = "Lợi nhuận", NumberOrder = 9,
                    ParentId = 168
                },
                new Privileges
                {
                    Id = 178, Code = "ProductReport_ProductBySale", Name = "Bán hàng", NumberOrder = 10, ParentId = 168
                },


                //ID = 179
                new Privileges
                    { Id = 179, Code = "CustomerReport", Name = "Khách hàng", NumberOrder = 5, ParentId = 5 },
                new Privileges
                {
                    Id = 180, Code = "CustomerReport_BigCustomerDebt", Name = "Công nợ", NumberOrder = 1, ParentId = 179
                },
                new Privileges
                {
                    Id = 181, Code = "CustomerReport_CustomerProduct", Name = "Hàng bán theo khách", NumberOrder = 2,
                    ParentId = 179
                },
                new Privileges
                {
                    Id = 182, Code = "CustomerReport_CustomerSale", Name = "Bán hàng", NumberOrder = 3, ParentId = 179
                },
                new Privileges
                {
                    Id = 183, Code = "CustomerReport_CustomerProfit", Name = "Lợi nhuận", NumberOrder = 4,
                    ParentId = 179
                },


                //ID = 179
                new Privileges
                    { Id = 184, Code = "BankAccount", Name = "Tài khoản ngân hàng", NumberOrder = 2, ParentId = 6 },
                new Privileges
                    { Id = 185, Code = "BankAccount_Create", Name = "Thêm mới", NumberOrder = 1, ParentId = 184 },
                new Privileges
                    { Id = 186, Code = "BankAccount_Update", Name = "Cập nhập", NumberOrder = 2, ParentId = 184 },
                new Privileges { Id = 187, Code = "BankAccount_Delete", Name = "Xóa", NumberOrder = 3, ParentId = 184 },


                //ID = 188
                new Privileges { Id = 188, Code = "CashFlow", Name = "Sổ quỹ", NumberOrder = 1, ParentId = 6 },
                new Privileges { Id = 189, Code = "CashFlow_Read", Name = "Xem DS", NumberOrder = 1, ParentId = 188 },
                new Privileges
                    { Id = 190, Code = "CashFlow_Create", Name = "Thêm mới", NumberOrder = 2, ParentId = 188 },
                new Privileges
                    { Id = 191, Code = "CashFlow_Update", Name = "Cập nhập", NumberOrder = 3, ParentId = 188 },
                new Privileges { Id = 192, Code = "CashFlow_Delete", Name = "Xóa", NumberOrder = 4, ParentId = 188 },
                new Privileges
                    { Id = 193, Code = "CashFlow_Export", Name = "Xuất file", NumberOrder = 4, ParentId = 188 },


                //ID = 194
                new Privileges
                    { Id = 194, Code = "Campaign", Name = "Chương trình khuyến mại", NumberOrder = 1, ParentId = 7 },
                new Privileges { Id = 195, Code = "Campaign_Read", Name = "Xem DS", NumberOrder = 1, ParentId = 194 },
                new Privileges
                    { Id = 196, Code = "Campaign_Create", Name = "Thêm mới", NumberOrder = 2, ParentId = 194 },
                new Privileges
                    { Id = 197, Code = "Campaign_Update", Name = "Cập nhập", NumberOrder = 3, ParentId = 194 },
                new Privileges { Id = 198, Code = "Campaign_Delete", Name = "Xóa", NumberOrder = 4, ParentId = 194 },

                //ID = 199
                new Privileges
                    { Id = 199, Code = "VoucherCampaign", Name = "Quản lý voucher", NumberOrder = 1, ParentId = 8 },
                new Privileges
                    { Id = 200, Code = "VoucherCampaign_Read", Name = "Xem DS", NumberOrder = 1, ParentId = 199 },
                new Privileges
                    { Id = 201, Code = "VoucherCampaign_Create", Name = "Thêm mới", NumberOrder = 2, ParentId = 199 },
                new Privileges
                    { Id = 202, Code = "VoucherCampaign_Update", Name = "Cập nhập", NumberOrder = 3, ParentId = 199 },
                new Privileges
                    { Id = 203, Code = "VoucherCampaign_Delete", Name = "Xóa", NumberOrder = 4, ParentId = 199 },
                new Privileges
                    { Id = 204, Code = "VoucherCampaign_Release", Name = "Phát hành", NumberOrder = 4, ParentId = 199 },

                //ID = 205
                new Privileges
                {
                    Id = 205, Code = "EmployeeLimit",
                    Name = "Không cho phép xem chấm công, tính lương của nhân viên khác", NumberOrder = 1, ParentId = 9
                },
                new Privileges
                    { Id = 206, Code = "EmployeeLimit_Read", Name = "Xem DS", NumberOrder = 1, ParentId = 205 },

                //ID = 207
                new Privileges
                    { Id = 207, Code = "CouponCampaign", Name = "Quản lý coupon", NumberOrder = 1, ParentId = 10 },
                new Privileges
                    { Id = 208, Code = "CouponCampaign_Read", Name = "Xem DS", NumberOrder = 1, ParentId = 207 },
                new Privileges
                    { Id = 209, Code = "CouponCampaign_Create", Name = "Thêm mới", NumberOrder = 2, ParentId = 207 },
                new Privileges
                    { Id = 210, Code = "CouponCampaign_Update", Name = "Cập nhập", NumberOrder = 3, ParentId = 207 },
                new Privileges
                    { Id = 211, Code = "CouponCampaign_Delete", Name = "Xóa", NumberOrder = 4, ParentId = 207 },


                //ID = 212
                new Privileges
                    { Id = 212, Code = "CustomerAdjustment", Name = "Công nợ KH", NumberOrder = 2, ParentId = 4 },
                new Privileges
                    { Id = 213, Code = "CustomerAdjustment_Read", Name = "Xem DS", NumberOrder = 1, ParentId = 212 },
                new Privileges
                {
                    Id = 214, Code = "CustomerAdjustment_Create", Name = "Thêm mới", NumberOrder = 2, ParentId = 212
                },
                new Privileges
                {
                    Id = 215, Code = "CustomerAdjustment_Update", Name = "Cập nhập", NumberOrder = 3, ParentId = 212
                },
                new Privileges
                    { Id = 216, Code = "CustomerAdjustment_Delete", Name = "Xóa", NumberOrder = 4, ParentId = 212 },
            };
            return listPrivileges;
        }
    }
}