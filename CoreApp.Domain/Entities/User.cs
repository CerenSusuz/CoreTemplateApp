using CoreApp.Domain.Common;

namespace CoreApp.Domain.Entities
{
    public class User : BaseEntity
    {
        public string Username { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        public ICollection<Role> Roles { get; set; } = new List<Role>();

        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    }
}
