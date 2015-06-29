using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

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
            _env.MaxDatabases = 10;
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
                    .Select(i => Encoding.UTF8.GetBytes("key" + i))
                    .ToArray();

                //act
                foreach (var k in keys)
                {
                    cur.Put(k, k, CursorPutOptions.None);
                }
            }
        }

        [Fact]
        public void CursorShouldBeCreated()
        {
            //arrange
            _db = _txn.OpenDatabase("master", options: new DatabaseOptions { Flags = DatabaseOpenFlags.Create });
            
            //act

            //assert
            _txn.CreateCursor(_db);
        }

        [Fact]
        public void CursorShouldPutValues()
        {
            //arrange
            _db = _txn.OpenDatabase("master", options: new DatabaseOptions { Flags = DatabaseOpenFlags.Create });
            
            //act

            //assert
            this.PopulateCursorValues();
        }

        [Fact]
        public void CursorShouldMoveToLast()
        {
            //arrange
            _db = _txn.OpenDatabase("master", options: new DatabaseOptions { Flags = DatabaseOpenFlags.Create });
            this.PopulateCursorValues();

            //assert

            using (var cur = _txn.CreateCursor(_db))
            {
                var last = cur.MoveToLast().Value;

                var lastKey = Encoding.UTF8.GetString(last.Key);
                var lastValue = Encoding.UTF8.GetString(last.Value);

                Assert.Equal("key5", lastKey);
                Assert.Equal("key5", lastValue);
            }
        }

        [Fact]
        public void CursorShouldMoveToFirst()
        {
            //arrange
            _db = _txn.OpenDatabase("master", options: new DatabaseOptions { Flags = DatabaseOpenFlags.Create });
            this.PopulateCursorValues();

            //assert

            using (var cur = _txn.CreateCursor(_db))
            {
                var last = cur.MoveToFirst().Value;

                var lastKey = Encoding.UTF8.GetString(last.Key);
                var lastValue = Encoding.UTF8.GetString(last.Value);

                Assert.Equal("key1", lastKey);
                Assert.Equal("key1", lastValue);
            }
        }

        [Fact]
        public void ShouldIterateThroughCursor()
        {
            //arrange
            _db = _txn.OpenDatabase("master", options: new DatabaseOptions { Flags = DatabaseOpenFlags.Create });
            this.PopulateCursorValues();
                        
            using (var cur = _txn.CreateCursor(_db))
            {
                var i = 0;

                //act
                while (true)
                {
                    var current = cur.MoveNext();
                    if (!current.HasValue)
                        break;

                    var key = Encoding.UTF8.GetString(current.Value.Key);
                    var value = Encoding.UTF8.GetString(current.Value.Value);

                    var name = "key" + ++i;

                    //assert

                    Assert.Equal(name, key);
                    Assert.Equal(name, value);
                }                
            }
        }

        [Fact]
        public void ShouldIterateThroughCursorByExtensions()
        {
            //arrange
            _db = _txn.OpenDatabase("master", options: new DatabaseOptions { Flags = DatabaseOpenFlags.Create });
            this.PopulateCursorValues();

            using (var cur = _txn.CreateCursor(_db))
            {
                var i = 0;

                //act
                for(var current = cur.MoveToFirstBy(); current.PairExists; current = cur.MoveNextBy())
                {
                    var pair = current.Pair<string, string>();
                    var name = "key" + ++i;

                    //assert

                    Assert.Equal(name, pair.Key);
                    Assert.Equal(name, pair.Value);
                }
            }
        }

        [Fact]
        public void CursorShouldDeleteElements()
        {
            //arrange
            _db = _txn.OpenDatabase("master", options: new DatabaseOptions { Flags = DatabaseOpenFlags.Create });
            this.PopulateCursorValues();

            using (var cur = _txn.CreateCursor(_db))
            {
                var i = 0;

                //act
                for (var current = cur.MoveToFirstBy(); current.PairExists && i < 2; current = cur.MoveNextBy(), i++)
                {
                    cur.Delete();
                }

                //assert
                for (var current = cur.MoveToFirstBy(); current.PairExists; current = cur.MoveNextBy())
                {
                    var key = current.Key<string>();

                    Assert.NotEqual("key1", key);
                    Assert.NotEqual("key2", key);
                }
            }
        }

        [Fact]
        public void ShouldIterateThroughCursorByEnumerator()
        {
            //arrange
            _db = _txn.OpenDatabase("master", options: new DatabaseOptions { Flags = DatabaseOpenFlags.Create });
            this.PopulateCursorValues();

            var i = 0;
            foreach (var pair in _txn.EnumerateDatabase<string, string>(_db))
            {
                var name = "key" + ++i;

                //assert

                Assert.Equal(name, pair.Key);
                Assert.Equal(name, pair.Value);
            }
        }

        [Fact]
        public void ShouldPutMultiple()
        {
            //arrange
            _db = _txn.OpenDatabase("master", options: new DatabaseOptions { Flags = DatabaseOpenFlags.DuplicatesFixed | DatabaseOpenFlags.Create });

            var values = new[] { 1, 2, 3, 4, 5 };
            using (var cur = _txn.CreateCursor(_db))
                cur.PutMultiple("TestKey", values);

            var pairs = new KeyValuePair<string, int>[values.Length];

            //act
            using (var cur = _txn.CreateCursor(_db))
            {
                for (var i = 0; i < values.Length; i++)
                    cur.MoveNextDuplicate<string, int>(out pairs[i]);
            }

            //assert
            for (var i = 0; i < values.Length; i++)
                Assert.Equal(values[i], pairs[i].Value);
        }

        [Fact]
        public void ShouldGetMultiple()
        {
            //arrange
            _db = _txn.OpenDatabase("master", options: new DatabaseOptions { Flags = DatabaseOpenFlags.DuplicatesFixed | DatabaseOpenFlags.Create});

            var values = new[] { 1, 2, 3, 4, 5 };
            using (var cur = _txn.CreateCursor(_db))
                cur.PutMultiple("TestKey", values);

            bool result;
            int[] resultArray;
            using (var cur = _txn.CreateCursor(_db))
            {
                KeyValuePair<string, int> pair;
                cur.MoveNext(out pair);

                result = cur.GetMultiple(out resultArray);
            }

            Assert.True(result);
            Assert.Equal(values, resultArray);
        }

        [Fact]
        public void ShouldMoveNextMultiple()
        {
            //arrange
            _db = _txn.OpenDatabase("master", options: new DatabaseOptions { Flags = DatabaseOpenFlags.DuplicatesFixed | DatabaseOpenFlags.Create});
            const string key = "TestKey";

            var values = new[] { 1, 2, 3, 4, 5 };
            using (var cur = _txn.CreateCursor(_db))
                cur.PutMultiple(key, values);

            bool result;
            KeyValuePair<string, int[]> resultPair;
            using (var cur = _txn.CreateCursor(_db))
                result = cur.MoveNextMultiple(out resultPair);

            Assert.True(result);
            Assert.Equal(key, resultPair.Key);
            Assert.Equal(values, resultPair.Value);
        }
    }
}
