using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Reflection;

namespace OpenTelemetry.Heartbeat.Monitor;

public class Telemetry
{
    public Telemetry()
    {
        var name = Assembly.GetExecutingAssembly().GetName().Name ?? "HeartbeatMonitor";
        ActivitySource = new ActivitySource(name);
        Meter = new Meter(name);
    }
    
    public ActivitySource ActivitySource { get; }
    
    public Meter Meter { get; }
}