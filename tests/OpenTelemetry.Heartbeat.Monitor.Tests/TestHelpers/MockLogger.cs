using Microsoft.Extensions.Logging;

namespace OpenTelemetry.Heartbeat.Monitor.Tests.TestHelpers;

public abstract class MockLogger<T> : ILogger<T>
{
    void ILogger.Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception, string> formatter)
        => Log(logLevel, formatter(state, exception!), exception!);

    protected abstract void Log(LogLevel logLevel, object state, Exception exception = null!);
    public virtual bool IsEnabled(LogLevel logLevel) => true;
    public abstract IDisposable? BeginScope<TState>(TState state) where TState : notnull;
}