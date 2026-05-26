using Supabase;

namespace ComicBackend.WebApi.Services
{
    public class SupabaseService
    {
        public Client Client { get; }

        public SupabaseService(IConfiguration configuration)
        {
            var url = configuration["Supabase:Url"];
            var key = configuration["Supabase:AnonKey"];

            var options = new SupabaseOptions
            {
                AutoRefreshToken = true,
                AutoConnectRealtime = true
            };

            Client = new Client(url, key, options);
            Client.InitializeAsync().Wait();
        }
    }
}