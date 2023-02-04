using System.Diagnostics;
using System.IO;
using System.Text;
using Xunit;

namespace LightningDB.Tests;

[Collection("SharedFileSystem")]
public class MultiProcessTests 
{
    private readonly SharedFileSystem _fileSystem;

    public MultiProcessTests(SharedFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }

    [Fact]
    public void can_load_environment_from_multiple_processes()
    {
        var name = _fileSystem.CreateNewDirectoryForTest();
        using var env = new LightningEnvironment(name);
        env.Open();
        var otherProcessPath = Path.GetFullPath("../../../../SecondProcess/bin/Debug/net7.0/SecondProcess.dll");
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"{otherProcessPath} {name}",
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
        Assert.NotEqual(current.Id, process.Id);
            
        var result = process.StandardOutput.ReadLine();
        process.WaitForExit();
        Assert.Equal(expected, result);
    }

}