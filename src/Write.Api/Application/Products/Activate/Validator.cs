using FluentValidation;

namespace Write.Api.Application.Products.Activate;

public class Validator : AbstractValidator<Command>
{
    public Validator()
    {
        RuleFor(c => c.Id).NotEqual(Guid.Empty);
    }
}
