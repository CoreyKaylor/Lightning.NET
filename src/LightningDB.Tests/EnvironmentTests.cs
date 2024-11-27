using System;
using System.IO;
using Xunit;

namespace LightningDB.Tests;

public class EnvironmentTests(SharedFileSystem fileSystem) : TestBase(fileSystem, false)
{
    [Fact]
    public void EnvironmentShouldBeCreatedIfWithoutFlags()
    {
        CreateEnvironment();
        _env.Open();
    }

    [Fact]
    public void EnvironmentCreatedFromConfig()
    {
        const int mapExpected = 1024*1024*20;
        const int maxDatabaseExpected = 2;
        const int maxReadersExpected = 3;
        var config = new EnvironmentConfiguration {MapSize = mapExpected, MaxDatabases = maxDatabaseExpected, MaxReaders = maxReadersExpected};
        CreateEnvironment(config: config);
        Assert.Equal(mapExpected, _env.MapSize);
        Assert.Equal(maxDatabaseExpected, _env.MaxDatabases);
        Assert.Equal(maxReadersExpected, _env.MaxReaders);
    }

    [Fact]
    public void StartingTransactionBeforeEnvironmentOpen()
    {
        CreateEnvironment();
        Assert.Throws<InvalidOperationException>(() => _env.BeginTransaction());
    }

    [Fact]
    public void CanGetEnvironmentInfo()
    {
        const long mapSize = 1024 * 1024 * 200;
        CreateEnvironment(config: new EnvironmentConfiguration
        {
            MapSize = mapSize
        });
        _env.Open();
        var info = _env.Info;
        Assert.Equal(_env.MapSize, info.MapSize);
    }

    [Not32BitFact]
    public void CanGetLargeEnvironmentInfo()
    {
        const long mapSize = 1024 * 1024 * 1024 * 3L;
        CreateEnvironment(config: new EnvironmentConfiguration
        {
            MapSize = mapSize
        });
        _env.Open();
        var info = _env.Info;
        Assert.Equal(_env.MapSize, info.MapSize);
    }

    [Fact]
    public void MaxDatabasesWorksThroughConfigIssue62()
    {
        var config = new EnvironmentConfiguration { MaxDatabases = 2 };
        CreateEnvironment(config: config);
        _env.Open();
        using (var tx = _env.BeginTransaction())
        {
            using var db = tx.OpenDatabase("db1", new DatabaseConfiguration {Flags = DatabaseOpenFlags.Create});
            using var db2 = tx.OpenDatabase("db2", new DatabaseConfiguration {Flags = DatabaseOpenFlags.Create});
            tx.Commit();
        }
        Assert.Equal(2, _env.MaxDatabases);
    }

    [Fact]
    public void CanLoadAndDisposeMultipleEnvironments()
    {
        CreateEnvironment();
        _env.Dispose();
        CreateEnvironment();
    }

    [Fact]
    public void EnvironmentShouldBeCreatedIfReadOnly()
    {
        CreateEnvironment();
        _env.Open(); //readonly requires environment to have been created at least once before
        _env.Dispose();
        CreateEnvironment(_env.Path);
        _env.Open(EnvironmentOpenFlags.ReadOnly);
    }

    [Fact]
    public void EnvironmentShouldBeOpened()
    {
        CreateEnvironment();
        _env.Open();

        Assert.True(_env.IsOpened);
    }

    [Fact]
    public void EnvironmentShouldBeClosed()
    {
        CreateEnvironment();
        _env.Open();
        _env.Dispose();
        Assert.False(_env.IsOpened);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void EnvironmentShouldBeCopied(bool compact)
    {
        CreateEnvironment();
        _env.Open();

        var newPath = TempPath();
        _env.CopyTo(newPath, compact).ThrowOnError();

        if (Directory.GetFiles(newPath).Length == 0)
            Assert.Fail("Copied files doesn't exist");
    }

    [Fact]
    public void EnvironmentShouldFailCopyIfPathIsFile()
    {
        CreateEnvironment();
        _env.Open();

        var filePath = Path.Combine(TempPath(), "test.txt");
        File.WriteAllBytes(filePath, Array.Empty<byte>());
        
        MDBResultCode result = _env.CopyTo(filePath);
        Assert.NotEqual(MDBResultCode.Success, result);
    }

    [Fact]
    public void CanOpenEnvironmentMoreThan50Mb()
    {
        CreateEnvironment();
        _env.MapSize = 55 * 1024 * 1024;
        _env.Open();
    }
    
    [Fact]
    public void CanOpenEnvironmentWithSpecialCharacters()
    {
        //all include special character now
        _env = new LightningEnvironment(TempPath("ß"));
        _env.Open();
    }
}