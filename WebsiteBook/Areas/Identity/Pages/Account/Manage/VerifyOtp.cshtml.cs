using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;

using System.ComponentModel.DataAnnotations;

namespace FashionShopDemo.Areas.Identity.Pages.Account.Manage
{
    public class VerifyOtpModel : PageModel
    {
        [BindProperty]
        public InputModel Input { get; set; }

        public string StatusMessage { get; set; }

        public class InputModel
        {
            [Required]
            public string OTP { get; set; }
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var storedOtp = HttpContext.Session.GetString("VerificationCode");
            if (Input.OTP == storedOtp)
            {
                StatusMessage = "OTP xác nhận thành công.";
                return RedirectToPage("Index");
            }
            else
            {
                StatusMessage = "Mã OTP không đúng. Vui lòng thử lại.";
            }

            return Page();
        }
    }
}