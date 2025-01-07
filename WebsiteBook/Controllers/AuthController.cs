using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using FashionShopDemo.Models;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FashionShop.Models;

[Route("api/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IConfiguration _configuration;
    private readonly string _jwtKey;

    public AuthController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IConfiguration configuration)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
        _jwtKey = _configuration["Jwt:Key"];
        if (string.IsNullOrEmpty(_jwtKey))
        {
            _jwtKey = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
        }
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] Register model)
    {
        var user = new ApplicationUser { UserName = model.Username, Email = model.Email, FullName = model.FullName };
        var result = await _userManager.CreateAsync(user, model.Password);

        if (result.Succeeded)
        {
            return Ok(new { Message = "Đăng ký thành công" });
        }
        return BadRequest(result.Errors);
    }


    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] Login model)
    {
        var result = await _signInManager.PasswordSignInAsync(model.Username, model.Password, false, false);

        if (result.Succeeded)
        {
            var user = await _userManager.FindByNameAsync(model.Username);
            var token = GenerateJwtToken(user);

            return Ok(new {
                Token = token ,
                UserId = user.Id,              
                });
        }
        return Unauthorized("Đăng nhập không thành công");
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return Ok(new { Message = "Đã đăng xuất" });
    }

    // Hàm tạo JWT Token
    private string GenerateJwtToken(ApplicationUser user)
    {
        
        /*var key = new byte[32]; 
        using (var rng = new System.Security.Cryptography.RNGCryptoServiceProvider())
        {
            rng.GetBytes(key);
        }*/

        var claims = new[]
        {
            /*new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
            new Claim(JwtRegisteredClaimNames.NameId, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),*/
            new Claim(JwtRegisteredClaimNames.Sub, _configuration["Jwt:Subject"]),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("userId", user.Id.ToString()),
            new Claim("name", user.UserName.ToString())

        };
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
        var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // Tạo khóa SymmetricSecurityKey với độ dài 256 bit
        //var symmetricKey = new SymmetricSecurityKey(key);
        //var creds = new SigningCredentials(symmetricKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
             _configuration["Jwt:Issuer"],
             _configuration["Jwt:Audience"],
             claims: claims,
             expires: DateTime.Now.AddMinutes(30),
             signingCredentials: signIn

        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

