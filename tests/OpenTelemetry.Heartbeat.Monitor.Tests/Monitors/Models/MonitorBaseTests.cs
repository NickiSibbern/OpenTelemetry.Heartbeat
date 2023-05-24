using System.Diagnostics.Metrics;
using Atc.Test;
using FluentAssertions;
using FluentResults;
using NSubstitute;
using OpenTelemetry.Heartbeat.Monitor.Abstractions;
using OpenTelemetry.Heartbeat.Monitor.Monitors.Definitions.Models;
using OpenTelemetry.Heartbeat.Monitor.Monitors.Models;
using OpenTelemetry.Heartbeat.Monitor.Settings;

namespace OpenTelemetry.Heartbeat.Monitor.Tests.Monitors.Models;

public class MonitorBaseTests
{
    [Theory, AutoNSubstituteData]
    public async Task ExecuteAsync_Should_Return_Failure_If_Exception_Is_Thrown(
        MonitorDefinition monitorDefinition,
        MetricSettings settings,
        IDateTimeService dateTimeService,
        Meter meter,
        Exception exception,
        CancellationToken cancellationToken)
    {
        // Arrange

        dateTimeService.Now.Returns(DateTimeOffset.Now);
        var sut = new MonitorBaseTestClass(monitorDefinition, dateTimeService, settings, meter, () => throw exception);

        // Act
        var result = await sut.ExecuteAsync(cancellationToken);

        // Assert
        result.IsSuccess.Should().Be(false);
        result.Errors.Select(error => error.Message).Should().Contain(exception.Message);
        sut.GetUpMetric.Should().Be(0);
    }
    
    [Theory, AutoNSubstituteData]
    public async Task ExecuteAsync_Should_Return_Failure_If_It_Should_Not_Run_Yet(
        MonitorDefinition monitorDefinition,
        MetricSettings settings,
        IDateTimeService dateTimeService,
        Meter meter,
        CancellationToken cancellationToken)
    {
        // Arrange
        monitorDefinition.Interval = 50;
       
        dateTimeService.Now.Returns(DateTimeOffset.Now);

        var sut = new MonitorBaseTestClass(
            monitorDefinition,
            dateTimeService,
            settings,
            meter,
            () => { });

        // Act
        sut.SetLastRun(DateTimeOffset.Now.AddMinutes(1));
        var result = await sut.ExecuteAsync(cancellationToken);

        // Assert
        result.IsSuccess.Should().Be(false);
        result.Errors.Select(error => error.Message).Should().Contain($"Monitor for: {monitorDefinition.Name} should not run yet");
    }

    private class MonitorBaseTestClass : MonitorBase
    {
        private readonly Action _action;

        public MonitorBaseTestClass(
            MonitorDefinition monitorDefinition,
            IDateTimeService dateTime,
            MetricSettings settings,
            Meter meter,
            Action action)
            : base(monitorDefinition, dateTime, settings, meter)
        {
            _action = action;
        }

        public void SetLastRun(DateTimeOffset lastRun)
        {
            base.LastRun = lastRun;
        }

        public int GetUpMetric => base.UpMetric;
        
        public override async Task<Result<string>> ExecuteAsync(CancellationToken? cancellationToken = default)
        {
            return await base.ExecuteMonitorAsync(() =>
            {
                _action();
                return Task.FromResult(Result.Ok("Test"));
            });
        }
    }
}