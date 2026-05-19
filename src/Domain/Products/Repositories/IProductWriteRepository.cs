using Domain.Products.ValueObjects;

namespace Domain.Products.Repositories;

public interface IProductWriteRepository
{
    Task AddAsync(Product product, CancellationToken cancellationToken);

    Task<Product?> GetByIdAsync(ProductId id, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
