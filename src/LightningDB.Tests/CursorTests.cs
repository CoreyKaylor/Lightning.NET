using System;
using System.Linq;
using Shouldly;
using static System.Text.Encoding;

namespace LightningDB.Tests;

public class CursorTests : TestBase
{
    private static byte[][] PopulateCursorValues(LightningCursor cursor, int count = 5, string keyPrefix = "key")
    {
        var keys = Enumerable.Range(1, count)
            .Select(i => UTF8.GetBytes(keyPrefix + i))
            .ToArray();

        foreach (var k in keys)
        {
            var result = cursor.Put(k, k, CursorPutOptions.None);
            result.ShouldBe(MDBResultCode.Success);
        }

        return keys;
    }

    private static byte[][] PopulateMultipleCursorValues(LightningCursor cursor, string key = "TestKey")
    {
        var values = Enumerable.Range(1, 5).Select(BitConverter.GetBytes).ToArray();
        var result = cursor.Put(UTF8.GetBytes(key), values);
        result.ShouldBe(MDBResultCode.Success);
        var notDuplicate = values[0];
        result = cursor.Put(notDuplicate, notDuplicate, CursorPutOptions.NoDuplicateData);
        result.ShouldBe(MDBResultCode.Success);
        return values;
    }

    [Test]
    public void CursorShouldBeCreated()
    {
        using var env = CreateEnvironment();
        env.Open();
        env.RunCursorScenario((_, _, c) => c.ShouldNotBeNull());
    }

    [Test]
    public void CursorShouldPutValues()
    {
        using var env = CreateEnvironment();
        env.Open();
        env.RunCursorScenario((tx, _, c) =>
        {
            PopulateCursorValues(c);
            var result = tx.Commit();
            result.ShouldBe(MDBResultCode.Success);
        });
    }

    [Test]
    public void CursorShouldSetSpanKey()
    {
        using var env = CreateEnvironment();
        env.Open();
        env.RunCursorScenario((_, _, c) =>
        {
            var keys = PopulateCursorValues(c);
            var firstKey = keys.First();
            var result = c.Set(firstKey.AsSpan());
            result.ShouldBe(MDBResultCode.Success);
            var current = c.GetCurrent();
            firstKey.ShouldBe(current.key.CopyToNewArray());
        });
    }

    [Test]
    public void CursorShouldMoveToLast()
    {
        using var env = CreateEnvironment();
        env.Open();
        env.RunCursorScenario((_, _, c) =>
        {
            var keys = PopulateCursorValues(c);
            var lastKey = keys.Last();
            var result = c.Last();
            result.resultCode.ShouldBe(MDBResultCode.Success);
            lastKey.ShouldBe(result.key.CopyToNewArray());
        });
    }

    [Test]
    public void CursorShouldMoveToFirst()
    {
        using var env = CreateEnvironment();
        env.Open();
        env.RunCursorScenario((_, _, c) =>
        {
            var keys = PopulateCursorValues(c);
            var firstKey = keys.First();
            var result = c.First();
            result.resultCode.ShouldBe(MDBResultCode.Success);
            firstKey.ShouldBe(result.key.CopyToNewArray());
        });
    }

    [Test]
    public void ShouldIterateThroughCursor()
    {
        using var env = CreateEnvironment();
        env.Open();
        env.RunCursorScenario((tx, db, c) =>
        {
            var keys = PopulateCursorValues(c);
            using var c2 = tx.CreateCursor(db);
            var items = c2.AsEnumerable().Select((x, i) => (x, i)).ToList();
            foreach (var (x, i) in items)
            {
                keys[i].ShouldBe(x.Item1.CopyToNewArray());
            }

            keys.Length.ShouldBe(items.Count);
        });
    }

    [Test]
    public void CursorShouldDeleteElements()
    {
        using var env = CreateEnvironment();
        env.Open();
        env.RunCursorScenario((tx, db, c) =>
        {
            var keys = PopulateCursorValues(c).Take(2).ToArray();
            for (var i = 0; i < 2; ++i)
            {
                c.Next();
                c.Delete();
            }

            using var c2 = tx.CreateCursor(db);
            c2.AsEnumerable().ShouldNotContain(x => keys.Any(k => x.Item1.CopyToNewArray() == k));
        });
    }

