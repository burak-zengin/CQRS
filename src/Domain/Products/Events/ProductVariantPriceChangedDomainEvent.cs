using Domain.Common;

namespace Domain.Products.Events;

public sealed record ProductVariantPriceChangedDomainEvent(
    Guid ProductId,
    Guid VariantId,
    decimal PriceAmount,
    string Currency) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();

    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}
