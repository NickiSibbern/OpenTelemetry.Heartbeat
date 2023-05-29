using Microsoft.Extensions.Options;
using OpenTelemetry.Heartbeat.Monitor.Settings;

namespace OpenTelemetry.Heartbeat.Monitor;

public class Worker : BackgroundService
{
    private readonly IHeartbeatMonitor _heartbeatMonitor;
    private readonly HeartbeatConfig _heartbeatConfig;

    public Worker(IHeartbeatMonitor heartbeatMonitor, IOptions<HeartbeatConfig> heartbeatConfig)
    {
        _heartbeatMonitor = heartbeatMonitor;
        _heartbeatConfig = heartbeatConfig.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _heartbeatMonitor.SetupInitialMonitors(stoppingToken);
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await _heartbeatMonitor.ExecuteAsync(stoppingToken);
                await Task.Delay(_heartbeatConfig.JobInterval, stoppingToken);
            }
        }
        catch (TaskCanceledException)
        {
            // Graceful shutdown without exception
        }
    }
}