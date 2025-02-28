using Shouldly;
using static System.Text.Encoding;

namespace LightningDB.Tests;

public class DatabaseTests : TestBase
{
    
    public void database_should_be_created()
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

    public void database_should_be_closed()
    {
        using var env = CreateEnvironment();
        env.Open();
        using var txn = env.BeginTransaction();
        var db = txn.OpenDatabase();

        db.Dispose();

        db.IsOpened.ShouldBeFalse();
    }

    public void database_from_committed_transaction_should_be_accessible()
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

    public void named_database_name_exists_in_master()
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

    public void readonly_transaction_opened_databases_dont_get_reused()
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

    public void database_should_be_dropped()
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
            var ex = Should.Throw<LightningException>(() => txn.OpenDatabase("notmaster"));
            ex.StatusCode.ShouldBe(-30798);
        }
    }

    public void truncating_the_database()
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

    public void database_can_get_stats()
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
    
    public void can_get_database_flags()
    {
        using var env = CreateEnvironment();
        env.MaxDatabases = 10; // Allow more named databases
        env.Open();
        
        // Test with the transaction-based GetFlags method using a named database with IntegerKey flag
        using (var txn = env.BeginTransaction())
        {
            using var db = txn.OpenDatabase("intkey", new DatabaseConfiguration 
            { 
                Flags = DatabaseOpenFlags.Create | DatabaseOpenFlags.IntegerKey 
            });
            
            // Test using explicit transaction
            var flags = db.GetFlags(txn);
            flags.ShouldNotBe(DatabaseOpenFlags.None);
            
            // The flags should include DatabaseOpenFlags.IntegerKey
            flags.HasFlag(DatabaseOpenFlags.IntegerKey).ShouldBeTrue();
            txn.Commit();
        }
        
        // Test default database (should have no special flags)
        using (var txn = env.BeginTransaction())
        {
            using var db = txn.OpenDatabase(null, new DatabaseConfiguration { Flags = DatabaseOpenFlags.Create });
            
            var flags = db.GetFlags(txn);
            flags.ShouldBe(DatabaseOpenFlags.None);
            txn.Commit();
        }
        
        // Test DuplicatesSort flag
        using (var txn = env.BeginTransaction())
        {
            using var db = txn.OpenDatabase("dupsort", new DatabaseConfiguration 
            { 
                Flags = DatabaseOpenFlags.Create | DatabaseOpenFlags.DuplicatesSort 
            });
            
            var flags = db.GetFlags(txn);
            flags.HasFlag(DatabaseOpenFlags.DuplicatesSort).ShouldBeTrue();
            txn.Commit();
        }
    }
}