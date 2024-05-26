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
}

partial class ApiException<TResult>
{
    public override string Message =>
      $"{base.Message.Trim()}\nResult: {JsonConvert.SerializeObject(Result, Formatting.Indented)}";
}
