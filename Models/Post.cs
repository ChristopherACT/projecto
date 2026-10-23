using System.ComponentModel.DataAnnotations;
//using System.Text.RegularExpressions;

namespace FG_Corporation.Models
{
    public enum PostType
    {
        Image,
        Video,
        Mod,
        Map,
        Guide
    }

    public class Post
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public int? GroupId { get; set; }
        public Group? Group { get; set; }

        public int? GameId { get; set; }
        public Game? Game { get; set; }

        public PostType Type { get; set; }

        [Required, MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navegaciones
        public ICollection<PostFile> Files { get; set; } = new List<PostFile>();
        public ICollection<PostVersion> Versions { get; set; } = new List<PostVersion>();
        public ICollection<PostTag> Tags { get; set; } = new List<PostTag>();
        public ICollection<Like> Likes { get; set; } = new List<Like>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<Rating> Ratings { get; set; } = new List<Rating>();
        public ICollection<Download> Downloads { get; set; } = new List<Download>();
        public ICollection<Report> Reports { get; set; } = new List<Report>();
    }

    public class PostFile
    {
        [Key]
        public int Id { get; set; }

        public int PostId { get; set; }
        public Post Post { get; set; } = null!;

        [Required]
        public string FileUrl { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? FileType { get; set; } // zip, png, mp4, pdf, docx, etc

        public long FileSize { get; set; }

        public int Order { get; set; }
    }

    public class PostVersion
    {
        [Key]
        public int Id { get; set; }

        public int PostId { get; set; }
        public Post Post { get; set; } = null!;

        [Required, MaxLength(20)]
        public string VersionNumber { get; set; } = string.Empty; // v1.0, v1.1...

        public string? Changelog { get; set; }

        [Required]
        public string FileUrl { get; set; } = string.Empty;

        public string? RequiresGameVersion { get; set; }

        public DateTime ReleasedAt { get; set; } = DateTime.UtcNow;
    }
}
