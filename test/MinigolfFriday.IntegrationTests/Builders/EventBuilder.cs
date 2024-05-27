namespace MinigolfFriday.IntegrationTests.Builders;

internal sealed class EventBuilder
{
    private static object _lastLock = new();
    private static DateTime _next = new(2020, 1, 1);

    private readonly Sut _sut;
    private readonly CreateEventRequest _request;
    private readonly List<EventTimeslotBuilder> _timeslots = [];

    public EventBuilder(Sut sut)
    {
        _sut = sut;
        lock (_lastLock)
        {
            var date = _next;
            _next = _next.AddDays(1);
            _request = new() { Date = date, RegistrationDeadline = date.AddHours(12) };
        }
    }

    public EventBuilder AtDate(DateTime date)
    {
        _request.Date = date;
        return this;
    }

    public EventBuilder WithRegistrationDeadline(DateTime registrationDeadline)
    {
        _request.RegistrationDeadline = registrationDeadline;
        return this;
    }

    public EventBuilder WithTimeslot(string mapId, Action<EventTimeslotBuilder>? configure = null)
    {
        var builder = new EventTimeslotBuilder(_sut, new(_timeslots.Count + 13, 0, 0), mapId);
        configure?.Invoke(builder);
        _timeslots.Add(builder);
        return this;
    }

    public async Task<Event> BuildAsync()
    {
        var response = await _sut.AppClient.CreateEventAsync(_request);
        response.Event.Timeslots = await Task.WhenAll(
            _timeslots.Select(x => x.BuildAsync(response.Event.Id))
        );
        return response.Event;
    }

    public async Task<Event[]> BuildAsync(int amount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(amount);
        return amount switch
        {
            0 => [],
            1 => [await BuildAsync()],
            _
                => await Task.WhenAll(
                    Enumerable
                        .Range(0, amount)
                        .Select(async i =>
                        {
                            var request = new CreateEventRequest
                            {
                                Date = _request.Date.AddDays(i),
                                RegistrationDeadline = _request.RegistrationDeadline.AddDays(i)
                            };
                            var response = await _sut.AppClient.CreateEventAsync(request);
                            response.Event.Timeslots = await Task.WhenAll(
                                _timeslots.Select(x => x.BuildAsync(response.Event.Id))
                            );
                            return response.Event;
                        })
                )
        };
    }
}
