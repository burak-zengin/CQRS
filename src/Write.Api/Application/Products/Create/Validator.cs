using FluentValidation;

namespace Write.Api.Application.Products.Create;

public class Validator : AbstractValidator<Command>
{
    public Validator()
    {
        RuleFor(c => c.Name)
            .NotEmpty()
            .MaximumLength(200);
    }
}
