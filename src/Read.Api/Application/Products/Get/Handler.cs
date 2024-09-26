using Domain.Products;
using MediatR;
using Write.Api.Domain.Products;

namespace Read.Api.Application.Products.Get;

public class Handler(IProductReadRepository repository) : IRequestHandler<Query, Product>
{
    public async Task<Product> Handle(Query request, CancellationToken cancellationToken)
    {
        return await repository.GetAsync(request.Id);
    }
}
