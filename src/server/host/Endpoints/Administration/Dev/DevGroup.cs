using FastEndpoints;

namespace MinigolfFriday.Endpoints.Administration.Dev;

public class DevGroup : Group
{
    public DevGroup()
    {
        Configure(
            "dev",
            x =>
            {
                x.AllowAnonymous();
                x.Description(x => x.WithTags("Development"));
            }
        );
    }
}
