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
    
    protected MonitorBase(MonitorDefinition monitorDefinition, IDateTimeService dateTime, MetricSettings settings, Meter meter)
    {
        _monitorDefinition = monitorDefinition;
        _dateTime = dateTime;
        
        meter.CreateObservableGauge(
            settings.Name,
            () => new Measurement<int>(UpMetric,
                new List<KeyValuePair<string, object?>>
                {
                    new(OpenTelemetryConventions.ServiceName, monitorDefinition.Name),
                    new(OpenTelemetryConventions.Namespace, monitorDefinition.Namespace)
                }),
            description: settings.Description);
    }

    /// <summary>
    /// If 1, the application is up, if 0, the application is down
    /// </summary>
    protected int UpMetric { get; set; } = 0;

    /// <summary>
    /// Time of the last executed run
    /// </summary>
    protected DateTimeOffset LastRun { get; set; } = DateTimeOffset.MinValue;
    
    public abstract Task<Result<string>> ExecuteAsync(CancellationToken? cancellationToken = default);

    /// <summary>
    /// Executes the monitor and returns a result
    /// </summary>
    /// <param name="monitor"></param>
    /// <typeparam name="T"></typeparam>
    protected async Task<Result<T>> ExecuteMonitorAsync<T>(Func<Task<Result<T>>> monitor)
    {
        if (_dateTime.Now < LastRun.AddMilliseconds(_monitorDefinition.Interval))
        {
            return Result.Fail($"Monitor for: {_monitorDefinition.Name} should not run yet");
        }
        
        try
        {
            return await monitor();
        }
        catch (Exception e)
        {
            UpMetric = 0;
            return Result.Fail($"Monitor for: {_monitorDefinition.Name}").WithError(new Error(e.Message).CausedBy(e));
        }
        finally
        {
            LastRun = _dateTime.Now;
        }
    }
}