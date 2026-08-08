using System;
using System.Text;
using Shouldly;

namespace LightningDB.Tests;

public class EncryptionTests : TestBase
{
    private static byte[] Key() => "0123456789abcdef0123456789abcdef"u8.ToArray();

    private LightningEnvironment CreateEncryptedEnvironment(string path, byte[]? key = null) =>
        CreateEnvironment(path, new EnvironmentConfiguration
        {
            Encryption = new EncryptionConfiguration(new AesGcmCipher(), key ?? Key())
        });

    public void can_write_and_read_in_encrypted_environment()
    {
        using var env = CreateEncryptedEnvironment(TempPath());
        env.Open();

        using (var tx = env.BeginTransaction())
        using (var db = tx.OpenDatabase())
        {
            tx.Put(db, "key"u8.ToArray(), "value"u8.ToArray());
            tx.Commit().ThrowOnError();
        }

        using (var tx = env.BeginTransaction(TransactionBeginFlags.ReadOnly))
        using (var db = tx.OpenDatabase())
        {
            var (resultCode, _, value) = tx.Get(db, "key"u8.ToArray());
            resultCode.ShouldBe(MDBResultCode.Success);
            Encoding.UTF8.GetString(value.CopyToNewArray()).ShouldBe("value");
        }
    }

    public void encrypted_environment_survives_reopen_with_same_key()
    {
        var path = TempPath();
        using (var env = CreateEncryptedEnvironment(path))
        {
            env.Open();
            using var tx = env.BeginTransaction();
            using var db = tx.OpenDatabase();
            tx.Put(db, "persisted"u8.ToArray(), "still here"u8.ToArray());
            tx.Commit().ThrowOnError();
        }

        using (var env = CreateEncryptedEnvironment(path))
        {
            env.Open();
            using var tx = env.BeginTransaction(TransactionBeginFlags.ReadOnly);
            using var db = tx.OpenDatabase();
            var (resultCode, _, value) = tx.Get(db, "persisted"u8.ToArray());
            resultCode.ShouldBe(MDBResultCode.Success);
            Encoding.UTF8.GetString(value.CopyToNewArray()).ShouldBe("still here");
        }
    }

    public void opening_encrypted_environment_with_wrong_key_fails()
    {
        var path = TempPath();
        using (var env = CreateEncryptedEnvironment(path))
        {
            env.Open();
            using var tx = env.BeginTransaction();
            using var db = tx.OpenDatabase();
            tx.Put(db, "secret"u8.ToArray(), "data"u8.ToArray());
            tx.Commit().ThrowOnError();
        }

        var wrongKey = "ffffffffffffffffffffffffffffffff"u8.ToArray();
        using (var env = CreateEncryptedEnvironment(path, wrongKey))
        {
            //data pages decrypt lazily, so the failure surfaces on first read
            //rather than at open
            try
            {
                env.Open();
                using var tx = env.BeginTransaction(TransactionBeginFlags.ReadOnly);
                using var db = tx.OpenDatabase();
                var (resultCode, _, _) = tx.Get(db, "secret"u8.ToArray());
                resultCode.ShouldNotBe(MDBResultCode.Success);
            }
            catch (LightningException)
            {
            }
        }
    }

    public void opening_encrypted_environment_without_cipher_fails()
    {
        var path = TempPath();
        using (var env = CreateEncryptedEnvironment(path))
        {
            env.Open();
            using var tx = env.BeginTransaction();
            using var db = tx.OpenDatabase();
            tx.Put(db, "secret"u8.ToArray(), "data"u8.ToArray());
            tx.Commit().ThrowOnError();
        }

        using (var env = CreateEnvironment(path))
        {
            Should.Throw<Exception>(() =>
            {
                env.Open();
                using var tx = env.BeginTransaction(TransactionBeginFlags.ReadOnly);
                using var db = tx.OpenDatabase();
                tx.Get(db, "secret"u8.ToArray());
            });
        }
    }

    public void encrypted_environment_reports_encrypted_flag()
    {
        using var env = CreateEncryptedEnvironment(TempPath());
        env.Open();
        env.Flags.HasFlag(EnvironmentOpenFlags.Encrypted).ShouldBeTrue();
    }

    public void checksum_environment_round_trip()
    {
        var path = TempPath();
        var config = new EnvironmentConfiguration { Checksum = new Sha256Checksum() };
        using (var env = CreateEnvironment(path, config))
        {
            env.Open();
            using var tx = env.BeginTransaction();
            using var db = tx.OpenDatabase();
            tx.Put(db, "summed"u8.ToArray(), "verified"u8.ToArray());
            tx.Commit().ThrowOnError();
        }

        using (var env = CreateEnvironment(path, new EnvironmentConfiguration { Checksum = new Sha256Checksum() }))
        {
            env.Open();
            using var tx = env.BeginTransaction(TransactionBeginFlags.ReadOnly);
            using var db = tx.OpenDatabase();
            var (resultCode, _, value) = tx.Get(db, "summed"u8.ToArray());
            resultCode.ShouldBe(MDBResultCode.Success);
            Encoding.UTF8.GetString(value.CopyToNewArray()).ShouldBe("verified");
        }
    }
}
