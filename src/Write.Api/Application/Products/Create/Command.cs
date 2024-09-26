using MediatR;

namespace Write.Api.Application.Products.Create;

public record Command(string Name, string Barcode, string Color, string Size) : IRequest<int>;
