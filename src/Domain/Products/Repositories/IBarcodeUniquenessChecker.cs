using Domain.Products.ValueObjects;

namespace Domain.Products.Repositories;

public interface IBarcodeUniquenessChecker
{
    Task<bool> IsTakenAsync(Barcode barcode, ProductId excludeProductId, CancellationToken cancellationToken);
}
