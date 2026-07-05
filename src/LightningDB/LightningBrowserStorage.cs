#if NET8_0_OR_GREATER
using System;
using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LightningDB;

/// <summary>
/// IndexedDB-backed persistence for browser-wasm environments, built on
/// emscripten's IDBFS file system. Only supported on browser-wasm; the
/// LightningDB package links IDBFS automatically (opt out with the
/// <c>WasmEnableIDBFS=false</c> MSBuild property).
/// </summary>
/// <remarks>
/// <para>
/// The browser file system (MEMFS) is in-memory and wiped on page reload.
/// <see cref="MountAsync(string)"/> mounts a directory backed by IndexedDB and
/// restores its previous contents; <see cref="PersistAsync"/> checkpoints the
/// current contents back to IndexedDB. This gives <b>checkpoint durability</b>:
/// data survives a reload after each successful persist, not after every
/// commit. The intended pattern:
/// </para>
/// <code>
/// await LightningBrowserStorage.MountAsync("/persist"); // BEFORE opening the environment
/// using var env = new LightningEnvironment("/persist/db", ...);
/// env.Open(/* required browser flags */);
/// // ... transactions ...
/// env.Flush(force: true);                        // LMDB -> browser file system
/// await LightningBrowserStorage.PersistAsync();  // browser file system -> IndexedDB
/// </code>
/// <para>
/// The environment must be opened only after <see cref="MountAsync(string)"/>
/// completes: the restore pass makes the mounted directory mirror IndexedDB,
/// deleting any files created under the mount point beforehand.
/// </para>
/// <para>
/// Only one tab may use a mounted store at a time — each tab has a private
/// in-memory copy over the same IndexedDB database (named after the mount
/// point), and the last persist wins wholesale. IndexedDB is best-effort
/// storage; browsers may evict it under pressure unless the origin holds
/// persistent-storage permission (<c>navigator.storage.persist()</c>).
/// <see cref="PersistAsync"/> faults if the browser rejects the write
/// (e.g. quota exceeded).
/// </para>
/// <para>
/// The supporting JavaScript is imported from an embedded <c>data:</c> module
/// URL. Sites with a strict <c>script-src</c> content security policy block
/// <c>data:</c> imports — host the module yourself and assign its URL to
/// <see cref="JavaScriptModuleUrl"/> before the first call; the module source
/// is available via <see cref="JavaScriptModuleSource"/>.
/// </para>
/// </remarks>
[SupportedOSPlatform("browser")]
public static partial class LightningBrowserStorage
{
    private const string ModuleName = "LightningDB.IDBFS";

    private static Task? _import;
    private static readonly SemaphoreSlim SyncGate = new(1, 1);

    /// <summary>
    /// The JavaScript module backing this API. Exposed so strict-CSP
    /// applications can host it themselves (see <see cref="JavaScriptModuleUrl"/>).
    /// </summary>
    public const string JavaScriptModuleSource =
        """
        const FS = globalThis.getDotnetRuntime(0).Module.FS;
        const mounted = new Set();
        export function mount(path) {
          if (mounted.has(path)) return;
          try { FS.mkdir(path); } catch (e) { if (e.errno !== 20 /* EEXIST */) throw e; }
          FS.mount(FS.filesystems.IDBFS, {}, path);
          mounted.add(path);
        }
        export function syncfs(populate) {
          return new Promise((resolve, reject) =>
            FS.syncfs(populate, err => err ? reject(new Error(String(err))) : resolve()));
        }
        """;

    /// <summary>
    /// Optional override for where the supporting JavaScript module is loaded
    /// from. Defaults to an embedded <c>data:</c> URL; set this (before the
    /// first <see cref="MountAsync(string)"/>/<see cref="PersistAsync"/> call)
    /// to a URL serving <see cref="JavaScriptModuleSource"/> when a strict
    /// content security policy blocks <c>data:</c> module imports.
    /// </summary>
    public static string? JavaScriptModuleUrl { get; set; }

    /// <summary>
    /// Mounts <paramref name="mountPoint"/> as an IndexedDB-backed directory and
    /// restores its previous contents. Must complete before any
    /// <see cref="LightningEnvironment"/> under the mount point is opened.
    /// Idempotent: calling again for an already-mounted path performs only the
    /// restore (which overwrites unpersisted in-memory changes with the last
    /// persisted state).
    /// </summary>
    /// <param name="mountPoint">Absolute directory path, e.g. <c>"/persist"</c>.
    /// Also names the origin-scoped IndexedDB database.</param>
    public static async Task MountAsync(string mountPoint)
    {
        if (string.IsNullOrWhiteSpace(mountPoint))
            throw new ArgumentException("A mount point is required", nameof(mountPoint));
        await EnsureModuleAsync().ConfigureAwait(false);
        Mount(mountPoint);
        await GatedSyncFs(populate: true).ConfigureAwait(false);
    }

    /// <summary>
    /// Checkpoints all mounted IndexedDB-backed directories to IndexedDB.
    /// Call after <see cref="LightningEnvironment.Flush"/> with <c>force: true</c>,
    /// which is what moves committed LMDB data into the browser file system.
    /// </summary>
    public static async Task PersistAsync()
    {
        await EnsureModuleAsync().ConfigureAwait(false);
        await GatedSyncFs(populate: false).ConfigureAwait(false);
    }

    private static Task EnsureModuleAsync()
    {
        if (!OperatingSystem.IsBrowser())
            throw new PlatformNotSupportedException(
                "LightningBrowserStorage is only supported on browser-wasm.");
        return _import ??= JSHost.ImportAsync(ModuleName,
            JavaScriptModuleUrl ?? "data:text/javascript;base64," +
            Convert.ToBase64String(Encoding.UTF8.GetBytes(JavaScriptModuleSource)));
    }

    // emscripten warns on (and can misbehave with) concurrent FS.syncfs calls,
    // so all syncs funnel through one gate.
    private static async Task GatedSyncFs(bool populate)
    {
        await SyncGate.WaitAsync().ConfigureAwait(false);
        try
        {
            await SyncFs(populate).ConfigureAwait(false);
        }
        finally
        {
            SyncGate.Release();
        }
    }

    [JSImport("mount", ModuleName)]
    private static partial void Mount(string path);

    [JSImport("syncfs", ModuleName)]
    private static partial Task SyncFs(bool populate);
}
#endif
