using Microsoft.AspNetCore.Http;
using ComicBackend.WebApi.Services;
using ComicBackend.WebApi.Models;
using Postgrest;
using System.Threading.Tasks;
using System.Linq; // 🌟 Thêm thư viện này để sử dụng FirstOrDefault()

namespace ComicBackend.WebApi.Middlewares
{
    public class SupabaseAuthMiddleware
    {
        private readonly RequestDelegate _next;

        public SupabaseAuthMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, SupabaseService supabaseService)
        {
            var authHeader = context.Request.Headers["Authorization"].ToString();

            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
            {
                var token = authHeader.Substring(7);
                try
                {
                    // 1. Xác thực token với GoTrue Supabase
                    var user = await supabaseService.Client.Auth.GetUser(token);
                    
                    if (user != null)
                    {
                        context.Items["User"] = user;

                        // 2. Lấy Role của User từ bảng public.profiles thông qua Model User
                        // Sử dụng Guid.Parse nếu Id trong authState là string
                        var profileResponse = await supabaseService.Client
                            .From<User>()
                            .Filter(x => x.Id, Constants.Operator.Equals, System.Guid.Parse(user.Id))
                            .Get();

                        var userProfile = profileResponse.Models.FirstOrDefault();
                        
                        // 3. Lưu Role vào HttpContext
                        context.Items["UserRole"] = userProfile?.Role ?? "User";
                    }
                }
                catch
                {
                    // Token lỗi hoặc hết hạn -> Bỏ qua
                }
            }

            await _next(context);
        }
    }
}