namespace Domain.Products.ReadModels;

public sealed class ProductDetailVariantItem
{
    public Guid Id { get; init; }

    public string Sku { get; init; } = default!;

    public string Barcode { get; init; } = default!;

    public string Color { get; init; } = default!;

    public string Size { get; init; } = default!;

    public decimal PriceAmount { get; init; }

    public string Currency { get; init; } = default!;

    public string PriceDisplay { get; init; } = default!;

    public string Status { get; init; } = default!;
}
