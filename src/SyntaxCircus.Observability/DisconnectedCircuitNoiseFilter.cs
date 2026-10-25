using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Sentry;
using Sentry.AspNetCore;
using Sentry.Extensibility;

namespace SyntaxCircus.Observability;

/// <summary>
/// Opt-in filter for the unobserved <see cref="JSDisconnectedException"/> that components such as Blazor-ApexCharts
/// leave behind when a Blazor Server circuit drops: they dispose JS modules in an <c>async void Dispose()</c> without
/// catching it, so the finalizer rethrows the faulted task. The JS side is already gone, so it is harmless noise.
/// Nothing is applied unless a consumer calls <see cref="Register"/> or <see cref="AddTo"/>.
/// </summary>
public static class DisconnectedCircuitNoiseFilter
{
    private static readonly Action<ILogger, Exception?> IgnoredMessage = LoggerMessage.Define(
        LogLevel.Debug,
        new EventId(1, "UnobservedJsDisconnected"),
        "Ignored unobserved JSDisconnectedException from a disposed circuit");

    /// <summary>
    /// Marks unobserved task exceptions as observed when every inner exception is a <see cref="JSDisconnectedException"/>.
    /// Anything else is left alone. Call once at startup.
    /// </summary>
    public static void Register(ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        TaskScheduler.UnobservedTaskException += (_, args) =>
        {
            if (!IsDisconnectedOnly(args.Exception))
            {
                return;
            }

            args.SetObserved();
            IgnoredMessage(logger, null);
        };
    }

    /// <summary>
    /// Drops Sentry events caused only by <see cref="JSDisconnectedException"/>. Uses <c>AddEventProcessor</c>, not
    /// <c>SetBeforeSend</c>, because <c>ConfigureSentry</c> owns the single <c>SetBeforeSend</c> delegate.
    /// </summary>
    public static void AddTo(SentryAspNetCoreOptions sentry)
    {
        ArgumentNullException.ThrowIfNull(sentry);
        sentry.AddEventProcessor(new SentryProcessor());
    }

    internal static bool IsDisconnectedOnly(Exception? exception) => exception switch
    {
        null => false,
        JSDisconnectedException => true,
        AggregateException aggregate => aggregate.InnerExceptions.Count > 0
                                        && aggregate.InnerExceptions.All(IsDisconnectedOnly),
        _ => false,
    };

    internal sealed class SentryProcessor : ISentryEventProcessor
    {
        public SentryEvent? Process(SentryEvent @event)
            => IsDisconnectedOnly(@event.Exception) ? null : @event;
    }
}
