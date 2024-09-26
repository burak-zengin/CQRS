using Write.Api.Domain.Products;

namespace Domain.Products;

public interface IProductWriteRepository
{
    public Task<int> CreateAsync(Product product, CancellationToken cancellationToken);
}
