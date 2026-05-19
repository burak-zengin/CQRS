using MediatR;

namespace Write.Api.Application.Products.RemoveVariant;

public record Command(Guid ProductId, Guid VariantId) : IRequest;
