using Domain.Products.ReadModels;

namespace Domain.Products.Repositories;

public interface IProductProjectionRepository
{
    Task<ProductDetailReadModel?> GetDetailAsync(Guid productId, CancellationToken cancellationToken);

    Task UpsertDetailAsync(ProductDetailReadModel detail, CancellationToken cancellationToken);

    Task UpsertListAsync(ProductListReadModel list, CancellationToken cancellationToken);

    Task DeleteAsync(Guid productId, CancellationToken cancellationToken);

    Task<bool> HasProcessedAsync(Guid eventId, CancellationToken cancellationToken);

    Task MarkProcessedAsync(Guid eventId, CancellationToken cancellationToken);
}
