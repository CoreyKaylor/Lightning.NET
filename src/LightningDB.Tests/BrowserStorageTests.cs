using System;
using System.Threading.Tasks;
using Shouldly;

namespace LightningDB.Tests;

public class BrowserStorageTests
{
    public async Task mount_throws_off_browser()
    {
#pragma warning disable CA1416 // intentionally calling browser-only API off-browser
        await Should.ThrowAsync<PlatformNotSupportedException>(
            () => LightningBrowserStorage.MountAsync("/persist"));
#pragma warning restore CA1416
    }

    public async Task persist_throws_off_browser()
    {
#pragma warning disable CA1416
        await Should.ThrowAsync<PlatformNotSupportedException>(
            () => LightningBrowserStorage.PersistAsync());
#pragma warning restore CA1416
    }
}
