using Postgrest.Attributes;
using Postgrest.Models;
using System;

namespace ComicBackend.WebApi.Models
{
    [Table("chapters")]
    public class Chapter : BaseModel
    {
        [PrimaryKey("id", true)]
        public long Id { get; set; }

        [Column("comic_id")]
        public long ComicId { get; set; }

        [Column("chapter_number")]
        public float ChapterNumber { get; set; } // Dùng float/double ứng với NUMERIC(6,1)

        [Column("chapter_name")]
        public string? ChapterName { get; set; }

        [Column("uploader_id")]
        public string? UploaderId { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}