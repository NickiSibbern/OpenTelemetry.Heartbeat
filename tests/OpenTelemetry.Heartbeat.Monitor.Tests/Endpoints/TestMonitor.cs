using FluentResults;
using OpenTelemetry.Heartbeat.Monitor.Monitors;

namespace OpenTelemetry.Heartbeat.Monitor.Tests.Endpoints;

public class TestMonitor : IMonitor
{
    public required string Name { get; set; }
    public TimeSpan Interval { get; set; }
    public required DateTimeOffset LastRun { get; set; }
    
    public Task<Result> ExecuteAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}