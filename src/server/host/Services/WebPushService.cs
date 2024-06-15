using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MinigolfFriday.Data;
using MinigolfFriday.Domain.Models;
using MinigolfFriday.Domain.Models.Push;
using MinigolfFriday.Domain.Options;
using WebPush;

namespace MinigolfFriday.Host.Services;

[GenerateAutoInterface]
public sealed class WebPushService(
    IHttpClientFactory httpClientFactory,
    IOptions<WebPushOptions> webPushOptions,
    IConfigureOptions<JsonSerializerOptions> configureJsonSerializerOptions,
    DatabaseContext databaseContext,
    ILogger<WebPushService> logger
) : IWebPushService, IDisposable
{
    private readonly JsonSerializerOptions _serializerOptions = GetJsonSerializerOptions(
        configureJsonSerializerOptions
    );
    private readonly Lazy<WebPushClient> _webPushClient =
        new(() => new(httpClientFactory.CreateClient()));

    private static JsonSerializerOptions GetJsonSerializerOptions(
        IConfigureOptions<JsonSerializerOptions> configureOptions
    )
    {
        var options = new JsonSerializerOptions() { WriteIndented = false };
        configureOptions.Configure(options);
        return options;
    }

    public async Task SendAsync(
        IEnumerable<UserPushSubscription> subscriptions,
        IPushNotificationData data,
        CancellationToken cancellation = default
    )
    {
        var subscriptionsList = subscriptions.ToList();
        logger.LogDebug(
            "Sending push notification with data {Data} to {Count} subscriptions",
            data,
            subscriptionsList.Count
        );
        await Task.WhenAll(
            subscriptionsList.Select(async subscription =>
            {
                var payload = PushNotificationPayload.Create(subscription.Lang, data);
                try
                {
                    logger.LogDebug(
                        "Sending push notification to {Endpoint} with payload {Payload}",
                        subscription.Endpoint,
                        payload
                    );
                    await _webPushClient.Value.SendNotificationAsync(
                        subscription,
                        JsonSerializer.Serialize(
                            new { Notification = payload },
                            _serializerOptions
                        ),
                        webPushOptions.Value,
                        cancellationToken: cancellation
                    );
                    logger.LogDebug(
                        "Push notification to {Endpoint} sent successfully",
                        subscription.Endpoint
                    );
                }
                catch (WebPushException ex)
                {
                    logger.LogDebug(
                        ex,
                        "Failed to send push notification to {Endpoint}",
                        subscription.Endpoint
                    );
                    try
                    {
                        await HandleWebPushException(subscription, payload, ex, cancellation);
                    }
                    catch (Exception ex2)
                    {
                        logger.LogError(
                            ex2,
                            "Failed to handle WebPushException for {Endpoint}",
                            subscription.Endpoint
                        );
                    }
                }
            })
        );
    }

    public void Dispose()
    {
        if (_webPushClient.IsValueCreated)
            _webPushClient.Value.Dispose();
    }

    private async Task HandleWebPushException(
        UserPushSubscription subscription,
        PushNotificationPayload payload,
        WebPushException ex,
        CancellationToken cancellation
    )
    {
        DateTimeOffset? retryDate = null;

        switch (ex.StatusCode)
        {
            case HttpStatusCode.NotFound:
            case HttpStatusCode.Gone:
                // Subscription is not available anymore (e.g. user unsubscribed)
                logger.LogInformation(
                    "Subscription {Endpoint} is not available anymore. Removing it.",
                    subscription.Endpoint
                );
                await RemoveSubscriptionAsync(subscription, cancellation);
                break;

            case HttpStatusCode.NotAcceptable:
            case HttpStatusCode.TooManyRequests:
                // Too many notifications sent. Retry after the specified time
                logger.LogWarning(
                    "Failed to send push notification to {Endpoint} with payload {Payload} due to rate limiting.",
                    subscription.Endpoint,
                    payload
                );
                retryDate = ex.Headers.RetryAfter?.Date;
                break;

            case HttpStatusCode.ServiceUnavailable:
                // Service is unavailable. Check if the sending can be retried
                logger.LogWarning(
                    "Failed to send push notification to {Endpoint} with payload {Payload} due to service being unavailable.",
                    subscription.Endpoint,
                    payload
                );
                if (ex.Headers.RetryAfter?.Date != null)
                {
                    retryDate = ex.Headers.RetryAfter.Date;
                }
                else
                {
                    var content = TryParseJsonObject(
                        await ex.HttpResponseMessage.Content.ReadAsStringAsync(cancellation)
                    );
                    var errno = content?["errno"]?.ToString();
                    if (errno == "201")
                    {
                        retryDate = DateTimeOffset.UtcNow.AddMinutes(2);
                    }
                    else if (errno == "202")
                    {
                        retryDate = DateTimeOffset.UtcNow.AddSeconds(5);
                    }
                }
                break;
            default:
                logger.LogError(
                    ex,
                    "Failed to send push notification to {Endpoint} with payload {Payload} due to unexpected error",
                    subscription.Endpoint,
                    payload
                );
                break;
        }

        if (retryDate != null)
        {
            await RetrySendAsync(subscription, payload.Data, retryDate.Value, cancellation);
        }
    }

    private async Task RemoveSubscriptionAsync(
        UserPushSubscription subscription,
        CancellationToken cancellation
    )
    {
        await databaseContext
            .UserPushSubscriptions.Where(x => x.Id == subscription.Id)
            .ExecuteDeleteAsync(cancellation);
    }

    private Task RetrySendAsync(
        UserPushSubscription subscription,
        IPushNotificationData data,
        DateTimeOffset retryDate,
        CancellationToken cancellation
    )
    {
        logger.LogInformation(
            "(TODO) Retrying sending push notification to {Endpoint} with data {Data} at {RetryDate}",
            subscription.Endpoint,
            data,
            retryDate
        );
        // TODO: Retry sending the notification at the specified time
        return Task.CompletedTask;
    }

    private static JsonObject? TryParseJsonObject(string? json)
    {
        if (json == null)
            return null;
        try
        {
            var jsonNode = JsonNode.Parse(
                json,
                null,
                new() { CommentHandling = JsonCommentHandling.Skip }
            );
            return jsonNode is JsonObject obj ? obj : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
