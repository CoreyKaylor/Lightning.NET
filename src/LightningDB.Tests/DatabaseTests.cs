using Shouldly;
using static System.Text.Encoding;

namespace LightningDB.Tests;

public class DatabaseTests : TestBase
{
    
    [Test]
    public void DatabaseShouldBeCreated()
    {
        using var env = CreateEnvironment();
        const string dbName = "test";
        env.MaxDatabases = 2;
        env.Open();
        using (var txn = env.BeginTransaction())
        using (txn.OpenDatabase(dbName, new DatabaseConfiguration { Flags = DatabaseOpenFlags.Create }))
        {
            txn.Commit();
        }
        using (var txn = env.BeginTransaction())
        using (var db = txn.OpenDatabase(dbName, new DatabaseConfiguration { Flags = DatabaseOpenFlags.None }))
        {
            db.IsReleased.ShouldBeFalse();
            txn.Commit();
        }
    }

    [Test]
    public void DatabaseShouldBeClosed()
    {
        using var env = CreateEnvironment();
        env.Open();
        using var txn = env.BeginTransaction();
        var db = txn.OpenDatabase();

        db.Dispose();

        db.IsOpened.ShouldBeFalse();
    }

    [Test]
    public void DatabaseFromCommittedTransactionShouldBeAccessible()
    {
        using var env = CreateEnvironment();
        env.Open();

        LightningDatabase db;
        using (var committed = env.BeginTransaction())
        {
            db = committed.OpenDatabase();
            committed.Commit();
        }

        using (db)
        using (var txn = env.BeginTransaction())
        {
            txn.Put(db, "key", 1.ToString());
            txn.Commit();
        }
    }

    [Test]
    public void NamedDatabaseNameExistsInMaster()
    {
        using var env = CreateEnvironment();
        env.MaxDatabases = 2;
        env.Open();

        using (var tx = env.BeginTransaction())
        {
            using var db = tx.OpenDatabase("customdb", new DatabaseConfiguration {Flags = DatabaseOpenFlags.Create});
            tx.Commit();
        }
        using (var tx = env.BeginTransaction())
        {
            using var db = tx.OpenDatabase();
            using (var cursor = tx.CreateCursor(db))
            {
                cursor.Next();
                UTF8.GetString(cursor.GetCurrent().key.CopyToNewArray())
                    .ShouldBe("customdb");
            }
        }
    }

    [Test]
    public void ReadonlyTransactionOpenedDatabasesDontGetReused()
    {
        //This is here to assert that previous issues with the way manager
        //classes (since removed) worked don't happen anymore.
        using var env = CreateEnvironment();
        env.MaxDatabases = 2;
        env.Open();

        using (var tx = env.BeginTransaction())
        using (var db = tx.OpenDatabase("custom", new DatabaseConfiguration { Flags = DatabaseOpenFlags.Create }))
        {
            tx.Put(db, "hello", "world");
            tx.Commit();
        }
        using (var tx = env.BeginTransaction(TransactionBeginFlags.ReadOnly))
        {
            using var db = tx.OpenDatabase("custom");
            var result = tx.Get(db, "hello");
            result.ShouldBe("world");
        }
        using (var tx = env.BeginTransaction(TransactionBeginFlags.ReadOnly))
        {
            using var db = tx.OpenDatabase("custom");
            var result = tx.Get(db, "hello");
            result.ShouldBe("world");
        }
    }

    [Test]
    public void DatabaseShouldBeDropped()
    {
        using var env = CreateEnvironment();
        env.MaxDatabases = 2;
        env.Open();
        using (var txn = env.BeginTransaction())
        {
            using var db = txn.OpenDatabase("notmaster", new DatabaseConfiguration { Flags = DatabaseOpenFlags.Create });
            txn.Commit();
        }

        using (var txn = env.BeginTransaction())
        {
            using var db = txn.OpenDatabase("notmaster");
            db.Drop(txn);
            txn.Commit();
        }

        using (var txn = env.BeginTransaction())
        {
            var ex = Assert.Throws<LightningException>(() => txn.OpenDatabase("notmaster"));
            ex.StatusCode.ShouldBe(-30798);
        }
    }

    [Test]
    public void TruncatingTheDatabase()
    {
        using var env = CreateEnvironment();
        env.Open();
        using (var txn = env.BeginTransaction())
        {
            using var db = txn.OpenDatabase();

            txn.Put(db, "hello", "world");
            txn.Commit();
        }

        using (var txn = env.BeginTransaction())
        {
            using var db = txn.OpenDatabase();
            db.Truncate(txn);
            txn.Commit();
        }

        using (var txn = env.BeginTransaction())
        {
            using var db = txn.OpenDatabase();
            var result = txn.Get(db, "hello"u8.ToArray());
            result.resultCode.ShouldBe(MDBResultCode.NotFound);
        }
    }

    [Test]
    public void DatabaseCanGetStats()
    {
        using var env = CreateEnvironment();
        env.Open();
        using var txn = env.BeginTransaction();
        using var db = txn.OpenDatabase();
        
        txn.Put(db, "key", 1.ToString()).ThrowOnError();
        var stats = db.DatabaseStats;
        stats.Entries.ShouldBe(1);
        stats.BranchPages.ShouldBe(0);
        stats.LeafPages.ShouldBe(1);
        stats.OverflowPages.ShouldBe(0);
        stats.PageSize.ShouldBe(env.EnvironmentStats.PageSize);
        stats.BTreeDepth.ShouldBe(1);
    }
}