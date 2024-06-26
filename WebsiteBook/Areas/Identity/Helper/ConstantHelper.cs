using Microsoft.Extensions.Configuration;

namespace FashionShopDemo.Areas.Identity.Helper
{
    public class ConstantHelper
    {
        private readonly IConfiguration _configuration;

        public ConstantHelper(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string HostEmail => _configuration["EmailSettings:Host"];
        public int PortEmail => int.Parse(_configuration["EmailSettings:Port"]);
        public string EmailSender => _configuration["EmailSettings:Sender"];
        public string PasswordSender => _configuration["EmailSettings:Password"]; // Hoặc lấy từ Secret Manager
    }
}
