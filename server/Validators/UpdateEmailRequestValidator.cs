using FluentValidation;

namespace MinigolfFriday.Validators;

public class UpdateEmailRequestValidator : AbstractValidator<UpdateEmailRequest>
{
    public UpdateEmailRequestValidator()
    {
        RuleFor(x => x.NewEmail).NotEmpty().EmailAddress();
    }
}
