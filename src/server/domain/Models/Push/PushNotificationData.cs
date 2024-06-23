using System.Globalization;
using System.Text;
using System.Text.Json.Serialization;

namespace MinigolfFriday.Domain.Models.Push;

public interface IPushNotificationData
{
    string Type { get; }
    Dictionary<string, PushNotificationOnActionClick> OnActionClick { get; }

    string GetTitle(string lang);
    string GetBody(string lang);
}

public record NotificationTimeslotInfo(
    TimeOnly Time,
    string GroupCode,
    string MapName,
    int PlayerCount
);

public static class PushNotificationData
{
    private static readonly CultureInfo DeCulture = CultureInfo.GetCultureInfo("de");
    private static readonly CultureInfo EnCulture = CultureInfo.GetCultureInfo("en");

    public record EventPublished(string EventId, DateOnly Date) : IPushNotificationData
    {
        public string Type => "event-published";

        public Dictionary<string, PushNotificationOnActionClick> OnActionClick =>
            new() { { "default", new($"/events/{EventId}") } };

        public string GetTitle(string lang) =>
            NormalizeLang(lang) switch
            {
                "de" => "Es wird wieder ⛳ gespielt!",
                _ => "⛳ is being played again!",
            };

        public string GetBody(string lang) =>
            NormalizeLang(lang) switch
            {
                "de"
                    => string.Create(
                        DeCulture,
                        $"Du kannst dich ab jetzt für den {Date:m} anmelden."
                    ),
                _ => string.Create(EnCulture, $"You can now register for the {Date:m}."),
            };
    }

    public record EventStarted(
        string EventId,
        [property: JsonIgnore] NotificationTimeslotInfo[] Timeslots
    ) : IPushNotificationData
    {
        public string Type => "event-started";

        public Dictionary<string, PushNotificationOnActionClick> OnActionClick =>
            new() { { "default", new($"/events/{EventId}") } };

        public string GetTitle(string lang) =>
            NormalizeLang(lang) switch
            {
                "de" => "Die Räume wurden verteilt!",
                _ => "The rooms have been assigned!",
            };

        public string GetBody(string lang)
        {
            lang = NormalizeLang(lang);
            var builder = new StringBuilder();
            foreach (var timeslot in Timeslots)
            {
                builder.Append(
                    lang switch
                    {
                        "de"
                            => string.Create(
                                DeCulture,
                                $"um {timeslot.Time:t} Uhr spielst du in \"{timeslot.GroupCode.ToUpper(DeCulture)}\" auf \"{timeslot.MapName}\" in einem Spiel mit {timeslot.PlayerCount} Spielern.\n"
                            ),
                        _
                            => string.Create(
                                EnCulture,
                                $"at {timeslot.Time:t} you play in \"{timeslot.GroupCode.ToUpper(EnCulture)}\" on \"{timeslot.MapName}\" in a game with {timeslot.PlayerCount} players.\n"
                            ),
                    }
                );
            }
            builder.Append(
                lang switch
                {
                    "de" => "Viel Spaß!",
                    _ => "Have fun!",
                }
            );
            return builder.ToString();
        }
    }

    public record EventInstanceUpdated(
        string EventId,
        [property: JsonIgnore] NotificationTimeslotInfo[] Timeslots
    ) : IPushNotificationData
    {
        public string Type => "event-instance-updated";

        public Dictionary<string, PushNotificationOnActionClick> OnActionClick =>
            new() { { "default", new($"/events/{EventId}") } };

        public string GetTitle(string lang) =>
            NormalizeLang(lang) switch
            {
                "de" => "Dein Raum hat sich verändert!",
                _ => "Your room has changed!",
            };

        public string GetBody(string lang)
        {
            lang = NormalizeLang(lang);
            var builder = new StringBuilder();
            foreach (var timeslot in Timeslots)
            {
                builder.Append(
                    lang switch
                    {
                        "de"
                            => string.Create(
                                DeCulture,
                                $"um {timeslot.Time:t} Uhr spielst du in \"{timeslot.GroupCode.ToUpper(DeCulture)}\" auf \"{timeslot.MapName}\" in einem Spiel mit {timeslot.PlayerCount} Spielern.\n"
                            ),
                        _
                            => string.Create(
                                EnCulture,
                                $"at {timeslot.Time:t} you play in \"{timeslot.GroupCode.ToUpper(EnCulture)}\" on \"{timeslot.MapName}\" in a game with {timeslot.PlayerCount} players.\n"
                            ),
                    }
                );
            }
            builder.Append(
                lang switch
                {
                    "de" => "Viel Spaß!",
                    _ => "Have fun!",
                }
            );
            return builder.ToString();
        }
    }

    public record EventTimeslotStarting(
        string EventId,
        TimeSpan TimeToStart,
        int PlayerCount,
        string GroupCode,
        string MapName
    ) : IPushNotificationData
    {
        public string Type => "event-timeslot-starting";

        public Dictionary<string, PushNotificationOnActionClick> OnActionClick =>
            new() { { "default", new($"/events/{EventId}") } };

        public string GetTitle(string lang) =>
            NormalizeLang(lang) switch
            {
                "de"
                    => string.Create(
                        DeCulture,
                        $"In {TimeToStart.TotalMinutes:0} Minuten geht es los!"
                    ),
                _
                    => string.Create(
                        EnCulture,
                        $"It starts in {TimeToStart.TotalMinutes:0} minutes!"
                    ),
            };

        public string GetBody(string lang) =>
            NormalizeLang(lang) switch
            {
                "de"
                    => string.Create(
                        DeCulture,
                        $"Du Spielst mit {PlayerCount - 1} Spielern im Raum {GroupCode} auf Bahn {MapName}."
                    ),
                _
                    => string.Create(
                        EnCulture,
                        $"You play with {PlayerCount - 1} players in room {GroupCode} on map {MapName}."
                    ),
            };
    }

    private static string NormalizeLang(string lang)
    {
        if (lang.StartsWith("de", StringComparison.OrdinalIgnoreCase))
            return "de";
        return "en";
    }
}
