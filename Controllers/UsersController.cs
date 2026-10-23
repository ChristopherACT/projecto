using FG_Corporation.Data;
using FG_Corporation.DTOs;
using FG_Corporation.Models;
using FG_Corporation.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FG_Corporation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ApiControllerBase
    {
        private readonly AppDbContext _context;
        private readonly TokenService _tokenService;

        public UsersController(AppDbContext context, TokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        // POST api/users/register
        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDto>> Register(RegisterUserDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest("Username, email y password son obligatorios.");

            var exists = await _context.Users.AnyAsync(u => u.Email == dto.Email || u.Username == dto.Username);
            if (exists)
                return Conflict("Ya existe un usuario con ese email o username.");

            var user = new User
            {
                Username = dto.Username,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var (token, expiresAt) = _tokenService.GenerateToken(user);

            return CreatedAtAction(nameof(GetProfile), new { id = user.Id }, new AuthResponseDto
            {
                Token = token,
                ExpiresAt = expiresAt,
                User = new UserSummaryDto { Id = user.Id, Username = user.Username, AvatarUrl = user.AvatarUrl }
            });
        }

        // POST api/users/login
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login(LoginDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);

            // Mensaje generico a proposito: no revelamos si fallo el email o el password,
            // para no darle pistas a quien intenta adivinar cuentas.
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return Unauthorized("Email o password incorrectos.");

            var (token, expiresAt) = _tokenService.GenerateToken(user);

            return Ok(new AuthResponseDto
            {
                Token = token,
                ExpiresAt = expiresAt,
                User = new UserSummaryDto { Id = user.Id, Username = user.Username, AvatarUrl = user.AvatarUrl }
            });
        }

        // GET api/users/5  (publico, cualquiera puede ver un perfil)
        [HttpGet("{id}")]
        public async Task<ActionResult<UserProfileDto>> GetProfile(int id)
        {
            var exists = await _context.Users.AnyAsync(u => u.Id == id);
            if (!exists) return NotFound();

            return Ok(await BuildProfileDto(id));
        }

        // GET api/users/me  (requiere estar logueado, usa el token)
        [Authorize]
        [HttpGet("me")]
        public async Task<ActionResult<UserProfileDto>> GetMyProfile()
        {
            return Ok(await BuildProfileDto(CurrentUserId));
        }

        // PUT api/users/me  (edita SOLO tu propio perfil, ya no recibe id por la URL)
        [Authorize]
        [HttpPut("me")]
        public async Task<IActionResult> UpdateProfile(UpdateUserDto dto)
        {
            var user = await _context.Users.FindAsync(CurrentUserId);
            if (user == null) return NotFound();

            if (dto.Bio != null) user.Bio = dto.Bio;
            if (dto.AvatarUrl != null) user.AvatarUrl = dto.AvatarUrl;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // ---------- Secciones del perfil ----------

        // GET api/users/5/posts?type=Image  (publico)
        [HttpGet("{id}/posts")]
        public async Task<ActionResult<IEnumerable<PostDto>>> GetUserPosts(int id, [FromQuery] string? type)
        {
            var query = _context.Posts.Where(p => p.UserId == id);

            if (!string.IsNullOrEmpty(type) && Enum.TryParse<PostType>(type, true, out var postType))
                query = query.Where(p => p.Type == postType);

            var posts = await query
                .OrderByDescending(p => p.CreatedAt)
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

        // ---------- Follow / Unfollow (ahora usan el token, no query params) ----------

        // POST api/users/5/follow  (el usuario del token sigue al usuario 5)
        [Authorize]
        [HttpPost("{id}/follow")]
        public async Task<IActionResult> Follow(int id)
        {
            if (id == CurrentUserId) return BadRequest("No puedes seguirte a ti mismo.");

            var already = await _context.Follows.AnyAsync(f => f.FollowerId == CurrentUserId && f.FollowedId == id);
            if (already) return Conflict("Ya lo sigues.");

            _context.Follows.Add(new Follow { FollowerId = CurrentUserId, FollowedId = id });
            await _context.SaveChangesAsync();
            return Ok();
        }

        // DELETE api/users/5/follow
        [Authorize]
        [HttpDelete("{id}/follow")]
        public async Task<IActionResult> Unfollow(int id)
        {
            var follow = await _context.Follows.FirstOrDefaultAsync(f => f.FollowerId == CurrentUserId && f.FollowedId == id);
            if (follow == null) return NotFound();

            _context.Follows.Remove(follow);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // ---------- Helpers ----------

        private async Task<UserProfileDto> BuildProfileDto(int userId)
        {
            var user = await _context.Users.FirstAsync(u => u.Id == userId);

            return new UserProfileDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                AvatarUrl = user.AvatarUrl,
                Bio = user.Bio,
                CreatedAt = user.CreatedAt,
                FollowersCount = await _context.Follows.CountAsync(f => f.FollowedId == userId),
                FollowingCount = await _context.Follows.CountAsync(f => f.FollowerId == userId),
                PostsCount = await _context.Posts.CountAsync(p => p.UserId == userId)
            };
        }
    }
}
