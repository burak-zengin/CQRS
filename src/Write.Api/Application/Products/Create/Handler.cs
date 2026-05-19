using Domain.Products;
using Domain.Products.Repositories;
using MediatR;

namespace Write.Api.Application.Products.Create;

public class Handler(IProductWriteRepository repository) : IRequestHandler<Command, Guid>
{
    public async Task<Guid> Handle(Command request, CancellationToken cancellationToken)
    {
        var product = Product.Create(request.Name);

        await repository.AddAsync(product, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return product.Id.Value;
    }
}
