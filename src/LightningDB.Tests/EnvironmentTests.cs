using System;
using System.IO;
using Shouldly;

namespace LightningDB.Tests;

public class EnvironmentTests : TestBase
{
    public void CanGetEnvironmentVersion()
    {
        using var env = CreateEnvironment();
        var version = env.Version;
        version.ShouldNotBeNull();
        version.Minor.ShouldBeGreaterThan(0);
    }

    public void EnvironmentShouldBeCreatedIfWithoutFlags()
    {
        using var env = CreateEnvironment();
        env.Open();
    }

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

    public void StartingTransactionBeforeEnvironmentOpen()
    {
        using var env = CreateEnvironment();
        Should.Throw<InvalidOperationException>(() => env.BeginTransaction());
    }

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

    public void CanGetLargeEnvironmentInfoOnlyOn64BitPlatform()
    { 
        const long mapSize = 1024 * 1024 * 1024 * 3L;
        using var env = CreateEnvironment(config: new EnvironmentConfiguration
        {
            MapSize = mapSize
        });
        env.Open();
        var info = env.Info;
        env.MapSize.ShouldBe(info.MapSize);
    }

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

    public void CanLoadAndDisposeMultipleEnvironments()
    {
        var env = CreateEnvironment();
        env.Dispose();
        using var env2 = CreateEnvironment();
    }

    public void EnvironmentShouldBeCreatedIfReadOnly()
    {
        var env = CreateEnvironment();
        env.Open(); //readonly requires environment to have been created at least once before
        env.Dispose();
        env = CreateEnvironment(env.Path);
        using(env)
            env.Open(EnvironmentOpenFlags.ReadOnly);
    }

    public void EnvironmentShouldBeOpened()
    {
        using var env = CreateEnvironment();
        env.Open();

        env.IsOpened.ShouldBeTrue();
    }

    public void EnvironmentShouldBeClosed()
    {
        var env = CreateEnvironment();
        env.Open();
        env.Dispose();
        env.IsOpened.ShouldBeFalse();
    }

    public void CanPutMultipleKeyValuePairsAndFlushSuccessfully()
    {
        using var env = CreateEnvironment();
        env.Open();

        using var tx = env.BeginTransaction();
        using var db = tx.OpenDatabase(null, new DatabaseConfiguration { Flags = DatabaseOpenFlags.Create });

        for (int i = 0; i < 5; i++)
        {
            var key = BitConverter.GetBytes(i);
            var value = BitConverter.GetBytes(i * 10);
            tx.Put(db, key, value);
        }

        tx.Commit();

        var result = env.Flush(force: true);
        result.ShouldBe(MDBResultCode.Success);
    }

    public void EnvironmentShouldBeCopied()
    {
        void CopyTest(bool compact)
        {
            using var env = CreateEnvironment();
            env.Open();

            var newPath = TempPath();
            env.CopyTo(newPath, compact).ThrowOnError();

            (Directory.GetFiles(newPath).Length == 0)
                .ShouldBeFalse("Copied files doesn't exist");
        }
        CopyTest(true);
        CopyTest(false);
    }

    public void EnvironmentShouldFailCopyIfPathIsFile()
    {
        using var env = CreateEnvironment();
        env.Open();

        var filePath = Path.Combine(TempPath(), "test.txt");
        File.WriteAllBytes(filePath, Array.Empty<byte>());
        
        MDBResultCode result = env.CopyTo(filePath);
        result.ShouldNotBe(MDBResultCode.Success);
    }

    public void CanOpenEnvironmentMoreThan50Mb()
    {
        using var env = CreateEnvironment();
        env.MapSize = 55 * 1024 * 1024;
        env.Open();
    }
    
    public void CanOpenEnvironmentWithSpecialCharacters()
    {
        //all include special character now
        using var env = new LightningEnvironment(TempPath("ß"));
        env.Open();
    }
}