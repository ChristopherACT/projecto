using FG_Corporation.Data;
using FG_Corporation.DTOs;
using FG_Corporation.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FG_Corporation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PostsController : ApiControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        private static readonly string[] AllowedExtensions =
            { ".zip", ".rar", ".png", ".jpg", ".jpeg", ".gif", ".mp4", ".mov", ".pdf", ".docx", ".txt" };

        private const long MaxFileSizeBytes = 500_000_000; // 500 MB

        public PostsController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET api/posts/feed?type=Image&gameId=1&page=1&pageSize=20  (publico, no requiere login)
        [HttpGet("feed")]
        public async Task<ActionResult<IEnumerable<PostDto>>> GetFeed(
            [FromQuery] string? type,
            [FromQuery] int? gameId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var query = _context.Posts.AsQueryable();

            if (!string.IsNullOrEmpty(type))
            {
                if (!Enum.TryParse<PostType>(type, true, out var postType))
                    return BadRequest("Type invalido. Usa: Image, Video, Mod, Map, Guide.");
                query = query.Where(p => p.Type == postType);
            }

            if (gameId.HasValue)
                query = query.Where(p => p.GameId == gameId);

            var posts = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new PostDto
                {
                    Id = p.Id,
                    Type = p.Type.ToString(),
                    Title = p.Title,
                    Description = p.Description,
                    CreatedAt = p.CreatedAt,
                    User = new UserSummaryDto { Id = p.User.Id, Username = p.User.Username, AvatarUrl = p.User.AvatarUrl },
                    GameId = p.GameId,
                    GameName = p.Game != null ? p.Game.Name : null,
                    GroupId = p.GroupId,
                    LikesCount = p.Likes.Count,
                    CommentsCount = p.Comments.Count,
                    DownloadsCount = p.Downloads.Count,
                    AverageRating = p.Ratings.Any() ? p.Ratings.Average(r => r.Stars) : 0,
                    FileUrls = p.Files.OrderBy(f => f.Order).Select(f => f.FileUrl).ToList(),
                    Tags = p.Tags.Select(t => t.Tag.Name).ToList()
                })
                .ToListAsync();

            return Ok(posts);
        }

        // GET api/posts/5  (publico)
        [HttpGet("{id}")]
        public async Task<ActionResult<PostDto>> GetById(int id)
        {
            var post = await GetPostDtoById(id);
            if (post == null) return NotFound();
            return Ok(post);
        }

        // POST api/posts  (requiere login; el autor sale del token, ya no del query string)
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<PostDto>> Create(CreatePostDto dto)
        {
            if (!Enum.TryParse<PostType>(dto.Type, true, out var postType))
                return BadRequest("Type invalido. Usa: Image, Video, Mod, Map, Guide.");

            var post = new Post
            {
                UserId = CurrentUserId,
                Type = postType,
                Title = dto.Title,
                Description = dto.Description,
                GameId = dto.GameId,
                GroupId = dto.GroupId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            if (dto.Tags != null && dto.Tags.Any())
            {
                foreach (var tagName in dto.Tags.Distinct())
                {
                    var tag = await _context.Tags.FirstOrDefaultAsync(t => t.Name == tagName);
                    if (tag == null)
                    {
                        tag = new Tag { Name = tagName };
                        _context.Tags.Add(tag);
                        await _context.SaveChangesAsync();
                    }
                    _context.PostTags.Add(new PostTag { PostId = post.Id, TagId = tag.Id });
                }
                await _context.SaveChangesAsync();
            }

            var result = await GetPostDtoById(post.Id);
            return CreatedAtAction(nameof(GetById), new { id = post.Id }, result);
        }

        // POST api/posts/5/files  (requiere login y ser el dueno del post)
        [Authorize]
        [HttpPost("{id}/files")]
        [RequestSizeLimit(MaxFileSizeBytes)]
        public async Task<IActionResult> UploadFile(int id, IFormFile file)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null) return NotFound("Post no encontrado.");

            if (post.UserId != CurrentUserId)
                return Forbid();

            if (file == null || file.Length == 0)
                return BadRequest("No se envio ningun archivo.");

            if (file.Length > MaxFileSizeBytes)
                return BadRequest("El archivo supera el limite de 500 MB.");

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(ext))
                return BadRequest($"Extension no permitida: {ext}");

            var uploadsFolder = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads");
            Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{Guid.NewGuid()}{ext}";
            var fullPath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var order = await _context.PostFiles.CountAsync(f => f.PostId == id);

            var postFile = new PostFile
            {
                PostId = id,
                FileUrl = $"/uploads/{fileName}",
                FileType = ext.TrimStart('.'),
                FileSize = file.Length,
                Order = order
            };

            _context.PostFiles.Add(postFile);
            await _context.SaveChangesAsync();

            return Ok(new { url = postFile.FileUrl, fileType = postFile.FileType, size = postFile.FileSize });
        }

        // DELETE api/posts/5  (requiere login y ser el dueno)
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null) return NotFound();

            if (post.UserId != CurrentUserId)
                return Forbid();

            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // ---------- Helper: trae un post CON todas sus relaciones cargadas ----------
        private async Task<PostDto?> GetPostDtoById(int id)
        {
            var post = await _context.Posts
                .Include(p => p.User)
                .Include(p => p.Game)
                .Include(p => p.Files)
                .Include(p => p.Likes)
                .Include(p => p.Comments)
                .Include(p => p.Downloads)
                .Include(p => p.Ratings)
                .Include(p => p.Tags).ThenInclude(pt => pt.Tag)
                .FirstOrDefaultAsync(p => p.Id == id);

            return post == null ? null : MapToDto(post);
        }

        private static PostDto MapToDto(Post p) => new PostDto
        {
            Id = p.Id,
            Type = p.Type.ToString(),
            Title = p.Title,
            Description = p.Description,
            CreatedAt = p.CreatedAt,
            User = new UserSummaryDto { Id = p.User.Id, Username = p.User.Username, AvatarUrl = p.User.AvatarUrl },
            GameId = p.GameId,
            GameName = p.Game != null ? p.Game.Name : null,
            GroupId = p.GroupId,
            LikesCount = p.Likes.Count,
            CommentsCount = p.Comments.Count,
            DownloadsCount = p.Downloads.Count,
            AverageRating = p.Ratings.Any() ? p.Ratings.Average(r => r.Stars) : 0,
            FileUrls = p.Files.OrderBy(f => f.Order).Select(f => f.FileUrl).ToList(),
            Tags = p.Tags.Select(t => t.Tag.Name).ToList()
        };
    }
}