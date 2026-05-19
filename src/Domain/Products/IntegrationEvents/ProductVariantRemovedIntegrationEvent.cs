namespace Domain.Products.IntegrationEvents;

public sealed record ProductVariantRemovedIntegrationEvent(
    Guid Id,
    Guid ProductId,
    Guid VariantId,
    int Version,
    DateTimeOffset OccurredAt) : IIntegrationEvent;
