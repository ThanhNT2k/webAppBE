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
        // Chủ động quét cả hai định dạng phân cấp (Local) và phẳng (Biến môi trường Linux trên Render)
        string? secret = _configuration["Jwt:Secret"] ?? _configuration["Jwt__Secret"];
        string? issuer = _configuration["Jwt:Issuer"] ?? _configuration["Jwt__Issuer"];
        string? audience = _configuration["Jwt:Audience"] ?? _configuration["Jwt__Audience"];
        string? expiryStr = _configuration["Jwt:ExpiryInMinutes"] ?? _configuration["Jwt__ExpiryInMinutes"];

        // 2. CHẶN ĐỨNG NGUY CƠ BẢO MẬT (GIẢI PHÁP 1)
        // Nếu không tìm thấy Secret Key từ hệ thống, lập tức dừng chương trình để bảo vệ an toàn
        if (string.IsNullOrEmpty(secret))
        {
            throw new InvalidOperationException("CRITICAL ERROR: JWT Secret Key chưa được cấu hình trong Environment Variables trên Render hoặc appsettings.json!");
        }

        // 3. GÁN GIÁ TRỊ DỰ PHÒNG CHO CÁC THAM SỐ KHÔNG NHẠY CẢM
        if (string.IsNullOrEmpty(issuer))
        {
            issuer = "ComicBackend";
        }
        if (string.IsNullOrEmpty(audience))
        {
            audience = "ComicFrontend";
        }

        // Kiểm tra an toàn cho hàm double.Parse để phòng lỗi định dạng chuỗi rỗng
        double expiryInMinutes = 60; // Mặc định thời hạn token là 60 phút nếu thiếu cấu hình thời gian
        if (!string.IsNullOrEmpty(expiryStr) && double.TryParse(expiryStr, out double parsedExpiry))
        {
            expiryInMinutes = parsedExpiry;
        }

        // 4. KHỞI TẠO KEY BẢO MẬT TỪ KHÓA THẬT VÀ NHÚNG CLAIMS
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        // 5. BAN HÀNH TOKEN CHUẨN JWT
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