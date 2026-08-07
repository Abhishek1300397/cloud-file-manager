namespace CloudStorage.Domain.Entities
{
    public sealed class User
    {
        public Guid Id { get; private set; }

        public string Email { get; private set; } = string.Empty;

        public string PasswordHash { get; private set; } = string.Empty;

        public DateTime CreatedAtUtc { get; private set; }

        public DateTime? UpdatedAtUtc { get; private set; }
        private User()
        {
        }

        private User(Guid id, string email, string passwordHash, DateTime createdAtUtc)
        {
            Id = id;
            Email = email;
            PasswordHash = passwordHash;
            CreatedAtUtc = createdAtUtc;
        }



        public static User Create(string email, string passwordHash) => new(Guid.NewGuid(), email.Trim().ToLowerInvariant(), passwordHash, DateTime.UtcNow);
    }
}
