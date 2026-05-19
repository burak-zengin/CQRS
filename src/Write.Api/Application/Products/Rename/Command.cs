using MediatR;

namespace Write.Api.Application.Products.Rename;

public record Command(Guid Id, string Name) : IRequest;
