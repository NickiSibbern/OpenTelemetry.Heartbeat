using FluentResults;

namespace OpenTelemetry.Heartbeat.Monitor.Monitors.Models;

public sealed class WarningResult : Error
{
    public WarningResult(string message) : base(message)
    {
    }
}