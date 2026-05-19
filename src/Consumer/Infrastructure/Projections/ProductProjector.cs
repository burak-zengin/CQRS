using System.Globalization;
using Domain.Products;
using Domain.Products.IntegrationEvents;
using Domain.Products.Projections;
using Domain.Products.ReadModels;

namespace Consumer.Infrastructure.Projections;

public sealed class ProductProjector : IProductProjector
{
    public ProductDetailReadModel Apply(ProductCreatedIntegrationEvent @event) =>
        Build(
            id: @event.ProductId,
            name: @event.Name,
            status: @event.Status.ToString(),
            version: @event.Version,
            variants: Array.Empty<ProductDetailVariantItem>(),
            createdAt: @event.OccurredAt,
            updatedAt: @event.OccurredAt,
            lastEventId: @event.Id,
            lastEventAt: @event.OccurredAt,
            sourceOperation: "ProductCreated");

    public ProductDetailReadModel Apply(ProductRenamedIntegrationEvent @event, ProductDetailReadModel existing) =>
        Build(
            id: existing.Id,
            name: @event.Name,
            status: existing.Status,
            version: @event.Version,
            variants: existing.Variants,
            createdAt: existing.CreatedAt,
            updatedAt: @event.OccurredAt,
            lastEventId: @event.Id,
            lastEventAt: @event.OccurredAt,
            sourceOperation: "ProductRenamed");

    public ProductDetailReadModel Apply(ProductActivatedIntegrationEvent @event, ProductDetailReadModel existing) =>
        Build(
            id: existing.Id,
            name: existing.Name,
            status: ProductStatus.Active.ToString(),
            version: @event.Version,
            variants: existing.Variants,
            createdAt: existing.CreatedAt,
            updatedAt: @event.OccurredAt,
            lastEventId: @event.Id,
            lastEventAt: @event.OccurredAt,
            sourceOperation: "ProductActivated");

    public ProductDetailReadModel Apply(ProductArchivedIntegrationEvent @event, ProductDetailReadModel existing) =>
        Build(
            id: existing.Id,
            name: existing.Name,
            status: ProductStatus.Archived.ToString(),
            version: @event.Version,
            variants: existing.Variants,
            createdAt: existing.CreatedAt,
            updatedAt: @event.OccurredAt,
            lastEventId: @event.Id,
            lastEventAt: @event.OccurredAt,
            sourceOperation: "ProductArchived");

    public ProductDetailReadModel Apply(ProductVariantAddedIntegrationEvent @event, ProductDetailReadModel existing)
    {
        var newVariant = BuildVariant(
            id: @event.Variant.Id,
            sku: @event.Variant.Sku,
            barcode: @event.Variant.Barcode,
            color: @event.Variant.Color,
            size: @event.Variant.Size,
            priceAmount: @event.Variant.PriceAmount,
            currency: @event.Variant.Currency,
            status: VariantStatus.Active.ToString());

        var nextVariants = existing.Variants
            .Where(v => v.Id != @event.Variant.Id)
            .Append(newVariant)
            .ToArray();

        return Build(
            id: existing.Id,
            name: existing.Name,
            status: existing.Status,
            version: @event.Version,
            variants: nextVariants,
            createdAt: existing.CreatedAt,
            updatedAt: @event.OccurredAt,
            lastEventId: @event.Id,
            lastEventAt: @event.OccurredAt,
            sourceOperation: "ProductVariantAdded");
    }

    public ProductDetailReadModel Apply(ProductVariantRemovedIntegrationEvent @event, ProductDetailReadModel existing)
    {
        var nextVariants = existing.Variants
            .Where(v => v.Id != @event.VariantId)
            .ToArray();

        return Build(
            id: existing.Id,
            name: existing.Name,
            status: existing.Status,
            version: @event.Version,
            variants: nextVariants,
            createdAt: existing.CreatedAt,
            updatedAt: @event.OccurredAt,
            lastEventId: @event.Id,
            lastEventAt: @event.OccurredAt,
            sourceOperation: "ProductVariantRemoved");
    }

    public ProductDetailReadModel Apply(ProductVariantPriceChangedIntegrationEvent @event, ProductDetailReadModel existing)
    {
        var nextVariants = existing.Variants
            .Select(v => v.Id == @event.VariantId
                ? BuildVariant(
                    id: v.Id,
                    sku: v.Sku,
                    barcode: v.Barcode,
                    color: v.Color,
                    size: v.Size,
                    priceAmount: @event.PriceAmount,
                    currency: @event.Currency,
                    status: v.Status)
                : v)
            .ToArray();

        return Build(
            id: existing.Id,
            name: existing.Name,
            status: existing.Status,
            version: @event.Version,
            variants: nextVariants,
            createdAt: existing.CreatedAt,
            updatedAt: @event.OccurredAt,
            lastEventId: @event.Id,
            lastEventAt: @event.OccurredAt,
            sourceOperation: "ProductVariantPriceChanged");
    }

    private static ProductDetailReadModel Build(
        Guid id,
        string name,
        string status,
        int version,
        IReadOnlyCollection<ProductDetailVariantItem> variants,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt,
        Guid lastEventId,
        DateTimeOffset lastEventAt,
        string sourceOperation)
    {
        var variantArray = variants as ProductDetailVariantItem[] ?? variants.ToArray();

        decimal? minPrice = variantArray.Length > 0 ? variantArray.Min(v => v.PriceAmount) : null;
        decimal? maxPrice = variantArray.Length > 0 ? variantArray.Max(v => v.PriceAmount) : null;
        string? currency = variantArray.Length > 0 ? variantArray[0].Currency : null;

        var isAvailable =
            status == ProductStatus.Active.ToString()
            && variantArray.Length > 0
            && variantArray.Any(v => v.Status == VariantStatus.Active.ToString());

        return new ProductDetailReadModel
        {
            Id = id,
            Name = name,
            Status = status,
            IsAvailable = isAvailable,
            VariantCount = variantArray.Length,
            MinPriceAmount = minPrice,
            MaxPriceAmount = maxPrice,
            Currency = currency,
            PriceRangeDisplay = FormatPriceRange(minPrice, maxPrice, currency),
            Variants = variantArray,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt,
            Version = version,
            LastEventId = lastEventId,
            LastEventAt = lastEventAt,
            ProjectedAt = DateTimeOffset.UtcNow,
            SourceOperation = sourceOperation
        };
    }

    private static ProductDetailVariantItem BuildVariant(
        Guid id,
        string sku,
        string barcode,
        string color,
        string size,
        decimal priceAmount,
        string currency,
        string status) =>
        new()
        {
            Id = id,
            Sku = sku,
            Barcode = barcode,
            Color = color,
            Size = size,
            PriceAmount = priceAmount,
            Currency = currency,
            PriceDisplay = FormatPrice(priceAmount, currency),
            Status = status
        };

    private static string FormatPrice(decimal amount, string currency) =>
        $"{amount.ToString("0.00", CultureInfo.InvariantCulture)} {currency}";

    private static string? FormatPriceRange(decimal? min, decimal? max, string? currency)
    {
        if (min is null || max is null || currency is null)
        {
            return null;
        }

        return min.Value == max.Value
            ? FormatPrice(min.Value, currency)
            : $"{FormatPrice(min.Value, currency)} - {FormatPrice(max.Value, currency)}";
    }
}
