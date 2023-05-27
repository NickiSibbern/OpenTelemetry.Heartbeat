using Microsoft.Extensions.Options;
using OpenTelemetry.Heartbeat.Monitor.Settings;

namespace OpenTelemetry.Heartbeat.Monitor;

public class Worker : BackgroundService
{
    private readonly IHeartbeatMonitor _heartbeatMonitor;
    private readonly HeartbeatSettings _heartbeatSettings;
    private readonly ILogger<Worker> _logger;

    public Worker(
        IHeartbeatMonitor heartbeatMonitor,
        IOptions<HeartbeatSettings> heartbeatSettings,
        ILogger<Worker> logger )
    {
        _heartbeatMonitor = heartbeatMonitor;
        _heartbeatSettings = heartbeatSettings.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _heartbeatMonitor.SetupInitialMonitors(stoppingToken);
        
        while (!stoppingToken.IsCancellationRequested)
        {
            await _heartbeatMonitor.StartAsync(stoppingToken);
            await Task.Delay(_heartbeatSettings.JobInterval, stoppingToken);
        }
    }
}