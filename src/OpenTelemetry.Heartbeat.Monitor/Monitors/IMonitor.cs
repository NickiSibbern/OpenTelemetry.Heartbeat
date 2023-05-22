using OpenTelemetry.Heartbeat.Monitor.Monitors.Models;

namespace OpenTelemetry.Heartbeat.Monitor.Monitors;

public interface IMonitor
{
    Task<MonitorResult> ExecuteAsync(CancellationToken? cancellationToken = default);
}