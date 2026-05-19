using Domain.Products;

namespace Domain.Products.IntegrationEvents;

public sealed record ProductCreatedIntegrationEvent(
    Guid Id,
    Guid ProductId,
    string Name,
    ProductStatus Status,
    int Version,
    DateTimeOffset OccurredAt) : IIntegrationEvent;
