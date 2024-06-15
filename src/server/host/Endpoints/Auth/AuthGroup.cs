using FastEndpoints;

namespace MinigolfFriday.Host.Endpoints.Auth;

public class AuthGroup : Group
{
    public AuthGroup()
    {
        Configure(
            "auth",
            x =>
            {
                x.Description(x => x.WithTags("Authentication"));
            }
        );
    }
}
