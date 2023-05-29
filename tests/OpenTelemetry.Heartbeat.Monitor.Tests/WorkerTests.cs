using Atc.Test;
using FluentAssertions;
using Microsoft.Extensions.Options;
using NSubstitute;
using OpenTelemetry.Heartbeat.Monitor.Settings;

namespace OpenTelemetry.Heartbeat.Monitor.Tests;

public class WorkerTests
{
    [Theory, AutoNSubstituteData]
    public async Task ExecuteAsync_Should_Call_SetupInitialMonitors(
        IHeartbeatMonitor heartbeatMonitor,
        HeartbeatSettings heartbeatSettings,
        CancellationTokenSource cancellationTokenSource)
    {
        // Arrange
        using var sut = new TestWorker(heartbeatMonitor, Options.Create(heartbeatSettings));
        cancellationTokenSource.Cancel(); // cancel immediately
        var cancellationToken = cancellationTokenSource.Token;
        
        // Act
        await sut.ExecuteAsync(cancellationToken);

        // Assert
        await heartbeatMonitor.Received(1).SetupInitialMonitors(cancellationToken);
    }
    
    [Theory, AutoNSubstituteData]
    public async Task ExecuteAsync_Should_Call_StartAsync_On_Heartbeatmonitor(
        IHeartbeatMonitor heartbeatMonitor,
        HeartbeatSettings heartbeatSettings,
        CancellationTokenSource cancellationTokenSource)
    {
        // Arrange
        var cancellationToken = cancellationTokenSource.Token;
        using var sut = new TestWorker(heartbeatMonitor, Options.Create(heartbeatSettings));
        
        heartbeatMonitor.When(x => x.StartAsync(cancellationToken))
            .Do(x => cancellationTokenSource.Cancel()); // cancel immediately
        
        // Act
        await sut.ExecuteAsync(cancellationToken);

        // Assert
        await heartbeatMonitor.Received(1).StartAsync(cancellationToken);
    }
    
    [Theory, AutoNSubstituteData]
    public async Task ExecuteAsync_Should_Not_Throw_Exception_When_Token_Is_Cancelled(
        IHeartbeatMonitor heartbeatMonitor,
        HeartbeatSettings heartbeatSettings,
        CancellationTokenSource cancellationTokenSource)
    {
        // Arrange
        var cancellationToken = cancellationTokenSource.Token;
        var sut = new TestWorker(heartbeatMonitor, Options.Create(heartbeatSettings));
        
        heartbeatMonitor.When(x => x.StartAsync(cancellationToken))
            .Do(x => cancellationTokenSource.Cancel()); // cancel immediately
        
        // Act
        var act = async () =>
        {
            await sut.ExecuteAsync(cancellationToken);
            sut.Dispose();
        };

        // Assert
        await act.Should().NotThrowAsync<TaskCanceledException>();
    }
}