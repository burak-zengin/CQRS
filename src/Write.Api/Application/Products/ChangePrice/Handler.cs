using Domain.Products.Repositories;
using Domain.Products.ValueObjects;
using MediatR;

namespace Write.Api.Application.Products.ChangePrice;

public class Handler(IProductWriteRepository repository) : IRequestHandler<Command>
{
    public async Task Handle(Command request, CancellationToken cancellationToken)
    {
        var product = await repository.GetByIdAsync(ProductId.From(request.ProductId), cancellationToken)
            ?? throw new InvalidOperationException($"Product '{request.ProductId}' not found.");

        product.ChangeVariantPrice(
            VariantId.From(request.VariantId),
            Money.Create(request.Amount, request.Currency));

        await repository.SaveChangesAsync(cancellationToken);
    }
}
