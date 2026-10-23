using FG_Corporation.Data;
using FG_Corporation.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FG_Corporation.Controllers
{
    // DTOs simples, solo para este controller (Games no necesita mucho mas)
    public class GameDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? IconUrl { get; set; }
    }

    public class CreateGameDto
    {
        public string Name { get; set; } = string.Empty;
        public string? IconUrl { get; set; }
    }

    [ApiController]
    [Route("api/[controller]")]
    public class GamesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public GamesController(AppDbContext context)
        {
            _context = context;
        }

        // GET api/games
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GameDto>>> GetAll()
        {
            var games = await _context.Games
                .OrderBy(g => g.Name)
                .Select(g => new GameDto { Id = g.Id, Name = g.Name, IconUrl = g.IconUrl })
                .ToListAsync();

            return Ok(games);
        }

        // POST api/games  (normalmente solo un admin haria esto)
        [HttpPost]
        public async Task<ActionResult<GameDto>> Create(CreateGameDto dto)
        {
            var game = new Game { Name = dto.Name, IconUrl = dto.IconUrl };
            _context.Games.Add(game);
            await _context.SaveChangesAsync();

            var result = new GameDto { Id = game.Id, Name = game.Name, IconUrl = game.IconUrl };
            return CreatedAtAction(nameof(GetAll), new { id = game.Id }, result);
        }
    }
}