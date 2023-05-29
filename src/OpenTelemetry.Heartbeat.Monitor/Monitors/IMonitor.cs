using FluentResults;

namespace OpenTelemetry.Heartbeat.Monitor.Monitors;

public interface IMonitor
{
    /// <summary>
    /// Unique name of the monitor
    /// </summary>
    string Name { get; set; }
    
    /// <summary>
    /// Interval between each execution
    /// </summary>
    TimeSpan Interval { get; set; }
    
    /// <summary>
    /// Last execution time
    /// </summary>
    DateTimeOffset LastRun { get; set; }
    
    /// <summary>
    /// Execute the monitor and return a result
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<Result> ExecuteAsync(CancellationToken cancellationToken);
}