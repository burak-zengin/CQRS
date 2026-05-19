using FluentValidation;

namespace Write.Api.Application.Products.AddVariant;

public class Validator : AbstractValidator<Command>
{
    public Validator()
    {
        RuleFor(c => c.ProductId).NotEqual(Guid.Empty);

        RuleFor(c => c.Sku)
            .NotEmpty()
            .Length(3, 32)
            .Matches("^[a-zA-Z0-9-]+$").WithMessage("Sku must be alphanumeric or contain hyphens.");

        RuleFor(c => c.Barcode)
            .NotEmpty()
            .Length(8, 14)
            .Matches("^[a-zA-Z0-9]+$").WithMessage("Barcode must be alphanumeric.");

        RuleFor(c => c.Color)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(c => c.Size)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(c => c.PriceAmount)
            .GreaterThanOrEqualTo(0);

        RuleFor(c => c.Currency)
            .NotEmpty()
            .Length(3)
            .Matches("^[A-Za-z]{3}$").WithMessage("Currency must be a 3-letter ISO code.");
    }
}
