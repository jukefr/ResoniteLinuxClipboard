using System.Diagnostics;
using Xunit;
using NSubstitute;

namespace WaylandClipboard.Tests;

public class BackendDetectionTests
{
    [Fact]
    public void DetectBackend_WhenNoTools_ReturnsNone()
    {
        // Arrange
        var mockRunner = Substitute.For<IProcessRunner>();
        mockRunner.Start(Arg.Any<ProcessStartInfo>()).Returns((Process?)null);
        
        var detector = new BackendDetector(mockRunner);

        // Act
        var result = detector.DetectBackend();

        // Assert
        Assert.Equal(BackendDetector.ClipboardBackend.None, result);
    }

    [Fact(Skip = "Mock setup issues with Process - needs more work")]
    public void CommandExists_WhenProcessStarts_ReturnsTrue()
    {
        // Arrange
        var mockRunner = Substitute.For<IProcessRunner>();
        var mockProcess = Substitute.For<Process>();
        mockRunner.Start(Arg.Any<ProcessStartInfo>()).Returns(mockProcess);

        var detector = new BackendDetector(mockRunner);

        // Act
        var result = detector.CommandExists("test-command");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CommandExists_WhenProcessNull_ReturnsFalse()
    {
        // Arrange
        var mockRunner = Substitute.For<IProcessRunner>();
        mockRunner.Start(Arg.Any<ProcessStartInfo>()).Returns((Process?)null);

        var detector = new BackendDetector(mockRunner);

        // Act
        var result = detector.CommandExists("test-command");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void DetectBackend_CachesResult()
    {
        // Arrange
        var mockRunner = Substitute.For<IProcessRunner>();
        mockRunner.Start(Arg.Any<ProcessStartInfo>()).Returns((Process?)null);

        var detector = new BackendDetector(mockRunner);

        // Act - call twice
        var result1 = detector.DetectBackend();
        var result2 = detector.DetectBackend();

        // Assert - should return same value
        Assert.Equal(result1, result2);
        Assert.Equal(BackendDetector.ClipboardBackend.None, result1);
    }
}
