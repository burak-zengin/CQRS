using Domain.Products;
using Write.Api.Domain.Products;
using Write.Api.Persistence;

namespace Write.Api.Infrustructure.Repositories;

public class ProductRepository(WriteDbContext context) : IProductWriteRepository
{
    public async Task<int> CreateAsync(Product product, CancellationToken cancellationToken)
    {
        context.Add(product);
        await context.SaveChangesAsync(cancellationToken);

        return product.Id;
    }
}
