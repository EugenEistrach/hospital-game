using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;

/// <summary>
/// Guard clauses for fail-fast precondition validation.
/// Throws in all builds (Debug AND Release) - not compiled out like Debug.Assert.
/// Use at method entry to validate preconditions, keeping code flat and explicit.
/// </summary>
public static class Ensure
{
    /// <summary>
    /// Throws InvalidOperationException if condition is false.
    /// </summary>
    public static void That(
        [DoesNotReturnIf(false)] bool condition,
        string message,
        [CallerFilePath] string file = "",
        [CallerLineNumber] int line = 0)
    {
        if (!condition)
            throw new InvalidOperationException($"{message} (at {Path.GetFileName(file)}:{line})");
    }

    /// <summary>
    /// Throws InvalidOperationException if value is null. Returns non-null value.
    /// </summary>
    public static T NotNull<T>(
        [NotNull] T? value,
        string name,
        [CallerFilePath] string file = "",
        [CallerLineNumber] int line = 0) where T : class
    {
        if (value is null)
            throw new InvalidOperationException($"{name} must not be null (at {Path.GetFileName(file)}:{line})");
        return value;
    }
}
