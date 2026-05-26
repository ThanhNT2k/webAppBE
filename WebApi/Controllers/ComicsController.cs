using Microsoft.AspNetCore.Mvc;
using ComicBackend.WebApi.Services;
using ComicBackend.WebApi.Models;
using static Postgrest.Constants;

namespace ComicBackend.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ComicsController : ControllerBase
    {
        private readonly SupabaseService _supabase;

        public ComicsController(SupabaseService supabase)
        {
            _supabase = supabase;
        }

        // GET: api/comics
        [HttpGet]
        public async Task<IActionResult> GetComics(
            [FromQuery] int page = 1, 
            [FromQuery] int limit = 12,
            [FromQuery] string? status = null,
            [FromQuery] string sortBy = "created_at")
        {
            try
            {
                int from = (page - 1) * limit;
                int to = from + limit - 1;

                // Sử dụng Fluent API viết liền mạch để tránh lỗi không đồng nhất kiểu dữ liệu Table
                var query = _supabase.Client.From<Comic>();

                if (!string.IsNullOrEmpty(status))
                {
                    query.Filter(x => x.Status!, Operator.Equals, status);
                }

                if (sortBy == "total_views")
                {
                    query.Order(x => x.TotalViews, Ordering.Descending);
                }
                else
                {
                    query.Order(x => x.CreatedAt, Ordering.Descending);
                }

                // Thực thi Range và lấy dữ liệu trực tiếp từ query đã build chuỗi
                var response = await query.Range(from, to).Get();

                return Ok(new 
                {
                    data = response.Models,
                    page,
                    limit
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // GET: api/comics/{slug}
        [HttpGet("{slug}")]
        public async Task<IActionResult> GetComicBySlug(string slug)
        {
            try
            {
                var response = await _supabase.Client
                    .From<Comic>()
                    .Filter(x => x.Slug!, Operator.Equals, slug)
                    .Single();

                if (response == null) 
                    return NotFound(new { error = "Comic not found" });

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // GET: api/comics/genres
        [HttpGet("genres")]
        public async Task<IActionResult> GetGenres()
        {
            try
            {
                var response = await _supabase.Client
                    .From<Genre>()
                    .Order(x => x.Name!, Ordering.Ascending)
                    .Get();

                return Ok(response.Models);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}