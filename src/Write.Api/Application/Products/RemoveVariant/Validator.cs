using FluentValidation;

namespace Write.Api.Application.Products.RemoveVariant;

public class Validator : AbstractValidator<Command>
{
    public Validator()
    {
        RuleFor(c => c.ProductId).NotEqual(Guid.Empty);
        RuleFor(c => c.VariantId).NotEqual(Guid.Empty);
    }
}