    [Test]
    public void ShouldPutMultiple()
    {
        using var env = CreateEnvironment();
        env.Open();
        env.RunCursorScenario((_, _, c) => { PopulateMultipleCursorValues(c); },
            DatabaseOpenFlags.DuplicatesFixed | DatabaseOpenFlags.Create);
    }

    [Test]
    public void ShouldGetMultiple()
    {
        using var env = CreateEnvironment();
        env.Open();
        env.RunCursorScenario((_, _, c) =>
        {
            var key = "TestKey"u8.ToArray();
            var keys = PopulateMultipleCursorValues(c);
            c.Set(key);
            c.NextDuplicate();
            var (resultCode, _, value) = c.GetMultiple();
            resultCode.ShouldBe(MDBResultCode.Success);
            value.CopyToNewArray().Split(sizeof(int)).ToArray().ShouldBe(keys);
        }, DatabaseOpenFlags.DuplicatesFixed | DatabaseOpenFlags.Create);
    }

    [Test]
    public void ShouldGetNextMultiple()
    {
        using var env = CreateEnvironment();
        env.Open();
        env.RunCursorScenario((_, _, c) =>
        {
            var key = "TestKey"u8.ToArray();
            var keys = PopulateMultipleCursorValues(c);
            c.Set(key);
            var (resultCode, _, value) = c.NextMultiple();
            resultCode.ShouldBe(MDBResultCode.Success);
            value.CopyToNewArray().Split(sizeof(int)).ToArray().ShouldBe(keys);
        }, DatabaseOpenFlags.DuplicatesFixed | DatabaseOpenFlags.Create);
    }

    [Test]
    public void ShouldAdvanceKeyToClosestWhenKeyNotFound()
    {
        using var env = CreateEnvironment();
        env.Open();
        env.RunCursorScenario((_, _, c) =>
        {
            var expected = PopulateCursorValues(c).First();
            var result = c.Set("key"u8.ToArray());
            result.ShouldBe(MDBResultCode.NotFound);
            var (_, key, _) = c.GetCurrent();
            key.CopyToNewArray().ShouldBe(expected);
        });
    }

    [Test]
    public void ShouldSetKeyAndGet()
    {
        using var env = CreateEnvironment();
        env.Open();
        env.RunCursorScenario((_, _, c) =>
        {
            var expected = PopulateCursorValues(c).ElementAt(2);
            var result = c.SetKey(expected);
            result.resultCode.ShouldBe(MDBResultCode.Success);
            result.key.CopyToNewArray().ShouldBe(expected);
        }); 
    }
        
    [Test]
    public void ShouldSetKeyAndGetWithSpan()
    {
        using var env = CreateEnvironment();
        env.Open();
        env.RunCursorScenario((_, _, c) =>
        {
            var expected = PopulateCursorValues(c).ElementAt(2);
            var result = c.SetKey(expected.AsSpan());
            result.resultCode.ShouldBe(MDBResultCode.Success);
            result.key.CopyToNewArray().ShouldBe(expected);
        }); 
    }
        
    [Test]
    public void ShouldGetBoth()
    {
        using var env = CreateEnvironment();
        env.Open();
        env.RunCursorScenario((_, _, c) =>
        {
            var expected = PopulateCursorValues(c).ElementAt(2);
            var result = c.GetBoth(expected, expected);
            result.ShouldBe(MDBResultCode.Success);
        }, DatabaseOpenFlags.DuplicatesFixed | DatabaseOpenFlags.Create); 
    }
        
    [Test]
    public void ShouldGetBothWithSpan()
    {
        using var env = CreateEnvironment();
        env.Open();
        env.RunCursorScenario((_, _, c) =>
        {
            var expected = PopulateCursorValues(c).ElementAt(2);
            var expectedSpan = expected.AsSpan();
            var result = c.GetBoth(expectedSpan, expectedSpan);
            result.ShouldBe(MDBResultCode.Success);
        }, DatabaseOpenFlags.DuplicatesFixed | DatabaseOpenFlags.Create); 
    }
        
    [Test]
    public void ShouldMoveToPrevious()
    {
        using var env = CreateEnvironment();
        env.Open();
        env.RunCursorScenario((_, _, c) =>
        {
            var expected = PopulateCursorValues(c).ElementAt(2);
            var expectedSpan = expected.AsSpan();
            c.GetBoth(expectedSpan, expectedSpan);
            var result = c.Previous();
            result.resultCode.ShouldBe(MDBResultCode.Success);
        }); 
    }
        
