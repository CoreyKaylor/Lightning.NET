using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using static System.Text.Encoding;

namespace LightningDB.Tests
{
    [Collection("SharedFileSystem")]
    public class CursorTests : IDisposable
    {
        private LightningEnvironment _env;
        private LightningTransaction _txn;
        private LightningDatabase _db;

        public CursorTests(SharedFileSystem fileSystem)
        {
            var path = fileSystem.CreateNewDirectoryForTest();
            

            _env = new LightningEnvironment(path);
            _env.Open();

            _txn = _env.BeginTransaction();            
        }

        public void Dispose()
        {
            _txn.Dispose();
            _env.Dispose();
        }

        private void PopulateCursorValues()
        {
            using (var cur = _txn.CreateCursor(_db))
            {
                var keys = Enumerable.Range(1, 5)
                    .Select(i => UTF8.GetBytes("key" + i))
                    .ToArray();

                foreach (var k in keys)
                {
                    cur.Put(k, k, CursorPutOptions.None);
                }
            }
        }

        [Fact]
        public void CursorShouldBeCreated()
        {
            _db = _txn.OpenDatabase(configuration: new DatabaseConfiguration { Flags = DatabaseOpenFlags.Create });
            _txn.CreateCursor(_db).Dispose();
        }

        [Fact]
        public void CursorShouldPutValues()
        {
            _db = _txn.OpenDatabase(configuration: new DatabaseConfiguration { Flags = DatabaseOpenFlags.Create });
            PopulateCursorValues();
        }

        [Fact]
        public void CursorShouldMoveToLast()
        {
            _db = _txn.OpenDatabase(configuration: new DatabaseConfiguration { Flags = DatabaseOpenFlags.Create });
            PopulateCursorValues();

            using (var cur = _txn.CreateCursor(_db))
            {
                Assert.True(cur.MoveToLast());

                var lastKey = UTF8.GetString(cur.Current.Key);
                var lastValue = UTF8.GetString(cur.Current.Value);

                Assert.Equal("key5", lastKey);
                Assert.Equal("key5", lastValue);
            }
        }

        [Fact]
        public void CursorShouldMoveToFirst()
        {
            _db = _txn.OpenDatabase(configuration: new DatabaseConfiguration { Flags = DatabaseOpenFlags.Create });
            PopulateCursorValues();

            using (var cur = _txn.CreateCursor(_db))
            {
                cur.MoveToFirst();

                var lastKey = UTF8.GetString(cur.Current.Key);
                var lastValue = UTF8.GetString(cur.Current.Value);

                Assert.Equal("key1", lastKey);
                Assert.Equal("key1", lastValue);
            }
        }

        [Fact]
        public void ShouldIterateThroughCursor()
        {
            _db = _txn.OpenDatabase(configuration: new DatabaseConfiguration { Flags = DatabaseOpenFlags.Create });
            PopulateCursorValues();
                        
            using (var cur = _txn.CreateCursor(_db))
            {
                var i = 0;

                while (cur.MoveNext())
                {
                    var key = UTF8.GetString(cur.Current.Key);
                    var value = UTF8.GetString(cur.Current.Value);

                    var name = "key" + ++i;

                    Assert.Equal(name, key);
                    Assert.Equal(name, value);
                }                
                Assert.NotEqual(0, i);
            }
        }

        [Fact]
        public void CursorShouldDeleteElements()
        {
            _db = _txn.OpenDatabase(configuration: new DatabaseConfiguration { Flags = DatabaseOpenFlags.Create });
            PopulateCursorValues();

            using (var cursor = _txn.CreateCursor(_db))
            {
                for (int i = 0; i < 2; ++i)
                {
                    cursor.MoveNext();
                    cursor.Delete();
                }
            }
            using(var cursor = _txn.CreateCursor(_db))
            {
                var foundDeleted = cursor.Select(x => UTF8.GetString(x.Key))
                    .Any(x => x == "key1" || x == "key2");
                Assert.False(foundDeleted);
            }
        }

        [Fact]
        public void ShouldIterateThroughCursorByEnumerator()
        {
            _db = _txn.OpenDatabase(configuration: new DatabaseConfiguration { Flags = DatabaseOpenFlags.Create });
            PopulateCursorValues();

            var i = 0;
            using(var cursor = _txn.CreateCursor(_db))
            {
                foreach (var pair in cursor)
                {
                    var name = "key" + ++i;
                    Assert.Equal(name, UTF8.GetString(pair.Key));
                    Assert.Equal(name, UTF8.GetString(pair.Value));
                }
            }
        }

        [Fact]
        public void ShouldPutMultiple()
        {
            _db = _txn.OpenDatabase(configuration: new DatabaseConfiguration { Flags = DatabaseOpenFlags.DuplicatesFixed | DatabaseOpenFlags.Create });

            var values = new[] { 1, 2, 3, 4, 5 }.Select(BitConverter.GetBytes).ToArray();
            using (var cur = _txn.CreateCursor(_db))
                cur.PutMultiple(UTF8.GetBytes("TestKey"), values);

            var pairs = new KeyValuePair<byte[], byte[]>[values.Length];

            using (var cur = _txn.CreateCursor(_db))
            {
                for (var i = 0; i < values.Length; i++)
                {
                    cur.MoveNextDuplicate();
                    pairs[i] = cur.Current;
                }
            }

            Assert.Equal(values, pairs.Select(x => x.Value).ToArray());
        }

        [Fact]
        public void ShouldGetMultiple()
        {
            _db = _txn.OpenDatabase(configuration: new DatabaseConfiguration { Flags = DatabaseOpenFlags.DuplicatesFixed | DatabaseOpenFlags.Create});

            var original = new[] { 1, 2, 3, 4, 5 };
            var originalBytes = original.Select(BitConverter.GetBytes).ToArray();
            using (var cur = _txn.CreateCursor(_db))
                cur.PutMultiple(UTF8.GetBytes("TestKey"), originalBytes);

            using (var cur = _txn.CreateCursor(_db))
            {
                cur.MoveNext();
                cur.GetMultiple();
                var resultPair = cur.Current;
                Assert.Equal("TestKey", UTF8.GetString(resultPair.Key));
                var result = resultPair.Value.Split(sizeof(int))
                    .Select(x => BitConverter.ToInt32(x.ToArray(), 0)).ToArray();
                Assert.Equal(original, result);
            }
        }

        [Fact]
        public void ShouldMoveNextMultiple()
        {
            _db = _txn.OpenDatabase(configuration: new DatabaseConfiguration { Flags = DatabaseOpenFlags.DuplicatesFixed | DatabaseOpenFlags.Create });

            var original = new[] { 1, 2, 3, 4, 5 };
            var originalBytes = original.Select(BitConverter.GetBytes).ToArray();
            using (var cur = _txn.CreateCursor(_db))
                cur.PutMultiple(UTF8.GetBytes("TestKey"), originalBytes);

            using (var cur = _txn.CreateCursor(_db))
            {
                cur.MoveNextMultiple();
                var resultPair = cur.Current;
                Assert.Equal("TestKey", UTF8.GetString(resultPair.Key));
                var result = resultPair.Value.Split(sizeof(int))
                    .Select(x => BitConverter.ToInt32(x.ToArray(), 0)).ToArray();
                Assert.Equal(original, result);
            }
        }
    }
}
