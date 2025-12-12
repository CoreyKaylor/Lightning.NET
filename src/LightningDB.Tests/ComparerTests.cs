using System;
using System.Runtime.InteropServices;
using LightningDB.Comparers;
using Shouldly;

namespace LightningDB.Tests;

public class ComparerTests : TestBase
{
    public void bitwise_comparer_sorts_keys_lexicographically()
    {
        using var env = CreateEnvironment();
        env.Open();

        var config = new DatabaseConfiguration { Flags = DatabaseOpenFlags.Create };
        config.CompareWith(BitwiseComparer.Instance);

        using var txn = env.BeginTransaction();
        using var db = txn.OpenDatabase(configuration: config);

        txn.Put(db, new byte[] { 1, 2, 4 }, new byte[] { 1 });
        txn.Put(db, new byte[] { 1, 2, 3 }, new byte[] { 2 });
        txn.Put(db, new byte[] { 1, 2, 5 }, new byte[] { 3 });

        using var cursor = txn.CreateCursor(db);

        cursor.Next().Item1.ShouldBe(MDBResultCode.Success);
        cursor.GetCurrent().key.CopyToNewArray().ShouldBe(new byte[] { 1, 2, 3 });

        cursor.Next().Item1.ShouldBe(MDBResultCode.Success);
        cursor.GetCurrent().key.CopyToNewArray().ShouldBe(new byte[] { 1, 2, 4 });

        cursor.Next().Item1.ShouldBe(MDBResultCode.Success);
        cursor.GetCurrent().key.CopyToNewArray().ShouldBe(new byte[] { 1, 2, 5 });

        cursor.Next().Item1.ShouldBe(MDBResultCode.NotFound);
    }

    public void reverse_bitwise_comparer_sorts_keys_in_reverse()
    {
        using var env = CreateEnvironment();
        env.Open();

        var config = new DatabaseConfiguration { Flags = DatabaseOpenFlags.Create };
        config.CompareWith(ReverseBitwiseComparer.Instance);

        using var txn = env.BeginTransaction();
        using var db = txn.OpenDatabase(configuration: config);

        txn.Put(db, new byte[] { 1, 2, 4 }, new byte[] { 1 });
        txn.Put(db, new byte[] { 1, 2, 3 }, new byte[] { 2 });
        txn.Put(db, new byte[] { 1, 2, 5 }, new byte[] { 3 });

        using var cursor = txn.CreateCursor(db);

        cursor.Next().Item1.ShouldBe(MDBResultCode.Success);
        cursor.GetCurrent().key.CopyToNewArray().ShouldBe(new byte[] { 1, 2, 5 });

        cursor.Next().Item1.ShouldBe(MDBResultCode.Success);
        cursor.GetCurrent().key.CopyToNewArray().ShouldBe(new byte[] { 1, 2, 4 });

        cursor.Next().Item1.ShouldBe(MDBResultCode.Success);
        cursor.GetCurrent().key.CopyToNewArray().ShouldBe(new byte[] { 1, 2, 3 });

        cursor.Next().Item1.ShouldBe(MDBResultCode.NotFound);
    }

    public void signed_integer_comparer_sorts_int32_with_negatives_first()
    {
        using var env = CreateEnvironment();
        env.Open();

        var config = new DatabaseConfiguration { Flags = DatabaseOpenFlags.Create };
        config.CompareWith(SignedIntegerComparer.Instance);

        using var txn = env.BeginTransaction();
        using var db = txn.OpenDatabase(configuration: config);

        txn.Put(db, BitConverter.GetBytes(50), BitConverter.GetBytes(50));
        txn.Put(db, BitConverter.GetBytes(-10), BitConverter.GetBytes(-10));
        txn.Put(db, BitConverter.GetBytes(100), BitConverter.GetBytes(100));
        txn.Put(db, BitConverter.GetBytes(-50), BitConverter.GetBytes(-50));
        txn.Put(db, BitConverter.GetBytes(0), BitConverter.GetBytes(0));

        using var cursor = txn.CreateCursor(db);

        cursor.Next().Item1.ShouldBe(MDBResultCode.Success);
        MemoryMarshal.Read<int>(cursor.GetCurrent().key.AsSpan()).ShouldBe(-50);

        cursor.Next().Item1.ShouldBe(MDBResultCode.Success);
        MemoryMarshal.Read<int>(cursor.GetCurrent().key.AsSpan()).ShouldBe(-10);

        cursor.Next().Item1.ShouldBe(MDBResultCode.Success);
        MemoryMarshal.Read<int>(cursor.GetCurrent().key.AsSpan()).ShouldBe(0);

        cursor.Next().Item1.ShouldBe(MDBResultCode.Success);
        MemoryMarshal.Read<int>(cursor.GetCurrent().key.AsSpan()).ShouldBe(50);

        cursor.Next().Item1.ShouldBe(MDBResultCode.Success);
        MemoryMarshal.Read<int>(cursor.GetCurrent().key.AsSpan()).ShouldBe(100);

        cursor.Next().Item1.ShouldBe(MDBResultCode.NotFound);
    }

