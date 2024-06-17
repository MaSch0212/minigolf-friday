using FastEndpoints;

namespace MinigolfFriday.Host.Endpoints.UserSettings;

public class UserSettingsGroup : Group
{
    public UserSettingsGroup()
    {
        Configure(
            "user/settings",
            x =>
            {
                x.Description(x => x.WithTags("UserSettings"));
            }
        );
    }
}
