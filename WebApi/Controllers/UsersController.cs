using Microsoft.AspNetCore.Mvc;
using ComicBackend.WebApi.Services;
using ComicBackend.WebApi.Models;
using static Postgrest.Constants;

namespace ComicBackend.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly SupabaseService _supabase;

        public UsersController(SupabaseService supabase)
        {
            _supabase = supabase;
        }

        public record FollowRequest(long ComicId);
        public record CommentRequest(long ComicId, long? ChapterId, string Content, long? ParentId);
        public record HistoryRequest(long ComicId, long ChapterId);

        private Supabase.Gotrue.User? GetCurrentUser()
        {
            return HttpContext.Items["User"] as Supabase.Gotrue.User;
        }

        // POST: api/users/follow
        [HttpPost("follow")]
        public async Task<IActionResult> ToggleFollow([FromBody] FollowRequest request)
        {
            var user = GetCurrentUser();
            if (user == null) return Unauthorized(new { error = "You must be logged in to follow comics." });

            try
            {
                // Dùng .Get() kết hợp với Models.FirstOrDefault() để thay thế cho MaybeSingle()
                var response = await _supabase.Client
                    .From<Follow>()
                    .Filter(x => x.UserId!, Operator.Equals, user.Id)
                    .Filter(x => x.ComicId, Operator.Equals, request.ComicId)
                    .Get();

                var existing = response.Models.FirstOrDefault();

                if (existing != null)
                {
                    // Đã theo dõi -> Tiến hành hủy theo dõi (Unfollow)
                    await _supabase.Client.From<Follow>().Delete(existing);
                    return Ok(new { status = "unfollowed", message = "Unfollowed successfully." });
                }
                else
                {
                    // Chưa theo dõi -> Tiến hành thêm mới (Follow)
                    var newFollow = new Follow
                    {
                        UserId = user.Id!,
                        ComicId = request.ComicId,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _supabase.Client.From<Follow>().Insert(newFollow);
                    return Ok(new { status = "followed", message = "Followed successfully." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // POST: api/users/comment
        [HttpPost("comment")]
        public async Task<IActionResult> CreateComment([FromBody] CommentRequest request)
        {
            var user = GetCurrentUser();
            if (user == null) return Unauthorized(new { error = "You must be logged in to comment." });
            if (string.IsNullOrWhiteSpace(request.Content)) return BadRequest(new { error = "Comment content cannot be empty." });

            try
            {
                var newComment = new Comment
                {
                    UserId = user.Id!,
                    ComicId = request.ComicId,
                    ChapterId = request.ChapterId,
                    Content = request.Content,
                    ParentId = request.ParentId,
                    CreatedAt = DateTime.UtcNow
                };

                var response = await _supabase.Client.From<Comment>().Insert(newComment);
                return StatusCode(201, response.Models.FirstOrDefault());
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // POST: api/users/history
        [HttpPost("history")]
        public async Task<IActionResult> LogReadingHistory([FromBody] HistoryRequest request)
        {
            var user = GetCurrentUser();
            if (user == null) return Unauthorized(new { error = "User not identified." });

            try
            {
                var historyLog = new ReadingHistory
                {
                    UserId = user.Id!,
                    ComicId = request.ComicId,
                    ChapterId = request.ChapterId,
                    ReadAt = DateTime.UtcNow
                };

                await _supabase.Client.From<ReadingHistory>().Insert(historyLog);
                return Ok(new { message = "Reading history logged successfully." });
            }
            catch (Exception ex) 
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}