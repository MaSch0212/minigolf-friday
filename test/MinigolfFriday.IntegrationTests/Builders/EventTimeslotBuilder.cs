namespace MinigolfFriday.IntegrationTests.Builders;

internal sealed class EventTimeslotBuilder(Sut sut, TimeSpan time, string mapId)
{
    private readonly List<EventInstancePreconfigurationBuilder> _preconfigurations = [];
    private readonly CreateEventTimeslotRequest _request = new() { Time = time, MapId = mapId };

    public EventTimeslotBuilder WithPreconfiguration(
        Action<EventInstancePreconfigurationBuilder>? configure = null
    )
    {
        var builder = new EventInstancePreconfigurationBuilder(sut);
        configure?.Invoke(builder);
        _preconfigurations.Add(builder);
        return this;
    }

    public EventTimeslotBuilder AtTime(TimeSpan time)
    {
        _request.Time = time;
        return this;
    }

    public EventTimeslotBuilder WithMap(string mapId)
    {
        _request.MapId = mapId;
        return this;
    }

    public EventTimeslotBuilder AllowFallback()
    {
        _request.IsFallbackAllowed = true;
        return this;
    }

    public async Task<EventTimeslot> BuildAsync(string eventId)
    {
        var timeslotResponse = await sut.AppClient.CreateEventTimeslotAsync(eventId, _request);
        timeslotResponse.Timeslot.Preconfigurations = await Task.WhenAll(
            _preconfigurations.Select(x => x.BuildAsync(timeslotResponse.Timeslot.Id))
        );
        return timeslotResponse.Timeslot;
    }
}
