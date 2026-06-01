using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly TokenService _tokenService;

    public AuthController(TokenService tokenService)
    {
        _tokenService = tokenService;
    }

    // API Đăng nhập để lấy Token
    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginModel model)
    {
        // Đoạn này dùng để demo, thực tế bạn phải kiểm tra DB
        if (model.Username == "admin" && model.Password == "password123")
        {
            var token = _tokenService.GenerateToken(model.Username, "Admin");
            return Ok(new { Token = token });
        }

        return Unauthorized("Sai tài khoản hoặc mật khẩu");
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

public class LoginModel
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}