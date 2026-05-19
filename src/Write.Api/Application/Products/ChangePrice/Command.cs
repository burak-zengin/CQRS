using MediatR;

namespace Write.Api.Application.Products.ChangePrice;

public record Command(
    Guid ProductId,
    Guid VariantId,
    decimal Amount,
    string Currency) : IRequest;
