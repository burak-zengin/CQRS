namespace Domain.Products.Events;

public sealed record ProductVariantAddedDomainEvent(
    Guid ProductId,
    Guid VariantId,
    string Sku,
    string Barcode,
    string Color,
    string Size,
    decimal PriceAmount,
    string Currency) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();

    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}
