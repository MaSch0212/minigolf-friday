using FluentValidation;
using MinigolfFriday.Models;

namespace MinigolfFriday.Validators;

public class PlayerValidator : AbstractValidator<Player>
{
    public PlayerValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.PlayerPreferences).NotNull().SetValidator(new PlayerPreferencesValidator());
    }
}

public class PlayerPreferencesValidator : AbstractValidator<PlayerPreferences>
{
    public PlayerPreferencesValidator()
    {
        RuleFor(x => x.Avoid).NotNull();
        RuleFor(x => x.Prefer).NotNull();
    }
}
