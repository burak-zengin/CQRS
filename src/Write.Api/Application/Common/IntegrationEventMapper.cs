using System.Text.Json;
using System.Text.Json.Serialization;
using Domain.Common;
using Domain.Products;
using Domain.Products.Events;
using Domain.Products.IntegrationEvents;

namespace Write.Api.Application.Common;

public readonly record struct OutboxMessageData(
    Guid EventId,
    string AggregateType,
    string AggregateId,
    string Type,
    string Payload,
    DateTimeOffset OccurredAt);

public static class IntegrationEventMapper
{
    private const string ProductAggregateType = "Product";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public static OutboxMessageData Map(Product aggregate, RaisedDomainEvent raised)
    {
        var domainEvent = raised.Event;
        var eventId = domainEvent.EventId;
        var version = raised.AggregateVersion;
        var aggregateId = aggregate.Id.Value.ToString();

        return domainEvent switch
        {
            ProductCreatedDomainEvent e => Build(
                eventId,
                aggregateId,
                EventTypes.ProductCreated,
                new ProductCreatedIntegrationEvent(eventId, aggregate.Id.Value, aggregate.Name, aggregate.Status, version, e.OccurredAt),
                e.OccurredAt),

            ProductRenamedDomainEvent e => Build(
                eventId,
                aggregateId,
                EventTypes.ProductRenamed,
                new ProductRenamedIntegrationEvent(eventId, aggregate.Id.Value, e.NewName, version, e.OccurredAt),
                e.OccurredAt),

            ProductActivatedDomainEvent e => Build(
                eventId,
                aggregateId,
                EventTypes.ProductActivated,
                new ProductActivatedIntegrationEvent(eventId, aggregate.Id.Value, version, e.OccurredAt),
                e.OccurredAt),

            ProductArchivedDomainEvent e => Build(
                eventId,
                aggregateId,
                EventTypes.ProductArchived,
                new ProductArchivedIntegrationEvent(eventId, aggregate.Id.Value, version, e.OccurredAt),
                e.OccurredAt),

            ProductVariantAddedDomainEvent e => Build(
                eventId,
                aggregateId,
                EventTypes.ProductVariantAdded,
                new ProductVariantAddedIntegrationEvent(
                    eventId,
                    aggregate.Id.Value,
                    new ProductVariantPayload(
                        e.VariantId,
                        e.Sku,
                        e.Barcode,
                        e.Color,
                        e.Size,
                        e.PriceAmount,
                        e.Currency),
                    version,
                    e.OccurredAt),
                e.OccurredAt),

            ProductVariantRemovedDomainEvent e => Build(
                eventId,
                aggregateId,
                EventTypes.ProductVariantRemoved,
                new ProductVariantRemovedIntegrationEvent(eventId, aggregate.Id.Value, e.VariantId, version, e.OccurredAt),
                e.OccurredAt),

            ProductVariantPriceChangedDomainEvent e => Build(
                eventId,
                aggregateId,
                EventTypes.ProductVariantPriceChanged,
                new ProductVariantPriceChangedIntegrationEvent(
                    eventId,
                    aggregate.Id.Value,
                    e.VariantId,
                    e.PriceAmount,
                    e.Currency,
                    version,
                    e.OccurredAt),
                e.OccurredAt),

            _ => throw new InvalidOperationException(
                $"Unmapped domain event: {domainEvent.GetType().Name}")
        };
    }

    private static OutboxMessageData Build<TIntegrationEvent>(
        Guid eventId,
        string aggregateId,
        string type,
        TIntegrationEvent payload,
        DateTimeOffset occurredAt)
        where TIntegrationEvent : IIntegrationEvent
    {
        var json = JsonSerializer.Serialize(payload, payload.GetType(), JsonOptions);
        return new OutboxMessageData(
            EventId: eventId,
            AggregateType: ProductAggregateType,
            AggregateId: aggregateId,
            Type: type,
            Payload: json,
            OccurredAt: occurredAt);
    }
}

public static class EventTypes
{
    public const string ProductCreated = "ProductCreated";
    public const string ProductRenamed = "ProductRenamed";
    public const string ProductActivated = "ProductActivated";
    public const string ProductArchived = "ProductArchived";
    public const string ProductVariantAdded = "ProductVariantAdded";
    public const string ProductVariantRemoved = "ProductVariantRemoved";
    public const string ProductVariantPriceChanged = "ProductVariantPriceChanged";
}
