using Domain.Common;

namespace Domain.Products.Events;

public sealed record ProductActivatedDomainEvent(Guid ProductId) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();

    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}
