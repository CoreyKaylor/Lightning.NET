Lightning.NET
=============
[![Mono Build Status](https://travis-ci.org/CoreyKaylor/Lightning.NET.svg?branch=dnx)](https://travis-ci.org/CoreyKaylor/Lightning.NET)
[![Windows Build Status](https://ci.appveyor.com/api/projects/status/u0ch8mk5lkb7dv5q/branch/dnx?svg=true)](https://ci.appveyor.com/project/CoreyKaylor/lightning-net)

.NET library for OpenLDAP's LMDB key-value store.

The API is easy to use and extremely fast.

```cs
var env = new LightningEnvironment("pathtofolder");
env.MaxDatabases = 2;
env.Open();

using (var tx = env.BeginTransaction())
using (var db = tx.OpenDatabase("custom", new DatabaseConfiguration { Flags = DatabaseOpenFlags.Create }))
{
	tx.Put(db, Encoding.UTF8.GetBytes("hello"), Encoding.UTF8.GetBytes("world"));
	tx.Commit();
}
using (var tx = env.BeginTransaction(TransactionBeginFlags.ReadOnly))
{
	var db = tx.OpenDatabase("custom");
	var result = tx.Get(db, Encoding.UTF8.GetBytes("hello"));
	Assert.Equal(result, Encoding.UTF8.GetBytes("world"));
}
```

More examples can be found in the unit tests.

[Official LMDB API docs](http://symas.com/mdb/doc/group__mdb.html)

Library is available from NuGet: https://www.nuget.org/packages/LightningDB/

The library is published under MIT license.
