using Xunit;

namespace LinuxClipboard.Tests;

public class MimeTypeTests
{
    // Note: MIME type handling uses Process.Start for clipboard operations.
    // Testing this requires mocking Process.Start or having wl-clipboard/xclip installed.

    [Fact]
    public void Placeholder()
    {
        // MIME type testing requires mocking or actual clipboard tools
        Assert.True(true);
    }
}
