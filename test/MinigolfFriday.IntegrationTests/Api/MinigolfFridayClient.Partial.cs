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
    }
}
