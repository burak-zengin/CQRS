using MediatR;

namespace Write.Api.Application.Products.Archive;

public record Command(Guid Id) : IRequest;
