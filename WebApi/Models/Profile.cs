using Postgrest.Attributes;
using Postgrest.Models;

namespace ComicBackend.WebApi.Models
{
    [Table("profiles")]
    public class Profile : BaseModel
    {
        [PrimaryKey("id", false)]
        public string Id { get; set; } = null!;

        [Column("username")]
        public string? Username { get; set; }

        [Column("avatar_url")]
        public string? AvatarUrl { get; set; }

        [Column("role")]
        public string Role { get; set; } = "User"; // 'User', 'Uploader', 'Admin'

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }
    }
}