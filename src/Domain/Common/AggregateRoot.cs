using Domain.Products.Events;

namespace Domain.Common;

public abstract class AggregateRoot
{
    private readonly List<IDomainEvent> _domainEvents = new();

    public int Version { get; protected set; }

    public DateTimeOffset CreatedAt { get; protected set; }

    public DateTimeOffset UpdatedAt { get; protected set; }

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void ClearDomainEvents() => _domainEvents.Clear();

    protected void RaiseEvent(IDomainEvent domainEvent, DateTimeOffset occurredAt)
    {
        _domainEvents.Add(domainEvent);
        Version++;
        UpdatedAt = occurredAt;
    }
}
