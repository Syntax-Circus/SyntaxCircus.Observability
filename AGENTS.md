# Repository guide for agents

## Purpose and public surface

This repository publishes `SyntaxCircus.Observability`, a .NET 10 package for opt-in OTLP, Serilog OTLP, and Sentry-compatible web error reporting.

- `AddSyntaxCircusObservability()` works with `IHostApplicationBuilder` and configures traces, metrics, and optional custom meters.
- `SyntaxCircusObservabilityRegistration` supplies Serilog enrichment, web Sentry configuration, and safe startup warnings.
- `SyntaxCircusObservabilityOptions` binds the public `OpenTelemetry` and `Sentry` configuration sections.

## Behavior that must be preserved

- OTLP remains off unless `OpenTelemetry:Enabled` is true and `OtlpEndpoint` is an absolute URI.
- Logs, traces, and metrics are independently configurable once OTLP is enabled.
- Sentry remains off without a DSN; default PII is always disabled; events begin at `Error`; trace sampling defaults to zero.
- Consumer callbacks own route-specific Sentry exclusions. Do not add global Blazor or SignalR assumptions.
- Keep runtime dependencies limited to .NET server hosts. MAUI support is not part of this package.

## Validation

```powershell
dotnet restore SyntaxCircus.Observability.slnx
dotnet build SyntaxCircus.Observability.slnx --no-restore --configuration Release
dotnet test --solution SyntaxCircus.Observability.slnx --no-build --configuration Release
```
