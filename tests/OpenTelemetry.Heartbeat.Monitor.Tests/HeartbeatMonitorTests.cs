using Atc.Test;
using FluentResults;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using OpenTelemetry.Heartbeat.Monitor.Monitors;
using OpenTelemetry.Heartbeat.Monitor.Monitors.Definitions;
using OpenTelemetry.Heartbeat.Monitor.Monitors.Definitions.Models;
using OpenTelemetry.Heartbeat.Monitor.Settings;
using OpenTelemetry.Heartbeat.Monitor.Tests.TestHelpers;
using Xunit.Categories;

namespace OpenTelemetry.Heartbeat.Monitor.Tests;

[UnitTest]
public class HeartbeatMonitorTests
{
    [Theory, AutoNSubstituteData]
    public async Task SetupInitialMonitors_Should_Setup_Monitors_If_There_Are_Monitor_Definitions(
        IMonitorRepository monitorRepository,
        IMonitorDefinitionRepository monitorDefinitionRepository,
        IMonitorFactory httpMonitorFactory,
        HeartbeatSettings heartbeatSettingsOptions,
        ILogger<HeartbeatMonitor> logger,
        IMonitor monitor,
        List<MonitorDefinition> monitorDefinitions,
        CancellationToken cancellationToken)
    {
        // Arrange
        var heartbeatSettings = Options.Create(heartbeatSettingsOptions);
        var monitorFactories = new List<IMonitorFactory> { httpMonitorFactory };

        monitorDefinitionRepository.GetMonitorDefinitions(cancellationToken).Returns(monitorDefinitions);
        httpMonitorFactory.CanHandle(Arg.Any<MonitorDefinition>()).Returns(true);
        httpMonitorFactory.Create(Arg.Any<MonitorDefinition>()).Returns(monitor);

        var sut = new HeartbeatMonitor(
            monitorRepository,
            monitorDefinitionRepository,
            monitorFactories,
            heartbeatSettings,
            logger);

        // Act
        await sut.SetupInitialMonitors(cancellationToken);

        // Assert
        monitorRepository.Received(monitorDefinitions.Count).AddOrUpdate(Arg.Any<string>(), Arg.Any<IMonitor>());
    }

    [Theory, AutoNSubstituteData]
    public async Task SetupInitialMonitors_Should_Not_Setup_Monitors_If_There_Are_No_Monitor_Definitions_That_Can_Be_Handled(
        IMonitorRepository monitorRepository,
        IMonitorDefinitionRepository monitorDefinitionRepository,
        IMonitorFactory httpMonitorFactory,
        HeartbeatSettings heartbeatSettingsOptions,
        ILogger<HeartbeatMonitor> logger,
        List<MonitorDefinition> monitorDefinitions,
        CancellationToken cancellationToken)
    {
        // Arrange
        var heartbeatSettings = Options.Create(heartbeatSettingsOptions);
        var monitorFactories = new List<IMonitorFactory> { httpMonitorFactory };

        monitorDefinitionRepository.GetMonitorDefinitions(cancellationToken).Returns(monitorDefinitions);
        httpMonitorFactory.CanHandle(Arg.Any<MonitorDefinition>()).Returns(false);

        var sut = new HeartbeatMonitor(
            monitorRepository,
            monitorDefinitionRepository,
            monitorFactories,
            heartbeatSettings,
            logger);

        // Act
        await sut.SetupInitialMonitors(cancellationToken);

        // Assert
        monitorRepository.Received(0).AddOrUpdate(Arg.Any<string>(), Arg.Any<IMonitor>());
    }

    [Theory, AutoNSubstituteData]
    public async Task SetupInitialMonitors_Should_Setup_Monitors_With_FilePath_As_Key(
        IMonitorRepository monitorRepository,
        IMonitorDefinitionRepository monitorDefinitionRepository,
        IMonitorFactory httpMonitorFactory,
        HeartbeatSettings heartbeatSettingsOptions,
        ILogger<HeartbeatMonitor> logger,
        IMonitor monitor,
        MonitorDefinition monitorDefinition,
        CancellationToken cancellationToken)
    {
        // Arrange
        var heartbeatSettings = Options.Create(heartbeatSettingsOptions);
        var monitorFactories = new List<IMonitorFactory> { httpMonitorFactory };

        monitorDefinition.FilePath = "/test/file/path";
        monitorDefinitionRepository.GetMonitorDefinitions(cancellationToken).Returns(new List<MonitorDefinition> { monitorDefinition });

        httpMonitorFactory.CanHandle(Arg.Any<MonitorDefinition>()).Returns(true);
        httpMonitorFactory.Create(Arg.Any<MonitorDefinition>()).Returns(monitor);

        var sut = new HeartbeatMonitor(
            monitorRepository,
            monitorDefinitionRepository,
            monitorFactories,
            heartbeatSettings,
            logger);

        // Act
        await sut.SetupInitialMonitors(cancellationToken);

        // Assert
        monitorRepository.Received(1).AddOrUpdate(Arg.Is<string>(v => v == "/test/file/path"), Arg.Any<IMonitor>());
    }

