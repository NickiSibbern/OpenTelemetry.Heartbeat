using OpenTelemetry.Heartbeat.Monitor.Monitors.Definitions.Models;

namespace OpenTelemetry.Heartbeat.Monitor.Monitors;

public interface IMonitorFactory
{
    bool CanHandle(MonitorDefinition? monitorDefinition);
    
    IMonitor Create(MonitorDefinition monitorDefinition);
}