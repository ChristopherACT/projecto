using System.ComponentModel.DataAnnotations;

namespace FG_Corporation.Models
{
    public class Game
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public string? IconUrl { get; set; }

        // Navegaciones
        public ICollection<Post> Posts { get; set; } = new List<Post>();
        public ICollection<Server> Servers { get; set; } = new List<Server>();
    }
}

