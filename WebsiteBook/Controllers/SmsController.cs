using FashionShopDemo.Areas.Identity.Helper;
using Microsoft.AspNetCore.Mvc;

namespace FashionShopDemo.Controllers
{
    public class SmsController : Controller
    {
        private readonly SmsService _smsService;

        public SmsController(SmsService smsService)
        {
            _smsService = smsService;
        }

        [HttpGet]
        public IActionResult SendSms()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SendSms(string phoneNumber, string message)
        {
            await _smsService.SendSmsAsync(phoneNumber, message);
            ViewBag.Message = "Gửi SMS Thành Công!!";
            return View();
        }
    }
}
