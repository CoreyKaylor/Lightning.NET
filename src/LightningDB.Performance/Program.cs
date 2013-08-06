//#define TEST_1
//#define TEST_2
#define TEST_3

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LightningDB;
using LightningDB.Extensions;
using System.IO;
using System.Diagnostics;
using System.Threading;

namespace LightningDB.Performance
{
    class Program
    {
        const int Iterations = 1000000;
        const int Threads = 100;

        static string _dir = Path.GetDirectoryName(typeof(Program).Assembly.Location);
        static ManualResetEvent _threadsEvent;
        static ManualResetEvent _blockEvent;
        static int _finishedCount;

        static void Main(string[] args)
        {
            var sw = new Stopwatch();

#if TEST_1
            var testDbPath1 = Path.Combine(_dir, "TestDb1");
            if (!Directory.Exists(testDbPath1))
                Directory.CreateDirectory(testDbPath1);

            using (var env = new LightningEnvironment(testDbPath1, EnvironmentOpenFlags.None))
            {
                env.MapSize = 100 * Iterations;
                env.Open();
                using (var tran = env.BeginTransaction())
                using(var db = tran.OpenDatabase(null, DatabaseOpenFlags.None))
                {
                    sw.Start();

                    for (int i = 0; i < Iterations; i++)
                        db.Put("Key " + i.ToString(), "Value " + i.ToString());

                    tran.Commit();

                    sw.Stop();

                    Console.WriteLine("Sequential single transaction: {0}", sw.Elapsed);
                }
            }

            try
            {
                Directory.Delete(testDbPath1, true);
            }
            catch { }
#endif
#if TEST_2
            var testDbPath2 = Path.Combine(_dir, "TestDb2");
            if (!Directory.Exists(testDbPath2))
                Directory.CreateDirectory(testDbPath2);

            using (var env = new LightningEnvironment(testDbPath2, EnvironmentOpenFlags.None))
            {
                env.MapSize = 100 * Iterations;
                env.Open();

                sw.Reset();
                sw.Start();

                for (int i = 0; i < Iterations; i++)
                {
                    using (var tran = env.BeginTransaction())
                    using (var db = tran.OpenDatabase(null, DatabaseOpenFlags.None))
                    {
                        db.Put("Key " + i.ToString(), "Value " + i.ToString());

                        tran.Commit();
                    }
                }

                sw.Stop();

                Console.WriteLine("Sequential transaction per put: {0}", sw.Elapsed);
            }

            try
            {
                Directory.Delete(testDbPath2, true);
            }
            catch { }
#endif
#if TEST_3
            var testDbPath3 = Path.Combine(_dir, "TestDb3");
            if (!Directory.Exists(testDbPath3))
                Directory.CreateDirectory(testDbPath3);

            _threadsEvent = new ManualResetEvent(false);
            _blockEvent = new ManualResetEvent(false);

            using (_env = new LightningEnvironment(testDbPath3, EnvironmentOpenFlags.None))
            {
                _env.MapSize = (int)(Int32.MaxValue / 2);
                _env.Open();

                for (int i = 0; i < Threads; i++)
                {
                    new Thread(Test3ThreadWorker)
                        .Start(i);
                }

                _threadsEvent.Set();

                new Thread(Test3ThreadWaiter)
                    .Start();

                sw.Start();
                _blockEvent.WaitOne();

                sw.Stop();
                Console.WriteLine("Parallel transaction per put: {0}", sw.Elapsed);
            }
            try
            {
                Directory.Delete(testDbPath3, true);
            }
            catch { }

#endif
            Console.ReadLine();
        }

        static LightningEnvironment _env;

        static void Test3ThreadWaiter()
        {
            while (true)
            {
                Thread.Sleep(300);
                if (_finishedCount == Threads)
                    break;
            }

            _blockEvent.Set();
        }

        static void Test3ThreadWorker(object state)
        {
            var number = (int)state;
            var testDbPath3 = Path.Combine(_dir, "TestDb3");

            _threadsEvent.WaitOne();


            for (int i = 0; i < Iterations / Threads; i++)
            {
                using (var tran = _env.BeginTransaction())
                using (var db = tran.OpenDatabase(null, DatabaseOpenFlags.None))
                {
                    db.Put("Key " + number.ToString() + "." + i.ToString(), "Value " + i.ToString());

                    tran.Commit();
                }
            }

            Interlocked.Increment(ref _finishedCount);
        }
    }
}
