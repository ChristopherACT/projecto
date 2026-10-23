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
    public class GroupsController : ApiControllerBase
    {
        private readonly AppDbContext _context;

        public GroupsController(AppDbContext context)
        {
            _context = context;
        }

        // GET api/groups  (publico)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GroupDto>>> GetAll()
        {
            var groups = await _context.Groups
                .Select(g => new GroupDto
                {
                    Id = g.Id,
                    Name = g.Name,
                    Description = g.Description,
                    AvatarUrl = g.AvatarUrl,
                    Privacy = g.Privacy.ToString(),
                    MembersCount = g.Members.Count,
                    CreatedBy = new UserSummaryDto { Id = g.CreatedBy.Id, Username = g.CreatedBy.Username, AvatarUrl = g.CreatedBy.AvatarUrl }
                })
                .ToListAsync();

            return Ok(groups);
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<GroupDto>> Create(CreateGroupDto dto)
        {
            if (!Enum.TryParse<GroupPrivacy>(dto.Privacy, true, out var privacy))
                privacy = GroupPrivacy.Public;

            var group = new Group
            {
                CreatedById = CurrentUserId,
                Name = dto.Name,
                Description = dto.Description,
                AvatarUrl = dto.AvatarUrl,
                Privacy = privacy,
                CreatedAt = DateTime.UtcNow
            };

            _context.Groups.Add(group);
            await _context.SaveChangesAsync();

            _context.GroupMembers.Add(new GroupMember { GroupId = group.Id, UserId = CurrentUserId, Role = GroupRole.Admin, JoinedAt = DateTime.UtcNow });
            await _context.SaveChangesAsync();

            var creator = await _context.Users.FindAsync(CurrentUserId);

            return Ok(new GroupDto
            {
                Id = group.Id,
                Name = group.Name,
                Description = group.Description,
                AvatarUrl = group.AvatarUrl,
                Privacy = group.Privacy.ToString(),
                MembersCount = 1,
                CreatedBy = new UserSummaryDto { Id = creator!.Id, Username = creator.Username, AvatarUrl = creator.AvatarUrl }
            });
        }

        [Authorize]
        [HttpPost("{id}/join")]
        public async Task<IActionResult> Join(int id)
        {
            var groupExists = await _context.Groups.AnyAsync(g => g.Id == id);
            if (!groupExists) return NotFound();

            var already = await _context.GroupMembers.AnyAsync(m => m.GroupId == id && m.UserId == CurrentUserId);
            if (already) return Conflict("Ya eres miembro de este grupo.");

            _context.GroupMembers.Add(new GroupMember { GroupId = id, UserId = CurrentUserId, Role = GroupRole.Member, JoinedAt = DateTime.UtcNow });
            await _context.SaveChangesAsync();
            return Ok();
        }

        [Authorize]
        [HttpDelete("{id}/leave")]
        public async Task<IActionResult> Leave(int id)
        {
            var member = await _context.GroupMembers.FirstOrDefaultAsync(m => m.GroupId == id && m.UserId == CurrentUserId);
            if (member == null) return NotFound();

            _context.GroupMembers.Remove(member);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}

