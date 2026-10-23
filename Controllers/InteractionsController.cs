using FG_Corporation.Data;
using FG_Corporation.DTOs;
using FG_Corporation.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FG_Corporation.Controllers
{
    [ApiController]
    [Route("api/posts/{postId}")]
    public class InteractionsController : ApiControllerBase
    {
        private readonly AppDbContext _context;

        public InteractionsController(AppDbContext context)
        {
            _context = context;
        }

        // ---------- LIKES ----------

        [Authorize]
        [HttpPost("like")]
        public async Task<IActionResult> Like(int postId)
        {
            var postExists = await _context.Posts.AnyAsync(p => p.Id == postId);
            if (!postExists) return NotFound("Post no encontrado.");

            var already = await _context.Likes.AnyAsync(l => l.PostId == postId && l.UserId == CurrentUserId);
            if (already) return Conflict("Ya le diste like.");

            _context.Likes.Add(new Like { PostId = postId, UserId = CurrentUserId });
            await _context.SaveChangesAsync();
            return Ok();
        }

        [Authorize]
        [HttpDelete("like")]
        public async Task<IActionResult> Unlike(int postId)
        {
            var like = await _context.Likes.FirstOrDefaultAsync(l => l.PostId == postId && l.UserId == CurrentUserId);
            if (like == null) return NotFound();

            _context.Likes.Remove(like);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // ---------- COMMENTS ----------

        // GET publico, cualquiera puede leer comentarios
        [HttpGet("comments")]
        public async Task<ActionResult<IEnumerable<CommentDto>>> GetComments(int postId)
        {
            var comments = await _context.Comments
                .Where(c => c.PostId == postId)
                .OrderBy(c => c.CreatedAt)
                .Select(c => new CommentDto
                {
                    Id = c.Id,
                    Content = c.Content,
                    CreatedAt = c.CreatedAt,
                    User = new UserSummaryDto { Id = c.User.Id, Username = c.User.Username, AvatarUrl = c.User.AvatarUrl }
                })
                .ToListAsync();

            return Ok(comments);
        }

        [Authorize]
        [HttpPost("comments")]
        public async Task<ActionResult<CommentDto>> AddComment(int postId, CreateCommentDto dto)
        {
            var postExists = await _context.Posts.AnyAsync(p => p.Id == postId);
            if (!postExists) return NotFound("Post no encontrado.");

            if (string.IsNullOrWhiteSpace(dto.Content))
                return BadRequest("El comentario no puede estar vacio.");

            var comment = new Comment { PostId = postId, UserId = CurrentUserId, Content = dto.Content, CreatedAt = DateTime.UtcNow };
            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            var user = await _context.Users.FindAsync(CurrentUserId);

            return Ok(new CommentDto
            {
                Id = comment.Id,
                Content = comment.Content,
                CreatedAt = comment.CreatedAt,
                User = new UserSummaryDto { Id = user!.Id, Username = user.Username, AvatarUrl = user.AvatarUrl }
            });
        }

        // Solo el autor del comentario puede borrarlo
        [Authorize]
        [HttpDelete("comments/{commentId}")]
        public async Task<IActionResult> DeleteComment(int postId, int commentId)
        {
            var comment = await _context.Comments.FirstOrDefaultAsync(c => c.Id == commentId && c.PostId == postId);
            if (comment == null) return NotFound();

            if (comment.UserId != CurrentUserId)
                return Forbid();

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // ---------- DOWNLOADS ----------

        [Authorize]
        [HttpPost("download")]
        public async Task<IActionResult> RegisterDownload(int postId)
        {
            var post = await _context.Posts
                .Include(p => p.Files)
                .FirstOrDefaultAsync(p => p.Id == postId);

            if (post == null) return NotFound("Post no encontrado.");

            _context.Downloads.Add(new Download { PostId = postId, UserId = CurrentUserId, DownloadedAt = DateTime.UtcNow });
            await _context.SaveChangesAsync();

            return Ok(new { fileUrls = post.Files.OrderBy(f => f.Order).Select(f => f.FileUrl) });
        }

        // ---------- RATINGS ----------

        [Authorize]
        [HttpPost("rating")]
        public async Task<IActionResult> AddOrUpdateRating(int postId, CreateRatingDto dto)
        {
            if (dto.Stars < 1 || dto.Stars > 5)
                return BadRequest("Stars debe estar entre 1 y 5.");

            var postExists = await _context.Posts.AnyAsync(p => p.Id == postId);
            if (!postExists) return NotFound("Post no encontrado.");

            var rating = await _context.Ratings.FirstOrDefaultAsync(r => r.PostId == postId && r.UserId == CurrentUserId);

            if (rating == null)
            {
                rating = new Rating { PostId = postId, UserId = CurrentUserId, Stars = dto.Stars, Review = dto.Review, CreatedAt = DateTime.UtcNow };
                _context.Ratings.Add(rating);
            }
            else
            {
                rating.Stars = dto.Stars;
                rating.Review = dto.Review;
            }

            await _context.SaveChangesAsync();

            var average = await _context.Ratings.Where(r => r.PostId == postId).AverageAsync(r => r.Stars);
            return Ok(new { averageRating = average });
        }
    }
}

