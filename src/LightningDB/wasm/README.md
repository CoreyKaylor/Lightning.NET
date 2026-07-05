# Browser-wasm native sources

On browser-wasm, LMDB cannot ship as a prebuilt binary: the .NET WebAssembly
runtime statically links native dependencies into `dotnet.native.wasm` at app
build time (there is no dynamic loading of wasm side modules). This directory
therefore ships LMDB as **compilable C source**. The nupkg's
`buildTransitive/LightningDB.targets` adds `lmdb.c` as a `NativeFileReference`
when `RuntimeIdentifier == browser-wasm`, and the consumer's `wasm-tools`
workload compiles it with its own bundled emscripten — which also removes any
emscripten-version-matching concerns.

## Files

| file | provenance |
|---|---|
| `lmdb.c` | amalgamation entry point. The name is load-bearing: the wasm pinvoke module name is the file basename and must match `DllImport("lmdb")`. |
| `mdb.c`, `midl.c`, `lmdb.h`, `midl.h` | copied from the vendored OpenLDAP LMDB tree at `lmdb/lmdb/libraries/liblmdb/`, pinned to the `LMDB_1.0.1` tag — the same tree all shipped native binaries are built from. |
| `monocypher.c`, `monocypher.h` | [Monocypher](https://monocypher.org) 4.0.2, verbatim. License: CC0 / BSD-2 (see `MONOCYPHER-LICENCE.md`). |
| `lmdb_wasm_crypto.c` | LightningDB shim: ChaCha20-Poly1305 (RFC 8439) page cipher + BLAKE2b-256 checksum, installed natively via `lmdb_setup_encryption` / `lmdb_setup_checksum` (no managed callbacks — mono-wasm cannot marshal delegates as native callbacks). |

## Refreshing the LMDB copies

`lmdb/compile-lmdb-macos.sh` refreshes `mdb.c`/`midl.c`/`lmdb.h`/`midl.h` here
from the pinned tree as part of the native build run. Monocypher and the
shim/amalgamation are maintained manually.

## Licenses

- LMDB sources: OpenLDAP Public License (see `lmdb/lmdb/LICENSE` upstream).
- Monocypher: CC0-1.0 or BSD-2-Clause (`MONOCYPHER-LICENCE.md`).
