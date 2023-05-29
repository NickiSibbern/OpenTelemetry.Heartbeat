using System.ComponentModel.DataAnnotations;

namespace OpenTelemetry.Heartbeat.Monitor.Monitors.Definitions.Models;

public class MonitorDefinition
{
    /// <summary>
    /// Name of the monitor, must be unique.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// namespace for the monitor, <see cref="https://github.com/open-telemetry/opentelemetry-specification/blob/main/specification/resource/semantic_conventions/README.md" />
    /// </summary>
    public required string Namespace { get; set; }

    /// <summary>
    /// Type of monitor. <see cref="MonitorDefinitionType"/>
    /// </summary>
    public required MonitorDefinitionType MonitorType { get; set; }

    /// <summary>
    /// How often the monitor should be executed in milliseconds.
    /// </summary>
    [Range(100, int.MaxValue)]
    public required int Interval { get; set; }

    public required object Monitor { get; set; }
}