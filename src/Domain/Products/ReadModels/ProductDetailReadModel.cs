namespace Domain.Products.ReadModels;

public sealed class ProductDetailReadModel
{
    public Guid Id { get; init; }

    public string Name { get; init; } = default!;

    public string Status { get; init; } = default!;

    public bool IsAvailable { get; init; }

    public int VariantCount { get; init; }

    public decimal? MinPriceAmount { get; init; }

    public decimal? MaxPriceAmount { get; init; }

    public string? Currency { get; init; }

    public string? PriceRangeDisplay { get; init; }

    public ProductDetailVariantItem[] Variants { get; init; } = Array.Empty<ProductDetailVariantItem>();

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset UpdatedAt { get; init; }

    public int Version { get; init; }

    public Guid? LastEventId { get; init; }

    public DateTimeOffset? LastEventAt { get; init; }

    public DateTimeOffset ProjectedAt { get; init; }

    public string SourceOperation { get; init; } = default!;
}
