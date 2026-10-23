namespace FG_Corporation.DTOs
{
    public class GroupDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? AvatarUrl { get; set; }
        public string Privacy { get; set; } = string.Empty;
        public int MembersCount { get; set; }
        public UserSummaryDto CreatedBy { get; set; } = null!;
    }

    public class CreateGroupDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? AvatarUrl { get; set; }
        public string Privacy { get; set; } = "Public";
    }

    public class ServerDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string? IpAddress { get; set; }
        public int? Port { get; set; }
        public string? InviteLink { get; set; }
        public string? Description { get; set; }
        public int? MaxPlayers { get; set; }
        public bool IsOnline { get; set; }
        public int VotesCount { get; set; }
        public string? GameName { get; set; }
        public UserSummaryDto PostedBy { get; set; } = null!;
    }

    public class CreateServerDto
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string? IpAddress { get; set; }
        public int? Port { get; set; }
        public string? InviteLink { get; set; }
        public string? Description { get; set; }
        public int? MaxPlayers { get; set; }
        public int? GameId { get; set; }
        public int? GroupId { get; set; }
    }
}

