using Write.Api.Domain.Products;

namespace Domain.Products;

public interface IProductReadRepository
{
    Task<Product> GetAsync(int id, CancellationToken cancellationToken);

    Task<List<Product>> GetAllAsync(CancellationToken cancellationToken);
}