    public void signed_integer_comparer_sorts_int64_with_negatives_first()
    {
        using var env = CreateEnvironment();
        env.Open();

        var config = new DatabaseConfiguration { Flags = DatabaseOpenFlags.Create };
        config.CompareWith(SignedIntegerComparer.Instance);

        using var txn = env.BeginTransaction();
        using var db = txn.OpenDatabase(configuration: config);

        txn.Put(db, BitConverter.GetBytes(50L), BitConverter.GetBytes(50L));
        txn.Put(db, BitConverter.GetBytes(-10L), BitConverter.GetBytes(-10L));
        txn.Put(db, BitConverter.GetBytes(0L), BitConverter.GetBytes(0L));

        using var cursor = txn.CreateCursor(db);

        cursor.Next().Item1.ShouldBe(MDBResultCode.Success);
        MemoryMarshal.Read<long>(cursor.GetCurrent().key.AsSpan()).ShouldBe(-10L);

        cursor.Next().Item1.ShouldBe(MDBResultCode.Success);
        MemoryMarshal.Read<long>(cursor.GetCurrent().key.AsSpan()).ShouldBe(0L);

        cursor.Next().Item1.ShouldBe(MDBResultCode.Success);
        MemoryMarshal.Read<long>(cursor.GetCurrent().key.AsSpan()).ShouldBe(50L);

        cursor.Next().Item1.ShouldBe(MDBResultCode.NotFound);
    }

    public void reverse_signed_integer_comparer_sorts_descending()
    {
        using var env = CreateEnvironment();
        env.Open();

        var config = new DatabaseConfiguration { Flags = DatabaseOpenFlags.Create };
        config.CompareWith(ReverseSignedIntegerComparer.Instance);

        using var txn = env.BeginTransaction();
        using var db = txn.OpenDatabase(configuration: config);

        txn.Put(db, BitConverter.GetBytes(50), BitConverter.GetBytes(50));
        txn.Put(db, BitConverter.GetBytes(-10), BitConverter.GetBytes(-10));
        txn.Put(db, BitConverter.GetBytes(0), BitConverter.GetBytes(0));

        using var cursor = txn.CreateCursor(db);

        cursor.Next().Item1.ShouldBe(MDBResultCode.Success);
        MemoryMarshal.Read<int>(cursor.GetCurrent().key.AsSpan()).ShouldBe(50);

        cursor.Next().Item1.ShouldBe(MDBResultCode.Success);
        MemoryMarshal.Read<int>(cursor.GetCurrent().key.AsSpan()).ShouldBe(0);

        cursor.Next().Item1.ShouldBe(MDBResultCode.Success);
        MemoryMarshal.Read<int>(cursor.GetCurrent().key.AsSpan()).ShouldBe(-10);

        cursor.Next().Item1.ShouldBe(MDBResultCode.NotFound);
    }

