using System.Diagnostics;
using System.IO;
using System.Text;
using Shouldly;

namespace LightningDB.Tests;

public class MultiProcessTests : TestBase 
{

    [Test]
    public void can_load_environment_from_multiple_processes()
    {
        var env = CreateEnvironment();
        env.Open();
        var otherProcessPath = Path.GetFullPath("SecondProcess.dll");
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"{otherProcessPath} {env.Path}",
                RedirectStandardError = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = Directory.GetCurrentDirectory()
            }
        };

        const string expected = "world";
        using var tx = env.BeginTransaction();
        using var db = tx.OpenDatabase();
        tx.Put(db, "hello"u8.ToArray(), Encoding.UTF8.GetBytes(expected));
        tx.Commit();
            
        var current = Process.GetCurrentProcess();
        process.Start();
        current.Id.ShouldNotBe(process.Id);
            
        var result = process.StandardOutput.ReadLine();
        process.WaitForExit();
        result.ShouldBe(expected);
    }

}