using FastEndpoints;

namespace MinigolfFriday.Host.Endpoints.Administration.Events;

public class EventAdministrationGroup : Group
{
    public EventAdministrationGroup()
    {
        Configure(
            "events",
            x =>
            {
                x.Group<AdministrationGroup>();
                x.Description(x => x.WithTags("Event Administration"));
            }
        );
    }
}
