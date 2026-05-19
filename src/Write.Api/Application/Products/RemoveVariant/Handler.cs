using Domain.Products.Repositories;
using Domain.Products.ValueObjects;
using MediatR;

namespace Write.Api.Application.Products.RemoveVariant;

public class Handler(IProductWriteRepository repository) : IRequestHandler<Command>
{
    public async Task Handle(Command request, CancellationToken cancellationToken)
    {
        var productId = ProductId.From(request.ProductId);

        var product = await repository.GetByIdAsync(productId, cancellationToken)
            ?? throw new InvalidOperationException($"Product '{request.ProductId}' not found.");

        product.RemoveVariant(VariantId.From(request.VariantId));

        await repository.SaveChangesAsync(cancellationToken);
    }
}
