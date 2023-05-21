using System.Text;
using Atc.Test;
using AutoFixture.Xunit2;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using OpenTelemetry.Heartbeat.Monitor.Monitors.Models;
using OpenTelemetry.Heartbeat.Monitor.Monitors.Serialization;

namespace OpenTelemetry.Heartbeat.Monitor.Tests.Monitors.Serialization;

public class MonitorDefinitionSerializerTests
{
    [Theory, AutoNSubstituteData]
    public async Task Deserialize_Should_Return_MonitorDefinition_When_Json_Is_Valid(
        [Frozen] ILogger<MonitorDefinitionSerializer> logger,
        MonitorDefinitionSerializer sut,
        CancellationToken cancellationToken)
    {
        // Arrange
        const string json = @"{
            ""Name"": ""Test Monitor"",
            ""Namespace"": ""namespace"",
            ""Interval"": 300,
            ""TimeOut"": 100,
            ""Url"": ""https://localhost"" 
        }";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));

        // Act
        var result = await sut.DeserializeAsync(stream, cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeAssignableTo<MonitorDefinition>();

        result!.Name.Should().BeEquivalentTo("Test Monitor");
        result.Interval.Should().Be(300);
        result.TimeOut.Should().Be(100);
        result.Url.Should().BeEquivalentTo("https://localhost");
        logger.DidNotReceiveWithAnyArgs().LogInformation("Unable to deserialize monitor definition: {Exception}", Arg.Any<Exception>());
    }

    [Theory, AutoNSubstituteData]
    public async Task Deserialize_Should_Return_Null_If_Json_Is_Invalid(
        [Frozen] ILogger<MonitorDefinitionSerializer> logger,
        MonitorDefinitionSerializer sut,
        CancellationToken cancellationToken)
    {
        // Arrange
        const string json = @"{
            ""Name"": ""Test Monitor,      
        }";
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
        logger.ReceivedWithAnyArgs(1).LogInformation("Unable to deserialize monitor definition: {Exception}", Arg.Any<Exception>());
    }
}