using System.Net;
using FluentResults;
using Microsoft.AspNetCore.Mvc;

namespace MinigolfFriday;

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

    private static IActionResult GetErrorActionResult(IEnumerable<IError> errors)
    {
        var statusCodeError = errors
            .Where(
                error =>
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
