using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using Sentry;
using Sentry.AspNetCore;
using Sentry.Extensibility;

namespace SyntaxCircus.Observability;

/// <summary>
/// Opt-in filter for the unobserved task exceptions a message-broker client leaves behind while the broker is
/// unreachable (for example RabbitMQ.Client's dual-stack connect attempts that fault with "Connection refused").
/// The bus keeps retrying on its own, so each stray faulted task is only noise, but Sentry reports every one as a
/// <c>fatal</c> <see cref="AggregateException"/>. Readiness checks are the signal for a real outage.
/// Nothing is applied unless a consumer calls <see cref="Register"/> or <see cref="AddTo"/>.
/// The package takes no RabbitMQ or MassTransit dependency, so exceptions are matched by type name.
/// </summary>
public static class BrokerUnreachableNoiseFilter
{
    private static readonly Action<ILogger, Exception?> IgnoredMessage = LoggerMessage.Define(
        LogLevel.Warning,
        new EventId(2, "UnobservedBrokerUnreachable"),
        "Ignored unobserved broker connection failure; the bus keeps retrying");

    /// <summary>
    /// Marks unobserved task exceptions as observed when every inner exception is a broker connection failure.
    /// Anything else is left alone. Call once at startup.
    /// </summary>
    public static void Register(ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        TaskScheduler.UnobservedTaskException += (_, args) =>
        {
            if (!IsBrokerUnreachableOnly(args.Exception))
            {
                return;
            }

            args.SetObserved();
            IgnoredMessage(logger, null);
        };
    }

    /// <summary>
    /// Drops Sentry events caused only by broker connection failures. Uses <c>AddEventProcessor</c>, not
    /// <c>SetBeforeSend</c>, because <c>ConfigureSentry</c> owns the single <c>SetBeforeSend</c> delegate.
    /// </summary>
    public static void AddTo(SentryAspNetCoreOptions sentry)
    {
        ArgumentNullException.ThrowIfNull(sentry);
        sentry.AddEventProcessor(new SentryProcessor());
    }

    /// <summary>
    /// True only for an <see cref="AggregateException"/> (the unobserved-task wrapper) whose leaves are all broker
    /// connection failures. A bare broker exception, such as one the bus logged at Error, is never matched.
    /// </summary>
    internal static bool IsBrokerUnreachableOnly(Exception? exception)
        => exception is AggregateException aggregate && IsBrokerConnectFailure(aggregate);

    private static bool IsBrokerConnectFailure(Exception? exception) => exception switch
    {
        null => false,
        AggregateException aggregate => aggregate.InnerExceptions.Count > 0
                                        && aggregate.InnerExceptions.All(IsBrokerConnectFailure),
        SocketException => true,
        _ => IsBrokerType(exception.GetType()),
    };

    private static bool IsBrokerType(Type type) => type.FullName switch
    {
        "RabbitMQ.Client.Exceptions.BrokerUnreachableException" => true,
        "RabbitMQ.Client.Exceptions.ConnectFailureException" => true,
        "MassTransit.RabbitMqConnectionException" => true,
        _ => false,
    };

    internal sealed class SentryProcessor : ISentryEventProcessor
    {
        public SentryEvent? Process(SentryEvent @event)
            => IsBrokerUnreachableOnly(@event.Exception) ? null : @event;
    }
}
