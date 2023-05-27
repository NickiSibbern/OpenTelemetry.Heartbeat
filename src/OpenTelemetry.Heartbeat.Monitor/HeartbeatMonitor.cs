using Microsoft.Extensions.Options;
using OpenTelemetry.Heartbeat.Monitor.Monitors;
using OpenTelemetry.Heartbeat.Monitor.Monitors.Definitions;
using OpenTelemetry.Heartbeat.Monitor.Settings;

namespace OpenTelemetry.Heartbeat.Monitor;

public interface IHeartbeatMonitor
{
    Task SetupInitialMonitors(CancellationToken cancellationToken);
    Task StartAsync(CancellationToken cancellationToken);
}

public class HeartbeatMonitor : IHeartbeatMonitor
{
    private readonly IMonitorRepository _monitorRepository;
    private readonly IMonitorDefinitionRepository _monitorDefinitionRepository;
    private readonly IEnumerable<IMonitorFactory> _monitorFactories;
    private readonly HeartbeatSettings _heartbeatSettings;
    private readonly ILogger<HeartbeatMonitor> _logger;

    public HeartbeatMonitor(
        IMonitorRepository monitorRepository,
        IMonitorDefinitionRepository monitorDefinitionRepository,
        IEnumerable<IMonitorFactory> monitorFactories,
        IOptions<HeartbeatSettings> heartbeatSettings,
        ILogger<HeartbeatMonitor> logger)
    {
        _monitorRepository = monitorRepository;
        _monitorDefinitionRepository = monitorDefinitionRepository;
        _monitorFactories = monitorFactories;
        _heartbeatSettings = heartbeatSettings.Value;
        _logger = logger;
    }

    public async Task SetupInitialMonitors(CancellationToken cancellationToken)
    {
        var monitorDefinitions = await _monitorDefinitionRepository.GetMonitorDefinitions(cancellationToken);
        foreach (var definition in monitorDefinitions)
        {
            foreach (var monitorFactory in _monitorFactories)
            {
                if (!monitorFactory.CanHandle(definition))
                {
                    continue;
                }

                var key = definition.FilePath ?? definition.Name;
                var monitor = monitorFactory.Create(definition);
                _monitorRepository.AddOrUpdate(key, monitor);
            }
        }
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        foreach (var batch in _monitorRepository.Monitors.Chunk(_heartbeatSettings.JobBatchSize).ToArray())
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            await Task.WhenAll(batch.Select(async monitor =>
            {
                var result = await monitor.ExecuteAsync(cancellationToken);
                if (result.IsSuccess)
                {
                    _logger.LogDebug("{MonitorMessage}", result.Successes);
                }
                else
                {
                    _logger.LogWarning("{MonitorMessage}", result.Reasons);
                }
            }));
        }
    }
}