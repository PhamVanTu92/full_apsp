namespace BackEndAPI.Service.Payments;

public class PaymentRequest
{
    public string VpcVersion { get; set; } = "2";
    public string VpcCurrency { get; set; } = "VND";
    public string VpcCommand { get; set; } = "pay";
    public string VpcAccessCode { get; set; }
    public string VpcMerchant { get; set; }
    public string VpcLocale { get; set; } = "vn";
    public string VpcReturnURL { get; set; } = "https://demo.apsp.foxai.com.vn:999/payment-status";
    public string VpcMerchTxnRef { get; set; }
    public string VpcOrderInfo { get; set; } = "Ma Don Hang";
    public string VpcAmount { get; set; } = "10000000";
    public string VpcTicketNo { get; set; } = "192.168.166.149";
    public string AgainLink { get; set; } = "https://mtf.onepay.vn/client/qt/";
    public string Title { get; set; } = "PHP VPC 3-Party";
    public string VpcCustomerPhone { get; set; } = "84987654321";
    public string VpcCustomerEmail { get; set; } = "test@onepay.vn";
    public string VpcCustomerId { get; set; } = "test";
    public string RedirectLink { get; set; } = "http://demo.apsp.foxai.com.vn:999/payment-status";


    public Dictionary<string, string> ToDictionary()
    {
        return new Dictionary<string, string>
        {
            { "vpc_Version", VpcVersion },
            { "vpc_Currency", VpcCurrency },
            { "vpc_Command", VpcCommand },
            { "vpc_AccessCode", VpcAccessCode },
            { "vpc_Merchant", VpcMerchant },
            { "vpc_Locale", VpcLocale },
            { "vpc_ReturnURL", VpcReturnURL },
            { "vpc_MerchTxnRef", VpcMerchTxnRef },
            { "vpc_OrderInfo", VpcOrderInfo },
            { "vpc_Amount", VpcAmount },
            { "vpc_TicketNo", VpcTicketNo },
            { "AgainLink", AgainLink },
            { "Title", Title },
            { "vpc_Customer_Phone", VpcCustomerPhone },
            { "vpc_Customer_Email", VpcCustomerEmail },
            { "vpc_Customer_Id", VpcCustomerId }
        };
    }
}