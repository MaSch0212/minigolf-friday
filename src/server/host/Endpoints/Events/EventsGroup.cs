using FastEndpoints;
using MinigolfFriday.Domain.Models;

namespace MinigolfFriday.Endpoints.Events;

public class EventsGroup : Group
{
    public EventsGroup()
    {
        Configure(
            "events",
            x =>
            {
                x.Roles(nameof(Role.Player));
                x.Description(x => x.WithTags("Events"));
            }
        );
    }
}