    public void unsigned_integer_comparer_sorts_uint32()
    {
        using var env = CreateEnvironment();
        env.Open();

        var config = new DatabaseConfiguration { Flags = DatabaseOpenFlags.Create };
        config.CompareWith(UnsignedIntegerComparer.Instance);

        using var txn = env.BeginTransaction();
        using var db = txn.OpenDatabase(configuration: config);

        txn.Put(db, BitConverter.GetBytes(50u), BitConverter.GetBytes(50u));
        txn.Put(db, BitConverter.GetBytes(100u), BitConverter.GetBytes(100u));
        txn.Put(db, BitConverter.GetBytes(0u), BitConverter.GetBytes(0u));
        txn.Put(db, BitConverter.GetBytes(uint.MaxValue), BitConverter.GetBytes(uint.MaxValue));

        using var cursor = txn.CreateCursor(db);

        cursor.Next().Item1.ShouldBe(MDBResultCode.Success);
        MemoryMarshal.Read<uint>(cursor.GetCurrent().key.AsSpan()).ShouldBe(0u);

        cursor.Next().Item1.ShouldBe(MDBResultCode.Success);
        MemoryMarshal.Read<uint>(cursor.GetCurrent().key.AsSpan()).ShouldBe(50u);

        cursor.Next().Item1.ShouldBe(MDBResultCode.Success);
        MemoryMarshal.Read<uint>(cursor.GetCurrent().key.AsSpan()).ShouldBe(100u);

        cursor.Next().Item1.ShouldBe(MDBResultCode.Success);
        MemoryMarshal.Read<uint>(cursor.GetCurrent().key.AsSpan()).ShouldBe(uint.MaxValue);

        cursor.Next().Item1.ShouldBe(MDBResultCode.NotFound);
    }

    public void unsigned_integer_comparer_sorts_uint64()
    {
        using var env = CreateEnvironment();
        env.Open();

        var config = new DatabaseConfiguration { Flags = DatabaseOpenFlags.Create };
        config.CompareWith(UnsignedIntegerComparer.Instance);

        using var txn = env.BeginTransaction();
        using var db = txn.OpenDatabase(configuration: config);

        txn.Put(db, BitConverter.GetBytes(50UL), BitConverter.GetBytes(50UL));
        txn.Put(db, BitConverter.GetBytes(0UL), BitConverter.GetBytes(0UL));
        txn.Put(db, BitConverter.GetBytes(ulong.MaxValue), BitConverter.GetBytes(ulong.MaxValue));

        using var cursor = txn.CreateCursor(db);

        cursor.Next().Item1.ShouldBe(MDBResultCode.Success);
        MemoryMarshal.Read<ulong>(cursor.GetCurrent().key.AsSpan()).ShouldBe(0UL);

        cursor.Next().Item1.ShouldBe(MDBResultCode.Success);
        MemoryMarshal.Read<ulong>(cursor.GetCurrent().key.AsSpan()).ShouldBe(50UL);

        cursor.Next().Item1.ShouldBe(MDBResultCode.Success);
        MemoryMarshal.Read<ulong>(cursor.GetCurrent().key.AsSpan()).ShouldBe(ulong.MaxValue);

        cursor.Next().Item1.ShouldBe(MDBResultCode.NotFound);
    }

    public void reverse_unsigned_integer_comparer_sorts_descending()
    {
        using var env = CreateEnvironment();
        env.Open();

        var config = new DatabaseConfiguration { Flags = DatabaseOpenFlags.Create };
        config.CompareWith(ReverseUnsignedIntegerComparer.Instance);

        using var txn = env.BeginTransaction();
        using var db = txn.OpenDatabase(configuration: config);

        txn.Put(db, BitConverter.GetBytes(50u), BitConverter.GetBytes(50u));
        txn.Put(db, BitConverter.GetBytes(100u), BitConverter.GetBytes(100u));
        txn.Put(db, BitConverter.GetBytes(0u), BitConverter.GetBytes(0u));

        using var cursor = txn.CreateCursor(db);

        cursor.Next().Item1.ShouldBe(MDBResultCode.Success);
        MemoryMarshal.Read<uint>(cursor.GetCurrent().key.AsSpan()).ShouldBe(100u);

        cursor.Next().Item1.ShouldBe(MDBResultCode.Success);
        MemoryMarshal.Read<uint>(cursor.GetCurrent().key.AsSpan()).ShouldBe(50u);

        cursor.Next().Item1.ShouldBe(MDBResultCode.Success);
        MemoryMarshal.Read<uint>(cursor.GetCurrent().key.AsSpan()).ShouldBe(0u);

        cursor.Next().Item1.ShouldBe(MDBResultCode.NotFound);
    }

