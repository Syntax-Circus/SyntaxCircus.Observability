using Microsoft.Extensions.Configuration;
using Shouldly;
using Xunit;

namespace SyntaxCircus.Observability.Tests;

public sealed class ObservabilityOptionsTests
{
    [Fact]
    public void FromConfiguration_DefaultsToDisabledWithStrictSentryPrivacy()
    {
        var options = SyntaxCircusObservabilityOptions.FromConfiguration(new ConfigurationBuilder().Build());

        options.OpenTelemetry.IsEnabled.ShouldBeFalse();
        options.OpenTelemetry.ExportLogs.ShouldBeTrue();
        options.OpenTelemetry.ExportTraces.ShouldBeTrue();
        options.OpenTelemetry.ExportMetrics.ShouldBeTrue();
        options.Sentry.IsEnabled.ShouldBeFalse();
        options.Sentry.SendDefaultPii.ShouldBeFalse();
        options.Sentry.TracesSampleRate.ShouldBe(0d);
    }

    [Fact]
    public void FromConfiguration_DisablesInvalidEnabledOtlpEndpointWithoutExposingItInWarning()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["OpenTelemetry:Enabled"] = "true",
                ["OpenTelemetry:OtlpEndpoint"] = "not a uri"
            })
            .Build();

        var options = SyntaxCircusObservabilityOptions.FromConfiguration(configuration);

        options.OpenTelemetry.IsEnabled.ShouldBeFalse();
        var warning = options.OpenTelemetry.StartupWarning;
        warning.ShouldNotBeNull();
        warning.ShouldContain("disabled");
        warning.ShouldNotContain("not a uri");
    }
}
