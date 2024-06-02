using FluentValidation.Resources;

namespace MinigolfFriday.Common;

public class ValidationLanguageManager : LanguageManager
{
    public ValidationLanguageManager()
    {
        AddTranslation("en", "NotNullValidator", "'{PropertyName}' is required.");
    }
}
