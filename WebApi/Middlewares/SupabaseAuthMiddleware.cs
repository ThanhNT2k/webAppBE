using Microsoft.AspNetCore.Http;
using ComicBackend.WebApi.Services;
using ComicBackend.WebApi.Models;
using Postgrest;
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
                var token = authHeader.Substring(7);
                try
                {
                    // 1. Xác thực token với GoTrue Supabase
                    var authState = await supabaseService.Client.Auth.GetUser(token);
                    
                    if (authState != null)
                    {
                        context.Items["User"] = authState;

                        // 2. Lấy Role của User từ bảng public.profiles
                        var profileResponse = await supabaseService.Client
                            .From<User>()
                            .Filter(x => x.Id, Constants.Operator.Equals, authState.Id)
                            .Get();

                        var profile = profileResponse.Models.FirstOrDefault();
                        
                        // 3. Lưu Role vào HttpContext để các Custom Attribute kiểm tra
                        context.Items["UserRole"] = profile?.Role ?? "User";
                    }
                }
                catch
                {
                    // Token lỗi hoặc hết hạn -> Bỏ qua, để Attribute tự xử lý Unauthorized
                }
            }

            await _next(context);
        }
    }
}