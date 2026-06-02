using Microsoft.AspNetCore.Mvc;
using ComicBackend.WebApi.Services;
using ComicBackend.WebApi.Models;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Supabase.Gotrue;

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
                var authResponse = await _supabase.Client.Auth.SignUp(model.Email, model.Password, new SignUpOptions {
                    Data = new Dictionary<string, object> {
                        { "username", model.Username ?? model.Email.Split('@')[0] }
                    }
                });

                if (authResponse?.User == null) {
                    return BadRequest(new { error = "Đăng ký thất bại, vui lòng kiểm tra lại Email hoặc Mật khẩu." });
                }
                
                return Ok(new { message = "Đăng ký thành công!" });
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
            // Kiểm tra dữ liệu đầu vào
            if (string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Password))
                return BadRequest(new { error = "Email và Mật khẩu không được để trống!" });

            try
            {
                var response = await _supabase.Client.Auth.SignIn(model.Email, model.Password);

                if (response?.User == null) {
                    return BadRequest(new { error = "Tài khoản hoặc mật khẩu không chính xác!" });
                }

                var token = _tokenService.GenerateToken(response.User.Email, "user");
                
                return Ok(new { 
                    token = token,
                    user = new { 
                        id = response.User.Id, 
                        email = response.User.Email, 
                        role = "user" 
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Lỗi server: " + ex.Message });
            }
        }
    }

    public class RegisterModel 
    { 
        public string? Username { get; set; } 
        public string Email { get; set; } = string.Empty; 
        public string Password { get; set; } = string.Empty; 
    }

    public class LoginModel 
    { 
        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty; 
        
        [JsonPropertyName("password")]
        public string Password { get; set; } = string.Empty; 
    }
}