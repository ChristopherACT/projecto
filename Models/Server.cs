using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace FG_Corporation.Models
{
    public class Server
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public int? GameId { get; set; }
        public Game? Game { get; set; }

        public int? GroupId { get; set; }
        public Group? Group { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(30)]
        public string Type { get; set; } = string.Empty; // Discord, Minecraft, TeamSpeak, CS2, etc

        public string? IpAddress { get; set; }

        public int? Port { get; set; }

        public string? InviteLink { get; set; }

        public string? Description { get; set; }

        public int? MaxPlayers { get; set; }

        public bool IsOnline { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<ServerVote> Votes { get; set; } = new List<ServerVote>();
    }

    public class ServerVote
    {
        [Key]
        public int Id { get; set; }

        public int ServerId { get; set; }
        public Server Server { get; set; } = null!;

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}