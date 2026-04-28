using Xunit;

namespace LinuxClipboard.Tests;

public class ConfigurationTests
{
    // Note: These tests require ResoniteModLoader which isn't available.
    // The configuration properties are:
    // - ClipboardTimeoutMs: defaults to 5000 if config is null
    // These can't be directly tested without loading the main assembly.

    [Fact]
    public void Placeholder()
    {
        // Configuration testing requires ResoniteModLoader.dll
        // Marking this as a placeholder
        Assert.True(true);
    }
}
