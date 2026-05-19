using System.Text.Json;
using Domain.Products.IntegrationEvents;
using Domain.Products.Projections;
using Domain.Products.ReadModels;
using Domain.Products.Repositories;

namespace Consumer.Infrastructure.Messaging;

public sealed class IntegrationEventDispatcher
{
    public const string ProductCreated = "ProductCreated";
    public const string ProductRenamed = "ProductRenamed";
    public const string ProductActivated = "ProductActivated";
    public const string ProductArchived = "ProductArchived";
    public const string ProductVariantAdded = "ProductVariantAdded";
    public const string ProductVariantRemoved = "ProductVariantRemoved";
    public const string ProductVariantPriceChanged = "ProductVariantPriceChanged";

    private readonly IProductProjector _projector;
    private readonly IProductListProjector _listProjector;
    private readonly IProductProjectionRepository _projectionRepository;
    private readonly JsonSerializerOptions _jsonOptions;

    public IntegrationEventDispatcher(
        IProductProjector projector,
        IProductListProjector listProjector,
        IProductProjectionRepository projectionRepository)
    {
        _projector = projector;
        _listProjector = listProjector;
        _projectionRepository = projectionRepository;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
        };
    }

    public async Task DispatchAsync(string eventType, string payload, CancellationToken cancellationToken)
    {
        switch (eventType)
        {
            case ProductCreated:
                await ApplyCreatedAsync(payload, cancellationToken);
                break;

            case ProductRenamed:
                await ApplyWithExistingAsync<ProductRenamedIntegrationEvent>(
                    payload,
                    (e, existing) => _projector.Apply(e, existing),
                    e => e.ProductId,
                    e => e.Version,
                    cancellationToken);
                break;

            case ProductActivated:
                await ApplyWithExistingAsync<ProductActivatedIntegrationEvent>(
                    payload,
                    (e, existing) => _projector.Apply(e, existing),
                    e => e.ProductId,
                    e => e.Version,
                    cancellationToken);
                break;

            case ProductArchived:
                await ApplyWithExistingAsync<ProductArchivedIntegrationEvent>(
                    payload,
                    (e, existing) => _projector.Apply(e, existing),
                    e => e.ProductId,
                    e => e.Version,
                    cancellationToken);
                break;

            case ProductVariantAdded:
                await ApplyWithExistingAsync<ProductVariantAddedIntegrationEvent>(
                    payload,
                    (e, existing) => _projector.Apply(e, existing),
                    e => e.ProductId,
                    e => e.Version,
                    cancellationToken);
                break;

            case ProductVariantRemoved:
                await ApplyWithExistingAsync<ProductVariantRemovedIntegrationEvent>(
                    payload,
                    (e, existing) => _projector.Apply(e, existing),
                    e => e.ProductId,
                    e => e.Version,
                    cancellationToken,
                    allowMissingExisting: true);
                break;

            case ProductVariantPriceChanged:
                await ApplyWithExistingAsync<ProductVariantPriceChangedIntegrationEvent>(
                    payload,
                    (e, existing) => _projector.Apply(e, existing),
                    e => e.ProductId,
                    e => e.Version,
                    cancellationToken);
                break;

            default:
                Console.Error.WriteLine($"[Consumer] Unknown eventType '{eventType}', skipping.");
                break;
        }
    }

    private async Task ApplyCreatedAsync(string payload, CancellationToken cancellationToken)
    {
        var @event = Deserialize<ProductCreatedIntegrationEvent>(payload);

        var existing = await _projectionRepository.GetDetailAsync(@event.ProductId, cancellationToken);
        if (existing is not null && existing.Version >= @event.Version)
        {
            Console.WriteLine(
                $"[Consumer] Skipping ProductCreated for product {@event.ProductId}: " +
                $"event.Version={@event.Version} <= existing.Version={existing.Version}");
            return;
        }

        var detail = _projector.Apply(@event);
        await PersistAsync(detail, cancellationToken);
        Console.WriteLine(
            $"[Consumer] Projected ProductCreated for {@event.ProductId} at Version={@event.Version}");
    }

    private async Task ApplyWithExistingAsync<TEvent>(
        string payload,
        Func<TEvent, ProductDetailReadModel, ProductDetailReadModel> apply,
        Func<TEvent, Guid> productIdSelector,
        Func<TEvent, int> versionSelector,
        CancellationToken cancellationToken,
        bool allowMissingExisting = false)
    {
        var @event = Deserialize<TEvent>(payload);
        var productId = productIdSelector(@event);

        var existing = await _projectionRepository.GetDetailAsync(productId, cancellationToken);
        if (existing is null)
        {
            if (allowMissingExisting)
            {
                return;
            }

            throw new InvalidOperationException(
                $"Cannot project event for missing product '{productId}'.");
        }

        // Out-of-order / replay guard: only apply events strictly newer than the current projection.
        var eventVersion = versionSelector(@event);
        if (eventVersion <= existing.Version)
        {
            Console.WriteLine(
                $"[Consumer] Skipping {typeof(TEvent).Name} for product {productId}: " +
                $"event.Version={eventVersion} <= existing.Version={existing.Version}");
            return;
        }

        var detail = apply(@event, existing);
        await PersistAsync(detail, cancellationToken);
        Console.WriteLine(
            $"[Consumer] Projected {typeof(TEvent).Name} for {productId}: " +
            $"{existing.Version} -> {eventVersion}");
    }

    /// <summary>
    /// Single write-fan-out: detail is the canonical projection, list is derived from it.
    /// Keeping both writes in the same dispatcher call ensures the two views stay consistent
    /// for a given event, even if they live in different Mongo collections.
    /// </summary>
    private async Task PersistAsync(ProductDetailReadModel detail, CancellationToken cancellationToken)
    {
        var list = _listProjector.Project(detail);
        await _projectionRepository.UpsertDetailAsync(detail, cancellationToken);
        await _projectionRepository.UpsertListAsync(list, cancellationToken);
    }

    private TEvent Deserialize<TEvent>(string payload)
    {
        return JsonSerializer.Deserialize<TEvent>(payload, _jsonOptions)
            ?? throw new InvalidOperationException(
                $"Payload could not be deserialized as {typeof(TEvent).Name}.");
    }
}
