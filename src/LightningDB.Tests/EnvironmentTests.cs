using System;
using System.IO;
using Shouldly;

namespace LightningDB.Tests;

public class EnvironmentTests : TestBase
{
    public void can_get_environment_version()
    {
        using var env = CreateEnvironment();
        var version = env.Version;
        version.ShouldNotBeNull();
        version.Minor.ShouldBeGreaterThan(0);
    }

    public void environment_should_be_created_if_without_flags()
    {
        using var env = CreateEnvironment();
        env.Open();
    }

    public void environment_created_from_config()
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

    public void starting_transaction_before_environment_open()
    {
        using var env = CreateEnvironment();
        Should.Throw<InvalidOperationException>(() => env.BeginTransaction());
    }

    public void can_get_environment_info()
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

    public void can_get_large_environment_info_only_on_64bit_platform()
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

    public void max_databases_works_through_config_issue_62()
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

    public void can_load_and_dispose_multiple_environments()
    {
        var env = CreateEnvironment();
        env.Dispose();
        using var env2 = CreateEnvironment();
    }

    public void environment_should_be_created_if_read_only()
    {
        var env = CreateEnvironment();
        env.Open(); //readonly requires environment to have been created at least once before
        env.Dispose();
        env = CreateEnvironment(env.Path);
        using(env)
            env.Open(EnvironmentOpenFlags.ReadOnly);
    }

    public void environment_should_be_opened()
    {
        using var env = CreateEnvironment();
        env.Open();

        env.IsOpened.ShouldBeTrue();
    }

    public void environment_should_be_closed()
    {
        var env = CreateEnvironment();
        env.Open();
        env.Dispose();
        env.IsOpened.ShouldBeFalse();
    }

    public void can_put_multiple_key_value_pairs_and_flush_successfully()
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

    public void environment_should_be_copied()
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

    public void environment_should_fail_copy_if_path_is_file()
    {
        using var env = CreateEnvironment();
        env.Open();

        var filePath = Path.Combine(TempPath(), "test.txt");
        File.WriteAllBytes(filePath, Array.Empty<byte>());

        MDBResultCode result = env.CopyTo(filePath);
        result.ShouldNotBe(MDBResultCode.Success);
    }

    public void can_open_environment_more_than_50mb()
    {
        using var env = CreateEnvironment();
        env.MapSize = 55 * 1024 * 1024;
        env.Open();
    }

    public void can_open_environment_with_special_characters()
    {
        //all include special character now
        using var env = CreateEnvironment(TempPath("ß"));
        env.Open();
    }

    public void can_get_and_set_flags()
    {
        using var env = CreateEnvironment();
        env.Open();

        var flags = env.Flags;

        env.Flags = flags;

        env.Flags = EnvironmentOpenFlags.None;
        env.Flags.ShouldBe(EnvironmentOpenFlags.None);

        env.Flags = EnvironmentOpenFlags.NoSync;
        env.Flags.ShouldBe(EnvironmentOpenFlags.NoSync);

        env.Flags = EnvironmentOpenFlags.NoSync | EnvironmentOpenFlags.NoMetaSync;
        env.Flags.ShouldBe(EnvironmentOpenFlags.NoSync | EnvironmentOpenFlags.NoMetaSync);

        env.Flags.HasFlag(EnvironmentOpenFlags.NoSync).ShouldBeTrue();
        env.Flags.HasFlag(EnvironmentOpenFlags.NoMetaSync).ShouldBeTrue();

        env.Flags = flags;
        env.Flags.ShouldBe(flags);
    }

    public void can_check_stale_readers()
    {
        using var env = CreateEnvironment();
        env.Open();

        var staleReaders = env.CheckStaleReaders();
        staleReaders.ShouldBeGreaterThanOrEqualTo(0); // Should be 0 if no stale readers
    }

    public void should_get_version_info()
    {
        using var env = CreateEnvironment();
        var versionInfo = env.Version;

        // Verify version info properties are valid
        versionInfo.ShouldNotBeNull();
        versionInfo.Major.ShouldBeGreaterThanOrEqualTo(0);
        versionInfo.Minor.ShouldBeGreaterThan(0);
        versionInfo.Patch.ShouldBeGreaterThanOrEqualTo(0);
    }

    public void should_get_version_string()
    {
        using var env = CreateEnvironment();
        var versionInfo = env.Version;

        // LightningVersionInfo doesn't override ToString(), so it returns the type name
        // Just check that it returns a non-empty string
        var versionString = versionInfo.ToString();
        versionString.ShouldNotBeNullOrEmpty();
    }

    public void should_get_version_description()
    {
        using var env = CreateEnvironment();
        var versionInfo = env.Version;

        var versionDesc = versionInfo.Version;
        versionDesc.ShouldNotBeNullOrEmpty();

        // Version description should contain the library name and version
        versionDesc.ToLowerInvariant().ShouldContain("lmdb");
    }

