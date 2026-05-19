namespace Write.Api.Infrastructure.Outbox;

public sealed class OutboxMessage
{
    public Guid Id { get; init; }

    public string AggregateType { get; init; } = default!;

    public string AggregateId { get; init; } = default!;

    public string Type { get; init; } = default!;

    public string Payload { get; init; } = default!;

    public DateTimeOffset OccurredAt { get; init; }
}
