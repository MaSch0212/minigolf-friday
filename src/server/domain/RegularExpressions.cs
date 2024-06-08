using System.Text.RegularExpressions;

namespace MinigolfFriday.Domain;

public partial class RegularExpressions
{
    [GeneratedRegex("(?<=\\{)[^\\}]+(?=\\})")]
    public static partial Regex RouteParams();
}
