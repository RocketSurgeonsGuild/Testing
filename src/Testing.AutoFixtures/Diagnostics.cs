using Microsoft.CodeAnalysis;

namespace Rocket.Surgery.Extensions.Testing.AutoFixtures;

public static class Diagnostics
{
    /// <summary>
    /// Diagnostic for unsupported parameter arrays as constructors.
    /// </summary>
    public static DiagnosticDescriptor AutoFixture0001 = new(
        nameof(AutoFixture0001),
        "params arrays are not currently supported with AutoFixture.",
        "Consider using IEnumerable<T>",
        "Usage",
        DiagnosticSeverity.Info,
        true
    );
}
