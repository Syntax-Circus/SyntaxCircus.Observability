using System.Net.Sockets;
using Sentry;
using Shouldly;

using Xunit;

namespace SyntaxCircus.Observability.Tests;

public sealed class BrokerUnreachableNoiseFilterTests
{
    [Fact]
    public void IsBrokerUnreachableOnly_TrueForWrappedMassTransitConnectionException()
    {
        var ex = new AggregateException(new MassTransit.RabbitMqConnectionException("Broker unreachable"));

        BrokerUnreachableNoiseFilter.IsBrokerUnreachableOnly(ex).ShouldBeTrue();
    }

    [Fact]
    public void IsBrokerUnreachableOnly_TrueForNestedAggregateOfBrokerAndSocketErrors()
    {
        var ex = new AggregateException(
            new AggregateException(new RabbitMQ.Client.Exceptions.BrokerUnreachableException("none reachable")),
            new RabbitMQ.Client.Exceptions.ConnectFailureException("failed"),
            new SocketException((int)SocketError.ConnectionRefused));

        BrokerUnreachableNoiseFilter.IsBrokerUnreachableOnly(ex).ShouldBeTrue();
    }

    [Fact]
    public void IsBrokerUnreachableOnly_FalseWhenAnyOtherExceptionIsPresent()
    {
        var ex = new AggregateException(
            new MassTransit.RabbitMqConnectionException("Broker unreachable"),
            new InvalidOperationException("real"));

        BrokerUnreachableNoiseFilter.IsBrokerUnreachableOnly(ex).ShouldBeFalse();
    }

    [Fact]
    public void IsBrokerUnreachableOnly_FalseForBareBrokerExceptionAndNull()
    {
        BrokerUnreachableNoiseFilter.IsBrokerUnreachableOnly(new MassTransit.RabbitMqConnectionException("x")).ShouldBeFalse();
        BrokerUnreachableNoiseFilter.IsBrokerUnreachableOnly(new AggregateException()).ShouldBeFalse();
        BrokerUnreachableNoiseFilter.IsBrokerUnreachableOnly(null).ShouldBeFalse();
    }

    [Fact]
    public void SentryProcessor_DropsOnlyBrokerUnreachableEvents()
    {
        var processor = new BrokerUnreachableNoiseFilter.SentryProcessor();

        processor.Process(new SentryEvent(new AggregateException(new MassTransit.RabbitMqConnectionException("x")))).ShouldBeNull();
        processor.Process(new SentryEvent(new InvalidOperationException("real"))).ShouldNotBeNull();
    }
}
