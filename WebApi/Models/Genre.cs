using Postgrest.Attributes;
using Postgrest.Models;

namespace ComicBackend.WebApi.Models
{
    [Table("genres")]
    public class Genre : BaseModel
    {
        [PrimaryKey("id", true)] // true nghĩa là SERIAL tự động tăng
        public int Id { get; set; }

        [Column("name")]
        public string Name { get; set; } = null!;

        [Column("slug")]
        public string Slug { get; set; } = null!;
    }
}