using FashionShopDemo.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace FashionShopDemo.Areas.Identity.Pages.Account.Manage
{
    public class VerifyPhoneNumberModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public VerifyPhoneNumberModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            public string VerificationCode { get; set; }
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var storedCode = HttpContext.Session.GetString("VerificationCode");
            if (Input.VerificationCode == storedCode)
            {
                var newPhoneNumber = TempData["NewPhoneNumber"]?.ToString();
                if (newPhoneNumber != null)
                {
                    var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, newPhoneNumber);
                    if (!setPhoneResult.Succeeded)
                    {
                        ModelState.AddModelError(string.Empty, "Unexpected error when trying to set phone number.");
                        return Page();
                    }

                    await _signInManager.RefreshSignInAsync(user);
                    TempData["StatusMessage"] = "Your phone number has been verified and updated.";
                    return RedirectToPage("./Index");
                }
            }

            ModelState.AddModelError(string.Empty, "Invalid verification code.");
            return Page();
        }
    }
}
