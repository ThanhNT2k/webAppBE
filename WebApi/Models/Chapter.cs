using System;
using Postgrest.Attributes;
using Postgrest.Models;

namespace ComicBackend.WebApi.Models
{
    [Table("chapters")] // Map chính xác với tên bảng viết thường dưới Supabase
    public class Chapter : BaseModel
    {
        [PrimaryKey("id", false)]
        public long Id { get; set; }

        [Column("comic_id")]
        public long ComicId { get; set; }

        [Column("title")] // Tiêu đề chương (Ví dụ: "Chương 1: Khởi đầu")
        public string Title { get; set; } = string.Empty;

        [Column("chapter_number")] // Số chương phục vụ việc sắp xếp tăng/giảm dần (Ví dụ: 1, 1.5, 2)
        public double ChapterNumber { get; set; }

        // Tùy thuộc vào DB của bạn: Trường lưu danh sách ảnh của chương truyện
        // Thường lưu ở dạng JSON hoặc Mảng chuỗi các link ảnh trong PostgreSQL (List<string>)
        [Column("images")] 
        public List<string> Images { get; set; } = new List<string>();

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}