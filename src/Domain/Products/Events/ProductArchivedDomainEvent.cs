namespace Domain.Products.Events;

public sealed record ProductArchivedDomainEvent(Guid ProductId) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();

    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}
