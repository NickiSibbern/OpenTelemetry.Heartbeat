using System.Diagnostics.CodeAnalysis;

namespace OpenTelemetry.Heartbeat.Monitor.Abstractions;

public interface IDateTimeService
{
    DateTimeOffset Now { get; }
}

[ExcludeFromCodeCoverage(Justification = "Enables testing of datetime-dependent code")]
public class DateTimeService : IDateTimeService
{
    public DateTimeOffset Now => DateTimeOffset.Now;
}