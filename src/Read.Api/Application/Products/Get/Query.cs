using Domain.Products.ReadModels;
using MediatR;

namespace Read.Api.Application.Products.Get;

public record Query(Guid Id) : IRequest<ProductDetailReadModel?>;
