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
}
