using System;
using System.ComponentModel.DataAnnotations;
using Postgrest.Models;
using Postgrest.Attributes; // Đảm bảo đã import namespace này

namespace ComicBackend.WebApi.Models
{
    [Table("profiles")] // Postgrest.Attributes.Table
    public class User : BaseModel
    {
        [PrimaryKey("id")] // Postgrest.Attributes.PrimaryKey
        public Guid Id { get; set; } = Guid.NewGuid();

        [Column("display_name")] // Postgrest.Attributes.Column
        public string DisplayName { get; set; } = string.Empty;

        [Column("avatar_url")]
        public string? AvatarUrl { get; set; }

        [Column("role")]
        public string Role { get; set; } = "user"; // Nên dùng chữ thường để khớp với DB

        [Column("birth_date")]
        public DateTime? BirthDate { get; set; }

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;
        
        [Column("email")] // 🌟 ĐÂY LÀ CHÌA KHÓA
        public string Email { get; set; } = string.Empty;

        [Column("password_hash")]
        public string PasswordHash { get; set; } = string.Empty;
    }
}