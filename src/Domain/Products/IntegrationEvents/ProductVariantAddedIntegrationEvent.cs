namespace Domain.Products.IntegrationEvents;

public sealed record ProductVariantAddedIntegrationEvent(
    Guid Id,
    Guid ProductId,
    ProductVariantPayload Variant,
    int Version,
    DateTimeOffset OccurredAt) : IIntegrationEvent;
