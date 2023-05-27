using Microsoft.Extensions.Options;
using OpenTelemetry.Heartbeat.Monitor.Abstractions;
using OpenTelemetry.Heartbeat.Monitor.Monitors.Definitions;
using OpenTelemetry.Heartbeat.Monitor.Monitors.Definitions.Models;
using OpenTelemetry.Heartbeat.Monitor.Monitors.Models;
using OpenTelemetry.Heartbeat.Monitor.Settings;
using OpenTelemetry.Heartbeat.Monitor.Telemetry;

namespace OpenTelemetry.Heartbeat.Monitor.Monitors;

public class HttpMonitorFactory : IMonitorFactory
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly TelemetrySource _telemetrySource;
    private readonly HeartbeatSettings _heartbeatSettings;
    private readonly IDateTimeService _dateTimeService;

    public HttpMonitorFactory(IDateTimeService dateTimeService, IHttpClientFactory httpClientFactory, TelemetrySource telemetrySource, IOptions<HeartbeatSettings> metricSettings)
    {
        _dateTimeService = dateTimeService;
        _httpClientFactory = httpClientFactory;
        _telemetrySource = telemetrySource;
        _heartbeatSettings = metricSettings.Value;
    }

    public bool CanHandle(MonitorDefinition monitorDefinition)
        => monitorDefinition.MonitorType == MonitorDefinitionType.Http;

    public IMonitor Create(MonitorDefinition monitorDefinition)
    {
        if (monitorDefinition.MonitorType is not MonitorDefinitionType.Http)
        {
            throw new InvalidOperationException("Cannot create HttpMonitor from non-Http MonitorDefinition");
        }
        
        var httpClient = _httpClientFactory.CreateClient(nameof(HttpMonitor));
        return new HttpMonitor(monitorDefinition, httpClient, _dateTimeService, _heartbeatSettings, _telemetrySource.Meter);
    }
}