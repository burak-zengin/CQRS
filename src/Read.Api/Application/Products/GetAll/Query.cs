using MediatR;
using Write.Api.Domain.Products;

namespace Read.Api.Application.Products.GetAll;

public record Query : IRequest<List<Product>>;