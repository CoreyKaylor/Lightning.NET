using System;
using System.IO;
using System.Reflection;
using Xunit;
using Xunit.Sdk;

namespace LightningDB.Tests;

public class SharedFileSystem : IDisposable
{
    private readonly string _testTempDir;

    public SharedFileSystem()
    {
        _testTempDir = Path.Combine(Path.GetTempPath(), Environment.Version.ToString(), "ldb");
    }

    public void Dispose()
    {
        if (Directory.Exists(_testTempDir))
            Directory.Delete(_testTempDir, true);
    }

    public string CreateNewDirectoryForTest(string seed = "")
    {
        var path = Path.Combine(_testTempDir, $"t{seed}", Guid.NewGuid().ToString());
        Directory.CreateDirectory(path);
        return path;
    }
}

[CollectionDefinition("SharedFileSystem")]
public class SharedFileSystemCollection : ICollectionFixture<SharedFileSystem>
{
}

public class PrintTestMethod : BeforeAfterTestAttribute
{
    public override void Before(MethodInfo methodUnderTest)
    {
        Console.WriteLine("Setup for test '{0}.'", methodUnderTest.Name);
    }

    public override void After(MethodInfo methodUnderTest)
    {
        Console.WriteLine("TearDown for test '{0}.'", methodUnderTest.Name);
    }
}