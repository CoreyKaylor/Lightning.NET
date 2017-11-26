#if NET451
using System;
using System.Text;
using Xunit;

namespace LightningDB.Tests
{
    [Collection("SharedFileSystem")]
    public class MultiProcessTests : IDisposable
    {
        private readonly LightningEnvironment _env;

        public MultiProcessTests(SharedFileSystem fileSystem)
        {
            var path = fileSystem.CreateNewDirectoryForTest();
            _env = new LightningEnvironment(path);
            _env.Open();
        }

        [Fact(Skip = "Not really multi-process, reinvestigate simple way to test this")]
        public void can_load_environment_from_multiple_processes()
        {
            var secondDomain = AppDomain.CreateDomain("2nd Process");

            var secondAppType = typeof(SecondApp);
            var app = secondDomain.CreateInstanceAndUnwrap(
                                     secondAppType.Assembly.FullName,
                                     secondAppType.FullName) as SecondApp;

            app.RemotePut(_env.Path);

            using (var tx = _env.BeginTransaction())
            using (var db = tx.OpenDatabase())
            {
                var bytes = tx.Get(db, Encoding.UTF8.GetBytes("hello"));
                Assert.Equal("world", Encoding.UTF8.GetString(bytes));
            }
        }

        public void Dispose()
        {
            _env.Dispose();
        }
    }

    public class SecondApp : MarshalByRefObject
    {
        public void RemotePut(string environment)
        {
            var env = new LightningEnvironment(environment);
            env.Open();
            using (env)
            using(var tx = env.BeginTransaction())
            using(var db = tx.OpenDatabase())
            {
                tx.Put(db, Encoding.UTF8.GetBytes("hello"), Encoding.UTF8.GetBytes("world"));
                tx.Commit();
            }
        }
    }
}
#endif