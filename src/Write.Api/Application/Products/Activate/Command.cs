using MediatR;

namespace Write.Api.Application.Products.Activate;

public record Command(Guid Id) : IRequest;
