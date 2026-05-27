using Microsoft.EntityFrameworkCore;
using ComicBackend.WebApi.Models;

namespace ComicBackend.WebApi.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Genre> Genres { get; set; }
        public DbSet<Comic> Comics { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. Loại bỏ cấu hình ClientOptions của Postgrest
            modelBuilder.Ignore<Postgrest.ClientOptions>();

            // 2. CÁCH FIX LỖI ÉP KIỂU: 
            // Quét và tự động cấu hình HasNoKey (Không cần khóa chính) một cách chuẩn xác cho EF Core
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (entityType.Name.Contains("ClientOptions") || entityType.Name.Contains("SupabaseOptions"))
                {
                    // Sử dụng trực tiếp Fluent API của EF Core thông qua định dạng chuỗi tên class 
                    // để cấu hình HasNoKey mà không bị lỗi ép kiểu dữ liệu phức tạp
                    modelBuilder.Entity(entityType.Name).HasNoKey();
                }
            }
        }
    }
}