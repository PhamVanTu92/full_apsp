using System.Web;
using BackEndAPI.Models.Banks;
using Org.BouncyCastle.Math.EC;

namespace BackEndAPI.Service.Payments;

public class PayOne
{
    public static PayOneConfig _config { get; set; } = new PayOneConfig();

    public static string  MerchantSendRequet(PaymentRequest paymentRequest)
    {
        long ticks = DateTime.Now.Ticks;
        string vpcMerchantTxnRef = "TEST_" + ticks.ToString();
        paymentRequest.VpcAccessCode = _config.PayNow.AccessCode;
        paymentRequest.VpcMerchant = _config.PayNow.Id;
        paymentRequest.VpcMerchTxnRef = vpcMerchantTxnRef;
        paymentRequest.VpcReturnURL = _config.RedirectLink;
        var merchantParams = paymentRequest.ToDictionary();

        Dictionary<string, string> dictSorted = Util.SortParamsMap(merchantParams);
        var stringToHash = Util.GenerateStringToHash(dictSorted);
        Console.WriteLine("merchant's string to hash: " + stringToHash);
        var sign = Util.GenSecurehash(stringToHash, _config!.PayNow!.HashCode);
        merchantParams.Add("vpc_SecureHash", sign);
        var queryParam = "";
        foreach (var items in merchantParams)
        {
            var key = items.Key;
            var value = items.Value;
            queryParam += key + "=" + HttpUtility.UrlEncode(value) + "&";
        }

        var requestsUrl = _config.BaseUrl + _config.UrlPrefix + queryParam;
        return Util.ExcuteGetMethod(requestsUrl);
    }

    public static void QueryDRApi(
        string merchTxnRef)
    {
        Dictionary<string, string> merchantParams = new Dictionary<string, string>
        {
            { "vpc_Version", "2" },
            { "vpc_Command", "queryDR" },
            { "vpc_AccessCode", _config.PayNow.AccessCode },
            { "vpc_Merchant", _config.PayNow.Id },
            { "vpc_Password", "admin@123456" },
            { "vpc_User", "Administrator" },
            { "vpc_MerchTxnRef", merchTxnRef },
        };
        Dictionary<string, string> dictSorted = Util.SortParamsMap(merchantParams);
        var stringToHash = Util.GenerateStringToHash(dictSorted);
        Console.WriteLine("merchant's string to hash: " + stringToHash);
        var sign = Util.GenSecurehash(stringToHash, _config.PayNow.HashCode);
        merchantParams.Add("vpc_SecureHash", sign);

        Dictionary<string, string> headerRequest = new Dictionary<string, string>();

        string url = _config.BaseUrl + "/msp/api/v1/vpc/invoices/queries";

        Util.ExcutePostMethod(url, merchantParams, headerRequest);
    }
}