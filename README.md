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

## Configuration

The package reads standard `OpenTelemetry` and `Sentry` sections. Telemetry is disabled by default and an enabled but invalid OTLP endpoint produces a safe warning without exporting data. Sentry remains disabled until a DSN is supplied; it sends error-level events only, disables default PII collection, and has a trace sample rate of `0.0` by default.

Keep OTLP headers and Sentry DSNs in deployment secrets. Use the Sentry sampling callback for application-specific noisy routes.

## License

MIT. See [LICENSE.txt](LICENSE.txt).
