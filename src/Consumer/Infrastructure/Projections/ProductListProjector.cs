using Domain.Products.Projections;
using Domain.Products.ReadModels;

namespace Consumer.Infrastructure.Projections;

public sealed class ProductListProjector : IProductListProjector
{
    public ProductListReadModel Project(ProductDetailReadModel detail)
    {
        var colorOptions = detail.Variants
            .Select(v => v.Color)
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .Select(c => c.ToLowerInvariant())
            .Distinct()
            .ToArray();

        var sizeOptions = detail.Variants
            .Select(v => v.Size)
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(s => s.ToLowerInvariant())
            .Distinct()
            .ToArray();

        var tags = colorOptions.Concat(sizeOptions).Distinct().ToArray();

        var searchText = string.Join(
            ' ',
            new[] { detail.Name }
                .Concat(detail.Variants.SelectMany(v => new[] { v.Sku, v.Barcode, v.Color, v.Size }))
                .Where(s => !string.IsNullOrWhiteSpace(s)))
            .ToLowerInvariant();

        return new ProductListReadModel
        {
            Id = detail.Id,
            Name = detail.Name,
            Status = detail.Status,
            IsAvailable = detail.IsAvailable,
            VariantCount = detail.VariantCount,
            MinPriceAmount = detail.MinPriceAmount,
            MaxPriceAmount = detail.MaxPriceAmount,
            Currency = detail.Currency,
            PriceRangeDisplay = detail.PriceRangeDisplay,
            ColorOptions = colorOptions,
            SizeOptions = sizeOptions,
            Tags = tags,
            SearchText = searchText,
            CreatedAt = detail.CreatedAt,
            UpdatedAt = detail.UpdatedAt,
            Version = detail.Version,
            LastEventId = detail.LastEventId,
            LastEventAt = detail.LastEventAt,
            ProjectedAt = detail.ProjectedAt,
            SourceOperation = detail.SourceOperation
        };
    }
}
