using FastEndpoints;
using MinigolfFriday.Domain.Models;

namespace MinigolfFriday.Host.Endpoints.Events;

public class EventsAdministrationGroup : Group
{
    public EventsAdministrationGroup()
    {
        Configure(
            "events",
            x =>
            {
                x.Roles(nameof(Role.Admin));
                x.Description(x => x.WithTags("Events"));
            }
        );
    }
}
