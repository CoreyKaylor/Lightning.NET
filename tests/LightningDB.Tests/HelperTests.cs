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
    }
}