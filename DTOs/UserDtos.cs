namespace FG_Corporation.DTOs
{
    // Lo minimo para mostrar un usuario dentro de otro objeto (ej. autor de un post)
    public class UserSummaryDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
    }

    // Perfil completo de un usuario
    public class UserProfileDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public string? Bio { get; set; }
        public DateTime CreatedAt { get; set; }
        public int FollowersCount { get; set; }
        public int FollowingCount { get; set; }
        public int PostsCount { get; set; }
    }

    // Lo que el cliente manda para registrarse
    public class RegisterUserDto
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    // Lo que el cliente manda para editar su perfil
    public class UpdateUserDto
    {
        public string? Bio { get; set; }
        public string? AvatarUrl { get; set; }
    }
}