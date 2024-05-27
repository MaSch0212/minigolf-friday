using System.Net.Http.Headers;
using Newtonsoft.Json;

namespace MinigolfFriday.IntegrationTests.Api;

partial class MinigolfFridayClient
{
    public string? Token { get; set; }

    partial void PrepareRequest(HttpClient client, HttpRequestMessage request, string url)
    {
        if (Token != null)
        {
            request.Headers.Authorization = new("Bearer", Token);
        }

        // Workaround for POST requests with empty body
        if (
            request.Content is StringContent stringContent
            && stringContent.Headers.ContentLength == 0
            && stringContent.Headers.ContentType?.MediaType == "application/json"
        )
        {
            request.Content = null;
        }
    }

    public async Task ResetDatabase(CancellationToken ct = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, _baseUrl + "api/dev/resetdb");
        PrepareRequest(_httpClient, request, (string)null!);
        var response = await _httpClient.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();
    }

    public async Task SetRegistrationDeadline(
        string eventId,
        DateTimeOffset deadline,
        CancellationToken ct = default
    )
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            _baseUrl + "api/dev/setdeadline"
        );
        var json = JsonConvert.SerializeObject(
            new { EventId = eventId, Deadline = deadline },
            _settings.Value
        );
        request.Content = new StringContent(json, MediaTypeHeaderValue.Parse("application/json"));
        PrepareRequest(_httpClient, request, (string)null!);
        var response = await _httpClient.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();
    }
}

partial class ApiException<TResult>
{
    public override string Message =>
        $"{base.Message.Trim()}\nResult: {JsonConvert.SerializeObject(Result, Formatting.Indented)}";
}
