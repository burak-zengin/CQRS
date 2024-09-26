using Domain.Products;
using MediatR;
using Write.Api.Domain.Products;

namespace Read.Api.Application.Products.GetAll;

public class Handler(IProductReadRepository repository) : IRequestHandler<Query, List<Product>>
{
    public async Task<List<Product>> Handle(Query request, CancellationToken cancellationToken)
    {
        return await repository.GetAllAsync(cancellationToken);
    }
}
