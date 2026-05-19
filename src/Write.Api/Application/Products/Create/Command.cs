using MediatR;

namespace Write.Api.Application.Products.Create;

public record Command(string Name) : IRequest<Guid>;
