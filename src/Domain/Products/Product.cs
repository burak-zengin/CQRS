using Domain.Common;
using Domain.Products.Events;
using Domain.Products.ValueObjects;

namespace Domain.Products;

public sealed class Product : AggregateRoot
{
    private const int MaxNameLength = 200;
    private const int MaxVariantCount = 100;

    private readonly List<ProductVariant> _variants = new();

    private Product() { }

    public ProductId Id { get; private set; } = default!;

    public string Name { get; private set; } = default!;

    public ProductStatus Status { get; private set; }

    public IReadOnlyCollection<ProductVariant> Variants => _variants.AsReadOnly();

    public static Product Create(string name)
    {
        var trimmed = ValidateName(name);

        var now = DateTimeOffset.UtcNow;
        var product = new Product
        {
            Id = ProductId.New(),
            Name = trimmed,
            Status = ProductStatus.Draft,
            CreatedAt = now
        };

        product.RaiseEvent(
            new ProductCreatedDomainEvent(product.Id.Value, product.Name, product.Status),
            now);
        return product;
    }

    public void Rename(string newName)
    {
        EnsureNotArchived();

        var trimmed = ValidateName(newName);
        if (string.Equals(Name, trimmed, StringComparison.Ordinal))
        {
            return;
        }

        Name = trimmed;
        RaiseEvent(new ProductRenamedDomainEvent(Id.Value, Name), DateTimeOffset.UtcNow);
    }

    public void Activate()
    {
        if (Status == ProductStatus.Active)
        {
            return;
        }

        if (Status == ProductStatus.Archived)
        {
            throw new InvalidOperationException("Archived products cannot be activated.");
        }

        Status = ProductStatus.Active;
        RaiseEvent(new ProductActivatedDomainEvent(Id.Value), DateTimeOffset.UtcNow);
    }

    public void Archive()
    {
        if (Status == ProductStatus.Archived)
        {
            return;
        }

        Status = ProductStatus.Archived;
        RaiseEvent(new ProductArchivedDomainEvent(Id.Value), DateTimeOffset.UtcNow);
    }

    public ProductVariant AddVariant(
        Sku sku,
        Barcode barcode,
        string color,
        string size,
        Money price)
    {
        EnsureNotArchived();

        ArgumentNullException.ThrowIfNull(sku);
        ArgumentNullException.ThrowIfNull(barcode);
        ArgumentNullException.ThrowIfNull(price);

        if (_variants.Count >= MaxVariantCount)
        {
            throw new InvalidOperationException(
                $"Product '{Id}' has reached the maximum number of variants ({MaxVariantCount}).");
        }

        if (_variants.Any(v => v.Sku.Value.Equals(sku.Value, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException($"Variant with sku '{sku.Value}' already exists for this product.");
        }

        if (_variants.Any(v => v.Barcode.Value.Equals(barcode.Value, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException($"Variant with barcode '{barcode.Value}' already exists for this product.");
        }

        var variant = ProductVariant.Create(Id, sku, barcode, color, size, price);
        _variants.Add(variant);

        RaiseEvent(
            new ProductVariantAddedDomainEvent(
                Id.Value,
                variant.Id.Value,
                variant.Sku.Value,
                variant.Barcode.Value,
                variant.Color,
                variant.Size,
                variant.Price.Amount,
                variant.Price.Currency),
            DateTimeOffset.UtcNow);

        return variant;
    }

    public void RemoveVariant(VariantId variantId)
    {
        EnsureNotArchived();
        ArgumentNullException.ThrowIfNull(variantId);

        var variant = _variants.FirstOrDefault(v => v.Id == variantId)
            ?? throw new InvalidOperationException($"Variant '{variantId}' not found on product '{Id}'.");

        _variants.Remove(variant);
        RaiseEvent(new ProductVariantRemovedDomainEvent(Id.Value, variantId.Value), DateTimeOffset.UtcNow);
    }

    public void ChangeVariantPrice(VariantId variantId, Money newPrice)
    {
        EnsureNotArchived();
        ArgumentNullException.ThrowIfNull(variantId);
        ArgumentNullException.ThrowIfNull(newPrice);

        var variant = RequireVariant(variantId);

        if (variant.Price.Amount == newPrice.Amount
            && string.Equals(variant.Price.Currency, newPrice.Currency, StringComparison.Ordinal))
        {
            return;
        }

        variant.ChangePrice(newPrice);
        RaiseEvent(
            new ProductVariantPriceChangedDomainEvent(
                Id.Value,
                variant.Id.Value,
                variant.Price.Amount,
                variant.Price.Currency),
            DateTimeOffset.UtcNow);
    }

    private ProductVariant RequireVariant(VariantId variantId) =>
        _variants.FirstOrDefault(v => v.Id == variantId)
        ?? throw new InvalidOperationException($"Variant '{variantId}' not found on product '{Id}'.");

    private void EnsureNotArchived()
    {
        if (Status == ProductStatus.Archived)
        {
            throw new InvalidOperationException($"Product '{Id}' is archived and cannot be modified.");
        }
    }

    private static string ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name is required.", nameof(name));
        }

        var trimmed = name.Trim();
        if (trimmed.Length > MaxNameLength)
        {
            throw new ArgumentException($"Name length must be <= {MaxNameLength}.", nameof(name));
        }

        return trimmed;
    }
}
