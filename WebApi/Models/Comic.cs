using System;
using Postgrest.Attributes; // Bắt buộc phải có

namespace ComicBackend.WebApi.Models
{
    [Postgrest.Attributes.Table("comics")] // Ép Supabase SDK gọi trúng bảng "comics" viết thường
    public class Comic : Postgrest.Models.BaseModel
    {
        [PrimaryKey("id", false)]
        public int Id { get; set; }

        [Column("title")]
        public string Title { get; set; } = string.Empty;

        [Column("slug")]
        public string Slug { get; set; } = string.Empty;

        [Column("author")]
        public string Author { get; set; } = string.Empty;

        [Column("status")]
        public string Status { get; set; } = "Ongoing";

        [Column("cover_url")]
        public string? CoverUrl { get; set; }

        [Column("views")]
        public int TotalViews { get; set; } = 0;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}