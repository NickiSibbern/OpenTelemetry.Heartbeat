namespace OpenTelemetry.Heartbeat.Monitor.Monitors.Models;

public record MonitorResult(string MonitorName, bool IsSuccess, string? ErrorMessage = null);
