namespace Domain.Products.IntegrationEvents;

public sealed record ProductRenamedIntegrationEvent(
    Guid Id,
    Guid ProductId,
    string Name,
    int Version,
    DateTimeOffset OccurredAt) : IIntegrationEvent;
