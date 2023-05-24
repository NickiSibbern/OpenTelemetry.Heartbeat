using Atc.Test;
using FluentAssertions;
using Microsoft.Extensions.Options;
using NSubstitute;
using OpenTelemetry.Heartbeat.Monitor.Abstractions;
using OpenTelemetry.Heartbeat.Monitor.Monitors;
using OpenTelemetry.Heartbeat.Monitor.Monitors.Definitions;
using OpenTelemetry.Heartbeat.Monitor.Monitors.Definitions.Models;
using OpenTelemetry.Heartbeat.Monitor.Monitors.Models;
using OpenTelemetry.Heartbeat.Monitor.Settings;

namespace OpenTelemetry.Heartbeat.Monitor.Tests.Monitors;

public class HttpMonitorFactoryTests
{
    [Theory, AutoNSubstituteData]
    public void CanHandle_Should_Return_True_When_MonitorType_Is_Http(
        MonitorDefinition monitorDefinition,
        HttpMonitorFactory sut)
    {
        // Arrange
        monitorDefinition.MonitorType = MonitorDefinitionType.Http;

        // Act
        var result = sut.CanHandle(monitorDefinition);

        // Assert
        result.Should().BeTrue();
    }
    
    [Theory, AutoNSubstituteData]
    public void CanHandle_Should_Return_False_When_MonitorType_Is_Not_Http(
        MonitorDefinition monitorDefinition,
        HttpMonitorFactory sut)
    {
        // Arrange
        monitorDefinition.MonitorType = MonitorDefinitionType.Tcp;

        // Act
        var result = sut.CanHandle(monitorDefinition);

        // Assert
        result.Should().BeFalse();
    }
    
    [Theory, AutoNSubstituteData]
    public void Create_Should_Throw_InvalidOperationException_When_MonitorType_Is_Not_Http(
        MonitorDefinition monitorDefinition,
        HttpMonitorFactory sut)
    {
        // Arrange
        monitorDefinition.MonitorType = MonitorDefinitionType.Tcp;

        // Act
        Action act = () => sut.Create(monitorDefinition);

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }
    
    [Theory, AutoNSubstituteData]
    public void Create_Should_Return_HttpMonitor_With_Named_HttpClient(
        IHttpClientFactory httpClientFactory,
        IDateTimeService dateTimeService,
        Telemetry telemetry,
        MonitorDefinition monitorDefinition,
        MetricSettings metricSettings)
    {
        // Arrange
        var metricSettingsOptions = Options.Create(metricSettings);
        monitorDefinition.MonitorType = MonitorDefinitionType.Http;

        var sut = new HttpMonitorFactory(dateTimeService, httpClientFactory, telemetry, metricSettingsOptions);
        // Act
        var result = sut.Create(monitorDefinition);

        // Assert
        httpClientFactory.Received(1).CreateClient(nameof(HttpMonitor));
        result.Should().BeOfType<HttpMonitor>();
    }
}