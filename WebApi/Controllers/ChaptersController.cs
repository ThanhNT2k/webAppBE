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
    public class ChaptersController : ControllerBase
    {
        private readonly SupabaseService _supabaseService;

        public ChaptersController(SupabaseService supabaseService)
        {
            _supabaseService = supabaseService;
        }

        // =========================================================================
        // 1. API LẤY DANH SÁCH CHƯƠNG CỦA MỘT BỘ TRUYỆN (Theo ComicID)
        // =========================================================================
        // GET: api/chapters/comic/5
        // Đồng bộ hoàn hảo với hàm getChaptersByComic(comicId) ở file api.js
        [HttpGet("comic/{comicId}")]
        public async Task<IActionResult> GetChaptersByComic(long comicId)
        {
            try
            {
                // Truy vấn danh sách chương thuộc về truyện và sắp xếp chương mới nhất lên đầu
                var response = await _supabaseService.Client
                    .From<Chapter>()
                    .Where(ch => ch.ComicId == comicId)
                    .Order("chapter_number", Postgrest.Constants.Ordering.Descending) 
                    .Get();

                return Ok(response.Models);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống khi lấy danh sách chương truyện", error = ex.Message });
            }
        }

        // =========================================================================
        // 2. API LẤY CHI TIẾT NỘI DUNG ẢNH CỦA MỘT CHƯƠNG (Theo ChapterID)
        // =========================================================================
        // GET: api/chapters/12
        // Đồng bộ hoàn hảo với hàm getChapterImages(chapterId) ở file api.js
        [HttpGet("{chapterId}")]
        public async Task<IActionResult> GetChapterDetails(long chapterId)
        {
            try
            {
                var response = await _supabaseService.Client
                    .From<Chapter>()
                    .Where(ch => ch.Id == chapterId)
                    .Single(); // Chỉ lấy duy nhất chương đang đọc

                if (response == null)
                {
                    return NotFound(new { message = "Không tìm thấy nội dung chương truyện này!" });
                }

                // Trả về toàn bộ object chương truyện chứa mảng dữ liệu "images" để Front-end render ảnh
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống khi lấy nội dung chương truyện", error = ex.Message });
            }
        }
    }
}