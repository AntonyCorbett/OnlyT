# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What This Project Is

**OnlyT** is a Windows WPF meeting timer application (C#/.NET 8, net8.0-windows). It manages countdown/elapsed timers for structured meetings, with automatic schedule fetching, adaptive timing, a REST API for remote control, and PDF reporting.

## Commands

```powershell
# Build
dotnet build OnlyT.sln

# Run all tests
dotnet test OnlyT.Tests/OnlyT.Tests.csproj

# Run a single test class
dotnet test OnlyT.Tests/OnlyT.Tests.csproj --filter TestAdaptiveTimer

# Run the app
dotnet run --project OnlyT/OnlyT.csproj

# Build installer + portable ZIP (requires Inno Setup 6)
.\CreateDeliverables.cmd

# Publish release build
dotnet publish OnlyT/OnlyT.csproj -p:PublishProfile=FolderProfile -c:Release
```

## Solution Structure

| Project | Target | Purpose |
|---|---|---|
| `OnlyT` | net8.0-windows | Main WPF app |
| `OnlyT.Common` | net8.0 | Shared utilities and interfaces |
| `OnlyT.AnalogueClock` | net8.0-windows | Custom WPF analogue clock control |
| `OnlyT.CountdownTimer` | net8.0-windows | Countdown display window component |
| `OnlyT.Report` | net8.0 | PDF report generation + LiteDB storage |
| `OnlyT.Tests` | net8.0-windows | MSTest + Moq unit tests |
| `OnlyT.WebClient` | net10.0 | ASP.NET Core remote web UI |
| `OnlyTFirewallPorts` | net8.0 | Console utility for firewall config |

## Architecture

### Dependency Injection & Entry Point
`App.xaml.cs` bootstraps `Microsoft.Extensions.DependencyInjection` with MVVM Toolkit's IoC container. All major services register as singletons there.

### Key Services
- `ITalkTimerService` — orchestrates the active timer
- `ITalkScheduleService` — owns the meeting schedule; tracks completed talk times
- `IAdaptiveTimerService` — recalculates remaining durations proportionally as a meeting runs early/late (the core business logic — see `TestAdaptiveTimer` for all scenarios)
- `IOptionsService` — all user settings
- `IBellService` — audio notifications via NAudio
- `IHttpServer` — embedded REST API server; controllers live under `OnlyT/WebServer/Controllers/`
- `ITimerOutputDisplayService` — manages the separate timer output window
- `IMonitorsService` — detects connected displays

### MVVM Pattern
Uses `CommunityToolkit.Mvvm` (8.4.0). ViewModels: `MainViewModel`, `OperatorPageViewModel`, `SettingsPageViewModel`, `TimerOutputWindowViewModel`, `CountdownTimerViewModel`. Inter-ViewModel communication via event messages.

### Timer Modes
Three operating modes selectable at runtime:
- **Automatic** — fetches meeting schedule data from an online source
- **Manual** — user-driven; talks started/stopped explicitly
- **File-based** — schedule loaded from a local file

The adaptive timer (`AdaptiveTimerService`) distributes remaining meeting time proportionally across future talks when the meeting is running ahead or behind. It supports a "TwoWay" mode that both shortens and extends talks, and a one-way mode that only shortens.

### Web API
Custom HTTP server in `HttpServer.cs` (not Kestrel). Controllers handle `/api/timers`, `/api/bell`, `/api/datetime`, `/api/system`, `/api/webhooks`. Includes CORS support, request throttling via `ApiThrottler`, and QR code generation for the web UI URL.

### Output Windows
- **Timer Output Window** — separate WPF window showing elapsed/remaining time and the analogue clock
- **Countdown Window** — pre-meeting countdown with configurable background
- **NDI Output** — network video streaming support (unsafe interop code)

### Reports
`OnlyT.Report` uses LiteDB to persist per-talk timing data, then generates PDF reports via PdfSharp.

## Key Conventions
- Nullable reference types enabled (`<Nullable>Enable</Nullable>`) across all projects
- Unsafe code is allowed in the main `OnlyT` project (NDI interop)
- Application is single-instance enforced by mutex; pass `--nomutex` to bypass
- Accepts `--datetime` launch argument for testing with a custom time
- Serilog for logging (28-day rolling files); Sentry for unhandled exceptions
- Code style via `.editorconfig` and `OnlyT.ruleset` (StyleCop)
