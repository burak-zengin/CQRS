using Domain.Products.ReadModels;
using MediatR;

namespace Read.Api.Application.Products.GetAll;

public record Query : IRequest<List<ProductListReadModel>>;
