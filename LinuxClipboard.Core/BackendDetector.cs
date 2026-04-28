using System.Diagnostics;

namespace LinuxClipboard;

public class BackendDetector
{
    private readonly IProcessRunner? processRunner;
    private ClipboardBackend? backend;

    public enum ClipboardBackend
    {
        Linux,
        X11,
        None
    }

    public BackendDetector(IProcessRunner? runner = null)
    {
        processRunner = runner;
    }

    public ClipboardBackend DetectBackend()
    {
        if (backend.HasValue)
            return backend.Value;

        if (CommandExists("wl-copy") && CommandExists("wl-paste"))
        {
            backend = ClipboardBackend.Linux;
        }
        else if (CommandExists("xclip"))
        {
            backend = ClipboardBackend.X11;
        }
        else
        {
            backend = ClipboardBackend.None;
        }

        return backend.Value;
    }

    public bool CommandExists(string command)
    {
        try
        {
            var psi = new ProcessStartInfo(command, "--version")
            {
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false
            };

            var runner = processRunner ?? new DefaultProcessRunner();
            var p = runner.Start(psi);
            if (p == null)
                return false;

            p.WaitForExit(1000);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
