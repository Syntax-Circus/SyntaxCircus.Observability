using Microsoft.JSInterop;
using Sentry;
using Shouldly;

using Xunit;

namespace SyntaxCircus.Observability.Tests;

public sealed class DisconnectedCircuitNoiseFilterTests
{
    [Fact]
    public void IsDisconnectedOnly_TrueForWrappedJsDisconnected()
    {
        var ex = new AggregateException(new JSDisconnectedException("gone"));

        DisconnectedCircuitNoiseFilter.IsDisconnectedOnly(ex).ShouldBeTrue();
    }

    [Fact]
    public void IsDisconnectedOnly_FalseWhenAnyOtherExceptionIsPresent()
    {
        var ex = new AggregateException(new JSDisconnectedException("gone"), new InvalidOperationException("real"));

        DisconnectedCircuitNoiseFilter.IsDisconnectedOnly(ex).ShouldBeFalse();
        DisconnectedCircuitNoiseFilter.IsDisconnectedOnly(null).ShouldBeFalse();
    }

    [Fact]
    public void SentryProcessor_DropsOnlyDisconnectedEvents()
    {
        var processor = new DisconnectedCircuitNoiseFilter.SentryProcessor();

        processor.Process(new SentryEvent(new JSDisconnectedException("gone"))).ShouldBeNull();
        processor.Process(new SentryEvent(new InvalidOperationException("real"))).ShouldNotBeNull();
    }
}
