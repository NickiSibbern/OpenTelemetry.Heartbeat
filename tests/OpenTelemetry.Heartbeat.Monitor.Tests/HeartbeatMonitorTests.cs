using Atc.Test;
using FluentResults;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using OpenTelemetry.Heartbeat.Monitor.Abstractions;
using OpenTelemetry.Heartbeat.Monitor.Monitors;
using OpenTelemetry.Heartbeat.Monitor.Monitors.Definitions;
using OpenTelemetry.Heartbeat.Monitor.Monitors.Definitions.Models;
using OpenTelemetry.Heartbeat.Monitor.Monitors.Models;
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
        HeartbeatConfig heartbeatConfigConfig,
        ILogger<HeartbeatMonitor> logger,
        IMonitor monitor,
        List<MonitorDefinition> monitorDefinitions,
        IDateTimeService dateTimeService,
        CancellationToken cancellationToken)
    {
        // Arrange
        var heartbeatConfig = Options.Create(heartbeatConfigConfig);
        var monitorFactories = new List<IMonitorFactory> { httpMonitorFactory };

        monitorDefinitionRepository.GetMonitorDefinitions(cancellationToken).Returns(monitorDefinitions);
        httpMonitorFactory.CanHandle(Arg.Any<MonitorDefinition>()).Returns(true);
        httpMonitorFactory.Create(Arg.Any<MonitorDefinition>()).Returns(monitor);

        var sut = new HeartbeatMonitor(
            monitorRepository,
            monitorDefinitionRepository,
            monitorFactories,
            heartbeatConfig,
            dateTimeService,
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
        HeartbeatConfig heartbeatConfigConfig,
        ILogger<HeartbeatMonitor> logger,
        List<MonitorDefinition> monitorDefinitions,
        IDateTimeService dateTimeService,
        CancellationToken cancellationToken)
    {
        // Arrange
        var heartbeatConfig = Options.Create(heartbeatConfigConfig);
        var monitorFactories = new List<IMonitorFactory> { httpMonitorFactory };

        monitorDefinitionRepository.GetMonitorDefinitions(cancellationToken).Returns(monitorDefinitions);
        httpMonitorFactory.CanHandle(Arg.Any<MonitorDefinition>()).Returns(false);

        var sut = new HeartbeatMonitor(
            monitorRepository,
            monitorDefinitionRepository,
            monitorFactories,
            heartbeatConfig,
            dateTimeService,
            logger);

        // Act
        await sut.SetupInitialMonitors(cancellationToken);

        // Assert
        monitorRepository.Received(0).AddOrUpdate(Arg.Any<string>(), Arg.Any<IMonitor>());
    }
    
    [Theory, AutoNSubstituteData]
    public async Task SetupInitialMonitors_Should_Setup_Monitors_With_ServiceName_As_Key(
        IMonitorRepository monitorRepository,
        IMonitorDefinitionRepository monitorDefinitionRepository,
        IMonitorFactory httpMonitorFactory,
        HeartbeatConfig heartbeatConfigConfig,
        ILogger<HeartbeatMonitor> logger,
        IMonitor monitor,
        MonitorDefinition monitorDefinition,
        IDateTimeService dateTimeService,
        CancellationToken cancellationToken)
    {
        // Arrange
        var heartbeatConfig = Options.Create(heartbeatConfigConfig);
        var monitorFactories = new List<IMonitorFactory> { httpMonitorFactory };

        monitorDefinitionRepository.GetMonitorDefinitions(cancellationToken).Returns(new List<MonitorDefinition> { monitorDefinition });

        httpMonitorFactory.CanHandle(Arg.Any<MonitorDefinition>()).Returns(true);
        httpMonitorFactory.Create(Arg.Any<MonitorDefinition>()).Returns(monitor);

        var sut = new HeartbeatMonitor(
            monitorRepository,
            monitorDefinitionRepository,
            monitorFactories,
            heartbeatConfig,
            dateTimeService,
            logger);

        // Act
        await sut.SetupInitialMonitors(cancellationToken);

        // Assert
        monitorRepository.Received(1).AddOrUpdate(Arg.Is<string>(v => v == monitorDefinition.Name), Arg.Any<IMonitor>());
    }

    [Theory, AutoNSubstituteData]
    public async Task ExecuteAsync_Should_Do_Nothing_If_Cancellation_Is_Requested(
        MockLogger<HeartbeatMonitor> logger,
        IMonitorRepository monitorRepository,
        IMonitorDefinitionRepository monitorDefinitionRepository,
        IEnumerable<IMonitorFactory> monitorFactories,
        List<IMonitor> monitors,
        HeartbeatConfig heartbeatConfig,
        IDateTimeService dateTimeService,
        CancellationTokenSource cancellationTokenSource)
    {
        // Arrange
        cancellationTokenSource.Cancel();
        monitorRepository.Monitors.Returns(monitors);

        var sut = new HeartbeatMonitor(
            monitorRepository,
            monitorDefinitionRepository,
            monitorFactories, 
            Options.Create(heartbeatConfig),
            dateTimeService,
            logger);

        // Act
        await sut.ExecuteAsync(cancellationTokenSource.Token);

        // Assert
        logger.DidNotReceiveWithAnyArgs().LogDebug(default);
        logger.DidNotReceiveWithAnyArgs().LogWarning(default);
    }
    
    [Theory, AutoNSubstituteData]
    public async Task ExecuteAsync_Should_Log_Success(
        MockLogger<HeartbeatMonitor> logger,
        IMonitorRepository monitorRepository,
        IMonitorDefinitionRepository monitorDefinitionRepository,
        IEnumerable<IMonitorFactory> monitorFactories,
        IMonitor monitor,
        HeartbeatConfig heartbeatConfig,
        IDateTimeService dateTimeService,
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
            Options.Create(heartbeatConfig),
            dateTimeService,
            logger);

        // Act
        await sut.ExecuteAsync(cancellationToken);

        // Assert
        logger.ReceivedWithAnyArgs(1).LogDebug(default);
    }
    
    [Theory, AutoNSubstituteData]
    public async Task ExecuteAsync_Should_Log_Failed_Requests(
        MockLogger<HeartbeatMonitor> logger,
        IMonitorRepository monitorRepository,
        IMonitorDefinitionRepository monitorDefinitionRepository,
        IEnumerable<IMonitorFactory> monitorFactories,
        IMonitor monitor,
        HeartbeatConfig heartbeatConfig,
        IDateTimeService dateTimeService,
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
            Options.Create(heartbeatConfig),
            dateTimeService,
            logger);

        // Act
        await sut.ExecuteAsync(cancellationToken);

        // Assert
        logger.Received().LogWarning("Monitor: {MonitorName} executed with errors: {Errors}", monitor.Name, result.Reasons);
    }
    
    [Theory, AutoNSubstituteData]
    public async Task ExecuteAsync_Should_Not_Log_Warnings(
        MockLogger<HeartbeatMonitor> logger,
        IMonitorRepository monitorRepository,
        IMonitorDefinitionRepository monitorDefinitionRepository,
        IEnumerable<IMonitorFactory> monitorFactories,
        IMonitor monitor,
        HeartbeatConfig heartbeatConfig,
        IDateTimeService dateTimeService,
        CancellationToken cancellationToken)
    {
        // Arrange
        var result = Result.Fail(new WarningResult("warning"));
        monitor.ExecuteAsync(cancellationToken).Returns(result);
        var list = new List<IMonitor> { monitor };
        monitorRepository.Monitors.Returns(list);

        var sut = new HeartbeatMonitor(
            monitorRepository,
            monitorDefinitionRepository,
            monitorFactories, 
            Options.Create(heartbeatConfig),
            dateTimeService,
            logger);

        // Act
        await sut.ExecuteAsync(cancellationToken);

        // Assert
        logger.DidNotReceiveWithAnyArgs().LogWarning(default);
    }
}