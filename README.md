Lightning.NET
=============

![.NET Core Tests](https://github.com/CoreyKaylor/Lightning.NET/workflows/.NET%20Core%20Tests/badge.svg)

.NET library for OpenLDAP's LMDB key-value store.

The API is easy to use and extremely fast.

```cs
using System.Text;
using LightningDB;

using var env = new LightningEnvironment("pathtofolder");
env.Open();

using (var tx = env.BeginTransaction())
using (var db = tx.OpenDatabase(configuration: new DatabaseConfiguration { Flags = DatabaseOpenFlags.Create }))
{
    tx.Put(db, "hello"u8.ToArray(), "world"u8.ToArray());
    tx.Commit();
}
using (var tx = env.BeginTransaction(TransactionBeginFlags.ReadOnly))
using (var db = tx.OpenDatabase())
{
    var (resultCode, key, value) = tx.Get(db, "hello"u8.ToArray());
    Console.WriteLine($"{Encoding.UTF8.GetString(key.AsSpan())} {Encoding.UTF8.GetString(value.AsSpan())}");
}
```

More examples can be found in the unit tests.

> Note:
> Do not call OpenDatabase from multiple threads concurrently!

<a href="http://lmdb.tech/doc" target="_blank">Official LMDB API docs</a>

Library is available from NuGet: https://www.nuget.org/packages/LightningDB/
