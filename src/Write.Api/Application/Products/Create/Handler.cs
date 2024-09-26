using Domain.Products;
using MediatR;
using Write.Api.Domain.Products;

namespace Write.Api.Application.Products.Create;

public class Handler(IProductWriteRepository repository) : IRequestHandler<Command, int>
{
    public async Task<int> Handle(Command request, CancellationToken cancellationToken)
    {
        return await repository.CreateAsync(new Product()
        {
            Barcode = request.Barcode,
            Color = request.Color,
            Name = request.Name,
            Size = request.Size
        });
    }
}
