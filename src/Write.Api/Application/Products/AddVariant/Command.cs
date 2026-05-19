using MediatR;

namespace Write.Api.Application.Products.AddVariant;

public record Command(
    Guid ProductId,
    string Sku,
    string Barcode,
    string Color,
    string Size,
    decimal PriceAmount,
    string Currency) : IRequest<Guid>;
