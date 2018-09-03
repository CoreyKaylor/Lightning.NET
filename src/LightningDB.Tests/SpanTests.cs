using System;
using System.Runtime.InteropServices;
using Xunit;

namespace LightningDB.Tests
{
    [Collection("SharedFileSystem")]
    public class SpanTests : IDisposable
    {
        private LightningEnvironment _env;
        private Random generator;

        public SpanTests(SharedFileSystem fileSystem)
        {
            var path = fileSystem.CreateNewDirectoryForTest();
            _env = new LightningEnvironment(path);
            _env.Open();
            generator = new Random(Guid.NewGuid().GetHashCode());
        }

        public void Dispose()
        {
            _env.Dispose();
        }

        [Fact]
        public void ValueShouldBeReadProperly()
        {
            using (var txn = _env.BeginTransaction())
            using (var db = txn.OpenDatabase(configuration: new DatabaseConfiguration
            {
                Flags = DatabaseOpenFlags.Create
            }))
            {
                var value = generator.Next(int.MinValue, int.MaxValue);
                var key = BitConverter.GetBytes(1);

                txn.Put(db, key, BitConverter.GetBytes(value));
                var span = txn.GetSpan(db, key);
                var intSpan = MemoryMarshal.Cast<byte, int>(span);

                var savedValue = intSpan[0];

                Assert.Equal(savedValue, value);
            }
        }
    }
}