    [Theory, AutoNSubstituteData]
    public async Task SetupInitialMonitors_Should_Setup_Monitors_With_ServiceName_As_Key_When_FilePath_Is_Null(
        IMonitorRepository monitorRepository,
        IMonitorDefinitionRepository monitorDefinitionRepository,
        IMonitorFactory httpMonitorFactory,
        HeartbeatSettings heartbeatSettingsOptions,
        ILogger<HeartbeatMonitor> logger,
        IMonitor monitor,
        MonitorDefinition monitorDefinition,
        CancellationToken cancellationToken)
    {
        // Arrange
        var heartbeatSettings = Options.Create(heartbeatSettingsOptions);
        var monitorFactories = new List<IMonitorFactory> { httpMonitorFactory };

        monitorDefinition.FilePath = null;
        monitorDefinitionRepository.GetMonitorDefinitions(cancellationToken).Returns(new List<MonitorDefinition> { monitorDefinition });

        httpMonitorFactory.CanHandle(Arg.Any<MonitorDefinition>()).Returns(true);
        httpMonitorFactory.Create(Arg.Any<MonitorDefinition>()).Returns(monitor);

        var sut = new HeartbeatMonitor(
            monitorRepository,
            monitorDefinitionRepository,
            monitorFactories,
            heartbeatSettings,
            logger);

        // Act
        await sut.SetupInitialMonitors(cancellationToken);

        // Assert
        monitorRepository.Received(1).AddOrUpdate(Arg.Is<string>(v => v == monitorDefinition.Name), Arg.Any<IMonitor>());
    }

    [Theory, AutoNSubstituteData]
    public async Task StartAsync_Should_Do_Nothing_If_Cancellation_Is_Requested(
        MockLogger<HeartbeatMonitor> logger,
        IMonitorRepository monitorRepository,
        IMonitorDefinitionRepository monitorDefinitionRepository,
        IEnumerable<IMonitorFactory> monitorFactories,
        List<IMonitor> monitors,
        HeartbeatSettings heartbeatSettings,
        CancellationTokenSource cancellationTokenSource)
    {
        // Arrange
        cancellationTokenSource.Cancel();
        monitorRepository.Monitors.Returns(monitors);

        var sut = new HeartbeatMonitor(
            monitorRepository,
            monitorDefinitionRepository,
            monitorFactories, 
            Options.Create(heartbeatSettings),
            logger);

        // Act
        await sut.StartAsync(cancellationTokenSource.Token);

        // Assert
        logger.DidNotReceiveWithAnyArgs().LogDebug(default);
        logger.DidNotReceiveWithAnyArgs().LogWarning(default);
    }
    
    [Theory, AutoNSubstituteData]
    public async Task StartAsync_Should_Log_Success(
        MockLogger<HeartbeatMonitor> logger,
        IMonitorRepository monitorRepository,
        IMonitorDefinitionRepository monitorDefinitionRepository,
        IEnumerable<IMonitorFactory> monitorFactories,
        IMonitor monitor,
        HeartbeatSettings heartbeatSettings,
        CancellationToken cancellationToken)
    {
        // Arrange
        var result = Result.Ok().WithSuccess("test");
        monitor.ExecuteAsync(cancellationToken).Returns(result);
        var list = new List<IMonitor> { monitor };
        monitorRepository.Monitors.Returns(list);

        var sut = new HeartbeatMonitor(
            monitorRepository,
            monitorDefinitionRepository,
            monitorFactories, 
            Options.Create(heartbeatSettings),
            logger);

        // Act
        await sut.StartAsync(cancellationToken);

        // Assert
        logger.Received().LogDebug("{MonitorMessage}", result.Successes);
    }
    
    [Theory, AutoNSubstituteData]
    public async Task StartAsync_Should_Log_Failed_Requests(
        MockLogger<HeartbeatMonitor> logger,
        IMonitorRepository monitorRepository,
        IMonitorDefinitionRepository monitorDefinitionRepository,
        IEnumerable<IMonitorFactory> monitorFactories,
        IMonitor monitor,
        HeartbeatSettings heartbeatSettings,
        CancellationToken cancellationToken)
    {
        // Arrange
        var result = Result.Fail("test");
        monitor.ExecuteAsync(cancellationToken).Returns(result);
        var list = new List<IMonitor> { monitor };
        monitorRepository.Monitors.Returns(list);

        var sut = new HeartbeatMonitor(
            monitorRepository,
            monitorDefinitionRepository,
            monitorFactories, 
            Options.Create(heartbeatSettings),
            logger);

        // Act
        await sut.StartAsync(cancellationToken);

        // Assert
        logger.Received().LogWarning("{MonitorMessage}", result.Reasons);
    }
}