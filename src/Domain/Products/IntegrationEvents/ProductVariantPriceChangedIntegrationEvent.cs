namespace Domain.Products.IntegrationEvents;

public sealed record ProductVariantPriceChangedIntegrationEvent(
    Guid Id,
    Guid ProductId,
    Guid VariantId,
    decimal PriceAmount,
    string Currency,
    int Version,
    DateTimeOffset OccurredAt) : IIntegrationEvent;
