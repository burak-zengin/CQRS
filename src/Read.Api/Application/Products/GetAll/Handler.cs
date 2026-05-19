using Domain.Products.ReadModels;
using Domain.Products.Repositories;
using MediatR;

namespace Read.Api.Application.Products.GetAll;

public class Handler(IProductReadRepository repository) : IRequestHandler<Query, List<ProductListReadModel>>
{
    public async Task<List<ProductListReadModel>> Handle(Query request, CancellationToken cancellationToken)
    {
        return await repository.GetListAsync(cancellationToken);
    }
}
