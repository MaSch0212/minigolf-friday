using FastEndpoints;
using MinigolfFriday.Domain.Models;

namespace MinigolfFriday.Endpoints.Administration;

public class AdministrationGroup : Group
{
    public AdministrationGroup()
    {
        Configure(
            "administration",
            x =>
            {
                x.Roles(nameof(Role.Admin));
            }
        );
    }
}
