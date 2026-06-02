using Microsoft.AspNetCore.Mvc;
using ComicBackend.WebApi.Attributes;
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

        // GET: api/users/check-admin?id=chuoi-uuid-cua-user
        [HttpGet("check-admin")]
        public async Task<IActionResult> CheckAdminRole([FromQuery] string id)
        {
            if (string.IsNullOrWhiteSpace(id)) 
                return BadRequest(new { error = "User ID không được để trống." });

            // 🌟 ÉP KIỂU: Chuyển chuỗi text ID sang kiểu Guid tương thích với Model User
            if (!Guid.TryParse(id.Trim(), out Guid userGuid))
            {
                return BadRequest(new { error = "Định dạng User ID không hợp lệ (Phải là chuỗi GUID)." });
            }

            try
            {
                var response = await _supabase.Client
                    .From<Models.User>()
                    .Filter(x => x.Id, Operator.Equals, userGuid) // 🌟 Đã khớp kiểu dữ liệu Guid
                    .Get();

                var userProfile = response.Models.FirstOrDefault();

                if (userProfile == null)
                {
                    return NotFound(new { error = "Không tìm thấy thông tin tài khoản." });
                }

                return Ok(new {
                    success = true,
                    role = userProfile.Role?.ToLower().Trim()
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Lỗi kiểm tra quyền: " + ex.Message });
            }
        }

        // POST: api/users/follow
        [HttpPost("follow")]
        public async Task<IActionResult> ToggleFollow([FromBody] FollowRequest request)
        {
            var user = GetCurrentUser();
            if (user == null) return Unauthorized(new { error = "You must be logged in to follow comics." });

            try
            {
                var response = await _supabase.Client
                    .From<Follow>()
                    .Filter(x => x.UserId!, Operator.Equals, user.Id)
                    .Filter(x => x.ComicId, Operator.Equals, request.ComicId)
                    .Get();

                var existing = response.Models.FirstOrDefault();

                if (existing != null)
                {
                    await _supabase.Client.From<Follow>().Delete(existing);
                    return Ok(new { status = "unfollowed", message = "Unfollowed successfully." });
                }
                else
                {
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

        // GET: api/users/profile
        [HttpGet("profile")]
        [AuthorizeRoles("User", "Uploader", "Admin")]
        public async Task<IActionResult> GetProfile()
        {
            var user = GetCurrentUser();
            if (user == null) return Unauthorized(new { error = "You must be logged in." });

            try
            {
                if (!Guid.TryParse(user.Id, out Guid userGuid))
                {
                    return BadRequest(new { error = "Mã định danh User Auth không đúng định dạng GUID." });
                }

                var response = await _supabase.Client
                    .From<Models.User>()
                    .Filter(x => x.Id, Operator.Equals, userGuid)
                    .Single();

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Could not fetch profile: " + ex.Message });
            }
        }

        // POST: api/users/comment
        [HttpPost("comment")]
        [AuthorizeRoles("User", "Uploader", "Admin")]
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

        // DELETE: api/users/admin/manage-user/{id}
        [HttpDelete("admin/manage-user/{id}")]
        [AuthorizeRoles("Admin")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            if (!Guid.TryParse(id, out Guid userGuid))
            {
                return BadRequest(new { error = "ID người dùng cần xóa không hợp lệ." });
            }

            try
            {
                await _supabase.Client
                    .From<Models.User>()
                    .Filter(x => x.Id, Operator.Equals, userGuid)
                    .Delete();
        
                return Ok(new { message = $"User with ID {id} has been successfully deleted by Admin." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}