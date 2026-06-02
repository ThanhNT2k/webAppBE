using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ComicBackend.WebApi.Services;
using ComicBackend.WebApi.Models;
using static Postgrest.Constants;
using System;
using System.Threading.Tasks;

namespace ComicBackend.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly SupabaseService _supabase;
        private readonly TokenService _tokenService;

        public AuthController(SupabaseService supabase, TokenService tokenService)
        {
            _supabase = supabase;
            _tokenService = tokenService;
        }

        // 1. API ĐĂNG KÝ
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            if (string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Password))
                return BadRequest(new { error = "Vui lòng điền đầy đủ thông tin!" });

            try
            {
                // Kiểm tra xem đã tồn tại chưa bằng Supabase SDK
                var existing = await _supabase.Client
                    .From<User>()
                    .Filter(x => x.Email, Operator.Equals, model.Email.ToLower())
                    .Get();

                if (existing.Models.Any())
                    return BadRequest(new { error = "Email này đã được đăng ký!" });

                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.Password);

                // var newUser = new User
                // {
                //     Id = Guid.NewGuid(),
                //     DisplayName = model.Username ?? model.Email.Split('@')[0],
                //     Email = model.Email.ToLower(),
                //     PasswordHash = hashedPassword,
                //     Role = "user",
                //     UpdatedAt = DateTime.UtcNow
                // };

                // await _supabase.Client.From<User>().Insert(newUser);

                // string token = _tokenService.GenerateToken(newUser.DisplayName, newUser.Role);

                return Ok(new {
                    // token = token,
                    message = "Đăng ký thành công."
            });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // 2. API ĐĂNG NHẬP
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            try
            {
                var identifier = (model.Username ?? model.Email ?? "").ToLower();

                var response = await _supabase.Client
                    .From<User>()
                    .Filter(x => x.Email, Operator.Equals, identifier)
                    .Get();

                var account = response.Models.FirstOrDefault();

                if (account == null || !BCrypt.Net.BCrypt.Verify(model.Password, account.PasswordHash))
                {
                    return BadRequest(new { error = "Tài khoản hoặc mật khẩu không chính xác!" });
                }

                var token = _tokenService.GenerateToken(account.DisplayName, account.Role);
                
                return Ok(new { 
                    token = token,
                    user = new { id = account.Id, username = account.DisplayName, role = account.Role }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    // Models nội bộ cho Request
    public class RegisterModel { public string? Username { get; set; } public string Email { get; set; } = string.Empty; public string Password { get; set; } = string.Empty; }
    public class LoginModel { public string? Username { get; set; } public string? Email { get; set; } public string Password { get; set; } = string.Empty; }
}