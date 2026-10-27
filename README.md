# SyntaxCircus.Observability

[![Build](https://github.com/Syntax-Circus/SyntaxCircus.Observability/actions/workflows/build.yml/badge.svg)](https://github.com/Syntax-Circus/SyntaxCircus.Observability/actions/workflows/build.yml)
[![NuGet](https://img.shields.io/nuget/v/SyntaxCircus.Observability.svg)](https://www.nuget.org/packages/SyntaxCircus.Observability)

Opt-in OpenTelemetry/OTLP, Serilog OTLP, and Sentry-compatible error-reporting support for .NET 10 server hosts.

> **No support guaranteed.** Published as-is and maintained on a best-effort basis.

## Install

```powershell
dotnet add package SyntaxCircus.Observability
```

## Quick start

```csharp
using SyntaxCircus.Observability;
using SyntaxCircus.AspNetCore.Serilog;

var builder = WebApplication.CreateBuilder(args);
var telemetry = builder.AddSyntaxCircusObservability("my-service", ["MyCompany.MyMeter"]);
builder.AddStandardSerilog(configureEnrichment: telemetry.ConfigureSerilog);

if (telemetry.Options.Sentry.IsEnabled)
{
    builder.WebHost.UseSentry(sentry => telemetry.ConfigureSentry(
        sentry,
        context => context.TransactionContext.Name.Contains("/_blazor", StringComparison.OrdinalIgnoreCase) ? 0d : null));
}

var app = builder.Build();
telemetry.LogStartupWarning(app.Logger);
```

The registration also works with `HostApplicationBuilder` for OTLP traces, metrics, and Serilog logs. Sentry web capture is configured through `WebApplicationBuilder.WebHost` and is intentionally web-only in v1.

## Blazor Server circuit-disposal noise (opt-in)

Components that dispose JS modules in an `async void Dispose()` (for example Blazor-ApexCharts) leave an unobserved `JSDisconnectedException` when a circuit drops. It is harmless but is reported as an error. Opt in to ignore it:

```csharp
DisconnectedCircuitNoiseFilter.AddTo(sentry);            // inside UseSentry(...); drops matching Sentry events
DisconnectedCircuitNoiseFilter.Register(app.Logger);     // after Build(); observes the task, logs at Debug
```

Only exceptions made up entirely of `JSDisconnectedException` are affected.

## Broker-unreachable noise (opt-in)

While a message broker is down, its client (for example RabbitMQ.Client's dual-stack connect) can leave faulted tasks nobody observes. The bus keeps retrying, but Sentry reports each stray task as a `fatal` `AggregateException`. Opt in to ignore them:

```csharp
BrokerUnreachableNoiseFilter.AddTo(sentry);            // inside UseSentry(...); drops matching Sentry events
BrokerUnreachableNoiseFilter.Register(app.Logger);     // after Build(); observes the task, logs at Warning
```

Only an `AggregateException` made up entirely of broker connection failures (`SocketException`, RabbitMQ `BrokerUnreachableException`/`ConnectFailureException`, MassTransit `RabbitMqConnectionException`) is affected; the package takes no broker dependency. Keep a readiness check on the broker to catch a real outage.

## Configuration

The package reads standard `OpenTelemetry` and `Sentry` sections. Telemetry is disabled by default and an enabled but invalid OTLP endpoint produces a safe warning without exporting data. Sentry remains disabled until a DSN is supplied; it sends error-level events only, disables default PII collection, and has a trace sample rate of `0.0` by default.

Keep OTLP headers and Sentry DSNs in deployment secrets. Use the Sentry sampling callback for application-specific noisy routes.

## License

Deferred architecture proposal: [web-neutral observability](docs/enhancements/web-neutral-observability.md). No package split is implemented yet.

MIT. See [LICENSE.txt](LICENSE.txt).
