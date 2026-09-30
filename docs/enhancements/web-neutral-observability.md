# Deferred: web-neutral observability

Status: proposal only; outside the GAT PKI implementation milestone (2026-09-30).

## Motivation

AddSyntaxCircusObservability already accepts IHostApplicationBuilder, but the package references Microsoft.AspNetCore.App, Sentry.AspNetCore, ASP.NET instrumentation and SyntaxCircus.AspNetCore.Common. Short-lived CLI and headless worker consumers could benefit from OTLP bootstrap without carrying the web integration stack.

## Future design to evaluate

- Separate host-neutral OTLP registration/options/metrics/traces and safe diagnostics from ASP.NET instrumentation and web Sentry integration.
- Keep the existing package as a compatible facade or web adapter. Review registration handles/type identities and publish a migration guide; do not silently remove transitive capabilities from existing web consumers.
- Add explicit lifecycle support for finite commands: bounded telemetry flushing/disposal, startup failure, cancellation and exporters being unavailable must not hang process exit.
- Preserve opt-in export, disabled-by-default Sentry, no default PII, deployment-owned sampling and secrets. Host-neutral support is not authorization to export data from isolated machines.
- Test plain CLI/worker self-contained publishing on Windows/Linux without ASP.NET runtime installation, alongside existing web consumers and multi-host tests.

GAT PKI currently uses local sanitized logs, persisted monitoring state and durable notifications. Remote telemetry remains deferred; the offline root must never export unattended telemetry. This document does not implement modularization or enable a remote exporter.
