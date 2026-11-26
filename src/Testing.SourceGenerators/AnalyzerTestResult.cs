using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Rocket.Surgery.Extensions.Testing.SourceGenerators;

/// <summary>
///     The results of a specific analyzers execution
/// </summary>
/// <param name="Diagnostics">The resulting diagnostics of the analyzer</param>
[PublicAPI]
public record AnalyzerTestResult
(
    ImmutableArray<Diagnostic> Diagnostics
)
{
    /// <summary>
    ///     Ensure that the diagnostics are less than the specified severity within the generator results
    /// </summary>
    /// <param name="severity"></param>
    /// <exception cref="InvalidOperationException">Lists every diagnostic at or above <paramref name="severity" />.</exception>
    public void EnsureDiagnosticSeverity(DiagnosticSeverity severity = DiagnosticSeverity.Warning)
    {
        if (!Diagnostics.Any(x => x.Severity >= severity)) return;
        throw new InvalidOperationException(
            Helpers.FormatFailure($"Compilation failed: the analyzer reported diagnostics at or above {severity}.", [("Analyzer", Diagnostics.AtOrAbove(severity)),])
        );
    }
}
