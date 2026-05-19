using FluentValidation;

namespace Write.Api.Application.Products.ChangePrice;

public class Validator : AbstractValidator<Command>
{
    public Validator()
    {
        RuleFor(c => c.ProductId).NotEqual(Guid.Empty);
        RuleFor(c => c.VariantId).NotEqual(Guid.Empty);
        RuleFor(c => c.Amount).GreaterThanOrEqualTo(0);
        RuleFor(c => c.Currency)
            .NotEmpty()
            .Length(3)
            .Matches("^[A-Za-z]{3}$").WithMessage("Currency must be a 3-letter ISO code.");
    }
}
