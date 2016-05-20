Lightning.NET
=============

[![Join the chat at https://gitter.im/CoreyKaylor/Lightning.NET](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/CoreyKaylor/Lightning.NET?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)
[![Linux Build Status](https://travis-ci.org/CoreyKaylor/Lightning.NET.svg?branch=master)](https://travis-ci.org/CoreyKaylor/Lightning.NET)
[![Windows Build status](https://ci.appveyor.com/api/projects/status/u0ch8mk5lkb7dv5q?svg=true)](https://ci.appveyor.com/project/CoreyKaylor/lightning-net)

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
