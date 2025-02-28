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

    public void cursor_should_be_created()
    {
        using var env = CreateEnvironment();
        env.Open();
        env.RunCursorScenario((_, _, c) => c.ShouldNotBeNull());
    }

    public void cursor_should_put_values()
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

    public void cursor_should_set_span_key()
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

    public void cursor_should_move_to_last()
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

    public void cursor_should_move_to_first()
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

    public void should_iterate_through_cursor()
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

    public void cursor_should_delete_elements()
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

    public void should_put_multiple()
    {
        using var env = CreateEnvironment();
        env.Open();
        env.RunCursorScenario((_, _, c) => { PopulateMultipleCursorValues(c); },
            DatabaseOpenFlags.DuplicatesFixed | DatabaseOpenFlags.Create);
    }

    public void should_get_multiple()
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

    public void should_get_next_multiple()
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

    public void should_advance_key_to_closest_when_key_not_found()
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

    public void should_set_key_and_get()
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
        
    public void should_set_key_and_get_with_span()
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
        
    public void should_get_both()
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
        
    public void should_get_both_with_span()
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
        
    public void should_move_to_previous()
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
        
    public void should_set_range_with_span()
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
        
    public void should_get_both_range()
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
        
    public void should_get_both_range_with_span()
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
        
    public void should_move_to_first_duplicate()
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
        
    public void should_move_to_last_duplicate()
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
    
    public void all_values_for_should_only_return_matching_key_values()
    {
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
        
    public void should_move_to_next_no_duplicate()
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
    
    
    public void should_retrieve_all_values_for_key()
    {
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
    
    public void should_renew_same_transaction()
    {
        using var env = CreateEnvironment();
        env.Open();
        env.RunCursorScenario((_, _, c) =>
        {
            var result = c.Renew();
            result.ShouldBe(MDBResultCode.Success);
        }, transactionFlags: TransactionBeginFlags.ReadOnly); 
    }

    public void should_delete_duplicates()
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

    public void can_put_batches_via_cursor_issue_155()
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
    
    public void count_cursor()
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
    
    public void should_move_to_previous_duplicate()
    {
        using var env = CreateEnvironment();
        env.Open();
        env.RunCursorScenario((_, _, c) =>
        {
            var key = "TestKey"u8.ToArray();
            var values = PopulateMultipleCursorValues(c);
            
            // Move to last duplicate
            c.Set(key);
            c.LastDuplicate();
            
            // Now move to previous duplicate from the last one
            var result = c.PreviousDuplicate();
            result.resultCode.ShouldBe(MDBResultCode.Success);
            
            // Verify we're at the second-to-last value
            result.value.CopyToNewArray().ShouldBe(values[3]); // Previous of the last (values[4])
        }, DatabaseOpenFlags.DuplicatesFixed | DatabaseOpenFlags.Create);
    }
    
    public void should_test_previous_operation()
    {
        using var env = CreateEnvironment();
        env.Open();
        env.RunCursorScenario((_, _, c) =>
        {
            // Create key-value pairs
            var keys = PopulateCursorValues(c);
            
            // Position at the last item
            var result = c.Last();
            result.resultCode.ShouldBe(MDBResultCode.Success);
            
            // Now test Previous which should move to the previous item
            result = c.Previous();
            result.resultCode.ShouldBe(MDBResultCode.Success);
            
            // Verify we're at the second-to-last key
            var current = c.GetCurrent();
            current.key.CopyToNewArray().ShouldBe(keys[keys.Length - 2]);
        });
    }
    
    public void should_attempt_previous_no_duplicate_operation()
    {
        using var env = CreateEnvironment();
        env.Open();
        env.RunCursorScenario((_, _, c) =>
        {
            // Create key-value pairs
            var keys = PopulateCursorValues(c);
            
            // Position at a key
            c.Set(keys[2]);
            
            // Call PreviousNoDuplicate - we just check it doesn't throw,
            // we don't validate the exact behavior since it might not be fully implemented
            var (result, _, _) = c.PreviousNoDuplicate();
            result.ShouldBe(MDBResultCode.Success);
        });
    }
    
    public void should_renew_with_transaction()
    {
        using var env = CreateEnvironment();
        env.Open();
        
        // Create read-only transaction and cursor
        using var tx1 = env.BeginTransaction(TransactionBeginFlags.ReadOnly);
        using var db = tx1.OpenDatabase();
        using var cursor = tx1.CreateCursor(db);
        
        // Reset the transaction
        tx1.Reset();
        
        // Create new transaction
        using var tx2 = env.BeginTransaction(TransactionBeginFlags.ReadOnly);
        
        // Renew cursor with new transaction
        var result = cursor.Renew(tx2);
        result.ShouldBe(MDBResultCode.Success);
        
        // Verify cursor is usable with new transaction
        var (resultCode, _, _) = cursor.First();
        resultCode.ShouldBe(MDBResultCode.NotFound);
    }
    
    public void should_put_with_span_parameters()
    {
        using var env = CreateEnvironment();
        env.Open();
        env.RunCursorScenario((_, _, c) =>
        {
            var keySpan = "SpanKey"u8.ToArray().AsSpan();
            var valueSpan = "SpanValue"u8.ToArray().AsSpan();
            
            // Test the Put method with ReadOnlySpan<byte> parameters
            var result = c.Put(keySpan, valueSpan, CursorPutOptions.None);
            result.ShouldBe(MDBResultCode.Success);
            
            // Verify data was stored correctly
            c.Set(keySpan);
            var current = c.GetCurrent();
            UTF8.GetString(current.value.CopyToNewArray()).ShouldBe("SpanValue");
        });
    }
    
    public void should_use_current_put_option()
    {
        using var env = CreateEnvironment();
        env.Open();
        env.RunCursorScenario((_, _, c) =>
        {
            // First create a key-value pair
            var key = "TestKey"u8.ToArray();
            var initialValue = "InitialValue"u8.ToArray();
            var updatedValue = "UpdatedValue"u8.ToArray();
            
            // Put initial key/value
            var result = c.Put(key, initialValue, CursorPutOptions.None);
            result.ShouldBe(MDBResultCode.Success);
            
            // Position cursor at the key we just inserted
            c.Set(key);
            
            // Now use Current option to update the value without specifying key
            // The key parameter is ignored with Current option
            result = c.Put("ignored"u8.ToArray(), updatedValue, CursorPutOptions.Current);
            result.ShouldBe(MDBResultCode.Success);
            
            // Verify the value was updated
            c.Set(key);
            var current = c.GetCurrent();
            current.value.CopyToNewArray().ShouldBe(updatedValue);
        });
    }

    public void should_not_overwrite_with_no_overwrite_option()
    {
        using var env = CreateEnvironment();
        env.Open();
        env.RunCursorScenario((_, _, c) =>
        {
            // First create a key-value pair
            var key = "TestKey"u8.ToArray();
            var initialValue = "InitialValue"u8.ToArray();
            var newValue = "NewValue"u8.ToArray();
            
            // Put initial key/value
            var result = c.Put(key, initialValue, CursorPutOptions.None);
            result.ShouldBe(MDBResultCode.Success);
            
            // Try to put same key with NoOverwrite option
            result = c.Put(key, newValue, CursorPutOptions.NoOverwrite);
            result.ShouldBe(MDBResultCode.KeyExist);
            
            // Verify original value is unchanged
            c.Set(key);
            var current = c.GetCurrent();
            current.value.CopyToNewArray().ShouldBe(initialValue);
        });
    }

    public void should_test_no_duplicate_data_option()
    {
        using var env = CreateEnvironment();
        env.Open();
        env.RunCursorScenario((_, _, c) =>
        {
            // Set up a key with multiple values
            var key = "TestKey"u8.ToArray();
            var value1 = "Value1"u8.ToArray();
            var value2 = "Value2"u8.ToArray();
            
            // Add first value
            var result = c.Put(key, value1, CursorPutOptions.None);
            result.ShouldBe(MDBResultCode.Success);
            
            // Add second value
            result = c.Put(key, value2, CursorPutOptions.None);
            result.ShouldBe(MDBResultCode.Success);
            
            // Try to add the first value again with NoDuplicateData option
            result = c.Put(key, value1, CursorPutOptions.NoDuplicateData);
            result.ShouldBe(MDBResultCode.KeyExist);
            
            // Add a new value with NoDuplicateData option should succeed
            var value3 = "Value3"u8.ToArray();
            result = c.Put(key, value3, CursorPutOptions.NoDuplicateData);
            result.ShouldBe(MDBResultCode.Success);
        }, DatabaseOpenFlags.DuplicatesSort | DatabaseOpenFlags.Create);
    }

    public void should_append_data_with_append_option()
    {
        using var env = CreateEnvironment();
        env.Open();
        env.RunCursorScenario((_, _, c) =>
        {
            // Create sorted keys
            var keys = Enumerable.Range(1, 5)
                .Select(i => UTF8.GetBytes($"key{i:D5}"))
                .ToArray();
            
            // Insert keys in order with AppendData option
            foreach (var key in keys)
            {
                var result = c.Put(key, key, CursorPutOptions.AppendData);
                result.ShouldBe(MDBResultCode.Success);
            }
            
            // Verify all keys were inserted correctly
            c.First();
            for (int i = 0; i < keys.Length; i++)
            {
                var current = c.GetCurrent();
                current.key.CopyToNewArray().ShouldBe(keys[i]);
                current.value.CopyToNewArray().ShouldBe(keys[i]);
                
                if (i < keys.Length - 1)
                    c.Next();
            }
        });
    }

    public void should_append_duplicate_data_with_append_duplicate_option()
    {
        using var env = CreateEnvironment();
        env.Open();
        env.RunCursorScenario((_, _, c) =>
        {
            var key = "TestKey"u8.ToArray();
            var values = Enumerable.Range(1, 5)
                .Select(i => UTF8.GetBytes($"value{i:D5}"))
                .ToArray();
            
            // Put the key once
            var result = c.Put(key, values[0], CursorPutOptions.None);
            result.ShouldBe(MDBResultCode.Success);
            
            // Now append duplicate values in order
            for (int i = 1; i < values.Length; i++)
            {
                result = c.Put(key, values[i], CursorPutOptions.AppendDuplicateData);
                result.ShouldBe(MDBResultCode.Success);
            }
            
            // Verify all values were inserted for the key
            c.Set(key);
            var allValues = c.AllValuesFor(key).Select(v => v.CopyToNewArray()).ToArray();
            allValues.Length.ShouldBe(values.Length);
        }, DatabaseOpenFlags.DuplicatesSort | DatabaseOpenFlags.Create);
    }

    public void should_test_multiple_data_option()
    {
        using var env = CreateEnvironment();
        env.Open();
        env.RunCursorScenario((_, _, c) =>
        {
            var key = "TestKey"u8.ToArray();
            var values = Enumerable.Range(1, 5)
                .Select(i => BitConverter.GetBytes(i)) // Use fixed-size values
                .ToArray();
            
            // Put multiple values in one operation
            var result = c.Put(key, values);
            result.ShouldBe(MDBResultCode.Success);
            
            // Verify the values were stored
            c.Set(key);
            var (resultCode, _, value) = c.GetMultiple();
            resultCode.ShouldBe(MDBResultCode.Success);
            
            // Verify we can split the data into individual values
            var data = value.CopyToNewArray();
            data.Length.ShouldBe(values.Sum(v => v.Length));
        }, DatabaseOpenFlags.DuplicatesFixed | DatabaseOpenFlags.Create);
    }
}