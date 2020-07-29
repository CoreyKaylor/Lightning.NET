using System;
using System.Linq;
using Xunit;
using static System.Text.Encoding;

namespace LightningDB.Tests
{
    [Collection("SharedFileSystem")]
    public class CursorTests : IDisposable
    {
        private readonly LightningEnvironment _env;

        public CursorTests(SharedFileSystem fileSystem)
        {
            var path = fileSystem.CreateNewDirectoryForTest();
            _env = new LightningEnvironment(path);
            _env.Open();
        }

        public void Dispose()
        {
            _env.Dispose();
        }

        private static byte[][] PopulateCursorValues(LightningCursor cursor, int count = 5, string keyPrefix = "key")
        {
            var keys = Enumerable.Range(1, count)
                .Select(i => UTF8.GetBytes(keyPrefix + i))
                .ToArray();

            foreach (var k in keys)
            {
                var result = cursor.Put(k, k, CursorPutOptions.None);
                Assert.Equal(MDBResultCode.Success, result);
            }

            return keys;
        }

        private static byte[][] PopulateMultipleCursorValues(LightningCursor cursor, string key = "TestKey")
        {
            var values = Enumerable.Range(1, 5).Select(BitConverter.GetBytes).ToArray();
            var result = cursor.Put(UTF8.GetBytes(key), values);
            Assert.Equal(MDBResultCode.Success, result);
            var notDuplicate = values[0];
            result = cursor.Put(notDuplicate, notDuplicate, CursorPutOptions.NoDuplicateData);
            Assert.Equal(MDBResultCode.Success, result);
            return values;
        }

        [Fact]
        public void CursorShouldBeCreated()
        {
            _env.RunCursorScenario((tx, db, c) => Assert.NotNull(c));
        }

        [Fact]
        public void CursorShouldPutValues()
        {
            _env.RunCursorScenario((tx, db, c) =>
            {
                PopulateCursorValues(c);
                c.Dispose();
                //TODO evaluate how not to require this Dispose on Linux (test only fails there)
                var result = tx.Commit();
                Assert.Equal(MDBResultCode.Success, result);
            });
        }

        [Fact]
        public void CursorShouldSetSpanKey()
        {
            _env.RunCursorScenario((tx, db, c) =>
            {
                var keys = PopulateCursorValues(c);
                var firstKey = keys.First();
                var result = c.Set(firstKey.AsSpan());
                Assert.Equal(MDBResultCode.Success, result);
                var current = c.GetCurrent();
                Assert.Equal(firstKey, current.key.CopyToNewArray());
            });
        }

        [Fact]
        public void CursorShouldMoveToLast()
        {
            _env.RunCursorScenario((tx, db, c) =>
            {
                var keys = PopulateCursorValues(c);
                var lastKey = keys.Last();
                var result = c.Last();
                Assert.Equal(MDBResultCode.Success, result);
                var current = c.GetCurrent();
                Assert.Equal(lastKey, current.key.CopyToNewArray());
            });
        }

        [Fact]
        public void CursorShouldMoveToFirst()
        {
            _env.RunCursorScenario((tx, db, c) =>
            {
                var keys = PopulateCursorValues(c);
                var firstKey = keys.First();
                var result = c.First();
                Assert.Equal(MDBResultCode.Success, result);
                var current = c.GetCurrent();
                Assert.Equal(firstKey, current.key.CopyToNewArray());
            });
        }

        [Fact]
        public void ShouldIterateThroughCursor()
        {
            _env.RunCursorScenario((tx, db, c) =>
            {
                var keys = PopulateCursorValues(c);
                using var c2 = tx.CreateCursor(db);
                var items = c2.AsEnumerable().Select((x, i) => (x, i)).ToList();
                foreach (var (x, i) in items)
                {
                    Assert.Equal(keys[i], x.Item1.CopyToNewArray());
                }

                Assert.Equal(keys.Length, items.Count);
            });
        }

        [Fact]
        public void CursorShouldDeleteElements()
        {
            _env.RunCursorScenario((tx, db, c) =>
            {
                var keys = PopulateCursorValues(c).Take(2).ToArray();
                for (var i = 0; i < 2; ++i)
                {
                    c.Next();
                    c.Delete();
                }

                using var c2 = tx.CreateCursor(db);
                Assert.DoesNotContain(c2.AsEnumerable(), x =>
                    keys.Any(k => x.Item1.CopyToNewArray() == k));
            });
        }

