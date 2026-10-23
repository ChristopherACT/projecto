namespace FG_Corporation.DTOs
{
    // Lo que se devuelve en el feed / detalle de un post
    public class PostDto
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty; // Image, Video, Mod, Map, Guide
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public UserSummaryDto User { get; set; } = null!;
        public int? GameId { get; set; }
        public string? GameName { get; set; }
        public int? GroupId { get; set; }
        public int LikesCount { get; set; }
        public int CommentsCount { get; set; }
        public int DownloadsCount { get; set; }
        public double AverageRating { get; set; }
        public List<string> FileUrls { get; set; } = new();
        public List<string> Tags { get; set; } = new();
    }

    // Lo que el cliente manda para crear un post (sin archivos, esos van aparte por multipart/form-data)
    public class CreatePostDto
    {
        public string Type { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int? GameId { get; set; }
        public int? GroupId { get; set; }
        public List<string>? Tags { get; set; }
    }

    public class CommentDto
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public UserSummaryDto User { get; set; } = null!;
    }

    public class CreateCommentDto
    {
        public string Content { get; set; } = string.Empty;
    }

    public class CreateRatingDto
    {
        public int Stars { get; set; }
        public string? Review { get; set; }
    }
}

