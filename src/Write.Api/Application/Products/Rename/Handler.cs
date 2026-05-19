using Domain.Products.Repositories;
using Domain.Products.ValueObjects;
using MediatR;

namespace Write.Api.Application.Products.Rename;

public class Handler(IProductWriteRepository repository) : IRequestHandler<Command>
{
    public async Task Handle(Command request, CancellationToken cancellationToken)
    {
        var productId = ProductId.From(request.Id);

        var product = await repository.GetByIdAsync(productId, cancellationToken)
            ?? throw new InvalidOperationException($"Product '{request.Id}' not found.");

        product.Rename(request.Name);

        await repository.SaveChangesAsync(cancellationToken);
    }
}
