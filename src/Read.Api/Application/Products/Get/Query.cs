using MediatR;
using Write.Api.Domain.Products;

namespace Read.Api.Application.Products.Get;

public record Query(int Id) : IRequest<Product>;