using FluentValidation;

namespace Write.Api.Application.Products.Rename;

public class Validator : AbstractValidator<Command>
{
    public Validator()
    {
        RuleFor(c => c.Id).NotEqual(Guid.Empty);

        RuleFor(c => c.Name)
            .NotEmpty()
            .MaximumLength(200);
    }
}
