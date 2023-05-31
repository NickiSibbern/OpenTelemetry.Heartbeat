using System.Text.Json.Serialization;

namespace OpenTelemetry.Heartbeat.Monitor.Monitors.Definitions.Models;

public interface IMonitorDefinition
{
    /// <summary>
    /// Type of monitor. <see cref="MonitorDefinitionType"/>
    /// </summary>
    public MonitorDefinitionType Type { get; set; }
}