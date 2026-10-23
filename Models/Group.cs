using System.ComponentModel.DataAnnotations;

namespace FG_Corporation.Models
{
    public enum GroupPrivacy
    {
        Public,
        Private
    }

    public class Group
    {
        [Key]
        public int Id { get; set; }

        public int CreatedById { get; set; }
        public User CreatedBy { get; set; } = null!;

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string? AvatarUrl { get; set; }

        public GroupPrivacy Privacy { get; set; } = GroupPrivacy.Public;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navegaciones
        public ICollection<GroupMember> Members { get; set; } = new List<GroupMember>();
        public ICollection<Post> Posts { get; set; } = new List<Post>();
        public ICollection<Server> Servers { get; set; } = new List<Server>();
    }

    public enum GroupRole
    {
        Member,
        Admin
    }

    public class GroupMember
    {
        [Key]
        public int Id { get; set; }

        public int GroupId { get; set; }
        public Group Group { get; set; } = null!;

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public GroupRole Role { get; set; } = GroupRole.Member;

        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    }
}
