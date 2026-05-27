using System.ComponentModel.DataAnnotations; // Giữ lại cho EF Core nếu cần
using Postgrest.Attributes; // BẮT BUỘC PHẢI CÓ DÒNG NÀY ĐỂ CHẠY SUPABASE SDK

namespace ComicBackend.WebApi.Models
{
    // Sử dụng thẻ [Table] của Postgrest để ép Supabase SDK gọi xuống bảng "genres" viết thường
    [Postgrest.Attributes.Table("genres")] 
    public class Genre : Postgrest.Models.BaseModel
    {
        [PrimaryKey("id", false)] // Thuộc tính khóa chính của Supabase SDK (false = tự tăng)
        public int Id { get; set; }

        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [Column("slug")]
        public string Slug { get; set; } = string.Empty;
    }
}