using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Reflection;

namespace OpenTelemetry.Heartbeat.Monitor.Telemetry;

public class TelemetrySource
{
    public TelemetrySource()
    {
        var name = Assembly.GetExecutingAssembly().GetName().Name ?? "HeartbeatMonitor";
        ActivitySource = new ActivitySource(name);
        Meter = new Meter(name);
    }
    
    public ActivitySource ActivitySource { get; }
    
    public Meter Meter { get; }
}