﻿using System;
using Xunit;
using static System.Text.Encoding;

namespace LightningDB.Tests;

[Collection("SharedFileSystem")]
public class DatabaseTests : IDisposable
{
    private readonly LightningEnvironment _env;
    private LightningTransaction _txn;

    public DatabaseTests(SharedFileSystem fileSystem)
    {
        var path = fileSystem.CreateNewDirectoryForTest();
        _env = new LightningEnvironment(path);
    }

    public void Dispose()
    {
        _env.Dispose();
    }
        
    [Fact]
    public void DatabaseShouldBeCreated()
    {
        const string dbName = "test";
        _env.MaxDatabases = 2;
        _env.Open();
        using (var txn = _env.BeginTransaction())
        using (txn.OpenDatabase(dbName, new DatabaseConfiguration { Flags = DatabaseOpenFlags.Create }))
        {
            txn.Commit();
        }
        using (var txn = _env.BeginTransaction())
        using (var db = txn.OpenDatabase(dbName, new DatabaseConfiguration { Flags = DatabaseOpenFlags.None }))
        {
            Assert.False(db.IsReleased);
            txn.Commit();
        }
    }

    [Fact]
    public void DatabaseShouldBeClosed()
    {
        _env.Open();
        _txn = _env.BeginTransaction();
        var db = _txn.OpenDatabase();

        db.Dispose();

        Assert.False(db.IsOpened);
    }

    [Fact]
    public void DatabaseFromCommittedTransactionShouldBeAccessible()
    {
        _env.Open();

        LightningDatabase db;
        using (var committed = _env.BeginTransaction())
        {
            db = committed.OpenDatabase();
            committed.Commit();
        }

        using (db)
        using (var txn = _env.BeginTransaction())
        {
            txn.Put(db, "key", 1.ToString());
            txn.Commit();
        }
    }

    [Fact]
    public void NamedDatabaseNameExistsInMaster()
    {
        _env.MaxDatabases = 2;
        _env.Open();

        using (var tx = _env.BeginTransaction())
        {
            tx.OpenDatabase("customdb", new DatabaseConfiguration {Flags = DatabaseOpenFlags.Create});
            tx.Commit();
        }
        using (var tx = _env.BeginTransaction())
        {
            var db = tx.OpenDatabase();
            using (var cursor = tx.CreateCursor(db))
            {
                cursor.Next();
                Assert.Equal("customdb", UTF8.GetString(cursor.GetCurrent().key.CopyToNewArray()));
            }
        }
    }

    [Fact]
    public void ReadonlyTransactionOpenedDatabasesDontGetReused()
    {
        //This is here to assert that previous issues with the way manager
        //classes (since removed) worked don't happen anymore.
        _env.MaxDatabases = 2;
        _env.Open();

        using (var tx = _env.BeginTransaction())
        using (var db = tx.OpenDatabase("custom", new DatabaseConfiguration { Flags = DatabaseOpenFlags.Create }))
        {
            tx.Put(db, "hello", "world");
            tx.Commit();
        }
        using (var tx = _env.BeginTransaction(TransactionBeginFlags.ReadOnly))
        {
            var db = tx.OpenDatabase("custom");
            var result = tx.Get(db, "hello");
            Assert.Equal("world", result);
        }
        using (var tx = _env.BeginTransaction(TransactionBeginFlags.ReadOnly))
        {
            var db = tx.OpenDatabase("custom");
            var result = tx.Get(db, "hello");
            Assert.Equal("world", result);
        }
    }

    [Fact]
    public void DatabaseShouldBeDropped()
    {
        _env.MaxDatabases = 2;
        _env.Open();
        _txn = _env.BeginTransaction();
        var db = _txn.OpenDatabase("notmaster", new DatabaseConfiguration {Flags = DatabaseOpenFlags.Create});
        _txn.Commit();
        _txn.Dispose();
        db.Dispose();

        _txn = _env.BeginTransaction();
        db = _txn.OpenDatabase("notmaster");

        db.Drop(_txn);
        _txn.Commit();
        _txn.Dispose();

        _txn = _env.BeginTransaction();

        var ex = Assert.Throws<LightningException>(() => _txn.OpenDatabase("notmaster"));

        Assert.Equal(-30798, ex.StatusCode);
    }

    [Fact]
    public void TruncatingTheDatabase()
    {
        _env.Open();
        _txn = _env.BeginTransaction();
        var db = _txn.OpenDatabase();

        _txn.Put(db, "hello", "world");
        _txn.Commit();
        _txn.Dispose();
        _txn = _env.BeginTransaction();
        db = _txn.OpenDatabase();
        db.Truncate(_txn);
        _txn.Commit();
        _txn.Dispose();
        _txn = _env.BeginTransaction();
        db = _txn.OpenDatabase();
        var result = _txn.Get(db, "hello"u8.ToArray());

        Assert.Equal(MDBResultCode.NotFound, result.resultCode);
    }
}