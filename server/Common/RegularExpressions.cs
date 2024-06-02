using System.Text.RegularExpressions;

namespace MinigolfFriday.Common;

public partial class RegularExpressions
{
    [GeneratedRegex("(?<=\\{)[^\\}]+(?=\\})")]
    public static partial Regex RouteParams();
}
