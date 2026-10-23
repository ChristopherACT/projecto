using System.ComponentModel.DataAnnotations;

namespace FG_Corporation.Models
{
    public class Follow
    {
        [Key]
        public int Id { get; set; }

        public int FollowerId { get; set; }
        public User Follower { get; set; } = null!;

        public int FollowedId { get; set; }
        public User Followed { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
