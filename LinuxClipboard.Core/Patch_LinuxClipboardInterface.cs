using System.Diagnostics;

namespace LinuxClipboard;

public class Patch_LinuxClipboardInterface
{
    internal enum ClipboardBackend
    {
        Linux,
        X11,
        None
    }

    private static ClipboardBackend? backend;
    private static IProcessRunner? processRunner;

    internal static void SetProcessRunner(IProcessRunner runner) => processRunner = runner;
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
    }

    internal static bool CommandExists(string command)
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
            using var p = runner.Start(psi);
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

    internal static string[] GetClipboardMimes()
    {
        // Simplified version - actual implementation uses Process.Start
        return [];
    }
}

internal class DefaultProcessRunner : IProcessRunner
{
    public Process? Start(ProcessStartInfo psi)
    {
        return Process.Start(psi);
    }
}
