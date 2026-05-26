using Postgrest.Attributes;
using Postgrest.Models;
using System;

namespace ComicBackend.WebApi.Models
{
    [Table("profiles")]
    public class Profile : BaseModel
    {
        [PrimaryKey("id", false)] // false nghĩa là ID dạng UUID do Auth sinh ra, không tự tăng
        public string Id { get; set; } = null!;

        [Column("display_name")]
        public string DisplayName { get; set; } = "New Member";

        [Column("avatar_url")]
        public string? AvatarUrl { get; set; }

        [Column("role")]
        public string Role { get; set; } = "user";

        [Column("birth_date")]
        public DateTime? BirthDate { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}