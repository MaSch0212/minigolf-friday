using System.ComponentModel.DataAnnotations;
using FastEndpoints;
using Microsoft.Extensions.Options;
using MinigolfFriday.Domain.Options;

namespace MinigolfFriday.Host.Endpoints;

public record GetWellKnownConfigurationResponse([property: Required] string VapidPublicKey);

public class GetWellKnownConfigurationEndpoint(IOptions<WebPushOptions> webPushOptions)
    : EndpointWithoutRequest<GetWellKnownConfigurationResponse>
{
    public override void Configure()
    {
        Get(".well-known");
        Description(x => x.WithTags("WellKnown"));
        AllowAnonymous();
        RoutePrefixOverride("");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        await SendAsync(
            new GetWellKnownConfigurationResponse(webPushOptions.Value.PublicKey),
            cancellation: ct
        );
    }
}
