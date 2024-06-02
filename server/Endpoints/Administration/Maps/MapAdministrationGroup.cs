using FastEndpoints;

namespace MinigolfFriday.Endpoints.Administration.Maps;

public class MapAdministrationGroup : Group
{
    public MapAdministrationGroup()
    {
        Configure(
            "maps",
            x =>
            {
                x.Group<AdministrationGroup>();
                x.Description(x => x.WithTags("Map Administration"));
            }
        );
    }
}