    public void utf8_string_comparer_sorts_strings()
    {
        using var env = CreateEnvironment();
        env.Open();

        var config = new DatabaseConfiguration { Flags = DatabaseOpenFlags.Create };
        config.CompareWith(Utf8StringComparer.Instance);

        using var txn = env.BeginTransaction();
        using var db = txn.OpenDatabase(configuration: config);

        txn.Put(db, "banana"u8.ToArray(), new byte[] { 1 });
        txn.Put(db, "apple"u8.ToArray(), new byte[] { 2 });
        txn.Put(db, "cherry"u8.ToArray(), new byte[] { 3 });

        using var cursor = txn.CreateCursor(db);

        cursor.Next().Item1.ShouldBe(MDBResultCode.Success);
        System.Text.Encoding.UTF8.GetString(cursor.GetCurrent().key.AsSpan()).ShouldBe("apple");

        cursor.Next().Item1.ShouldBe(MDBResultCode.Success);
        System.Text.Encoding.UTF8.GetString(cursor.GetCurrent().key.AsSpan()).ShouldBe("banana");

        cursor.Next().Item1.ShouldBe(MDBResultCode.Success);
        System.Text.Encoding.UTF8.GetString(cursor.GetCurrent().key.AsSpan()).ShouldBe("cherry");

        cursor.Next().Item1.ShouldBe(MDBResultCode.NotFound);
    }

    public void reverse_utf8_string_comparer_sorts_descending()
    {
        using var env = CreateEnvironment();
        env.Open();

        var config = new DatabaseConfiguration { Flags = DatabaseOpenFlags.Create };
        config.CompareWith(ReverseUtf8StringComparer.Instance);

        using var txn = env.BeginTransaction();
        using var db = txn.OpenDatabase(configuration: config);

        txn.Put(db, "banana"u8.ToArray(), new byte[] { 1 });
        txn.Put(db, "apple"u8.ToArray(), new byte[] { 2 });
        txn.Put(db, "cherry"u8.ToArray(), new byte[] { 3 });

        using var cursor = txn.CreateCursor(db);

        cursor.Next().Item1.ShouldBe(MDBResultCode.Success);
        System.Text.Encoding.UTF8.GetString(cursor.GetCurrent().key.AsSpan()).ShouldBe("cherry");

        cursor.Next().Item1.ShouldBe(MDBResultCode.Success);
        System.Text.Encoding.UTF8.GetString(cursor.GetCurrent().key.AsSpan()).ShouldBe("banana");

        cursor.Next().Item1.ShouldBe(MDBResultCode.Success);
        System.Text.Encoding.UTF8.GetString(cursor.GetCurrent().key.AsSpan()).ShouldBe("apple");

        cursor.Next().Item1.ShouldBe(MDBResultCode.NotFound);
    }

    public void length_comparer_sorts_by_length_first()
    {
        using var env = CreateEnvironment();
        env.Open();

        var config = new DatabaseConfiguration { Flags = DatabaseOpenFlags.Create };
        config.CompareWith(LengthComparer.Instance);

        using var txn = env.BeginTransaction();
        using var db = txn.OpenDatabase(configuration: config);

        txn.Put(db, new byte[] { 255, 255, 255 }, new byte[] { 1 });
        txn.Put(db, new byte[] { 0 }, new byte[] { 2 });
        txn.Put(db, new byte[] { 128, 128 }, new byte[] { 3 });

        using var cursor = txn.CreateCursor(db);

        cursor.Next().Item1.ShouldBe(MDBResultCode.Success);
        cursor.GetCurrent().key.CopyToNewArray().ShouldBe(new byte[] { 0 });

        cursor.Next().Item1.ShouldBe(MDBResultCode.Success);
        cursor.GetCurrent().key.CopyToNewArray().ShouldBe(new byte[] { 128, 128 });

        cursor.Next().Item1.ShouldBe(MDBResultCode.Success);
        cursor.GetCurrent().key.CopyToNewArray().ShouldBe(new byte[] { 255, 255, 255 });

        cursor.Next().Item1.ShouldBe(MDBResultCode.NotFound);
    }

    public void reverse_length_comparer_sorts_longer_first()
    {
        using var env = CreateEnvironment();
        env.Open();

        var config = new DatabaseConfiguration { Flags = DatabaseOpenFlags.Create };
        config.CompareWith(ReverseLengthComparer.Instance);

        using var txn = env.BeginTransaction();
        using var db = txn.OpenDatabase(configuration: config);

        txn.Put(db, new byte[] { 1, 2, 3 }, new byte[] { 1 });
        txn.Put(db, new byte[] { 1 }, new byte[] { 2 });
        txn.Put(db, new byte[] { 1, 2 }, new byte[] { 3 });

        using var cursor = txn.CreateCursor(db);

        cursor.Next().Item1.ShouldBe(MDBResultCode.Success);
        cursor.GetCurrent().key.CopyToNewArray().ShouldBe(new byte[] { 1, 2, 3 });

        cursor.Next().Item1.ShouldBe(MDBResultCode.Success);
        cursor.GetCurrent().key.CopyToNewArray().ShouldBe(new byte[] { 1, 2 });

        cursor.Next().Item1.ShouldBe(MDBResultCode.Success);
        cursor.GetCurrent().key.CopyToNewArray().ShouldBe(new byte[] { 1 });

        cursor.Next().Item1.ShouldBe(MDBResultCode.NotFound);
    }

