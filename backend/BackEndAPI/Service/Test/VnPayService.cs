using System.ComponentModel.DataAnnotations;
using BackEndAPI.Data;
using BackEndAPI.Extensions;
using BackEndAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace BackEndAPI.Service.Test;

public class VnPayService(IConfiguration configuration, IHttpContextAccessor httpContextAccessor, AppDbContext context)
{
    public class OrderInfo()
    {
        public long OrderId { get; set; }
        public long Amount { get; set; }
        public string? OrderDesc { get; set; }

        public DateTime CreatedDate { get; set; }
        public string? Status { get; set; }

        public long PaymentTranId { get; set; }
        public string? BankCode { get; set; }
        public string? PayStatus { get; set; }
    }

    public class VnPayResponse()
    {
        public string? RspCode { get; set; }
        public string? Message { get; set; }
        public string? TerminalId { get; set; }
        public string OrderId { get; set; } = string.Empty;
        public string? TxnRef { get; set; }
        public long VnpayTransId { get; set; }
        public long Amount { get; set; }
        public string? BankCode { get; set; }

        [MaxLength(255)] public string? VnpTransactionStatus { get; set; } = string.Empty;
    }


    public class QueryRequest
    {
        public string? TxnRef { get; set; }
        public string? TransactionDate { get; set; } // format: yyyyMMddHHmmss
    }

    public class VnPayRefundRequest
    {
        public string TransactionType { get; set; } // 02, 03, 04...
        public string TxnRef { get; set; }          // OrderId
        public long Amount { get; set; }            // VND
        public string TransactionDate { get; set; } // yyyyMMddHHmmss
        public string CreateBy { get; set; }
    }

    public async Task<string> RefundAsync(VnPayRefundRequest req)
    {
        var vnpApi        = configuration["VnpaySettings:VnpApi"];
        var vnpHashSecret = configuration["VnpaySettings:VnpHashSecret"];
        var vnpTmnCode    = configuration["VnpaySettings:VnpTmnCode"];

        var          requestId = DateTime.Now.Ticks.ToString();
        const string version   = VnPayLibrary.VERSION;
        const string command   = "refund";

        var amount        = req.Amount * 100;
        var txnRef        = req.TxnRef;
        var orderInfo     = $"Hoàn tiền giao dịch: {txnRef}";
        var transactionNo = ""; // merchant không có giá trị này
        var createDate    = DateTime.Now.ToString("yyyyMMddHHmmss");
        var ip            = GetClientIdAddress();

        // build data to sign
        var signData = string.Join("|", new[]
        {
            requestId,
            version,
            command,
            vnpTmnCode,
            req.TransactionType,
            txnRef,
            amount.ToString(),
            transactionNo,
            req.TransactionDate,
            req.CreateBy,
            createDate,
            ip,
            orderInfo
        });

        var secureHash = Utils.HmacSha512(vnpHashSecret, signData);

        var payload = new
        {
            vnp_RequestId       = requestId,
            vnp_Version         = version,
            vnp_Command         = command,
            vnp_TmnCode         = vnpTmnCode,
            vnp_TransactionType = req.TransactionType,
            vnp_TxnRef          = txnRef,
            vnp_Amount          = amount,
            vnp_OrderInfo       = orderInfo,
            vnp_TransactionNo   = transactionNo,
            vnp_TransactionDate = req.TransactionDate,
            vnp_CreateBy        = req.CreateBy,
            vnp_CreateDate      = createDate,
            vnp_IpAddr          = ip,
            vnp_SecureHash      = secureHash
        };

        using var client   = new HttpClient();
        var       response = await client.PostAsJsonAsync(vnpApi, payload);
        var       result   = await response.Content.ReadAsStringAsync();
        return result;
    }

