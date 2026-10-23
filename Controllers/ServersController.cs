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
    public class ServersController : ApiControllerBase
    {
        private readonly AppDbContext _context;

        public ServersController(AppDbContext context)
        {
            _context = context;
        }

        // GET api/servers?gameId=1  (publico)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ServerDto>>> GetAll([FromQuery] int? gameId)
        {
            var query = _context.Servers.AsQueryable();
            if (gameId.HasValue) query = query.Where(s => s.GameId == gameId);

            var servers = await query
                .OrderByDescending(s => s.Votes.Count)
                .Select(s => new ServerDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    Type = s.Type,
                    IpAddress = s.IpAddress,
                    Port = s.Port,
                    InviteLink = s.InviteLink,
                    Description = s.Description,
                    MaxPlayers = s.MaxPlayers,
                    IsOnline = s.IsOnline,
                    VotesCount = s.Votes.Count,
                    GameName = s.Game != null ? s.Game.Name : null,
                    PostedBy = new UserSummaryDto { Id = s.User.Id, Username = s.User.Username, AvatarUrl = s.User.AvatarUrl }
                })
                .ToListAsync();

            return Ok(servers);
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<ServerDto>> Create(CreateServerDto dto)
        {
            var server = new Server
            {
                UserId = CurrentUserId,
                Name = dto.Name,
                Type = dto.Type,
                IpAddress = dto.IpAddress,
                Port = dto.Port,
                InviteLink = dto.InviteLink,
                Description = dto.Description,
                MaxPlayers = dto.MaxPlayers,
                GameId = dto.GameId,
                GroupId = dto.GroupId,
                IsOnline = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.Servers.Add(server);
            await _context.SaveChangesAsync();

            var poster = await _context.Users.FindAsync(CurrentUserId);
            var game = dto.GameId.HasValue ? await _context.Games.FindAsync(dto.GameId) : null;

            return Ok(new ServerDto
            {
                Id = server.Id,
                Name = server.Name,
                Type = server.Type,
                IpAddress = server.IpAddress,
                Port = server.Port,
                InviteLink = server.InviteLink,
                Description = server.Description,
                MaxPlayers = server.MaxPlayers,
                IsOnline = server.IsOnline,
                VotesCount = 0,
                GameName = game?.Name,
                PostedBy = new UserSummaryDto { Id = poster!.Id, Username = poster.Username, AvatarUrl = poster.AvatarUrl }
            });
        }

        [Authorize]
        [HttpPost("{id}/vote")]
        public async Task<IActionResult> Vote(int id)
        {
            var serverExists = await _context.Servers.AnyAsync(s => s.Id == id);
            if (!serverExists) return NotFound();

            var already = await _context.ServerVotes.AnyAsync(v => v.ServerId == id && v.UserId == CurrentUserId);
            if (already) return Conflict("Ya votaste por este servidor.");

            _context.ServerVotes.Add(new ServerVote { ServerId = id, UserId = CurrentUserId, CreatedAt = DateTime.UtcNow });
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}


