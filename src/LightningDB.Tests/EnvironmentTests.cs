using System;
using System.IO;
using System.Runtime.InteropServices;
using Shouldly;

namespace LightningDB.Tests;

public class EnvironmentTests : TestBase
{
    [Test]
    public void EnvironmentShouldBeCreatedIfWithoutFlags()
    {
        using var env = CreateEnvironment();
        env.Open();
    }

    [Test]
    public void EnvironmentCreatedFromConfig()
    {
        const int mapExpected = 1024*1024*20;
        const int maxDatabaseExpected = 2;
        const int maxReadersExpected = 3;
        var config = new EnvironmentConfiguration {MapSize = mapExpected, MaxDatabases = maxDatabaseExpected, MaxReaders = maxReadersExpected};
        using var env = CreateEnvironment(config: config);
        env.MapSize.ShouldBe(mapExpected);
        env.MaxDatabases.ShouldBe(maxDatabaseExpected);
        env.MaxReaders.ShouldBe(maxReadersExpected);
    }

    [Test]
    public void StartingTransactionBeforeEnvironmentOpen()
    {
        using var env = CreateEnvironment();
        Assert.Throws<InvalidOperationException>(() => env.BeginTransaction());
    }

    [Test]
    public void CanGetEnvironmentInfo()
    {
        const long mapSize = 1024 * 1024 * 200;
        using var env = CreateEnvironment(config: new EnvironmentConfiguration
        {
            MapSize = mapSize
        });
        env.Open();
        var info = env.Info;
        info.MapSize.ShouldBe(env.MapSize);
    }

    [Test]
    public void CanGetLargeEnvironmentInfo()
    { 
        Skip.When(RuntimeInformation.OSArchitecture == Architecture.X86,"Skipping for x86 platform");
        const long mapSize = 1024 * 1024 * 1024 * 3L;
        using var env = CreateEnvironment(config: new EnvironmentConfiguration
        {
            MapSize = mapSize
        });
        env.Open();
        var info = env.Info;
        env.MapSize.ShouldBe(info.MapSize);
    }

    [Test]
    public void MaxDatabasesWorksThroughConfigIssue62()
    {
        var config = new EnvironmentConfiguration { MaxDatabases = 2 };
        using var env = CreateEnvironment(config: config);
        env.Open();
        using (var tx = env.BeginTransaction())
        {
            using var db = tx.OpenDatabase("db1", new DatabaseConfiguration {Flags = DatabaseOpenFlags.Create});
            using var db2 = tx.OpenDatabase("db2", new DatabaseConfiguration {Flags = DatabaseOpenFlags.Create});
            tx.Commit();
        }
        env.MaxDatabases.ShouldBe(2);
    }

    [Test]
    public void CanLoadAndDisposeMultipleEnvironments()
    {
        var env = CreateEnvironment();
        env.Dispose();
        using var env2 = CreateEnvironment();
    }

    [Test]
    public void EnvironmentShouldBeCreatedIfReadOnly()
    {
        var env = CreateEnvironment();
        env.Open(); //readonly requires environment to have been created at least once before
        env.Dispose();
        env = CreateEnvironment(env.Path);
        using(env)
            env.Open(EnvironmentOpenFlags.ReadOnly);
    }

    [Test]
    public void EnvironmentShouldBeOpened()
    {
        using var env = CreateEnvironment();
        env.Open();

        env.IsOpened.ShouldBeTrue();
    }

    [Test]
    public void EnvironmentShouldBeClosed()
    {
        var env = CreateEnvironment();
        env.Open();
        env.Dispose();
        env.IsOpened.ShouldBeFalse();
    }

    [Test]
    [Arguments(true)]
    [Arguments(false)]
    public void EnvironmentShouldBeCopied(bool compact)
    {
        using var env = CreateEnvironment();
        env.Open();

        var newPath = TempPath();
        env.CopyTo(newPath, compact).ThrowOnError();

        if (Directory.GetFiles(newPath).Length == 0)
            Assert.Fail("Copied files doesn't exist");
    }

    [Test]
    public void EnvironmentShouldFailCopyIfPathIsFile()
    {
        using var env = CreateEnvironment();
        env.Open();

        var filePath = Path.Combine(TempPath(), "test.txt");
        File.WriteAllBytes(filePath, Array.Empty<byte>());
        
        MDBResultCode result = env.CopyTo(filePath);
        result.ShouldNotBe(MDBResultCode.Success);
    }

    [Test]
    public void CanOpenEnvironmentMoreThan50Mb()
    {
        using var env = CreateEnvironment();
        env.MapSize = 55 * 1024 * 1024;
        env.Open();
    }
    
    [Test]
    public void CanOpenEnvironmentWithSpecialCharacters()
    {
        //all include special character now
        using var env = new LightningEnvironment(TempPath("ß"));
        env.Open();
    }
}