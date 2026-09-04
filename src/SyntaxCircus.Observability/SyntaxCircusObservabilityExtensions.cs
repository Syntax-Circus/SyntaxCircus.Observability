using Microsoft.Extensions.Hosting;

namespace SyntaxCircus.Observability;

public static class SyntaxCircusObservabilityExtensions
{
    public static SyntaxCircusObservabilityRegistration AddSyntaxCircusObservability(
        this IHostApplicationBuilder builder,
        string defaultServiceName,
        IEnumerable<string>? meterNames = null)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return SyntaxCircusObservabilityRegistration.Register(
            builder.Services,
            builder.Configuration,
            builder.Environment,
            defaultServiceName,
            meterNames);
    }
}
