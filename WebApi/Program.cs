using ComicBackend.WebApi.Services;
using ComicBackend.WebApi.Middlewares;
using Newtonsoft.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        // Giữ nguyên định dạng chữ thường đầu (camelCase) giống chuẩn API truyền thống
        options.SerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver();
        // Bỏ qua các vòng lặp tham chiếu nếu có
        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
    });

builder.Services.AddEndpointsApiExplorer();

// Đăng ký SupabaseService dưới dạng Singleton Độc nhất
builder.Services.AddSingleton<SupabaseService>();

// Cấu hình CORS để Frontend gọi được API
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// 2. Cấu hình HTTP Request Pipeline
app.UseCors("AllowAll");

app.UseHttpsRedirection();

// Kích hoạt Custom Middleware xác thực token Supabase trước khi đi vào Controllers
app.UseMiddleware<SupabaseAuthMiddleware>();

app.UseAuthorization();

app.MapControllers();

app.MapGet("/health", () => Results.Ok(new { status = "OK", message = "C# ASP.NET Core Backend is running perfectly!" }));

app.Run();