namespace KrtBank.Domain.Common
{
    public abstract class BaseEntity
    {
        public Guid Id { get; protected set; } = Guid.NewGuid();
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; protected set; }

        public void MarkAsUpdated() => UpdatedAt = DateTime.UtcNow;
    }
}
