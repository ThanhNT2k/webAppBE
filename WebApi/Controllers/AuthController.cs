using Microsoft.AspNetCore.Mvc;
using ComicBackend.WebApi.Services;
using ComicBackend.WebApi.Models;
using Supabase.Gotrue;

namespace ComicBackend.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly SupabaseService _supabase;

        public AuthController(SupabaseService supabase) => _supabase = supabase;

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest req)
        {
            var response = await _supabase.Client.Auth.SignUp(req.Email, req.Password, new SignUpOptions {
                Data = new Dictionary<string, object> { { "username", req.Username ?? req.Email.Split('@')[0] } }
            });

            if (response.Error != null) return BadRequest(new { error = response.Error.Message });
            
            return Ok(new { message = "Đăng ký thành công! Vui lòng kiểm tra email (nếu cần)." });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest req)
        {
            try
            {
                var response = await _supabase.Client.Auth.SignIn(req.Email, req.Password);

                if (response == null)
                    return Unauthorized(new { error = "Tài khoản hoặc mật khẩu không chính xác!" });

                if (response.Error != null)
                    return Unauthorized(new { error = response.Error.Message });

                if (response.User == null)
                    return Unauthorized(new { error = "Tài khoản hoặc mật khẩu không chính xác!" });

                var accessToken = response.Session?.AccessToken;

                return Ok(new
                {
                    token = accessToken,
                    user = new { id = response.User.Id, email = response.User.Email }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}