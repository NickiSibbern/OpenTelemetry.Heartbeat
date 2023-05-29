using System.Text;
using Atc.Test;
using AutoFixture.Xunit2;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using OpenTelemetry.Heartbeat.Monitor.Monitors.Definitions;
using OpenTelemetry.Heartbeat.Monitor.Monitors.Definitions.Models;
using Xunit.Categories;

namespace OpenTelemetry.Heartbeat.Monitor.Tests.Monitors.Definitions;

[UnitTest]
public class MonitorDefinitionSerializerTests
{
    [Theory, AutoNSubstituteData]
    public async Task Deserialize_Should_Return_MonitorDefinition_When_Json_Is_Valid_HttpMonitor(
        MonitorDefinitionSerializer sut,
        CancellationToken cancellationToken)
    {
        // Arrange
        const string json = @"{
            ""name"": ""Test Monitor"",
            ""namespace"": ""namespace"",
            ""interval"": 300,
            ""MonitorType"": ""http"",
            ""monitor"": {
                ""timeOut"": 100,
                ""url"": ""https://localhost"",
                ""responseCode"": 200
            }
        }";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));

        // Act
        var result = await sut.DeserializeAsync(stream, cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeAssignableTo<MonitorDefinition>();

        result!.Name.Should().BeEquivalentTo("Test Monitor");
        result.MonitorType.Should().Be(MonitorDefinitionType.Http);
        result.Monitor.Should().NotBeNull();
        result.Interval.Should().Be(300);

        var httpMonitorDefinition = result.Monitor as HttpMonitorDefinition;

        httpMonitorDefinition?.TimeOut.Should().Be(100);
        httpMonitorDefinition?.ResponseCode.Should().Be(200);
        httpMonitorDefinition?.Url.Should().BeEquivalentTo(new Uri("https://localhost"));
    }


    [Theory, AutoNSubstituteData]
    public async Task Deserialize_Should_Return_Null_If_Json_Is_Invalid(
        MonitorDefinitionSerializer sut,
        CancellationToken cancellationToken)
    {
        // Arrange
        const string json = @"{""Name"": ""Test Monitor,}";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));

        // Act
        var result = await sut.DeserializeAsync(stream, cancellationToken);

        // Assert
        result.Should().BeNull();
    }

    [Theory]
    [InlineAutoNSubstituteData(@"{
            ""name"": ""Test Monitor"",
            ""namespace"": ""namespace"",
            ""interval"": 300,
            ""MonitorType"": ""Unknown"",
            ""monitor"": {
                ""timeOut"": 100,
                ""url"": ""https://localhost"",
                ""responseCode"": 200
            }
        }")]
    [InlineAutoNSubstituteData(@"{
            ""name"": ""Test Monitor"",
            ""namespace"": ""namespace"",
            ""interval"": ""300"",
            ""MonitorType"": ""Http"",       
        }")]
    public async Task Deserialize_Should_Return_Null_If_Json_Cannot_Be_Deserialized(
        string json,
        MonitorDefinitionSerializer sut,
        CancellationToken cancellationToken)
    {
        // Arrange
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));

        // Act
        var result = await sut.DeserializeAsync(stream, cancellationToken);

        // Assert
        result.Should().BeNull();
    }

    [Theory, AutoNSubstituteData]
    public async Task DeserializeAsync_Should_Return_Null_When_Json_Is_Null(
        [Frozen] ILogger<MonitorDefinitionSerializer> logger,
        MonitorDefinitionSerializer sut)
    {
        // Arrange && Act
        var result = await sut.DeserializeAsync(null!, CancellationToken.None);

        // Assert
        result.Should().BeNull();
        logger.ReceivedWithAnyArgs(1).LogError(default, Arg.Any<Exception>());
    }
}