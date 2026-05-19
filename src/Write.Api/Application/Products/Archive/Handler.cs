using Domain.Products.Repositories;
using Domain.Products.ValueObjects;
using MediatR;

namespace Write.Api.Application.Products.Archive;

public class Handler(IProductWriteRepository repository) : IRequestHandler<Command>
{
    public async Task Handle(Command request, CancellationToken cancellationToken)
    {
        var product = await repository.GetByIdAsync(ProductId.From(request.Id), cancellationToken)
            ?? throw new InvalidOperationException($"Product '{request.Id}' not found.");

        product.Archive();

        await repository.SaveChangesAsync(cancellationToken);
    }
}
