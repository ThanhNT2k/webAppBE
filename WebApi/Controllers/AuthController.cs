using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ComicBackend.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly TokenService _tokenService;

        // Nếu bạn cần dùng DB Postgres sau này, hãy inject ApplicationDbContext vào đây
        public AuthController(TokenService tokenService)
        {
            _tokenService = tokenService;
        }

        // 1. BỔ SUNG: API ĐĂNG KÝ (Sign Up) -> POST: api/auth/register
        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterModel model)
        {
            // Thực tế: Kiểm tra DB xem email/username đã tồn tại chưa, mã hóa password rồi lưu vào DB.
            // Ví dụ mock tạm thời để Frontend test thông luồng thành công:
            if (string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Password))
            {
                return BadRequest(new { error = "Vui lòng điền đầy đủ thông tin!" });
            }

            // Giả lập trả về dữ liệu chuẩn cấu trúc mà file auth.js của bạn đang đợi:
            // auth.js dòng 38: mong muốn nhận về response có { token, user: { role } }
            var mockToken = _tokenService.GenerateToken(model.Username ?? model.Email, "User");

            return Ok(new
            {
                token = mockToken,
                user = new
                {
                    username = model.Username ?? model.Email,
                    email = model.Email,
                    role = "User"
                }
            });
        }

        // 2. CẬP NHẬT: API ĐĂNG NHẬP (Sign In) -> POST: api/auth/login
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginModel model)
        {
            // Chấp nhận đăng nhập bằng cả username lẫn email từ frontend gửi về
            var identifier = !string.IsNullOrEmpty(model.Username) ? model.Username : model.Email;

            // Đoạn này dùng để demo, thực tế bạn phải kiểm tra DB
            if ((identifier == "admin" || identifier == "admin@gmail.com") && model.Password == "password123")
            {
                var token = _tokenService.GenerateToken(identifier, "Admin");
                
                // Trả về đúng cấu trúc file auth.js yêu cầu (gồm token và object user)
                return Ok(new { 
                    Token = token,
                    User = new { username = identifier, role = "Admin" }
                });
            }

            return Unauthorized(new { error = "Sai tài khoản hoặc mật khẩu" });
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

    // --- Cấu trúc Models đồng bộ với JSON từ Frontend gửi lên ---
    public class RegisterModel
    {
        public string? Username { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class LoginModel
    {
        public string? Username { get; set; }
        public string? Email { get; set; }     // Bổ sung thêm trường Email phòng khi Frontend gửi email thay vì username
        public string Password { get; set; } = string.Empty;
    }
}