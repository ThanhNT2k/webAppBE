using Microsoft.AspNetCore.Http;
using ComicBackend.WebApi.Services;
using System.Threading.Tasks;

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
                var token = authHeader.Substring("Bearer ".Length).Trim();
                try
                {
                    // Đưa token vào Supabase client để lấy thông tin user hiện tại
                    var user = await supabaseService.Client.Auth.GetUser(token);
                    
                    if (user != null)
                    {
                        // Lưu thông tin user id vào Items của HttpContext để sử dụng ở Controller
                        context.Items["User"] = user;
                    }
                }
                catch
                {
                    // Token không hợp lệ hoặc hết hạn - Bỏ qua để controller tự xử lý 401 nếu cần
                }
            }

            await _next(context);
        }
    }
}