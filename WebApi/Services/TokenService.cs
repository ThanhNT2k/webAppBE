using System;
using System.Text;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

public class TokenService
{
    private readonly IConfiguration _configuration;

    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(string username, string role)
    {
        // 1. ĐỌC CẤU HÌNH AN TOÀN TRÊN RENDER & LOCAL
        // Thử đọc theo kiểu phân cấp trước, nếu null thì chủ động quét trực tiếp từ Env Variables
        string? secret = _configuration["Jwt:Secret"] ?? _configuration["Jwt__Secret"];
        string? issuer = _configuration["Jwt:Issuer"] ?? _configuration["Jwt__Issuer"];
        string? audience = _configuration["Jwt:Audience"] ?? _configuration["Jwt__Audience"];
        string? expiryStr = _configuration["Jwt:ExpiryInMinutes"] ?? _configuration["Jwt__ExpiryInMinutes"];

        // 2. KHU VỰC CỨU HỘ (FALLBACK VALUES): Tự động gán nếu bị Null
        if (string.IsNullOrEmpty(secret))
        {
            // Key bắt buộc phải dài trên 32 ký tự để thuật toán HmacSha256 không bị crash lỗi bảo mật
            secret = "Chuoi_Bi_Mat_Du_Phong_Sieu_Cap_Cua_CMCTruyen_Nam_2026_Chong_Sap_He_Thong";
        }
        if (string.IsNullOrEmpty(issuer))
        {
            issuer = "ComicBackend";
        }
        if (string.IsNullOrEmpty(audience))
        {
            audience = "ComicFrontend";
        }

        // Xử lý an toàn cho hàm double.Parse, tránh lỗi FormatException
        double expiryInMinutes = 60; // Mặc định thời hạn token là 60 phút nếu lỗi cấu hình
        if (!string.IsNullOrEmpty(expiryStr) && double.TryParse(expiryStr, out double parsedExpiry))
        {
            expiryInMinutes = parsedExpiry;
        }

        // 3. KHỞI TẠO KEY BẢO MẬT
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));

        // Các thông tin nhúng vào token (Claims)
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        // 4. KHỞI TẠO TOKEN KHÔNG LO BỊ CRASH
        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryInMinutes),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}