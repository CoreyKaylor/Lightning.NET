Lightning.NET
=============

[![Actions Status](https://github.com/CoreyKaylor/Lightning.NET/workflows/unit-tests/badge.svg)](https://github.com/CoreyKaylor/Lightning.NET/actions)

.NET library for OpenLDAP's LMDB key-value store.

The API is easy to use and extremely fast.

```cs
using (var env = new LightningEnvironment("pathtofolder"))
{
	env.MaxDatabases = 2;
	env.Open();

	using (var tx = env.BeginTransaction())
	using (var db = tx.OpenDatabase("custom", new DatabaseConfiguration { Flags = DatabaseOpenFlags.Create }))
	{
		tx.Put(db, Encoding.UTF8.GetBytes("hello"), Encoding.UTF8.GetBytes("world"));
		tx.Commit();
	}
	using (var tx = env.BeginTransaction(TransactionBeginFlags.ReadOnly))
	using (var db = tx.OpenDatabase("custom"))
	{
		var result = tx.Get(db, Encoding.UTF8.GetBytes("hello"));
		Assert.Equal(result, Encoding.UTF8.GetBytes("world"));
	}
}
```

More examples can be found in the unit tests.

<a href="http://lmdb.tech/doc" target="_blank">Official LMDB API docs</a>

Library is available from NuGet: https://www.nuget.org/packages/LightningDB/

The library is published under MIT license.
