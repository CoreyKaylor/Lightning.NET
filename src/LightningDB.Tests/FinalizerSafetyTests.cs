using System.Diagnostics;
using System.IO;
using Shouldly;

namespace LightningDB.Tests;

// These scenarios run in a child process because the bug they reproduce is an
// unhandled exception on the finalizer thread, which is fatal to the process:
// there is no way to observe it in-process other than the whole test process dying.
// Before the fix, the child process aborts (exit code 134) instead of printing "OK".
public class FinalizerSafetyTests : TestBase
{
    private static (int exitCode, string output) RunSecondProcess(string mode, string path)
    {
        var otherProcessPath = Path.GetFullPath("SecondProcess.dll");
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"{otherProcessPath} {mode} {path}",
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = Directory.GetCurrentDirectory()
            }
        };

        process.Start();
        var output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();
        return (process.ExitCode, output);
    }

    public void cursor_constructor_failure_must_not_crash_finalizer_thread()
    {
        var (exitCode, output) = RunSecondProcess("cursor-bad-txn", TempPath());
        exitCode.ShouldBe(0);
        output.ShouldContain("OK");
    }

    public void transaction_constructor_failure_must_not_crash_finalizer_thread()
    {
        var (exitCode, output) = RunSecondProcess("txn-readers-full", TempPath());
        exitCode.ShouldBe(0);
        output.ShouldContain("OK");
    }
}
