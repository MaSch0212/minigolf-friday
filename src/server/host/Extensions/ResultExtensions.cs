using System.Net;
using FastEndpoints;
using FluentResults;
using Microsoft.AspNetCore.Mvc;

namespace MinigolfFriday.Host.Extensions;

public static class ResultExtensions
{
    private const string StatusCodeMetadataKey = "StatusCode";

    public static Error WithStatusCode(this Error error, HttpStatusCode statusCode) =>
        error.WithMetadata(StatusCodeMetadataKey, statusCode);

    public static Error WithStatusCode(this Error error, int statusCode) =>
        error.WithMetadata(StatusCodeMetadataKey, statusCode);

    public static IActionResult ToActionResult(this Result result)
    {
        return result.IsSuccess ? new OkResult() : GetErrorActionResult(result.Errors);
    }

    public static IActionResult ToActionResult<T>(this Result<T> result)
    {
        return result.IsSuccess
            ? new OkObjectResult(result.Value)
            : GetErrorActionResult(result.Errors);
    }

    public static async Task SendResultAsync<T>(
        this BaseEndpoint endpoint,
        Result<T> result,
        CancellationToken cancellation
    )
    {
        if (result.IsSuccess)
        {
            await endpoint.HttpContext.Response.SendAsync(result.Value, cancellation: cancellation);
        }
        else
        {
            var statusCodeError = result
                .Errors.Where(error =>
                    error.HasMetadataKey(StatusCodeMetadataKey)
                    && error.Metadata[StatusCodeMetadataKey] is int or HttpStatusCode
                )
                .FirstOrDefault();
            if (statusCodeError is not null)
            {
                await endpoint.HttpContext.Response.SendStringAsync(
                    statusCodeError.Message,
                    (int)statusCodeError.Metadata[StatusCodeMetadataKey],
                    cancellation: cancellation
                );
            }
            else
            {
                await endpoint.HttpContext.Response.SendStringAsync(
                    "Internal Server Error",
                    500,
                    cancellation: cancellation
                );
            }
        }
    }

    private static IActionResult GetErrorActionResult(IEnumerable<IError> errors)
    {
        var statusCodeError = errors
            .Where(error =>
                error.HasMetadataKey(StatusCodeMetadataKey)
                && error.Metadata[StatusCodeMetadataKey] is int or HttpStatusCode
            )
            .FirstOrDefault();
        if (statusCodeError is not null)
        {
            return new ObjectResult(statusCodeError.Message)
            {
                StatusCode = (int)statusCodeError.Metadata[StatusCodeMetadataKey]!
            };
        }
        return new StatusCodeResult(500);
    }
}
