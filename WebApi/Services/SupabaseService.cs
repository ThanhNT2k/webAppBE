using Supabase;

namespace ComicBackend.WebApi.Services
{
    public class SupabaseService
    {
        private readonly Client _client;

        public SupabaseService(IConfiguration configuration)
        {
            var url = configuration["Supabase:Url"];
            var key = configuration["Supabase:Key"];

            var options = new SupabaseOptions
            {
                AutoRefreshToken = true,
                AutoConnectRealtime = true
            };

            // Khởi tạo client
            _client = new Client(url, key, options);
        }

        // Cung cấp một property để các Controller khác sử dụng
        public Client Client => _client;
    }
}