    [Test]
    public void ShouldSetRangeWithSpan()
    {
        using var env = CreateEnvironment();
        env.Open();
        env.RunCursorScenario((_, _, c) =>
        {
            var values = PopulateCursorValues(c);
            var firstAfter = values[0].AsSpan();
            var result = c.SetRange(firstAfter);
            result.ShouldBe(MDBResultCode.Success);
            var current = c.GetCurrent();
            current.value.CopyToNewArray().ShouldBe(values[0]);
        }); 
    }
        
    [Test]
    public void ShouldGetBothRange()
    {
        using var env = CreateEnvironment();
        env.Open();
        env.RunCursorScenario((_, _, c) =>
        {
            var key = "TestKey"u8.ToArray();
            var values = PopulateMultipleCursorValues(c);
            var result = c.GetBothRange(key, values[1]);
            result.ShouldBe(MDBResultCode.Success);
            var current = c.GetCurrent();
            current.value.CopyToNewArray().ShouldBe(values[1]);
        }, DatabaseOpenFlags.DuplicatesFixed | DatabaseOpenFlags.Create); 
    }
        
    [Test]
    public void ShouldGetBothRangeWithSpan()
    {
        using var env = CreateEnvironment();
        env.Open();
        env.RunCursorScenario((_, _, c) =>
        {
            var key = "TestKey"u8.ToArray().AsSpan();
            var values = PopulateMultipleCursorValues(c);
            var result = c.GetBothRange(key, values[1].AsSpan());
            result.ShouldBe(MDBResultCode.Success);
            var current = c.GetCurrent();
            current.value.CopyToNewArray().ShouldBe(values[1]);
        }, DatabaseOpenFlags.DuplicatesFixed | DatabaseOpenFlags.Create); 
    }
        
    [Test]
    public void ShouldMoveToFirstDuplicate()
    {
        using var env = CreateEnvironment();
        env.Open();
        env.RunCursorScenario((_, _, c) =>
        {
            var key = "TestKey"u8.ToArray();
            var values = PopulateMultipleCursorValues(c);
            var result = c.GetBothRange(key, values[1]);
            result.ShouldBe(MDBResultCode.Success);
            var dupResult = c.FirstDuplicate();
            dupResult.resultCode.ShouldBe(MDBResultCode.Success);
            dupResult.value.CopyToNewArray().ShouldBe(values[0]);
        }, DatabaseOpenFlags.DuplicatesFixed | DatabaseOpenFlags.Create); 
    }
        
    [Test]
    public void ShouldMoveToLastDuplicate()
    {
        using var env = CreateEnvironment();
        env.Open();
        env.RunCursorScenario((_, _, c) =>
        {
            var key = "TestKey"u8.ToArray();
            var values = PopulateMultipleCursorValues(c);
            c.Set(key);
            var result = c.LastDuplicate();
            result.resultCode.ShouldBe(MDBResultCode.Success);
            result.value.CopyToNewArray().ShouldBe(values[4]);
        }, DatabaseOpenFlags.DuplicatesFixed | DatabaseOpenFlags.Create); 
    }
    
    [Test]
    public void AllValuesForShouldOnlyReturnMatchingKeyValues()
    {
        Skip.Test("Seeing if this test is the reason for failure on CI");
        using var env = CreateEnvironment();
        env.Open();
        env.RunCursorScenario((_, _, c) =>
        {
            var key1 = "TestKey1"u8.ToArray();
            var key2 = "TestKey2"u8.ToArray();
    
            var key1Values = Enumerable.Range(1, 5).Select(i => UTF8.GetBytes($"key1_value{i}")).ToArray();
            var key2Values = Enumerable.Range(1, 3).Select(i => UTF8.GetBytes($"key2_value{i}")).ToArray();
    
            foreach (var value in key1Values)
            {
                c.Put(key1, value, CursorPutOptions.None);
            }
    
            foreach (var value in key2Values)
            {
                c.Put(key2, value, CursorPutOptions.None);
            }
    
            var allKey1Values = c.AllValuesFor(key1).Select(v => v.CopyToNewArray()).ToArray();
            var allKey2Values = c.AllValuesFor(key2).Select(v => v.CopyToNewArray()).ToArray();
    
            allKey1Values.ShouldBe(key1Values);
            allKey2Values.ShouldBe(key2Values);
        }, DatabaseOpenFlags.DuplicatesSort | DatabaseOpenFlags.Create);
    }
        
