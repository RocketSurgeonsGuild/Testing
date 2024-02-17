using System.Collections.Immutable;
using JetBrains.Annotations;
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
    ///     Ensure that the diagnostics are less than the specified severity within the generator results
    /// </summary>
    /// <param name="severity"></param>
    /// <exception cref="InvalidOperationException"></exception>
    public void EnsureDiagnosticSeverity(DiagnosticSeverity severity = DiagnosticSeverity.Warning)
    {
        if (Diagnostics.Any(x => x.Severity >= severity))
        {
            throw new InvalidOperationException("Compilation failed");
        }
    }
}