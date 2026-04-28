using System.Diagnostics;

namespace LinuxClipboard;

public interface IProcessRunner
{
    Process? Start(ProcessStartInfo psi);
}

public class DefaultProcessRunner : IProcessRunner
{
    public Process? Start(ProcessStartInfo psi)
    {
        return Process.Start(psi);
    }
}
