using System;
using System.IO;
using System.Runtime.InteropServices;
using Shouldly;

namespace LightningDB.Tests;

public class EnvironmentTests() : TestBase(false)
{
    [Test]
    public void EnvironmentShouldBeCreatedIfWithoutFlags()
    {
        _env = CreateEnvironment();
        _env.Open();
    }

    [Test]
    public void EnvironmentCreatedFromConfig()
    {
        const int mapExpected = 1024*1024*20;
        const int maxDatabaseExpected = 2;
        const int maxReadersExpected = 3;
        var config = new EnvironmentConfiguration {MapSize = mapExpected, MaxDatabases = maxDatabaseExpected, MaxReaders = maxReadersExpected};
        _env = CreateEnvironment(config: config);
        _env.MapSize.ShouldBe(mapExpected);
        _env.MaxDatabases.ShouldBe(maxDatabaseExpected);
        _env.MaxReaders.ShouldBe(maxReadersExpected);
    }

    [Test]
    public void StartingTransactionBeforeEnvironmentOpen()
    {
        _env = CreateEnvironment();
        Assert.Throws<InvalidOperationException>(() => _env.BeginTransaction());
    }

    [Test]
    public void CanGetEnvironmentInfo()
    {
        const long mapSize = 1024 * 1024 * 200;
        _env = CreateEnvironment(config: new EnvironmentConfiguration
        {
            MapSize = mapSize
        });
        _env.Open();
        var info = _env.Info;
        info.MapSize.ShouldBe(_env.MapSize);
    }

    [Test]
    public void CanGetLargeEnvironmentInfo()
    { 
        Skip.When(RuntimeInformation.OSArchitecture == Architecture.X86,"Skipping for x86 platform");
        const long mapSize = 1024 * 1024 * 1024 * 3L;
        _env = CreateEnvironment(config: new EnvironmentConfiguration
        {
            MapSize = mapSize
        });
        _env.Open();
        var info = _env.Info;
        _env.MapSize.ShouldBe(info.MapSize);
    }

    [Test]
    public void MaxDatabasesWorksThroughConfigIssue62()
    {
        var config = new EnvironmentConfiguration { MaxDatabases = 2 };
        _env = CreateEnvironment(config: config);
        _env.Open();
        using (var tx = _env.BeginTransaction())
        {
            using var db = tx.OpenDatabase("db1", new DatabaseConfiguration {Flags = DatabaseOpenFlags.Create});
            using var db2 = tx.OpenDatabase("db2", new DatabaseConfiguration {Flags = DatabaseOpenFlags.Create});
            tx.Commit();
        }
        _env.MaxDatabases.ShouldBe(2);
    }

    [Test]
    public void CanLoadAndDisposeMultipleEnvironments()
    {
        _env = CreateEnvironment();
        _env.Dispose();
        _env = CreateEnvironment();
    }

    [Test]
    public void EnvironmentShouldBeCreatedIfReadOnly()
    {
        _env = CreateEnvironment();
        _env.Open(); //readonly requires environment to have been created at least once before
        _env.Dispose();
        _env = CreateEnvironment(_env.Path);
        _env.Open(EnvironmentOpenFlags.ReadOnly);
    }

    [Test]
    public void EnvironmentShouldBeOpened()
    {
        _env = CreateEnvironment();
        _env.Open();

        _env.IsOpened.ShouldBeTrue();
    }

    [Test]
    public void EnvironmentShouldBeClosed()
    {
        _env = CreateEnvironment();
        _env.Open();
        _env.Dispose();
        _env.IsOpened.ShouldBeFalse();
    }

    [Test]
    [Arguments(true)]
    [Arguments(false)]
    public void EnvironmentShouldBeCopied(bool compact)
    {
        _env = CreateEnvironment();
        _env.Open();

        var newPath = TempPath();
        _env.CopyTo(newPath, compact).ThrowOnError();

        if (Directory.GetFiles(newPath).Length == 0)
            Assert.Fail("Copied files doesn't exist");
    }

    [Test]
    public void EnvironmentShouldFailCopyIfPathIsFile()
    {
        _env = CreateEnvironment();
        _env.Open();

        var filePath = Path.Combine(TempPath(), "test.txt");
        File.WriteAllBytes(filePath, Array.Empty<byte>());
        
        MDBResultCode result = _env.CopyTo(filePath);
        result.ShouldNotBe(MDBResultCode.Success);
    }

    [Test]
    public void CanOpenEnvironmentMoreThan50Mb()
    {
        _env = CreateEnvironment();
        _env.MapSize = 55 * 1024 * 1024;
        _env.Open();
    }
    
    [Test]
    public void CanOpenEnvironmentWithSpecialCharacters()
    {
        //all include special character now
        _env = new LightningEnvironment(TempPath("ß"));
        _env.Open();
    }
}