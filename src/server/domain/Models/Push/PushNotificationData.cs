using System.Globalization;

namespace MinigolfFriday.Domain.Models.Push;

public interface IPushNotificationData
{
    string Type { get; }

    string GetTitle(string lang);
    string GetBody(string lang);
}

public static class PushNotificationData
{
    private static readonly CultureInfo DeCulture = CultureInfo.GetCultureInfo("de");
    private static readonly CultureInfo EnCulture = CultureInfo.GetCultureInfo("en");

    public record EventPublished(string EventId, DateOnly Date) : IPushNotificationData
    {
        public string Type => "event-published";

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

    public record EventStarted(string EventId) : IPushNotificationData
    {
        public string Type => "event-started";

        public string GetTitle(string lang) =>
            NormalizeLang(lang) switch
            {
                "de" => "Die Räume wurden verteilt!",
                _ => "The rooms have been assigned!",
            };

        public string GetBody(string lang) =>
            NormalizeLang(lang) switch
            {
                "de" => "Schaue nach in welchen Räumen du spielst.",
                _ => "Check which rooms you are playing in.",
            };
    }

    public record EventInstanceUpdated(string EventId) : IPushNotificationData
    {
        public string Type => "event-instance-updated";

        public string GetTitle(string lang) =>
            NormalizeLang(lang) switch
            {
                "de" => "Dein Raum hat sich verändert!",
                _ => "Your room has changed!",
            };

        public string GetBody(string lang) =>
            NormalizeLang(lang) switch
            {
                "de" => "Schaue nach in welchen Räumen du jetzt spielst.",
                _ => "Check which rooms you are playing in now.",
            };
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
