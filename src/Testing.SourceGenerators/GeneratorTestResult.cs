using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Rocket.Surgery.Extensions.Testing.SourceGenerators;

/// <summary>
///     The results of a specific generators execution
/// </summary>
/// <param name="Compilation">The resulting compilation</param>
/// <param name="Diagnostics">The resulting diagnostics of the compilation</param>
/// <param name="SyntaxTrees">The syntax trees that returned from the generator</param>
[PublicAPI]
public record GeneratorTestResult
(
    CSharpCompilation Compilation,
    ImmutableArray<Diagnostic> Diagnostics,
    ImmutableArray<SyntaxTree> SyntaxTrees
)
{
    /// <summary>
    ///     The generator driver after it ran. Run it again on an edited compilation to test incremental caching,
    ///     or use <see cref="GeneratorTestResults.RerunAsync" />.
    /// </summary>
    public GeneratorDriver? Driver { get; init; }

    /// <summary>
    ///     The driver's run result for this generator's run.
    /// </summary>
    public GeneratorDriverRunResult? DriverRunResult { get; init; }

    /// <summary>
    ///     The run result for this generator: hint names, generated sources, any exception the generator threw and,
    ///     with <see cref="GeneratorTestContextBuilder.WithIncrementalTracking" />, the tracked steps.
    /// </summary>
    public GeneratorRunResult? RunResult => DriverRunResult?.Results is [var result,] ? result : null;

    /// <summary>
    ///     Ensure that the diagnostics are less than the specified severity within the generator results
    /// </summary>
    /// <param name="severity"></param>
    /// <exception cref="InvalidOperationException">Lists every diagnostic at or above <paramref name="severity" />.</exception>
    public void EnsureDiagnosticSeverity(DiagnosticSeverity severity = DiagnosticSeverity.Warning)
    {
        if (!Diagnostics.Any(x => x.Severity >= severity)) return;
        throw new InvalidOperationException(
            Helpers.FormatFailure($"Compilation failed: the generator reported diagnostics at or above {severity}.", [("Generator", Diagnostics.AtOrAbove(severity)),])
        );
    }
}
