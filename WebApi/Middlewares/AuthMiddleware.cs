using Microsoft.AspNetCore.Http;
using ComicBackend.WebApi.Services;
using ComicBackend.WebApi.Models;
using Postgrest;
using System.Linq;
using System.Threading.Tasks;

namespace ComicBackend.WebApi.Middlewares
{
    public class SupabaseAuthMiddleware
    {
        private readonly RequestDelegate _next;

        public SupabaseAuthMiddleware(RequestDelegate next) => _next = next;

        public async Task InvokeAsync(HttpContext context, SupabaseService supabaseService)
        {
            var authHeader = context.Request.Headers["Authorization"].ToString();

            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
            {
                var token = authHeader.Substring(7);
                try
                {
                    // 1. Lấy thông tin User từ Supabase Auth (GoTrue)
                    var user = await supabaseService.Client.Auth.GetUser(token);
                    
                    if (user != null)
                    {
                        context.Items["User"] = user;

                        // 2. Lấy Role từ bảng 'profiles'
                        // Lưu ý: Dùng UserProfile thay vì User
                        var profileResponse = await supabaseService.Client
                            .From<UserProfile>()
                            .Select(x => new object[] { x.Id, x.Role })
                            .Filter("id", Postgrest.Constants.Operator.Equals, user.Id)
                            .Single();

                        // 3. Gán Role
                        context.Items["UserRole"] = profileResponse?.Role ?? "User";
                    }
                }
                catch
                {
                    // Token hết hạn hoặc lỗi kết nối, không làm gì cả
                }
            }

            await _next(context);
        }
    }
}