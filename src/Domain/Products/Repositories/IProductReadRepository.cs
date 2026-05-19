using Domain.Products.ReadModels;

namespace Domain.Products.Repositories;

public interface IProductReadRepository
{
    Task<ProductDetailReadModel?> GetDetailAsync(Guid id, CancellationToken cancellationToken);

    Task<List<ProductListReadModel>> GetListAsync(CancellationToken cancellationToken);
}
