using Postgrest.Attributes;
using Postgrest.Models;

namespace ComicBackend.WebApi.Models
{
    [Table("comic_genres")]
    public class ComicGenre : BaseModel
    {
        [Column("comic_id")]
        public long ComicId { get; set; }

        [Column("genre_id")]
        public int GenreId { get; set; }
    }
}