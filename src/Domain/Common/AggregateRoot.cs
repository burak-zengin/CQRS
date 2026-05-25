namespace Domain.Common;

public abstract class AggregateRoot
{
    private readonly List<RaisedDomainEvent> _domainEvents = new();

    public int Version { get; protected set; }

    public DateTimeOffset CreatedAt { get; protected set; }

    public DateTimeOffset UpdatedAt { get; protected set; }

    public IReadOnlyCollection<RaisedDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void ClearDomainEvents() => _domainEvents.Clear();

    protected void RaiseEvent(IDomainEvent domainEvent, DateTimeOffset occurredAt)
    {
        Version++;
        _domainEvents.Add(new RaisedDomainEvent(domainEvent, Version));
        UpdatedAt = occurredAt;
    }
}
