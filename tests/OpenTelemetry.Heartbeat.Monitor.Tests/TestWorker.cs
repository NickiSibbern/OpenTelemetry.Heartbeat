using Microsoft.Extensions.Options;
using OpenTelemetry.Heartbeat.Monitor.Settings;

namespace OpenTelemetry.Heartbeat.Monitor.Tests;

public class TestWorker : Worker
{
    public TestWorker(IHeartbeatMonitor heartbeatMonitor, IOptions<HeartbeatConfig> heartbeatConfig) 
        : base(heartbeatMonitor, heartbeatConfig)
    {
    }

    public new Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return base.ExecuteAsync(stoppingToken);
    }
}