namespace OpenTelemetry.Heartbeat.Monitor;

public class Worker : BackgroundService
{
    private readonly IHeartbeatMonitor _heartbeatMonitor;
    private readonly ILogger<Worker> _logger;

    public Worker(
        IHeartbeatMonitor heartbeatMonitor,
        ILogger<Worker> logger)
    {
        _heartbeatMonitor = heartbeatMonitor;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _heartbeatMonitor.SetupInitialMonitors(stoppingToken);
        
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            await _heartbeatMonitor.StartAsync(stoppingToken);
            
            await Task.Delay(1000, stoppingToken);
        }
    }
}