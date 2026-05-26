using Postgrest.Attributes;
using Postgrest.Models;
using System;

namespace ComicBackend.WebApi.Models
{
    [Table("comments")]
    public class Comment : BaseModel
    {
        [PrimaryKey("id", true)]
        public long Id { get; set; }

        [Column("user_id")]
        public string UserId { get; set; } = null!;

        [Column("comic_id")]
        public long ComicId { get; set; }

        [Column("chapter_id")]
        public long? ChapterId { get; set; }

        [Column("content")]
        public string Content { get; set; } = null!;

        [Column("parent_id")]
        public long? ParentId { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}