using System;
using System.Diagnostics;
using Xunit;
using System.Text;

namespace LightningDB.Tests
{
    public class HelperTests
    {
        [Fact]
        public void ByteArrayStartsWith()
        {
            var message = "Hello World!";
            var hello = "Hello";
            var messageBytes = Encoding.UTF8.GetBytes(message);
            var helloBytes = Encoding.UTF8.GetBytes(hello);
            Assert.True(messageBytes.StartsWith(helloBytes));
        }

        [Fact]
        public void ByteArrayDoesntStartWith()
        {
            var message = "Hello World!";
            var hello = "hello";
            var messageBytes = Encoding.UTF8.GetBytes(message);
            var heloBytes = Encoding.UTF8.GetBytes(hello);
            Assert.False(messageBytes.StartsWith(heloBytes));
        }

        [Fact]
        public void ByteArrayContains()
        {
            var message = "Hello World!";
            var world = "World!";
            var messageBytes = Encoding.UTF8.GetBytes(message);
            var worldBytes = Encoding.UTF8.GetBytes(world);
            Assert.False(messageBytes.StartsWith(worldBytes));
        }

        [Fact]
        public void SimpleStartsWithIsFasterThanStringStartsWithAndConversion()
        {
            var idString = $"{Guid.NewGuid()}";
            var keyString = $"{idString}/someotherinformation";
            var keyBytes = Encoding.UTF8.GetBytes(keyString);
            var idBytes = Encoding.UTF8.GetBytes(idString);
            var stopwatch = Stopwatch.StartNew();
            for (var i = 0; i < 100; ++i)
            {
                var keyAfterConversion = Encoding.UTF8.GetString(keyBytes);
                var result = keyAfterConversion.StartsWith(idString);
            }
            stopwatch.Stop();
            var totalForStringConversion = stopwatch.Elapsed.TotalMilliseconds;
            stopwatch.Restart();
            for (var i = 0; i < 100; ++i)
            {
                var result = keyBytes.StartsWith(idBytes);
            }
            stopwatch.Stop();
            var totalForSimple = stopwatch.Elapsed.TotalMilliseconds;
            Assert.True(totalForSimple < totalForStringConversion);
        }
    }
}