using Microsoft.AspNetCore.Mvc;
using ComicBackend.WebApi.Services;
using ComicBackend.WebApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ComicBackend.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ComicsController : ControllerBase
    {
        private readonly SupabaseService _supabaseService;

        public ComicsController(SupabaseService supabaseService)
        {
            _supabaseService = supabaseService;
        }

        // GET: api/comics?page=1&limit=12&sortBy=created_at&status=Ongoing&genreId=3
        // API lấy danh sách truyện công khai có hỗ trợ phân trang, lọc trạng thái, sắp xếp và lọc thể loại
        [HttpGet]
        public async Task<IActionResult> GetComics(
            [FromQuery] int page = 1, 
            [FromQuery] int limit = 12, 
            [FromQuery] string? status = null, 
            [FromQuery] string sortBy = "created_at",
            [FromQuery] int? genreId = null)
        {
            try
            {
                // Ép kiểu tường minh về Postgrest.Table để đồng bộ xuyên suốt chuỗi hàm xử lý truy vấn phía dưới
                var query = (Postgrest.Table<Comic>)_supabaseService.Client.From<Comic>();

                // 1. XỬ LÝ LỌC THEO THỂ LOẠI (BẢNG TRUNG GIAN COMIC_GENRES TRÊN SƠ ĐỒ)
                if (genreId.HasValue)
                {
                    // Tìm danh sách các comic_id thuộc về genre_id này trong bảng trung gian comic_genres
                    var mappingResponse = await _supabaseService.Client
                        .From<ComicGenre>()
                        .Where(cg => cg.GenreId == genreId.Value)
                        .Get();

                    var comicIds = mappingResponse.Models.Select(cg => (object)cg.ComicId).ToList();

                    // Nếu thể loại được chọn chưa có bất kỳ bộ truyện nào, trả về danh sách rỗng lập tức
                    if (!comicIds.Any())
                    {
                        return Ok(new List<Comic>());
                    }

                    // Lọc những bộ truyện nằm trong tập hợp danh sách IDs tìm được
                    query = (Postgrest.Table<Comic>)query.Filter("id", Postgrest.Constants.Operator.In, comicIds);
                }

                // 2. XỬ LÝ LỌC TRẠNG THÁI (Ongoing / Completed từ api.js)
                if (!string.IsNullOrEmpty(status))
                {
                    query = (Postgrest.Table<Comic>)query.Where(c => c.Status == status);
                }

                // 3. XỬ LÝ SẮP XẾP DỮ LIỆU ĐỘNG (sortBy = total_views hoặc created_at)
                if (sortBy == "total_views")
                {
                    // Map trực tiếp với cột 'views' viết thường dưới DB Supabase thông qua cấu hình của Model Comic
                    query = (Postgrest.Table<Comic>)query.Order("views", Postgrest.Constants.Ordering.Descending);
                }
                else
                {
                    query = (Postgrest.Table<Comic>)query.Order("created_at", Postgrest.Constants.Ordering.Descending);
                }

                // 4. XỬ LÝ PHÂN TRANG (Thuật toán dịch chuyển vị trí bản ghi Skip / Take của Supabase SDK)
                int offset = (page - 1) * limit;
                query = (Postgrest.Table<Comic>)query.Range(offset, offset + limit - 1);

                // 5. Thực thi truy vấn đẩy kết quả về Front-end
                var response = await query.Get();
                return Ok(response.Models);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống khi truy vấn danh sách truyện", error = ex.Message });
            }
        }

        // GET: api/comics/{slug}
        // API lấy chi tiết một bộ truyện dựa trên đường dẫn định danh dạng Slug (Ví dụ: solo-leveling)
        [HttpGet("{slug}")]
        public async Task<IActionResult> GetComicBySlug(string slug)
        {
            try
            {
                var response = await _supabaseService.Client
                    .From<Comic>()
                    .Where(c => c.Slug == slug)
                    .Single(); // Lấy duy nhất một bản ghi có slug trùng khớp

                if (response == null)
                {
                    return NotFound(new { message = "Không tìm thấy bộ truyện tranh này!" });
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống khi lấy chi tiết truyện", error = ex.Message });
            }
        }
    }
}