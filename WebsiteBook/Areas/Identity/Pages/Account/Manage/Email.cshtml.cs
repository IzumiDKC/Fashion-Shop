// Licensed to the .NET Foundation under one or more agreements.
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
using MimeKit;

namespace FashionShopDemo.Areas.Identity.Pages.Account.Manage
{
    public class EmailModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
      //  private readonly IEmailSender _emailSender;
        private readonly SendMail _sendMail;
        private readonly ILogger<EmailModel> _logger;

        public EmailModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailSender emailSender,
            SendMail sendMail,
            ILogger<EmailModel> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
          //  _emailSender = emailSender;
            _sendMail = sendMail;
            _logger = logger;
        }


        public string Email { get; set; }
        public bool IsEmailConfirmed { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "New email")]
            public string NewEmail { get; set; }
        }

        private async Task LoadAsync(ApplicationUser user)
        {
            var email = await _userManager.GetEmailAsync(user);
            Email = email;

            Input = new InputModel
            {
                NewEmail = email,
            };

            IsEmailConfirmed = await _userManager.IsEmailConfirmedAsync(user);
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }
        

        public async Task<IActionResult> OnPostChangeEmailAsync()
        {

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            var email = await _userManager.GetEmailAsync(user);
            if (Input.NewEmail != email)
            {
                var userId = await _userManager.GetUserIdAsync(user);
                var code = await _userManager.GenerateChangeEmailTokenAsync(user, Input.NewEmail);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Page(
                    "/Account/ConfirmEmailChange",
                    pageHandler: null,
                    values: new { area = "Identity", userId, email = Input.NewEmail, code },
                    protocol: Request.Scheme);
                Console.WriteLine($"Callback URL: {callbackUrl}");
                // await _emailSender.SendEmailAsync(
                //     Input.NewEmail,
                //     "Confirm your email",
                //     $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                var emailContent = $@"
                                        Xin chào,
                                        Chào mừng bạn đến với FashionShop! Vui lòng nhấp vào liên kết dưới đây để THAY ĐỔI địa chỉ email của bạn:
                                        {callbackUrl}

                                        Đây là thư tự động từ hệ thống, vui lòng không phản hồi thư này. Xin cảm ơn!
                                    ";


                bool emailSent = _sendMail.SendEmail(
                        user.Email,
                        "Xác Nhận Thay Đổi Email",
                        "?",
                        emailContent);

                if (!emailSent)
                {
                    _logger.LogError("Không thể đổi email xác nhận.");
                    StatusMessage = "Đã xảy ra lỗi khi thay đổi email. Vui lòng thử lại sau.";
                }
                else
                {
                    StatusMessage = "Email xác nhận đã được gửi. Vui lòng kiểm tra hòm thư của bạn.";
                }
                return RedirectToPage();
            }


            StatusMessage = "Email của bạn không thay đổi!.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostSendVerificationEmailAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            Console.WriteLine("Xác nhận mail gốc");
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            var userId = await _userManager.GetUserIdAsync(user);
            var email = await _userManager.GetEmailAsync(user);
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = Url.Page(
                "/Account/ConfirmEmail",
                pageHandler: null,
                values: new { area = "Identity", userId, code },
                protocol: Request.Scheme
            );
                
          //  var encodedUrl = HtmlEncoder.Default.Encode(callbackUrl);
            Console.WriteLine($"Callback URL: {callbackUrl}");

            var emailContent = $"{callbackUrl}."; 
            emailContent = emailContent.Replace("&amp;", "&");

            var emailContent2 = $@"
                                        Xin chào,
                                        Chào mừng bạn đến với FashionShop! Vui lòng nhấp vào liên kết dưới đây để XÁC NHẬN địa chỉ email của bạn:
                                        {callbackUrl}
                                        
                                        Đây là thư tự động từ hệ thống, vui lòng không phản hồi thư này. Xin cảm ơn!
                                    ";


            bool emailSent = _sendMail.SendEmail(
                user.Email,
                "Xác Nhận Email",
                "?",
               //  $"{encodedUrl}."
               emailContent2);

            if (!emailSent)
            {
                _logger.LogError("Không thể gửi email xác nhận.");
                StatusMessage = "Đã xảy ra lỗi khi gửi email xác nhận. Vui lòng thử lại sau.";
            }
            else
            {
                StatusMessage = "Email xác nhận đã được gửi. Vui lòng kiểm tra email của bạn.";
            }

            return RedirectToPage();


       //     await _sendMail.SendEmail(user.Email, "Confirm your email", $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.", "");
        }
    }
}
