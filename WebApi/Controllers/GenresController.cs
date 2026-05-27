using Microsoft.AspNetCore.Mvc;
using ComicBackend.WebApi.Services; // Thư mục chứa SupabaseService của bạn
using ComicBackend.WebApi.Models;
using System.Threading.Tasks;

namespace ComicBackend.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GenresController : ControllerBase
    {
        private readonly SupabaseService _supabaseService;

        public GenresController(SupabaseService supabaseService)
        {
            _supabaseService = supabaseService;
        }

        [HttpGet]
        public async Task<IActionResult> GetGenres()
        {
            try
            {
                // Gọi qua Supabase SDK, tương thích 100% với BaseModel và không sinh lỗi SQL bậy
                var response = await _supabaseService.Client
                    .From<Genre>()
                    .Get();

                return Ok(response.Models);
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống khi lấy danh sách thể loại", error = ex.Message });
            }
        }
    }
}