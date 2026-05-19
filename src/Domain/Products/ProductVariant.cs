using Domain.Products.ValueObjects;

namespace Domain.Products;

public sealed class ProductVariant
{
    private const int MaxColorLength = 50;
    private const int MaxSizeLength = 50;

    private ProductVariant() { }

    public VariantId Id { get; private set; } = default!;

    public ProductId ProductId { get; private set; } = default!;

    public Sku Sku { get; private set; } = default!;

    public Barcode Barcode { get; private set; } = default!;

    public string Color { get; private set; } = default!;

    public string Size { get; private set; } = default!;

    public Money Price { get; private set; } = default!;

    public VariantStatus Status { get; private set; }

    internal static ProductVariant Create(
        ProductId productId,
        Sku sku,
        Barcode barcode,
        string color,
        string size,
        Money price)
    {
        ValidateColor(color);
        ValidateSize(size);

        return new ProductVariant
        {
            Id = VariantId.New(),
            ProductId = productId,
            Sku = sku,
            Barcode = barcode,
            Color = color.Trim(),
            Size = size.Trim(),
            Price = price,
            Status = VariantStatus.Active
        };
    }

    internal void ChangePrice(Money newPrice)
    {
        ArgumentNullException.ThrowIfNull(newPrice);

        if (!Price.HasSameCurrency(newPrice))
        {
            throw new InvalidOperationException(
                $"Cannot change variant price from {Price.Currency} to {newPrice.Currency}.");
        }

        Price = newPrice;
    }

    private static void ValidateColor(string color)
    {
        if (string.IsNullOrWhiteSpace(color))
        {
            throw new ArgumentException("Color is required.", nameof(color));
        }

        if (color.Trim().Length > MaxColorLength)
        {
            throw new ArgumentException($"Color length must be <= {MaxColorLength}.", nameof(color));
        }
    }

    private static void ValidateSize(string size)
    {
        if (string.IsNullOrWhiteSpace(size))
        {
            throw new ArgumentException("Size is required.", nameof(size));
        }

        if (size.Trim().Length > MaxSizeLength)
        {
            throw new ArgumentException($"Size length must be <= {MaxSizeLength}.", nameof(size));
        }
    }
}
