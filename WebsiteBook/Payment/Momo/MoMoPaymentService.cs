using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using FashionShopDemo.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

public class MoMoPaymentService
{
    private readonly IConfiguration _configuration;

    public MoMoPaymentService(IConfiguration configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    public async Task<string> CreatePaymentRequest(Order order)
    {
        if (order == null)
        {
            throw new ArgumentNullException(nameof(order), "Order cannot be null");
        }

        string endpoint = _configuration["MoMo:ApiEndpoint"] ?? throw new ArgumentNullException(nameof(_configuration), "ApiEndpoint cannot be null");
        string partnerCode = _configuration["MoMo:PartnerCode"] ?? throw new ArgumentNullException(nameof(_configuration), "PartnerCode cannot be null");
        string accessKey = _configuration["MoMo:AccessKey"] ?? throw new ArgumentNullException(nameof(_configuration), "AccessKey cannot be null");
        string secretKey = _configuration["MoMo:SecretKey"] ?? throw new ArgumentNullException(nameof(_configuration), "SecretKey cannot be null");
        string orderInfo = "Order Info"; 
        string redirectUrl = "YourRedirectUrl"; 
        string ipnUrl = "YourIpnUrl"; 
        string amount = order.TotalPrice.ToString(); 
        string requestId = Guid.NewGuid().ToString(); 
        string requestType = "captureMoMoWallet";

        string rawHash = $"partnerCode={partnerCode}&accessKey={accessKey}&requestId={requestId}&amount={amount}&orderInfo={orderInfo}&returnUrl={redirectUrl}&notifyUrl={ipnUrl}&extraData=";
        string signature = GenerateSignature(rawHash);

        var message = new
        {
            partnerCode,
            accessKey,
            requestId,
            amount,
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
            return responseContent;
        }
    }


    private string GenerateSignature(string rawHash)
    {
        if (string.IsNullOrEmpty(rawHash))
        {
            throw new ArgumentNullException(nameof(rawHash), "rawHash cannot be null or empty");
        }

        string secretKey = _configuration["MoMo:SecretKey"];
        using (var hmacsha256 = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey)))
        {
            byte[] hashmessage = hmacsha256.ComputeHash(Encoding.UTF8.GetBytes(rawHash));
            return BitConverter.ToString(hashmessage).Replace("-", "").ToLower();
        }
    }

}
