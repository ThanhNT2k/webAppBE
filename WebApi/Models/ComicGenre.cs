using Postgrest.Attributes;
using Postgrest.Models;

namespace ComicBackend.WebApi.Models
{
    [Table("comic_genres")]
    public class ComicGenre : BaseModel
    {
        // Supabase-csharp yêu cầu chỉ định PrimaryKey, với bảng composite key ta chỉ định cột đầu tiên
        [PrimaryKey("comic_id", false)]
        public long ComicId { get; set; }

        [Column("genre_id")]
        public int GenreId { get; set; }
    }
}