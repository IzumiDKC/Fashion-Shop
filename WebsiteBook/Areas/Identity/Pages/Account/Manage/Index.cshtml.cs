using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using FashionShopDemo.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Vonage;
using Vonage.Request;
using Vonage.Messaging;

namespace FashionShopDemo.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;

        public IndexModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
        }

        public string Username { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Phone]
            [Display(Name = "Phone number")]
            public string PhoneNumber { get; set; }
        }

        private async Task LoadAsync(ApplicationUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

            Username = userName;

            Input = new InputModel
            {
                PhoneNumber = phoneNumber
            };
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

        public async Task<IActionResult> OnPostAsync()
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

            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (Input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to set phone number.";
                    return RedirectToPage();
                }
            }

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your profile has been updated";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostSendVerificationCodeAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (string.IsNullOrEmpty(phoneNumber))
            {
                StatusMessage = "You must provide a phone number first.";
                return RedirectToPage();
            }

            var verificationCode = new Random().Next(100000, 999999).ToString();
            HttpContext.Session.SetString("VerificationCode", verificationCode);

            SendVerificationCode(phoneNumber, verificationCode);

            StatusMessage = "A verification code has been sent to your phone number.";
            return RedirectToPage("VerifyOtp");
        }

        private async void SendVerificationCode(string phoneNumber, string verificationCode)
        {
            try
            {
                var apiKey = _configuration["Vonage:ApiKey"];
                var apiSecret = _configuration["Vonage:ApiSecret"];
                var credentials = Credentials.FromApiKeyAndSecret(apiKey, apiSecret);
                var client = new VonageClient(credentials);

                var response = await client.SmsClient.SendAnSmsAsync(new SendSmsRequest
                {
                    To = phoneNumber,
                    From = "VonageSMS",
                    Text = $"Your verification code is {verificationCode}"
                });

                if (response.Messages[0].Status != "0")
                {
                    throw new Exception($"Failed to send SMS: {response.Messages[0].ErrorText}");
                }
                else
                {
                    Console.WriteLine("SMS sent successfully!");
                    Console.WriteLine($"Verification code: {verificationCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send SMS: {ex.Message}");
            }
        }


    }
}
