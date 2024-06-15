using System.Diagnostics.CodeAnalysis;
using System.Text;
using FastEndpoints;

namespace MinigolfFriday.Host.Common;

public abstract class BaseEndpointError(int statusCode, string message)
{
    public int StatusCode { get; } = statusCode;
    public CompositeFormat Message { get; } = CompositeFormat.Parse(message);
    public abstract string LogTemplate { get; }
}

public class EndpointError(int statusCode, string message) : BaseEndpointError(statusCode, message)
{
    public override string LogTemplate => Message.Format;

    public class Params1(
        int statusCode,
        [StringSyntax("CompositeFormat")] string message,
        string paramName
    ) : BaseEndpointError(statusCode, message)
    {
        public override string LogTemplate => string.Format(null, Message, $"{{{paramName}}}");
    }

    public class Params2(
        int statusCode,
        [StringSyntax("CompositeFormat")] string message,
        string param1Name,
        string param2Name
    ) : BaseEndpointError(statusCode, message)
    {
        public override string LogTemplate =>
            string.Format(null, Message, $"{{{param1Name}}}", $"{{{param2Name}}}");
    }

    public class Params3(
        int statusCode,
        [StringSyntax("CompositeFormat")] string message,
        string param1Name,
        string param2Name,
        string param3Name
    ) : BaseEndpointError(statusCode, message)
    {
        public override string LogTemplate =>
            string.Format(
                null,
                Message,
                $"{{{param1Name}}}",
                $"{{{param2Name}}}",
                $"{{{param3Name}}}"
            );
    }

    public class Params4(
        int statusCode,
        [StringSyntax("CompositeFormat")] string message,
        string param1Name,
        string param2Name,
        string param3Name,
        string param4Name
    ) : BaseEndpointError(statusCode, message)
    {
        public override string LogTemplate =>
            string.Format(
                null,
                Message,
                $"{{{param1Name}}}",
                $"{{{param2Name}}}",
                $"{{{param3Name}}}",
                $"{{{param4Name}}}"
            );
    }
}

public static class EndpointErrorExtensions
{
    public static async Task SendErrorAsync(
        this BaseEndpoint endpoint,
        EndpointError error,
        CancellationToken cancellation
    )
    {
        var text = error.Message.Format;
        await endpoint.HttpContext.Response.SendStringAsync(
            text,
            error.StatusCode,
            cancellation: cancellation
        );
    }

    public static async Task SendErrorAsync<T>(
        this BaseEndpoint endpoint,
        EndpointError.Params1 error,
        T param,
        CancellationToken cancellation
    )
    {
        var text = string.Format(null, error.Message, param);
        await endpoint.HttpContext.Response.SendStringAsync(
            text,
            error.StatusCode,
            cancellation: cancellation
        );
    }

    public static async Task SendErrorAsync<T1, T2>(
        this BaseEndpoint endpoint,
        EndpointError.Params2 error,
        T1 param1,
        T2 param2,
        CancellationToken cancellation
    )
    {
        var text = string.Format(null, error.Message, param1, param2);
        await endpoint.HttpContext.Response.SendStringAsync(
            text,
            error.StatusCode,
            cancellation: cancellation
        );
    }

    public static async Task SendErrorAsync<T1, T2, T3>(
        this BaseEndpoint endpoint,
        EndpointError.Params3 error,
        T1 param1,
        T2 param2,
        T3 param3,
        CancellationToken cancellation
    )
    {
        var text = string.Format(null, error.Message, param1, param2, param3);
        await endpoint.HttpContext.Response.SendStringAsync(
            text,
            error.StatusCode,
            cancellation: cancellation
        );
    }

    public static async Task SendErrorAsync<T1, T2, T3, T4>(
        this BaseEndpoint endpoint,
        EndpointError.Params4 error,
        T1 param1,
        T2 param2,
        T3 param3,
        T4 param4,
        CancellationToken cancellation
    )
    {
        var text = string.Format(null, error.Message, param1, param2, param3, param4);
        await endpoint.HttpContext.Response.SendStringAsync(
            text,
            error.StatusCode,
            cancellation: cancellation
        );
    }

    public static void LogWarning(this ILogger logger, EndpointError error)
    {
        logger.LogWarning(error.LogTemplate);
    }

    public static void LogWarning<T>(this ILogger logger, EndpointError.Params1 error, T param)
    {
        logger.LogWarning(error.LogTemplate, param);
    }

    public static void LogWarning<T1, T2>(
        this ILogger logger,
        EndpointError.Params2 error,
        T1 param1,
        T2 param2
    )
    {
        logger.LogWarning(error.LogTemplate, param1, param2);
    }

    public static void LogWarning<T1, T2, T3>(
        this ILogger logger,
        EndpointError.Params3 error,
        T1 param1,
        T2 param2,
        T3 param3
    )
    {
        logger.LogWarning(error.LogTemplate, param1, param2, param3);
    }

    public static void LogWarning<T1, T2, T3, T4>(
        this ILogger logger,
        EndpointError.Params4 error,
        T1 param1,
        T2 param2,
        T3 param3,
        T4 param4
    )
    {
        logger.LogWarning(error.LogTemplate, param1, param2, param3, param4);
    }

    public static void ProducesError(this BaseEndpoint endpoint, BaseEndpointError error)
    {
        endpoint.Definition.Description(x => x.Produces(error.StatusCode));
        endpoint.Definition.Summary(x =>
            x.AppendResponseDescription(error.StatusCode, error.LogTemplate)
        );
    }

    public static void ProducesErrors(this BaseEndpoint endpoint, params BaseEndpointError[] errors)
    {
        endpoint.Definition.Description(x =>
        {
            foreach (var error in errors)
                x.Produces(error.StatusCode);
        });
        endpoint.Definition.Summary(x =>
        {
            foreach (var error in errors)
                x.AppendResponseDescription(error.StatusCode, error.LogTemplate);
        });
    }
}
