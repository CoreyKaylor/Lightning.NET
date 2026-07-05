# LightningDB.WasmTest

A Blazor WebAssembly app that verifies LightningDB's browser-wasm support
end-to-end. It doubles as the sample for using LMDB in the browser.

This project is intentionally **not** in the solution: building it requires
the `wasm-tools` workload, which contributors working on the core library
shouldn't need.

## Running

```sh
dotnet workload install wasm-tools   # one-time
dotnet run --project src/LightningDB.WasmTest
```

Open the printed localhost URL. The page runs the test suite automatically and
renders one `PASS`/`FAIL` line per test plus a final marker:

```
DONE: 13 passed, 0 failed
```

Headless (what CI does):

```sh
google-chrome --headless=new --disable-gpu --dump-dom \
  --virtual-time-budget=90000 http://localhost:5203/ | grep 'DONE:'
```

## What the tests prove

| test | proves |
|---|---|
| 0 | pinvoke table generated; native LMDB linked (version reported) |
| a, b | put/get and read-after-commit coherence under the required flag set |
| c | cursor iteration (1000 ordered entries) |
| d | `MDB_MAP_FULL` surfaces and `MapSize` growth works post-open |
| e | `Flush(true)` + dispose + reopen preserves data within the page session |
| f (control) | default mode (no `WriteMap`) silently loses commits on MEMFS — this test PASSES when the failure occurs as documented |
| g | delete + stats (also shows the 64 KB emscripten page size) |
| h (control) | sync-at-commit (no `NoSync`) fails — emscripten's `msync` only accepts mapping base addresses; LMDB's alternating meta page hits this |
| i | encryption (`NativeChaCha20Poly1305Cipher`): 50 entries + a 100 KB overflow value roundtrip through reopen-with-key |
| i2 (control) | encrypted envs: committed data invisible to later read txns in the same env session (REMAP_CHUNKS + stale MEMFS meta mapping) — PASSES when the documented limitation reproduces |
| j | wrong key is rejected (`MDB_CRYPTO_FAIL`) |
| k | encryption + keyed `NativeBlake2bChecksum` roundtrip |
| l | checksum-only roundtrip |
| m | IndexedDB persistence: write + `Flush(true)` + `PersistAsync`, wipe the MEMFS copy, restore via `MountAsync`, read back |

## Cross-reload persistence test

`?phase=write` mounts `/persist`, writes 25 entries, flushes and persists to
IndexedDB, then renders/titles `PHASE-WRITE-OK`. `?phase=verify` mounts
(restoring from IndexedDB) and reads them back (`PHASE-VERIFY-OK`). Run the two
phases in separate browser processes sharing a profile to prove data survives a
full cold start:

```sh
PROFILE=$(mktemp -d)
google-chrome --headless=new --user-data-dir=$PROFILE --remote-debugging-port=9222 'http://localhost:5203/?phase=write' &
# poll curl http://localhost:9222/json/list until the page title is PHASE-WRITE-OK, kill chrome, then repeat with ?phase=verify
```

Note for harnesses: the page publishes its final result in `document.title`
because Chrome's `--virtual-time-budget`/`--dump-dom` stalls on IndexedDB —
poll the DevTools `/json/list` endpoint instead (see `.github/workflows/wasm.yml`).
Persistence is **checkpoint durability**: it survives reload after each
successful `Flush(true)` + `PersistAsync()` pair, not after every commit.

## Required environment flags on browser-wasm

```csharp
env.Open(EnvironmentOpenFlags.WriteMap |
         EnvironmentOpenFlags.NoLock |
         EnvironmentOpenFlags.NoThreadLocalStorage |
         EnvironmentOpenFlags.NoSync);
// durability point:
env.Flush(force: true);
```

Every flag is load-bearing; see the "Browser (WebAssembly)" section of the
main README for the reasons and the storage/persistence caveats.

## How the native library gets here

`LightningDB.WasmTest.csproj` adds `../LightningDB/wasm/lmdb.c` as a
`NativeFileReference`; the wasm-tools workload compiles it with its bundled
emscripten and statically links it into `dotnet.native.wasm`. NuGet consumers
get the same wiring automatically from the package's
`buildTransitive/LightningDB.targets` when `RuntimeIdentifier` is
`browser-wasm` — the direct reference here exists only because buildTransitive
targets don't apply to ProjectReferences.
