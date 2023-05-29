using Atc.Test;
using FluentAssertions;
using OpenTelemetry.Heartbeat.Monitor.Monitors;
using Xunit.Categories;

namespace OpenTelemetry.Heartbeat.Monitor.Tests.Monitors;

[UnitTest]
public class MonitorRepositoryTests
{
    [Theory, AutoNSubstituteData]
    public void AddOrUpdate_Should_Not_Add_Or_Update_If_Monitor_Is_Null(
        MonitorRepository sut,
        string filePath)
    {
        // Arrange & Act
        var result = sut.AddOrUpdate(filePath, null);

        // Assert
        result.Should().BeFalse();
        sut.Monitors.Should().BeEmpty();
    }
    
    [Theory, AutoNSubstituteData]
    public void AddOrUpdate_Should_Add_Monitor_If_Key_Does_Not_Exists(
        MonitorRepository sut,
        IMonitor preExistingMonitor,
        IMonitor monitor)
    {
        // Arrange
        sut.AddOrUpdate("foo", preExistingMonitor);

        // Act
        var result = sut.AddOrUpdate("/foobar", monitor);

        // Assert
        result.Should().BeTrue();
        sut.Monitors.Should().Contain(monitor);
    }

    [Theory, AutoNSubstituteData]
    public void AddOrUpdate_Should_Update_Monitor_If_Key_Already_Exists(
        MonitorRepository sut,
        IMonitor originalMonitor,
        IMonitor newMonitor)
    {
        // Arrange
        sut.AddOrUpdate("foo", originalMonitor);

        // Act
        var result = sut.AddOrUpdate("foo", newMonitor);

        // Assert
        result.Should().BeTrue();
        sut.Monitors.Should().OnlyContain(probe => probe == newMonitor);
    }

    [Theory, AutoNSubstituteData]
    public void Remove_Should_Remove_Monitor_If_Key_Exists(
        MonitorRepository sut,
        IMonitor monitor)
    {
        // Arrange
        sut.AddOrUpdate("foo", monitor);

        // Act
        sut.Remove("foo");

        // Assert
        sut.Monitors.Should().BeEmpty();
    }

    [Theory, AutoNSubstituteData]
    public void Contains_Should_Check_If_Key_Exists(
        MonitorRepository sut,
        IMonitor monitor)
    {
        // Arrange
        sut.AddOrUpdate("foo", monitor);

        // Act
        var result = sut.Contains("foo");

        // Assert
        result.Should().BeTrue();
    }
}