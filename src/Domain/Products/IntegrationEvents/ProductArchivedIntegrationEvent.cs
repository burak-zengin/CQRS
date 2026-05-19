namespace Domain.Products.IntegrationEvents;

public sealed record ProductArchivedIntegrationEvent(
    Guid Id,
    Guid ProductId,
    int Version,
    DateTimeOffset OccurredAt) : IIntegrationEvent;
