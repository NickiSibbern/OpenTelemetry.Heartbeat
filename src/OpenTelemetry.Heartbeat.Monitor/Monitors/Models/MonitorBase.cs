using System.Diagnostics.Metrics;
using FluentResults;
using OpenTelemetry.Heartbeat.Monitor.Abstractions;
using OpenTelemetry.Heartbeat.Monitor.Monitors.Definitions.Models;
using OpenTelemetry.Heartbeat.Monitor.Settings;

namespace OpenTelemetry.Heartbeat.Monitor.Monitors.Models;

public abstract class MonitorBase : IMonitor
{
    private readonly MonitorDefinition _monitorDefinition;
    private readonly IDateTimeService _dateTime;

    protected MonitorBase(MonitorDefinition monitorDefinition, IDateTimeService dateTime, HeartbeatConfig config,
        Meter meter)
    {
        _monitorDefinition = monitorDefinition;
        _dateTime = dateTime;
        Name = monitorDefinition.Name;
        Interval = TimeSpan.FromMilliseconds(monitorDefinition.Interval);
        meter.CreateObservableGauge(
            config.MetricName,
            () => new Measurement<int>(UpMetric,
                new List<KeyValuePair<string, object?>>
                {
                    new(OpenTelemetryConventions.ServiceName, monitorDefinition.Name),
                    new(OpenTelemetryConventions.Namespace, monitorDefinition.Namespace)
                }),
            description: config.MetricDescription);
    }

    /// <summary>
    /// If 1, the application is up, if 0, the application is down
    /// </summary>
    protected int UpMetric { get; set; }

    public TimeSpan Interval { get; set; }

    /// <summary>
    /// Time of the last executed run
    /// </summary>
    public DateTimeOffset LastRun { get; set; } = DateTimeOffset.MinValue;

    public string Name { get; set; }

    public abstract Task<Result> ExecuteAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Executes the monitor and returns a result
    /// </summary>
    /// <param name="monitor"></param>
    protected async Task<Result> ExecuteMonitorAsync(Func<Task<Result>> monitor)
    {
        if (_dateTime.Now < LastRun.AddMilliseconds(Interval.TotalMilliseconds))
        {
            return Result.Fail(new WarningResult($"Monitor for: {_monitorDefinition.Name} should not run yet"));
        }

        try
        {
            return await monitor();
        }
        catch (Exception e)
        {
            UpMetric = 0;
            return Result.Fail($"Monitor for: {_monitorDefinition.Name} failed with errors")
                .WithError(new Error(e.Message));
        }
        finally
        {
            LastRun = _dateTime.Now;
        }
    }
}