using System.Text.Json.Serialization;

namespace MinigolfFriday.Domain.Models.RealtimeEvents;

public interface IRealtimeEvent
{
    static abstract string MethodName { get; }
}

public interface IGroupRealtimeEvent : IRealtimeEvent
{
    RealtimeEventGroup Group { get; }
}

public interface IUserRealtimeEvent : IRealtimeEvent
{
    string UserId { get; }
}

public static class RealtimeEvent
{
    /// <summary>Event that is triggered when a user changed.</summary>
    public record UserChanged(string UserId, RealtimeEventChangeType ChangeType)
        : IGroupRealtimeEvent
    {
        public static string MethodName => "UserChanged";

        [JsonIgnore]
        public RealtimeEventGroup Group => RealtimeEventGroup.Admin;
    }

    /// <summary>Event that is triggered when a map changed.</summary>
    public record MapChanged(string MapId, RealtimeEventChangeType ChangeType) : IGroupRealtimeEvent
    {
        public static string MethodName => "MapChanged";

        [JsonIgnore]
        public RealtimeEventGroup Group => RealtimeEventGroup.Admin;
    }

    /// <summary>Event that is triggered when an event changed (does not trigger for instances, preconfigurations or timeslots).</summary>
    public record EventChanged(string EventId, RealtimeEventChangeType ChangeType)
        : IGroupRealtimeEvent
    {
        public static string MethodName => "EventChanged";

        [JsonIgnore]
        public RealtimeEventGroup Group => RealtimeEventGroup.Admin;
    }

    /// <summary>Event that is triggered when a timeslot of an event changed (does not trigger for instances or preconfigurations).</summary>
    public record EventTimeslotChanged(
        string EventId,
        string EventTimeslotId,
        RealtimeEventChangeType ChangeType
    ) : IGroupRealtimeEvent
    {
        public static string MethodName => "EventTimeslotChanged";

        [JsonIgnore]
        public RealtimeEventGroup Group => RealtimeEventGroup.Admin;
    }

    /// <summary>Event that is triggered when instances of an event changed.</summary>
    public record EventInstancesChanged(string EventId) : IGroupRealtimeEvent
    {
        public static string MethodName => "EventInstancesChanged";

        [JsonIgnore]
        public RealtimeEventGroup Group => RealtimeEventGroup.Admin;
    }

    /// <summary>Event that is triggered when a preconfiguration of an event timeslot changed.</summary>
    public record EventPreconfigurationChanged(
        string EventId,
        string EventTimeslotId,
        string EventPreconfigurationId,
        RealtimeEventChangeType ChangeType
    ) : IGroupRealtimeEvent
    {
        public static string MethodName => "EventPreconfigurationChanged";

        [JsonIgnore]
        public RealtimeEventGroup Group => RealtimeEventGroup.Admin;
    }

    /// <summary>Event that is triggered when a player event changed.</summary>
    public record PlayerEventChanged(string EventId, RealtimeEventChangeType ChangeType)
        : IGroupRealtimeEvent
    {
        public static string MethodName => "PlayerEventChanged";

        [JsonIgnore]
        public RealtimeEventGroup Group => RealtimeEventGroup.Player;
    }

    /// <summary>Event that is triggered when registrations of a player for an event changed.</summary>
    public record PlayerEventRegistrationChanged(
        [property: JsonIgnore] string UserId,
        string EventId
    ) : IUserRealtimeEvent
    {
        public static string MethodName => "PlayerEventRegistrationChanged";
    }

    /// <summary>Event that is triggered when settings of a user changed.</summary>
    public record UserSettingsChanged([property: JsonIgnore] string UserId) : IUserRealtimeEvent
    {
        public static string MethodName => "UserSettingsChanged";
    }

    /// <summary>Event that is triggered when a player event timeslot registration changed.</summary>
    public record PlayerEventTimeslotRegistrationChanged(
        string EventId,
        string EventTimeslotId,
        string UserId,
        bool IsRegistered
    ) : IGroupRealtimeEvent
    {
        public static string MethodName => "PlayerEventTimeslotRegistrationChanged";

        [JsonIgnore]
        public RealtimeEventGroup Group => RealtimeEventGroup.All;
    }
}