    public async Task<string> QueryTransaction(QueryRequest request)
    {
        var vnpApi        = configuration["VnpaySettings:VnpApi"];
        var vnpHashSecret = configuration["VnpaySettings:VnpHashSecret"];
        var vnpTmnCode    = configuration["VnpaySettings:VnpTmnCode"];

        var vnpRequestId       = DateTime.Now.Ticks.ToString();
        var vnpVersion         = VnPayLibrary.VERSION;
        var vnpCommand         = "querydr";
        var vnpTxnRef          = request.TxnRef;
        var vnpOrderInfo       = $"Truy van giao dich: {vnpTxnRef}";
        var vnpTransactionDate = request.TransactionDate;
        var vnpCreateDate      = DateTime.Now.ToString("yyyyMMddHHmmss");
        var vnpIpAddr          = GetClientIdAddress();

        var signData = string.Join("|", new[]
        {
            vnpRequestId,
            vnpVersion,
            vnpCommand,
            vnpTmnCode,
            vnpTxnRef,
            vnpTransactionDate,
            vnpCreateDate,
            vnpIpAddr,
            vnpOrderInfo
        });

        var vnpSecureHash = Utils.HmacSha512(vnpHashSecret, signData);

        var payload = new
        {
            vnp_RequestId       = vnpRequestId,
            vnp_Version         = vnpVersion,
            vnp_Command         = vnpCommand,
            vnp_TmnCode         = vnpTmnCode,
            vnp_TxnRef          = vnpTxnRef,
            vnp_OrderInfo       = vnpOrderInfo,
            vnp_TransactionDate = vnpTransactionDate,
            vnp_CreateDate      = vnpCreateDate,
            vnp_IpAddr          = vnpIpAddr,
            vnp_SecureHash      = vnpSecureHash
        };

        using var client = new HttpClient();

        var response     = await client.PostAsJsonAsync(vnpApi, payload);
        var jsonResponse = await response.Content.ReadAsStringAsync();

        return jsonResponse;
    }


    public string Ipn(VnpayLogging order)
    {
        var vnpHashSecret = configuration["VnpaySettings:VnpHashSecret"];
        var vnpReturnUrl  = configuration["VnpaySettings:VnpReturnUrl"];
        var vnpTmnCode    = configuration["VnpaySettings:VnpTmnCode"];
        var vnpUrl        = configuration["VnpaySettings:VnpUrl"];

        order.Status = "0";
        var vnpay = new VnPayLibrary();
        vnpay.AddRequestData("vnp_Version", VnPayLibrary.VERSION);
        vnpay.AddRequestData("vnp_Command", "pay");
        vnpay.AddRequestData("vnp_TmnCode", vnpTmnCode!);
        vnpay.AddRequestData("vnp_Amount",
                             (order.Amount * 100)
                             .ToString()!);
        vnpay.AddRequestData("vnp_BankCode", order.BankCode);
        vnpay.AddRequestData("vnp_CreateDate", DateTime.UtcNow.ToString("yyyyMMddHHmmss"));
        vnpay.AddRequestData("vnp_CurrCode", "VND");
        vnpay.AddRequestData("vnp_Locale", "vn");
        vnpay.AddRequestData("vnp_OrderInfo", "Thanh toan don hang:" + order.OrderId);
        vnpay.AddRequestData("vnp_OrderType", "other"); //default value: other
        vnpay.AddRequestData("vnp_ReturnUrl", vnpReturnUrl!);
        vnpay.AddRequestData("vnp_TxnRef", order.TxnRef);
        vnpay.AddRequestData("vnp_IpAddr", GetClientIdAddress());
        var paymentUrl = vnpay.CreateRequestUrl(vnpUrl!, vnpHashSecret!);
        return paymentUrl;
    }
    
