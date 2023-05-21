namespace OpenTelemetry.Heartbeat.Monitor.Monitors.Models;

public record MonitorDefinition(string Name, string Namespace, int Interval, int TimeOut, string Url);