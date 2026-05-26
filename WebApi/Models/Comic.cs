using Postgrest.Attributes;
using Postgrest.Models;
using System;

namespace ComicBackend.WebApi.Models
{
    [Table("comics")]
    public class Comic : BaseModel
    {
        [PrimaryKey("id", true)] // BIGSERIAL tự động tăng
        public long Id { get; set; }

        [Column("title")]
        public string Title { get; set; } = null!;

        [Column("slug")]
        public string Slug { get; set; } = null!;

        [Column("cover_url")]
        public string? CoverUrl { get; set; }

        [Column("description")]
        public string? Description { get; set; }

        [Column("author")]
        public string? Author { get; set; }

        [Column("status")]
        public string Status { get; set; } = "Ongoing";

        [Column("total_views")]
        public long TotalViews { get; set; }

        [Column("uploader_id")]
        public string? UploaderId { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}