using System.Runtime.InteropServices;
using Xunit;

namespace LightningDB.Tests;

public class WindowsOnlyFactAttribute : FactAttribute
{
    public WindowsOnlyFactAttribute()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Skip = "Skipped for for non-Windows OS";
        }
    }
}

public class Not32BitFactAttribute : FactAttribute
{
    public Not32BitFactAttribute()
    {
        if (RuntimeInformation.OSArchitecture == Architecture.X86)
        {
            Skip = "Skipping for x86 platform";
        }
    }
}