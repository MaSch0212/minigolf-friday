using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FluentAssertions.Primitives;

namespace MinigolfFriday.IntegrationTests.Assertions;

internal static class StringExtensions
{
    public static AndConstraint<StringAssertions> BeJwt(
        this StringAssertions should,
        string sub,
        DateTimeOffset exp,
        IEnumerable<string> roles
    )
    {
        var token = new JwtSecurityTokenHandler().ReadJwtToken(should.Subject);
        token
            .Claims.Where(x => x.Type == ClaimTypes.Role)
            .Select(x => x.Value)
            .Should()
            .BeEquivalentTo(roles);
        token
            .Claims.Where(x => x.Type == JwtRegisteredClaimNames.Sub)
            .Select(x => x.Value)
            .Should()
            .BeEquivalentTo([sub]);
        token
            .Claims.Where(x => x.Type == JwtRegisteredClaimNames.Exp)
            .Select(x => x.Value)
            .Should()
            .BeEquivalentTo([exp.ToUnixTimeSeconds().ToString()]);

        return new AndConstraint<StringAssertions>(should);
    }
}
