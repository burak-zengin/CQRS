using FluentValidation;

namespace Write.Api.Application.Products.Create;

public class Validator : AbstractValidator<Command>
{
    public Validator()
    {
        RuleFor(c => c.Name).NotNull().NotEmpty();
        RuleFor(c => c.Barcode).NotNull().NotEmpty();
        RuleFor(c => c.Color).NotNull().NotEmpty();
        RuleFor(c => c.Size).NotNull().NotEmpty();
    }
}
