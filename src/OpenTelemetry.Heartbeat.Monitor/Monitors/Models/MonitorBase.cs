using System.Diagnostics.Metrics;
using OpenTelemetry.Heartbeat.Monitor.Settings;

namespace OpenTelemetry.Heartbeat.Monitor.Monitors.Models;

public abstract class MonitorBase : IMonitor
{
    protected MonitorBase(string monitorName, string monitorNamespace, MetricSettings settings, Meter meter)
    {
        meter.CreateObservableGauge(
            settings.Name,
            () => new Measurement<int>(UpMetric,
                new List<KeyValuePair<string, object?>>
                {
                    new(OpenTelemetryConventions.ServiceName, monitorName),
                    new(OpenTelemetryConventions.Namespace, monitorNamespace)
                }),
            description: settings.Description);
    }

    /// <summary>
    /// If 1, the application is up, if 0, the application is down
    /// </summary>
    protected int UpMetric { get; set; }
    
    public virtual Task<MonitorResult> ExecuteAsync(CancellationToken? cancellationToken = default) => throw new NotImplementedException();
}