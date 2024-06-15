using FastEndpoints;

namespace MinigolfFriday.Host.Endpoints.Administration.Users;

public class UserAdministrationGroup : Group
{
    public UserAdministrationGroup()
    {
        Configure(
            "users",
            x =>
            {
                x.Group<AdministrationGroup>();
                x.Description(x => x.WithTags("User Administration"));
            }
        );
    }
}