    public string IpnTest(VnpayLogging order)
    {
        var vnpHashSecret = configuration["VnpaySettings:VnpHashSecret"];
        var vnpReturnUrl  = configuration["VnpaySettings:VnpReturnUrl"];
        var vnpTmnCode    = configuration["VnpaySettings:VnpTmnCode"];
        var vnpUrl        = configuration["VnpaySettings:VnpUrl"];

        order.Status = "0";
        var vnpay = new VnPayLibrary();
        vnpay.AddRequestData("vnp_Version", VnPayLibrary.VERSION);
        vnpay.AddRequestData("vnp_Command", "pay");
        vnpay.AddRequestData("vnp_TmnCode", vnpTmnCode!);
        vnpay.AddRequestData("vnp_Amount",
                             (order.Amount * 100)
                             .ToString()!);
        vnpay.AddRequestData("vnp_BankCode", order.BankCode);
        vnpay.AddRequestData("vnp_CreateDate", DateTime.UtcNow.ToString("yyyyMMddHHmmss"));
        vnpay.AddRequestData("vnp_CurrCode", "VND");
        vnpay.AddRequestData("vnp_Locale", "vn");
        vnpay.AddRequestData("vnp_OrderInfo", "Thanh toan don hang:" + order.OrderId);
        vnpay.AddRequestData("vnp_OrderType", "other"); //default value: other
        vnpay.AddRequestData("vnp_ReturnUrl", vnpReturnUrl!);
        vnpay.AddRequestData("vnp_TxnRef", order.TxnRef);
        vnpay.AddRequestData("vnp_IpAddr", GetClientIdAddress());
        var paymentUrl = vnpay.CreateRequestUrlTest(vnpUrl!, vnpHashSecret!);
        return paymentUrl;
    }
    


    public VnPayResponse CallBack(IQueryCollection queries)
    {
        if (!queries.Any())
            return new VnPayResponse
            {
                RspCode = "97",
                Message = "Failed",
            };

        var vnpay         = new VnPayLibrary();
        var vnpHashSecret = configuration["VnpaySettings:VnpHashSecret"];

        foreach (var (key, value) in queries)
        {
            if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
            {
                vnpay.AddResponseData(key, value!);
            }
        }

        var orderId              = vnpay.GetResponseData("vnp_TxnRef");
        var vnpAmount            = Convert.ToInt64(vnpay.GetResponseData("vnp_Amount")) / 100;
        var vnpTranId            = Convert.ToInt64(vnpay.GetResponseData("vnp_TransactionNo"));
        var vnpResponseCode      = vnpay.GetResponseData("vnp_ResponseCode");
        var vnpTransactionStatus = vnpay.GetResponseData("vnp_TransactionStatus");
        var vnpSecureHash        = queries["vnp_SecureHash"];
        var terminalId           = queries["vnp_TmnCode"];
        var bankCode             = queries["vnp_BankCode"];

        var checkSignature = vnpay.ValidateSignature(vnpSecureHash!, vnpHashSecret!);

        if (!checkSignature)
        {
            return new VnPayResponse
            {
                RspCode = "97",
                Message = "Invalid signature",
            };
        }

        if (vnpResponseCode == "00" && vnpTransactionStatus == "00")
        {
            return new VnPayResponse()
            {
                RspCode              = "00",
                Message              = "Payment Success",
                OrderId              = orderId,
                TxnRef               = orderId,
                VnpayTransId         = vnpTranId,
                VnpTransactionStatus = vnpTransactionStatus,
                Amount               = vnpAmount,
                BankCode             = bankCode
            };
        }

        return new VnPayResponse()
        {
            RspCode              = "00",
            Message              = "Confirm Success",
            OrderId              = orderId,
            TxnRef               = orderId,
            VnpayTransId         = vnpTranId,
            VnpTransactionStatus = vnpTransactionStatus,
            Amount               = vnpAmount,
            BankCode             = bankCode
        };
    }


    private string GetClientIdAddress()
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext == null) return "127.0.0.1";
        var forwardedFor = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            return forwardedFor.Split(',')[0].Trim();
        }

        return httpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
    }
}
