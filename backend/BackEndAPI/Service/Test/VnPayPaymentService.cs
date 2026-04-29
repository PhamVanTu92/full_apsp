using BackEndAPI.Data;
using BackEndAPI.Extensions;
using BackEndAPI.Models;
using BackEndAPI.Models.Banks;
using BackEndAPI.Models.Other;
using Microsoft.EntityFrameworkCore;

namespace BackEndAPI.Service.Test;

public class VnPayPaymentService(
    AppDbContext context,
    VnPayService vnPayService,
    IConfiguration configuration,
    LoggingSystemService logSystem)
    : IVnPayPaymentService
{
    public async Task<(string?, Mess?)> PaymentAsync(CreatePaymentDto dto)
    {
        var odoc = await context.ODOC.Include(odoc => odoc.PaymentInfo)
            .Include(x => x.ItemDetail)
            .FirstOrDefaultAsync(x => x.Id == dto.DocId && x.Status == "CTT");

        if (odoc == null)
            return (null, new Mess
            {
                Status = 400,
                Errors = "Phiếu không hợp lệ"
            });


        var isAllVisa = odoc.ItemDetail.All(x => x.PaymentMethodCode == "PayVisa");

        long amount;

        if (isAllVisa)
        {
            amount = (long)Math.Round(odoc.PaymentInfo.TotalBeforeVat + odoc.VATAmount ?? 0, 0);
        }
        else
        {
            amount = (long)Math.Round(
                odoc.PaymentInfo.TotalAfterVat - odoc.PaymentInfo.BonusAmount,
                0
            );
        }

        var existedPayment = await context.VnpayLogging.FirstOrDefaultAsync(x => x.OrderId == odoc.InvoiceCode);

        var newVnPayLogging = new VnpayLogging
        {
            OrderId       = odoc.InvoiceCode!,
            TxnRef        = odoc.InvoiceCode! + "with" + DateTime.Now.ToString("yyMMddHHmmss"),
            Amount        = amount,
            Status        = "0",
            PaymentTranId = 0,
            BankCode      = dto.PaymentType,
            CreatedDate   = DateTime.Now,
        };

        var url = vnPayService.Ipn(newVnPayLogging);


        if (existedPayment is null)
        {
            newVnPayLogging.Payload = url;
            context.VnpayLogging.Add(newVnPayLogging);
        }
        else
        {
            existedPayment.Amount      = newVnPayLogging.Amount;
            existedPayment.BankCode    = newVnPayLogging.BankCode;
            existedPayment.CreatedDate = newVnPayLogging.CreatedDate;
            existedPayment.Payload     = url;
        }

        await context.SaveChangesAsync();

        return (url, null);
    }

    public async Task<(string?, Mess?, object?)> CallBackAsync(IQueryCollection query)
    {
        var rawTxnRef = query["vnp_TxnRef"].ToString().Split("with")[0];
        var vnpay     = await context.VnpayLogging.FirstOrDefaultAsync(x => x.OrderId == rawTxnRef);
        if (vnpay is null)
            return (null, new Mess
                { Status = 0, Errors = "Đơn hàng không hợp lệ" }, null);

        var odoc = await context.ODOC.Include(odoc => odoc.Payments)
            .FirstOrDefaultAsync(x => x.InvoiceCode == vnpay.OrderId);

        if (odoc is null)
            return (null, new Mess
            {
                Status = 0,
                Errors = "Đơn hàng không hợp lệ"
            }, null);


        if (vnpay.Status == "1")
        {
            return ($"00/{odoc.Id}", null, odoc);
        }

        return (null, new Mess
        {
            Status = 0,
            Errors = "Thanh toán thất bại"
        }, odoc);
    }

    public async Task<VnPayRespons> IpnAsync(IQueryCollection queries)
    {
        var vnpay         = new VnPayLibrary();
        var vnpHashSecret = configuration["VnpaySettings:VnpHashSecret"];

        var mess = new VnPayRespons
        {
            RspCode = "99",
            Message = "Unknow error",
        };
        var flag = 0;


        foreach (var (key, value) in queries)
        {
            if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
            {
                vnpay.AddResponseData(key, value!);
            }
        }


        var callBack = vnPayService.CallBack(queries);


        var txnRef        = vnpay.GetResponseData("vnp_TxnRef");
        var orderId       = txnRef.Split("with")[0];
        var vnpAmount     = Convert.ToInt64(vnpay.GetResponseData("vnp_Amount"));
        var vnpSecureHash = queries["vnp_SecureHash"];


        var vnpayLog = await context.VnpayLogging.FirstOrDefaultAsync(x => x.OrderId == orderId && x.Status == "0");
        if (vnpayLog is null)
        {
            return new VnPayRespons
            {
                Message = "Order Not Found",
                RspCode = "01"
            };
        }


        var odoc = await context.ODOC.Include(odoc => odoc.PaymentInfo)
            .FirstOrDefaultAsync(x => x.InvoiceCode == orderId);

        if (odoc is null)
        {
            return new VnPayRespons
            {
                Message = "Order Not Found",
                RspCode = "01"
            };
        }


        if (odoc.Status != "CTT")
        {
            return new VnPayRespons
            {
                Message = "Order already confirmed",
                RspCode = "02"
            };
        }


        if (vnpAmount !=
            (long)(Math.Round((odoc.PaymentInfo.TotalAfterVat - odoc.PaymentInfo.BonusAmount), 0)) * 100)
        {
            return new VnPayRespons
            {
                Message = "Invalid amount",
                RspCode = "04"
            };
        }


        // Chỉ khi toàn bộ điều kiện hợp lệ và giao dịch VNPAY thành công mới được ghi nhận Payment.
        if (callBack is { RspCode: "00", VnpTransactionStatus: "00" })
        {
            var payment = new Payment()
            {
                PaymentDate       = DateTime.UtcNow,
                TotalAmount       = 0,
                PaymentAmount     = vnpAmount,
                PartnerId         = odoc.CardId,
                PartnerName       = odoc.CardName ?? "",
                PartnerContactNo  = odoc.CardCode ?? "",
                PaymentMethodCode = "VnPay",
                PaymentMethodName = "VnPay",
                CreatedDate       = DateTime.UtcNow,
                DocId             = odoc.Id,
                Status            = "A",
                DocCode           = odoc.InvoiceCode!,
                DocType           = odoc.ObjType,
                Crd4              = null,
            };
            odoc.Payments.Add(payment);

            if (vnpayLog is not null) vnpayLog.Status = "1";
            odoc.Status      = "CXN";
            odoc.VnPayStatus = true;

            mess = new VnPayRespons
            {
                RspCode = "00",
                Message = "Payment Success",
            };
        }
        else
        {
            odoc.VnPayStatus = false;
        }


        await logSystem.SaveWithTransAsync("INFO", "IPNASYNC",
                                           $"flag {flag}, paymentStatus {odoc.VnPayStatus} Callback: callbackAmount: {callBack.OrderId}, callBackMessage {callBack.Message}, callback.rspcode: {callBack.RspCode}, callback.vnpaytransId: {callBack.VnpayTransId}, callback.vnptransactionstatus {callBack.VnpTransactionStatus} ," +
                                           $" TxnRef: {txnRef}",
                                           "vnpay", 1);


        await context.SaveChangesAsync();

        return mess;
    }
}