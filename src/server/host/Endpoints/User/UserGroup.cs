using FastEndpoints;

namespace MinigolfFriday.Host.Endpoints.User;

public class UserGroup : Group
{
    public UserGroup()
    {
        Configure("user", x => { });
    }
}
