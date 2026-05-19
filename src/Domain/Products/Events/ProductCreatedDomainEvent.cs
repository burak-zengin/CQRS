namespace Domain.Products.Events;

public sealed record ProductCreatedDomainEvent(
    Guid ProductId,
    string Name,
    ProductStatus Status) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();

    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}
