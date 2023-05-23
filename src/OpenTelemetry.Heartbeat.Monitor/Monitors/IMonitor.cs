using FluentResults;

namespace OpenTelemetry.Heartbeat.Monitor.Monitors;

public interface IMonitor
{
    /// <summary>
    /// Execute the monitor and return a result
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<Result<string>> ExecuteAsync(CancellationToken? cancellationToken = default);
}