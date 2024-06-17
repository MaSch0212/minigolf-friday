using FastEndpoints;

namespace MinigolfFriday.Host.Endpoints.UserSettings;

public class UserSettingsGroup : Group
{
    public UserSettingsGroup()
    {
        Configure(
            "usersettings",
            x =>
            {
                x.Description(x => x.WithTags("UserSettings"));
            }
        );
    }
}
