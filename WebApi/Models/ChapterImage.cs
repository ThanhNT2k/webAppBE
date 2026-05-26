using Postgrest.Attributes;
using Postgrest.Models;

namespace ComicBackend.WebApi.Models
{
    [Table("chapter_images")]
    public class ChapterImage : BaseModel
    {
        [PrimaryKey("id", true)]
        public long Id { get; set; }

        [Column("chapter_id")]
        public long ChapterId { get; set; }

        [Column("page_number")]
        public int PageNumber { get; set; }

        [Column("image_url")]
        public string ImageUrl { get; set; } = null!;
    }
}