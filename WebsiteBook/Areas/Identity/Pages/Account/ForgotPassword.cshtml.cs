// Licensed to the .NET Foundation under one hoặc nhiều agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using FashionShopDemo.Areas.Identity.Helper;
using FashionShopDemo.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace FashionShop.Areas.Identity.Pages.Account
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SendMail _sendMail;
        private readonly ILogger<ForgotPasswordModel> _logger;

        [TempData]
        public string StatusMessage { get; set; }

        public ForgotPasswordModel(
            UserManager<ApplicationUser> userManager,
            SendMail sendMail,
            ILogger<ForgotPasswordModel> logger)
        {
            _userManager = userManager;
            _sendMail = sendMail;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(Input.Email);
                if (user == null)
                {
                    // Không tiết lộ rằng người dùng không tồn tại
                    var message = $"Yêu cầu quên mật khẩu cho email không tồn tại: {Input.Email}";
                    _logger.LogInformation(message);
                    Console.WriteLine(message);
                    StatusMessage = "Nếu có tài khoản với email đó, bạn sẽ nhận được email hướng dẫn đặt lại mật khẩu.";
                    return RedirectToPage("./ForgotPasswordConfirmation");
                }

                if (!await _userManager.IsEmailConfirmedAsync(user))
                {
                    var message = $"Yêu cầu quên mật khẩu cho email chưa xác nhận: {Input.Email}";
                    _logger.LogInformation(message);
                    Console.WriteLine(message);
                    StatusMessage = "Nếu có tài khoản với email đó, bạn sẽ nhận được email hướng dẫn đặt lại mật khẩu.";
                    return RedirectToPage("./ForgotPasswordConfirmation");
                }

                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Page(
                    "/Account/ResetPassword",
                    pageHandler: null,
                    values: new { area = "Identity", code },
                    protocol: Request.Scheme);

                // Ghi log URL callback vào console
                Console.WriteLine($"Callback URL: {callbackUrl}");

                var emailForgotContent = $@"
                    Xin chào,
                    Chào mừng bạn đến với FashionShop! Bạn đã kích hoạt chức năng quên mật khẩu. Vui lòng nhấn vào liên kết sau để cấp lại mật khẩu:
                    {callbackUrl}

                    Đây là thư tự động từ hệ thống, vui lòng không phản hồi thư này. Xin cảm ơn!
                ";

                _logger.LogInformation($"Bắt đầu gửi email quên mật khẩu tới {user.Email}");
                bool emailSent = _sendMail.SendEmail(
                    user.Email,
                    "Quên Mật Khẩu",
                    "?",
                    emailForgotContent);

                if (!emailSent)
                {
                    var errorMessage = $"Không thể gửi email quên mật khẩu tới {user.Email}.";
                    _logger.LogError(errorMessage);
                    Console.WriteLine(errorMessage);
                    StatusMessage = "Đã xảy ra lỗi. Vui lòng thử lại sau.";
                }
                else
                {
                    var successMessage = $"Email thay đổi mật khẩu đã được gửi tới {user.Email}.";
                    _logger.LogInformation(successMessage);
                    Console.WriteLine(successMessage);
                    StatusMessage = "Email thay đổi mật khẩu đã được gửi. Vui lòng kiểm tra hòm thư của bạn.";
                }
                return RedirectToPage("./ForgotPasswordConfirmation");
            }
            return Page();
        }
    }
}
