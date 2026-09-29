using System;
using System.IO;
using System.Linq;
using System.Text;
using LightningDB;

namespace SecondProcess;

class Program
{
    static void Main(string[] args)
    {
        switch (args.First())
        {
            case "cursor-bad-txn":
                ReproduceCursorConstructorFinalizerCrash(args[1]);
                return;
            case "txn-readers-full":
                ReproduceTransactionConstructorFinalizerCrash(args[1]);
                return;
        }

        var name = args.First();
        using var env = new LightningEnvironment(name);
        env.Open(EnvironmentOpenFlags.ReadOnly);
        byte[] results;
        using (var tx = env.BeginTransaction(TransactionBeginFlags.ReadOnly))
        {
            using var db = tx.OpenDatabase();
            var result = tx.Get(db, "hello"u8.ToArray());
            results = result.value.CopyToNewArray();
            tx.Commit();
        }

        Console.WriteLine(Encoding.UTF8.GetString(results));
    }

    // Poisons a write transaction with MDB_MAP_FULL, then calls CreateCursor on it so
    // mdb_cursor_open fails with MDB_BAD_TXN inside the LightningCursor constructor.
    // Before the fix, the half-built LightningCursor is still finalizer-registered and
    // its finalizer dereferences the never-assigned Database field, throwing a
    // NullReferenceException on the finalizer thread and aborting the process.
    static void ReproduceCursorConstructorFinalizerCrash(string path)
    {
        Directory.CreateDirectory(path);
        using var env = new LightningEnvironment(path) { MapSize = 64 * 1024 };
        env.Open();

        using (var setup = env.BeginTransaction())
        {
            using var setupDb = setup.OpenDatabase(configuration: new DatabaseConfiguration { Flags = DatabaseOpenFlags.Create });
            setup.Commit();
        }

        using var tx = env.BeginTransaction();
        using var db = tx.OpenDatabase();

        var value = new byte[4096];
        MDBResultCode result;
        var i = 0;
        do
        {
            result = tx.Put(db, BitConverter.GetBytes(i++), value);
        } while (result == MDBResultCode.Success);

        if (result != MDBResultCode.MapFull)
            throw new InvalidOperationException($"Expected MDB_MAP_FULL while filling the map, got {result}");

        try
        {
            tx.CreateCursor(db);
            throw new InvalidOperationException("Expected CreateCursor to throw on the poisoned transaction");
        }
        catch (LightningException)
        {
            // Expected: mdb_cursor_open returns MDB_BAD_TXN for a poisoned transaction.
        }

        GC.Collect();
        GC.WaitForPendingFinalizers();

        Console.WriteLine("OK");
    }

    // Exhausts the reader lock table (MaxReaders = 1) and then attempts a second
    // read-only BeginTransaction, so mdb_txn_begin fails with MDB_READERS_FULL inside
    // the LightningTransaction constructor. Before the fix, the half-built
    // LightningTransaction is still finalizer-registered; after the environment is
    // disposed and GC runs, its finalizer throws "A transaction must be disposed
    // before closing the environment" on the finalizer thread and aborts the process.
    static void ReproduceTransactionConstructorFinalizerCrash(string path)
    {
        Directory.CreateDirectory(path);
        var env = new LightningEnvironment(path, new EnvironmentConfiguration { MaxReaders = 1 });
        env.Open();

        using (env.BeginTransaction(TransactionBeginFlags.ReadOnly))
        {
            try
            {
                env.BeginTransaction(TransactionBeginFlags.ReadOnly);
                throw new InvalidOperationException("Expected BeginTransaction to throw with the reader lock table full");
            }
            catch (LightningException)
            {
                // Expected: mdb_txn_begin returns MDB_READERS_FULL.
            }
        }

        env.Dispose();

        GC.Collect();
        GC.WaitForPendingFinalizers();

        Console.WriteLine("OK");
    }
}
