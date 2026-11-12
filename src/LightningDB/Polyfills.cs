#if NETSTANDARD2_0
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace LightningDB;

/// <summary>
/// C# 14 static extension members that backfill guard-clause APIs missing from netstandard2.0,
/// so call sites can be written identically across all target frameworks.
/// </summary>
internal static class Polyfills
{
    extension(ArgumentNullException)
    {
        public static void ThrowIfNull([NotNull] object? argument,
            [CallerArgumentExpression(nameof(argument))] string? paramName = null)
        {
            if (argument is null)
                Throw(paramName);
        }
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Throw(string? paramName) => throw new ArgumentNullException(paramName);
}
#endif
