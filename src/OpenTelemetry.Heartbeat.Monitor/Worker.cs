using Microsoft.Extensions.Options;
using OpenTelemetry.Heartbeat.Monitor.Settings;

namespace OpenTelemetry.Heartbeat.Monitor;

public class Worker : BackgroundService
{
    private readonly IHeartbeatMonitor _heartbeatMonitor;
    private readonly HeartbeatSettings _heartbeatSettings;

    public Worker(IHeartbeatMonitor heartbeatMonitor, IOptions<HeartbeatSettings> heartbeatSettings)
    {
        _heartbeatMonitor = heartbeatMonitor;
        _heartbeatSettings = heartbeatSettings.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _heartbeatMonitor.SetupInitialMonitors(stoppingToken);
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await _heartbeatMonitor.StartAsync(stoppingToken);
                await Task.Delay(_heartbeatSettings.JobInterval, stoppingToken);
            }
        }
        catch (TaskCanceledException)
        {
            // Graceful shutdown without exception
        }
    }
}