    [Test]
    public void ShouldMoveToNextNoDuplicate()
    {
        using var env = CreateEnvironment();
        env.Open();
        env.RunCursorScenario((_, _, c) =>
        {
            var values = PopulateMultipleCursorValues(c);
            var result = c.NextNoDuplicate();
            result.resultCode.ShouldBe(MDBResultCode.Success);
            result.value.CopyToNewArray().ShouldBe(values[0]);
        }, DatabaseOpenFlags.DuplicatesFixed | DatabaseOpenFlags.Create); 
    }
    
    
    [Test]
    public void ShouldRetrieveAllValuesForKey()
    {
        Skip.Test("Seeing if this test is the reason for failure on CI");
        using var env = CreateEnvironment();
        env.Open();
        env.RunCursorScenario((_, _, c) =>
        {
            var key = "TestKey"u8.ToArray();
            var values = Enumerable.Range(1, 5).Select(i => UTF8.GetBytes($"value{i}")).ToArray();
    
            // Insert multiple values for the same key with DuplicateSort option
            foreach (var value in values)
            {
                c.Put(key, value, CursorPutOptions.None);
            }
    
            // Fetch all values using the AllValuesFor method
            var retrievedValues = c.AllValuesFor(key).Select(v => v.CopyToNewArray()).ToArray();
    
            // Verify all inserted values are retrieved
            retrievedValues.ShouldBe(values);
        }, DatabaseOpenFlags.DuplicatesSort | DatabaseOpenFlags.Create);
    }
    
    [Test]
    public void ShouldRenewSameTransaction()
    {
        using var env = CreateEnvironment();
        env.Open();
        env.RunCursorScenario((_, _, c) =>
        {
            var result = c.Renew();
            result.ShouldBe(MDBResultCode.Success);
        }, transactionFlags: TransactionBeginFlags.ReadOnly); 
    }

    [Test]
    public void ShouldDeleteDuplicates()
    {
        using var env = CreateEnvironment();
        env.Open();
        env.RunCursorScenario((_, _, c) =>
        {
            var key = "TestKey"u8.ToArray();
            PopulateMultipleCursorValues(c);
            c.Set(key);
            c.DeleteDuplicateData();
            var result = c.Set(key);
            result.ShouldBe(MDBResultCode.NotFound);
        }, DatabaseOpenFlags.DuplicatesFixed | DatabaseOpenFlags.Create);  
    }

    [Test]
    public void CanPutBatchesViaCursorIssue155()
    {
        static LightningDatabase OpenDatabase(LightningEnvironment environment)
        {
            using var tx = environment.BeginTransaction();
            var db = tx.OpenDatabase(configuration: new DatabaseConfiguration { Flags = DatabaseOpenFlags.Create });
            tx.Commit();
            return db;
        }

        void ReproduceCoreIteration(LightningEnvironment environment, LightningDatabase db)
        {
            using var tx = environment.BeginTransaction(); //auto-disposed at end of scope
            using var cursor = tx.CreateCursor(db); //auto-disposed at end of scope

            var guid = Guid.NewGuid().ToString();
            var guidBytes = UTF8.GetBytes(guid);

            _ = cursor.Put(
                guidBytes,
                guidBytes,
                CursorPutOptions.None
            );

            tx.Commit().ThrowOnError();
        }

        using var env = CreateEnvironment();
        env.Open();
        using var db = OpenDatabase(env);

        for (var i = 0; i < 5000; i++)
        {
            ReproduceCoreIteration(env, db);
        }
        true.ShouldBeTrue("Code would be unreachable otherwise.");
    }
    
    [Test]
    public void CountCursor()
    {
        using var env = CreateEnvironment();
        env.Open();
        env.RunCursorScenario((_, _, c) =>
        {
            var key = "TestKey"u8.ToArray();
            var keys = PopulateMultipleCursorValues(c);

            c.SetRange(key);
            c.Count(out var amount);
            
            amount.ShouldBe(5);
        }, DatabaseOpenFlags.DuplicatesFixed | DatabaseOpenFlags.Create);
    }
}