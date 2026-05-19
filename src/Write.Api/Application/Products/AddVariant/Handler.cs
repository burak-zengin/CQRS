using Domain.Products.Repositories;
using Domain.Products.ValueObjects;
using MediatR;

namespace Write.Api.Application.Products.AddVariant;

public class Handler(
    IProductWriteRepository repository,
    IBarcodeUniquenessChecker barcodeUniquenessChecker) : IRequestHandler<Command, Guid>
{
    public async Task<Guid> Handle(Command request, CancellationToken cancellationToken)
    {
        var productId = ProductId.From(request.ProductId);

        var product = await repository.GetByIdAsync(productId, cancellationToken)
            ?? throw new InvalidOperationException($"Product '{request.ProductId}' not found.");

        var sku = Sku.Create(request.Sku);
        var barcode = Barcode.Create(request.Barcode);
        var price = Money.Create(request.PriceAmount, request.Currency);

        if (await barcodeUniquenessChecker.IsTakenAsync(barcode, productId, cancellationToken))
        {
            throw new InvalidOperationException($"Barcode '{barcode.Value}' is already used by another product.");
        }

        var variant = product.AddVariant(
            sku,
            barcode,
            request.Color,
            request.Size,
            price);

        await repository.SaveChangesAsync(cancellationToken);

        return variant.Id.Value;
    }
}
