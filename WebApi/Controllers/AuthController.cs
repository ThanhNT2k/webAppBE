using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;

namespace ComicBackend.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly TokenService _tokenService;

        public AuthController(TokenService tokenService)
        {
            _tokenService = tokenService;
        }

        // 1. CẬP NHẬT: API ĐĂNG KÝ (Chống sập 500 & Map an toàn) -> POST: api/auth/register
        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterModel model)
        {
            try
            {
                // Kiểm tra an toàn để không bao giờ bị lỗi NullReferenceException
                if (model == null || string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Password))
                {
                    return BadRequest(new { error = "Vui lòng điền đầy đủ thông tin Email và Mật khẩu!" });
                }

                // Lấy tên hiển thị an toàn (nếu username trống thì lấy phần trước dấu @ của email)
                string safeUsername = !string.IsNullOrEmpty(model.Username) 
                    ? model.Username 
                    : model.Email.Split('@')[0];

                // Sinh mock token thông luồng
                var mockToken = _tokenService.GenerateToken(safeUsername, "User");

                // Trả về đúng định dạng JSON viết thường/hoa linh hoạt mà Frontend (auth.js) đang đợi
                return Ok(new
                {
                    token = mockToken,
                    user = new
                    {
                        username = safeUsername,
                        email = model.Email,
                        role = "User"
                    }
                });
            }
            catch (Exception ex)
            {
                // Nếu có lỗi phát sinh trong _tokenService, server vẫn trả về phản hồi 500 kèm CORS hợp lệ thay vì sập ngầm
                return StatusCode(500, new { error = $"Lỗi xử lý Đăng ký trên Server: {ex.Message}" });
            }
        }

        // 2. CẬP NHẬT: API ĐĂNG NHẬP -> POST: api/auth/login
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginModel model)
        {
            try
            {
                if (model == null || (string.IsNullOrEmpty(model.Username) && string.IsNullOrEmpty(model.Email)) || string.IsNullOrEmpty(model.Password))
                {
                    return BadRequest(new { error = "Vui lòng điền tài khoản và mật khẩu!" });
                }

                // Chấp nhận đăng nhập bằng cả username lẫn email từ frontend gửi về
                var identifier = !string.IsNullOrEmpty(model.Username) ? model.Username : model.Email;

                // Đoạn này dùng để demo test luồng admin mặc định
                if ((identifier == "admin" || identifier == "admin@gmail.com") && model.Password == "password123")
                {
                    var token = _tokenService.GenerateToken(identifier, "Admin");
                    
                    return Ok(new { 
                        token = token, // Viết thường đồng bộ cho Frontend dễ đọc
                        user = new { username = identifier, role = "Admin" }
                    });
                }

                // Trả về BadRequest hoặc Unauthorized kèm JSON object thay vì Text thuần để bảo vệ Frontend khỏi crash
                return BadRequest(new { error = "Sai tài khoản hoặc mật khẩu" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Lỗi xử lý Đăng nhập trên Server: {ex.Message}" });
            }
        }

        // API yêu cầu phải có Token hợp lệ mới truy cập được
        [HttpGet("protected-data")]
        [Authorize] 
        public IActionResult GetProtectedData()
        {
            return Ok(new { Message = "Chúc mừng! Bạn đã truy cập được dữ liệu bảo mật." });
        }
        
        // API chỉ dành riêng cho Admin
        [HttpGet("admin-only")]
        [Authorize(Roles = "Admin")] 
        public IActionResult GetAdminData()
        {
            return Ok(new { Message = "Chỉ Admin mới thấy dòng này." });
        }
    }

    // --- Cấu trúc Models bổ sung dữ liệu trống phòng lỗi mapping ---
    public class RegisterModel
    {
        public string? Username { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class LoginModel
    {
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string Password { get; set; } = string.Empty;
    }
}