# Lightning.NET
![.NET Tests](https://github.com/CoreyKaylor/Lightning.NET/workflows/.NET%20Tests/badge.svg)
[![NuGet version](https://img.shields.io/nuget/v/LightningDB.svg)](https://www.nuget.org/packages/LightningDB/)

Lightning.NET is a .NET library that provides a fast and easy-to-use interface to the Lightning Memory-Mapped Database (LMDB), a high-performance key-value store. This library enables .NET developers to leverage LMDB's efficiency and reliability in their applications.

## Features

- **High Performance**: Direct interaction with LMDB ensures minimal overhead (no copies / 0-alloc when using Span) and maximum speed.
- **Simplicity**: The API is designed to be straightforward, making it easy to integrate into existing projects.
- **Flexibility**: Supports various database configurations, including handling multiple values for the same key.
- **Reliable**: It is fully transactional with complete ACID semantics.

## Installation

Lightning.NET is available as a NuGet package. To install it, run the following command in the Package Manager Console:

```bash
Install-Package LightningDB
```

Alternatively, you can install it via the .NET CLI:

```bash
dotnet add package LightningDB
```

### iOS

The package ships iOS binaries in two forms (minimum iOS 12.0; the arm64
simulator slice requires iOS 14.0):

- **.NET for iOS / MAUI**: works automatically. The dylibs under
  `runtimes/ios-arm64` and `runtimes/iossimulator-*` carry an
  `@rpath/lmdb.dylib` install name and an ad-hoc signature; the .NET iOS SDK
  embeds them in the app bundle and re-signs them with your app identity at
  build time.
- **Unity (and other Xcode-based pipelines)**: use the bundled
  `ios/lmdb.xcframework`. A nupkg is a zip archive, so extract
  `ios/lmdb.xcframework` from it and copy it into `Assets/Plugins/iOS`
  (Unity 2021.3+ imports xcframework plugins directly). In the plugin
  importer, enable the iOS platform and "Add to Embedded Binaries" so Xcode
  embeds and re-signs the framework during the app build — required for
  release/App Store builds. On older Unity versions, use the device slice
  (`ios-arm64/lmdb.framework`) on its own instead.

### Browser (WebAssembly)

LightningDB has experimental support for browser-wasm (Blazor WebAssembly,
Uno Platform). There is no prebuilt binary for the browser — the .NET
WebAssembly runtime statically links native code at app build time — so the
package ships LMDB as compilable C source and wires it up automatically when
your app's `RuntimeIdentifier` is `browser-wasm`. The only prerequisite:

```bash
dotnet workload install wasm-tools
```

The first build performs a one-time native relink (~15 s, cached afterwards).

**Required open flags.** Each is load-bearing on the browser's in-memory file
system (MEMFS); any other configuration is broken in ways verified by the
test suite in `src/LightningDB.WasmTest`:

```csharp
using var env = new LightningEnvironment("/db", new EnvironmentConfiguration { MapSize = 16 * 1024 * 1024 });
env.Open(EnvironmentOpenFlags.WriteMap |               // MEMFS mmaps are copies; without WriteMap,
         EnvironmentOpenFlags.NoLock |                 //   committed data is silently invisible to readers
         EnvironmentOpenFlags.NoThreadLocalStorage |   // no file locking / robust mutexes in the sandbox
         EnvironmentOpenFlags.NoSync);                 // emscripten msync only accepts mapping base
                                                       //   addresses; sync-at-commit would poison the env
// ... transactions as usual ...
env.Flush(force: true); // the durability point (replaces sync-at-commit)
```

**Persistence with IndexedDB.** The browser file system (MEMFS) is in-memory —
by itself it does not survive a page reload. `LightningBrowserStorage` mounts a
directory backed by IndexedDB (emscripten IDBFS; the package links it
automatically, opt out with the `WasmEnableIDBFS=false` MSBuild property):

```csharp
await LightningBrowserStorage.MountAsync("/persist"); // restore from IndexedDB — BEFORE opening
using var env = new LightningEnvironment("/persist/db", new EnvironmentConfiguration { MapSize = 16 * 1024 * 1024 });
env.Open(/* the required flags above */);
// ... transactions ...
env.Flush(force: true);                        // LMDB -> browser file system
await LightningBrowserStorage.PersistAsync();  // browser file system -> IndexedDB
```

- This is **checkpoint durability**: data survives a reload after each
  successful `PersistAsync()` (always preceded by `Flush(true)`), not after
  every commit. IndexedDB writes are asynchronous and explicit by nature.
- The environment must be opened only after `MountAsync` completes — the
  restore pass makes the mounted directory mirror IndexedDB, deleting files
  created under the mount point beforehand.
- One tab at a time: concurrent tabs share the IndexedDB store but not the
  in-memory copy, and the last persist wins wholesale.
- IndexedDB is best-effort storage; browsers may evict it under pressure.
  Consider requesting `navigator.storage.persist()`. `PersistAsync` faults if
  the browser rejects the write (e.g. quota exceeded).
- Strict `script-src` content security policies block the embedded `data:`
  module import; host `LightningBrowserStorage.JavaScriptModuleSource`
  yourself and set `LightningBrowserStorage.JavaScriptModuleUrl`.

**What to expect:**

- Single process, single writer; the page size is 64 KB (emscripten), and the
  address space is 32-bit — keep map sizes modest.
- `AesGcmCipher` and `Sha256Checksum` are unavailable in the browser (no
  platform AEAD, and managed page callbacks can't cross the wasm boundary).
  Use the wasm-only `NativeChaCha20Poly1305Cipher` (RFC 8439, 32-byte key,
  16-byte tag) and `NativeBlake2bChecksum` (keyed BLAKE2b-256 when combined
  with encryption); the crypto is compiled into the native library and runs
  at native speed.
- **Encryption is experimental** with a known limitation: data committed in
  one transaction is not visible to later read transactions in the same
  environment session (encrypted environments bypass WriteMap). The supported
  encrypted pattern is write → `Flush(true)` → dispose → reopen → read; reads
  inside the writing transaction work normally. Unencrypted environments have
  no such limitation.
- Encrypted database files are not portable to 64-bit desktop builds (raw
  LMDB files never are across 32/64-bit).

`src/LightningDB.WasmTest` is a runnable Blazor sample and the verification
suite for all of the above.

## Basic Usage

Here's a simple example demonstrating how to create an environment, open a database, and perform basic put and get operations:

```csharp
using System;
using System.Text;
using LightningDB;

class Program
{
    static void Main()
    {
        // Specify the path to the database environment
        using var env = new LightningEnvironment("path_to_your_database");
        env.Open();

        // Begin a transaction and open (or create) a database
        using (var tx = env.BeginTransaction())
        using (var db = tx.OpenDatabase(configuration: new DatabaseConfiguration { Flags = DatabaseOpenFlags.Create }))
        {
            // Put a key-value pair into the database
            tx.Put(db, UTF8.GetBytes("hello"), UTF8.GetBytes("world"));
            tx.Commit();
        }

        // Begin a read-only transaction to retrieve the value
        using (var tx = env.BeginTransaction(TransactionBeginFlags.ReadOnly))
        using (var db = tx.OpenDatabase())
        {
            var (resultCode, key, value) = tx.Get(db, Encoding.UTF8.GetBytes("hello"));
            if (resultCode == MDBResultCode.Success)
            {
                Console.WriteLine($"{UTF8.GetString(key)}: {UTF8.GetString(value)}");
            }
            else
            {
                Console.WriteLine("Key not found.");
            }
        }
    }
}
```

In this example:

- We create a new LMDB environment at the specified path.
- We open a database within a transaction, inserting the key-value pair ("hello", "world").
- We commit the transaction to save the changes.
- We then start a read-only transaction to retrieve and display the value associated with the key "hello".

## Handling Multiple Values for the Same Key

LMDB supports storing multiple values for a single key when the database is configured with the `Dupsort` flag. Here's how you can work with duplicate keys and use the cursor's `NextDuplicate` function:

```csharp
using System;
using System.Text;
using LightningDB;

class Program
{
    static void Main()
    {
        using var env = new LightningEnvironment("path_to_your_database");
        env.Open();

        // Configure the database to support duplicate keys
        var dbConfig = new DatabaseConfiguration { Flags = DatabaseOpenFlags.Create | DatabaseOpenFlags.DuplicatesSort };

        // Begin a transaction and open the database
        using (var tx = env.BeginTransaction())
        using (var db = tx.OpenDatabase(configuration: dbConfig))
        {
            var key = Encoding.UTF8.GetBytes("fruit");
            var value1 = Encoding.UTF8.GetBytes("apple");
            var value2 = Encoding.UTF8.GetBytes("cherry");
            var value3 = Encoding.UTF8.GetBytes("banana");

            // Insert multiple values for the same key
            tx.Put(db, key, value1);
            tx.Put(db, key, value2);
            tx.Put(db, key, value3);
            tx.Commit();
        }

        // Begin a read-only transaction to retrieve the values
        using (var tx = env.BeginTransaction(TransactionBeginFlags.ReadOnly))
        using (var db = tx.OpenDatabase())
        using (var cursor = tx.CreateCursor(db))
        {
            var key = Encoding.UTF8.GetBytes("fruit");

            // Position the cursor at the first occurrence of the key
            var result = cursor.Set(key);
            if(result == MDBResultCode.Success)
            {
                do
                {
                    var current = cursor.GetCurrent();
                    var currentKey = current.key.AsSpan();
                    var currentValue = current.value.AsSpan();
                    Console.WriteLine($"{UTF8.GetString(currentKey)}: {UTF8.GetString(currentValue)}");
                }
                // Move to the next duplicate value
                while (cursor.NextDuplicate().resultCode == MDBResultCode.Success);
            }
            else
            {
                Console.WriteLine("Key not found.");
            }
            
            //Or even simpler
            var values = cursor.AllValuesFor(key);
            foreach(var value in values)
            {
                Console.WriteLine($"fruit: {Encoding.UTF8.GetString(value.AsSpan())}");
            }
        }
    }
}
```

In this example:

- We configure the database with the `DupSort` flag to allow multiple values for a single key.
- We insert three different values ("apple", "cherry", "banana") under the same key "fruit".
- Using a cursor, we iterate over all values associated with the key "fruit" by moving to the next duplicate entry and see the values retrieved are ordered.
- Then we demonstrate doing the same thing with IEnumerable instead.

## Custom Key Ordering

LightningDB provides built-in, allocation-free comparers for custom key sorting and duplicate ordering. Use them with `CompareWith()` for keys or `FindDuplicatesWith()` for duplicate values:

```csharp
var config = new DatabaseConfiguration
{
    Flags = DatabaseOpenFlags.Create | DatabaseOpenFlags.DuplicatesSort
};

// Sort keys as signed integers (negative values sort before positive)
config.CompareWith(SignedIntegerComparer.Instance);

// Sort duplicate values in reverse order
config.FindDuplicatesWith(ReverseBitwiseComparer.Instance);

using var db = tx.OpenDatabase(configuration: config);
```

**Available comparers in `LightningDB.Comparers`:**

| Comparer | Description |
|----------|-------------|
| `BitwiseComparer` | Lexicographic byte comparison (default LMDB behavior) |
| `ReverseBitwiseComparer` | Lexicographic descending |
| `SignedIntegerComparer` | 4/8-byte signed integers with proper negative ordering |
| `UnsignedIntegerComparer` | 4/8-byte unsigned integers |
| `Utf8StringComparer` | Ordinal UTF-8 string comparison |
| `LengthComparer` | Sort by length first, then content |
| `LengthOnlyComparer` | Sort by length only |
| `HashCodeComparer` | Hash-based comparison for large values |

Reverse variants are available for most comparers (e.g., `ReverseSignedIntegerComparer`).

## LMDB 1.0

Starting with LightningDB 0.23.0, the bundled native library is from the LMDB 1.0 line.

> **⚠️ Data migration required:** the LMDB 1.0 on-disk format is incompatible with 0.9.x.
> Existing environments created by earlier LightningDB versions cannot be opened; migrate
> them once by dumping with the 0.9 `mdb_dump` tool and loading with the 1.0 `mdb_load`
> tool. Named-database (DBI) names are also stored differently (NUL-terminated) in 1.0.

New capabilities exposed by LightningDB:

### Encryption and checksums (opt-in)

Every page can be encrypted and/or checksummed. LMDB itself ships no cipher — the
application supplies one; LightningDB includes hardware-accelerated AES-256-GCM and
SHA-256 implementations out of the box (custom `LightningCipher`/`LightningChecksum`
implementations are also supported, and required on netstandard2.0; on browser-wasm
use `NativeChaCha20Poly1305Cipher`/`NativeBlake2bChecksum` — see
[Browser (WebAssembly)](#browser-webassembly)):

```csharp
var config = new EnvironmentConfiguration
{
    Encryption = new EncryptionConfiguration(new AesGcmCipher(), key), // key: e.g. 32 random bytes
    Checksum = new Sha256Checksum(), // independent of encryption; either can be used alone
};
using var env = new LightningEnvironment("path_to_your_database", config);
env.Open();
```

The same cipher and key must be configured every time the environment is opened.

### Two-phase commit

`tx.Prepare()` performs the first phase of a two-phase commit; follow with `Commit()` or
`Abort()`. If a remote participant fails after a local commit, the last committed
transaction can be undone with `env.RollbackLastTransaction(txId)`.

### Incremental backup

`env.CopyTo(path)` remains the full-backup primitive; `env.IncrementalCopyTo(file, sinceTxnId)`
dumps only pages newer than a previous backup's `env.Info.LastTransactionId`, and
`env.LoadIncrementalFromStream(stream)` applies the dump to a restored copy.

### Other additions

- `EnvironmentConfiguration.PageSize` — set the database page size (power of 2, 512–65536) at creation time.
- `EnvironmentOpenFlags.PreviousSnapshot` — open using the previous snapshot, recovering from a bad last transaction.
- `TransactionBeginFlags.NoSync`/`NoMetaSync` are now honored per-transaction.

## Additional Resources

For more detailed examples and advanced usage, refer to the unit tests in the [Lightning.NET](https://github.com/CoreyKaylor/Lightning.NET) repository. 

The <a href="http://lmdb.tech/doc" target="_blank">Official LMDB API documentation</a>
is also a valuable resource for understanding the underlying database engine.
