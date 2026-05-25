namespace Domain.Common;

public sealed record RaisedDomainEvent(IDomainEvent Event, int AggregateVersion);
