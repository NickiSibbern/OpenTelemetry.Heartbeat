using Microsoft.Extensions.Options;
using OpenTelemetry.Heartbeat.Monitor.Abstractions;
using OpenTelemetry.Heartbeat.Monitor.Monitors.Definitions;
using OpenTelemetry.Heartbeat.Monitor.Monitors.Definitions.Models;
using OpenTelemetry.Heartbeat.Monitor.Monitors.Models;
using OpenTelemetry.Heartbeat.Monitor.Settings;

namespace OpenTelemetry.Heartbeat.Monitor.Monitors;

public class HttpMonitorFactory : IMonitorFactory
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly Telemetry _telemetry;
    private readonly MetricSettings _metricSettings;
    private readonly IDateTimeService _dateTimeService;

    public HttpMonitorFactory(IDateTimeService dateTimeService, IHttpClientFactory httpClientFactory, Telemetry telemetry, IOptions<MetricSettings> metricSettings)
    {
        _dateTimeService = dateTimeService;
        _httpClientFactory = httpClientFactory;
        _telemetry = telemetry;
        _metricSettings = metricSettings.Value;
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
        return new HttpMonitor(monitorDefinition, httpClient, _dateTimeService, _metricSettings, _telemetry.Meter);
    }
}