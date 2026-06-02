using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ComicBackend.WebApi.Data;
using ComicBackend.WebApi.Models;
using System;
using System.Threading.Tasks;

namespace ComicBackend.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly TokenService _tokenService;
        private readonly ApplicationDbContext _context; // Khai báo DbContext

        // Inject cả TokenService và ApplicationDbContext vào đây
        public AuthController(TokenService tokenService, ApplicationDbContext context)
        {
            _tokenService = tokenService;
            _context = context;
        }

        // 1. API ĐĂNG KÝ THẬT -> Ghi vào bảng profiles trong DB
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            try
            {
                // 1. Kiểm tra dữ liệu đầu vào từ Frontend
                if (model == null)
                {
                    return BadRequest(new { error = "Dữ liệu gửi lên bị rỗng (null)!" });
                }

                if (string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Password))
                {
                    return BadRequest(new { error = "Vui lòng điền đầy đủ thông tin Email và Mật khẩu!" });
                }

                // Xử lý display_name an toàn nếu frontend gửi trống
                string safeDisplayName = !string.IsNullOrEmpty(model.Username) 
                    ? model.Username 
                    : model.Email.Split('@')[0];

                // 2. Kiểm tra trùng lặp Email hoặc DisplayName trong bảng profiles
                if (await _context.Profiles.AnyAsync(p => p.Email == model.Email.ToLower()))
                {
                    return BadRequest(new { error = "Email này đã được đăng ký sử dụng!" });
                }
                if (await _context.Profiles.AnyAsync(p => p.DisplayName == safeDisplayName))
                {
                    return BadRequest(new { error = "Tên hiển thị (Username) này đã tồn tại!" });
                }

                // 3. Mã hóa mật khẩu bảo mật (Cần cài package: BCrypt.Net-Next)
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.Password);

                // 4. Khởi tạo Object User khớp với cấu trúc bảng profiles thực tế
                var newProfile = new User
                {
                    Id = Guid.NewGuid(), // Sinh chuỗi UUID mới hoàn toàn cho Postgres
                    DisplayName = safeDisplayName,
                    Email = model.Email.ToLower(),
                    PasswordHash = hashedPassword,
                    Role = "User", // Mặc định gán Role là User
                    UpdatedAt = DateTime.UtcNow
                };

                // Lưu thực tế xuống Database
                _context.Profiles.Add(newProfile);
                await _context.SaveChangesAsync();

                // 5. Sinh mã Token từ DisplayName và Role thật vừa tạo
                string mockToken = string.Empty;
                try 
                {
                    mockToken = _tokenService.GenerateToken(newProfile.DisplayName, newProfile.Role);
                }
                catch (Exception tokenEx)
                {
                    return StatusCode(500, new { error = $"Tài khoản đã tạo nhưng lỗi sập tại TokenService: {tokenEx.Message}." });
                }

                // 6. Trả về kết quả thông suốt cho Frontend
                return Ok(new
                {
                    token = mockToken,
                    user = new
                    {
                        username = newProfile.DisplayName,
                        email = newProfile.Email,
                        role = newProfile.Role
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Lỗi xử lý Đăng ký trên Database: {ex.Message}" });
            }
        }

        // 2. API ĐĂNG NHẬP THẬT -> Kiểm tra đối chiếu với Database
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            try
            {
                if (model == null || (string.IsNullOrEmpty(model.Username) && string.IsNullOrEmpty(model.Email)) || string.IsNullOrEmpty(model.Password))
                {
                    return BadRequest(new { error = "Vui lòng điền đầy đủ thông tin tài khoản và mật khẩu!" });
                }

                var identifier = !string.IsNullOrEmpty(model.Username) ? model.Username.ToLower() : model.Email.ToLower();

                // Truy vấn tìm User trong bảng profiles bằng Email hoặc display_name
                var account = await _context.Profiles.FirstOrDefaultAsync(p => p.Email == identifier || p.DisplayName.ToLower() == identifier);

                // Nếu không tìm thấy hoặc mật khẩu giải mã so khớp thất bại
                if (account == null || !BCrypt.Net.BCrypt.Verify(model.Password, account.PasswordHash))
                {
                    return BadRequest(new { error = "Tài khoản hoặc mật khẩu không chính xác!" });
                }

                // Đăng nhập thành công -> Sinh Token thật từ dữ liệu DB
                var token = _tokenService.GenerateToken(account.DisplayName, account.Role);
                
                return Ok(new { 
                    token = token,
                    user = new { username = account.DisplayName, role = account.Role }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Lỗi xử lý Đăng nhập trên Database: {ex.Message}" });
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

    // --- Cấu trúc Models giữ nguyên để Frontend map dữ liệu ---
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