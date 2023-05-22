using System.Diagnostics.Metrics;
using OpenTelemetry.Heartbeat.Monitor.Monitors.Definitions.Models;
using OpenTelemetry.Heartbeat.Monitor.Settings;

namespace OpenTelemetry.Heartbeat.Monitor.Monitors.Models;

public sealed class HttpMonitor : MonitorBase
{
    private readonly MonitorDefinition _monitorDefinition;
    private readonly HttpClient _httpClient;

    public HttpMonitor(MonitorDefinition monitorDefinition, HttpClient httpClient, MetricSettings settings, Meter meter)
        : base(monitorDefinition.Name, monitorDefinition.Namespace, settings, meter)
    {
        ArgumentNullException.ThrowIfNull(monitorDefinition.Http);

        _monitorDefinition = monitorDefinition;
        _httpClient = httpClient;
    }

    public override async Task<MonitorResult> ExecuteAsync(CancellationToken? cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(cancellationToken);

        var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken.Value);
        linkedTokenSource.CancelAfter(_monitorDefinition.Http!.TimeOut);

        string? errorMessage = null;

        try
        {
            var response = await _httpClient.GetAsync(_monitorDefinition.Http!.Url, linkedTokenSource.Token);
            if ((int)response.StatusCode == _monitorDefinition.Http.ResponseCode)
            {
                base.UpMetric = 1;
            }
            else
            {
                base.UpMetric = 0;
                errorMessage = string.IsNullOrWhiteSpace(response.ReasonPhrase)
                    ? "Unknown error occurred - reason provided by the server was null"
                    : response.ReasonPhrase;
            }
        }
        catch (Exception e)
        {
            base.UpMetric = 0;
            errorMessage = e.Message;
        }

        return new MonitorResult(MonitorName: _monitorDefinition.Name, IsSuccess: UpMetric > 0, ErrorMessage: errorMessage);
    }
}