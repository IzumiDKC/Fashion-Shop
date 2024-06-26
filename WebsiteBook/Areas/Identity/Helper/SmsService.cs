using Microsoft.Extensions.Configuration;
using Vonage;
using Vonage.Request;
using Vonage.Messaging;
using System.Threading.Tasks;
namespace FashionShopDemo.Areas.Identity.Helper;

public class SmsService
{
    private readonly string apiKey;
    private readonly string apiSecret;

    public SmsService(IConfiguration configuration)
    {
        apiKey = configuration["Nexmo:ApiKey"];
        apiSecret = configuration["Nexmo:ApiSecret"];
    }

    public async Task SendSmsAsync(string to, string text)
    {
        var credentials = Credentials.FromApiKeyAndSecret(apiKey, apiSecret);
        var client = new VonageClient(credentials);
        var response = await client.SmsClient.SendAnSmsAsync(new SendSmsRequest
        {
            To = to,
            From = "Vonage",
            Text = text
        });
    }
}
