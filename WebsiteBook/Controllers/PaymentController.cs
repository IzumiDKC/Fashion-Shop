using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

public class PaymentController : Controller
{
    private readonly IConfiguration _configuration;

    public PaymentController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<IActionResult> CreatePayment()
    {
        string endpoint = _configuration["MoMo:ApiEndpoint"];
        string partnerCode = _configuration["MoMo:PartnerCode"];
        string accessKey = _configuration["MoMo:AccessKey"];
        string secretKey = _configuration["MoMo:SecretKey"];
        string orderInfo = "Order Info";
        string redirectUrl = "YourRedirectUrl";
        string ipnUrl = "YourIpnUrl";
        string amount = "1000";
        string orderId = Guid.NewGuid().ToString();
        string requestId = Guid.NewGuid().ToString();
        string requestType = "captureMoMoWallet";

        string rawHash = $"partnerCode={partnerCode}&accessKey={accessKey}&requestId={requestId}&amount={amount}&orderId={orderId}&orderInfo={orderInfo}&returnUrl={redirectUrl}&notifyUrl={ipnUrl}&extraData=";
        string signature = SignSHA256(rawHash, secretKey);

        var message = new
        {
            partnerCode,
            accessKey,
            requestId,
            amount,
            orderId,
            orderInfo,
            returnUrl = redirectUrl,
            notifyUrl = ipnUrl,
            extraData = "",
            requestType,
            signature
        };

        using (var client = new HttpClient())
        {
            var response = await client.PostAsync(endpoint, new StringContent(JsonConvert.SerializeObject(message), Encoding.UTF8, "application/json"));
            var responseContent = await response.Content.ReadAsStringAsync();
            return Content(responseContent);
        }
    }

    [HttpGet]
    public IActionResult PaymentResult(string partnerCode, string orderId, string requestId, string amount, string orderInfo, string orderType, string transId, int resultCode, string message, string payType, long responseTime, string extraData, string signature)
    {
        // Kiểm tra chữ ký
        string secretKey = _configuration["MoMo:SecretKey"];
        string rawHash = $"partnerCode={partnerCode}&orderId={orderId}&requestId={requestId}&amount={amount}&orderInfo={orderInfo}&orderType={orderType}&transId={transId}&resultCode={resultCode}&message={message}&payType={payType}&responseTime={responseTime}&extraData={extraData}";
        string mySignature = SignSHA256(rawHash, secretKey);

        if (mySignature == signature)
        {
            if (resultCode == 0)
            {
                // Thanh toán thành công, cập nhật trạng thái đơn hàng
                ViewBag.Message = "Thanh toán thành công!";
            }
            else
            {
                // Thanh toán thất bại
                ViewBag.Message = "Thanh toán thất bại: " + message;
            }
        }
        else
        {
            ViewBag.Message = "Chữ ký không hợp lệ!";
        }

        return View();
    }

    private static string SignSHA256(string data, string secretKey)
    {
        using (var hmacsha256 = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey)))
        {
            byte[] hashmessage = hmacsha256.ComputeHash(Encoding.UTF8.GetBytes(data));
            return BitConverter.ToString(hashmessage).Replace("-", "").ToLower();
        }
    }
    [HttpPost]
    public async Task<IActionResult> IpnListener()
    {
        using (var reader = new StreamReader(Request.Body))
        {
            var body = await reader.ReadToEndAsync();
            var ipnData = JsonConvert.DeserializeObject<MoMoIPNData>(body);

            // Kiểm tra chữ ký
            string secretKey = _configuration["MoMo:SecretKey"];
            string rawHash = $"partnerCode={ipnData.partnerCode}&accessKey={ipnData.accessKey}&requestId={ipnData.requestId}&amount={ipnData.amount}&orderId={ipnData.orderId}&orderInfo={ipnData.orderInfo}&orderType={ipnData.orderType}&transId={ipnData.transId}&resultCode={ipnData.resultCode}&message={ipnData.message}&payType={ipnData.payType}&responseTime={ipnData.responseTime}&extraData={ipnData.extraData}";
            string mySignature = SignSHA256(rawHash, secretKey);

            if (mySignature == ipnData.signature)
            {
                if (ipnData.resultCode == 0)
                {
                    // Cập nhật trạng thái đơn hàng
                    // Ví dụ: MarkOrderAsPaid(ipnData.orderId);
                }
            }

            return Ok();
        }
    }

    public class MoMoIPNData
    {
        public string partnerCode { get; set; }
        public string accessKey { get; set; }
        public string requestId { get; set; }
        public string amount { get; set; }
        public string orderId { get; set; }
        public string orderInfo { get; set; }
        public string orderType { get; set; }
        public string transId { get; set; }
        public int resultCode { get; set; }
        public string message { get; set; }
        public string payType { get; set; }
        public long responseTime { get; set; }
        public string extraData { get; set; }
        public string signature { get; set; }
    }

}
