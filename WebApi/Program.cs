using ComicBackend.WebApi.Services;
using ComicBackend.WebApi.Middlewares;
using Microsoft.EntityFrameworkCore;
using ComicBackend.WebApi.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 1. Cấu hình Supabase (Service quan trọng nhất)
builder.Services.AddSingleton<SupabaseService>();

// 2. Database & Auth
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// 3. JWT Setup (Giữ nguyên nếu bạn vẫn muốn dùng JWT cục bộ)
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => {
        options.TokenValidationParameters = new TokenValidationParameters {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"])),
            ValidateIssuer = false, // Supabase quản lý token, nên để false cho dễ quản lý
            ValidateAudience = false
        };
    });

builder.Services.AddControllers()
    .AddNewtonsoftJson(options => {
        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
    });

// 4. CORS
builder.Services.AddCors(options => {
    options.AddPolicy("AllowAll", p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

// 5. Middleware Pipeline
app.UseCors("AllowAll");
app.UseHttpsRedirection();

// Chỗ này quan trọng: Middleware này phải xử lý được User từ Supabase mà không cần class Profile cũ
app.UseAuthentication(); 
app.UseMiddleware<SupabaseAuthMiddleware>(); 
app.UseAuthorization();

app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "OK" }));

app.Run();