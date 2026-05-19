using Domain.Products.Repositories;
using Domain.Products.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Write.Api.Infrastructure.Persistence;

namespace Write.Api.Infrastructure.Repositories;

public sealed class BarcodeUniquenessChecker(WriteDbContext context) : IBarcodeUniquenessChecker
{
    public async Task<bool> IsTakenAsync(
        Barcode barcode,
        ProductId excludeProductId,
        CancellationToken cancellationToken)
    {
        return await context.ProductVariants
            .AsNoTracking()
            .AnyAsync(
                v => v.Barcode == barcode && v.ProductId != excludeProductId,
                cancellationToken);
    }
}
