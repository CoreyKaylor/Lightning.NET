/* Amalgamation for browser-wasm builds.
 *
 * The file name ("lmdb") is load-bearing: the .NET wasm build registers one
 * pinvoke module per NativeFileReference file basename, and it must match
 * the managed DllImport("lmdb") module name for the pinvoke table to be
 * generated.
 *
 * The defines live here (not in EmccExtraCFlags) because NativeFileReference
 * C sources are compiled during the emcc link step, which does not receive
 * the extra compile flags.
 *
 * - MDB_USE_POSIX_MUTEX / MDB_USE_ROBUST=0: emscripten's libc has no robust
 *   mutex support; the lock table is bypassed at runtime anyway (browser
 *   environments must be opened with MDB_NOLOCK).
 * - module.c is intentionally omitted: it only exports mdb_modload (never
 *   P/Invoked) and pulls in dlfcn, which wasm static linking cannot honor.
 */
#define MDB_USE_POSIX_MUTEX 1
#define MDB_USE_ROBUST 0

#include "mdb.c"
#include "midl.c"

/* mdb.c defines Z as a printf format modifier; it would clobber monocypher's
 * ge struct fields. mdb.c's own uses are already expanded at this point. */
#undef Z

#include "monocypher.c"
#include "lmdb_wasm_crypto.c"