        [Fact]
        public void ShouldPutMultiple()
        {
            _env.RunCursorScenario((tx, db, c) => { PopulateMultipleCursorValues(c); },
                DatabaseOpenFlags.DuplicatesFixed | DatabaseOpenFlags.Create);
        }

        [Fact]
        public void ShouldGetMultiple()
        {
            _env.RunCursorScenario((tx, db, c) =>
            {
                var key = UTF8.GetBytes("TestKey");
                var keys = PopulateMultipleCursorValues(c);
                c.Set(key);
                c.NextDuplicate();
                var (resultCode, _, value) = c.GetMultiple();
                Assert.Equal(MDBResultCode.Success, resultCode);
                Assert.Equal(keys, value.CopyToNewArray().Split(sizeof(int)).ToArray());
            }, DatabaseOpenFlags.DuplicatesFixed | DatabaseOpenFlags.Create);
        }

        [Fact]
        public void ShouldGetNextMultiple()
        {
            _env.RunCursorScenario((tx, db, c) =>
            {
                var key = UTF8.GetBytes("TestKey");
                var keys = PopulateMultipleCursorValues(c);
                c.Set(key);
                var (resultCode, _, value) = c.NextMultiple();
                Assert.Equal(MDBResultCode.Success, resultCode);
                Assert.Equal(keys, value.CopyToNewArray().Split(sizeof(int)).ToArray());
            }, DatabaseOpenFlags.DuplicatesFixed | DatabaseOpenFlags.Create);
        }

        [Fact]
        public void ShouldAdvanceKeyToClosestWhenKeyNotFound()
        {
            _env.RunCursorScenario((tx, db, c) =>
            {
                var expected = PopulateCursorValues(c).First();
                var result = c.Set(UTF8.GetBytes("key"));
                Assert.Equal(MDBResultCode.NotFound, result);
                var (_, key, _) = c.GetCurrent();
                Assert.Equal(expected, key.CopyToNewArray());
            });
        }

        [Fact]
        public void ShouldSetKeyAndGet()
        {
            _env.RunCursorScenario((tx, db, c) =>
            {
                var expected = PopulateCursorValues(c).ElementAt(2);
                var result = c.SetKey(expected);
                Assert.Equal(MDBResultCode.Success, result.resultCode);
                Assert.Equal(expected, result.key.CopyToNewArray());
            }); 
        }
        
        [Fact]
        public void ShouldSetKeyAndGetWithSpan()
        {
            _env.RunCursorScenario((tx, db, c) =>
            {
                var expected = PopulateCursorValues(c).ElementAt(2);
                var result = c.SetKey(expected.AsSpan());
                Assert.Equal(MDBResultCode.Success, result.resultCode);
                Assert.Equal(expected, result.key.CopyToNewArray());
            }); 
        }
        
        [Fact]
        public void ShouldGetBoth()
        {
            _env.RunCursorScenario((tx, db, c) =>
            {
                var expected = PopulateCursorValues(c).ElementAt(2);
                var result = c.GetBoth(expected, expected);
                Assert.Equal(MDBResultCode.Success, result);
            }, DatabaseOpenFlags.DuplicatesFixed | DatabaseOpenFlags.Create); 
        }
        
        [Fact]
        public void ShouldGetBothWithSpan()
        {
            _env.RunCursorScenario((tx, db, c) =>
            {
                var expected = PopulateCursorValues(c).ElementAt(2);
                var expectedSpan = expected.AsSpan();
                var result = c.GetBoth(expectedSpan, expectedSpan);
                Assert.Equal(MDBResultCode.Success, result);
            }, DatabaseOpenFlags.DuplicatesFixed | DatabaseOpenFlags.Create); 
        }
        
        [Fact]
        public void ShouldMoveToPrevious()
        {
            _env.RunCursorScenario((tx, db, c) =>
            {
                var expected = PopulateCursorValues(c).ElementAt(2);
                var expectedSpan = expected.AsSpan();
                c.GetBoth(expectedSpan, expectedSpan);
                var result = c.Previous();
                Assert.Equal(MDBResultCode.Success, result);
            }); 
        }
        
