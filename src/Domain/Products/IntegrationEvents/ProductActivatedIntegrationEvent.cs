namespace Domain.Products.IntegrationEvents;

public sealed record ProductActivatedIntegrationEvent(
    Guid Id,
    Guid ProductId,
    int Version,
    DateTimeOffset OccurredAt) : IIntegrationEvent;
