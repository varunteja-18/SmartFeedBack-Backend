using FeedbackApp.API.Dtos;
using FeedbackApp.API.Models;
using FeedbackApp.API.Helpers;
using FeedbackApp.API.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;


namespace FeedbackApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        public AuthController(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto formData)
        {
            if (await _context.Users.AnyAsync(u => u.Email == formData.Email))
                return BadRequest(new { message = "❌ Email already registered" });

            // Generate key and hash password
            using var hmac = new HMACSHA256();
            var key = Convert.ToBase64String(hmac.Key);
            var hashedPassword = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(formData.Password)));

            var user = new User
            {
                Username = formData.Username,
                Email = formData.Email,
                PasswordHash = hashedPassword,
                PasswordKey = key
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "✅ Registration successful!" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto formData)
        {
            var token = "";
            if (formData.Email == "admin@gmail.com")
            {
                if (formData.Password == "admin@123")
                {
                    token = JwtHelper.GenerateToken(0, "Admin", "admin@gmail.com", "admin", _config);

                    return Ok(new
                    {
                        message = "✅ Login successful",
                        role = "admin",
                        token,
                        email = "admin@gmail.com",
                        username = "Admin"
                    });
                }
                else
                {
                    return Unauthorized(new { message = "❌ Invalid email or password" });

                }

            }
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == formData.Email);
            if (user == null)
                return Unauthorized(new { message = "❌ User not found" });

            var keyBytes = Convert.FromBase64String(user.PasswordKey);
            using var hmac = new HMACSHA256(keyBytes);
            var passwordHash = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(formData.Password)));

            if (user.PasswordHash != passwordHash)
                return Unauthorized(new { message = "❌ Invalid email or password" });

            token = JwtHelper.GenerateToken(user.Id, user.Username, user.Email, "user", _config);
            return Ok(new
            {
                message = "✅ Login successful",
                token,
                email = user.Email,
                role = "user",
                username = user.Username
            });
        }
    }
}
