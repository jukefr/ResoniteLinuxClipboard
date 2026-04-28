using System.Reflection;
using Xunit;

namespace LinuxClipboard.Tests;

public class DiscoveryToolsTests
{
    [Theory]
    [InlineData("Inspector", 1)]
    [InlineData("Font", 1)]
    [InlineData("FontChain", 2)]
    [InlineData("Text", 1)]
    [InlineData("Style", 1)]
    [InlineData("Theme", 1)]
    [InlineData("Label", 1)]
    [InlineData("InspectorText", 2)]
    [InlineData("FontStyle", 2)]
    [InlineData("SomeInspectorFont", 2)]
    [InlineData("UnrelatedClass", 0)]
    [InlineData("", 0)]
    [InlineData(null, 0)]
    public void CountKeywordMatches_ReturnsExpectedCount(string? input, int expected)
    {
        // Act
        var result = DiscoveryTools.CountKeywordMatches(input);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void IsInterestingAssembly_WithCurrentAssembly_ReturnsFalse()
    {
        // Arrange
        var assembly = typeof(DiscoveryToolsTests).Assembly;

        // Act
        var result = DiscoveryTools.IsInterestingAssembly(assembly);

        // Assert - our test assembly won't start with FrooxEngine or Elements.
        Assert.False(result);
    }

    [Fact]
    public void ScoreType_WithStringType_ReturnsZero()
    {
        // Arrange
        var type = typeof(string);

        // Act
        var result = DiscoveryTools.ScoreType(type);

        // Assert - "String" doesn't contain any keywords
        Assert.Equal(0, result);
    }

    [Fact]
    public void ScoreMethod_WithSubstringMethod_ReturnsZero()
    {
        // Arrange
        var method = typeof(string).GetMethod(nameof(string.Substring), new[] { typeof(int) })!;

        // Act
        var result = DiscoveryTools.ScoreMethod(method);

        // Assert - "Substring" doesn't contain any keywords
        Assert.Equal(0, result);
    }

    // Helper class to test scoring with inspector-related names
    public class InspectorClass
    {
        public void ProcessText() { }
        public void SetFont() { }
        public int Label { get; set; }
    }

    [Fact]
    public void ScoreType_WithInspectorClassName_ReturnsPositive()
    {
        // Arrange
        var type = typeof(InspectorClass);

        // Act
        var result = DiscoveryTools.ScoreType(type);

        // Assert - "Inspector" is in the class name
        Assert.True(result > 0);
    }

    [Fact]
    public void ScoreMethod_WithTextInMethodName_ReturnsPositive()
    {
        // Arrange
        var method = typeof(InspectorClass).GetMethod(nameof(InspectorClass.ProcessText))!;

        // Act
        var result = DiscoveryTools.ScoreMethod(method);

        // Assert - "Text" is in the method name
        Assert.True(result > 0);
    }
}
