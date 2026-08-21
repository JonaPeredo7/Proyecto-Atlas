namespace Gimnasio.Domain.Common;

public abstract class Entity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
    public DateTimeOffset CreatedAt { get; protected set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; protected set; }
    public bool IsActive { get; protected set; } = true;

    public void RestoreMetadata(Guid id, DateTimeOffset createdAt, DateTimeOffset? updatedAt, bool isActive)
    {
        if (id == Guid.Empty) throw new ArgumentException("El identificador restaurado no es válido.");
        if (createdAt == default) throw new ArgumentException("La fecha de creación restaurada no es válida.");
        Id = id;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
        IsActive = isActive;
    }
}
