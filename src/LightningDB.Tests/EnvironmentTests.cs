using System;
using System.IO;
using Xunit;

namespace LightningDB.Tests;

[Collection("SharedFileSystem")]
public class EnvironmentTests : IDisposable
{
    private readonly string _path,  _pathCopy, _pathSpecial;
    private LightningEnvironment _env;

    public EnvironmentTests(SharedFileSystem fileSystem)
    {
        _path = fileSystem.CreateNewDirectoryForTest();
        _pathCopy = fileSystem.CreateNewDirectoryForTest();
        _pathSpecial = fileSystem.CreateNewDirectoryForSpecialCharacterTest();
    }

    public void Dispose()
    {
        _env?.Dispose();
        _env = null;
    }

    [Fact]
    public void EnvironmentShouldBeCreatedIfWithoutFlags()
    {
        _env = new LightningEnvironment(_path);
        _env.Open();
    }

    [Fact]
    public void EnvironmentCreatedFromConfig()
    {
        const int mapExpected = 1024*1024*20;
        const int maxDatabaseExpected = 2;
        const int maxReadersExpected = 3;
        var config = new EnvironmentConfiguration {MapSize = mapExpected, MaxDatabases = maxDatabaseExpected, MaxReaders = maxReadersExpected};
        _env = new LightningEnvironment(_path, config);
        Assert.Equal(mapExpected, _env.MapSize);
        Assert.Equal(maxDatabaseExpected, _env.MaxDatabases);
        Assert.Equal(maxReadersExpected, _env.MaxReaders);
    }

    [Fact]
    public void StartingTransactionBeforeEnvironmentOpen()
    {
        _env = new LightningEnvironment(_path);
        Assert.Throws<InvalidOperationException>(() => _env.BeginTransaction());
    }

    [Fact]
    public void CanGetEnvironmentInfo()
    {
        const long mapSize = 1024 * 1024 * 200;
        _env = new LightningEnvironment(_path, new EnvironmentConfiguration
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
        _env = new LightningEnvironment(_path, new EnvironmentConfiguration
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
        _env = new LightningEnvironment(_path, config);
        _env.Open();
        using (var tx = _env.BeginTransaction())
        {
            tx.OpenDatabase("db1", new DatabaseConfiguration {Flags = DatabaseOpenFlags.Create});
            tx.OpenDatabase("db2", new DatabaseConfiguration {Flags = DatabaseOpenFlags.Create});
            tx.Commit();
        }
        Assert.Equal(2, _env.MaxDatabases);
    }

    [Fact]
    public void CanLoadAndDisposeMultipleEnvironments()
    {
        _env = new LightningEnvironment(_path);
        _env.Dispose();
        _env = new LightningEnvironment(_path);
    }

    [Fact]
    public void EnvironmentShouldBeCreatedIfReadOnly()
    {
        _env = new LightningEnvironment(_path);
        _env.Open(); //readonly requires environment to have been created at least once before
        _env.Dispose();
        _env = new LightningEnvironment(_path);
        _env.Open(EnvironmentOpenFlags.ReadOnly);
    }

    [Fact]
    public void EnvironmentShouldBeOpened()
    {
        _env = new LightningEnvironment(_path);
        _env.Open();

        Assert.True(_env.IsOpened);
    }

    [Fact]
    public void EnvironmentShouldBeClosed()
    {
        _env = new LightningEnvironment(_path);
        _env.Open();

        _env.Dispose();

        Assert.False(_env.IsOpened);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void EnvironmentShouldBeCopied(bool compact)
    {
        _env = new LightningEnvironment(_path);
        _env.Open(); 

        _env.CopyTo(_pathCopy, compact);

        if (Directory.GetFiles(_pathCopy).Length == 0)
            Assert.True(false, "Copied files doesn't exist");
    }

    [Fact]
    public void CanOpenEnvironmentMoreThan50Mb()
    {
        _env = new LightningEnvironment(_path)
        {
            MapSize = 55 * 1024 * 1024
        };

        _env.Open();
    }
    
    [Fact]
    public void CanOpenEnvironmentWithSpecialCharacters()
    {
        _env = new LightningEnvironment(_pathSpecial);

        _env.Open();
    }
        
        
#if NETCOREAPP3_1_OR_GREATER
    [WindowsOnlyFact]
    public void CreateEnvironmentWithAutoResize()
    {
        using (var env = new LightningEnvironment(_path, new EnvironmentConfiguration
               {
                   MapSize = 1048576,
                   AutoResizeWindows = true
               }))
        {
            env.Open();
        }

        using (var env = new LightningEnvironment(_path, new EnvironmentConfiguration
               {
                   MapSize = 1048576,
                   AutoResizeWindows = true
               }))
        {
            env.Open();
        }

        using (var dbFile = File.OpenRead(Path.Combine(_path, "data.mdb")))
        {
            Assert.Equal(8192, dbFile.Length);
        }
    }
#endif
}