    public void length_only_comparer_treats_same_length_as_equal()
    {
        using var env = CreateEnvironment();
        env.Open();

        var config = new DatabaseConfiguration { Flags = DatabaseOpenFlags.Create };
        config.CompareWith(LengthOnlyComparer.Instance);

        using var txn = env.BeginTransaction();
        using var db = txn.OpenDatabase(configuration: config);

        txn.Put(db, new byte[] { 1, 2, 3 }, new byte[] { 1 });
        txn.Put(db, new byte[] { 4, 5, 6 }, new byte[] { 2 });

        using var cursor = txn.CreateCursor(db);

        cursor.Next().Item1.ShouldBe(MDBResultCode.Success);
        cursor.GetCurrent().value.CopyToNewArray().ShouldBe(new byte[] { 2 });

        cursor.Next().Item1.ShouldBe(MDBResultCode.NotFound);
    }

    public void hash_code_comparer_provides_consistent_ordering()
    {
        using var env = CreateEnvironment();
        env.Open();

        var config = new DatabaseConfiguration { Flags = DatabaseOpenFlags.Create };
        config.CompareWith(HashCodeComparer.Instance);

        using var txn = env.BeginTransaction();
        using var db = txn.OpenDatabase(configuration: config);

        var key1 = new byte[] { 1, 2, 3 };
        var key2 = new byte[] { 4, 5, 6 };
        var key3 = new byte[] { 7, 8, 9 };

        txn.Put(db, key1, key1);
        txn.Put(db, key2, key2);
        txn.Put(db, key3, key3);

        var order1 = new System.Collections.Generic.List<byte[]>();
        using (var cursor = txn.CreateCursor(db))
        {
            while (cursor.Next().Item1 == MDBResultCode.Success)
                order1.Add(cursor.GetCurrent().key.CopyToNewArray());
        }

        order1.Count.ShouldBe(3);

        txn.Commit();
        using var txn2 = env.BeginTransaction(TransactionBeginFlags.ReadOnly);
        using var db2 = txn2.OpenDatabase(configuration: config);

        var order2 = new System.Collections.Generic.List<byte[]>();
        using (var cursor = txn2.CreateCursor(db2))
        {
            while (cursor.Next().Item1 == MDBResultCode.Success)
                order2.Add(cursor.GetCurrent().key.CopyToNewArray());
        }

        order1.Count.ShouldBe(order2.Count);
        for (var i = 0; i < order1.Count; i++)
            order1[i].ShouldBe(order2[i]);
    }

    public void reverse_bitwise_comparer_works_with_duplicate_values()
    {
        using var env = CreateEnvironment();
        env.Open();

        var config = new DatabaseConfiguration { Flags = DatabaseOpenFlags.Create | DatabaseOpenFlags.DuplicatesSort };
        config.FindDuplicatesWith(ReverseBitwiseComparer.Instance);

        using var txn = env.BeginTransaction();
        using var db = txn.OpenDatabase(configuration: config);

        var key = new byte[] { 1 };
        txn.Put(db, key, new byte[] { 1, 2, 3 });
        txn.Put(db, key, new byte[] { 4, 5, 6 });
        txn.Put(db, key, new byte[] { 2, 3, 4 });

        using var cursor = txn.CreateCursor(db);
        cursor.SetKey(key);

        cursor.GetCurrent().value.CopyToNewArray().ShouldBe(new byte[] { 4, 5, 6 });

        cursor.NextDuplicate().Item1.ShouldBe(MDBResultCode.Success);
        cursor.GetCurrent().value.CopyToNewArray().ShouldBe(new byte[] { 2, 3, 4 });

        cursor.NextDuplicate().Item1.ShouldBe(MDBResultCode.Success);
        cursor.GetCurrent().value.CopyToNewArray().ShouldBe(new byte[] { 1, 2, 3 });

        cursor.NextDuplicate().Item1.ShouldBe(MDBResultCode.NotFound);
    }
}
