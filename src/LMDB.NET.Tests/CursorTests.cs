﻿using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace LMDB.Tests
{
    [TestFixture]
    public class CursorTests
    {
        private string _path;
        private LightningEnvironment _env;
        private LightningTransaction _txn;
        private LightningDatabase _db;

        public CursorTests()
        {
            var location = typeof(EnvironmentTests).Assembly.Location;
            _path = Path.Combine(Path.GetDirectoryName(location), "TestDb");
        }

        [SetUp]
        public void Init()
        {
            Directory.CreateDirectory(_path);

            _env = new LightningEnvironment(_path, EnvironmentOpenFlags.None);
            _env.Open();

            _txn = _env.BeginTransaction();
            _db = _txn.OpenDatabase();
        }

        [TearDown]
        public void Cleanup()
        {
            _txn.Commit();
            _env.Close();

            if (Directory.Exists(_path))
                Directory.Delete(_path, true);
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

        [Test]
        public void CursorShouldBeCreated()
        {
            //arrange
            
            //act

            //assert
            _txn.CreateCursor(_db);
        }

        [Test]
        public void CursorShouldPutValues()
        {
            //arrange
            
            //act

            //assert
            this.PopulateCursorValues();
        }

        [Test]
        public void CursorShouldMoveToLast()
        {
            //arrange
            this.PopulateCursorValues();

            //assert

            using (var cur = _txn.CreateCursor(_db))
            {
                var last = cur.MoveToLast().Value;

                var lastKey = Encoding.UTF8.GetString(last.Key);
                var lastValue = Encoding.UTF8.GetString(last.Value);

                Assert.AreEqual("key5", lastKey);
                Assert.AreEqual("key5", lastValue);
            }
        }

        [Test]
        public void CursorShouldMoveToFirst()
        {
            //arrange
            this.PopulateCursorValues();

            //assert

            using (var cur = _txn.CreateCursor(_db))
            {
                var last = cur.MoveToFirst().Value;

                var lastKey = Encoding.UTF8.GetString(last.Key);
                var lastValue = Encoding.UTF8.GetString(last.Value);

                Assert.AreEqual("key1", lastKey);
                Assert.AreEqual("key1", lastValue);
            }
        }

        [Test]
        public void ShouldIterateThroughCursor()
        {
            //arrange
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

                    Assert.AreEqual(name, key);
                    Assert.AreEqual(name, value);
                }                
            }
        }

        [Test]
        public void ShouldIterateThroughCursorByExtensions()
        {
            //arrange
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

                    Assert.AreEqual(name, pair.Key);
                    Assert.AreEqual(name, pair.Value);
                }
            }
        }

        [Test]
        public void CursorShouldDeleteElements()
        {
            //arrange
            this.PopulateCursorValues();

            using (var cur = _txn.CreateCursor(_db))
            {
                var i = 0;

                //act
                for (var current = cur.MoveToFirstBy(); 
                    current.PairExists && i < 2;
                    current = cur.MoveNextBy(), i++)
                {
                    var key = current.Key<string>();
                    cur.Delete();
                }

                //assert
                for (var current = cur.MoveToFirstBy(); 
                    current.PairExists; 
                    current = cur.MoveNextBy())
                {
                    var key = current.Key<string>();

                    Assert.AreNotEqual("key1", key);
                    Assert.AreNotEqual("key2", key);
                }
            }
        }

        [Test]
        public void ShouldIterateThroughCursorByEnumerator()
        {
            //arrange
            this.PopulateCursorValues();

            var i = 0;
            foreach (var pair in _txn.EnumerateDatabase<string, string>(_db))
            {
                var name = "key" + ++i;

                //assert

                Assert.AreEqual(name, pair.Key);
                Assert.AreEqual(name, pair.Value);
            }
        }
    }
}
