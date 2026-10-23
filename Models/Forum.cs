using System.ComponentModel.DataAnnotations;

namespace FG_Corporation.Models
{
    public class ForumCategory
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public ICollection<ForumThread> Threads { get; set; } = new List<ForumThread>();
    }

    public class ForumThread
    {
        [Key]
        public int Id { get; set; }

        public int CategoryId { get; set; }
        public ForumCategory Category { get; set; } = null!;

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        [Required, MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<ForumReply> Replies { get; set; } = new List<ForumReply>();
    }

    public class ForumReply
    {
        [Key]
        public int Id { get; set; }

        public int ThreadId { get; set; }
        public ForumThread Thread { get; set; } = null!;

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        [Required]
        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}