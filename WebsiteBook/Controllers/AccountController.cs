using FashionShopDemo.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FashionShop.Controllers
{
    [Route("api/account")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public AccountController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [HttpPut("update-profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            user.UserName = request.UserName;
            user.Email = request.Email;
            user.FullName = request.FullName;
            user.Age = request.Age;
            user.Address = request.Address;
            user.PhoneNumber = request.PhoneNumber;


            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return Ok(user); 
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            var userProfile = new UserProfile
            {
                UserName = user.UserName,
                Email = user.Email,
                FullName = user.FullName,
                Age = user.Age,
                Address = user.Address, 
                PhoneNumber = user.PhoneNumber 
            };

            return Ok(userProfile); 
        }
    }

    public class UpdateProfileRequest
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string? Age { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
    }

    public class UserProfile
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string? Age { get; set; } 
        public string Address { get; set; } 
        public string PhoneNumber { get; set; } 
    }

}
