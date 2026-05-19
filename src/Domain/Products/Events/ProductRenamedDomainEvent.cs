namespace Domain.Products.Events;

public sealed record ProductRenamedDomainEvent(Guid ProductId, string NewName) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();

    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}
