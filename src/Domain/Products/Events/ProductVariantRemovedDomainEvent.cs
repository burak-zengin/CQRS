using Domain.Common;

namespace Domain.Products.Events;

public sealed record ProductVariantRemovedDomainEvent(Guid ProductId, Guid VariantId) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();

    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}