    public void can_copy_to_file_stream()
    {
        // Setup a source environment with some data
        using var sourceEnv = CreateEnvironment();
        sourceEnv.Open();

        // Add some data to the source environment
        using (var tx = sourceEnv.BeginTransaction())
        using (var db = tx.OpenDatabase(null, new DatabaseConfiguration { Flags = DatabaseOpenFlags.Create }))
        {
            for (int i = 0; i < 5; i++)
            {
                tx.Put(db, $"key{i}", $"value{i}");
            }
            tx.Commit();
        }

        // Create a temporary file for the copy
        var tempFilePath = Path.Combine(TempPath(), "env_copy.mdb");

        // Ensure directory exists
        Directory.CreateDirectory(Path.GetDirectoryName(tempFilePath));

        // Create a FileStream to the destination file
        using (var fileStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.ReadWrite))
        {
            // Copy the environment to the FileStream
            var result = sourceEnv.CopyToStream(fileStream);
            result.ShouldBe(MDBResultCode.Success);
        }

        // Verify the file exists and has content
        File.Exists(tempFilePath).ShouldBeTrue("Destination file should exist");
        new FileInfo(tempFilePath).Length.ShouldBeGreaterThan(0, "Destination file should have content");
    }

    public void can_copy_with_compaction_to_file_stream()
    {
        // Setup a source environment with some data
        using var sourceEnv = CreateEnvironment();
        sourceEnv.Open();

        // Add some data to the source environment and then delete half of it to create free space
        using (var tx = sourceEnv.BeginTransaction())
        using (var db = tx.OpenDatabase(null, new DatabaseConfiguration { Flags = DatabaseOpenFlags.Create }))
        {
            for (int i = 0; i < 10; i++)
            {
                tx.Put(db, $"key{i}", $"value{i}");
            }
            tx.Commit();
        }

        // Delete some entries to create free space for compaction
        using (var tx = sourceEnv.BeginTransaction())
        using (var db = tx.OpenDatabase())
        {
            for (int i = 0; i < 5; i++)
            {
                tx.Delete(db, $"key{i}");
            }
            tx.Commit();
        }

        // Create a temporary file for the copy
        var tempFilePath = Path.Combine(TempPath(), "env_copy_compact.mdb");

        // Ensure directory exists
        Directory.CreateDirectory(Path.GetDirectoryName(tempFilePath));

        // Create a FileStream to the destination file
        using (var fileStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.ReadWrite))
        {
            // Copy the environment to the FileStream with compaction
            var result = sourceEnv.CopyToStream(fileStream, compact: true);
            result.ShouldBe(MDBResultCode.Success);
        }

        // Verify the file exists and has content
        File.Exists(tempFilePath).ShouldBeTrue("Destination file should exist");
        new FileInfo(tempFilePath).Length.ShouldBeGreaterThan(0, "Destination file should have content");
    }

    public void can_get_environment_file_stream()
    {
        // Skip this test on platforms where SafeFileHandle creation might not be supported
        using var env = CreateEnvironment();
        env.Open();

        // Add some data to make sure the file has content
        using (var tx = env.BeginTransaction())
        using (var db = tx.OpenDatabase(null, new DatabaseConfiguration { Flags = DatabaseOpenFlags.Create }))
        {
            tx.Put(db, "testkey", "testvalue");
            tx.Commit();
        }

        // Try to get a FileStream for the environment
        using var fileStream = env.GetFileStream();

        // Verify the FileStream is valid
        fileStream.ShouldNotBeNull();
        fileStream.CanRead.ShouldBeTrue();

        // Should be able to read data from the beginning of the file
        fileStream.Position = 0;
        var buffer = new byte[16];
        int bytesRead = fileStream.Read(buffer, 0, buffer.Length);

        // We should have read some data (the exact content depends on LMDB format)
        bytesRead.ShouldBeGreaterThan(0);
    }

    public void stream_throws_exception_for_null_argument()
    {
        using var env = CreateEnvironment();
        env.Open();

        // Null FileStream
        Should.Throw<ArgumentNullException>(() => env.CopyToStream(null));
    }

    public void stream_throws_exception_for_read_only_file_stream()
    {
        using var env = CreateEnvironment();
        env.Open();

        // Create a temporary file
        var tempFilePath = Path.Combine(TempPath(), "temp.txt");
        File.WriteAllText(tempFilePath, "test");

        // Open as read-only and verify it throws the expected exception
        using var readOnlyStream = new FileStream(tempFilePath, FileMode.Open, FileAccess.Read);
        Should.Throw<ArgumentException>(() => env.CopyToStream(readOnlyStream));
    }

    public void can_get_max_key_size()
    {
        using var env = CreateEnvironment();
        env.Open();

        // Get the maximum key size
        var maxKeySize = env.MaxKeySize;

        // The default max key size for LMDB is typically 511 bytes for non-dupsort databases
        // But we'll just verify it's a reasonable, positive value
        maxKeySize.ShouldBeGreaterThan(0);

        // Typical value is 511 or 511 * 4 (for larger page sizes)
        // Let's verify it's at least 100 bytes, which should be true for all configurations
        maxKeySize.ShouldBeGreaterThanOrEqualTo(100);
    }

    public void max_key_size_throws_when_environment_not_opened()
    {
        using var env = CreateEnvironment();
        // Don't open the environment

        // Attempting to get MaxKeySize should throw, as the environment must be opened
        Should.Throw<InvalidOperationException>(() => { _ = env.MaxKeySize; });
    }
}