        [Fact]
        public void ShouldSetRangeWithSpan()
        {
            _env.RunCursorScenario((tx, db, c) =>
            {
                var values = PopulateCursorValues(c);
                var firstAfter = values[0].AsSpan();
                var result = c.SetRange(firstAfter);
                Assert.Equal(MDBResultCode.Success, result);
                var current = c.GetCurrent();
                Assert.Equal(values[0], current.value.CopyToNewArray());
            }); 
        }
        
        [Fact]
        public void ShouldGetBothRange()
        {
            _env.RunCursorScenario((tx, db, c) =>
            {
                var key = UTF8.GetBytes("TestKey");
                var values = PopulateMultipleCursorValues(c);
                var result = c.GetBothRange(key, values[1]);
                Assert.Equal(MDBResultCode.Success, result);
                var current = c.GetCurrent();
                Assert.Equal(values[1], current.value.CopyToNewArray());
            }, DatabaseOpenFlags.DuplicatesFixed | DatabaseOpenFlags.Create); 
        }
        
        [Fact]
        public void ShouldGetBothRangeWithSpan()
        {
            _env.RunCursorScenario((tx, db, c) =>
            {
                var key = UTF8.GetBytes("TestKey").AsSpan();
                var values = PopulateMultipleCursorValues(c);
                var result = c.GetBothRange(key, values[1].AsSpan());
                Assert.Equal(MDBResultCode.Success, result);
                var current = c.GetCurrent();
                Assert.Equal(values[1], current.value.CopyToNewArray());
            }, DatabaseOpenFlags.DuplicatesFixed | DatabaseOpenFlags.Create); 
        }
        
        [Fact]
        public void ShouldMoveToFirstDuplicate()
        {
            _env.RunCursorScenario((tx, db, c) =>
            {
                var key = UTF8.GetBytes("TestKey");
                var values = PopulateMultipleCursorValues(c);
                var result = c.GetBothRange(key, values[1]);
                Assert.Equal(MDBResultCode.Success, result);
                result = c.FirstDuplicate();
                Assert.Equal(MDBResultCode.Success, result);
                var current = c.GetCurrent();
                Assert.Equal(values[0], current.value.CopyToNewArray());
            }, DatabaseOpenFlags.DuplicatesFixed | DatabaseOpenFlags.Create); 
        }
        
        [Fact]
        public void ShouldMoveToLastDuplicate()
        {
            _env.RunCursorScenario((tx, db, c) =>
            {
                var key = UTF8.GetBytes("TestKey");
                var values = PopulateMultipleCursorValues(c);
                c.Set(key);
                var result = c.LastDuplicate();
                Assert.Equal(MDBResultCode.Success, result);
                var current = c.GetCurrent();
                Assert.Equal(values[4], current.value.CopyToNewArray());
            }, DatabaseOpenFlags.DuplicatesFixed | DatabaseOpenFlags.Create); 
        }
        
        [Fact]
        public void ShouldMoveToNextNoDuplicate()
        {
            _env.RunCursorScenario((tx, db, c) =>
            {
                var values = PopulateMultipleCursorValues(c);
                var result = c.NextNoDuplicate();
                Assert.Equal(MDBResultCode.Success, result);
                var current = c.GetCurrent();
                Assert.Equal(values[0], current.value.CopyToNewArray());
            }, DatabaseOpenFlags.DuplicatesFixed | DatabaseOpenFlags.Create); 
        }
        
        [Fact]
        public void ShouldRenewSameTransaction()
        {
            _env.RunCursorScenario((tx, db, c) =>
            {
                var result = c.Renew();
                Assert.Equal(MDBResultCode.Success, result);
            }, transactionFlags: TransactionBeginFlags.ReadOnly); 
        }

        [Fact]
        public void ShouldDeleteDuplicates()
        {
            _env.RunCursorScenario((tx, db, c) =>
            {
                var key = UTF8.GetBytes("TestKey");
                PopulateMultipleCursorValues(c);
                c.Set(key);
                c.DeleteDuplicateData();
                var result = c.Set(key);
                Assert.Equal(MDBResultCode.NotFound, result);
            }, DatabaseOpenFlags.DuplicatesFixed | DatabaseOpenFlags.Create);  
        }
    }
}