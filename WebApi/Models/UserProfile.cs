using Postgrest.Attributes;
using Postgrest.Models;

namespace ComicBackend.WebApi.Models
{
    [Table("profiles")] // Tên bảng trong Supabase
    public class UserProfile : BaseModel
    {
        [PrimaryKey("id")]
        public string Id { get; set; } = string.Empty;

        [Column("role")] // Cột vai trò trong bảng profiles
        public string Role { get; set; } = "user";
    }
}