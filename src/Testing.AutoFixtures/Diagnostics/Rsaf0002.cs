using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Rocket.Surgery.Extensions.Testing.AutoFixtures.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class Rsaf0002 : DiagnosticAnalyzer
{
    /// <summary>
    ///     Diagnostic for unsupported parameter arrays as constructors.
    /// </summary>
    public static DiagnosticDescriptor Descriptor = new(
        nameof(Rsaf0002),
        "params arrays are not currently supported with AutoFixture.",
        "Consider using IEnumerable<T>",
        "Support",
        DiagnosticSeverity.Info,
        true
    );

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }
}
