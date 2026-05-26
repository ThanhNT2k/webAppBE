using Microsoft.AspNetCore.Mvc;
using ComicBackend.WebApi.Services;
using ComicBackend.WebApi.Models;
using static Postgrest.Constants;

namespace ComicBackend.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChaptersController : ControllerBase
    {
        private readonly SupabaseService _supabase;

        public ChaptersController(SupabaseService supabase)
        {
            _supabase = supabase;
        }

        // GET: api/chapters/comic/{comicId}
        [HttpGet("comic/{comicId}")]
        public async Task<IActionResult> GetChaptersByComic(long comicId)
        {
            try
            {
                var response = await _supabase.Client
                    .From<Chapter>()
                    .Filter(x => x.ComicId, Operator.Equals, comicId)
                    .Order(x => x.ChapterNumber, Ordering.Descending)
                    .Get();

                return Ok(response.Models);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // GET: api/chapters/{chapterId}/images
        [HttpGet("{chapterId}/images")]
        public async Task<IActionResult> GetChapterImages(long chapterId)
        {
            try
            {
                var response = await _supabase.Client
                    .From<ChapterImage>()
                    .Filter(x => x.ChapterId, Operator.Equals, chapterId)
                    .Order(x => x.PageNumber, Ordering.Ascending)
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