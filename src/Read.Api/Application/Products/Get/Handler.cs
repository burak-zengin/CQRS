using Domain.Products.ReadModels;
using Domain.Products.Repositories;
using MediatR;

namespace Read.Api.Application.Products.Get;

public class Handler(IProductReadRepository repository) : IRequestHandler<Query, ProductDetailReadModel?>
{
    public async Task<ProductDetailReadModel?> Handle(Query request, CancellationToken cancellationToken)
    {
        return await repository.GetDetailAsync(request.Id, cancellationToken);
    }
}
