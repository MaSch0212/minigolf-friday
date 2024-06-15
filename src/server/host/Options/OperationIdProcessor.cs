using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;

namespace MinigolfFriday.Host.Options;

public sealed class OperationIdProcessor : IOperationProcessor
{
    private const string EndpointClassNameSuffix = "Endpoint";

    public bool Process(OperationProcessorContext context)
    {
        var operationId = context.OperationDescription.Operation.OperationId;
        if (operationId.EndsWith(EndpointClassNameSuffix, StringComparison.Ordinal))
            operationId = operationId[0..^EndpointClassNameSuffix.Length];
        context.OperationDescription.Operation.OperationId = operationId;

        return true;
    }
}
