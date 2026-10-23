using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FG_Corporation.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required, MaxLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        public string? AvatarUrl { get; set; }

        public string? Bio { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navegaciones
        public ICollection<Post> Posts { get; set; } = new List<Post>();
        public ICollection<Group> GroupsCreated { get; set; } = new List<Group>();
        public ICollection<GroupMember> GroupMemberships { get; set; } = new List<GroupMember>();
        public ICollection<Like> Likes { get; set; } = new List<Like>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<Rating> Ratings { get; set; } = new List<Rating>();
        public ICollection<Download> Downloads { get; set; } = new List<Download>();
        public ICollection<Report> Reports { get; set; } = new List<Report>();
        public ICollection<Server> Servers { get; set; } = new List<Server>();
        public ICollection<ServerVote> ServerVotes { get; set; } = new List<ServerVote>();
        public ICollection<Message> MessagesSent { get; set; } = new List<Message>();
        public ICollection<ForumThread> ForumThreads { get; set; } = new List<ForumThread>();
        public ICollection<ForumReply> ForumReplies { get; set; } = new List<ForumReply>();
    }
}