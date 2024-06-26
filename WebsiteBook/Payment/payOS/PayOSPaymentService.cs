using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using FashionShopDemo.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

public class PayOSPaymentService
{
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    private readonly ILogger<PayOSPaymentService> _logger;

    public PayOSPaymentService(IConfiguration configuration, ILogger<PayOSPaymentService> logger)
    {
        _configuration = configuration;
        _httpClient = new HttpClient();
        _logger = logger;
    }

    public async Task<string> CreatePaymentRequest(Order order)
    {
        var payOSConfig = _configuration.GetSection("PayOS");
        var clientId = payOSConfig["ClientId"];
        var apiKey = payOSConfig["ApiKey"];
        var checksumKey = payOSConfig["ChecksumKey"];
        var paymentUrl = payOSConfig["PaymentUrl"];

        var amount = (int)(order.TotalPrice * 100); // Số tiền thanh toán (chuyển sang đơn vị nhỏ nhất)
        var orderId = order.Id.ToString();
        var requestId = Guid.NewGuid().ToString();

        var rawData = $"{clientId}|{requestId}|{orderId}|{amount}|{checksumKey}";
        var checksum = GenerateChecksum(rawData);

        var requestBody = new
        {
            clientId,
            requestId,
            orderId,
            amount,
            checksum
        };

        try
        {
            var response = await _httpClient.PostAsJsonAsync(paymentUrl, requestBody);
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            var jsonResponse = JsonConvert.DeserializeObject<PayOSResponse>(responseContent);

            // Giả sử PayOSResponse có một thuộc tính PayUrl chứa URL thanh toán
            _logger.LogInformation("PayOS Payment URL: {0}", jsonResponse.PayUrl);
            return jsonResponse.PayUrl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating PayOS payment request");
            throw;
        }
    }

    private string GenerateChecksum(string rawData)
    {
        using (var sha256 = SHA256.Create())
        {
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawData));
            return BitConverter.ToString(bytes).Replace("-", "").ToLower();
        }
    }
}

public class PayOSResponse
{
    public string PayUrl { get; set; }
}
