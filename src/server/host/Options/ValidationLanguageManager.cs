using FluentValidation.Resources;

namespace MinigolfFriday.Host.Options;

public class ValidationLanguageManager : LanguageManager
{
    public ValidationLanguageManager()
    {
        AddTranslation("en", "NotNullValidator", "'{PropertyName}' is required.");
    }
}
