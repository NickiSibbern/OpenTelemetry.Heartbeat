using System.IO.Abstractions.TestingHelpers;
using Atc.Test;
using FluentAssertions;
using Microsoft.Extensions.Options;
using NSubstitute;
using OpenTelemetry.Heartbeat.Monitor.Monitors;
using OpenTelemetry.Heartbeat.Monitor.Monitors.Models;
using OpenTelemetry.Heartbeat.Monitor.Monitors.Serialization;

namespace OpenTelemetry.Heartbeat.Monitor.Tests.Monitors;

public class MonitorDefinitionRepositoryTests
{
    [Theory, AutoNSubstituteData]
    public async Task GetFiles_Should_Return_Latest_File_For_Duplicated_Services(
        MonitorDefinitionSerializer serializer,
        MockFileSystem fileSystem,
        DateTime creationTime,
        CancellationToken cancellationToken)
    {
        // Arrange
        var options = Options.Create(new SearchOptions { RootDirectory = "/root", SearchPattern = "Heartbeat.json", IncludeSubDirectories = true });

        var oldFile = new MockFileData(CreateHttpMonitorJson("service", "foo", 200, 10, 5, "http://foo.com"))
        {
            CreationTime = creationTime.AddHours(-2)
        };
        
        var newFile = new MockFileData(CreateHttpMonitorJson("service", "foo", 200, 15, 2, "http://foo.com"))
        {
            CreationTime = creationTime.AddHours(-1)
        };
        
        var anotherFile = new MockFileData(CreateHttpMonitorJson("another-service", "foo", 200, 15, 2, "http://foo.com"))
        {
            CreationTime = creationTime.AddHours(-1)
        };
        
        fileSystem.AddFile("/root/my-service/v1.0.0/Heartbeat.json", oldFile);
        fileSystem.AddFile("/root/my-service/v2.0.0/Heartbeat.json", newFile);
        fileSystem.AddFile("/root/another-service/v1.0.0/Heartbeat.json", anotherFile);

        var sut = new MonitorDefinitionRepository(fileSystem, serializer, options);

        // Act
        var result = (await sut.GetMonitorDefinitions(cancellationToken)).ToList();

        // Assert
        result.Count.Should().BeGreaterThan(1);
        result.Should().Contain(x => x.Name == "service" && x.Interval == 15 && x.Http.TimeOut == 2);
        result.Should().Contain(x => x.Name == "another-service");
    }
    
    [Theory, AutoNSubstituteData]
    public async Task GetFiles_Should_Only_Return_Specified_Files_From_Options(
        IMonitorDefinitionSerializer serializer,
        MockFileSystem fileSystem,
        Stream stream,
        MonitorDefinition monitorDefinition,
        CancellationToken cancellationToken)
    {
        // Arrange
        var options = Options.Create(new SearchOptions { RootDirectory = "/root", SearchPattern = "Heartbeat.json", IncludeSubDirectories = true });
        serializer.DeserializeAsync(stream, cancellationToken).ReturnsForAnyArgs(monitorDefinition);
        
        fileSystem.AddFile("/root/v1.0.0/Heartbeat.json", new MockFileData(string.Empty));
        fileSystem.AddFile("/root/v2.0.0/another-file.json", new MockFileData(string.Empty));

        var sut = new MonitorDefinitionRepository(fileSystem, serializer, options);

        // Act
        var result = (await sut.GetMonitorDefinitions(cancellationToken)).ToList();

        // Assert
        result.Should().HaveCount(1);
    }
    
    // Top / sub directories
    [Theory, AutoNSubstituteData]
    public async Task GetFiles_Should_Only_Return_Top_Level_Files_If_IncludeSubDirectories_Is_False(
        IMonitorDefinitionSerializer serializer,
        MockFileSystem fileSystem,
        Stream stream,
        MonitorDefinition monitorDefinition,
        CancellationToken cancellationToken)
    {
        // Arrange
        var options = Options.Create(new SearchOptions { RootDirectory = "/root", SearchPattern = "Heartbeat.json", IncludeSubDirectories = true });
        serializer.DeserializeAsync(stream, cancellationToken).ReturnsForAnyArgs(monitorDefinition);
        
        fileSystem.AddFile("/root/Heartbeat.json", new MockFileData(string.Empty));
        fileSystem.AddFile("/root/v2.0.0/another-file.json", new MockFileData(string.Empty));

        var sut = new MonitorDefinitionRepository(fileSystem, serializer, options);

        // Act
        var result = (await sut.GetMonitorDefinitions(cancellationToken)).ToList();

        // Assert
        result.Should().HaveCount(1);
    }
    
    private static string CreateHttpMonitorJson (string name, string @namespace, int responseCode, int interval, int timeout, string url)
    {
        return $@"{{
            ""name"": ""{name}"",
            ""namespace"": ""{@namespace}"",
            ""interval"": {interval},
            ""type"": ""Http"",
            ""http"": {{
                ""ResponseCode"": {responseCode},
                ""TimeOut"": {timeout},
                ""Url"": ""{url}""
            }}
        }}";
    }
}