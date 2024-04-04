using Microsoft.CodeAnalysis;

namespace Rocket.Surgery.Extensions.Testing.AutoFixtures;

/// <summary>
/// Represents the supported <see cref="DiagnosticDescriptor"/>.
/// </summary>
public static class Diagnostics
{
    /// <summary>
    ///     Diagnostic for unsupported classes with no constructors.
    /// </summary>
    public static DiagnosticDescriptor AutoFixture0001 = new(
        nameof(AutoFixture0001),
        "classes without constructors are currently not supported.",
        "",
        "Support",
        DiagnosticSeverity.Info,
        true
    );
    /// <summary>
    ///     Diagnostic for unsupported parameter arrays as constructors.
    /// </summary>
    public static DiagnosticDescriptor AutoFixture0002 = new(
        nameof(AutoFixture0002),
        "params arrays are not currently supported with AutoFixture.",
        "Consider using IEnumerable<T>",
        "Support",
        DiagnosticSeverity.Info,
        true
    );
}
