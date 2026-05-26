using Postgrest.Attributes;
using Postgrest.Models;
using System;

namespace ComicBackend.WebApi.Models
{
    [Table("reading_history")]
    public class ReadingHistory : BaseModel
    {
        [PrimaryKey("id", true)]
        public long Id { get; set; }

        [Column("user_id")]
        public string UserId { get; set; } = null!;

        [Column("comic_id")]
        public long ComicId { get; set; }

        [Column("chapter_id")]
        public long ChapterId { get; set; }

        [Column("read_at")]
        public DateTime ReadAt { get; set; } = DateTime.UtcNow;
    }
}