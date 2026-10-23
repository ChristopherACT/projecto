using System.ComponentModel.DataAnnotations;

namespace FG_Corporation.Models
{
    public class Conversation
    {
        [Key]
        public int Id { get; set; }

        public int User1Id { get; set; }
        public User User1 { get; set; } = null!;

        public int User2Id { get; set; }
        public User User2 { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Message> Messages { get; set; } = new List<Message>();
    }

    public class Message
    {
        [Key]
        public int Id { get; set; }

        public int ConversationId { get; set; }
        public Conversation Conversation { get; set; } = null!;

        public int SenderId { get; set; }
        public User Sender { get; set; } = null!;

        [Required]
        public string Content { get; set; } = string.Empty;

        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        public DateTime? ReadAt { get; set; }
    }
} 
