using FastEndpoints;
using MinigolfFriday.Host.Endpoints.User;

namespace MinigolfFriday.Host.Endpoints.User.Settings;

public class UserSettingsGroup : Group
{
    public UserSettingsGroup()
    {
        Configure(
            "settings",
            x =>
            {
                x.Group<UserGroup>();
                x.Description(x => x.WithTags("UserSettings"));
            }
        );
    }
}
