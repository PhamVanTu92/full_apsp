using BackEndAPI.Models.Account;
using BackEndAPI.Models.BusinessPartners;
using BackEndAPI.Models.ItemMasterData;
using BackEndAPI.Models.Other;
using BackEndAPI.Service.Account;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;

namespace BackEndAPI.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider, UserManager<AppUser> userManager, RoleManager<AppRole> roleManager, AppDbContext _context)
        {
            //string[] roleNames = { "Admin"};
            //IdentityResult roleResult;

            //foreach (var roleName in roleNames)
            //{
            //    var role = await roleManager.FindByNameAsync(roleName);
            //    if (role == null)
            //    {
            //        var appRole = new AppRole();
            //        appRole.Name = roleName;
            //        roleResult = await roleManager.CreateAsync(appRole);
            //    }
            //    else
            //    {
            //        var listClaim = ClaimList.GetAppRoleClaims(role.Id);
            //        foreach (var claim in listClaim)
            //        {
            //            var claimExists = await roleManager.GetClaimsAsync(role);
            //            var alreadyExist = claimExists.Where(c => c.Type == claim.ClaimType && c.Value == claim.ClaimValue);
            //            if (alreadyExist.Count() == 0)
            //            {
            //                _context.Set<AppRoleClaim>().Add(claim);
            //            }
            //        }
            //        await _context.SaveChangesAsync();
            //    }
            //}

            var defaultUser = new AppUser
            {
                UserName = "Admin",
                FullName = "Admin",
                Email = "Admin@gmail.com",
            };

            string userPassword = "Admin@123456";
            var user = await userManager.FindByNameAsync(defaultUser.UserName);
            if (user == null)
            {
                var createPowerUser = await userManager.CreateAsync(defaultUser, userPassword);
                //if (createPowerUser.Succeeded)
                //{
                //    await userManager.AddToRoleAsync(defaultUser, defaultUser.UserName);
                //}
            }
        }
        public static async Task AddPaymentRule( AppDbContext _context)
        {
            List<PaymentRule> lst = new List<PaymentRule>();
            lst.Add(new PaymentRule { Id = 1, Name = "HỘ KINH DOANH", PromotionTax = 0.5, BonusPaynow = 1.97, BonusVolumn = 98.5 });
            lst.Add(new PaymentRule { Id = 2, Name = "CTY, DOANH NGHIỆP", PromotionTax = 0, BonusPaynow = 2, BonusVolumn = 100 });

            var user =  _context.PaymentRule.FirstOrDefault(e=>e.Name == "HỘ KINH DOANH");
            if (user == null)
            {
                PaymentRule p = new PaymentRule {  Name = "HỘ KINH DOANH", PromotionTax = 0.5, BonusPaynow = 1.97, BonusVolumn = 98.5 };
                var createnew = _context.PaymentRule.Add(p);
                await _context.SaveChangesAsync();
            }
            var user1 =  _context.PaymentRule.FirstOrDefault(e => e.Name == "CTY, DOANH NGHIỆP");
            if (user1 == null)
            {
                PaymentRule p = new PaymentRule { Name = "CTY, DOANH NGHIỆP", PromotionTax = 0, BonusPaynow = 2, BonusVolumn = 100 };
                var createnew = _context.PaymentRule.Add(p);
                await _context.SaveChangesAsync();
            }
        }


        public static async Task AppSetting(AppDbContext _context)
        {
            List<AppSetting> lst = new List<AppSetting>();
            lst.Add(new AppSetting { Id = 1, UserType = "APSP", Is2FARequired = true, TwoFactorType = "Email" });
            lst.Add(new AppSetting { Id = 2, UserType = "Nhà phân phối", Is2FARequired = true, TwoFactorType = "Email" });

            var user = _context.AppSetting.FirstOrDefault(e => e.UserType == "APSP");
            if (user == null)
            {
                AppSetting app = new AppSetting {UserType = "APSP", Is2FARequired = true, TwoFactorType = "Email" };
                var createnew = _context.AppSetting.Add(app);
                await _context.SaveChangesAsync();
            }
            var user1 = _context.AppSetting.FirstOrDefault(e => e.UserType == "Nhà phân phối");
            if (user1 == null)
            {
                AppSetting app = new AppSetting { UserType = "Nhà phân phối", Is2FARequired = true, TwoFactorType = "Email" };
                var createnew = _context.AppSetting.Add(app);
                await _context.SaveChangesAsync();
            }
        }

        public static async Task SeedPrivilegeData( AppDbContext _context)
        {
 

             var privileges = new List<Privileges>
            {
                new() { Id =219, Code = "Order", Name = "Đơn hàng", NumberOrder = 1, ParentId = 341},
                new() { Id =220, Code = "Order.View", Name = "Xem đơn hàng", NumberOrder = 1, ParentId = 219},
                new() { Id =221, Code = "Order.Create", Name = "Tạo mới đơn hàng", NumberOrder = 2, ParentId = 219},
                new() { Id =222, Code = "Order.Edit", Name = "Chỉnh sửa đơn hàng", NumberOrder = 3, ParentId = 219},
                new() { Id =223, Code = "Order.Cancel", Name = "Hủy đơn hàng", NumberOrder = 4, ParentId = 219},
                new() { Id =224, Code = "Order.UpdateNote", Name = "Cập nhật ghi chú", NumberOrder = 5, ParentId = 219},
                new() { Id =225, Code = "Order.RequestAdditionalDocuments", Name = "Yêu cầu bổ sung chứng từ", NumberOrder = 6, ParentId = 219},
                new() { Id =226, Code = "Order.UploadDocuments", Name = "Upload tài liệu đơn hàng", NumberOrder = 7, ParentId = 219},
                new() { Id =227, Code = "Order.SendForApproval", Name = "Gửi phê duyệt đơn hàng", NumberOrder = 8, ParentId = 219},
                new() { Id =228, Code = "Order.Confirm", Name = "Xác nhận đơn hàng", NumberOrder = 9, ParentId = 219},
                new() { Id =229, Code = "Order.Deliver", Name = "Giao hàng", NumberOrder = 10, ParentId = 219},
                new() { Id =230, Code = "Order.Complete", Name = "Hoàn tất đơn hàng", NumberOrder = 11, ParentId = 219},
                new() { Id =243, Code = "PurchaseRequest.View", Name = "Xem danh sách yêu cầu lấy hàng", NumberOrder = 1, ParentId = 254},
                new() { Id =244, Code = "PurchaseRequest.Create", Name = "Tạo mới yêu cầu lấy hàng", NumberOrder = 2, ParentId = 254},
                new() { Id =245, Code = "PurchaseRequest.Edit", Name = "Sửa yêu cầu lấy hàng", NumberOrder = 3, ParentId = 254},
                new() { Id =246, Code = "PurchaseRequest.Cancel", Name = "Hủy yêu cầu lấy hàng", NumberOrder = 4, ParentId = 254},
                new() { Id =247, Code = "PurchaseRequest.UpdateNote", Name = "Cập nhật ghi chú", NumberOrder = 5, ParentId = 254},
                new() { Id =248, Code = "PurchaseRequest.RequestAdditionalDocuments", Name = "Yêu cầu bổ sung chứng từ", NumberOrder = 6, ParentId = 254},
                new() { Id =249, Code = "PurchaseRequest.UploadDocuments", Name = "Upload tài liệu yêu cầu lấy hàng", NumberOrder = 7, ParentId = 254},
                new() { Id =250, Code = "PurchaseRequest.SendForApproval", Name = "Gửi phê duyệt yêu cầu lấy hàng", NumberOrder = 8, ParentId = 254},
                new() { Id =251, Code = "PurchaseRequest.Confirm", Name = "Xác nhận yêu cầu lấy hàng", NumberOrder = 9, ParentId = 254},
                new() { Id =252, Code = "PurchaseRequest.Deliver", Name = "Giao hàng", NumberOrder = 10, ParentId = 254},
                new() { Id =253, Code = "PurchaseRequest.Complete", Name = "Hoàn tất đơn hàng", NumberOrder = 11, ParentId = 254},
                new() { Id =254, Code = "PurchaseRequest", Name = "Yêu cầu lấy hàng gửi", NumberOrder = 2, ParentId = 341},
                new() { Id =261, Code = "Customer", Name = "Khách hàng", NumberOrder = 1, ParentId = 342},
                new() { Id =262, Code = "Customer.View", Name = "Xem thông tin khách hàng", NumberOrder = 2, ParentId = 261},
                new() { Id =263, Code = "Customer.Create", Name = "Tạo mới khách hàng", NumberOrder = 3, ParentId = 261},
                new() { Id =264, Code = "Customer.Edit", Name = "Sửa thông tin khách hàng", NumberOrder = 4, ParentId = 261},
                new() { Id =265, Code = "Customer.Delete", Name = "Xóa khách hàng", NumberOrder = 5, ParentId = 261},
                new() { Id =267, Code = "CustomerGroup.View", Name = "Xem nhóm khách hàng", NumberOrder = 1, ParentId = 344},
                new() { Id =268, Code = "CustomerGroup.Create", Name = "Tạo mới nhóm khách hàng", NumberOrder = 2, ParentId = 344},
                new() { Id =269, Code = "CustomerGroup.Edit", Name = "Chỉnh sửa nhóm khách hàng", NumberOrder = 3, ParentId = 344},
                new() { Id =270, Code = "CustomerGroup.Delete", Name = "Xóa nhóm khách hàng", NumberOrder = 4, ParentId = 344},
                new() { Id =271, Code = "SaleForecast.View", Name = "Xem danh sách kế hoạch nhập hàng", NumberOrder = 1, ParentId = 277},
                new() { Id =272, Code = "SaleForecast.ViewDetails", Name = "Xem chi tiết kế hoạch nhập hàng", NumberOrder = 2, ParentId = 277},
                new() { Id =273, Code = "SaleForecast.Create", Name = "Tạo mới kế hoạch nhập hàng", NumberOrder = 3, ParentId = 277},
                new() { Id =274, Code = "SaleForecast.Edit", Name = "Sửa thông tin kế hoạch nhập hàng", NumberOrder = 4, ParentId = 277},
                new() { Id =275, Code = "SaleForecast.Delete", Name = "Xóa kế hoạch nhập hàng", NumberOrder = 5, ParentId = 277},
                new() { Id =276, Code = "SaleForecast.Confirm", Name = "Xác nhận kế hoạch nhập hàng", NumberOrder = 6, ParentId = 277},
                new() { Id =277, Code = "SaleForecast", Name = "Kế hoạch nhập hàng", NumberOrder = 3, ParentId = 342},
                new() { Id =278, Code = "Committed.View", Name = "Xem danh sách cam kết sản lượng", NumberOrder = 1, ParentId = 288},
                new() { Id =279, Code = "Committed.ViewDetails", Name = "Xem chi tiết cam kết sản lượng", NumberOrder = 2, ParentId = 288},
                new() { Id =280, Code = "Committed.Create", Name = "Tạo mới cam kết sản lượng", NumberOrder = 3, ParentId = 288},
                new() { Id =281, Code = "Committed.Edit", Name = "Sửa thông tin cam kết sản lượng", NumberOrder = 4, ParentId = 288},
                new() { Id =282, Code = "Committed.Delete", Name = "Xóa cam kết sản lượng", NumberOrder = 5, ParentId = 288},
                new() { Id =283, Code = "Committed.Confirm", Name = "Xác nhận cam kết sản lượng", NumberOrder = 6, ParentId = 288},
                new() { Id =284, Code = "Committed.Reject", Name = "Từ chối cam kết sản lượng", NumberOrder = 7, ParentId = 288},
                new() { Id =285, Code = "Committed.Cancel", Name = "Hủy cam kết sản lượng", NumberOrder = 8, ParentId = 288},
                new() { Id =286, Code = "Committed.Send", Name = "Gửi cam kết sản lượng", NumberOrder = 9, ParentId = 288},
                new() { Id =287, Code = "Committed.ImportFile", Name = "Import file cam kết sản lượng", NumberOrder = 10, ParentId = 288},
                new() { Id =288, Code = "Committed", Name = "Cam kết sản lượng", NumberOrder = 4, ParentId = 342},
                new() { Id =289, Code = "FeePeriod", Name = "Biên bản tính phí lưu kho", NumberOrder = 5, ParentId = 342},
                new() { Id =290, Code = "FeePeriod.ExportFile", Name = "Xuất file biên bản tính phí lưu kho", NumberOrder = 1, ParentId = 289},
                new() { Id =291, Code = "FeePeriod.Cancel", Name = "Hủy biên bản tính phí lưu kho", NumberOrder = 2, ParentId = 289},
                new() { Id =292, Code = "FeePeriod.Reject", Name = "Từ chối biên bản tính phí lưu kho", NumberOrder = 3, ParentId = 289},
                new() { Id =293, Code = "FeePeriod.Confirm", Name = "Xác nhận biên bản tính phí lưu kho", NumberOrder = 4, ParentId = 289},
                new() { Id =294, Code = "FeePeriod.Delete", Name = "Xóa biên bản tính phí lưu kho", NumberOrder = 5, ParentId = 289},
                new() { Id =295, Code = "FeePeriod.Edit", Name = "Sửa biên bản tính phí lưu kho", NumberOrder = 6, ParentId = 289},
                new() { Id =296, Code = "FeePeriod.SendToCustomer", Name = "Gửi biên bản cho khách hàng", NumberOrder = 7, ParentId = 289},
                new() { Id =297, Code = "FeePeriod.Calculate", Name = "Tính phí lưu kho", NumberOrder = 8, ParentId = 289},
                new() { Id =298, Code = "FeePeriod.ViewDetails", Name = "Xem chi tiết biên bản tính phí lưu kho", NumberOrder = 9, ParentId = 289},
                new() { Id =299, Code = "FeePeriod.ViewList", Name = "Xem danh sách biên bản tính phí lưu kho", NumberOrder = 10, ParentId = 289},
                new() { Id =300, Code = "Fee.ImportFile ", Name = "Xem danh sách bảng giá tính phí lưu kho", NumberOrder = 1, ParentId = 345},
                new() { Id =301, Code = "Fee.Setup ", Name = "Xem chi tiết bảng giá tính phí lưu kho", NumberOrder = 2, ParentId = 345},
                new() { Id =302, Code = "Fee.Create ", Name = "Thêm mới bảng giá tính phí lưu kho", NumberOrder = 3, ParentId = 345},
                new() { Id =303, Code = "Fee.Edit ", Name = "Sửa bảng giá tính phí lưu kho", NumberOrder = 4, ParentId = 345},
                new() { Id =304, Code = "Fee.ViewDetails ", Name = "Thiết lập bảng giá tính phí lưu kho", NumberOrder = 5, ParentId = 345},
                new() { Id =305, Code = "Fee.ViewList ", Name = "Import file bảng giá tính phí lưu kho", NumberOrder = 6, ParentId = 345},
                new() { Id =307, Code = "Item.ImportFile", Name = "Xem danh sách sản phẩm", NumberOrder = 1, ParentId = 312},
                new() { Id =308, Code = "Item.Edit", Name = "Cập nhật sản phẩm", NumberOrder = 2, ParentId = 312},
                new() { Id =309, Code = "Item.Sync", Name = "Đồng bộ sản phẩm", NumberOrder = 3, ParentId = 312},
                new() { Id =310, Code = "Item.View", Name = "Xem danh sách sản phẩm", NumberOrder = 4, ParentId = 312},
                new() { Id =311, Code = "Item.ViewList", Name = "Xem danh sách sản phẩm", NumberOrder = 5, ParentId = 312},
                new() { Id =312, Code = "Item", Name = "Sản phẩm", NumberOrder = 1, ParentId = 346},
                new() { Id =313, Code = "ItemType", Name = "Loại sản phẩm", NumberOrder = 2, ParentId = 346},
                new() { Id =314, Code = "ItemType.View", Name = "Xem loại sản phẩm", NumberOrder = 1, ParentId = 313},
                new() { Id =315, Code = "ItemType.Sync", Name = "Đồng bộ loại sản phẩm", NumberOrder = 2, ParentId = 313},
                new() { Id =316, Code = "Packing", Name = "Quy cách bao bì", NumberOrder = 3, ParentId = 346},
                new() { Id =317, Code = "Packing.View", Name = "Xem danh sách quy cách bao bì", NumberOrder = 1, ParentId = 316},
                new() { Id =318, Code = "Packing.Sync", Name = "Đồng bộ quy cách bao bì", NumberOrder = 2, ParentId = 316},
                new() { Id =319, Code = "Packing.Edit", Name = "Sửa quy cách bao bì", NumberOrder = 3, ParentId = 316},
                new() { Id =320, Code = "UnitGroup", Name = "Nhóm đơn vị tính", NumberOrder = 4, ParentId = 346},
                new() { Id =321, Code = "UnitGroup.View", Name = "Xem nhóm đơn vị tính", NumberOrder = 1, ParentId = 320},
                new() { Id =322, Code = "UnitGroup.Sync", Name = "Đồng bộ đơn vị tính", NumberOrder = 2, ParentId = 320},
                new() { Id =323, Code = "UnitGroup.Edit", Name = "Sủa nhóm đơn vị tính", NumberOrder = 3, ParentId = 320},
                new() { Id =324, Code = "UnitGroup.Delete", Name = "Xóa nhóm đơn vị tính", NumberOrder = 4, ParentId = 320},
                new() { Id =325, Code = "PromotionProgram", Name = "Chương trình khuyến mại", NumberOrder = 1, ParentId = 347},
                new() { Id =326, Code = "PromotionProgram.ImportFile", Name = "Thêm file", NumberOrder = 1, ParentId = 325},
                new() { Id =327, Code = "PromotionProgram.Delete", Name = "Xóa chương trình khuyến mại", NumberOrder = 2, ParentId = 325},
                new() { Id =328, Code = "PromotionProgram.Edit", Name = "Sửa thông tin chương trình khuyến mại", NumberOrder = 3, ParentId = 325},
                new() { Id =329, Code = "PromotionProgram.Create", Name = "Thêm mới chương trình khuyến mại", NumberOrder = 4, ParentId = 325},
                new() { Id =330, Code = "PromotionProgram.View", Name = "Xem chương trình khuyến mại", NumberOrder = 5, ParentId = 325},
                new() { Id =332, Code = "Voucher", Name = "Voucher", NumberOrder = 2, ParentId = 347},
                new() { Id =333, Code = "Voucher.Delete", Name = "Xem danh sách voucher", NumberOrder = 1, ParentId = 332},
                new() { Id =334, Code = "Voucher.Edit", Name = "Xem chi tiết voucher", NumberOrder = 2, ParentId = 332},
                new() { Id =335, Code = "Voucher.Create", Name = "Thêm mới voucher", NumberOrder = 3, ParentId = 332},
                new() { Id =336, Code = "Voucher.ViewDetails", Name = "Sửa voucher", NumberOrder = 4, ParentId = 332},
                new() { Id =337, Code = "Voucher.ViewList", Name = "Xóa voucher", NumberOrder = 5, ParentId = 332},
                new() { Id =338, Code = "Approval", Name = "Phê duyệt", NumberOrder = 3, ParentId = 341},
                new() { Id =339, Code = "Approval.View", Name = "Xem danh sách phê duyệt", NumberOrder = 1, ParentId = 338},
                new() { Id =340, Code = "Approval.Approve", Name = "Phê duyệt chứng từ", NumberOrder = 2, ParentId = 338},
                new() { Id =341, Code = "TransactionManagement", Name = "Quản lý giao dịch", NumberOrder = 1, ParentId = null},
                new() { Id =342, Code = "CustomerManagement", Name = "Quản lý khách hàng", NumberOrder = 2, ParentId = null},
                new() { Id =344, Code = "CustomerGroup", Name = "Nhóm khách hàng", NumberOrder = 2, ParentId = 342},
                new() { Id =345, Code = "Fee", Name = "Bảng tính phí lưu kho", NumberOrder = 6, ParentId = 342},
                new() { Id =346, Code = "InventoryManagement", Name = "Quản lý hàng hóa", NumberOrder = 4, ParentId = null},
                new() { Id =347, Code = "Promotion", Name = "Khuyến mại", NumberOrder = 5, ParentId = null},
                new() { Id =348, Code = "UnitGroup.Create", Name = "Tạo mới nhóm đơn vị tính", NumberOrder = 5, ParentId = 320},
                new() { Id =349, Code = "Role", Name = "Quản lý vai trò", NumberOrder = 0, ParentId = null},
                new() { Id =350, Code = "Role.View", Name = "Xem vai trò", NumberOrder = 1, ParentId = 349},
                new() { Id =351, Code = "Role.Create", Name = "Tạo vai trò", NumberOrder = 2, ParentId = 349},
                new() { Id =352, Code = "Role.Edit", Name = "Cập nhật vai trò", NumberOrder = 3, ParentId = 349},
                new() { Id =353, Code = "Role.Delete", Name = "Xóa vai trò", NumberOrder = 4, ParentId = 349},
                new() { Id =354, Code = "User", Name = "Quản lý người dùng", NumberOrder = 0, ParentId = null},
                new() { Id =355, Code = "User.Create", Name = "Tạo người dùng", NumberOrder = 1, ParentId = 354},
                new() { Id =356, Code = "User.Edit", Name = "Chỉnh sửa người dùng", NumberOrder = 2, ParentId = 354},
                new() { Id =357, Code = "User.View", Name = "Xem người dùng", NumberOrder = 3, ParentId = 354},
                new() { Id =358, Code = "User.Delete", Name = "Xóa người dùng", NumberOrder = 4, ParentId = 354},
                new() { Id =359, Code = "Order.ChangeDiscount", Name = "Chỉnh sửa đơn hàng", NumberOrder = 0, ParentId = 219},
                new() { Id =361, Code = "Report", Name = "Báo cáo", NumberOrder = 0, ParentId = null},
                new() { Id =362, Code = "Report.InventoryReport", Name = "Báo cáo tồn kho hàng gửi", NumberOrder = 0, ParentId = 361},
                new() { Id =365, Code = "Report.DebtReport", Name = "Báo cáo công nợ khách hàng", NumberOrder = 0, ParentId = 361},
                new() { Id =366, Code = "Report.PurchaseItemReport", Name = "Báo cáo mua theo sản phẩm", NumberOrder = 0, ParentId = 361},
                new() { Id =367, Code = "Report.PurchaseOrderReport", Name = "Báo cáo mua theo đơn hàng", NumberOrder = 0, ParentId = 361},
                new() { Id =368, Code = "Report.DebtDetail", Name = "Sổ chi tiết công nợ theo đối tượng", NumberOrder = 0, ParentId = 361},
                new() { Id =369, Code = "Report.TrungBinhItem", Name = "Báo cáo giá trung bình sản phẩm", NumberOrder = 0, ParentId = 361},
                new() { Id =370, Code = "Report.PolicyPayNow", Name = "Báo cáo chính sách thưởng thanh toán ngay", NumberOrder = 0, ParentId = 361},
                new() { Id =371, Code = "Report.CommitedReport", Name = "Báo cáo cam kết sản lượng", NumberOrder = 0, ParentId = 361},
                new() { Id =376, Code = "User.ChangePassword", Name = "Đặt lại mật khẩu", NumberOrder = 0, ParentId = 354},
                new() { Id =377, Code = "User.ActiveDeActive", Name = "Kích hoạt/ Ngừng hoạt động tài khoản", NumberOrder = 0, ParentId = 354},
                new() { Id =379, Code = "ApprovalStage", Name = "Cấp phê duyệt", NumberOrder = 0, ParentId = null},
                new() { Id =380, Code = "ApprovalStage.View", Name = "Xem danh sách phê duyệt", NumberOrder = 0, ParentId = 379},
                new() { Id =381, Code = "ApprovalStage.Create", Name = "Thêm mới cấp phê duyệt", NumberOrder = 0, ParentId = 379},
                new() { Id =382, Code = "ApprovalStage.Update", Name = "Chỉnh sửa cấp phê duyệt", NumberOrder = 0, ParentId = 379},
                new() { Id =383, Code = "ApprovalStage.Delete", Name = "Xóa cấp phê duyệt", NumberOrder = 0, ParentId = 379},
                new() { Id =384, Code = "ApprovalTemplate", Name = "Mẫu phê duyệt", NumberOrder = 0, ParentId = null},
                new() { Id =385, Code = "ApprovalTemplate.View", Name = "Xem mẫu phê duyệt", NumberOrder = 0, ParentId = 384},
                new() { Id =386, Code = "ApprovalTemplate.Create", Name = "Thêm mới cấp phê duyệt", NumberOrder = 0, ParentId = 384},
                new() { Id =387, Code = "ApprovalTemplate.Update", Name = "Chỉnh sửa cấp phê duyệt", NumberOrder = 0, ParentId = 384},
                new() { Id =388, Code = "ApprovalTemplate.Delete", Name = "Xóa cấp phê duyệt", NumberOrder = 0, ParentId = 384},
                new() { Id =389, Code = "PortalUser", Name = "Quản lý tài khoản khách hàng", NumberOrder = 0, ParentId = null},
                new() { Id =390, Code = "PortalUser.View", Name = "Xem danh sách tài khoản khách hàng", NumberOrder = 0, ParentId = 389},
                new() { Id =391, Code = "PortalUser.Create", Name = "Tạo tài khoản khách hàng", NumberOrder = 0, ParentId = 389},
                new() { Id =392, Code = "PortalUser.Update", Name = "Chỉnh sửa tài khoản", NumberOrder = 0, ParentId = 389},
                new() { Id =393, Code = "PortalUser.ChangePassword", Name = "Đặt lại mật khẩu", NumberOrder = 0, ParentId = 389},
                new() { Id =394, Code = "Test", Name = "Yêu cầu cấp mẫu thử nghiệm", NumberOrder = 0, ParentId = null},
                new() { Id =395, Code = "Test.View", Name = "Xem danh sách yêu cầu cấp mẫu thử nghiệm", NumberOrder = 0, ParentId = 395},
                new() { Id =396, Code = "Test.Create", Name = "Tạo mới yêu cầu cấp mẫu thử nghiệm", NumberOrder = 0, ParentId = 395},
                new() { Id =397, Code = "Test.SendApproval", Name = "Gửi phê duyệt", NumberOrder = 0, ParentId = 395},
                new() { Id =398, Code = "Test.Update", Name = "Cập nhật kết quả thử nghiệm", NumberOrder = 0, ParentId = 395},
                new() { Id =399, Code = "Author", Name = "Thiết lập bảo mật", NumberOrder = 0, ParentId = null},
                new() { Id =400, Code = "Payment", Name = "Thiết lập thanh toán", NumberOrder = 0, ParentId = null},
                new() { Id =401, Code = "ItemGroup", Name = "Nhóm sản phẩm", NumberOrder = 0, ParentId = null},
                new() { Id =402, Code = "ItemGroup.View", Name = "Xem danh sách nhóm sản phẩm", NumberOrder = 0, ParentId = 401},
                new() { Id =403, Code = "ItemGroup.Create", Name = "Thêm mới nhóm sản phẩm", NumberOrder = 0, ParentId = 401},
                new() { Id =404, Code = "ItemGroup.Update", Name = "Chỉnh sửa nhóm sản phẩm", NumberOrder = 0, ParentId = 401},


                 new() { Id =405, Code = "OrderNet", Name = "Đơn hàng giá NET", NumberOrder = 1, ParentId = 341},
                new() { Id =406, Code = "OrderNet.View", Name = "Xem đơn hàng", NumberOrder = 1, ParentId = 405},
                new() { Id =407, Code = "OrderNet.Create", Name = "Tạo mới đơn hàng", NumberOrder = 2, ParentId = 405},
                new() { Id =408, Code = "OrderNet.Edit", Name = "Chỉnh sửa đơn hàng", NumberOrder = 3, ParentId = 405},
                new() { Id =409, Code = "OrderNet.Cancel", Name = "Hủy đơn hàng", NumberOrder = 4, ParentId = 405},
                new() { Id =410, Code = "OrderNet.UpdateNote", Name = "Cập nhật ghi chú", NumberOrder = 5, ParentId = 405},
                new() { Id =411, Code = "OrderNet.RequestAdditionalDocuments", Name = "Yêu cầu bổ sung chứng từ", NumberOrder = 6, ParentId = 405},
                new() { Id =412, Code = "OrderNet.UploadDocuments", Name = "Upload tài liệu đơn hàng", NumberOrder = 7, ParentId = 405},
                new() { Id =413, Code = "OrderNet.SendForApproval", Name = "Gửi phê duyệt đơn hàng", NumberOrder = 8, ParentId = 405},
                new() { Id =414, Code = "OrderNet.Confirm", Name = "Xác nhận đơn hàng", NumberOrder = 9, ParentId = 405},
                new() { Id =415, Code = "OrderNet.Deliver", Name = "Giao hàng", NumberOrder = 10, ParentId = 405},
                new() { Id =416, Code = "OrderNet.Complete", Name = "Hoàn tất đơn hàng", NumberOrder = 11, ParentId = 405},

                new() { Id =417, Code = "OrderVPKM", Name = "Đơn hàng giá VPKM", NumberOrder = 1, ParentId = 341},
                new() { Id =418, Code = "OrderVPKM.View", Name = "Xem đơn hàng", NumberOrder = 1, ParentId = 417},
                new() { Id =419, Code = "OrderVPKM.Create", Name = "Tạo mới đơn hàng", NumberOrder = 2, ParentId = 417},
                new() { Id =420, Code = "OrderVPKM.Edit", Name = "Chỉnh sửa đơn hàng", NumberOrder = 3, ParentId = 417},
                new() { Id =421, Code = "OrderVPKM.Cancel", Name = "Hủy đơn hàng", NumberOrder = 4, ParentId = 417},
                new() { Id =422, Code = "OrderVPKM.UpdateNote", Name = "Cập nhật ghi chú", NumberOrder = 5, ParentId = 417},
                new() { Id =423, Code = "OrderVPKM.RequestAdditionalDocuments", Name = "Yêu cầu bổ sung chứng từ", NumberOrder = 6, ParentId = 417},
                new() { Id =424, Code = "OrderVPKM.UploadDocuments", Name = "Upload tài liệu đơn hàng", NumberOrder = 7, ParentId = 417},
                new() { Id =425, Code = "OrderVPKM.SendForApproval", Name = "Gửi phê duyệt đơn hàng", NumberOrder = 8, ParentId = 417},
                new() { Id =426, Code = "OrderVPKM.Confirm", Name = "Xác nhận đơn hàng", NumberOrder = 9, ParentId = 417},
                new() { Id =427, Code = "OrderVPKM.Deliver", Name = "Giao hàng", NumberOrder = 10, ParentId = 417},
                new() { Id =428, Code = "OrderVPKM.Complete", Name = "Hoàn tất đơn hàng", NumberOrder = 11, ParentId = 417},


                new() { Id =429, Code = "OrderExchange", Name = "Yêu cầu đổi VPKM", NumberOrder = 1, ParentId = 341},
                new() { Id =430, Code = "OrderExchange.View", Name = "Xem đơn hàng", NumberOrder = 1, ParentId = 429},
                new() { Id =431, Code = "OrderExchange.Create", Name = "Tạo mới đơn hàng", NumberOrder = 2, ParentId = 429},
                new() { Id =432, Code = "OrderExchange.Edit", Name = "Chỉnh sửa đơn hàng", NumberOrder = 3, ParentId = 429},
                new() { Id =433, Code = "OrderExchange.Cancel", Name = "Hủy đơn hàng", NumberOrder = 4, ParentId = 429},
                new() { Id =434, Code = "OrderExchange.UpdateNote", Name = "Cập nhật ghi chú", NumberOrder = 5, ParentId = 429},
                new() { Id =435, Code = "OrderExchange.RequestAdditionalDocuments", Name = "Yêu cầu bổ sung chứng từ", NumberOrder = 6, ParentId = 429},
                new() { Id =436, Code = "OrderExchange.UploadDocuments", Name = "Upload tài liệu đơn hàng", NumberOrder = 7, ParentId = 429},
                new() { Id =437, Code = "OrderExchange.SendForApproval", Name = "Gửi phê duyệt đơn hàng", NumberOrder = 8, ParentId = 429},
                new() { Id =438, Code = "OrderExchange.Confirm", Name = "Xác nhận đơn hàng", NumberOrder = 9, ParentId = 429},
                new() { Id =439, Code = "OrderExchange.Deliver", Name = "Giao hàng", NumberOrder = 10, ParentId = 429},
                new() { Id =440, Code = "OrderExchange.Complete", Name = "Hoàn tất đơn hàng", NumberOrder = 11, ParentId = 429},

                new() { Id =441, Code = "Pricelist", Name = "Danh sách sản phẩm bán lẻ", NumberOrder = 1, ParentId = 346},
                 new() { Id =442, Code = "Pricelist.View", Name = "Xem danh sách", NumberOrder = 1, ParentId = 441},
                new() { Id =443, Code = "Pricelist.Create", Name = "Tạo mới", NumberOrder = 2, ParentId = 441},
                new() { Id =444, Code = "Pricelist.Edit", Name = "Cập nhập", NumberOrder = 3, ParentId = 441},


                new() { Id =445, Code = "SetupPoints", Name = "Thiết lập quy đổi VPKM", NumberOrder = 1, ParentId = null},

                new() { Id =446, Code = "SetupPointsPurchase", Name = "Thiết lập điểm hàng mua", NumberOrder = 1, ParentId = 445},
                new() { Id =447, Code = "SetupPointsPurchase.View", Name = "Xem danh sách", NumberOrder = 1, ParentId = 446},
                new() { Id =448, Code = "SetupPointsPurchase.Create", Name = "Tạo mới", NumberOrder = 2, ParentId = 446},
                new() { Id =449, Code = "SetupPointsPurchase.Edit", Name = "Cập nhập", NumberOrder = 3, ParentId = 446},


                new() { Id =450, Code = "SetupPointsVPKM", Name = "Thiết lập điểm VPKM", NumberOrder = 2, ParentId = 445},
                new() { Id =451, Code = "SetupPointsVPKM.View", Name = "Xem danh sách", NumberOrder = 1, ParentId = 450},
                new() { Id =452, Code = "SetupPointsVPKM.Create", Name = "Tạo mới", NumberOrder = 2, ParentId = 450},
                new() { Id =453, Code = "SetupPointsVPKM.Edit", Name = "Cập nhập", NumberOrder = 3, ParentId = 450},

             };
             await _context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Privileges ON");
            var existingCodes = _context.Privileges.Select(x => x.Code).ToList();
            var newPrivileges = privileges.Where(p => !existingCodes.Contains(p.Code)).ToList();

            if (newPrivileges.Any())
            {
                await _context.Privileges.AddRangeAsync(newPrivileges);
                await _context.SaveChangesAsync();
            }

            await _context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Privileges OFF");

        }
    }
}
